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
            Console.WriteLine("------ AVVIO SERVIZIO TEST  ----------------\n\n1)Q-Exit\n\n2)E-EnergiaTotaleIstantaneaPrelevata\n\n3)P-PotenzaContrattuale");
            String inputValues = Console.ReadLine();
            while(!inputValues.Equals("q")) {
                if (inputValues.Equals("e") || inputValues.Equals("E")) {
                    ap.EnergiaTotaleIstantaneaPrelevata();
                }
                else if (inputValues.Equals("p") || inputValues.Equals("P")) {
                    ap.PotenzaContrattuale();
                }
                Console.WriteLine("------ AVVIO SERVIZIO TEST  ----------------\n\n1) q per uscire\n\n2) E) EnergiaTotaleIstantaneaPrelevata\n\n3)P PotenzaContrattuale");

                inputValues = Console.ReadLine();
            }
                EliminaVecchiLog();
            }
        }
    
}