using Microsoft.EntityFrameworkCore;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using PomodoroApp.Infrastructure.Data;

namespace PomodoroApp.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetAllByUserAsync(Guid userId)
        {
            return await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Subtasks)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.Tasks
                .Include(t => t.Category)
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(TaskItem task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddSubtasksAsync(List<Subtask> subtasks)
        {
            _context.Subtasks.AddRange(subtasks);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubtasksByTaskAsync(Guid taskId)
        {
            var subtasks = await _context.Subtasks
                .Where(s => s.TaskId == taskId)
                .ToListAsync();
            _context.Subtasks.RemoveRange(subtasks);
            await _context.SaveChangesAsync();
        }

        public async Task LogTimeAsync(Guid taskId, int minutes)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                task.TimeSpentMinutes += minutes;
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateWeeklyGoalAsync(
            Guid taskId,
            Guid userId,
            int weeklyGoalMinutes)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.UserId == userId);

            if (task == null)
                throw new KeyNotFoundException("Task not found.");

            task.WeeklyGoalMinutes = Math.Max(0, weeklyGoalMinutes);

            await _context.SaveChangesAsync();
        }
    }
}