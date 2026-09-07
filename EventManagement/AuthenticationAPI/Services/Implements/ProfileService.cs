using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Repositories;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static System.Net.WebRequestMethods;

namespace AuthenticationAPI.Services.Implements
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IRoleRepository _roleRepository;

       public ProfileService(IUserRepository userRepository,
                              IPasswordHelper passwordHelper,
                              IJwtHelper jwtHelper,
                              IMemoryCache cache, 
                              IMapper mapper,
                              IEmailService emailService, 
                              IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _passwordHelper = passwordHelper;
            _jwtHelper = jwtHelper;
            _cache = cache;
            _mapper = mapper;
            _emailService = emailService;
            _roleRepository = roleRepository;
        }

        public async Task<ProfileDTO?> GetProfile(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return null;

            return _mapper.Map<ProfileDTO>(user);
        }

        public async Task<ProfileResponseDTO?> UpdateProfile(int userId, UpdateProfileDTO dto)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new Exception("User not found");

            _mapper.Map(dto, user);

            // 1. Đổi Stamp mới
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);

            _cache.Remove($"security_stamp_{userId}");
            user.Role = await _roleRepository.GetById(user.RoleId);

            var newToken = _jwtHelper.GenerateToken(user);

            return new ProfileResponseDTO
            {
                Profile = _mapper.Map<ProfileDTO>(user),
                NewToken = newToken
            };
        }

        public async Task ChangePassword(int userId, ChangePasswordDTO dto)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new Exception("User not found.");

            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new Exception("This account uses social login (Google/Facebook).");

            string cacheKey = $"failed_attempts_{userId}";
            int failedCount = _cache.Get<int>(cacheKey);

            var isValid = _passwordHelper.VerifyPassword(user.PasswordHash, dto.CurrentPassword);

            if (!isValid)
            {
                failedCount++;
                _cache.Set(cacheKey, failedCount, TimeSpan.FromMinutes(10));

                if (failedCount >= 3)
                {
                    _cache.Remove(cacheKey);
                    user.SecurityStamp = Guid.NewGuid().ToString();
                    await _userRepository.Update(user);
                    _cache.Remove($"security_stamp_{userId}");

                    throw new UnauthorizedAccessException("Too many failed attempts. Your account has been logged out for security.");
                }

                throw new Exception($"Current password is incorrect. Attempt ({failedCount}/3).");
            }

            user.PasswordHash = _passwordHelper.HashPassword(dto.NewPassword);

            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);

            _cache.Remove(cacheKey);
            _cache.Remove($"security_stamp_{userId}");
        }

        public async Task<ProfileResponseDTO?> UpdateAvatarUrl(int userId, string avatarUrl)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new Exception("User not found.");

            user.AvatarUrl = avatarUrl;

            // Đổi Stamp khi đổi ảnh đại diện
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);
            _cache.Remove($"security_stamp_{userId}");

            user.Role = await _roleRepository.GetById(user.RoleId);

            var newToken = _jwtHelper.GenerateToken(user);

            return new ProfileResponseDTO
            {
                Profile = _mapper.Map<ProfileDTO>(user),
                NewToken = newToken
            };
        }

        public async Task<bool> IsGoogleAccount(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                throw new Exception("User not found");
            return user.PasswordHash == null;

        }

        public async Task Logout(string jti, TimeSpan remainingTime)
        {
            // Lưu JTI (Token ID) vào MemoryCache.
            // Thời gian tồn tại trong cache bằng đúng thời gian còn hạn của Token.
            // Khi Token tự hết hạn, cache cũng tự xóa để tiết kiệm bộ nhớ.
            _cache.Set($"blacklist_{jti}", true, remainingTime);

            await Task.CompletedTask;
        }



        // Thêm interface IEmailService vào constructor nếu bạn đã có, 
        // hoặc tạm thời comment lại dòng gửi mail để test logic trước.

        public async Task RequestBecomeOrganizer(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new Exception("User not found.");

            // Kiểm tra role (Giả sử 2 là Customer)
            if (user.RoleId != 2) throw new Exception("Only Customers can become Organizers.");

            // Tạo OTP 6 số
            string otpCode = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào cache với thời hạn 5 phút
            _cache.Set($"organizer_otp_{userId}", otpCode, TimeSpan.FromMinutes(5));

            // Gọi Email Service để gửi mã otpCode đến user.Email
            await _emailService.SendEmailAsync(
                user.Email,
                "Verify Your Organizer Account",
                $@"
        <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
            <h2 style='color: #2c3e50;'>Organizer Account Upgrade</h2>
            <p>Hello,</p>
            <p>We received a request to upgrade your account to an Organizer on our platform.</p>
            <p>Your verification OTP is: <strong style='font-size: 24px; color: #e74c3c;'>{otpCode}</strong></p>
            <p><em>This code will expire in 5 minutes. Please do not share this code with anyone.</em></p>
            <p>If you did not request this upgrade, please safely ignore this email.</p>
            <p>Best regards,<br>The Event Management Team</p>
        </div>
        "
            );
        }

        public async Task<ProfileResponseDTO?> VerifyBecomeOrganizer(int userId, VerifyOrganizerOtpDTO dto)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new Exception("User not found.");
            if (user.RoleId != 2) throw new Exception("Invalid role.");

            string cacheKey = $"organizer_otp_{userId}";

            if (!_cache.TryGetValue(cacheKey, out string? savedOtp) || savedOtp != dto.OtpCode)
            {
                throw new Exception("Invalid or expired OTP.");
            }

            user.RoleId = 3;

            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);

            _cache.Remove(cacheKey);
            _cache.Remove($"security_stamp_{userId}");
            user.Role = await _roleRepository.GetById(3);

            var newToken = _jwtHelper.GenerateToken(user);


            return new ProfileResponseDTO
            {
                Profile = _mapper.Map<ProfileDTO>(user),
                NewToken = newToken
            };
        }
    }
}