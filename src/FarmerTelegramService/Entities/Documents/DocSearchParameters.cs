using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmerTelegramService.Entities.Documents;

public class DocSearchParameters
{
    public string? MainDocNum { get; set; }
    public string? DocNum { get; set; }
    public string? DocType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
