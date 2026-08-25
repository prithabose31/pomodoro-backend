using PomodoroApp.Core.DTOs.Pomodoro;
using PomodoroApp.Core.Entities;
using PomodoroApp.Core.Interfaces;

namespace PomodoroApp.Application.Services
{
    public class PomodoroService
    {
        private readonly IPomodoroRepository _pomodoroRepository;
        private readonly ITaskRepository _taskRepository;

        public PomodoroService(
            IPomodoroRepository pomodoroRepository,
            ITaskRepository taskRepository)
        {
            _pomodoroRepository = pomodoroRepository;
            _taskRepository = taskRepository;
        }

        public async Task<PomodoroSessionDto> LogSessionAsync(
            Guid userId, LogSessionDto dto)
        {
            var session = new PomodoroSession
            {
                UserId = userId,
                TaskId = dto.TaskId,
                WorkMinutes = dto.WorkMinutes,
                BreakMinutes = dto.BreakMinutes,
                CyclesCompleted = dto.CyclesCompleted,
                TotalMinutesLogged = dto.TotalMinutesLogged
            };

            var saved = await _pomodoroRepository.LogSessionAsync(session);

            // Auto-update task time if linked
            if (dto.TaskId.HasValue && dto.TotalMinutesLogged > 0)
            {
                await _taskRepository.LogTimeAsync(
                    dto.TaskId.Value, dto.TotalMinutesLogged);
            }

            return new PomodoroSessionDto
            {
                Id = saved.Id,
                TaskId = saved.TaskId,
                WorkMinutes = saved.WorkMinutes,
                BreakMinutes = saved.BreakMinutes,
                CyclesCompleted = saved.CyclesCompleted,
                TotalMinutesLogged = saved.TotalMinutesLogged,
                CompletedAt = saved.CompletedAt
            };
        }

        public async Task<List<PomodoroSessionDto>> GetHistoryAsync(Guid userId)
        {
            var sessions = await _pomodoroRepository
                .GetUserSessionsAsync(userId);

            return sessions.Select(s => new PomodoroSessionDto
            {
                Id = s.Id,
                TaskId = s.TaskId,
                TaskTitle = s.Task?.Title,
                WorkMinutes = s.WorkMinutes,
                BreakMinutes = s.BreakMinutes,
                CyclesCompleted = s.CyclesCompleted,
                TotalMinutesLogged = s.TotalMinutesLogged,
                CompletedAt = s.CompletedAt
            }).ToList();
        }
    }
}