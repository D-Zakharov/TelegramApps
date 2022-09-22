using FarmerTelegramService;
using FarmerTelegramService.CommandHandlers;
using FarmerTelegramService.CommandHandlers.MessageChains;
using FarmerTelegramService.CommandHandlers.MessageChains.Implementation;
using FarmerTelegramService.Entities;
using FarmerTelegramService.Entities.Documents;
using FarmerTelegramService.Services;
using KernelDatabase;
using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ServiceUnitTests
{
    public class UnitTests
    {
        private const int TelegramChatId = 0;
        private const int TelegramUserId = 1;
        private const string SearchDocType = "¿ÍÚ";

        [Fact]
        public async Task Should_Perform_Doc_Search()
        {
            var tgBotMessages = new List<string>();
            var mockInteraction = CreateMockInteractionService(tgBotMessages);

            var mockDbCtx = CreateMockDbContext();
            var mockAppMiddleware = CreateMiddlewareMock(mockDbCtx);

            var mockCommandUpdate = CreateMockCommandUpdate(Consts.Callback_ActionSearch);
            AbstractCommandHandler commandHandler = new CallbackCommandHandler(mockInteraction, mockCommandUpdate,
                new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            Update mockInputUpdate = CreateMockInputUpdate("test_doc_num");
            commandHandler = new MessageCommandHandler(mockInteraction, mockInputUpdate, new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            mockInputUpdate = CreateMockInputUpdate("test_main_doc_num");
            commandHandler = new MessageCommandHandler(mockInteraction, mockInputUpdate, new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            mockInputUpdate = CreateMockInputUpdate("02.02.2002");
            commandHandler = new MessageCommandHandler(mockInteraction, mockInputUpdate, new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            mockInputUpdate = CreateMockInputUpdate("03.03.2003");
            commandHandler = new MessageCommandHandler(mockInteraction, mockInputUpdate, new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            mockCommandUpdate = CreateMockCommandUpdate(SearchDocType);
            commandHandler = new CallbackCommandHandler(mockInteraction, mockCommandUpdate, new CancellationToken(), mockAppMiddleware);
            await commandHandler.Handle();

            Assert.Equal(6, tgBotMessages.Count);
        }

        private AppMiddleware CreateMiddlewareMock(KernelDbContext mockDbCtx)
        {
            return new AppMiddleware(
                new BotConfig() { PortalUrl = "http://", SearchResultsLimit = 10 },
                CreateMockDbFactory(mockDbCtx),
                CreateMockLogger(),
                CreateDocServiceMockFactory()
            );
        }

        private IDocumentsServiceFactory CreateDocServiceMockFactory()
        {
            var res = new Mock<IDocumentsServiceFactory>();
            var mockDocService = new Mock<IDocumentsService>();

            mockDocService.Setup(c => c.DownloadDocument(It.IsAny<int>())).Returns<int>((docId) =>
            {
                var resultPath = Path.GetTempFileName();
                System.IO.File.WriteAllText(resultPath, "mock content");
                return Task.FromResult(resultPath);
            });

            mockDocService.Setup(c => c.GetAvailableForSearchDocTypes())
                .Returns(Task.FromResult((IEnumerable<string>)new List<string>() { SearchDocType }));

            mockDocService.Setup(c => c.GetSearchResults(It.IsAny<DocSearchParameters>()))
                .Returns<DocSearchParameters>((@params) =>
                {
                    return Task.FromResult<List<DocSearchResult>?>(new List<DocSearchResult>() { new DocSearchResult() });
                });

            mockDocService.Setup(c => c.GetSearchResultsCount(It.IsAny<DocSearchParameters>()))
                .Returns(Task.FromResult(1));

            res.Setup(c => c.GetService()).Returns(mockDocService.Object);

            return res.Object;
        }

        private ILogger<Worker> CreateMockLogger()
        {
            var res = new Mock<ILogger<Worker>>();
            return res.Object;
        }

        private IDbContextFactory<KernelDbContext> CreateMockDbFactory(KernelDbContext mockDbCtx)
        {
            var res = new Mock<IDbContextFactory<KernelDbContext>>();

            res.Setup(c => c.CreateDbContext()).Returns(mockDbCtx);

            return res.Object;
        }

        private IUserInteractionService CreateMockInteractionService(List<string> tgBotMessages)
        {
            var res = new Mock<IUserInteractionService>();

            res.Setup(c => c.ShowMessageToUser(It.IsAny<Chat>(), It.IsAny<string>(), It.IsAny<IReplyMarkup?>(), It.IsAny<bool>()))
                .Returns<Chat, string, IReplyMarkup?, bool>(
                (chat, text, markup, parseMode) =>
                {
                    tgBotMessages.Add(text);
                    return Task.FromResult(new Message());
                });

            return res.Object;
        }

        private Update CreateMockCommandUpdate(string action)
        {
            var res = new Mock<Update>().Object;
            res.CallbackQuery = new CallbackQuery()
            {
                Message = new Message()
                {
                    Chat = new Chat() { Id = TelegramChatId }
                },
                Data = action,
                From = new User() { Id = TelegramUserId }
            };

            return res;
        }

        private Update CreateMockInputUpdate(string message)
        {
            var res = new Mock<Update>().Object;

            res.Message = new Message() { Chat = new Chat() { Id = TelegramChatId }, Text = message };

            return res;
        }

        private KernelDbContext CreateMockDbContext()
        {
            var res = new Mock<KernelDbContext>(new DbContextOptions<KernelDbContext>());

            res.Setup(x => x.FarmerUsers).Returns(CreateQueryableMockDbSet(new List<FarmerUser>()
            {
                  new FarmerUser() { Id = 1, DisplayName = "Test", Mail = "test@test.com", TelegramId = TelegramUserId }
            }));

            res.Setup(x => x.MessageChains).Returns(CreateQueryableMockDbSet(new List<MessageChainLink>()));

            return res.Object;
        }

        private static DbSet<T> CreateQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(i => i.Add(It.IsAny<T>())).Callback<T>((j) => sourceList.Add(j));

            return dbSet.Object;
        }
    }
}