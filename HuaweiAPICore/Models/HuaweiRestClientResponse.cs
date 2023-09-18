using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class HuaweiRestClientResponse
    {
        public bool success { get; set; }
        public int failCode { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> _params {get;set;}
        // "data": null,

        public dynamic data { get; set; }

        public string Error { get; set; }
        public string FullResponse { get; set; }



        //public string FullResponse { get; set; }
    }
}
