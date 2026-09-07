using Microsoft.EntityFrameworkCore;
using SocialAPI.Models;

namespace SocialAPI.Repositories.Implement
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly SocialDbContext _context;

        public ReviewRepository(SocialDbContext context)
        {
            _context = context;
        }

        public IQueryable<Review> GetReviewsQueryable()
        {
            return _context.Reviews.AsQueryable();
        }

        public async Task<Review> AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }

        public async Task<bool> ExistsAsync(int userId, int eventId)
        {
            return await _context.Reviews.AnyAsync(r => r.UserId == userId && r.EventId == eventId);
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Review>> GetByEventIdAsync(int eventId)
        {
            return await _context.Reviews
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int eventId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.EventId == eventId && r.Rating.HasValue)
                .ToListAsync();
            if (!reviews.Any()) return 0;
            return reviews.Average(r => r.Rating!.Value);
        }
    }
}
