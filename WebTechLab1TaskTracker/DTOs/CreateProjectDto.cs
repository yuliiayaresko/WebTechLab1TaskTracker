namespace WebTechLab1TaskTracker.DTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApplicationUserId { get; set; } // <-- Додайте цей рядок
        
    }
}