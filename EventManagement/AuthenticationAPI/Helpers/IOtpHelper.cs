namespace AuthenticationAPI.Helpers
{
    public interface IOtpHelper
    {
        string GenerateOtp(int length = 6);
    }
}
