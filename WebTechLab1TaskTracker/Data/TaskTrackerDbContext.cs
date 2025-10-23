using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebTechLab1TaskTracker.Models;
using Task = WebTechLab1TaskTracker.Models.Task;

namespace WebTechLab1TaskTracker.Data
{
    public class TaskTrackerDbContext : IdentityDbContext<ApplicationUser>
    {
        public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options)
            : base(options)
        {
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

            // --- Зв'язок: Користувач -> Проєкт ---
            modelBuilder.Entity<Project>()
                .HasOne(p => p.ApplicationUser)
                .WithMany(u => u.Projects) // Явно вказуємо, що у юзера є колекція "Projects"
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade); // ПРАВИЛЬНО: При видаленні юзера видаляємо його проєкти.

            // --- Зв'язок: Проєкт -> Завдання ---
            modelBuilder.Entity<Task>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks) // Явно вказуємо, що у проєкта є колекція "Tasks"
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // ПРАВИЛЬНО: При видаленні проєкту видаляємо його завдання.

            // --- Зв'язок: Користувач -> Завдання ---
            modelBuilder.Entity<Task>()
                .HasOne(t => t.ApplicationUser)
                .WithMany(u => u.Tasks) // Явно вказуємо, що у юзера є колекція "Tasks"
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict); // ВИПРАВЛЕНО: Забороняємо видаляти юзера, якщо у нього є завдання. Це розриває цикл конфліктів.

            // --- Зв'язок: Завдання -> Коментар ---
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Task)
                .WithMany(t => t.Comments) 
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(u => u.Comments) 
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}

