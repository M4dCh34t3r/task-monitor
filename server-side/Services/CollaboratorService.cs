using Microsoft.EntityFrameworkCore;
using TaskMonitor.Context;

namespace TaskMonitor.Services
{
    public interface ICollaboratorService
    {
        Task<IEnumerable<Models.Collaborator>> ReadAllAsync();
    }

    public class CollaboratorService(AppDbContext context) : ICollaboratorService
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<Models.Collaborator>> ReadAllAsync() =>
            await _context
                .Collaborators.AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .ToArrayAsync();
    }
}
