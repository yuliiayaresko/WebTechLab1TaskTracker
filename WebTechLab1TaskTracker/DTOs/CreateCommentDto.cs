public class CreateCommentDto
{
    public string Content { get; set; }
    public int TaskId { get; set; }
    public string ApplicationUserId { get; set; } // <-- Додайте цей рядок

}