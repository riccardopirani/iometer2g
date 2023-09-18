using EILib.Dao.Managers;
using HuaweiAPICore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Managers
{
    public sealed class ConfigCFGManager : ManagerBase
    {

        public bool LoadConfiguration(ref ConfigCFG config)
        {
            bool fl = true;

            try
            {
                config = new ConfigCFG();
                if (ConfigurationManager.ConnectionStrings["cloudenergyConnection"] != null)
                    config.cloudenergyConnection = ConfigurationManager.ConnectionStrings["cloudenergyConnection"].ToString();

                if (ConfigurationManager.AppSettings["DEBUG_MODE"] != null)
                {
                    bool debugMode = false;
                    if (bool.TryParse(ConfigurationManager.AppSettings["DEBUG_MODE"], out debugMode))
                        config.DEBUG_MODE = debugMode;
                }

                if (ConfigurationManager.AppSettings["skip_update_LAST_DATE_CALLED"] != null)
                {
                    bool skip_update_LAST_DATE_CALLED = false;
                    if (bool.TryParse(ConfigurationManager.AppSettings["skip_update_LAST_DATE_CALLED"], out skip_update_LAST_DATE_CALLED))
                        config.skip_update_LAST_DATE_CALLED = skip_update_LAST_DATE_CALLED;
                }

                

                if (ConfigurationManager.AppSettings["API_BASE_URL"] != null)
                    config.API_BASE_URL = ConfigurationManager.AppSettings["API_BASE_URL"].ToString();
                if (ConfigurationManager.AppSettings["login"] != null)
                    config.login = ConfigurationManager.AppSettings["login"].ToString();
                if (ConfigurationManager.AppSettings["getStations"] != null)
                    config.getStationList = ConfigurationManager.AppSettings["getStationList"].ToString();
                if (ConfigurationManager.AppSettings["getStationRealKpi"] != null)
                    config.getStationRealKpi = ConfigurationManager.AppSettings["getStationRealKpi"].ToString();
                if (ConfigurationManager.AppSettings["getKpiStationHour"] != null)
                    config.getKpiStationHour = ConfigurationManager.AppSettings["getKpiStationHour"].ToString();
                if (ConfigurationManager.AppSettings["getKpiStationDay"] != null)
                    config.getKpiStationDay = ConfigurationManager.AppSettings["getKpiStationDay"].ToString();
                if (ConfigurationManager.AppSettings["getDevList"] != null)
                    config.getDevList = ConfigurationManager.AppSettings["getDevList"].ToString();
                if (ConfigurationManager.AppSettings["getDevRealKpi"] != null)
                    config.getDevRealKpi = ConfigurationManager.AppSettings["getDevRealKpi"].ToString();
                if (ConfigurationManager.AppSettings["getDevFiveMinutes"] != null)
                    config.getDevFiveMinutes = ConfigurationManager.AppSettings["getDevFiveMinutes"].ToString();

                if (ConfigurationManager.AppSettings["API_USERNAME"] != null)
                    config.API_USERNAME= ConfigurationManager.AppSettings["API_USERNAME"].ToString();
                if (ConfigurationManager.AppSettings["API_PASSWORD"] != null)
                    config.API_PASSWORD = ConfigurationManager.AppSettings["API_PASSWORD"].ToString();

                /*   PARAMETRI EMAIL ALERT  */
                if (ConfigurationManager.AppSettings["ALERT_EMAIL_TO"] != null)
                    config.ALERT_EMAIL_TO = ConfigurationManager.AppSettings["ALERT_EMAIL_TO"].ToString();

                if (ConfigurationManager.AppSettings["MAIL_CFG_SMTP_ADDRESS"] != null)
                    config.MAIL_CFG_SMTP_ADDRESS = ConfigurationManager.AppSettings["MAIL_CFG_SMTP_ADDRESS"].ToString();
                if (ConfigurationManager.AppSettings["MAIL_CFG_SMTP_MAILFROM"] != null)
                    config.MAIL_CFG_SMTP_MAILFROM = ConfigurationManager.AppSettings["MAIL_CFG_SMTP_MAILFROM"].ToString();

                if (ConfigurationManager.AppSettings["MAIL_CFG_SMTP_PASSWORD"] != null)
                    config.MAIL_CFG_SMTP_PASSWORD = ConfigurationManager.AppSettings["MAIL_CFG_SMTP_PASSWORD"].ToString();

                if (ConfigurationManager.AppSettings["MAIL_CFG_REQUIRE_CREDENTIAL"] != null)
                {
                    bool parser = false;
                    if (bool.TryParse(ConfigurationManager.AppSettings["MAIL_CFG_REQUIRE_CREDENTIAL"], out parser))
                        config.MAIL_CFG_REQUIRE_CREDENTIAL = parser;
                }

                if (ConfigurationManager.AppSettings["MAIL_CFG_REQUIRE_SSL"] != null)
                {
                    bool parser = false;
                    if (bool.TryParse(ConfigurationManager.AppSettings["MAIL_CFG_REQUIRE_SSL"], out parser))
                        config.MAIL_CFG_REQUIRE_SSL = parser;
                }

                if (ConfigurationManager.AppSettings["RETRY_COUNT"] != null)
                {
                    int parser = 3;
                    if (int.TryParse(ConfigurationManager.AppSettings["RETRY_COUNT"], out parser))
                        config.RETRY_COUNT = parser;
                }

                if (ConfigurationManager.AppSettings["RETRY_SLEEP_MILLIS"] != null)
                {
                    int parser = 5000;
                    if (int.TryParse(ConfigurationManager.AppSettings["RETRY_SLEEP_MILLIS"], out parser))
                        config.RETRY_SLEEP_MILLIS = parser;
                }

                if (ConfigurationManager.AppSettings["RELAUNCH_SERVICE_POLLING_SECS"] != null)
                {
                    int parser = 5000;
                    if (int.TryParse(ConfigurationManager.AppSettings["RELAUNCH_SERVICE_POLLING_SECS"], out parser))
                        config.RELAUNCH_SERVICE_POLLING_SECS = parser;
                }

                if (ConfigurationManager.AppSettings["ADD_TLS_SECURITY"] != null)
                {
                    bool parser = false;
                    if (bool.TryParse(ConfigurationManager.AppSettings["ADD_TLS_SECURITY"], out parser))
                        config.ADD_TLS_SECURITY = parser;
                }

            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }
    }
}
