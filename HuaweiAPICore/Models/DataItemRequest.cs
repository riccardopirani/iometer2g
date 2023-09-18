using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class DataItemRequest
    {
        public string devIds { get; set; } // elenco id devices separati da virgola
        public int devTypeId { get; set; }
        public Int64? collectTime { get; set; }
    }
}
