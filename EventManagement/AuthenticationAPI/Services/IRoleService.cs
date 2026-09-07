using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Services
{
    public interface IRoleService
    {
        IQueryable<ReadRoleDTO> GetAll();
        Task<ReadRoleDTO?> GetById(int id);
        Task Create(CreateRoleDTO dto);
        Task Update(int id, UpdateRoleDTO dto);
        Task Delete(int id);
        Task<bool> IsExist(int id);
    }
}
