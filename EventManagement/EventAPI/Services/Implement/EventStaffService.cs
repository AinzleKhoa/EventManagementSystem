using AutoMapper;
using EventAPI.DTOs;
using EventAPI.Repositories;

namespace EventAPI.Services.Implement
{
    public class EventStaffService : IEventStaffService
    {
        private readonly IEventStaffRepository _repo;
        private readonly IMapper _mapper;
        public EventStaffService(IEventStaffRepository repo,IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EventStaffResponseDto>> GetStaffEventsAsync(int staffId)
        {
            var events = await _repo.GetEventsByStaffIdAsync(staffId);
            return _mapper.Map<IEnumerable<EventStaffResponseDto>>(events);
        }
    }
}
