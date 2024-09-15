using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskMonitor.Context;
using TaskMonitor.Enums;
using TaskMonitor.Exceptions;

namespace TaskMonitor.Services
{
    public interface ITaskService
    {
        Task<Models.Task> CreateAsync(Models.Task model);
        Task<IEnumerable<Models.Task>> FilterByIdsAsync(
            Guid? projectId,
            Guid? collaboratorId,
            bool includeProjects
        );
        Task<IEnumerable<Models.Task>> ReadAllAsync(bool includeProjects = false);
        Task<Models.Task> ReadByIdAsync(Guid id);
        Task<Models.Task> UpdateByIdAsync(Guid id, Models.Task model);
        Task SetDeletedAtByIdAsync(Guid id);
    }

    public class TaskService(AppDbContext context, ITimeTrackerService timeTrackerService)
        : ITaskService
    {
        private readonly AppDbContext _context = context;

        private readonly ITimeTrackerService _timeTrackerService = timeTrackerService;

        public async Task<Models.Task> CreateAsync(Models.Task model)
        {
            _context.Tasks.Add(model);

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<IEnumerable<Models.Task>> FilterByIdsAsync(
            Guid? projectId,
            Guid? collaboratorId,
            bool includeProjects
        )
        {
            var tasks = _context
                .Tasks.AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(t => t.DeletedAt == null);

            if (projectId is not null)
                tasks = tasks.Where(t => t.ProjectId == projectId.Value);

            if (collaboratorId is not null)
            {
                var timeTrackers = await _timeTrackerService.ReadByCollaboratorIdAsync(
                    collaboratorId.Value
                );
                tasks = tasks.Where(t => timeTrackers.Select(tt => tt.TaskId).Contains(t.Id));
            }

            return await ReadTasksAsync(tasks, includeProjects);
        }

        public async Task<IEnumerable<Models.Task>> ReadAllAsync(bool includeProjects)
        {
            var tasks = _context
                .Tasks.AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(t => t.DeletedAt == null);
            return await ReadTasksAsync(tasks, includeProjects);
        }

        public async Task<Models.Task> ReadByIdAsync(Guid id)
        {
            if (await _context.Tasks.FindAsync(id) is not Models.Task task)
                throw new ServiceException(
                    HttpStatusCode.NotFound,
                    ModalTheme.Warning,
                    "Task not found",
                    "The system couldn't locate the specified task"
                );

            if (task.DeletedAt is not null)
                throw GetDeletedWarning();

            return task;
        }

        public async Task<Models.Task> UpdateByIdAsync(Guid id, Models.Task model)
        {
            if (id != model.Id)
                throw ServiceException.InvalidRequestData;

            var task = await ReadByIdAsync(id);

            if (task.DeletedAt is not null)
                throw GetDeletedWarning();

            _context.Entry(task).CurrentValues.SetValues(model);
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task SetDeletedAtByIdAsync(Guid id)
        {
            var task = await ReadByIdAsync(id);

            task.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private static async Task<IEnumerable<Models.Task>> ReadTasksAsync(
            IQueryable<Models.Task> tasks,
            bool includeProjects = false
        )
        {
            if (includeProjects)
                tasks = tasks.Include(t => t.Project);

            return await tasks.ToArrayAsync();
        }

        private static ServiceException GetDeletedWarning() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Warning,
                "The task was deleted",
                "The specified task is not accessible"
            );
    }
}
