using AuthenticationAPI.DTOs;
using AuthenticationAPI.Helpers;
using AuthenticationAPI.Helpers.Implements;
using AuthenticationAPI.Models;
using AuthenticationAPI.Repositories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AuthenticationAPI.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IMemoryCache _cache; 

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            IPasswordHelper passwordHelper,
            IMemoryCache cache) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordHelper = passwordHelper;
            _cache = cache;
        }

        public IQueryable<ReadUserDTO> GetAll()
        {
            var usersQuery = _userRepository.GetAll();

            return usersQuery.ProjectTo<ReadUserDTO>(_mapper.ConfigurationProvider);
        }

        public async Task<ReadUserDTO?> GetById(int id)
        {
            var user = await _userRepository.GetById(id);
            return user == null ? null : _mapper.Map<ReadUserDTO>(user);
        }

        public async Task Create(CreateUserDTO dto)
        {
            var existingUser = await _userRepository.GetAll()
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (existingUser)
                throw new Exception("Email address is already in use.");
            var user = _mapper.Map<User>(dto);

            user.PasswordHash = _passwordHelper.HashPassword(dto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.IsEmailVerified = false;
            user.SecurityStamp = Guid.NewGuid().ToString();

            // AvatarUrl đã được Controller upload và gán vào dto.AvatarUrl trước khi gọi hàm này
            user.AvatarUrl = dto.AvatarUrl;

            await _userRepository.Add(user);
        }

        public async Task Update(int id, UpdateUserDTO dto)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) throw new KeyNotFoundException("User not found");

            // Map các trường như FullName, RoleId, IsActive...
            _mapper.Map(dto, user);

            // Gán thủ công AvatarUrl nếu trong DTO có giá trị mới (đã được Controller upload)
            if (!string.IsNullOrEmpty(dto.AvatarUrl))
            {
                user.AvatarUrl = dto.AvatarUrl;
            }
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _userRepository.Logout(id);
            await _userRepository.Update(user);
        }

        public async Task Delete(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                throw new Exception("User not found");

            await _userRepository.Delete(user);
        }

        public async Task<bool> IsExist(int id)
        {
            return await _userRepository.IsExist(id);
        }

        public async Task<PaginatedResultDTO<ReadUserDTO>> SearchAndFilter(UserSearchFilterDTO filter)
        {
            var query = _userRepository.GetAll(); // IQueryable<User>

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(u => u.FullName.Contains(filter.SearchTerm) ||
                                         u.Email.Contains(filter.SearchTerm) ||
                                         u.PhoneNumber.Contains(filter.SearchTerm));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            query = query.Where(u => u.RoleId != 1);

            if (filter.RoleId.HasValue)
            {
                if (filter.RoleId.Value != 1)
                {
                    query = query.Where(u => u.RoleId == filter.RoleId.Value);
                }
            }

            var totalCount = query.Count();
            var items = query.OrderBy(u => u.UserId)
                             .Skip((filter.PageNumber - 1) * filter.PageSize)
                             .Take(filter.PageSize)
                             .ProjectTo<ReadUserDTO>(_mapper.ConfigurationProvider)
                             .ToList();

            return new PaginatedResultDTO<ReadUserDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task LockAndUnlockAccount(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) throw new Exception("User not found");

            bool newStatus = !(user.IsActive ?? true);
            user.IsActive = newStatus;
            user.SecurityStamp = Guid.NewGuid().ToString();

            await _userRepository.Update(user);

            if (!newStatus)
            {
                await Logout(id);
            }

            _cache.Remove($"security_stamp_{id}");
        }

        public async Task Logout(int userId)
        {
            await _userRepository.Logout(userId);
        }
    }
}
