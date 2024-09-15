using System.Net;
using Microsoft.EntityFrameworkCore;
using TaskMonitor.Context;
using TaskMonitor.Enums;
using TaskMonitor.Exceptions;

namespace TaskMonitor.Services
{
    public interface IProjectService
    {
        Task<Models.Project> CreateAsync(Models.Project model);
        Task<IEnumerable<Models.Project>> ReadAllAsync(bool includeTasks = false);
        Task<Models.Project> ReadByIdAsync(Guid id);
        Task SetDeletedAtByIdAsync(Guid id);
        Task<Models.Project> UpdateByIdAsync(Guid id, Models.Project model);
    }

    public class ProjectService(AppDbContext context) : IProjectService
    {
        private readonly AppDbContext _context = context;

        public async Task<Models.Project> CreateAsync(Models.Project model)
        {
            _context.Projects.Add(model);

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var task = await ReadByIdAsync(id);
            _context.Projects.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Models.Project>> ReadAllAsync(bool includeTasks = false)
        {
            var projects = _context
                .Projects.AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(t => t.DeletedAt == null);

            if (includeTasks)
                projects = projects.Include(t => t.Tasks);

            return await projects.ToArrayAsync();
        }

        public async Task<Models.Project> ReadByIdAsync(Guid id)
        {
            if (await _context.Projects.FindAsync(id) is not Models.Project project)
                throw new ServiceException(
                    HttpStatusCode.NotFound,
                    ModalTheme.Warning,
                    "Task not found",
                    "The system couldn't locate the specified task"
                );

            if (project.DeletedAt is not null)
                throw GetDeletedWarning();

            return project;
        }

        public async Task SetDeletedAtByIdAsync(Guid id)
        {
            var project = await ReadByIdAsync(id);

            project.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<Models.Project> UpdateByIdAsync(Guid id, Models.Project model)
        {
            if (id != model.Id)
                throw ServiceException.InvalidRequestData;

            var project = await ReadByIdAsync(id);

            if (project.DeletedAt is not null)
                throw GetDeletedWarning();

            _context.Entry(project).CurrentValues.SetValues(model);
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return project;
        }

        private static ServiceException GetDeletedWarning() =>
            new(
                HttpStatusCode.BadRequest,
                ModalTheme.Warning,
                "The project was deleted",
                "The specified project is not accessible"
            );
    }
}
