namespace VoucherAPI.Models
{
    public class AppUser
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public int RoleId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsEmailVerified { get; set; }
        public string SecurityStamp { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
