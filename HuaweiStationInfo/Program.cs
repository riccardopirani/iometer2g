using EILib.Dao.Managers;
using HuaweiAPICore.Managers;
using HuaweiAPICore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiStationInfo
{
    class Program
    {
        
        
        static void Main(string[] args)
        {
            ConsoleAndLog(" ");
            ConsoleAndLog(" ");
            if (args.Length < 2)
            {
                ConsoleAndLog($"Errore - mancano argomenti di avvio");
                ConsoleAndLog($"HuaweiStationInfo.exe API_USERNAME API_PASSWORD   (fornite da huawei)");
                return;
            }

            string API_USERNAME = args[0];
            string API_PASSWORD = args[1];

            ProcessaInfoPlants(API_USERNAME, API_PASSWORD);
            EliminaVecchiLog();
            ConsoleAndLog(" ");
            ConsoleAndLog("Programma terminato.");

            ConsoleAndLog(" ");
            ConsoleAndLog(" ");

        }

        static void EliminaVecchiLog()
        {

            

            try
            {
                
                
                    string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(curAppPath))
                        curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                    if (!curAppPath.EndsWith("\\"))
                        curAppPath += "\\";

                    DateTime delPrevious = DateTime.Now.AddDays(-30);
                    EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
                
            }
            catch (Exception)
            {
            }
        }



        static bool ProcessaInfoPlants(string API_USERNAME, string API_PASSWORD)
        {
            bool fl = true;

            try
            {

                ConfigCFGManager configMan = new ConfigCFGManager();
                ConfigCFG config = new ConfigCFG();
                if (!configMan.LoadConfiguration(ref config))
                {
                    ConsoleAndLog($"Errore inatteso in LoadConfiguration");
                    return false;
                }

                HuaweiAPIClient client = new HuaweiAPIClient(config);
                client.OnNotifyMessage += Client_OnNotifyMessage;
                HuaweiRestClientResponse response = new HuaweiRestClientResponse();
                if (!client.LoginWithRetry(API_USERNAME, API_PASSWORD, ref response))
                {
                    ConsoleAndLog($"Errore facendo il login API {client.GetLastExceptionMessage()}");
                    return false;
                }
                if (response != null)
                {
                    if (!response.success)
                    {
                        ConsoleAndLog($"Errore nel login {client.GetLastExceptionMessage()}");
                        return false;
                    }


                    /**********************************/
                    /*    OTTENGO L'ELENCO IMPIANTI   */
                    /**********************************/
                    
                    List<GetStationListResponse> stations = null;
                    
                    ConsoleAndLog($"Ottengo l'elenco degli impianti...");

                    if(!GetStationList(config, client, ref stations))
                    {
                        ConsoleAndLog($"Errore in GetStationList {client.GetLastExceptionMessage()}");
                        return false;
                    }
                    
                    if (stations != null)
                    {
                        ConsoleAndLog($"Elenco stations associate al login:");
                        stations = stations.OrderBy(m => m.stationName).ToList();
                        for (int i = 0; i < stations.Count; i++)
                        {
                            ConsoleAndLog($"{i+1}  NAME '{stations[i].stationName}'  ID '{stations[i].stationCode}'");
                        }

                        ConsoleAndLog($"");
                        ConsoleAndLog($"Ottengo l'elenco dei dispositivi per ogni impianto");
                        
                        for (int i = 0; i < stations.Count; i++)
                        {
                            List<GetDevListResponse> devices = stations[i].util_devices;
                            FillStationDevices(config, client, i+1, stations[i].stationCode, ref devices);
                            stations[i].util_devices = devices;
                        }
                        
                        SaveStationJson(stations);
                    }
                    
                }
            }
            catch(Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }

        static bool GetStationList(ConfigCFG config, HuaweiAPIClient client, ref List<GetStationListResponse> stations)
        {
            bool fl = true;

            try
            {
                HuaweiRestClientResponse stationListResponse = null;

                int tentativi = 0;
                bool esci = false;
                while (esci == false)
                {
                    if(tentativi ==0)
                        ConsoleAndLog($"Richiedo lista impianti");
                    else
                        ConsoleAndLog($"Richiedo lista impianti- tentativo {tentativi + 1} ");
                    if (client.GetStationList(ref stationListResponse, ref stations))
                    {
                        ConsoleAndLog($"Scaricati {stations.Count} impianti");
                        esci = true;
                    }
                    else
                    {
                        tentativi++;
                        if (tentativi >= config.RETRY_COUNT)
                            esci = true;
                        else
                            System.Threading.Thread.Sleep(config.RETRY_SLEEP_MILLIS);
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        static bool FillStationDevices(ConfigCFG config, HuaweiAPIClient client, int index, string stationCode, ref List<GetDevListResponse> devices)
        {
            bool fl = true;

            try
            {
                HuaweiRestClientResponse deviceListResponse = null;
                devices = null;
                

                int tentativi = 0;
                bool esci = false;
                while(esci == false)
                {
                    if (tentativi == 0)
                        ConsoleAndLog($"{index} - Richiedo devices per station code {stationCode}");
                    else
                        ConsoleAndLog($"{index} - Richiedo devices per station code {stationCode} - tentativo {tentativi + 1} ");

                    if (client.GetDevList(stationCode, ref deviceListResponse, ref devices))
                        esci = true;
                    else
                    {
                        tentativi++;
                        if (tentativi >= config.RETRY_COUNT)
                        {
                            esci = true;
                            ConsoleAndLog($"Impossibile scaricare le informazioni in questro momento, per cortesia riprova tra qualche minuto");
                        }
                        else
                            System.Threading.Thread.Sleep(config.RETRY_SLEEP_MILLIS);
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        static bool SaveStationJson(List<GetStationListResponse> stations)
        {
            bool fl = true;

            try
            {
                string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(curAppPath))
                    curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                if (!curAppPath.EndsWith("\\"))
                    curAppPath += "\\";
                curAppPath += "01.Stations";
                System.IO.DirectoryInfo dd = new System.IO.DirectoryInfo(curAppPath);
                if (!dd.Exists)
                    dd.Create();
                curAppPath += $"\\{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}_stations.json";

                ConsoleAndLog("");
                ConsoleAndLog($"Salvo la struttura degli impianti sul file {curAppPath}");

                string jsonContent = JsonConvert.SerializeObject(stations, Formatting.Indented);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(curAppPath);
                sw.Write(jsonContent);
                sw.Close();
            }
            catch(Exception ce)
            {
                ConsoleAndLog($"Errore salvando il file json in SaveStationJson {ce.Message}");
            }
            return fl;
        }


        private static void Client_OnNotifyMessage(string msg)
        {
            ConsoleAndLog(msg);
        }

        static void ConsoleAndLog(string txt, string type = "")
        {
            if (type == "ERROR")
                LoggerBase.MyLogger.Error(txt);
            else if (type == "FATAL")
                LoggerBase.MyLogger.Fatal(txt);
            else
                LoggerBase.MyLogger.Info(txt);

            Console.WriteLine(txt);
        }
    }
}
