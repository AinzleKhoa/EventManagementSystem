using WebClient.DTOs.Request;
using WebClient.DTOs.Response;

namespace WebClient.Services
{
    public interface ISocialService
    {
        Task<List<ReadReviewDTO>> GetReviewsByEventId(int eventId, string odataQuery = "");
        Task CreateReview(CreateReviewDTO model);
        Task<double> GetEventRating(int eventId);
        Task<bool> UpdateReview(int reviewId, UpdateReviewDTO model);
        Task<bool> DeleteReview(int reviewId);

        // Wishlist
        Task<List<ReadWishlistDTO>> GetMyWishlist(string odataQuery = "");
        Task AddToWishlist(CreateWishlistDTO model);
        Task RemoveFromWishlist(int eventId);
    }
}