namespace WebTechLab1TaskTracker.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskId { get; set; }
        public string AuthorUserName { get; set; }
        public string ApplicationUserId { get; set; } // <-- Додайте цей рядок
    }
}