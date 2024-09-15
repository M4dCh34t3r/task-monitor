using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskMonitor.Models;
using TaskMonitor.Utils;

namespace TaskMonitor.Context
{
    public class AppDbContext(DbContextOptions options, IConfiguration configuration)
        : DbContext(options)
    {
        private readonly IConfiguration _configuration = configuration;

        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<TimeTracker> TimeTrackers { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Collaborator collaboratorOne = new() { Name = "Collab. One" };
            Collaborator collaboratorTwo = new() { Name = "Collab. Two" };

            User userAdmin =
                new()
                {
                    Password = HashUtil.ComputePBKDF2(
                        "admin",
                        _configuration.GetValue<string>("Secrets:Salts:PBKDF2")!
                    ),
                    UserName = "Admin",
                };
            User userGuest =
                new()
                {
                    Password = HashUtil.ComputePBKDF2(
                        "guest",
                        _configuration.GetValue<string>("Secrets:Salts:PBKDF2")!
                    ),
                    UserName = "Guest",
                };

            modelBuilder.UseCollation("Latin1_General_CI_AI");

            modelBuilder.Entity<Collaborator>().Property(c => c.Id).HasDefaultValueSql("NEWID()");
            modelBuilder
                .Entity<Collaborator>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder
                .Entity<Collaborator>()
                .HasMany(c => c.TimeTrackers)
                .WithOne(t => t.Collaborator)
                .HasForeignKey(t => t.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>().Property(p => p.Id).HasDefaultValueSql("NEWID()");
            modelBuilder
                .Entity<Project>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder
                .Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Task>().Property(t => t.Id).HasDefaultValueSql("NEWID()");
            modelBuilder
                .Entity<Models.Task>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            modelBuilder
                .Entity<Models.Task>()
                .HasMany(t => t.TimeTrackers)
                .WithOne(t => t.Task)
                .HasForeignKey(t => t.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeTracker>().Property(t => t.Id).HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().Property(c => c.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Collaborator>().HasData(collaboratorOne);
            modelBuilder.Entity<Collaborator>().HasData(collaboratorTwo);

            modelBuilder.Entity<User>().HasData(userAdmin);
            modelBuilder.Entity<User>().HasData(userGuest);
        }

        public string[] GetMigrationNames(bool includePending)
        {
            var migrationsAssembly = this.GetService<IMigrationsAssembly>();
            var history = this.GetService<IHistoryRepository>();

            string[] applied = history.Exists()
                ? [.. history.GetAppliedMigrations().Select(r => r.MigrationId)]
                : [];

            return
            [
                .. includePending
                    ? migrationsAssembly.Migrations.Keys.Except(applied)
                    : migrationsAssembly.Migrations.Keys,
            ];
        }
    }
}
