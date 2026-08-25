using Microsoft.EntityFrameworkCore;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using PomodoroApp.Infrastructure.Data;

namespace PomodoroApp.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.Categories
                .Include(c => c.Tasks)  // ← include tasks
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        public async Task<Category?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var category = await GetByIdAsync(id, userId);
            if (category == null) return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id, Guid userId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == id && c.UserId == userId);
        }
    }
}