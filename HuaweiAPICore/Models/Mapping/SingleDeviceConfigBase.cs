using EIEntityCore.Models.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public class SingleDeviceConfigBase
    {
        public string IDDEVICE { get; set; }
        public string IDPARK { get; set; }
        public string IDCUSTOMER { get; set; }
        public string DATETIME { get; set; }
        public string LAST_UPDATE { get; set; }
        public string STATUS { get; set; }
        public string ALARM { get; set; }
        public string DEVICE_TEMPERATURE;

        public string DEVICE_TYPE { get; set; }
        public string OUTPUT_NAME { get; set; }

        public List<HuaweiAPIDimension> parameters { get; set; }

        ~SingleDeviceConfigBase()
        {
            _Dispose();
        }

        void _Dispose()
        {
            if (parameters != null)
                parameters.Clear();
        }

        public void Dispose()
        {
            _Dispose();
        }


        public string EstraiVariabileDaLista(List<HuaweiAPIDimension> listaPar, string DIMENSION, string defaultValue)
        {
            string spp = defaultValue;
            if ((listaPar != null) && (listaPar.Count > 0))
            {
                HuaweiAPIDimension item = listaPar.Where(m => m.DIMENSION == DIMENSION).FirstOrDefault();
                if (item != null)
                {
                    //spp = item.FORMULA;
                    spp = item.util_realValue;
                }
            }
            return spp;
        }

    }

    public class DeviceTypeEnum
    {
        //METER
        //CONTATORE
        //METER PROD
        public const string INVERTER = "INVERTER";
        public const string METEO = "METEO";
        public const string STRINGBOX = "STRINGBOX";
    }
}
