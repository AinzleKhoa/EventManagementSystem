using System.ComponentModel.DataAnnotations;

namespace BookingAPI.DTOs.RoleDTO
{
    public class ReadRoleDTO
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
    public class CreateRoleDTO
    {
        public string RoleName { get; set; } = null!;
    }

    public class UpdateRoleDTO
    {
        public string RoleName { get; set; }
    }

    
}
