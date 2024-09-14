using System.ComponentModel.DataAnnotations;

namespace TaskMonitor.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(250)]
        public string UserName { get; set; } = string.Empty;

        [MinLength(4)]
        [MaxLength(512)]
        public string Password { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
