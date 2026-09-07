using WebClient.DTOs.Request;
using WebClient.DTOs.Response;


namespace WebClient.Services
{
    public interface IProfileService
    {
        Task<ProfileDTO?> GetProfile();
        Task<ProfileResponseDTO?> UpdateProfile(UpdateProfileDTO dto);
        Task<string?> ChangePassword(ChangePasswordDTO dto);
        Task<ProfileResponseDTO?> UploadAvatar(IFormFile file);
        Task<bool> IsGoogleAccount();
        Task<bool> Logout();
        Task<bool> RequestBecomeOrganizer();
        Task<ProfileResponseDTO?> VerifyBecomeOrganizer(VerifyOrganizerOtpDTO dto);
    }
}
