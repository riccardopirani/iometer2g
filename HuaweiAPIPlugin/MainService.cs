using HuaweiAPICore.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPIPlugin
{
    public partial class MainService : ServiceBase
    {
        HuaweiAPIMainService theService = null;

        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            theService = new HuaweiAPIMainService();
            theService.OnServerStopped += TheService_OnServerStopped;
            theService.StartService();
        }

        private void TheService_OnServerStopped()
        {

        }

        protected override void OnStop()
        {
            if (theService != null)
                theService.StopService();
        }
    }
}
