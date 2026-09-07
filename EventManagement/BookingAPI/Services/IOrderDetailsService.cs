using BookingAPI.DTOs.OrderDetailsDTO;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.Models;

namespace BookingAPI.Services;

public interface IOrderDetailsService
{
    Task Add(CreateOrderDetailsDTO model);
    IQueryable<ReadOrderDetailsDTO> GetAll();
    Task<ReadOrderDetailsDTO> GetById(int id);
    Task Update(int id, UpdateOrderDetailsDTO model);
    Task Delete(int id);
}
