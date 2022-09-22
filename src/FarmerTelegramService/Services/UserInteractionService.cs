using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace FarmerTelegramService.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        private protected readonly ITelegramBotClient _botClient;
        private protected readonly CancellationToken _cancellationToken;

        public UserInteractionService(ITelegramBotClient botClient, CancellationToken cancellationToken)
        {
            _botClient = botClient;
            _cancellationToken = cancellationToken;
        }
        public async Task ShowMessageToUser(Chat tgChat, string message, IReplyMarkup? keyboardMarkup = null, bool isHtml = false)
        {
            if (tgChat is not null)
                await _botClient.SendTextMessageAsync(tgChat,
                    text: message,
                    cancellationToken: _cancellationToken,
                    replyMarkup: keyboardMarkup,
                    parseMode: isHtml ? ParseMode.Html : null);
        }

        public async Task SendDocumentAsync(Chat tgChat, InputOnlineFile file)
        {
            if (tgChat is not null)
                await _botClient.SendDocumentAsync(tgChat.Id, file);
        }

        public async Task DownloadFileAsync(string filePath, FileStream fileStream)
        {
            await _botClient.DownloadFileAsync(filePath, fileStream, _cancellationToken);
        }

        public async Task<Telegram.Bot.Types.File> GetFileAsync(string fileId)
        {
            return await _botClient.GetFileAsync(fileId, _cancellationToken);
        }
    }
}
