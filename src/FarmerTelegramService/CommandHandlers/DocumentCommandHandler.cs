using FarmerTelegramService.Entities;
using FarmerTelegramService.Resources;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FarmerTelegramService.CommandHandlers;

/// <summary>
/// обработка прикрепленных файлов
/// </summary>
public class DocumentCommandHandler : AbstractCommandHandler
{
    private Document UserDocument = default!;

    public DocumentCommandHandler(IUserInteractionService msgSender, Update update, CancellationToken cancellationToken, AppMiddleware appMiddleware)
        : base(msgSender, update, cancellationToken, appMiddleware)
    {
    }

    public override async Task Handle()
    {
        //TODO: пока что получать файлы не требуется

        /*
        var fileLocalPath = await DownloadFile();
        if (fileLocalPath is not null)
        {
            try
            {
            }
            finally
            {
                System.IO.File.Delete(fileLocalPath);
            }
        }
        */
    }

    private protected override void CheckSpecificInputData()
    {
        if (_update.Message?.Document is null)
            throw new ApplicationException("Received a document, but Message?.Document is empty");
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
        UserDocument = _update.Message?.Document!;
    }

    private async Task<string?> DownloadFile()
    {
        var fileInfo = await _userInteraction.GetFileAsync(UserDocument.FileId);

        if (await InputFileIsCorrect(fileInfo))
        {
            return await DownloadFileToTempStorage(fileInfo);
        }

        return null;
    }

    private async Task<bool> InputFileIsCorrect(Telegram.Bot.Types.File fileInfo)
    {
        if (fileInfo.FilePath is null)
            throw new ApplicationException("fileInfo.FilePath is null");

        if (fileInfo.FileSize > _appMiddleware.BotConfig.MaxFileSize)
        {
            await ShowMessageToUser(Messages.FileIsTooBig);
            return false;
        }

        var extension = Path.GetExtension(fileInfo.FilePath);
        if (extension is null || !extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            await ShowMessageToUser(Messages.WrongExtension);
            return false;
        }

        return true;
    }

    private async Task<string> DownloadFileToTempStorage(Telegram.Bot.Types.File fileInfo)
    {
        string destinationFilePathAndName = Path.GetTempFileName();

        await using var fileStream = System.IO.File.OpenWrite(destinationFilePathAndName);
        await _userInteraction.DownloadFileAsync(fileInfo.FilePath!, fileStream);

        return destinationFilePathAndName;
    }
}
