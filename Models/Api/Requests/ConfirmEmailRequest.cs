using System.ComponentModel.DataAnnotations;

namespace AuthApp.Models
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string Email {get; set;}
        [Required]
        public string ConfirmationToken { get; set; }
    }
}