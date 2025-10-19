namespace WebTechLab1TaskTracker.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TaskCount { get; set; }
        public string ApplicationUserId { get; set; } // <-- Додайте цей рядок
        

    }
}