using SocialAPI.Models;

namespace SocialAPI.Repositories
{
    public interface IWishlistRepository
    {
        IQueryable<Wishlist> GetWishlistsQueryable();
        Task<Wishlist> AddAsync(Wishlist wishlist);
        Task<List<Wishlist>> GetByUserIdAsync(int userId);
        Task<bool> RemoveAsync(int userId, int eventId);
        Task<bool> ExistsAsync(int userId, int eventId);
    }
}
