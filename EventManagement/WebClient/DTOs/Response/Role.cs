using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Response
{
    public class ReadRoleDTO
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
}
