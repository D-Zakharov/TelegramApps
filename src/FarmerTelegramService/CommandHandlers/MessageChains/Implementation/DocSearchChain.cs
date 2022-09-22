using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Entities.Documents;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.ReplyMarkups;

namespace FarmerTelegramService.CommandHandlers.MessageChains.Implementation;

[ChainsCode(ChainsCodes.DocSearch)]
public class DocSearchChain : AbstractMessageChainManager
{
    private enum Steps { DocNum, MainDocNum, DocDateFrom, DocDateTo, DocType }

    private protected override bool IsStepCodeValid(int stepCode)
    {
        return Enum.IsDefined(typeof(Steps), stepCode);
    }

    private protected override async Task PerformaActionsAfterChainIsFinished(KernelDbContext dbCtx, AppMiddleware appMiddleware,  
        AbstractCommandHandler commandHandler, MessageChainLink lastChainLink)
    {
        var docService = appMiddleware.DocumentsServiceFactory.GetService();
        var searchParams = GetSearchParamsForApi(dbCtx, lastChainLink.ChatId);

        int resultsCount = await docService.GetSearchResultsCount(searchParams);
        if (resultsCount > appMiddleware.BotConfig.SearchResultsLimit)
        {
            await commandHandler.ShowMessageToUser(Messages.ResultsCountIsTooBig);
        }
        else if (resultsCount == 0)
        {
            await commandHandler.ShowMessageToUser(Messages.DocsNotFound);
        }
        else
        {
            await ShowSearchResults(commandHandler, docService, searchParams, appMiddleware.Logger);
        }
    }

    private protected override async Task ShowMessageForNextStep(AppMiddleware appMiddleware, AbstractCommandHandler commandHandler,
        int nextStepCode)
    {
        if (IsStepCodeValid(nextStepCode))
        {
            switch ((Steps)nextStepCode)
            {
                case Steps.DocDateFrom:
                    await commandHandler.ShowMessageToUser(string.Format(Messages.EnterDateFrom, Consts.SearchValueAny));
                    break;

                case Steps.DocDateTo:
                    await commandHandler.ShowMessageToUser(string.Format(Messages.EnterDateTo, Consts.SearchValueAny));
                    break;

                case Steps.MainDocNum:
                    await commandHandler.ShowMessageToUser(string.Format(Messages.EnterMainDocNum, Consts.SearchValueAny));
                    break;

                case Steps.DocNum:
                    await commandHandler.ShowMessageToUser(string.Format(Messages.EnterDocNum, Consts.SearchValueAny));
                    break;

                case Steps.DocType:
                    await ShowDocTypes(appMiddleware, commandHandler);
                    break;
            }
        }
    }

    private protected override ChainsCodes GetChainCode()
    {
        return ChainsCodes.DocSearch;
    }

    private async static Task ShowDocuments(AbstractCommandHandler commandHandler, List<DocSearchResult> documents)
    {
        var fileButtons = new List<InlineKeyboardButton[]>();
        foreach (var doc in documents)
        {
            if (doc.DocId is not null && doc.FileName is not null)
            {
                string command = new DocDownloadCommand(doc.DocId.Value, doc.FileName).GetTextCommand();
                fileButtons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData($"{doc.DocType} №{doc.DocNum}", command) });
            }
        }

        var buttons = new InlineKeyboardMarkup(fileButtons);
        await commandHandler.ShowMessageToUser(Messages.DocumentsList, buttons);
    }

    private DocSearchParameters GetSearchParamsForApi(KernelDbContext dbCtx, long chatId)
    {
        var res = new DocSearchParameters();

        var savedParameters = dbCtx.MessageChains.AsNoTracking().Where(i => i.ChainCode == (int)GetChainCode() && i.ChatId == chatId).ToArray();
        foreach (var param in savedParameters)
        {
            if (param.UserInput is not null && param.UserInput != Consts.SearchValueAny)
            {
                Steps step = (Steps)param.ChainLinkCode;
                switch (step)
                {
                    case Steps.DocDateFrom: res.DateFrom = DateTime.ParseExact(param.UserInput, DateFormat, null); break;
                    case Steps.DocDateTo: res.DateTo = DateTime.ParseExact(param.UserInput, DateFormat, null); break;
                    case Steps.DocNum: res.DocNum = param.UserInput; break;
                    case Steps.DocType: res.DocType = param.UserInput; break;
                    case Steps.MainDocNum: res.MainDocNum = param.UserInput; break;
                }
            }
        }

        return res;
    }

    private static async Task ShowDocTypes(AppMiddleware appMiddleware, AbstractCommandHandler commandHandler)
    {
        try
        {
            var availableDocTypesButons = new List<InlineKeyboardButton[]>() {
                new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(Messages.AnyDocType, Consts.SearchValueAny) }
            };

            var docService = appMiddleware.DocumentsServiceFactory.GetService();
            foreach (string docType in await docService.GetAvailableForSearchDocTypes())
            {
                availableDocTypesButons.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(docType, docType) });
            }

            var buttons = new InlineKeyboardMarkup(availableDocTypesButons);
            await commandHandler.ShowMessageToUser(Messages.ChooseDocType, buttons);
        }
        catch (Exception ex)
        {
            appMiddleware.Logger.LogCritical(ex.Message, ex);
            await commandHandler.ShowMessageToUser(Messages.DocApiIsUnavailable);
        }
    }

    private protected override InputCommandTypes GetStepInputType(int currentStepCode)
    {
        if (IsStepCodeValid(currentStepCode))
        {
            Steps step = (Steps)currentStepCode;
            switch (step)
            {
                case Steps.DocDateFrom:
                case Steps.DocDateTo: return InputCommandTypes.Date;

                case Steps.DocNum:
                case Steps.DocType:
                case Steps.MainDocNum: return InputCommandTypes.Text;
            }
        }

        return InputCommandTypes.Unknown;
    }

    private static async Task ShowSearchResults(AbstractCommandHandler commandHandler, IDocumentsService docService,
        DocSearchParameters searchParams, ILogger<Worker> logger)
    {
        List<DocSearchResult>? documents = null;
        try
        {
            documents = await docService.GetSearchResults(searchParams);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message, ex);
            await commandHandler.ShowMessageToUser(Messages.DocApiIsUnavailable);
        }

        if (documents is not null)
        {
            await ShowDocuments(commandHandler, documents);
        }
    }
}