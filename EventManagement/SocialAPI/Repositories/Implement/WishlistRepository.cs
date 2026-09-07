using Microsoft.EntityFrameworkCore;
using SocialAPI.Models;

namespace SocialAPI.Repositories.Implement
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly SocialDbContext _context;

        public WishlistRepository(SocialDbContext context)
        {
            _context = context;
        }

        public IQueryable<Wishlist> GetWishlistsQueryable()
        {
            return _context.Wishlists.AsQueryable();
        }

        public async Task<Wishlist> AddAsync(Wishlist wishlist)
        {
            await _context.Wishlists.AddAsync(wishlist);
            await _context.SaveChangesAsync();
            return wishlist;
        }

        public async Task<bool> RemoveAsync(int userId, int eventId)
        {
            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.EventId == eventId);

            if (item == null) return false;

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Wishlist>> GetByUserIdAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int userId, int eventId)
        {
            return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.EventId == eventId);
        }
    }
}
