using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebTechLab1TaskTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Project>? Projects { get; set; }
        public ICollection<Task>? Tasks { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        [Display(Name = "Telegram Chat ID")]
        public long? TelegramChatId { get; set; }
    }
}
