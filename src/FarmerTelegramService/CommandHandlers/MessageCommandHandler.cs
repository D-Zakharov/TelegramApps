using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace FarmerTelegramService.CommandHandlers;

/// <summary>
/// обработка текстовых команд (с точки зрения API к ним относятся и кнопки меню)
/// </summary>
public class MessageCommandHandler : AbstractCommandHandler
{
    public MessageCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken, AppMiddleware appMiddleware)
        : base(msgSender, update, cancellationToken, appMiddleware)
    {
    }

    public override async Task Handle()
    {
        await ExecuteCommandHandleMethod();
    }

    [HandleMethod("/start")]
    internal async Task HandleStartCommand()
    {
        await SendAuthorizeMenu();
    }

    [HandleMethod(Consts.Command_ShowMenu)]
    internal async Task HandleShowMenu()
    {
        await ShowBasicMenu();
    }

    private protected override void CheckSpecificInputData()
    {
        if (_update.Message?.Text is null)
            throw new ArgumentException(GetNullArgumentExceptionMessage("_update.Message.Text"));
    }

    private protected override Chat GetChat()
    {
        return _update.Message?.Chat!;
    }

    private protected override long? GetUserId()
    {
        return _update.Message?.From?.Id;
    }

    private protected override void ReadSpecificInputData()
    {
        Command = _update.Message?.Text;
    }
}
