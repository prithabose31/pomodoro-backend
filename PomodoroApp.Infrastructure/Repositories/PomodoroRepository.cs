using Microsoft.EntityFrameworkCore;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;
using PomodoroApp.Infrastructure.Data;

namespace PomodoroApp.Infrastructure.Repositories
{
    public class PomodoroRepository : IPomodoroRepository
    {
        private readonly AppDbContext _context;

        public PomodoroRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PomodoroSession> LogSessionAsync(PomodoroSession session)
        {
            _context.PomodoroSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<List<PomodoroSession>> GetUserSessionsAsync(Guid userId)
        {
            return await _context.PomodoroSessions
                .Include(p => p.Task)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CompletedAt)
                .Take(20)
                .ToListAsync();
        }
    }
}