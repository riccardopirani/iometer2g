using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class GetStationListResponse
    {

        public string stationCode { get; set; } // ID dell'impianto per Huawei
        public int aidType { get; set; }
        public double capacity { get; set; }
        public string linkmanPho { get; set; }
        public string stationAddr { get; set; }
        public string stationLinkman { get; set; }
        public string stationName { get; set; }

        //"buildState": null,
        //"combineType": null,

        public List<GetDevListResponse> util_devices { get; set; }

    }
}
