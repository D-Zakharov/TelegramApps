using FarmerTelegramService.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FarmerTelegramService.CommandHandlers;

internal interface ICommandHandlerFactory
{
    AbstractCommandHandler GetCommandHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken,
        AppMiddleware appMiddleware);
}