using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Models;
using AuthenticationAPI.Repositories;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;

namespace AuthenticationAPI.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;
        private readonly IOtpHelper _otpHelper;
        private readonly IOtpStorageService _otpStorage;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(
             IUserRepository userRepository,
             IRoleRepository roleRepository,
             IMapper mapper,
             IPasswordHelper passwordHelper,
             IJwtHelper jwtHelper,
             IConfiguration configuration,
             IOtpHelper otpHelper,
             IOtpStorageService otpStorage,
             IEmailService emailService,
             IMemoryCache cache)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _passwordHelper = passwordHelper;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _otpHelper = otpHelper;
            _otpStorage = otpStorage;
            _emailService = emailService;
            _cache = cache;
        }

        private async Task HandleFailedAttemptAsync(string email)
        {
            string lockoutKey = $"lockout_end_{email}";
            string attemptsKey = $"failed_login_attempts_{email}";
            string tierKey = $"lockout_tier_{email}";

            int attempts = _cache.Get<int>(attemptsKey);
            attempts++;

            if (attempts >= 5)
            {
                int tier = 1;
                if (_cache.TryGetValue(tierKey, out int savedTier))
                {
                    tier = savedTier;
                }

                int lockoutMinutes = tier * 5;
                _cache.Set(lockoutKey, DateTime.UtcNow.AddMinutes(lockoutMinutes), TimeSpan.FromMinutes(lockoutMinutes));

                tier++;
                _cache.Set(tierKey, tier, TimeSpan.FromHours(24));
                _cache.Remove(attemptsKey);

                await _emailService.SendEmailAsync(
                    email,
                    "Security Alert: Unusual Login Attempts Detected",
                    $@"
                    <h2>Security Alert</h2>
                    <p>We detected 5 consecutive failed login attempts on your account.</p>
                    <p>For your security, your account login has been temporarily locked for {lockoutMinutes} minute(s).</p>
                    <p>If this was not you, please secure your account immediately.</p>
                    "
                );

                throw new Exception($"Too many failed login attempts. Your account is locked for {lockoutMinutes} minute(s).");
            }
            else
            {
                _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(15));
                throw new Exception($"Invalid email or password. Attempt ({attempts}/5).");
            }
        }

        public async Task<AuthResponseDTO> Login(LoginDTO dto)
        {
            string lockoutKey = $"lockout_end_{dto.Email}";

            if (_cache.TryGetValue(lockoutKey, out DateTime lockoutEnd) && lockoutEnd > DateTime.UtcNow)
            {
                var remaining = (int)Math.Ceiling((lockoutEnd - DateTime.UtcNow).TotalMinutes);
                throw new Exception($"Account is temporarily locked. Please try again in {remaining} minute(s).");
            }

            var user = await _userRepository.GetByEmail(dto.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            if (user.IsActive == false)
                throw new Exception("Account has been locked.");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                await HandleFailedAttemptAsync(dto.Email);
            }

            var isValidPassword = _passwordHelper.VerifyPassword(user.PasswordHash, dto.Password);

            if (!isValidPassword)
            {
                await HandleFailedAttemptAsync(dto.Email);
            }

            _cache.Remove($"failed_login_attempts_{dto.Email}");
            _cache.Remove(lockoutKey);
            _cache.Remove($"lockout_tier_{dto.Email}");

            var accessToken = _jwtHelper.GenerateToken(user);
            var refreshTokenString = _jwtHelper.GenerateRefreshTokenString();
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _userRepository.AddRefreshToken(refreshToken);

            return new AuthResponseDTO
            {
                Token = accessToken,
                RefreshToken = refreshTokenString,
                User = _mapper.Map<ReadUserDTO>(user)
            };
        }

        public async Task<AuthResponseDTO> GoogleLogin(GoogleLoginDTO dto)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                dto.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

            if (!payload.EmailVerified)
                throw new Exception("Email not verified");

            var user = await _userRepository.GetByEmail(payload.Email);

            if (user == null)
            {
                var role = await _roleRepository.GetByName("customer");

                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    RoleId = role.RoleId,
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                await _userRepository.Add(user);

                user = await _userRepository.GetByEmail(payload.Email);
            }
            if (user.IsActive == false)
                throw new Exception("Account has been locked");

            var accessToken = _jwtHelper.GenerateToken(user);
            var refreshTokenString = _jwtHelper.GenerateRefreshTokenString();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            await _userRepository.AddRefreshToken(refreshToken);

            return new AuthResponseDTO
            {
                Token = accessToken,
                RefreshToken = refreshTokenString,
                User = _mapper.Map<ReadUserDTO>(user)
            };
        }

        public async Task Register(RegisterDTO dto)
        {
            var existingUser = await _userRepository.GetByEmail(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already exists");

            if (!dto.Password.Equals(dto.ConfirmPassword))
            {
                throw new Exception("Password and Confirm Password do not match");
            }

            var user = _mapper.Map<User>(dto);

            user.PasswordHash = _passwordHelper.HashPassword(dto.Password);
            user.RoleId = 2;
            user.IsActive = true;
            user.IsEmailVerified = false;
            user.CreatedAt = DateTime.UtcNow;
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Add(user);
        }

        public async Task ForgotPassword(string email)
        {
            var user = await _userRepository.GetByEmail(email);

            if (user == null)
                return;

            if (user.IsActive == false)
                return;

            if (user.PasswordHash == null)
                return;

            var otp = _otpHelper.GenerateOtp();

            _otpStorage.SaveOtp(email, otp, 5);

            await _emailService.SendEmailAsync(
                email,
                "Password Reset OTP",
                $@"
                <h2>Password Reset</h2>
                <p>Your OTP is: <strong>{otp}</strong></p>
                <p>This code will expire in 5 minutes.</p>
                "
            );
        }

        public async Task ResetPassword(ResetPasswordDTO dto)
        {
            var user = await _userRepository.GetByEmail(dto.Email);

            if (user == null)
                throw new Exception("Invalid request");

            if (user.IsActive == false)
                throw new Exception("Invalid request");

            if (user.PasswordHash == null)
                throw new Exception("Invalid request");

            if (!dto.NewPassword.Equals(dto.ConfirmPassword))
                throw new Exception("Invalid request");

            string cacheKey = $"failed_otp_attempts_{dto.Email}";
            int failedCount = _cache.Get<int>(cacheKey);

            var isValidOtp = _otpStorage.ValidateOtp(dto.Email, dto.Otp);

            if (!isValidOtp)
            {
                failedCount++;
                _cache.Set(cacheKey, failedCount, TimeSpan.FromMinutes(10));

                if (failedCount >= 5)
                {
                    _cache.Remove(cacheKey);

                    await _emailService.SendEmailAsync(
                        dto.Email,
                        "Security Alert: Unusual Access Detected",
                        $@"
                        <h2>Security Alert</h2>
                        <p>We detected 5 consecutive incorrect OTP attempts while resetting the password for your account.</p>
                        <p>There might be an unrecognized device attempting unauthorized access. If this was not you, please secure your account immediately.</p>
                        "
                    );

                    throw new Exception("Too many incorrect OTP attempts. A security alert has been sent to your email.");
                }

                throw new Exception($"Invalid or expired OTP. Attempt ({failedCount}/5).");
            }

            _cache.Remove(cacheKey);

            user.PasswordHash = _passwordHelper.HashPassword(dto.NewPassword);

            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);
        }

        public async Task Logout(int userId)
        {
            await _userRepository.Logout(userId);
        }

        public async Task<AuthResponseDTO> RefreshToken(string refreshTokenString)
        {
            var storedToken = await _userRepository.GetRefreshToken(refreshTokenString);

            if (storedToken == null)
                throw new Exception("Refresh token does not exist.");

            if (storedToken.IsRevoked == true)
                throw new Exception("Refresh token has been revoked.");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Refresh token has expired.");

            var user = storedToken.User;

            if (user == null)
                throw new Exception("User associated with this token was not found.");

            if (user.IsActive != true)
                throw new Exception("User account is inactive or locked.");

            var newAccessToken = _jwtHelper.GenerateToken(user);
            var newRefreshTokenString = _jwtHelper.GenerateRefreshTokenString();

            storedToken.Token = newRefreshTokenString;
            storedToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

            await _userRepository.Update(user);

            return new AuthResponseDTO
            {
                Token = newAccessToken,
                RefreshToken = newRefreshTokenString,
                User = _mapper.Map<ReadUserDTO>(user)
            };
        }
    }
}