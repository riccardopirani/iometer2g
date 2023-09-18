using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class ConfigCFG
    {
        public string cloudenergyConnection { get; set; }
        public bool DEBUG_MODE { get; set; }
        public string ALERT_EMAIL_TO { get; set; }
        
        public string API_BASE_URL { get; set; }

        public string login = "login";
        public string getStationList = "getStationList";
        public string getStationRealKpi = "getStationRealKpi";
        public string getKpiStationHour = "getKpiStationHour";
        public string getKpiStationDay = "getKpiStationDay";
        public string getDevList = "getDevList";
        public string getDevRealKpi = "getDevRealKpi";
        public string getDevFiveMinutes = "getDevFiveMinutes";

        public bool ADD_TLS_SECURITY { get; set; }

        public string API_USERNAME { get; set; }
        public string API_PASSWORD { get; set; }

        //Numero di tentativi di rilancio api in caso di errore ritornato dal server
        public int RETRY_COUNT { get; set; }
        //attesa dopo un errore prima di ritentare
        public int RETRY_SLEEP_MILLIS { get; set; }

        public string MAIL_CFG_SMTP_ADDRESS { get; set; }
        public string MAIL_CFG_SMTP_MAILFROM { get; set; }
        public string MAIL_CFG_SMTP_PASSWORD { get; set; }
        public bool MAIL_CFG_REQUIRE_CREDENTIAL { get; set; }
        public bool MAIL_CFG_REQUIRE_SSL { get; set; }


        public string FORCE_CALC_IDPARK { get; set; }
        public string FORCE_CALC_IDDEVICE{ get; set; }
        public bool skip_update_LAST_DATE_CALLED { get; set; } // metto a true nel programma di lancio manuale in modo da non alterare questo campo

        public int RELAUNCH_SERVICE_POLLING_SECS { get; set; }


    }
}
