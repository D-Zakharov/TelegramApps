using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FarmerTelegramService.Services
{
    public interface IUserInteractionService
    {
        Task DownloadFileAsync(string filePath, FileStream fileStream);
        Task<Telegram.Bot.Types.File> GetFileAsync(string fileId);
        Task SendDocumentAsync(Chat tgChat, InputOnlineFile file);
        Task ShowMessageToUser(Chat tgChat, string message, IReplyMarkup? keyboardMarkup = null, bool isHtml = false);
    }
}