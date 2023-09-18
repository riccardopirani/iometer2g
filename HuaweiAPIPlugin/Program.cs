using System.ServiceProcess;

namespace HuaweiAPIPlugin {

    internal static class Program {

        private static void Main() {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}