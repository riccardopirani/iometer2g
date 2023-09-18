using EIEntityCore.Managers.Huawei;
using EIEntityCore.Models;
using EIEntityCore.Models.Huawei;
using EILib.Dao.Managers;
using HuaweiAPICore.Managers;
using HuaweiAPICore.Models;
using HuaweiAPICore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiSinglePlantLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleAndLog(" ");
            ConsoleAndLog(" ");
            if (args.Length< 2)
            {
                ConsoleAndLog($"Errore - mancano argomenti di avvio");
                ConsoleAndLog($"HuaweiSinglePlantLauncher.exe IDPARK yyyy-MM-dd  per importare un singolo giorno");
                ConsoleAndLog($"HuaweiSinglePlantLauncher.exe IDPARK yyyy-MM-dd  yyyy-MM-dd per importare un periodo");
                return;
            }

            ConfigCFGManager configMan = new ConfigCFGManager();
            ConfigCFG config = new ConfigCFG();
            // metto a true nel programma di lancio manuale in modo da non alterare questo campo
            config.skip_update_LAST_DATE_CALLED = true;
            if (!configMan.LoadConfiguration(ref config))
            {
                ConsoleAndLog($"Errore inatteso in LoadConfiguration");
                return;
            }

            string IDPARK = args[0];
            string _dateFrom = args[1];
            string _dateTo = "";
            
            DateTime dateFrom = default(DateTime);
            DateTime? dateTo = null;
            if(!DateTime.TryParseExact(_dateFrom, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None,out dateFrom ))
            {
                ConsoleAndLog($"Errore parsando dateFrom ");
                return;
            }

            if (args.Length > 2)
            {
                _dateTo = args[2];
                DateTime dateToVal = default(DateTime);
                if (!DateTime.TryParseExact(_dateTo, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out dateToVal))
                {
                    ConsoleAndLog($"Errore parsando dateTo ");
                    return;
                }
                dateTo = dateToVal;
            }
            ProcessaImpianto(config, IDPARK, dateFrom, dateTo);

            EliminaVecchiLog();
            ConsoleAndLog(" ");
            ConsoleAndLog(" ");
        }

        static void EliminaVecchiLog()
        {

            //lancio eliminazione dei vecchi file di log

            try
            {
                //if ((DateTime.Now.Hour >= 6) && (DateTime.Now.Hour <= 9))
                //{
                string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(curAppPath))
                    curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                if (!curAppPath.EndsWith("\\"))
                    curAppPath += "\\";

                DateTime delPrevious = DateTime.Now.AddDays(-30);
                EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
                //}
            }
            catch (Exception)
            {
            }
        }

        static bool ProcessaImpianto(ConfigCFG config, string IDPARK, DateTime dateFrom, DateTime? dateTo)
        {
            bool fl = true;

            try
            {
                HuaweiAPIIntermediate huMan = new HuaweiAPIIntermediate(config);
                huMan.OnNotifyMessage += HuMan_OnNotifyMessage;

                HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
                List<HuaweiAPIDeviceMapping> plantsMapping = hwMan.GetByIDPARK(IDPARK, EIConstants.HU_API_Type_Enum.HUAWEI);
                plantsMapping = plantsMapping.Where(m => m.ENABLED == true).ToList();
                //SOLO PER DEBUG PER LIMITARE I CASI GESTITI
                if(config.DEBUG_MODE)
                {
                    //plantsMapping = plantsMapping.Where(m => m.STATIONCODE == "033699CF9E9F414EA26B297E6B7EF131").ToList();
                }


                foreach (HuaweiAPIDeviceMapping map in plantsMapping)
                {
                    if (dateTo == null)
                    {
                        //ConsoleAndLog($"Processo IDPARK '{IDPARK}' singolo giorno {dateFrom.ToString("yyyy-MM-dd")}");
                        huMan.ProcessaSingoloGiorno(map, dateFrom);
                    }
                    else
                    {
                        ConsoleAndLog($"Processo IDPARK '{IDPARK}' periodo da {dateFrom.ToString("yyyy-MM-dd")} a {dateTo.Value.ToString("yyyy-MM-dd")}");
                        DateTime curDate = dateFrom;
                        while (curDate <= dateTo)
                        {
                            huMan.ProcessaSingoloGiorno(map, curDate);
                            curDate = curDate.AddDays(1);
                        }
                    }
                }


               
            }
            catch (Exception ce)
            {
                ConsoleAndLog($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}");
                fl = false;
            }
            return fl;
        }

        private static void HuMan_OnNotifyMessage(string msg)
        {
            ConsoleAndLog(msg);
        }

        static void ConsoleAndLog(string txt, string type = "")
        {
            if (type == "ERROR")
                LoggerBase.MyLogger.Error(txt);
            else if (type == "FATAL")
                LoggerBase.MyLogger.Fatal(txt);
            else
                LoggerBase.MyLogger.Info(txt);

            Console.WriteLine(txt);
        }
    }
}
