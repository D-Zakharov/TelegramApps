using HashidsNet;

namespace FarmerTelegramService.Entities.Documents;


/// <summary>
/// парсинг и генерация команды для загрузки документа
/// ID в открытом виде не передается, формируется хеш
/// </summary>
public class DocDownloadCommand
{
    private const string Salt = "cjhjr nsczxj,tp]zy";
    private const string CommandPrefix = "/DownloadDoc";
    private const int MinHashLength = 8;

    public int DocId { get; }
    public string? FileName { get; }
    public bool IsValid { get; }

    public DocDownloadCommand(int docId, string fileName)
    {
        FileName = fileName;
        DocId = docId;
        IsValid = true;
    }

    public DocDownloadCommand(string? command)
    {
        if (command is not null)
        {
            var parts = command.Split(':');
            if (parts.Length == 3 && parts[0] == CommandPrefix)
            {
                var hash = new Hashids(Salt, MinHashLength);
                var res = hash.Decode(parts[1]);
                if (res.Length > 0)
                {
                    DocId = res[0];
                    FileName = parts[2];
                    IsValid = true;
                }
            }
        }
    }

    public string GetTextCommand()
    {
        var hash = new Hashids(Salt, MinHashLength);
        return $"{CommandPrefix}:{hash.Encode(DocId)}:{FileName}";
    }
}