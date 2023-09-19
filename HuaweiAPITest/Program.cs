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
            string menu = "\n\n\n\n1)Q-Exit\n\n2)E-EnergiaTotaleIstantaneaPrelevata\n\n3)P-PotenzaAttivaPrelevataInstantanea\n\n4)CT - Costante Trasformazione\n\n5) CC - Codice Cliente\n\n6) POD -- Codice Pod\n\n7) PD - Potenza Disponibile\n\n8) FO - Fascia oraria corrente\n\n ----- Inserisci il tuo valore ----------";
            Console.WriteLine(menu);
            String inputValues = Console.ReadLine();
            while (!inputValues.Equals("q")) {
                if (inputValues.Equals("e") || inputValues.Equals("E")) {
                    ap.EnergiaTotaleIstantaneaPrelevata();
                }
                else if (inputValues.Equals("p") || inputValues.Equals("P")) {
                    ap.PotenzaAttivaPrelevataInstantanea();
                }
                else if (inputValues.Equals("CT") || inputValues.Equals("ct")) {
                    ap.CostanteTrasformazione();
                }
                else if (inputValues.Equals("CC") || inputValues.Equals("cc")) {
                    ap.CodiceCliente();
                }
                else if (inputValues.Equals("POD") || inputValues.Equals("pod")) {
                    ap.CodicePOD();
                }
                else if (inputValues.Equals("PD") || inputValues.Equals("pd")) {
                    ap.PotenzaDisponibile();
                }
                else if (inputValues.Equals("FO") || inputValues.Equals("fo")) {
                    ap.FasciaOrariaCorrente();
                }
                Console.WriteLine(menu);
                inputValues = Console.ReadLine();
            }
            EliminaVecchiLog();
        }
    }
}