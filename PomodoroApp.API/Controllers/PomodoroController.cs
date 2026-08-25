using Microsoft.AspNetCore.Mvc;
using PomodoroApp.Application.Services;
using PomodoroApp.Core.DTOs.Pomodoro;
using System.IdentityModel.Tokens.Jwt;

namespace PomodoroApp.API.Controllers
{
    [ApiController]
    [Route("api/pomodoro")]
    public class PomodoroController : ControllerBase
    {
        private readonly PomodoroService _pomodoroService;

        public PomodoroController(PomodoroService pomodoroService)
        {
            _pomodoroService = pomodoroService;
        }

        private Guid GetUserId()
        {
            var accessToken = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(accessToken))
                throw new UnauthorizedAccessException();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);
            var sub = jwt.Claims.First(c =>
                c.Type == JwtRegisteredClaimNames.Sub).Value;
            return Guid.Parse(sub);
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogSession(
            [FromBody] LogSessionDto dto)
        {
            try
            {
                var userId = GetUserId();
                var session = await _pomodoroService
                    .LogSessionAsync(userId, dto);
                return Ok(session);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userId = GetUserId();
                var history = await _pomodoroService
                    .GetHistoryAsync(userId);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}