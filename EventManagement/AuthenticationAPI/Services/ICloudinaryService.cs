namespace AuthenticationAPI.Services
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file, string folderName = "Avatars", string? publicId = null);
    }
}
