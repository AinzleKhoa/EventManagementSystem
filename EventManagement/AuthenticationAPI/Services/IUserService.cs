using AuthenticationAPI.DTOs;
using AuthenticationAPI.Models;

namespace AuthenticationAPI.Services
{
    public interface IUserService
    {
        IQueryable<ReadUserDTO> GetAll();
        Task<ReadUserDTO?> GetById(int id);
        Task Create(CreateUserDTO dto);
        Task Update(int id, UpdateUserDTO dto);
        Task Delete(int id);
        Task<bool> IsExist(int id);
        Task<PaginatedResultDTO<ReadUserDTO>> SearchAndFilter(UserSearchFilterDTO filter);
        Task LockAndUnlockAccount(int id);
        Task Logout(int userId);
    }
}
