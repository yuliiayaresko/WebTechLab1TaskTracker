using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Models;

namespace WebTechLab1TaskTracker.Data
{
    public class TaskTrackerDbContext : IdentityDbContext<ApplicationUser>
    {
        public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Project> Projects { get; set; }
        public DbSet<WebTechLab1TaskTracker.Models.Task> Tasks { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=WebTechLab1TaskTracker;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ApplicationUser)
                .WithMany()
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WebTechLab1TaskTracker.Models.Task>()
                .HasOne(t => t.Project)
                .WithMany()
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WebTechLab1TaskTracker.Models.Task>()
                .HasOne(t => t.ApplicationUser)
                .WithMany()
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany()
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ApplicationUser)
                .WithMany()
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

