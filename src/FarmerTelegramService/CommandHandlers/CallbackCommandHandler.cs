using FarmerTelegramService.CommandHandlers.MessageChains;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Entities.Documents;
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
using Telegram.Bot.Types.InputFiles;

namespace FarmerTelegramService.CommandHandlers;

/// <summary>
/// Обработка кнопок (не меню)
/// </summary>
public class CallbackCommandHandler : AbstractCommandHandler
{
    public CallbackCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken, AppMiddleware appMiddleware)
        : base(msgSender, update, cancellationToken, appMiddleware)
    {
    }

    public override async Task Handle()
    {
        var downloadCommand = new DocDownloadCommand(Command);

        if (downloadCommand.IsValid)
        {
            var service = _appMiddleware.DocumentsServiceFactory.GetService();
            string filePath = await service.DownloadDocument(downloadCommand.DocId);
            try
            {
                using var contentStream = new FileStream(filePath, FileMode.Open);
                await _userInteraction.SendDocumentAsync(_tgChat, new InputOnlineFile(contentStream, downloadCommand.FileName));
            }
            finally
            {
                System.IO.File.Delete(filePath);
            }
        }
        else
        {
            await ExecuteCommandHandleMethod();
        }
    }

    [HandleMethod(Consts.Callback_ActionSearch)]
    internal async Task HandleStartSearch()
    {
        using (var dbCtx = _appMiddleware.DbContextFactory.CreateDbContext())
        {
            MessageChainsHelper.ClearMessagesChain(dbCtx, _tgChat.Id);
            var chainManager = MessageChainsHelper.GetChainManager(ChainsCodes.DocSearch);
            await chainManager.StartChain(dbCtx, _appMiddleware, this, _tgChat.Id);
        }
    }

    [HandleMethod(Consts.Callback_ActionShowContacts)]
    internal async Task HandleShowContacts()
    {
        string message = $"☎️ {_appMiddleware.BotConfig.FeedbackPhone}\r\n \r\n📧 {_appMiddleware.BotConfig.FeedbackMail}";
        await ShowMessageToUser(message);
        await SendBasicMenuForAuthorized();
    }

    private protected override Chat GetChat()
    {
        return _update.CallbackQuery!.Message!.Chat;
    }

    private protected override long? GetUserId()
    {
        return _update.CallbackQuery?.From.Id;
    }

    private protected override void ReadSpecificInputData()
    {
        Command = _update.CallbackQuery?.Data;
    }

    private protected override void CheckSpecificInputData()
    {
        if (_update.CallbackQuery is null)
            throw new ArgumentException(GetNullArgumentExceptionMessage("CallbackQuery"));

        if (_update.CallbackQuery?.Data is null)
            throw new ArgumentException(GetNullArgumentExceptionMessage("CallbackQuery.Data"));
    }
}
