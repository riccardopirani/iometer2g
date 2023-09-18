using EIEntityCore.Models.Huawei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models.Mapping
{
    public class ISTRDeviceConfiguration : SingleDeviceConfigBase
    {
        public string VDC { get; set; }
        public string TOTALI { get; set; }
        public string ISTR1 { get; set; }
        public string ISTR2 { get; set; }
        public string ISTR3 { get; set; }
        public string ISTR4 { get; set; }
        public string ISTR5 { get; set; }
        public string ISTR6 { get; set; }
        public string ISTR7 { get; set; }
        public string ISTR8 { get; set; }
        public string ISTR9 { get; set; }
        public string ISTR10 { get; set; }
        public string ISTR11 { get; set; }
        public string ISTR12 { get; set; }
        public string ISTR13 { get; set; }
        public string ISTR14 { get; set; }
        public string ISTR15 { get; set; }
        public string ISTR16 { get; set; }
        public string ISTR17 { get; set; }
        public string ISTR18 { get; set; }
        public string ISTR19 { get; set; }
        public string ISTR20 { get; set; }
        public string ISTR21 { get; set; }
        public string ISTR22 { get; set; }
        public string ISTR23 { get; set; }
        public string ISTR24 { get; set; }
        public string ISTR25 { get; set; }

        public ISTRDeviceConfiguration()
        {

        }

        ~ISTRDeviceConfiguration()
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

                    TOTALI = EstraiVariabileDaLista(parameters, $"TOTALI", null);
                    VDC = EstraiVariabileDaLista(parameters, $"VDC", null);

                    ISTR1 = EstraiVariabileDaLista(parameters, $"ISTR1", null);
                    ISTR2 = EstraiVariabileDaLista(parameters, $"ISTR2", null);
                    ISTR3 = EstraiVariabileDaLista(parameters, $"ISTR3", null);
                    ISTR4 = EstraiVariabileDaLista(parameters, $"ISTR4", null);
                    ISTR5 = EstraiVariabileDaLista(parameters, $"ISTR5", null);
                    ISTR6 = EstraiVariabileDaLista(parameters, $"ISTR6", null);
                    ISTR7 = EstraiVariabileDaLista(parameters, $"ISTR7", null);
                    ISTR8 = EstraiVariabileDaLista(parameters, $"ISTR8", null);
                    ISTR9 = EstraiVariabileDaLista(parameters, $"ISTR9", null);
                    ISTR10 = EstraiVariabileDaLista(parameters, $"ISTR10", null);
                    ISTR11 = EstraiVariabileDaLista(parameters, $"ISTR11", null);
                    ISTR12 = EstraiVariabileDaLista(parameters, $"ISTR12", null);
                    ISTR13 = EstraiVariabileDaLista(parameters, $"ISTR13", null);
                    ISTR14 = EstraiVariabileDaLista(parameters, $"ISTR14", null);
                    ISTR15 = EstraiVariabileDaLista(parameters, $"ISTR15", null);
                    ISTR16 = EstraiVariabileDaLista(parameters, $"ISTR16", null);
                    ISTR17 = EstraiVariabileDaLista(parameters, $"ISTR17", null);
                    ISTR18 = EstraiVariabileDaLista(parameters, $"ISTR18", null);
                    ISTR19 = EstraiVariabileDaLista(parameters, $"ISTR19", null);
                    ISTR20 = EstraiVariabileDaLista(parameters, $"ISTR20", null);
                    ISTR21 = EstraiVariabileDaLista(parameters, $"ISTR21", null);
                    ISTR22 = EstraiVariabileDaLista(parameters, $"ISTR22", null);
                    ISTR23 = EstraiVariabileDaLista(parameters, $"ISTR23", null);
                    ISTR24 = EstraiVariabileDaLista(parameters, $"ISTR24", null);
                    ISTR25 = EstraiVariabileDaLista(parameters, $"ISTR25", null);


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
