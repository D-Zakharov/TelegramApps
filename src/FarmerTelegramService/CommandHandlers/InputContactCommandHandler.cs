using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FarmerTelegramService.CommandHandlers;

/// <summary>
/// Обработка команды "отправить контакт"
/// </summary>
public class InputContactCommandHandler : AbstractCommandHandler
{
    private Contact? TgContact;

    public InputContactCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken, AppMiddleware appMiddleware) 
        : base(msgSender, update, cancellationToken, appMiddleware)
    {
    }

    public async override Task Handle()
    {
        using (var dbCtx = _appMiddleware.DbContextFactory.CreateDbContext())
        {
            var users = await dbCtx.FarmerUsers.Where(i => i.Status == UserStatus.Approved).ToListAsync();
            FarmerUser? user = users.FirstOrDefault(i => TgContact!.PhoneNumber.Equals(i.FixedPhoneNumber));
            if (user is null)
            {
                await HandleUserNotFound();
            }
            else
            {
                await HandleUserWasFound(dbCtx, user);
            }
        }
    }

    private async Task HandleUserNotFound()
    {
        await ShowMessageToUser(Messages.UnknownPhone);
        _appMiddleware.Logger.LogError("Phone number not found: {phone}", TgContact!.PhoneNumber);
    }

    private async Task HandleUserWasFound(KernelDbContext dbCtx, FarmerUser user)
    {
        user.TelegramId = _userId;
        dbCtx.SaveChanges();
        
        //await ShowMessageToUser(Messages.SuccessfullAuthorization, new ReplyKeyboardRemove());
        var keyboardMarkup = new ReplyKeyboardMarkup(new[] {
                new KeyboardButton(Consts.Command_ShowMenu)
            })
        {
            ResizeKeyboard = true
        };
        await ShowMessageToUser(Messages.SuccessfullAuthorization, keyboardMarkup);

        await SendBasicMenuForAuthorized();
    }

    private protected override void ReadSpecificInputData()
    {
        TgContact = _update.Message?.Contact;
    }

    private protected override void CheckSpecificInputData()
    {
        if (_update.Message?.Contact is null)
            throw new ArgumentException(GetNullArgumentExceptionMessage("_update.Message.Contact"));
    }

    private protected override Chat GetChat()
    {
        return _update.Message?.Chat!;
    }

    private protected override long? GetUserId()
    {
        return _update.Message?.Contact?.UserId;
    }
}
