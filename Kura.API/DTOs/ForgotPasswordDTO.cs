using System.ComponentModel.DataAnnotations;

namespace Kura.API.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address!")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpDTO
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP code is required!")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits!")]
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address!")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset token is required!")]
        public string Token { get; set; } = string.Empty;

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