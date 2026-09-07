using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.DTOs
{
    public class ReadRoleDTO
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }

    public class CreateRoleDTO
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers, and underscores.")]
        public string RoleName { get; set; } = null!;
    }

    public class UpdateRoleDTO
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers, and underscores.")]
        public string RoleName { get; set; } = null!;
    }
}