using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PomodoroApp.Application.Services;
using PomodoroApp.Core.DTOs.Auth;
using System.Security.Claims;
namespace PomodoroApp.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AuthController(AuthService authService, IConfiguration configuration, IWebHostEnvironment env)
        {
            _authService = authService;
            _configuration = configuration;
            _env = env;
        }

        // -- Register -----------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.RegisterAsync(dto);
                var accessToken = _authService.GenerateAccessToken(
                    await GetUserEntityAsync(user.Id));
                var refreshToken = await _authService
                    .GenerateAndSaveRefreshTokenAsync(user.Id);

                SetAuthCookies(accessToken, refreshToken);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // -- Login ---------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.LoginAsync(dto);
                var accessToken = _authService.GenerateAccessToken(
                    await GetUserEntityAsync(user.Id));
                var refreshToken = await _authService
                    .GenerateAndSaveRefreshTokenAsync(user.Id);

                SetAuthCookies(accessToken, refreshToken);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // -- Refresh -------------------------------------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawRefreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(rawRefreshToken))
                return Unauthorized(new { message = "No refresh token found." });

            try
            {
                var (user, newRefreshToken) = await _authService
                    .RefreshAsync(rawRefreshToken);
                var accessToken = _authService.GenerateAccessToken(
                    await GetUserEntityAsync(user.Id));

                SetAuthCookies(accessToken, newRefreshToken);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // -- Logout --------------------------------------------
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawRefreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(rawRefreshToken))
                await _authService.LogoutAsync(rawRefreshToken);

            // Clear both cookies
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully." });
        }

        // -- Me (get current user) -----------------------------
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var accessToken = Request.Cookies["accessToken"];

            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized(new { message = "Not logged in." });

            try
            {
                var userId = _authService.GetUserIdFromToken(accessToken);
                var user = await _authService.GetCurrentUserAsync(userId);
                return Ok(user);
            }
            catch
            {
                return Unauthorized(new { message = "Invalid token." });
            }
        }

        // -- Private Helpers -----------------------------------
        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, 
                SameSite = SameSiteMode.None, 
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("accessToken", accessToken, cookieOptions);
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private async Task<PomodoroApp.Core.Entities.User> GetUserEntityAsync(Guid userId)
        {
            return await _authService.GetUserEntityAsync(userId);
        }

        // -- Google OAuth ------------------------------------------
        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "https://localhost:7252/api/auth/google/callback"
            };
            return Challenge(properties, "Google");
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");

            if (!result.Succeeded)
                return Unauthorized(new { message = "Google authentication failed." });

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (email == null || providerId == null)
                return Unauthorized(new { message = "Could not retrieve Google profile." });

            var user = await _authService.HandleOAuthAsync(
                email, name ?? email, providerId, Core.Entities.AuthProvider.Google);

            var accessToken = _authService.GenerateAccessToken(
                await GetUserEntityAsync(user.Id));
            var refreshToken = await _authService
                .GenerateAndSaveRefreshTokenAsync(user.Id);

            SetAuthCookies(accessToken, refreshToken);

            var frontendUrl = _configuration["Frontend:BaseUrl"];
            return Redirect($"{frontendUrl}/home");
        }

        // -- GitHub OAuth ------------------------------------------
        [HttpGet("github")]
        public IActionResult GitHubLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GitHubCallback")
            };
            return Challenge(properties, "GitHub");
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");

            if (!result.Succeeded)
                return Unauthorized(new { message = "GitHub authentication failed." });

            var claims = result.Principal.Claims.ToList();
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (providerId == null)
                return Unauthorized(new { message = "Could not retrieve GitHub profile." });

            var user = await _authService.HandleOAuthAsync(
                email ?? $"github_{providerId}@placeholder.com",
                name ?? $"GitHub User {providerId}",
                providerId,
                Core.Entities.AuthProvider.GitHub);

            var accessToken = _authService.GenerateAccessToken(
                await GetUserEntityAsync(user.Id));
            var refreshToken = await _authService
                .GenerateAndSaveRefreshTokenAsync(user.Id);

            SetAuthCookies(accessToken, refreshToken);

            var frontendUrl = _configuration["Frontend:BaseUrl"];
            return Redirect($"{frontendUrl}/home");
        }
    }
}