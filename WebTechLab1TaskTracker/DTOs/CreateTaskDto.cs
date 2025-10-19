
namespace WebTechLab1TaskTracker.DTOs
{
    public class CreateTaskDto
    {
        internal DateTime? Deadline;

        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ProjectId { get; set; }
        public string ApplicationUserId { get; set; } // <-- Додайте цей рядок

    }
}