using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Extensions.Polling;
using FarmerTelegramService.Factories;
using Microsoft.EntityFrameworkCore;
using FarmerTelegramService.Entities;

namespace FarmerTelegramService;

internal class BotUpdateHandler : IUpdateHandler
{
    private readonly AppMiddleware _appMiddleware;

    public BotUpdateHandler(AppMiddleware appMiddleware)
    {
        _appMiddleware = appMiddleware;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var commandHandlerFactory = CommandHandlerFactory.Instance;
            var handler = commandHandlerFactory.GetCommandHandler(botClient, update, cancellationToken, _appMiddleware);
            await handler.Handle();
        }
        catch (Exception ex)
        {
            _appMiddleware.Logger.LogError(ex, ex.Message);
        }
    }
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.Error.WriteLine(exception);
        return Task.CompletedTask;
    }
}