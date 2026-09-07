// WebClient/Helpers/FileHelper.cs
public static class FileHelper
{
    public static async Task<string> UploadFileAsync(IFormFile file, IWebHostEnvironment env)
    {
        if (file == null || file.Length == 0) return null;

        var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Trả về đường dẫn tương đối để lưu vào DB
        return "/uploads/" + uniqueFileName;
    }
}