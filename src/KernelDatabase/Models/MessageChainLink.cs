using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KernelDatabase.Models
{
    [Table("MessageChains", Schema = "TG")]
    public class MessageChainLink
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public int ChainCode { get; set; }
        public int ChainLinkCode { get; set; }
        public string? UserInput { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
