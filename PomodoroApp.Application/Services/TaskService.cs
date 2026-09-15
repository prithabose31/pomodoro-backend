using PomodoroApp.Core.DTOs.Tasks;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using TaskStatus = PomodoroApp.Core.Entities.TaskStatus;

namespace PomodoroApp.Application.Services
{
    public class TaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskDto>> GetAllAsync(Guid userId)
        {
            var tasks = await _taskRepository.GetAllByUserAsync(userId);
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskDto> CreateAsync(Guid userId, CreateTaskDto dto)
        {
            if (!Enum.TryParse<TaskStatus>(dto.Status, out var status))
                status = TaskStatus.New;

            var task = new TaskItem
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Description = dto.Description,
                Status = status,
                WeeklyGoalMinutes = dto.WeeklyGoalMinutes,
                Subtasks = dto.Subtasks.Select(s => new Subtask
                {
                    Title = s.Title,
                    IsCompleted = s.IsCompleted
                }).ToList()
            };

            var created = await _taskRepository.CreateAsync(task);

            // Reload with includes
            var full = await _taskRepository.GetByIdAsync(created.Id, userId);
            return MapToDto(full!);
        }

        public async Task<TaskDto> UpdateAsync(Guid id, Guid userId, UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException("Task not found.");

            if (!Enum.TryParse<TaskStatus>(dto.Status, out var status))
                status = TaskStatus.New;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = status;
            task.CategoryId = dto.CategoryId;
            task.WeeklyGoalMinutes = dto.WeeklyGoalMinutes;

            // Replace subtasks
            await _taskRepository.DeleteSubtasksByTaskAsync(id);
            await _taskRepository.AddSubtasksAsync(
                dto.Subtasks.Select(s => new Subtask
                {
                    TaskId = id,
                    Title = s.Title,
                    IsCompleted = s.IsCompleted
                }).ToList()
            );

            await _taskRepository.UpdateAsync(task);

            var updated = await _taskRepository.GetByIdAsync(id, userId);
            return MapToDto(updated!);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(id, userId)
                ?? throw new KeyNotFoundException("Task not found.");
            await _taskRepository.DeleteAsync(id);
        }

        public async Task LogTimeAsync(Guid taskId, Guid userId, int minutes)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, userId)
                ?? throw new KeyNotFoundException("Task not found.");
            await _taskRepository.LogTimeAsync(taskId, minutes);
        }

        private static TaskDto MapToDto(TaskItem task) => new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            CategoryId = task.CategoryId,
            CategoryName = task.Category?.Name,
            CategoryColor = task.Category?.Color,
            CategoryEmoji = task.Category?.Emoji,
            WeeklyGoalMinutes = task.WeeklyGoalMinutes,
            TimeSpentMinutes = task.TimeSpentMinutes,
            CreatedAt = task.CreatedAt,
            Subtasks = task.Subtasks.Select(s => new SubtaskDto
            {
                Id = s.Id,
                Title = s.Title,
                IsCompleted = s.IsCompleted
            }).ToList()
        };

        public async Task UpdateWeeklyGoalAsync(
        Guid taskId,
        Guid userId,
        int weeklyGoalMinutes)
        {
             if (weeklyGoalMinutes < 0)
             throw new ArgumentException("Weekly goal cannot be negative.");

                await _taskRepository.UpdateWeeklyGoalAsync(
                    taskId,
                    userId,
                    weeklyGoalMinutes
                );
        }
    }
}