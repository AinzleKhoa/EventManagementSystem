using AutoMapper;
using SocialAPI.DTOs;
using SocialAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Wishlist
            CreateMap<CreateWishlistDTO, Wishlist>();
            CreateMap<Wishlist, ReadWishlistDTO>();

            // Review
            CreateMap<CreateReviewDTO, Review>();
            CreateMap<Review, ReadReviewDTO>();
            CreateMap<UpdateReviewDTO, Review>();
        }
    }
}
