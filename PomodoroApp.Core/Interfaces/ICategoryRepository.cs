using PomodoroApp.Core.Entities;

namespace PomodoroApp.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllByUserIdAsync(Guid userId);
        Task<Category?> GetByIdAsync(Guid id, Guid userId);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<bool> ExistsAsync(Guid id, Guid userId);
    }
}