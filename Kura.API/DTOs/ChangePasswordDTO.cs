using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required!")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required!")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number!")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password!")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match!")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}