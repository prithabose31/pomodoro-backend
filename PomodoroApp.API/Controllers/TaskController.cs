using Microsoft.AspNetCore.Mvc;
using PomodoroApp.Application.Services;
using PomodoroApp.Core.DTOs.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace PomodoroApp.API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetUserId();
                var tasks = await _taskService.GetAllAsync(userId);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = GetUserId();
                var task = await _taskService.CreateAsync(userId, dto);
                return Ok(task);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = GetUserId();
                var task = await _taskService.UpdateAsync(id, userId, dto);
                return Ok(task);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _taskService.DeleteAsync(id, userId);
                return Ok(new { message = "Task deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("{id}/log-time")]
        public async Task<IActionResult> LogTime(Guid id, [FromBody] LogTimeDto dto)
        {
            try
            {
                var userId = GetUserId();
                await _taskService.LogTimeAsync(id, userId, dto.Minutes);
                return Ok(new { message = "Time logged." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
        [HttpPut("{id}/weekly-goal")]
        public async Task<IActionResult> UpdateWeeklyGoal(
        Guid id,
        [FromBody] UpdateWeeklyGoalDto dto)
        {
            try
            {
                var userId = GetUserId();

                await _taskService.UpdateWeeklyGoalAsync(
                    id,
                    userId,
                    dto.WeeklyGoalMinutes
                );

                return Ok(new
                {
                    taskId = id,
                    weeklyGoalMinutes = dto.WeeklyGoalMinutes
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}