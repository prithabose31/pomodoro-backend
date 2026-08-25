using System.ComponentModel.DataAnnotations;

namespace PomodoroApp.Core.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string Status { get; set; } = "New";
        public Guid? CategoryId { get; set; }
        public int WeeklyGoalMinutes { get; set; } = 0;
        public List<CreateSubtaskDto> Subtasks { get; set; } = new();
    }

    public class CreateSubtaskDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
    }

    public class UpdateTaskDto : CreateTaskDto { }

    public class LogTimeDto
    {
        [Range(1, 480)]
        public int Minutes { get; set; }
    }
}