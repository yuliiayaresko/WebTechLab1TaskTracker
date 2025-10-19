using Telegram.Bot;
using WebTechLab1TaskTracker.Services;

namespace WebTechLab1TaskTracker.Services
{
    public class TelegramNotificationService : INotificationService
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramNotificationService(IConfiguration configuration)
        {
            var token = configuration["TelegramBotToken"];
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "TelegramBotToken cannot be null or empty in configuration.");
            }
            _botClient = new TelegramBotClient(token);
        }

        public async Task SendNotificationAsync(long chatId, string message)
        {
            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: message
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending Telegram message to Chat ID {chatId}: {ex.Message}");
            }
        }
    }
}