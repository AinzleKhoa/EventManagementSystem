using EventAPI.DTOs;

namespace EventAPI.Services
{
    public interface IChatApiClient
    {
        Task<CreateEventConversationResponse> CreateEventConversationAsync(int eventId, string eventTitle, int organizerId);
    }

    public class CreateEventConversationResponse
    {
        public int ConversationId { get; set; }
        public int EventId { get; set; }
        public string Title { get; set; }
    }
}
