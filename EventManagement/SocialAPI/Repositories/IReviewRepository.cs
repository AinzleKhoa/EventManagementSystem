using SocialAPI.Models;

namespace SocialAPI.Repositories
{
    public interface IReviewRepository
    {
        IQueryable<Review> GetReviewsQueryable();
        Task<Review> AddAsync(Review review);
        Task<List<Review>> GetByEventIdAsync(int eventId);
        Task<Review?> GetByIdAsync(int reviewId);
        Task<bool> ExistsAsync(int userId, int eventId);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Review review);
        Task<double> GetAverageRatingAsync(int eventId);
    }
}
