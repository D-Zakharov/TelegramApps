using FarmerTelegramService.CommandHandlers.MessageChains;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FarmerTelegramService.CommandHandlers;

public abstract class AbstractCommandHandler
{
    private protected readonly IUserInteractionService _userInteraction;
    private protected readonly Update _update;
    private protected readonly CancellationToken _cancellationToken;
    private protected readonly AppMiddleware _appMiddleware;

    private protected readonly Chat _tgChat;
    private protected readonly long? _userId;

    private protected static readonly Dictionary<Type, Dictionary<string, MethodInfo>> MethodsForHandling = GetAllMethodsForHandling();

    private protected string? Command { get; set; }


    public AbstractCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken,
        AppMiddleware appMiddleware)
    {
        (_update, _userInteraction, _cancellationToken, _appMiddleware) = 
            (update, msgSender, cancellationToken, appMiddleware);

        CheckSpecificInputData();
        ReadSpecificInputData();

        _tgChat = GetChat();
        if (_tgChat is null)
            throw new ArgumentException(GetNullArgumentExceptionMessage("Chat"));

        _userId = GetUserId();
    }

    internal async Task ShowMessageToUser(string message, IReplyMarkup? keyboardMarkup = null, bool isHtml = false)
    {
        if (_tgChat is not null)
            await _userInteraction.ShowMessageToUser(_tgChat, message, keyboardMarkup, isHtml);
    }

    private protected async Task ExecuteCommandHandleMethod()
    {
        MethodInfo? method = null;
        var currentType = GetType();
        if (MethodsForHandling.ContainsKey(currentType))
        {
            if (Command is not null && MethodsForHandling[currentType].ContainsKey(Command))
            {
                method = MethodsForHandling[currentType][Command];
            }
        }

        if (method is not null)
        {
            Task? res = (Task?)method.Invoke(this, Array.Empty<object>());
            if (res is not null)
                await res;
            else
                throw new ApplicationException($"Failed to run method {method.Name}");
        }
        else
        {
            await HandleChainCommand();
        }
    }

    //команды цепочек, которые не являются корневыми
    //нет смысла под каждую из них писать отдельный обработчик
    private async Task HandleChainCommand()
    {
        using (var dbCtx = _appMiddleware.DbContextFactory.CreateDbContext())
        {
            (IMessageChainManager? chain, var lastChainLink) = MessageChainsHelper.GetCurrentChainManager(dbCtx, _tgChat.Id);
            if (chain is null || lastChainLink is null)
            {
                await ShowMessageToUser(Messages.ChainNotFound);
                await ShowBasicMenu();
            }
            else
            {
                await chain.ProcessInputAsync(dbCtx, _appMiddleware, this, lastChainLink, Command);
            }
        }
    }

    private static Dictionary<Type, Dictionary<string, MethodInfo>> GetAllMethodsForHandling()
    {
        Dictionary<Type, Dictionary<string, MethodInfo>> res = new();

        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(p => typeof(AbstractCommandHandler).IsAssignableFrom(p) && p.Name != "AbstractCommandHandler"))
        {
            Dictionary<string, MethodInfo> typeMethods = new();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                foreach (var attr in method.GetCustomAttributes(false))
                {
                    if (attr is HandleMethodAttribute info)
                    {
                        typeMethods.Add(info.Command, method);
                        break;
                    }
                }
            }
            res.Add(type, typeMethods);
        }

        return res;
    }

    private protected async Task ShowBasicMenu()
    {
        if (await IsAuthorized())
            await SendBasicMenuForAuthorized();
        else
            await SendAuthorizeMenu();
    }

    private protected async Task SendBasicMenuForAuthorized()
    {
        var keyboardMarkup = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: Buttons.DocSearch, callbackData: Consts.Callback_ActionSearch),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: Buttons.Contacts, callbackData: Consts.Callback_ActionShowContacts),
                    InlineKeyboardButton.WithUrl(text: Buttons.PortalLink, url: _appMiddleware.BotConfig.PortalUrl),
                }
            }
        );

        await ShowMessageToUser(Messages.MenuCaption, keyboardMarkup);
    }

    private protected async Task SendAuthorizeMenu()
    {
        var keyboardMarkup = new ReplyKeyboardMarkup(new[] {
                KeyboardButton.WithRequestContact(Buttons.SendContact)
            })
        {
            ResizeKeyboard = true
        };

        await ShowMessageToUser(Messages.WelcomeMessage, keyboardMarkup);
    }

    private protected async Task<bool> IsAuthorized()
    {
        using (var dbCtx = _appMiddleware.DbContextFactory.CreateDbContext())
        {
            return await dbCtx.FarmerUsers.AnyAsync(i => i.TelegramId == _userId);
        }
    }

    private protected string GetNullArgumentExceptionMessage(string varName)
    {
        return $"{varName} is null, message: {JsonSerializer.Serialize(_update)}";
    }

    private protected abstract void ReadSpecificInputData();
    
    private protected abstract void CheckSpecificInputData();
    
    private protected abstract Chat GetChat();

    private protected abstract long? GetUserId();

    public abstract Task Handle();
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class HandleMethodAttribute : Attribute
{
    internal string Command { get; init; }

    public HandleMethodAttribute(string command)
    {
        Command = command;
    }
}