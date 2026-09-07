// WebClient/DTOs/Request/CreateEventWithFileDTO.cs
using WebClient.DTOs.Request;

public class CreateEventWithFileDTO : CreateEventDTO // Kế thừa DTO gốc
{
    public IFormFile? PosterFile { get; set; } // Thêm trường nhận file
}

// WebClient/DTOs/Request/UpdateEventWithFileDTO.cs
public class UpdateEventWithFileDTO : UpdateEventDTO
{
    public IFormFile? PosterFile { get; set; }
}