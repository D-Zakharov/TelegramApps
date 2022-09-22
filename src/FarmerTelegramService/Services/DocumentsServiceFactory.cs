using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmerTelegramService.Entities;
using Microsoft.Extensions.Options;

namespace FarmerTelegramService.Services;

public sealed class DocumentsServiceFactory : IDocumentsServiceFactory, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentService> _logger;

    public DocumentsServiceFactory(IOptions<BotConfig> options, ILogger<DocumentService> logger)
    {
        _httpClient = new HttpClient() { BaseAddress = new Uri(options.Value.FarmersApiServerUrl) };
        
        _logger = logger;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public IDocumentsService GetService()
    {
        return new DocumentService(_httpClient, _logger);
    }
}
