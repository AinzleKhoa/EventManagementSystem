using System.Security.Cryptography;

namespace AuthenticationAPI.Helpers.Implements
{
    public class OtpHelper : IOtpHelper
    {
        public string GenerateOtp(int length = 6)
        {
            if (length <= 0)
                throw new ArgumentException("OTP length must be greater than 0.");

            var otp = string.Empty;

            using var rng = RandomNumberGenerator.Create();
            var byteBuffer = new byte[length];

            rng.GetBytes(byteBuffer);

            foreach (var b in byteBuffer)
            {
                otp += (b % 10).ToString();
            }

            return otp;
        }
    }
}
