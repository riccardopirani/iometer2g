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

namespace HuaweiRecuperoDaFile
{
    class Program
    {

        /*
         * legge i dati dentro i file json su disco, li parsa e li salva sul db
          */

        static void Main(string[] args)
        {
            ConsoleAndLog(" ");
            ConsoleAndLog(" ");
            if (args.Length < 2)
            {
                ConsoleAndLog($"Errore - mancano argomenti di avvio");
                ConsoleAndLog($"HuaweiRecuperoDaFile.exe IDPARK yyyy-MM-dd  per importare un singolo giorno");
                ConsoleAndLog($"HuaweiRecuperoDaFile.exe IDPARK yyyy-MM-dd  yyyy-MM-dd per importare un periodo");
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
            if (!DateTime.TryParseExact(_dateFrom, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out dateFrom))
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
            RecuperaDatImpianto(config, IDPARK, dateFrom, dateTo);

            ConsoleAndLog(" ");
            ConsoleAndLog(" ");
        }

        private static void HuMan_OnNotifyMessage(string msg)
        {
            ConsoleAndLog(msg);
        }

        static bool RecuperaDatImpianto(ConfigCFG config, string IDPARK, DateTime dateFrom, DateTime? dateTo)
        {
            bool fl = true;

            try
            {
                HuaweiAPIIntermediate huMan = new HuaweiAPIIntermediate(config);
                huMan.OnNotifyMessage += HuMan_OnNotifyMessage;
                if (dateTo == null)
                {
                    //ConsoleAndLog($"Processo IDPARK '{IDPARK}' singolo giorno {dateFrom.ToString("yyyy-MM-dd")}");
                    huMan.RecuperaDaFileSingoloGiorno(IDPARK,  dateFrom);
                }
                else
                {
                    ConsoleAndLog($"Processo MAPID '{IDPARK}' periodo da {dateFrom.ToString("yyyy-MM-dd")} a {dateTo.Value.ToString("yyyy-MM-dd")}");
                    DateTime curDate = dateFrom;
                    while (curDate <= dateTo)
                    {
                        huMan.RecuperaDaFileSingoloGiorno(IDPARK,  curDate);
                        curDate = curDate.AddDays(1);
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
