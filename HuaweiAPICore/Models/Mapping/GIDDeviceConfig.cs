using EIEntityCore.Models.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public class GIDDeviceConfig : SingleDeviceConfigBase
    {
        //rappresenta la configurazione di un device tipo
        //public string IDDEVICE;
        //public string IDPARK;
        //public string IDCUSTOMER;
        public string ENERGY_YELDED;
        public string MODULE_TEMPERATURE;

        public string RUN_TIME;
        public string VAC;
        public string IAC;
        public string PAC;
        public string VDC;
        public string IDC;
        public string PDC;
        public string PAC_SIGN;
        public string RUN_TIME_SIGN;


        //public List<MonitoringDataManagerOutputDeviceParameter> parameters { get; set; }

        public GIDDeviceConfig()
        {
            parameters = new List<HuaweiAPIDimension>();
        }

        ~GIDDeviceConfig()
        {
            Dispose();
        }

        //void _Dispose()
        //{
        //    if (parameters != null)
        //        parameters.Clear();
        //}

        //public void Dispose()
        //{
        //    _Dispose();
        //}




        public bool MappaParametriSuVariabili(/*HuaweiAPIDeviceMapping map,*/ List<HuaweiAPIDimension> parameters)
        {
            bool fl = true;

            try
            {
                if ((parameters != null) && (parameters.Count > 0))
                {
                    ENERGY_YELDED = EstraiVariabileDaLista(parameters, "ENERGY YELDED", null);
                    STATUS = EstraiVariabileDaLista(parameters, "STATUS", null);
                    ALARM = EstraiVariabileDaLista(parameters, "ALARM", null);
                    MODULE_TEMPERATURE = EstraiVariabileDaLista(parameters, "MODULE TEMPERATURE", null);
                    DEVICE_TEMPERATURE = EstraiVariabileDaLista(parameters, "DEVICE TEMPERATURE", null);
                    RUN_TIME = EstraiVariabileDaLista(parameters, "RUN TIME", "");
                    //RUN_TIME_SIGN = EstraiVariabileDaLista(parameters, "RUN TIME_SIGN", "");
                    VAC = EstraiVariabileDaLista(parameters, "VAC", null);
                    IAC = EstraiVariabileDaLista(parameters, "IAC", null);
                    PAC = EstraiVariabileDaLista(parameters, "PAC", null);
                    VDC = EstraiVariabileDaLista(parameters, "VDC", null);
                    IDC = EstraiVariabileDaLista(parameters, "IDC", null);
                    PDC = EstraiVariabileDaLista(parameters, "PDC", null);
                    IDDEVICE = EstraiVariabileDaLista(parameters, "IDDEVICE", null);
                    DATETIME = EstraiVariabileDaLista(parameters, "DATETIME", null);

                    RUN_TIME_SIGN = EstraiVariabileDaLista(parameters, "RUN TIME_SIGN", "");
                    PAC_SIGN = EstraiVariabileDaLista(parameters, "PAC_SIGN", "");

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
