using AutoMapper;
using NotificationAPI.DTOs;
using NotificationAPI.Models;

namespace NotificationAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<Notification, ReadNotificationDTO>()
                    .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead ?? false))
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.UtcNow));

            // Từ API lưu vào DB
            CreateMap<CreateNotificationDTO, Notification>()
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false)) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); 
        }
    }
}
