using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FarmerTelegramService.CommandHandlers;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FarmerTelegramService.Factories;

/// <summary>
/// Реализация в виде синглтона
/// </summary>
public sealed class CommandHandlerFactory : ICommandHandlerFactory
{
    private static readonly CommandHandlerFactory instance = new ();

    //чтобы компилятор не пометил тип как beforefieldinit
    static CommandHandlerFactory()
    { }

    private CommandHandlerFactory() 
    {
    }

    public static CommandHandlerFactory Instance { get => instance; }

    public AbstractCommandHandler GetCommandHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken,
        AppMiddleware appMiddleware)
    {
        IUserInteractionService msgSender = new UserInteractionService(botClient, cancellationToken);

        return update switch
        {
            {
                Type: UpdateType.Message,
                Message.Type: MessageType.Contact
            } => new InputContactCommandHandler(msgSender, update, cancellationToken, appMiddleware),
            
            {
                Type: UpdateType.Message,
                Message.Type: MessageType.Text
            } => new MessageCommandHandler(msgSender, update, cancellationToken, appMiddleware),

            {
                Type: UpdateType.Message,
                Message.Type: MessageType.Document
            } => new DocumentCommandHandler(msgSender, update, cancellationToken, appMiddleware),

            {
                Type: UpdateType.CallbackQuery,
            } => new CallbackCommandHandler(msgSender, update, cancellationToken, appMiddleware),

            _ => new UnknownCommandHandler(msgSender, update, cancellationToken, appMiddleware),
            //_ => throw new NotImplementedException($"Command handler not implemented: {JsonSerializer.Serialize(update)}")
        };
    }
}
