using System.ComponentModel.DataAnnotations;

namespace AuthApp.Models
{
    public class CreateUserRequest
    {
        [Required]
        public string Email {get; set;}
        [Required]
        public string FirstName {get; set;}
        [Required]
        public string LastName {get; set;}
        [Required]
        public string Password {get; set;}
    }
}