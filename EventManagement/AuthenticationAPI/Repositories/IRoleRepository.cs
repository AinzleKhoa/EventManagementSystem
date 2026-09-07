using AuthenticationAPI.Models;

namespace AuthenticationAPI.Repositories
{
    public interface IRoleRepository
    {
        IQueryable<Role> GetAll();
        Task<Role?> GetById(int id);
        Task<Role?> GetByName(string roleName);
        Task Add(Role role);
        Task Update(Role role);
        Task Delete(Role role);
        Task<bool> IsExist(int id);
    }
}
