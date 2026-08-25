using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PomodoroApp.Core.DTOs.Auth;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace PomodoroApp.Application.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        // ── Register ──────────────────────────────────────────
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Check if email already exists
            if (await _authRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already in use.");

            // 2. Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Build the user object
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = passwordHash,
                AuthProvider = AuthProvider.Local,
            };

            // 4. Save user to database
            var savedUser = await _authRepository.CreateUserAsync(user);

            // 5. Return user info (tokens handled in controller via cookies)
            return MapToAuthResponse(savedUser);
        }

        // ── Login ─────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Find user by email
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);

            // 2. User not found OR wrong password → same error (security)
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // 3. Return user info
            return MapToAuthResponse(user);
        }

        // ── Generate Access Token (JWT) ───────────────────────
        public string GenerateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpiryMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim("provider", user.AuthProvider.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ── Generate Refresh Token ────────────────────────────
        public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId)
        {
            // 1. Generate a raw random token (this goes in the cookie)
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            // 2. Hash it (this goes in the database)
            var tokenHash = HashToken(rawToken);

            var expiryDays = int.Parse(
                _configuration["JwtSettings:RefreshTokenExpiryDays"]!);

            // 3. Build refresh token object
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            };

            // 4. Save hashed token to DB
            await _authRepository.CreateRefreshTokenAsync(refreshToken);

            // 5. Return RAW token (goes into HTTP-only cookie)
            return rawToken;
        }

        // ── Refresh Token ─────────────────────────────────────
        public async Task<(AuthResponseDto user, string newRefreshToken)> RefreshAsync(string rawRefreshToken)
        {
            // 1. Hash the incoming raw token to look it up in DB
            var tokenHash = HashToken(rawRefreshToken);

            // 2. Find token in DB (checks not revoked + not expired)
            var storedToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

            if (storedToken == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            // 3. Revoke the old refresh token (rotation — each token used once)
            await _authRepository.RevokeRefreshTokenAsync(storedToken.Id);

            // 4. Generate brand new refresh token
            var newRawRefreshToken = await GenerateAndSaveRefreshTokenAsync(storedToken.UserId);

            // 5. Return user info + new refresh token
            return (MapToAuthResponse(storedToken.User), newRawRefreshToken);
        }

        // ── Logout ────────────────────────────────────────────
        public async Task LogoutAsync(string rawRefreshToken)
        {
            var tokenHash = HashToken(rawRefreshToken);
            var storedToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

            if (storedToken != null)
                await _authRepository.RevokeRefreshTokenAsync(storedToken.Id);
        }

        // ── Get Current User ──────────────────────────────────
        public async Task<AuthResponseDto> GetCurrentUserAsync(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);

            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            return MapToAuthResponse(user);
        }

        // ── Helper: Hash Token ────────────────────────────────
        private static string HashToken(string rawToken)
        {
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        // ── Helper: Map User to AuthResponseDto ───────────────
        private static AuthResponseDto MapToAuthResponse(User user)
        {
            return new AuthResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AuthProvider = user.AuthProvider.ToString()
            };
        }

        // ── Get User Entity (for token generation) ────────────────
        public async Task<User> GetUserEntityAsync(Guid userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");
            return user;
        }

        // ── Get UserId From Token ─────────────────────────────────
        public Guid GetUserIdFromToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            var sub = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
            return Guid.Parse(sub);
        }

        // ── Handle OAuth (Google + GitHub) ───────────────────────
        public async Task<AuthResponseDto> HandleOAuthAsync(
            string email,
            string name,
            string providerId,
            AuthProvider provider)
        {
            // 1. Check if user already exists with this provider ID
            var existingUser = await _authRepository
                .GetUserByProviderIdAsync(providerId, provider);

            if (existingUser != null)
                return MapToAuthResponse(existingUser);

            // 2. Check if email already exists (signed up with password before)
            var userByEmail = await _authRepository.GetUserByEmailAsync(email);

            if (userByEmail != null)
            {
                // Link OAuth to existing account
                userByEmail.ProviderId = providerId;
                userByEmail.AuthProvider = provider;
                await _authRepository.UpdateUserAsync(userByEmail);
                return MapToAuthResponse(userByEmail);
            }

            // 3. Brand new user → create account
            var newUser = new User
            {
                Name = name,
                Email = email.ToLower().Trim(),
                AuthProvider = provider,
                ProviderId = providerId,
                PasswordHash = null
            };

            var savedUser = await _authRepository.CreateUserAsync(newUser);
            return MapToAuthResponse(savedUser);
        }
    }
}