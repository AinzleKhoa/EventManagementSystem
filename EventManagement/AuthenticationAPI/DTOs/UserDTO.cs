using System.ComponentModel.DataAnnotations;
using AuthenticationAPI.Validations;
namespace AuthenticationAPI.DTOs
{
    public class ReadUserDTO
    {
        [Key]
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
        public ReadRoleDTO? Role { get; set; }
    }

    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [Url]
        public string? AvatarUrl { get; set; }

        [Required(ErrorMessage = "Role ID is required.")]
        [Range(2, 4, ErrorMessage = "Invalid Role ID.")]
        public int RoleId { get; set; }
        public IFormFile? AvatarFile { get; set; }

    }

    public class UpdateUserDTO
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters.")]
        public string? FullName { get; set; }

        [PhoneNumber(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }
        [Url]
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }

        [Range(2, 4, ErrorMessage = "Invalid Role ID.")]
        public int? RoleId { get; set; }
        public IFormFile? AvatarFile { get; set; }

    }
}