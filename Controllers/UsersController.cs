using Microsoft.AspNetCore.Mvc;
using AuthApp.Models;
using AuthApp.Services;

namespace AuthApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect"});
            
            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpPost]
        public IActionResult Create(CreateUserRequest userRequest)
        {
            if (_userService.GetByEmail(userRequest.Email) != null)
            {
                return BadRequest(new {message = "Email taken"});
            }

            var user = new User
            {
                Email = userRequest.Email,
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                Password = userRequest.Password
            };

            return Ok(_userService.Create(user));
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordRequest forgotPasswordRequest)
        {
            var user = _userService.GetByEmail(forgotPasswordRequest.Email);
            if(user == null)
            {
                return emailNotFound();
            }
            _userService.ForgotPassword(user);
            
            return Ok(new {message = "Forgot password email sent"});
        }
        
        [HttpPost("confirm-email")]
        public IActionResult ConfirmEmail(ConfirmEmailRequest confirmEmailRequest)
        {
            var user = _userService.GetByEmail(confirmEmailRequest.Email);
            if (user == null)
            {
                return emailNotFound();
            }

            if (!_userService.ConfirmEmail(user, confirmEmailRequest.ConfirmationToken))
            {
                return BadRequest(new {message = "Confirmation token not valid"});
            }

            return Ok(new {message = "email confirmed"});
        }

        private IActionResult emailNotFound()
        {
            return BadRequest(new {message = "Email not found"});
        }
    }
}