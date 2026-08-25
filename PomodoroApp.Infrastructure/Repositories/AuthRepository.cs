using Microsoft.EntityFrameworkCore;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using PomodoroApp.Infrastructure.Data;

namespace PomodoroApp.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── User Operations ───────────────────────────────────

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByProviderIdAsync(string providerId, AuthProvider provider)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.ProviderId == providerId &&
                    u.AuthProvider == provider);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // ── Refresh Token Operations ──────────────────────────

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == tokenHash &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow);
        }

        public async Task RevokeRefreshTokenAsync(Guid tokenId)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == tokenId);

            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}