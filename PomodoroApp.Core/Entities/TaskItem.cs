namespace PomodoroApp.Core.Entities
{
    public enum TaskStatus
    {
        New,
        Backlog,
        InProgress,
        OnHold,
        Done
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.New;

        public int WeeklyGoalMinutes { get; set; } = 0;
        public int TimeSpentMinutes { get; set; } = 0;

        public DateTime WeekStartDate { get; set; } = DateTime.UtcNow.Date;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Category? Category { get; set; }
        public ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();
    }
}