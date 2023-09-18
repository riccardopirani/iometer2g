using EIEntityCore.Models.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public class WSDDeviceConfiguration : SingleDeviceConfigBase
    {
        public string PLANE_DIRECT_RADIATION { get; set; }
        public string INCLINED_DIRECT_RADIATION { get; set; }
        public string TRACKER_DIRECT_RADIATION { get; set; }
        public string PLANE_DIFFUSE_RADIATION { get; set; }
        public string INCLINED_DIFFUSE_RADIATION { get; set; }
        public string TRACKER_DIFFUSE_RADIATION { get; set; }
        public string WIND_DIRECTION { get; set; }
        public string WIND_SPEED { get; set; }
        public string ENVIRONMENTAL_TEMPERATURE { get; set; }
        public string MODULE_TEMPERATURE { get; set; }

        public WSDDeviceConfiguration()
        {

        }

        ~WSDDeviceConfiguration()
        {
            Dispose();
        }

        public bool MappaParametriSuVariabili(/*HuaweiAPIDeviceMapping map,*/ List<HuaweiAPIDimension> parameters)
        {
            bool fl = true;

            try
            {
                if ((parameters != null) && (parameters.Count > 0))
                {
                    IDDEVICE = EstraiVariabileDaLista(parameters, "IDDEVICE", null);
                    DATETIME = EstraiVariabileDaLista(parameters, "DATETIME", null);
                    STATUS = EstraiVariabileDaLista(parameters, "STATUS", null);
                    ALARM = EstraiVariabileDaLista(parameters, "ALARM", null);
                    DEVICE_TEMPERATURE = EstraiVariabileDaLista(parameters, "DEVICE TEMPERATURE", null);

                    PLANE_DIRECT_RADIATION = EstraiVariabileDaLista(parameters, "PLANE DIRECT RADIATION", null);
                    INCLINED_DIRECT_RADIATION = EstraiVariabileDaLista(parameters, "INCLINED DIRECT RADIATION", null);
                    TRACKER_DIRECT_RADIATION = EstraiVariabileDaLista(parameters, "TRACKER DIRECT RADIATION", null);
                    PLANE_DIFFUSE_RADIATION = EstraiVariabileDaLista(parameters, "PLANE DIFFUSE RADIATION", null);
                    INCLINED_DIFFUSE_RADIATION = EstraiVariabileDaLista(parameters, "INCLINED DIFFUSE RADIATION", null);
                    TRACKER_DIFFUSE_RADIATION = EstraiVariabileDaLista(parameters, "TRACKER DIFFUSE RADIATION", null);
                    WIND_DIRECTION = EstraiVariabileDaLista(parameters, "WIND DIRECTION", null);
                    WIND_SPEED = EstraiVariabileDaLista(parameters, "WIND SPEED", null);
                    ENVIRONMENTAL_TEMPERATURE = EstraiVariabileDaLista(parameters, "ENVIRONMENTAL TEMPERATURE", null);
                    MODULE_TEMPERATURE = EstraiVariabileDaLista(parameters, "MODULE TEMPERATURE", null);

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
