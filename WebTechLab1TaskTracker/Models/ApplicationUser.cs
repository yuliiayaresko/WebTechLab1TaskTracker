using Microsoft.AspNetCore.Identity;

namespace WebTechLab1TaskTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Project> Projects { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
