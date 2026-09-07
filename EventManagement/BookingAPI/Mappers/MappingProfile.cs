using AutoMapper;
using BookingAPI.DTOs.OrderDetailsDTO;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.TicketDTO;
using BookingAPI.Models;

namespace BookingAPI.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderDetail, ReadOrderDetailsDTO>();
            CreateMap<ReadOrderDetailsDTO, OrderDetail>();
            CreateMap<CreateOrderDetailsDTO, OrderDetail>();
            CreateMap<UpdateOrderDetailsDTO, OrderDetail>();

            CreateMap<Ticket, ReadTicketDTO>();
            CreateMap<ReadTicketDTO, Ticket>();
            CreateMap<CreateTicketDTO, Ticket>();
            CreateMap<UpdateTicketDTO, Ticket>();

            CreateMap<CreateOrderDTO, Order>();
            CreateMap<UpdateOrderDTO, Order>();
            CreateMap<Order, ReadOrderDTO>()
             .ForMember(dest => dest.OrderDetails,
                 opt => opt.MapFrom(src => src.OrderDetails))
             .ForMember(dest => dest.Tickets,
                 opt => opt.MapFrom(src => src.Tickets));

            CreateMap<ReadOrderDTO, Order>()
             .ForMember(dest => dest.OrderDetails,
                 opt => opt.MapFrom(src => src.OrderDetails))
             .ForMember(dest => dest.Tickets,
                 opt => opt.MapFrom(src => src.Tickets));
            CreateMap<TicketValidationDTO, Ticket>();
            CreateMap<Ticket, TicketValidationDTO>();
        }
    }


}
