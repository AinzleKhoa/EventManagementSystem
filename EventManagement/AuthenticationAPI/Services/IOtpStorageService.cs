namespace AuthenticationAPI.Services
{
    public interface IOtpStorageService
    {
        void SaveOtp(string email, string otp, int expireMinutes);
        bool ValidateOtp(string email, string otp);

    }
}
