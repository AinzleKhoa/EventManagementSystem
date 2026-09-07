using AutoMapper;
using AutoMapper.QueryableExtensions;
using SocialAPI.DTOs;
using SocialAPI.Models;
using SocialAPI.Repositories;

namespace SocialAPI.Services.Implement
{
    public class SocialService : ISocialService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IReviewRepository _reviewRepo;
        private readonly IMapper _mapper;

        public SocialService(IWishlistRepository wishlistRepo, IReviewRepository reviewRepo, IMapper mapper)
        {
            _wishlistRepo = wishlistRepo;
            _reviewRepo = reviewRepo;
            _mapper = mapper;
        }

        // ==========================================
        // PHẦN ODATA (TRUY VẤN - LẤY DANH SÁCH)
        // ==========================================
        public IQueryable<ReadReviewDTO> GetAllReviewsQueryable()
        {
            return _reviewRepo.GetReviewsQueryable()
                .ProjectTo<ReadReviewDTO>(_mapper.ConfigurationProvider);
        }

        public IQueryable<ReadWishlistDTO> GetAllWishlistsQueryable()
        {
            return _wishlistRepo.GetWishlistsQueryable()
                .ProjectTo<ReadWishlistDTO>(_mapper.ConfigurationProvider);
        }

        public IQueryable<ReadReviewDTO> GetEventReviewsQueryable(int eventId)
        {
            return _reviewRepo.GetReviewsQueryable()
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReadReviewDTO>(_mapper.ConfigurationProvider);
        }

        public IQueryable<ReadWishlistDTO> GetUserWishlistsQueryable(int userId)
        {
            return _wishlistRepo.GetWishlistsQueryable()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ProjectTo<ReadWishlistDTO>(_mapper.ConfigurationProvider);
        }

        // ==========================================
        // PHẦN LOGIC THÊM, SỬA, XÓA WISHLIST
        // ==========================================
        public async Task<ReadWishlistDTO> AddToWishlistAsync(CreateWishlistDTO dto)
        {
            bool exists = await _wishlistRepo.ExistsAsync(dto.UserId, dto.EventId);
            if (exists) throw new Exception("Event already in wishlist");

            var wishlist = _mapper.Map<Wishlist>(dto);
            var created = await _wishlistRepo.AddAsync(wishlist);
            return _mapper.Map<ReadWishlistDTO>(created);
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int eventId)
        {
            return await _wishlistRepo.RemoveAsync(userId, eventId);
        }

        // ==========================================
        // PHẦN LOGIC THÊM, SỬA, XÓA, TÍNH ĐIỂM REVIEW
        // ==========================================
        public async Task<ReadReviewDTO> CreateReviewAsync(CreateReviewDTO dto)
        {
            bool exists = await _reviewRepo.ExistsAsync(dto.UserId, dto.EventId);
            if (exists) throw new Exception("You have already reviewed this event.");

            var review = _mapper.Map<Review>(dto);
            var created = await _reviewRepo.AddAsync(review);
            return _mapper.Map<ReadReviewDTO>(created);
        }

        public async Task<double> GetEventRatingAsync(int eventId)
        {
            // Hàm này giữ lại vì nó tính toán con số trung bình (scalar value) chứ không phải danh sách
            return await _reviewRepo.GetAverageRatingAsync(eventId);
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDTO dto)
        {
            var existingReview = await _reviewRepo.GetByIdAsync(reviewId);

            if (existingReview == null || existingReview.UserId != userId)
            {
                return false;
            }

            existingReview.Rating = dto.Rating;
            existingReview.Comment = dto.Comment;

            await _reviewRepo.UpdateAsync(existingReview);
            return true;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            var existingReview = await _reviewRepo.GetByIdAsync(reviewId);

            if (existingReview == null || existingReview.UserId != userId)
            {
                return false;
            }

            await _reviewRepo.DeleteAsync(existingReview);
            return true;
        }
    }
}
