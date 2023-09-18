using EILib.Dao.Managers;
using HuaweiAPICore.Managers;
using HuaweiAPICore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Services
{
    public sealed class HuaweiPluginService : ManagerBase
    {
        public BackgroundWorker service { get; set; }


        public delegate void ServerStartedHandler();
        public delegate void ServerStoppedHandler();

        public event ServerStartedHandler OnServerStarted;
        public event ServerStoppedHandler OnServerStopped;

        public delegate void NotifyActionHandler(string action, string type, string msg, bool skipDate = false);
        public event NotifyActionHandler OnNotifyAction;

        private string UIDITEM { get; set; }

        void OnNotifyActionEV(string action, string type, string msg, bool skipDate = false)
        {
            if (OnNotifyAction != null)
                OnNotifyAction(action, type, msg, skipDate);
        }

        public bool StartService(string _UIDITEM = null)
        {
            bool fl = true;

            try
            {
                UIDITEM = _UIDITEM;

                ConfigCFG config = new ConfigCFG();
                ConfigCFGManager configurationCFGManager = new ConfigCFGManager();
                if (configurationCFGManager.LoadConfiguration(ref config))
                {
                    service = new BackgroundWorker();
                    service.WorkerSupportsCancellation = true;
                    service.DoWork += Service_DoWork;
                    service.RunWorkerCompleted += Service_RunWorkerCompleted;
                    service.RunWorkerAsync();
                }
            }
            catch (Exception ce)
            {
                fl = false;
                SetLastException(ce);
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }

        private void ServiceManager_OnNotifyAction(string action, string type, string msg, bool skipDate = false)
        {
            OnNotifyActionEV(action, type, msg, skipDate);
        }

        private void Service_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoggerBase.MyLogger.Info("Servizio terminato");
            LoggerBase.MyLogger.Info("//*******************************//");
            LoggerBase.MyLogger.Info(" ");

            if (OnServerStopped != null)
                OnServerStopped();
        }

        bool IsStopService()
        {
            bool stop = false;
            if (service.CancellationPending)
                stop = true;
            return stop;

        }



        public bool StopService()
        {
            bool fl = true;
            LoggerBase.MyLogger.Info("Richiesta terminazione servizio");

            try
            {
                service.CancelAsync();
            }
            catch (Exception ce)
            {
                fl = false;
                SetLastException(ce);
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        private void Service_DoWork(object sender, DoWorkEventArgs e)
        {

            ConfigCFG config = new ConfigCFG();
            ConfigCFGManager configurationCFGManager = new ConfigCFGManager();
            bool fl = configurationCFGManager.LoadConfiguration(ref config);

            
            if (config.DEBUG_MODE == true)
            {

            }

            bool termina = false;

            if (OnServerStarted != null)
                OnServerStarted();

            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("Avvio del servizio");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("");

            while (!termina)
            {
                termina = IsStopService();
                DateTime initWork = DateTime.Now;




                if (IsStopService())
                {
                    termina = true;
                    break;
                }


                //lancio eliminazione dei vecchi file di log

                try
                {
                    if ((DateTime.Now.Hour >= 6) && (DateTime.Now.Hour <= 9))
                    {
                        string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        if (!string.IsNullOrEmpty(curAppPath))
                            curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                        if (!curAppPath.EndsWith("\\"))
                            curAppPath += "\\";

                        DateTime delPrevious = DateTime.Now.AddDays(-30);
                        EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
                    }

                }
                catch (Exception)
                {
                }

                LoggerBase.MyLogger.Info("");
                LoggerBase.MyLogger.Info("/************************************/");
                LoggerBase.MyLogger.Info("Servizio arrestato");
                LoggerBase.MyLogger.Info("/************************************/");
                LoggerBase.MyLogger.Info("");

                OnNotifyActionEV("", "", "");
                OnNotifyActionEV("", "", "/************************************/");
                OnNotifyActionEV("", "", "Servizio arrestato");
                OnNotifyActionEV("", "", "/************************************/");
                OnNotifyActionEV("", "", "");

            }


        }
    }
}
