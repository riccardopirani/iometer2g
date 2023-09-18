using EILib.Dao.Managers;
using EILib.Models.JSON;
using HuaweiAPICore.Managers;
using HuaweiAPICore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EIEntityCore.Models.EIConstants;

namespace HuaweiAPITest
{
    class Program
    {
        /********************************************/
        /*  TESTA TUTTE LE CHIAMATE API DI HUAWEI   */
        /********************************************/

        static void Main(string[] args)
        {
            ConfigCFGManager configMan = new ConfigCFGManager();
            ConfigCFG config = new ConfigCFG();
            if(!configMan.LoadConfiguration(ref config))
            {
                ConsoleAndLog($"Errore inatteso in LoadConfiguration");
                return;
            }

            IOMeterAPIClient client = new IOMeterAPIClient(config);
            client.OnNotifyMessage += Client_OnNotifyMessage;
            HuaweiRestClientResponse response = new HuaweiRestClientResponse();
            client.util_USERNAME = config.API_USERNAME;
            client.util_PASSWORD = config.API_PASSWORD;
            if (!client.LoginWithRetry(config.API_USERNAME, config.API_PASSWORD, ref response))
            {
                ConsoleAndLog($"Errore facendo il login API {client.GetLastExceptionMessage()}");
                return;
            }
            if(response!= null)
            {
                if(!response.success)
                {
                    ConsoleAndLog($"Errore nel login {client.GetLastExceptionMessage()}");
                    return;
                }


                /**********************************/
                /*    OTTENGO L'ELENCO IMPIANTI   */
                /**********************************/
                HuaweiRestClientResponse stationListResponse = null;
                List<GetStationListResponse> stations = null;
                //ottengo tutti gli impianti
                if (!client.GetStationList(ref stationListResponse, ref stations))
                {
                    ConsoleAndLog($"Errore in GetStationList {client.GetLastExceptionMessage()}");
                    return;
                }
                if(stationListResponse != null)
                {
                    if(stations != null)
                    {
                        ConsoleAndLog($"Elenco stations associate al login:");
                        for (int i=0; i< stations.Count;i++)
                        {
                            ConsoleAndLog($"NAME '{stations[i].stationName}'  ID '{stations[i].stationCode}'");
                        }
                    }
                }


                /******************************************************/
                /*    OTTENGO L'ELENCO DISPOSITIVI PER UN IMPIANTO    */
                /******************************************************/

                //string stationCode = "6D39ABA6B3D240479D6697AC7AFFBEE1"; // doteco
                string stationCode = "033699CF9E9F414EA26B297E6B7EF131"; // Giordano 21

                HuaweiRestClientResponse deviceListResponse = null;
                List<GetDevListResponse> devices = null;
                //ottengo tutti gli impianti
                if (!client.GetDevList(stationCode, ref deviceListResponse, ref devices))
                {
                    ConsoleAndLog($"Errore in getDevList {client.GetLastExceptionMessage()}");
                    return;
                }
                if (stationListResponse != null)
                {
                    if (devices != null)
                    {
                        ConsoleAndLog($"Elenco devices associate all'impianto '{stationCode}':");
                        for (int i = 0; i < devices.Count; i++)
                        {
                            ConsoleAndLog($"NAME '{devices[i].devName}'  ID '{devices[i].id}' TypeId '{devices[i].devTypeId}'");
                        }
                    }
                }


                /*******************************************************************************************/
                /*    OTTENGO I DATI AI 5 MINUTI DI TUTTI I DISPOSITIVI INVERTER E STRINGHE UN IMPIANTO    */
                /*******************************************************************************************/

                if(devices.Count>0)
                {
                    //tengo solo i devices di tipo inverter e stringhe, li accorpo per tipo per fare meno chiamate 
                    List<GetDevListResponse> devs = devices.Where(m => m.devTypeId == HuaweiConstants.DeviceTypeEnum.STRING_INVERTER || m.devTypeId == HuaweiConstants.DeviceTypeEnum.HOUSEHOLD_INVERTER || m.devTypeId == HuaweiConstants.DeviceTypeEnum.STRING).ToList();
                    if(devs.Count>0)
                    {
                        List<GetDevListResponse> inv_stringa = devs.Where(m => m.devTypeId == HuaweiConstants.DeviceTypeEnum.STRING_INVERTER).ToList();
                        List<GetDevListResponse> inv_casa = devs.Where(m => m.devTypeId == HuaweiConstants.DeviceTypeEnum.HOUSEHOLD_INVERTER).ToList();
                        List<GetDevListResponse> strings = devs.Where(m => m.devTypeId == HuaweiConstants.DeviceTypeEnum.STRING).ToList();

                        HuaweiRestClientResponse devFiveMinutesResponse = null;
                        List<GetDevFiveMinutesRequest> items = null;

                        if (inv_stringa.Count>0)
                        {

                        }
                        if (inv_casa.Count > 0)
                        {
                            DataItemRequest req = new DataItemRequest ();
                            req.devIds = "";
                            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

                            req.collectTime = EILib.Utilities.Dates.UtilityDates.DateTimeToUnixTimestamp(today, false);

                            //il dato deve essere moltiplicato per mille
                            req.collectTime = req.collectTime * 1000;
                            //req.collectTime = 1654520938000;

                            req.devTypeId = HuaweiConstants.DeviceTypeEnum.HOUSEHOLD_INVERTER;
                            for (int i=0; i< inv_casa.Count; i++)
                            {
                                req.devIds += inv_casa[i].id.ToString();
                                if (i < inv_casa.Count - 1)
                                    req.devIds += ",";
                            }

                            string jsonResponse = "";
                            bool needRelogin = false;
                            if (!client.GetDevFiveMinutes(req, ref devFiveMinutesResponse, ref items, ref needRelogin, ref jsonResponse))
                            {
                                ConsoleAndLog($"Errore in GetDevFiveMinutes {client.GetLastExceptionMessage()}");
                                return;
                            }
                            if (devFiveMinutesResponse != null)
                            {
                                if (items != null)
                                {
                                    string[] idDevices = req.devIds.Split(',');
                                    
                                    foreach (string idItem in idDevices)
                                    {
                                        Int64 idDev = 0;
                                        if(Int64.TryParse(idItem, out idDev))
                                        {
                                            ConsoleAndLog($"Elenco DataMap del dispositivo '{idDev}':");
                                            List<GetDevFiveMinutesRequest> plantDevData = items.Where(m => m.devId == idDev).ToList();
                                            foreach(GetDevFiveMinutesRequest dato in plantDevData)
                                            {
                                                //ConsoleAndLog($"date '{dato.util_date.Value.ToString("dd/MM/yyyy HH:mm")}' PDC '{dato.dataItemMap.active_power}' EY '{dato.dataItemMap.total_cap}'");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (strings.Count > 0)
                        {
                        }
                    }
                    else
                    {
                        ConsoleAndLog("Nessun device di tipo inverter o stringa nell'impianto");
                    }
                }
            }

            EliminaVecchiLog();
        }

        static void EliminaVecchiLog()
        {

            //lancio eliminazione dei vecchi file di log

            try
            {
                //if ((DateTime.Now.Hour >= 6) && (DateTime.Now.Hour <= 9))
                //{
                string curAppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(curAppPath))
                    curAppPath = System.IO.Path.GetDirectoryName(curAppPath);
                if (!curAppPath.EndsWith("\\"))
                    curAppPath += "\\";

                DateTime delPrevious = DateTime.Now.AddDays(-30);
                EILib.Dao.Managers.LoggerBase.DeleteLogOlderThanDate(curAppPath, "00.Log", delPrevious);
                //}
            }
            catch (Exception)
            {
            }
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
