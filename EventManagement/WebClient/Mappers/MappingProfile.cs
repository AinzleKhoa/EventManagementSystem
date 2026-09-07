using AutoMapper;
using WebClient.DTOs.Request;
using WebClient.DTOs.Response;
using WebClient.ViewModel;
using WebClient.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ReadCategoryDTO, UpdateCategoryDTO>();

        CreateMap<ReadEventDTO, CreateEventDTO>()
                .ForMember(dest => dest.TicketTypes, opt => opt.MapFrom(src => src.TicketTypes));

        CreateMap<ReadTicketTypeDTO, CreateTicketTypeDTO>();

        CreateMap<ReadEventDTO, UpdateEventDTO>();

        CreateMap<ReadTicketTypeDTO, UpdateTicketTypeDTO>();
        // ===== VOUCHER =====
        CreateMap<ReadVoucherDTO, UpdateVoucherDTO>();

        CreateMap<CheckoutViewModel, CreateOrderDTO>();

        // ===== VOUCHER =====
        CreateMap<ReadVoucherDTO, UpdateVoucherDTO>();

        // Review
        CreateMap<Review, ReadReviewDTO>();

        // ===== USER =====
        CreateMap<ReadUserDTO, UpdateUserDTO>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleId : 0));

        CreateMap<NotificationResponseDTO, NotificationViewModel>()
                .ForMember(dest => dest.TimeAgo, opt => opt.MapFrom(src => CalculateTimeAgo(src.CreatedAt)));
    }

    private string CalculateTimeAgo(DateTime createdAt)
    {
        var timeSpan = DateTime.UtcNow - createdAt;
        if (timeSpan.TotalMinutes < 1) return "Vừa xong";
        if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
        if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
        return $"{(int)timeSpan.TotalDays} ngày trước";
    }
}
