using AuthenticationAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;

namespace AuthenticationAPI.Repositories.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public IQueryable<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Role)
                .AsQueryable();
        }

        public Task<User?> GetById(int id)
        {
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public Task<User?> GetByEmail(string email)
        {
            return _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task Add(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public Task<bool> IsExist(int id)
        {
            return _context.Users.AnyAsync(u => u.UserId == id);
        }

        public async Task Logout(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.SecurityStamp = Guid.NewGuid().ToString();

                var refreshTokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
                foreach (var token in refreshTokens)
                {
                    _context.RefreshTokens.Remove(token);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshToken(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .Include(rt => rt.User.Role)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task UpdateRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
