using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class StationCodesRequest
    {
        public string stationCodes { get; set; }  // elenco separato da virgola
        public int? collectTime { get; set; }
    }
    
}
