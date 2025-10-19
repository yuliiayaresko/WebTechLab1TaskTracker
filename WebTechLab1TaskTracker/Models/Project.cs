namespace WebTechLab1TaskTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApplicationUserId { get; set; }
        public string? ImagePath { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }
        public ICollection<Task>? Tasks { get; internal set; }
    }
}
