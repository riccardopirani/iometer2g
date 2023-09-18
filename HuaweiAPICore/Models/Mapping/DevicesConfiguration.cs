using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public class DevicesConfiguration
    {

        public string IDPARK { get; set; }
        public string IDCUSTOMER { get; set; }
        public string IDGROUP { get; set; }
        public List<SingleDeviceConfigBase> devices { get; set; }
        public string filePath { get; set; }



        public DevicesConfiguration()
        {
            devices = new List<SingleDeviceConfigBase>();
        }

        public void Dispose()
        {
            _Dispose();
        }
        void _Dispose()
        {
            if (devices != null)
                devices.Clear();
        }
        ~DevicesConfiguration()
        {
            _Dispose();
        }

    }
}
