using AuthApp.Models;
using System.Collections.Generic;
using System;

namespace AuthApp.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        IEnumerable<User> GetAll();
        User GetById(string id);
        User GetByEmail(string id);
        User Create(User user);
        void ForgotPassword(User user);
    }
}