using AutoMapper;
using EventAPI.CustomValidation;
using EventAPI.DTOs;
using EventAPI.Models;
using EventAPI.Repositories;
using EventAPI.Repositories.Implement;

namespace EventAPI.Services.Implement
{
    public class TicketTypeService : ITicketTypeService
    {
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public TicketTypeService(ITicketTypeRepository ticketTypeRepository, IEventRepository eventRepository, IMapper mapper)
        {
            _ticketTypeRepository = ticketTypeRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<List<ReadTicketTypeDTO>> GetTicketTypesByEventIdAsync(int eventId)
        {
            var ticketTypes = await _ticketTypeRepository.GetByEventIdAsync(eventId);
            return _mapper.Map<List<ReadTicketTypeDTO>>(ticketTypes);
        }

        public async Task<ReadTicketTypeDTO?> GetTicketTypeByIdAsync(int id)
        {
            var ticketType = await _ticketTypeRepository.GetByIdAsync(id);
            if (ticketType == null) return null;
            return _mapper.Map<ReadTicketTypeDTO>(ticketType);
        }

        public async Task<ReadTicketTypeDTO> CreateTicketTypeAsync(CreateTicketTypeDTO createDto)
        {
            await ThrowIfEventNotEditable(createDto.EventId, "add ticket type to");
            await ValidateSaleDatesAsync(createDto.EventId, createDto.SaleStartDate, createDto.SaleEndDate);

            var ticketType = _mapper.Map<TicketType>(createDto);
            var createdTicket = await _ticketTypeRepository.CreateAsync(ticketType);
            return _mapper.Map<ReadTicketTypeDTO>(createdTicket);
        }

        public async Task<ReadTicketTypeDTO?> UpdateTicketTypeAsync(int id, UpdateTicketTypeDTO updateDto)
        {
            var existingTicket = await _ticketTypeRepository.GetByIdAsync(id);
            if (existingTicket == null) return null;

            await ThrowIfEventNotEditable(existingTicket.EventId, "edit ticket type of");
            await ValidateSaleDatesAsync(existingTicket.EventId, updateDto.SaleStartDate, updateDto.SaleEndDate);

            _mapper.Map(updateDto, existingTicket);

            if (existingTicket.Quantity < existingTicket.SoldQuantity)
            {
                throw new InvalidOperationException("Total quantity cannot be less than the sold quantity.");
            }

            var updatedTicket = await _ticketTypeRepository.UpdateAsync(existingTicket);
            return _mapper.Map<ReadTicketTypeDTO>(updatedTicket);
        }

        private async Task ValidateSaleDatesAsync(int eventId, DateTime? saleStartDate, DateTime? saleEndDate)
        {
            if (saleStartDate.HasValue && saleEndDate.HasValue && saleEndDate <= saleStartDate)
            {
                throw new ArgumentException("Ticket sale end date must be greater than ticket sale start date.");
            }

            var eventModel = await _eventRepository.GetByIdAsync(eventId);
            if (eventModel != null)
            {
                if (saleStartDate.HasValue && saleStartDate >= eventModel.EndDate)
                {
                    throw new ArgumentException("Ticket sale start date must be before the event end date.");
                }
                if (saleEndDate.HasValue && saleEndDate > eventModel.EndDate)
                {
                    throw new ArgumentException("Ticket sale end date cannot be after the event end date.");
                }
            }
        }

        public async Task<bool> DeleteTicketTypeAsync(int id)
        {
            var existingTicket = await _ticketTypeRepository.GetByIdAsync(id);
            if (existingTicket == null) return false;

            await ThrowIfEventNotEditable(existingTicket.EventId, "delete ticket type of");

            if (existingTicket.SoldQuantity > 0)
            {
                throw new InvalidOperationException("Cannot delete this ticket type because tickets have already been sold.");
            }

            return await _ticketTypeRepository.DeleteAsync(id);
        }

        public async Task UpdateTicketTypeQuantityAsync(int id, UpdateTicketTypeQuantityDTO updateQuanDTO)
        {
            var existingTicket = await _ticketTypeRepository.GetByIdAsync(id);
            if (existingTicket == null) throw new Exception("Not found");

            existingTicket.Quantity -= updateQuanDTO.Quantity;
            existingTicket.SoldQuantity += updateQuanDTO.Quantity;

            if (existingTicket.Quantity < existingTicket.SoldQuantity)
            {
                throw new InvalidOperationException("Total of quantity cannot be lower than sold quantity.");
            }

            await _ticketTypeRepository.UpdateAsync(existingTicket);
        }

        private async Task ThrowIfEventNotEditable(int eventId, string action)
        {
            var eventModel = await _eventRepository.GetByIdAsync(eventId);
            if (eventModel != null)
            {
                EventEditableStatusValidation.ThrowIfNotEditable(eventModel.Status, $"{action} an approved event");
            }
        }
    }
}