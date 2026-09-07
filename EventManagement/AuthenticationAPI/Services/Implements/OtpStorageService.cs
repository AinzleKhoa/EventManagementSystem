using Microsoft.Extensions.Caching.Memory;

namespace AuthenticationAPI.Services.Implements
{
    public class OtpStorageService : IOtpStorageService
    {
        private readonly IMemoryCache _cache;

        public OtpStorageService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void SaveOtp(string email, string otp, int expireMinutes)
        {
            _cache.Set(email, otp, TimeSpan.FromMinutes(expireMinutes));
        }

        public bool ValidateOtp(string email, string otp)
        {
            if (_cache.TryGetValue(email, out string? storedOtp))
            {
                if (storedOtp == otp)
                {
                    _cache.Remove(email); // dùng 1 lần
                    return true;
                }
            }

            return false;
        }
    }
}
