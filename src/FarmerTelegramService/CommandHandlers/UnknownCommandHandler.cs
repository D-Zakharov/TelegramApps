using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FarmerTelegramService.CommandHandlers;

public class UnknownCommandHandler : AbstractCommandHandler
{
    public UnknownCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken, AppMiddleware appMiddleware)
        : base(msgSender, update, cancellationToken, appMiddleware)
    {
    }

    public async override Task Handle()
    {
        _appMiddleware.Logger.LogCritical("Command handler not implemented: {data}", JsonSerializer.Serialize(_update));
        await ShowMessageToUser(Messages.UnknownCommand);

        await ShowBasicMenu();
    }

    private protected override Chat GetChat()
    {
        return _update.Message?.Chat!;
    }

    private protected override void CheckSpecificInputData()
    {
    }

    private protected override void ReadSpecificInputData()
    {
    }

    private protected override long? GetUserId()
    {
        return _update.Message?.Contact?.UserId;
    }
}
