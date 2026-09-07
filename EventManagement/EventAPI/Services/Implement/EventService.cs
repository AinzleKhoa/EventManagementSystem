using AutoMapper;
using EventAPI.CustomValidation;
using EventAPI.DTOs;
using EventAPI.Models;
using EventAPI.Repositories;

namespace EventAPI.Services.Implement
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IChatApiClient _chatApiClient;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventRepository eventRepository,
            IMapper mapper,
            IChatApiClient chatApiClient,
            ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _chatApiClient = chatApiClient;
            _logger = logger;
        }

        public async Task<List<ReadEventDTO>> GetAllAsync(int? organizerId)
        {
            var events = await _eventRepository.GetAllAsync();

            if (organizerId.HasValue)
            {
                events = events.Where(e => e.OrganizerId == organizerId.Value).ToList();
            }

            return _mapper.Map<List<ReadEventDTO>>(events);
        }

        public async Task<ReadEventDTO?> GetEventByIdAsync(int id)
        {
            var eventModel = await _eventRepository.GetByIdAsync(id);
            if (eventModel == null) return null;
            return _mapper.Map<ReadEventDTO>(eventModel);
        }

        public async Task<ReadEventDTO> CreateEventAsync(CreateEventDTO createDto)
        {
            if (createDto.EndDate <= createDto.StartDate)
            {
                throw new ArgumentException("The end time must be greater than the start time.");
            }

            if (createDto.TicketTypes != null)
            {
                foreach (var ticket in createDto.TicketTypes)
                {
                    ValidateTicketSaleDates(ticket.SaleStartDate, ticket.SaleEndDate, createDto.StartDate, createDto.EndDate, ticket.TypeName);
                }
            }

            var eventModel = _mapper.Map<Event>(createDto);
            var createdEvent = await _eventRepository.CreateAsync(eventModel);

            try
            {
                _logger.LogInformation($"[EVENT] Creating chat conversation for new event {createdEvent.EventId}: {createdEvent.Title}, organizer: {createdEvent.OrganizerId}");
                var conversationResponse = await _chatApiClient.CreateEventConversationAsync(createdEvent.EventId, createdEvent.Title, createdEvent.OrganizerId);

                if (conversationResponse != null)
                {
                    createdEvent.ConversationId = conversationResponse.ConversationId;
                    await _eventRepository.UpdateAsync(createdEvent.EventId, createdEvent);
                    _logger.LogInformation($"[EVENT] ✅ Successfully linked conversation {conversationResponse.ConversationId} to event {createdEvent.EventId}");
                }
                else
                {
                    _logger.LogWarning($"[EVENT] ⚠️ Failed to create chat conversation for event {createdEvent.EventId}, but event was created successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EVENT] ❌ Error creating chat conversation for event {createdEvent.EventId}. Event created but chat failed: {ex.Message}");
            }

            return _mapper.Map<ReadEventDTO>(createdEvent);
        }

        public async Task<ReadEventDTO?> UpdateEventAsync(int id, UpdateEventDTO updateDto)
        {
            if (updateDto.EndDate <= updateDto.StartDate)
            {
                throw new ArgumentException("The end time must be greater than the start time.");
            }

            if (updateDto.TicketTypes != null)
            {
                foreach (var ticket in updateDto.TicketTypes)
                {
                    ValidateTicketSaleDates(ticket.SaleStartDate, ticket.SaleEndDate, updateDto.StartDate, updateDto.EndDate, ticket.TypeName);
                }
            }

            var existingEvent = await _eventRepository.GetByIdAsync(id);

            if (existingEvent == null) return null;

            bool isApproved = EventEditableStatusValidation.IsApproved(existingEvent.Status);
            if (isApproved)
            {
                // Non-sensitive edits allowed: Description, PosterUrl, Location, Latitude, Longitude
                // If Title or Dates are modified on an approved event, block or require re-approval
                if (!string.Equals(existingEvent.Title, updateDto.Title, StringComparison.OrdinalIgnoreCase) ||
                    existingEvent.StartDate != updateDto.StartDate ||
                    existingEvent.EndDate != updateDto.EndDate)
                {
                    throw new InvalidOperationException("Approved events can only have non-sensitive fields (Description, Poster, Location) updated directly. Changing Title or Event Dates requires submitting a new event or admin re-approval.");
                }
            }
            else
            {
                EventEditableStatusValidation.ThrowIfNotEditable(existingEvent.Status, "edit");
            }

            _mapper.Map(updateDto, existingEvent);

            await _eventRepository.UpdateAsync(id, existingEvent);
            return _mapper.Map<ReadEventDTO>(existingEvent);
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent == null) return false;

            EventEditableStatusValidation.ThrowIfNotEditable(existingEvent.Status, "delete");

            return await _eventRepository.DeleteAsync(id);
        }

        public async Task<bool> SubmitForApprovalAsync(int id)
        {
            return await ChangeStatusAsync(id, "Pending");
        }

        public async Task<bool> HideEventAsync(int id)
        {
            return await ChangeStatusAsync(id, "Hidden");
        }

        public async Task<bool> ResumeEventAsync(int id)
        {
            var ev = await _eventRepository.GetByIdAsync(id);
            if (ev == null) return false;

            ev.Status = "Approved";
            await _eventRepository.UpdateAsync(id, ev);
            return true;
        }

        private async Task<bool> ChangeStatusAsync(int id, string newStatus)
        {
            var ev = await _eventRepository.GetByIdAsync(id);
            if (ev == null) return false;

            ev.Status = newStatus;
            await _eventRepository.UpdateAsync(id, ev);
            return true;
        }

        public async Task<List<ReadEventDTO>> SearchEventsAsync(string? keyword, int[]? categoryIds, DateTime? fromDate, DateTime? toDate, decimal? minPrice, decimal? maxPrice)
        {
            var events = await _eventRepository.SearchAsync(keyword, categoryIds, fromDate, toDate, minPrice, maxPrice);
            return _mapper.Map<List<ReadEventDTO>>(events);
        }

        public async Task<List<ReadEventDTO>> GetPendingEventsAsync()
        {
            var events = await _eventRepository.GetByStatusAsync("Pending");
            return _mapper.Map<List<ReadEventDTO>>(events);
        }

        public async Task<bool> ApproveEventAsync(int id)
        {
            var ev = await _eventRepository.GetByIdAsync(id);

            if (ev == null) return false;

            ev.Status = "Approved";
            ev.RejectedReason = null;

            await _eventRepository.UpdateAsync(id, ev);
            return true;
        }

        public async Task<bool> RejectEventAsync(int id, string reason)
        {
            var ev = await _eventRepository.GetByIdAsync(id);

            if (ev == null) return false;
            if (ev.Status != "Pending") return false;

            ev.Status = "Rejected";
            ev.RejectedReason = reason;

            await _eventRepository.UpdateAsync(id, ev);
            return true;
        }


        public async Task<IEnumerable<ReadEventDTO>> GetAssignedEventsAsync(int staffId)
        {
            var events = await _eventRepository.GetAssignedEventsByStaffIdAsync(staffId);
            return _mapper.Map<IEnumerable<ReadEventDTO>>(events);
        }

         public async Task<ReadEventValidatorDTO> AddValidatorAsync(AddValidatorDTO addValidatorDto)
         {
             if (string.IsNullOrWhiteSpace(addValidatorDto.ValidatorEmail))
             {
                 throw new ArgumentException("Validator email cannot be empty.");
             }

             if (!IsValidEmail(addValidatorDto.ValidatorEmail))
             {
                 throw new ArgumentException($"Email '{addValidatorDto.ValidatorEmail}' is invalid. Please check the email format.");
             }

             var eventModel = await _eventRepository.GetByIdAsync(addValidatorDto.EventId);
             if (eventModel == null)
             {
                 throw new ArgumentException($"Event with ID {addValidatorDto.EventId} does not exist.");
             }

             var existingValidator = await _eventRepository.GetValidatorByEmailAndEventAsync(addValidatorDto.EventId, addValidatorDto.ValidatorEmail);
             if (existingValidator != null && existingValidator.IsActive == true)
             {
                 throw new ArgumentException($"Validator {addValidatorDto.ValidatorEmail} has already been added to this event.");
             }

             var eventStaff = new EventStaff
             {
                 EventId = addValidatorDto.EventId,
                 StaffId = 0,
                 StaffEmail = addValidatorDto.ValidatorEmail,
                 ValidatorRole = "Validator",
                 AssignedAt = DateTime.Now,
                 ExpiredAt = eventModel.EndDate,
                 IsActive = true
             };

             var createdStaff = await _eventRepository.AddValidatorAsync(eventStaff);
             return _mapper.Map<ReadEventValidatorDTO>(createdStaff);
         }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ReadEventValidatorDTO>> GetValidatorsByEventIdAsync(int eventId)
        {
            var validators = await _eventRepository.GetValidatorsByEventIdAsync(eventId);
            return _mapper.Map<List<ReadEventValidatorDTO>>(validators);
        }

        public async Task<bool> RemoveValidatorAsync(int eventStaffId)
        {
            return await _eventRepository.RemoveValidatorAsync(eventStaffId);
        }

        public async Task<bool> DeactivateExpiredValidatorsAsync()
        {
            var allValidators = await _eventRepository.GetAllAsync();

            var expiredValidators = new List<int>();
            foreach (var ev in allValidators)
            {
                var validators = await _eventRepository.GetValidatorsByEventIdAsync(ev.EventId);
                var expired = validators.Where(v => v.ExpiredAt <= DateTime.Now && v.IsActive == true).ToList();
                expiredValidators.AddRange(expired.Select(v => v.EventStaffId));
            }

            foreach (var validatorId in expiredValidators)
            {
                var validator = await _eventRepository.GetValidatorByIdAsync(validatorId);
                if (validator != null && validator.IsActive == true)
                {
                    validator.IsActive = false;
                    await _eventRepository.UpdateValidatorAsync(validatorId, validator);
                }
            }

            return true;
        }

        public async Task<List<ReadEventDTO>> GetEventsByValidatorEmailAsync(string validatorEmail)
        {
            if (string.IsNullOrWhiteSpace(validatorEmail))
            {
                return new List<ReadEventDTO>();
            }

            var allEvents = await _eventRepository.GetAllAsync();

            var validatorEvents = new List<ReadEventDTO>();
            foreach (var ev in allEvents)
            {
                var validators = await _eventRepository.GetValidatorsByEventIdAsync(ev.EventId);
                var isValidator = validators.Any(v => 
                    v.StaffEmail == validatorEmail && 
                    v.IsActive == true && 
                    v.ExpiredAt > DateTime.Now);

                if (isValidator)
                {
                    validatorEvents.Add(_mapper.Map<ReadEventDTO>(ev));
                }
            }

            return validatorEvents;
        }

        public async Task<bool> IsValidatorOfEventAsync(int eventId, string validatorEmail)
        {
            if (string.IsNullOrWhiteSpace(validatorEmail))
                return false;

            var validators = await _eventRepository.GetValidatorsByEventIdAsync(eventId);
            return validators.Any(v => 
                v.StaffEmail == validatorEmail && 
                v.IsActive == true && 
                v.ExpiredAt > DateTime.Now);
        }

        public async Task<List<ReadEventDTO>> GetEventByOrganizerId(int Organizerid)
        {
            var events = await _eventRepository.GetEventByOrganizerId(Organizerid);
            return _mapper.Map<List<ReadEventDTO>>(events);
        }

        public async Task<int> GetTotalApprovedEventsAsync()
        {
            return await _eventRepository.CountActiveByStatusAsync("Approved");
        }

        public async Task<int> GetTotalEventsAsync()
        {
            return await _eventRepository.CountAllAsync();
        }

        public async Task<List<ReadEventDTO>> GetTopSellingEventsAsync(int count = 5)
        {
            var topEvents = await _eventRepository.GetTopSellingEventsAsync(count);
            return _mapper.Map<List<ReadEventDTO>>(topEvents);
        }

        private static void ValidateTicketSaleDates(DateTime? saleStartDate, DateTime? saleEndDate, DateTime eventStartDate, DateTime eventEndDate, string? ticketTypeName)
        {
            string name = string.IsNullOrWhiteSpace(ticketTypeName) ? "Standard" : ticketTypeName;
            if (saleStartDate.HasValue && saleEndDate.HasValue && saleEndDate <= saleStartDate)
            {
                throw new ArgumentException($"Ticket '{name}': Sale end date must be greater than sale start date.");
            }
            if (saleStartDate.HasValue && saleStartDate >= eventEndDate)
            {
                throw new ArgumentException($"Ticket '{name}': Sale start date must be before the event end date.");
            }
            if (saleEndDate.HasValue && saleEndDate > eventEndDate)
            {
                throw new ArgumentException($"Ticket '{name}': Sale end date cannot be after the event end date.");
            }
        }
    }
}
