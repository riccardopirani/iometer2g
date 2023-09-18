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
    public sealed class HuaweiAPIMainService : ManagerBase
    {
        public BackgroundWorker service { get; set; }

        public delegate void ServerStartedHandler();
        public delegate void ServerStoppedHandler();

        public event ServerStartedHandler OnServerStarted;
        public event ServerStoppedHandler OnServerStopped;

        public delegate void NotifyActionHandler(string action, string type, string msg, bool skipDate = false);
        public event NotifyActionHandler OnNotifyAction;

        string IDPARK_DEBUG = null;
        string IDDEVICE_DEBUG = null;
        string DATE_DEBUG = null;

        HuaweiAPIIntermediate huMan = null;


        void OnNotifyActionEV(string action, string type, string msg, bool skipDate = false)
        {
            if (OnNotifyAction != null)
                OnNotifyAction(action, type, msg, skipDate);
        }

        //gli argomenti di lancio servono per lanciare in debug la procesura su un unico impianto
        //normalmente non ci sono e vengono considerati tutti i record huawei_api_device_mapping
        public bool StartService(string _IDPARK = null, string _IDDEVICE=null)
        {
            bool fl = true;

            try
            {
                IDPARK_DEBUG = _IDPARK;
                IDDEVICE_DEBUG = _IDDEVICE;
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
            LoggerBase.MyLogger.Info("       Servizio terminato");
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


        private void Service_DoWork(object sender, DoWorkEventArgs e)
        {

            ConfigCFG config = new ConfigCFG();
            ConfigCFGManager configurationCFGManager = new ConfigCFGManager();
            bool fl = configurationCFGManager.LoadConfiguration(ref config);
            //string erros = "";

            //int MAX_PLANT_TO_LOAD = 2000;
            //bool forzaComunita = false;
            //forzaImpianto = true;

            //string forzaComunity_IDCE = "";
            if (config.DEBUG_MODE == true)
            {
                if (!string.IsNullOrEmpty(IDPARK_DEBUG))
                {
                    config.FORCE_CALC_IDPARK = IDPARK_DEBUG;
                    config.FORCE_CALC_IDDEVICE = IDDEVICE_DEBUG;
                }
            }

            bool termina = false;

            if (OnServerStarted != null)
                OnServerStarted();

            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("           Avvio del servizio");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("");


            while (!termina)
            {
                termina = IsStopService();
                DateTime initWork = DateTime.Now;
                DateTime processingDate = DateTime.Now;

                //se ho passato una data di debug specifica
                if (config.DEBUG_MODE == true)
                {
                    if (!string.IsNullOrEmpty(DATE_DEBUG))
                        DateTime.TryParseExact(DATE_DEBUG, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out processingDate);
                }

                processingDate = new DateTime(processingDate.Year, processingDate.Month, processingDate.Day, 0, 0, 0);

                if (fl)
                {
                    if(huMan == null)
                    {
                        huMan = new HuaweiAPIIntermediate(config);
                        huMan.OnNotifyMessage += HuMan_OnNotifyMessage;
                    }
                    huMan.ProcessaSingoloGiornoTuttiGliImpianti(processingDate);
                    //huMan.ProcessaSingoloGiorno(config.FORCE_CALC_IDPARK, config.FORCE_CALC_IDDEVICE, processingDate);
                    //attendo prima di ricompinciare

                    if (config.RELAUNCH_SERVICE_POLLING_SECS > 0)
                    {
                        OnNotifyActionEV("", "",$"Attesa di {config.RELAUNCH_SERVICE_POLLING_SECS } secondi prima di rilanciare il task");
                        OnNotifyActionEV("", "", $"");
                        OnNotifyActionEV("", "", $"");

                        LoggerBase.MyLogger.Info($"Attesa di {config.RELAUNCH_SERVICE_POLLING_SECS } secondi prima di rilanciare il task");
                        LoggerBase.MyLogger.Info($"");
                        LoggerBase.MyLogger.Info($"");

                        
                        for (int i=0; i< config.RELAUNCH_SERVICE_POLLING_SECS;i++)
                        {
                            if (IsStopService())
                                break;
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                }

                if (IsStopService())
                {
                    termina = true;
                    break;
                }

                //ciclo tutti i precalcoli e se e' scattato il tempo di ricalcolo, lancio il ricalcolo
                //DateTime endWork = DateTime.Now;
                //TimeSpan execDate = endWork - initWork;
                //LoggerBase.MyLogger.Info($"Esecuzione ciclo terminata in ore: {execDate.Hours} min: {execDate.Minutes} sec: {execDate.Seconds} TotalMinutes: {execDate.TotalMinutes} TotalSeconds: {execDate.TotalSeconds} ");
                //OnNotifyActionEV("", "", $"Esecuzione ciclo terminata in ore: {execDate.Hours} min: {execDate.Minutes} sec: {execDate.Seconds} TotalMinutes: {execDate.TotalMinutes} TotalSeconds: {execDate.TotalSeconds} ");

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

                //if (!termina)
                //{
                //    OnNotifyActionEV("", "", "Attesa di 1 minuto prima di rilanciare il task");
                //    System.Threading.Thread.Sleep(60 * 1000);
                //}
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

        private void HuMan_OnNotifyMessage(string msg)
        {
            OnNotifyActionEV("", "", msg);
            LoggerBase.MyLogger.Info(msg);
        }
        //private void CeManager_OnNotifyMessage(string type, string action, string IDPARK, string msg)
        //{
        //    OnNotifyActionEV("", "", msg);
        //    LoggerBase.MyLogger.Info(msg);
        //}

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


    }
}
