using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace AuthApp.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        // [JsonIgnore]
        public string ConfirmEmailToken { get; set; }
        public bool EmailConfirmed { get; set; }
        [JsonIgnore]
        public string ResetPasswordToken { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}