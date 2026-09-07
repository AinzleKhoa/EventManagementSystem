using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Response
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

    public class UserExistResponse
    {
        public int Id { get; set; }
        public bool Exists { get; set; }
    }
}
