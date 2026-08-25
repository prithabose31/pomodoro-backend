namespace PomodoroApp.Core.DTOs.Tasks
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public string? CategoryEmoji { get; set; }
        public int WeeklyGoalMinutes { get; set; }
        public int TimeSpentMinutes { get; set; }
        public List<SubtaskDto> Subtasks { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class SubtaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}