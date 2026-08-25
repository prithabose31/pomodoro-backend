using PomodoroApp.Core.DTOs.Category;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;

namespace PomodoroApp.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync(Guid userId)
        {
            var categories = await _categoryRepository.GetAllByUserIdAsync(userId);
            return categories.Select(MapToResponse).ToList();
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryDto dto, Guid userId)
        {
            var category = new Category
            {
                UserId = userId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Emoji = dto.Emoji,
                Color = dto.Color
            };

            var saved = await _categoryRepository.CreateAsync(category);
            return MapToResponse(saved);
        }

        public async Task<CategoryResponseDto> UpdateAsync(Guid id, CategoryDto dto, Guid userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException("Category not found.");

            category.Name = dto.Name.Trim();
            category.Description = dto.Description?.Trim();
            category.Emoji = dto.Emoji;
            category.Color = dto.Color;

            var updated = await _categoryRepository.UpdateAsync(category);
            return MapToResponse(updated);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var deleted = await _categoryRepository.DeleteAsync(id, userId);
            if (!deleted)
                throw new KeyNotFoundException("Category not found.");
        }

        // ── Helper ────────────────────────────────────────────
        private static CategoryResponseDto MapToResponse(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Emoji = category.Emoji,
                Color = category.Color,
                TaskCount = category.Tasks?.Count ?? 0,  // ← real count now
                CreatedAt = category.CreatedAt
            };
        }
    }
}