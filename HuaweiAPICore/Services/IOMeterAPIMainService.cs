using EILib.Dao.Managers;

using System;
using System.ComponentModel;
using System.Reflection;

namespace HuaweiAPICore.Services {

    public sealed class IOMeterAPIMainService : ManagerBase {
        public BackgroundWorker service { get; set; }

        public delegate void ServerStartedHandler();

        public delegate void ServerStoppedHandler();

        public event ServerStartedHandler OnServerStarted;

        public event ServerStoppedHandler OnServerStopped;

        public delegate void NotifyActionHandler(string action, string type, string msg, bool skipDate = false);

        public event NotifyActionHandler OnNotifyAction;

        private string IDPARK_DEBUG = null;
        private string IDDEVICE_DEBUG = null;
        private string DATE_DEBUG = null;

        private void OnNotifyActionEV(string action, string type, string msg, bool skipDate = false) {
            if (OnNotifyAction != null)
                OnNotifyAction(action, type, msg, skipDate);
        }

        public bool StartService(string _IDPARK = null, string _IDDEVICE = null) {
            bool fl = true;

            try {
                IDPARK_DEBUG = _IDPARK;
                IDDEVICE_DEBUG = _IDDEVICE;

                service = new BackgroundWorker();
                service.WorkerSupportsCancellation = true;
                service.DoWork += Service_DoWork;
                service.RunWorkerCompleted += Service_RunWorkerCompleted;
                service.RunWorkerAsync();
            }
            catch (Exception ce) {
                fl = false;
                SetLastException(ce);
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }

        private void ServiceManager_OnNotifyAction(string action, string type, string msg, bool skipDate = false) {
            OnNotifyActionEV(action, type, msg, skipDate);
        }

        private void Service_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (OnServerStopped != null)
                OnServerStopped();
        }

        private bool IsStopService() {
            bool stop = false;
            if (service.CancellationPending)
                stop = true;
            return stop;
        }

        private void Service_DoWork(object sender, DoWorkEventArgs e) {
            bool termina = false;

            if (OnServerStarted != null)
                OnServerStarted();

            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("           Avvio del servizio");
            LoggerBase.MyLogger.Info("/************************************/");
            LoggerBase.MyLogger.Info("");

            while (!termina) {
                termina = IsStopService();
                DateTime initWork = DateTime.Now;
                DateTime processingDate = DateTime.Now;

                processingDate = new DateTime(processingDate.Year, processingDate.Month, processingDate.Day, 0, 0, 0);

                if (IsStopService()) {
                    termina = true;
                    break;
                }

                try {
                    if ((DateTime.Now.Hour >= 6) && (DateTime.Now.Hour <= 9)) {
                        string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        if (!string.IsNullOrEmpty(curAppPath))
                            curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                        if (!curAppPath.EndsWith("\\"))
                            curAppPath += "\\";

                        DateTime delPrevious = DateTime.Now.AddDays(-30);
                        EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
                    }
                }
                catch (Exception) {
                }
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

        private void HuMan_OnNotifyMessage(string msg) {
            OnNotifyActionEV("", "", msg);
            LoggerBase.MyLogger.Info(msg);
        }

        public bool StopService() {
            bool fl = true;
            LoggerBase.MyLogger.Info("Richiesta terminazione servizio");

            try {
                service.CancelAsync();
            }
            catch (Exception ce) {
                fl = false;
                SetLastException(ce);
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }
    }
}