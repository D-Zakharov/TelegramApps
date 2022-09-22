namespace FarmerTelegramService.Entities.Documents;

public class DocSearchResult
{
    public int? DocId { get; set; }
    public string? DocType { get; set; }
    public string? DocNum { get; set; }
    public DateTime? DocDate { get; set; }
    public string? FileName { get; set; }
}