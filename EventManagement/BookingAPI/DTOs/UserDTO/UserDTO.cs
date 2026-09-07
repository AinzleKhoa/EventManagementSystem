using BookingAPI.DTOs.RoleDTO;
using System.ComponentModel.DataAnnotations;

namespace BookingAPI.DTOs.UserDTO
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
        public string? FullName { get; set; }

        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public int RoleId { get; set; }
    }

    public class UpdateUserDTO
    {
        public string? FullName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public bool? IsActive { get; set; }

        public int? RoleId { get; set; }
    }
}
