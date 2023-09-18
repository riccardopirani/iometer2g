using HuaweiAPICore.Services;
using System.ServiceProcess;

namespace HuaweiAPIPlugin {

    public partial class MainService : ServiceBase {
        private IOMeterAPIMainService theService = null;

        public MainService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            theService = new IOMeterAPIMainService();
            theService.OnServerStopped += TheService_OnServerStopped;
            theService.StartService();
        }

        private void TheService_OnServerStopped() {
        }

        protected override void OnStop() {
            if (theService != null)
                theService.StopService();
        }
    }
}