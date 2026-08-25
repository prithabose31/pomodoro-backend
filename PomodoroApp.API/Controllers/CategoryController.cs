using Microsoft.AspNetCore.Mvc;
using PomodoroApp.Application.Services;
using PomodoroApp.Core.DTOs.Category;
using System.IdentityModel.Tokens.Jwt;

namespace PomodoroApp.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;
        private readonly AuthService _authService;
        public CategoryController(CategoryService categoryService, AuthService authService)
        {
            _categoryService = categoryService;
            _authService = authService;
        }

        // ── GET /api/categories ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var categories = await _categoryService.GetAllAsync(userId.Value);
            return Ok(categories);
        }

        // ── POST /api/categories ──────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var category = await _categoryService.CreateAsync(dto, userId.Value);
            return CreatedAtAction(nameof(GetAll), category);
        }

        // ── PUT /api/categories/{id} ──────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            try
            {
                var category = await _categoryService.UpdateAsync(id, dto, userId.Value);
                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── DELETE /api/categories/{id} ───────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            try
            {
                await _categoryService.DeleteAsync(id, userId.Value);
                return Ok(new { message = "Category deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── Helper — extract userId from JWT cookie ───────────
        private Guid? GetUserId()
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                return _authService.GetUserIdFromToken(token);
            }
            catch
            {
                return null;
            }
        }
    }
}