using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskMonitor.Context;
using TaskMonitor.Enums;
using TaskMonitor.Exceptions;

namespace TaskMonitor.Services
{
    public interface ITimeTrackerService
    {
        Task<Models.TimeTracker> CreateAsync(Models.TimeTracker model);
        Task<IEnumerable<Models.TimeTracker>> ReadByCollaboratorIdAsync(Guid collaboratorId);
        Task<IEnumerable<Models.TimeTracker>> ReadByTaskIdAsync(Guid taskId, bool includeCollaborators);
        Task<Models.TimeTracker> SetStartDateAsync(Guid id);
        Task<Models.TimeTracker> SetEndDateAsync(Guid id);
        Task<Models.TimeTracker> UpdateByIdAsync(Guid id, Models.TimeTracker model);
    }

    public class TimeTrackerService(AppDbContext context) : ITimeTrackerService
    {
        private readonly AppDbContext _context = context;

        public async Task<Models.TimeTracker> CreateAsync(Models.TimeTracker model)
        {
            ValidateDateOrder(model);

            if (
                await _context.TimeTrackers.AnyAsync(t =>
                    t.StartDate < model.EndDate
                    && t.EndDate > model.StartDate
                    && t.TaskId == model.TaskId
                )
            )
                throw GetUnableToCreateWarning(
                    "There is already an active time tracker between this interval"
                );

            _context.TimeTrackers.Add(model);

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IEnumerable<Models.TimeTracker>> ReadByCollaboratorIdAsync(
            Guid collaboratorId
        ) =>
            await _context
                .TimeTrackers.AsNoTracking()
                .Where(t => t.CollaboratorId == collaboratorId)
                .ToArrayAsync();

        public async Task<IEnumerable<Models.TimeTracker>> ReadByTaskIdAsync(Guid taskId, bool includeCollaborator)
        {
            var timeTrackers = _context
                .TimeTrackers.AsNoTracking()
                .Where(t => t.TaskId == taskId)
                .IgnoreAutoIncludes();

            if (includeCollaborator)
                timeTrackers = timeTrackers.Include(t => t.Collaborator);

            return await timeTrackers.ToArrayAsync();
        }

        public async Task<Models.TimeTracker> SetStartDateAsync(Guid id)
        {
            var timeTracker = await ReadByIdAsync(id);

            if (timeTracker.EndDate is not null)
                throw GetAlreadyInactiveInfo();

            if (timeTracker.StartDate is not null)
                throw GetActiveInfo();

            var date = DateTime.Now.Date;
            var ticks = _context
                .TimeTrackers.AsNoTracking()
                .Where(t => t.StartDate.HasValue && t.CollaboratorId == timeTracker.CollaboratorId)
                .AsEnumerable()
                .Where(t => t.StartDate!.Value >= date)
                .Sum(t => date.Ticks - t.StartDate!.Value.Ticks);

            if (TimeSpan.FromTicks(ticks) > TimeSpan.FromHours(24))
                throw GetNotStartedInfo();

            timeTracker.StartDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return timeTracker;
        }

        public async Task<Models.TimeTracker> SetEndDateAsync(Guid id)
        {
            var timeTracker = await ReadByIdAsync(id);

            if (timeTracker.StartDate is null)
                throw GetNotActiveInfo();

            if (timeTracker.EndDate is not null)
                throw GetAlreadyInactiveInfo();

            timeTracker.EndDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return timeTracker;
        }

        public async Task<Models.TimeTracker> ReadByIdAsync(Guid id)
        {
            if (await _context.TimeTrackers.FindAsync(id) is not Models.TimeTracker timeTracker)
                throw new ServiceException(
                    HttpStatusCode.NotFound,
                    ModalTheme.Warning,
                    "Time tracker not found",
                    "The system couldn't locate the specified time tracker"
                );

            return timeTracker;
        }

        public async Task<Models.TimeTracker> UpdateByIdAsync(Guid id, Models.TimeTracker model)
        {
            if (id != model.Id)
                throw ServiceException.InvalidRequestData;

            bool hasStartDate = model.StartDate is not null;
            bool hasEndDate = model.EndDate is not null;

            var timeTracker = await ReadByIdAsync(id);

            if (hasEndDate)
            {
                if (timeTracker.StartDate is null)
                    throw GetNotActiveInfo();

                if (timeTracker.EndDate is not null)
                    throw GetAlreadyInactiveInfo();

                timeTracker.EndDate = ConvertDate(model.EndDate!.Value);
            }
            else if (hasStartDate)
            {
                if (timeTracker.EndDate is not null)
                    throw GetAlreadyInactiveInfo();
                else
                    model.EndDate = DateTime.UtcNow;

                if (timeTracker.StartDate is not null)
                    throw GetActiveInfo();

                var date = DateTime.Now.Date;
                var ticks = _context
                    .TimeTrackers.AsNoTracking()
                    .Where(t =>
                        t.StartDate.HasValue && t.CollaboratorId == timeTracker.CollaboratorId
                    )
                    .AsEnumerable()
                    .Where(t => t.StartDate!.Value >= date)
                    .Sum(t => date.Ticks - t.StartDate!.Value.Ticks);

                if (TimeSpan.FromTicks(ticks) > TimeSpan.FromHours(24))
                    throw GetNotStartedInfo();

                timeTracker.StartDate = ConvertDate(model.StartDate!.Value);
            }
            else
                throw new ServiceException(
                    HttpStatusCode.BadRequest,
                    ModalTheme.Info,
                    "Time tracker not updated",
                    "The specified time tracker does not contain any significant change"
                );

            ValidateDateOrder(model);

            await _context.SaveChangesAsync();
            return timeTracker;
        }

        private static DateTime ConvertDate(DateTime dateTime) =>
            dateTime.Kind == DateTimeKind.Local || dateTime.Kind == DateTimeKind.Unspecified
                ? dateTime.ToUniversalTime()
                : dateTime;

        private static ServiceException GetActiveInfo() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Info,
                "The time tracker is active",
                "The specified time tracker has already started it's processing"
            );

        private static ServiceException GetAlreadyInactiveInfo() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Info,
                "Time tracker already inactive",
                "The specified time tracker has reached it's end date"
            );

        private static ServiceException GetNotStartedInfo() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Info,
                "The time tracker cannot be started",
                "The total tracked time in the current period exceed the 24-hour limit"
            );

        private static ServiceException GetNotActiveInfo() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Info,
                "The time tracker is not active",
                "The specified time tracker hasn't started it's processing"
            );

        private static ServiceException GetUnableToCreateWarning(string text) =>
            throw new ServiceException(
                HttpStatusCode.BadRequest,
                ModalTheme.Warning,
                "Unable to create time tracker",
                text
            );

        private static void ValidateDateOrder(Models.TimeTracker model)
        {
            if (model.EndDate < model.StartDate)
                throw GetUnableToCreateWarning("The end date cannot be lesser than the start date");
        }
    }
}
