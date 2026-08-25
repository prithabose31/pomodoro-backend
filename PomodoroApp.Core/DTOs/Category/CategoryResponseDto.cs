namespace PomodoroApp.Core.DTOs.Category
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}