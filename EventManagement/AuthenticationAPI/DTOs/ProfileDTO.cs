using System.ComponentModel.DataAnnotations;
using AuthenticationAPI.Validations; 

namespace AuthenticationAPI.DTOs
{
    public class ProfileDTO
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UpdateProfileDTO
    {
        // Input DTO: Cần validate
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string? FullName { get; set; }

        [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [StrongPassword(ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ProfileResponseDTO
    {
        public ProfileDTO Profile { get; set; } = null!;
        public string NewToken { get; set; } = string.Empty;
    }
}