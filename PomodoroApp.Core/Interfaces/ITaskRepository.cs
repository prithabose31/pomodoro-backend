using PomodoroApp.Core.Entities;

namespace PomodoroApp.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllByUserAsync(Guid userId);
        Task<TaskItem?> GetByIdAsync(Guid id, Guid userId);
        Task<TaskItem> CreateAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task DeleteAsync(Guid id);
        Task AddSubtasksAsync(List<Subtask> subtasks);
        Task DeleteSubtasksByTaskAsync(Guid taskId);
        Task LogTimeAsync(Guid taskId, int minutes);
        Task UpdateWeeklyGoalAsync(Guid taskId, Guid userId, int weeklyGoalMinutes);
    }
}