using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class GetDevListResponse
    {
        public string devName { get; set; }
        public int devTypeId { get; set; }
        public string esnCode { get; set; }
        public Int64 id { get; set; }
        public string invType { get; set; }
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
        public string softwareVersion { get; set; }
        public string stationCode { get; set; }
        //public string optimizerNumber
    }
}
