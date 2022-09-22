using System.Collections;
using System.Net.Http.Json;
using System.Linq;
using FarmerTelegramService.Entities.Documents;

namespace FarmerTelegramService.Services;

public class DocumentService : IDocumentsService
{
    private const string ApiVersion = "V1";

    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentService>? _logger;

    public DocumentService(HttpClient httpClient, ILogger<DocumentService>? logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> DownloadDocument(int docId)
    {
        var url = $"api/{ApiVersion}/Docs/DownloadFile?Id={docId}";
        var resultPath = Path.GetTempFileName();
        using (var resultContentStream = await _httpClient.GetStreamAsync(url))
        using (var fs = new FileStream(resultPath, FileMode.Open))
        {
            await resultContentStream.CopyToAsync(fs);
        }
        return resultPath;
    }

    public async Task<IEnumerable<string>> GetAvailableForSearchDocTypes()
    {
        var url = $"api/{ApiVersion}/Docs/GetAvailableDocTypes";
        var res = await _httpClient.GetFromJsonAsync<List<string>?>(url);
        
        if (res is null)
            return Enumerable.Empty<string>();

        return res;
    }

    public async Task<List<DocSearchResult>?> GetSearchResults(DocSearchParameters searchParams)
    {
        var url = $"api/{ApiVersion}/Docs/GetSearchResultsCount?{GetFilterUrlParameters(searchParams)}";
        return await _httpClient.GetFromJsonAsync<List<DocSearchResult>>(url);
    }

    public async Task<int> GetSearchResultsCount(DocSearchParameters searchParams)
    {
        try
        {
            var url = $"api/{ApiVersion}/Docs/GetSearchResultsCount?{GetFilterUrlParameters(searchParams)}";
            var res = await _httpClient.GetStringAsync(url);

            _logger?.LogDebug("Url: {url}", url);
            _logger?.LogDebug("GetSearchResultsCount: {count}", res);
            
            return int.Parse(res);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, ex.Message);
            return 0;
        }
    }

    private static string GetFilterUrlParameters(DocSearchParameters searchParams)
    {
        return string.Format("DocNum={0}&MainDocNum={1}&DocType={2}&DateFrom={3}&DateTo={4}",
                searchParams.DocNum, searchParams.MainDocNum, searchParams.DocType,
                searchParams.DateFrom?.ToString("yyyy-MM-dd"), searchParams.DateTo?.ToString("yyyy-MM-dd"));
    }
}