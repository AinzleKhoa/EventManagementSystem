using AuthenticationAPI.Models;

namespace AuthenticationAPI.Repositories
{
    public interface IUserRepository
    {
        IQueryable<User> GetAll();
        Task<User?> GetById(int id);
        Task<User?> GetByEmail(string email);
        Task Add(User user);
        Task Update(User user);
        Task Delete(User user);
        Task<bool> IsExist(int id);
        Task Logout(int userId);
        Task AddRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshToken(string token);
        Task UpdateRefreshToken(RefreshToken refreshToken);
    }
}
