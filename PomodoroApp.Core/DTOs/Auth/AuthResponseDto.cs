namespace PomodoroApp.Core.DTOs.Auth
{
    public class AuthResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AuthProvider { get; set; } = string.Empty;
    }
}