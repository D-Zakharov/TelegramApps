using System;
using FarmerTelegramService;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.Configure<BotConfig>(configuration.GetSection(nameof(BotConfig)));

        services.AddSingleton<ITelegramBotClient>(x => new TelegramBotClient(configuration.GetValue<string>("BotToken")));
        services.AddSingleton<IDocumentsServiceFactory, DocumentsServiceFactory>();

        services.AddDbContextFactory<KernelDbContext>(options => 
        {
            options.UseSqlServer(
                configuration.GetValue<string>("ConnectionString"),
                serverDbContextOptionsBuilder =>
                {
                    serverDbContextOptionsBuilder.EnableRetryOnFailure();
                });
        });

        services.AddHostedService<Worker>();
    })
.Build();

await host.RunAsync();
