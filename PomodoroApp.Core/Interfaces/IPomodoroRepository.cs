using PomodoroApp.Core.Entities;

namespace PomodoroApp.Core.Interfaces
{
    public interface IPomodoroRepository
    {
        Task<PomodoroSession> LogSessionAsync(PomodoroSession session);
        Task<List<PomodoroSession>> GetUserSessionsAsync(Guid userId);
    }
}