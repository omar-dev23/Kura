using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; } = string.Empty;
    }
}