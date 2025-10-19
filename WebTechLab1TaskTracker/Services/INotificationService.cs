namespace WebTechLab1TaskTracker.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(long chatId, string message);
    }
}