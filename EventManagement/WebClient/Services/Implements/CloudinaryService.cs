using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace WebClient.Services.Implements
{
    public interface IPhotoService
    {
        Task<string> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }

    public class CloudinaryService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            // --- BƯỚC TEST: ĐIỀN TRỰC TIẾP KEY VÀO ĐÂY ĐỂ TRÁNH LỖI FILE JSON ---
            // Hãy copy chính xác từ web Cloudinary vào 3 dòng dưới:
            string cloudName = "dqisfo4hh";
            string apiKey = "728372688977156";
            string apiSecret = "-6yhq8lC3l4QY6TSuvO6HruslbM"; // Bấm hình con mắt trên web để lấy mã này

            // Khởi tạo tài khoản
            var acc = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(acc);
            _cloudinary.Api.Secure = true; // Luôn dùng HTTPS
        }

        public async Task<string> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        // Resize ảnh nhỏ lại để upload nhanh hơn, tránh timeout làm sập app
                        Transformation = new Transformation().Height(500).Width(500).Crop("fill")
                    };

                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                catch (Exception ex)
                {
                    // Ghi log ra màn hình debug của Visual Studio thay vì crash
                    System.Diagnostics.Debug.WriteLine($"LỖI UPLOAD: {ex.Message}");
                    throw new Exception("Không thể upload ảnh: " + ex.Message);
                }
            }

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}