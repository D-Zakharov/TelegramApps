using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmerTelegramService.Entities
{
    public class BotConfig
    {
        public int MaxFileSize { get; set; }
        public string PortalUrl { get; set; } = default!;
        public string FeedbackMail { get; set; } = default!;
        public string FeedbackPhone { get; set; } = default!;
        public int SearchResultsLimit { get; set; }
        public string FarmersApiServerUrl { get; set; } = default!;
    }
}
