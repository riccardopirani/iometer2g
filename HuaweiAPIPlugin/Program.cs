using HuaweiAPICore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPIPlugin
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main()
        {

            //per testare direttamente 

            //HuaweiAPIMainService hw = new HuaweiAPIMainService();
            //hw.StartService();
            //while(true)
            //{
            //    System.Threading.Thread.Sleep(100);
            //}

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
