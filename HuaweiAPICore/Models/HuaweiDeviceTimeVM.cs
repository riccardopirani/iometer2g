using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class HuaweiDeviceTimeVM
    {
        public string MAPID{ get; set; }//varchar(36) NOT NULL,
        public string STATIONCODE { get; set; }//varchar(45) NOT NULL,
        public string DEVID { get; set; }//varchar(45) NOT NULL,
        public int DEVTYPEID { get; set; }//int NOT NULL,
        public string IDPARK { get; set; }//varchar(10) NOT NULL,
        public string IDDEVICE { get; set; }//varchar(20) NOT NULL,

        public List<GetDevFiveMinutesRequest> items { get; set; }

    }
}
