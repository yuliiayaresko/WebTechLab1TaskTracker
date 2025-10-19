// Models/TaskDetailViewModel.cs
namespace WebTechLab1TaskTracker.Models
{
    public class TaskDetailViewModel
    {
        public Task Task { get; set; }

        public IEnumerable<Comment> Comments { get; set; }

        public string NewCommentText { get; set; }
    }
}