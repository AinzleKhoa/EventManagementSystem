using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface IUserService
    {
        Task<List<ReadUserDTO>> GetAll();
        Task<ReadUserDTO> GetById(int id);
        Task Create(CreateUserDTO model);
        Task Update(int id, UpdateUserDTO model);
        Task<bool> IsExist(int id);
        Task<PaginatedResultDTO<ReadUserDTO>> SearchAndFilter(UserSearchFilterDTO filter);
        Task LockAndUnlockAccount(int id);
        Task<long> GetTotalCustomers();

    }
}
