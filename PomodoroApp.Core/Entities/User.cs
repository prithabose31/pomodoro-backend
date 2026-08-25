namespace PomodoroApp.Core.Entities
{
    public enum AuthProvider
    {
        Local = 0,
        Google = 1,
        GitHub = 2
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
        public string? ProviderId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<RefreshToken> RefreshToken { get; set; }
            = new List<RefreshToken>();
    }
}