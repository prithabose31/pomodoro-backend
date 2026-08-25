using System.ComponentModel.DataAnnotations;

namespace PomodoroApp.Core.DTOs.Category
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Emoji { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int TaskCount { get; set; }  // ← add this
    }
}