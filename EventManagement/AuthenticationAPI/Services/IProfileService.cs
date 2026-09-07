using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Services
{
    public interface IProfileService
    {
        Task<ProfileDTO?> GetProfile(int userId);
        Task<ProfileResponseDTO?> UpdateProfile(int userId, UpdateProfileDTO dto);
        Task ChangePassword(int userId, ChangePasswordDTO dto);
        Task<ProfileResponseDTO?> UpdateAvatarUrl(int userId, string avatarUrl);
        Task<bool> IsGoogleAccount(int id);
        Task Logout(string jti, TimeSpan remainingTime);
        Task RequestBecomeOrganizer(int userId);
        Task<ProfileResponseDTO?> VerifyBecomeOrganizer(int userId, VerifyOrganizerOtpDTO dto);
    }
}
