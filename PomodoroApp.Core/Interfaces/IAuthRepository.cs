using PomodoroApp.Core.Entities;

namespace PomodoroApp.Core.Interfaces
{
    public interface IAuthRepository
    {
        // User operations
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByProviderIdAsync(string providerId, AuthProvider provider);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);

        // Refresh token operations
        Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);
        Task RevokeRefreshTokenAsync(Guid tokenId);
        Task RevokeAllUserRefreshTokensAsync(Guid userId);
    }
}