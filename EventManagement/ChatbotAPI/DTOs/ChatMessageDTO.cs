using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAPI.DTOs
{
    public class ChatMessageDTO
    {
        public string Role { get; set; } // Sẽ nhận giá trị "user" hoặc "model"
        public string Text { get; set; } // Nội dung tin nhắn
    }
}
