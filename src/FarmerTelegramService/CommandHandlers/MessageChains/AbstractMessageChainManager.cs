using FarmerTelegramService.CommandHandlers.MessageChains.Implementation;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using KernelDatabase.Models;

namespace FarmerTelegramService.CommandHandlers.MessageChains;

public abstract class AbstractMessageChainManager : IMessageChainManager
{
    private protected const string DateFormat = "dd.MM.yyyy";

    private protected enum InputCommandTypes { Unknown, Text, Date }

    private protected abstract ChainsCodes GetChainCode();

    private protected abstract Task ShowMessageForNextStep(AppMiddleware appMiddleware, AbstractCommandHandler commandHandler, int nextStepCode);

    private protected abstract Task PerformaActionsAfterChainIsFinished(KernelDbContext dbCtx, AppMiddleware appMiddleware, 
        AbstractCommandHandler commandHandler, MessageChainLink lastChainLink);

    private protected abstract InputCommandTypes GetStepInputType(int currentStepCode);

    private protected abstract bool IsStepCodeValid(int stepCode);

    public async Task ProcessInputAsync(KernelDbContext dbCtx, AppMiddleware appMiddleware, AbstractCommandHandler commandHandler, 
        MessageChainLink lastChainLink, string? command)
    {
        int currentStepCode = lastChainLink.ChainLinkCode + 1;
        if (IsStepCodeValid(currentStepCode))
        {
            bool isProcessed = await ProcessStep(commandHandler, dbCtx, lastChainLink.ChatId, currentStepCode, command);

            if (isProcessed)
            {
                int nextStepCode = currentStepCode + 1;
                if (IsStepCodeValid(nextStepCode))
                {
                    await ShowMessageForNextStep(appMiddleware, commandHandler, nextStepCode);
                }
                else
                {
                    await PerformaActionsAfterChainIsFinished(dbCtx, appMiddleware, commandHandler, lastChainLink);
                    MessageChainsHelper.ClearMessagesChain(dbCtx, lastChainLink.ChatId);
                }
            }
        }
        else
        {
            throw new ApplicationException($"Chain {nameof(DocSearchChain)} cannot process next step after {lastChainLink.ChainLinkCode}, command: {command}");
        }
    }

    public async Task StartChain(KernelDbContext dbCtx, AppMiddleware appMiddleware, AbstractCommandHandler commandHandler, long chatId)
    {
        dbCtx.MessageChains.Add(new MessageChainLink()
        {
            ChainCode = (int)GetChainCode(),
            ChatId = chatId,
            ChainLinkCode = -1,
            CreationDate = DateTime.Now
        });
        await dbCtx.SaveChangesAsync();

        await ShowMessageForNextStep(appMiddleware, commandHandler, 0);
    }


    private protected async ValueTask<bool> ProcessStep(AbstractCommandHandler commandHandler, KernelDbContext dbCtx,
        long chatId, int currentStepCode, string? command)
    {
        if (command is null)
        {
            await commandHandler.ShowMessageToUser(Messages.CommandIsEmpty);
            return false;
        }

        InputCommandTypes inputType = GetStepInputType(currentStepCode);
        if (inputType != InputCommandTypes.Unknown)
        {
            return await ProcessInputCommand(commandHandler, dbCtx, chatId, currentStepCode, command, inputType);
        }
        else
        {
            await commandHandler.ShowMessageToUser(Messages.UnknownStepCode);
            MessageChainsHelper.ClearMessagesChain(dbCtx, chatId);
            return false;
        }
    }

    private protected async ValueTask<bool> ProcessInputCommand(AbstractCommandHandler commandHandler, KernelDbContext dbCtx, 
        long chatId, int currentStepCode, string command, InputCommandTypes inputType)
    {
        if (inputType == InputCommandTypes.Date && command != Consts.SearchValueAny)
        {
            bool parsed = DateTime.TryParseExact(command, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime _);
            if (!parsed)
            {
                await commandHandler.ShowMessageToUser(Messages.WrongDateFormat);
                return false;
            }
        }

        dbCtx.MessageChains.Add(new MessageChainLink()
        {
            ChainCode = (int)GetChainCode(),
            ChainLinkCode = currentStepCode,
            ChatId = chatId,
            CreationDate = DateTime.Now,
            UserInput = command
        });
        dbCtx.SaveChanges();

        return true;
    }
}
