using EIEntityCore.Models.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public sealed class BatteryDeviceConfig : SingleDeviceConfigBase
    {
        public string HEALTH { get; set; }
        public string CHARGE_PERC { get; set; }
        public string POWER { get; set; }
        public string CHARGE_CAP { get; set; }
        public string DISCHARGE_CAP { get; set; }
        public string VOLTAGE { get; set; }
        public string CURRENT { get; set; }
        public string MODULE_TEMPERATURE { get; set; }


        public BatteryDeviceConfig()
        {
            parameters = new List<HuaweiAPIDimension>();
        }

        ~BatteryDeviceConfig()
        {
            Dispose();
        }


        public bool MappaParametriSuVariabili(/*HuaweiAPIDeviceMapping map, */List<HuaweiAPIDimension> parameters)
        {
            bool fl = true;

            try
            {
                if ((parameters != null) && (parameters.Count > 0))
                {
                    HEALTH = EstraiVariabileDaLista(parameters, "HEALTH", null);
                    STATUS = EstraiVariabileDaLista(parameters, "STATUS", null);
                    ALARM = EstraiVariabileDaLista(parameters, "ALARM", null);
                    MODULE_TEMPERATURE = EstraiVariabileDaLista(parameters, "MODULE TEMPERATURE", null);
                    DEVICE_TEMPERATURE = EstraiVariabileDaLista(parameters, "DEVICE TEMPERATURE", null);
                    POWER = EstraiVariabileDaLista(parameters, "POWER", null);
                    CHARGE_PERC = EstraiVariabileDaLista(parameters, "CHARGE_PERC", null);
                    CHARGE_CAP = EstraiVariabileDaLista(parameters, "CHARGE_CAP", null);
                    DISCHARGE_CAP = EstraiVariabileDaLista(parameters, "DISCHARGE_CAP", null);
                    VOLTAGE = EstraiVariabileDaLista(parameters, "VOLTAGE", null);
                    CURRENT = EstraiVariabileDaLista(parameters, "CURRENT", null);
                    DISCHARGE_CAP = EstraiVariabileDaLista(parameters, "DISCHARGE_CAP", null);
                    IDDEVICE = EstraiVariabileDaLista(parameters, "IDDEVICE", null);
                    DATETIME = EstraiVariabileDaLista(parameters, "DATETIME", null);

                    //if (map != null)
                    //{
                    //    IDPARK = map.IDPARK;
                    //    IDCUSTOMER = map.IDCUSTOMER;
                    //}

                }
            }
            catch (Exception)
            {
                fl = false;
            }
            return fl;
        }
    }
}
