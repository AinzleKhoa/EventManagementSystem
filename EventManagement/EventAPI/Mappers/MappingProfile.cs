using AutoMapper;
using EventAPI.DTOs;
using EventAPI.Models;

namespace EventAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<CreateEventDTO, Event>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Draft"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.TicketTypes, opt => opt.MapFrom(src => src.TicketTypes));

            CreateMap<UpdateEventDTO, Event>();

            CreateMap<Event, ReadEventDTO>()
                .ForMember(dest => dest.TicketTypes, opt => opt.MapFrom(src => src.TicketTypes))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.Ignore());

            CreateMap<UpdateTicketTypeDTO, TicketType>();
            CreateMap<CreateTicketTypeDTO, TicketType>()
                .ForMember(dest => dest.Event, opt => opt.Ignore())
                .ForMember(dest => dest.SoldQuantity, opt => opt.MapFrom(src => 0));

            CreateMap<TicketType, ReadTicketTypeDTO>()
                .ForMember(dest => dest.SoldQuantity, opt => opt.MapFrom(src => src.SoldQuantity.HasValue ? src.SoldQuantity.Value : 0))
                .ForMember(dest => dest.AvailableQty,
                           opt => opt.MapFrom(src => src.Quantity - (src.SoldQuantity.HasValue ? src.SoldQuantity.Value : 0)));
            CreateMap<Event, EventStaffResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<EventStaff, ReadEventValidatorDTO>();
            CreateMap<ReadEventValidatorDTO, EventStaff>();

        }

    }
}
