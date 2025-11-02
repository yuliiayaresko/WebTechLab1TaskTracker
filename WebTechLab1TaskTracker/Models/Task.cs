namespace WebTechLab1TaskTracker.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }


        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
}
