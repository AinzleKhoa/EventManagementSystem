using SocialAPI.DTOs;

namespace SocialAPI.Services
{
    public interface ISocialService
    {
        // ==========================================
        // PHẦN ODATA (TRUY VẤN - LẤY DANH SÁCH)
        // ==========================================
        IQueryable<ReadReviewDTO> GetAllReviewsQueryable();
        IQueryable<ReadWishlistDTO> GetAllWishlistsQueryable();
        IQueryable<ReadReviewDTO> GetEventReviewsQueryable(int eventId);
        IQueryable<ReadWishlistDTO> GetUserWishlistsQueryable(int userId);

        // ==========================================
        // PHẦN WISHLIST (THÊM, XÓA)
        // ==========================================
        Task<ReadWishlistDTO> AddToWishlistAsync(CreateWishlistDTO dto);
        Task<bool> RemoveFromWishlistAsync(int userId, int eventId);

        // ==========================================
        // PHẦN REVIEW (THÊM, SỬA, XÓA, TÍNH ĐIỂM)
        // ==========================================
        Task<ReadReviewDTO> CreateReviewAsync(CreateReviewDTO dto);
        Task<double> GetEventRatingAsync(int eventId);
        Task<bool> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDTO dto);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
    }
}