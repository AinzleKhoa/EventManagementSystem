using ChatbotAPI.DTOs;

namespace ChatbotAPI.Services
{
    public interface IGroqService
    {
        Task<string> SendMessageAsync(List<ChatMessageDTO> chatHistory);
    }
}
