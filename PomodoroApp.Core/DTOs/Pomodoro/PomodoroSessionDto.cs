namespace PomodoroApp.Core.DTOs.Pomodoro
{
    public class LogSessionDto
    {
        public Guid? TaskId { get; set; }
        public int WorkMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int CyclesCompleted { get; set; }
        public int TotalMinutesLogged { get; set; }
    }

    public class PomodoroSessionDto
    {
        public Guid Id { get; set; }
        public Guid? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public int WorkMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int CyclesCompleted { get; set; }
        public int TotalMinutesLogged { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}