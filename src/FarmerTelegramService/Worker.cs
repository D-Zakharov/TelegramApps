using FarmerTelegramService.Entities;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;

namespace FarmerTelegramService;

public class Worker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly AppMiddleware _appMiddleware;

    public Worker(ILogger<Worker> logger, ITelegramBotClient bot, IDbContextFactory<KernelDbContext> dbContextFactory,
        IOptions<BotConfig> options, IDocumentsServiceFactory documentsServiceFactory)
    {
        _bot = bot;
        _appMiddleware = new AppMiddleware(options.Value, dbContextFactory, logger, documentsServiceFactory);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var handler = new BotUpdateHandler(_appMiddleware);
            var receiverOptions = new ReceiverOptions();
            _bot.StartReceiving(handler, receiverOptions, cancellationToken: stoppingToken);

            _appMiddleware.Logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _appMiddleware.Logger.LogError(ex, message: ex.Message);
            throw;
        }
    }
}