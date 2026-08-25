namespace PomodoroApp.Core.Entities
{
    public class PomodoroSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid? TaskId { get; set; }
        public int WorkMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int CyclesCompleted { get; set; }
        public int TotalMinutesLogged { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public TaskItem? Task { get; set; }
    }
}