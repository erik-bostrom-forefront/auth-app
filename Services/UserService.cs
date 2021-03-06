using AuthApp.Helpers;
using AuthApp.Models;
using BC = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MongoDB.Driver;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AuthApp.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings, IUserDatabaseSettings settings)
        {
            _appSettings = appSettings.Value;

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _users.Find<User>(
                u => u.Email == model.Email
            ).FirstOrDefault();
            
            if (user == null || !BC.Verify(model.Password, user.Password)) return null;

            //Generate token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public IEnumerable<User> GetAll()
        {
            return _users.Find(u => true).ToList();
        }

        public User GetById(string id) => _users.Find<User>(u => u.Id == id).FirstOrDefault();

        public User GetByEmail(string email) => _users.Find<User>(u => u.Email == email).FirstOrDefault();

        public User Create(User user)
        {
            user.Password = BC.HashPassword(user.Password);
            user.ConfirmEmailToken = createConfirmationToken();
            user.EmailConfirmed = false;
            _users.InsertOne(user);
            return user;
        }

        public async void ForgotPassword(User user)
        {
            await sendForgotPasswordEmail(user);
        }

        public bool ConfirmEmail(User user, string token)
        {
            if (user.ConfirmEmailToken != token || !validateConfirmationToken(token)) return false;
            var filter = Builders<User>.Filter.Eq("Email", user.Email);
            var update = Builders<User>.Update.Set("EmailConfirmed", true);
            _users.UpdateOne(filter, update);
            return true;
        }

        private string generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString())}),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task sendForgotPasswordEmail(User user)
        {
            var apiKey = _appSettings.SendgridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("erik.bostrom@forefront.se", "Erik Boström");
            const string subject = "Forgot password";
            var to = new EmailAddress(user.Email, user.FirstName + " " + user.LastName);
            const string plainTextContent = "You have forgotten your email";
            const string htmlContent = "<strong>remember your password</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        private string createConfirmationToken()
        {
            var time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            var key = Guid.NewGuid().ToByteArray();
            var token = Convert.ToBase64String(time.Concat(key).ToArray());

            return token;
        }

        private bool validateConfirmationToken(string token)
        {
            var data = Convert.FromBase64String(token);
            var when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            
            return when >= DateTime.UtcNow.AddHours(-24);
        }
    }
}