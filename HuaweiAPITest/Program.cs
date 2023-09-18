using EILib.Dao.Managers;
using HuaweiAPICore.Managers;
using System;

namespace HuaweiAPITest {

    internal class Program {

        private static void EliminaVecchiLog() {
            try {
                string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(curAppPath))
                    curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                if (!curAppPath.EndsWith("\\"))
                    curAppPath += "\\";

                DateTime delPrevious = DateTime.Now.AddDays(-30);
                EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Main(string[] args) {
            IOMeterAPIClient ap = new IOMeterAPIClient();
            ap.GetValue();
            EliminaVecchiLog();
        }
    }
}