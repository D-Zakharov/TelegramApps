using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmerTelegramService.Services;
using Microsoft.EntityFrameworkCore;

namespace FarmerTelegramService.Entities
{
    public class AppMiddleware
    {
        public ILogger<Worker> Logger { get; init; }
        public IDbContextFactory<KernelDbContext> DbContextFactory { get; init; }
        public BotConfig BotConfig { get; init; }
        public IDocumentsServiceFactory DocumentsServiceFactory { get; init; }

        public AppMiddleware(BotConfig botConfig, IDbContextFactory<KernelDbContext> dbContextFactory, ILogger<Worker> logger, 
            IDocumentsServiceFactory documentsServiceFactory)
        {
            BotConfig = botConfig;
            DbContextFactory = dbContextFactory;
            Logger = logger;
            DocumentsServiceFactory = documentsServiceFactory;
        }
    }
}
