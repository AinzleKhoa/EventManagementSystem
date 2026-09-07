using System.ComponentModel.DataAnnotations;

namespace WebClient.DTOs.Request
{
    public class UserQuestionRequest
    {
        [Required]
        public string Question { get; set; }
    }
}