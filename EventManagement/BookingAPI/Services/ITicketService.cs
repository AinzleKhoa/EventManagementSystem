using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.TicketDTO;
using BookingAPI.Models;

namespace BookingAPI.Services;

public interface ITicketService
{
    Task Add(CreateTicketDTO model);
    IQueryable<ReadTicketDTO> GetAll();
    Task<ReadTicketDTO> GetById(int id);
    Task Update(int id, UpdateTicketDTO model);
    Task Delete(int id);
    Task<TicketCheckInResponseDTO> CheckInAsync(TicketCheckInRequestDTO request, int staffId);
    IQueryable<ReadTicketDTO> GetAll(int eventID);
    Task<TicketValidationDTO> GetTicketByCode(Guid code);
}
