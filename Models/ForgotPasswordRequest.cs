using System.ComponentModel.DataAnnotations;

namespace AuthApp.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Email {get; set;}
    }
}