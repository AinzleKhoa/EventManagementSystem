using AutoMapper;
using AutoMapper.QueryableExtensions;
using BookingAPI.DTOs.OrderDetailsDTO;
using BookingAPI.DTOs.OrderDTO;
using BookingAPI.DTOs.TicketDTO;
using BookingAPI.Models;
using BookingAPI.Repositories;

namespace BookingAPI.Services.Implements
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        IMapper _mapper;

        public TicketService(ITicketRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task Add(CreateTicketDTO model)
        {
            await _repository.Add(_mapper.Map<Ticket>(model));
        }

        public async Task Delete(int id)
        {
            await _repository.Delete(id);
        }

        public IQueryable<ReadTicketDTO> GetAll()
        {
            return _repository.GetAll()
            .ProjectTo<ReadTicketDTO>(_mapper.ConfigurationProvider);

        }

        public async Task<ReadTicketDTO> GetById(int id)
        {
            return _mapper.Map<ReadTicketDTO>(await _repository.GetById(id));
        }

        public async Task Update(int id, UpdateTicketDTO model)
        {
            var p = await _repository.GetById(id);
            _mapper.Map(model, p);
            await _repository.Update(p);
        }
        public async Task<TicketCheckInResponseDTO> CheckInAsync(TicketCheckInRequestDTO request, int staffId)
        {
            Guid validGuid = request.TicketCode;

            var ticket = await _repository.GetTicketByCodeAsync(validGuid);

            if (ticket == null)
                return new TicketCheckInResponseDTO
                {
                    IsSuccess = false,
                    Message = "Ticket does not exist in the system."
                };

            if (ticket.EventId != request.EventId)
                return new TicketCheckInResponseDTO
                {
                    IsSuccess = false,
                    Message = "This ticket is not valid for the current event."
                };

            if (ticket.Status != "Active")
                return new TicketCheckInResponseDTO
                {
                    IsSuccess = false,
                    Message = $"This ticket has already been used at {ticket.CheckedInAt:dd/MM/yyyy HH:mm}."
                };

            bool isUpdated = await _repository.MarkTicketAsCheckedInAsync(validGuid, staffId);

            if (!isUpdated)
            {
                return new TicketCheckInResponseDTO
                {
                    IsSuccess = false,
                    Message = "WARNING: This ticket has just been scanned at another check-in gate!"
                };
            }

            return new TicketCheckInResponseDTO
            {
                IsSuccess = true,
                Message = "Check-in successful!",
                OwnerName = ticket.OwnerName
            };
        }

        public IQueryable<ReadTicketDTO> GetAll(int eventID)
        {
            return _repository.GetAll(eventID)
            .ProjectTo<ReadTicketDTO>(_mapper.ConfigurationProvider);
        }

        public async Task<TicketValidationDTO> GetTicketByCode(Guid code)
        {
            
          
            Ticket ticket = await _repository.GetTicketByCodeAsync(code);

            return _mapper.Map<TicketValidationDTO>(ticket);
        }
        }
}
