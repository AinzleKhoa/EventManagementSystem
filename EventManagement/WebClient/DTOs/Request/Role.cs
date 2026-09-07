namespace WebClient.DTOs.Request
{
    public class CreateRoleDTO
    {
        public string RoleName { get; set; } = null!;
    }

    public class UpdateRoleDTO
    {
        public string RoleName { get; set; }
    }
}
