using FarmerTelegramService.Entities.Documents;

namespace FarmerTelegramService.Services;

public interface IDocumentsService
{
    public Task<string> DownloadDocument(int docId);
    public Task<IEnumerable<string>> GetAvailableForSearchDocTypes();
    public Task<List<DocSearchResult>?> GetSearchResults(DocSearchParameters searchParams);
    public Task<int> GetSearchResultsCount(DocSearchParameters searchParams);
}