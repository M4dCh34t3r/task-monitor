using System.ComponentModel.DataAnnotations;

namespace TaskMonitor.Models
{
    public class TimeTracker
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public Guid CollaboratorId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [MaxLength(200)]
        public string TimeZoneId { get; set; } = string.Empty;

        public Task? Task { get; set; }
        public Collaborator? Collaborator { get; set; }
    }
}
