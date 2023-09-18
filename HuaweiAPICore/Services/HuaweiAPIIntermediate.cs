using EIEntityCore.Managers;
using EIEntityCore.Managers.Huawei;
using EIEntityCore.Models;
using EIEntityCore.Models.Huawei;
using EILib.Dao.Managers;
using HuaweiAPICore.Managers;
using HuaweiAPICore.Models;
using HuaweiAPICore.Models.Mapping;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;


/**************************************************/
/* NB: Per far funzionare sul server  monitoraggio con win 2008 r2 occore usare i runtime4.8*/

namespace HuaweiAPICore.Services
{
    public sealed class HuaweiAPIIntermediate : ManagerBase
    {
        ConfigCFG config = null;
        IOMeterAPIClient client = null;

        public HuaweiAPIIntermediate(ConfigCFG _config)
        {
            config = _config;
        }

        public delegate void OnNotifyMessageHandler(string msg);
        public event OnNotifyMessageHandler OnNotifyMessage;

        public void OnNotifyMessage_ex(string msg)
        {
            if (OnNotifyMessage != null)
                OnNotifyMessage(msg);
        }



        public bool ProcessaSingoloGiornoTuttiGliImpianti(DateTime dateFrom)
        {
            bool fl = true;

            try
            {

                OnNotifyMessage_ex($"Processo tutti gli impianti per il giorno {dateFrom.ToString("dd/MM/yyyy")}");

                HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
                List<HuaweiAPIDeviceMapping> plantsMapping = hwMan.GetByAPI_TYPE_ID(EIConstants.HU_API_Type_Enum.HUAWEI);
                //tengo solo gli impianti attivi;
                plantsMapping = plantsMapping.Where(m => m.ENABLED == true).ToList();

                if (plantsMapping.Count == 0)
                    throw new Exception($"Non esiste alcun record {HuaweiAPIDeviceMappingManager.tableName}");

                //se sono in debug e sto testando un solo impianto alla volta 
                //if(config.DEBUG_MODE)
                //{
                //    if (!string.IsNullOrEmpty(config.FORCE_CALC_IDPARK))
                //    {
                //        plantsMapping = plantsMapping.Where(m => m.IDPARK.ToUpper() == config.FORCE_CALC_IDPARK.ToUpper()).ToList();
                //        if (!string.IsNullOrEmpty(config.FORCE_CALC_IDDEVICE))
                //            plantsMapping = plantsMapping.Where(m => m.NOTE.ToUpper() == config.FORCE_CALC_IDDEVICE.ToUpper()).ToList();
                //    }
                //}

                OnNotifyMessage_ex($"Trovati {plantsMapping.Count} dispositivi da processare");
                int i = 1;
                foreach (HuaweiAPIDeviceMapping map in plantsMapping)
                {
                    //ProcessaSingoloGiorno(map.IDPARK, map.NOTE, dateFrom);
                    OnNotifyMessage_ex($"");
                    OnNotifyMessage_ex($"-- Elaborazione {i} su {plantsMapping.Count}");
                    ProcessaSingoloGiorno(map,  dateFrom);
                    i++;
                }
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public bool ProcessaSingoloGiorno(HuaweiAPIDeviceMapping map,/* string IDDEVICE,*/ DateTime dateFrom)
        {
            bool fl = true;

            try
            {
                OnNotifyMessage_ex($"-- Processo IDPARK '{map.IDPARK}' singolo giorno {dateFrom.ToString("yyyy-MM-dd")}");

                if (client == null)
                {
                    client = new IOMeterAPIClient(config);
                    client.util_USERNAME = map.util_USERNAME;
                    client.util_PASSWORD = map.util_PASSWORD;
                    client.OnNotifyMessage += Client_OnNotifyMessage;
                }

                HuaweiRestClientResponse response = new HuaweiRestClientResponse();

                //faccio il login alle API se non ero già passato per il login in precedenza
               // if (string.IsNullOrEmpty(client.XSRF_TOKEN))
                //{
                // faccio il logn ogni volta perche' potrei avere api key differenti.
                //TODO: ottimizzare, potrei accorpare tutti gli account in modo da loggarmi, fare tutti gli impiantidi un account usando medesimo token,
                //poi passare all'accoutn successivo

                //eseguo il login solo se il token e' vuoto, in questo modo riesco a gestire il caso in cui sia già loggato ed evito ri rifare il login che spesso da errore
                //quando in realtà potrei usare per ore il primissimo token generato in precedenza
                if(string.IsNullOrEmpty( client.XSRF_TOKEN))
                {
                    if (!client.LoginWithRetry(map.util_USERNAME, map.util_PASSWORD, ref response))
                        throw new Exception($"Errore facendo il login API {client.GetLastExceptionMessage()}");

                    if (response == null)
                        throw new Exception($"Errore facendo il login API - response null {client.GetLastExceptionMessage()}");

                    if (response != null)
                    {
                        if (!response.success)
                            throw new Exception($"Errore facendo il login API {client.GetLastExceptionMessage()}");
                    }
                }
                    
                //}

                List<HuaweiAPIDeviceMapping> lstMap = new List<HuaweiAPIDeviceMapping>();
                lstMap.Add(map);
                ProcessaDevices(client, lstMap, dateFrom);
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        //public bool ProcessaSingoloGiorno(string IDPARK,/* string IDDEVICE,*/ DateTime dateFrom)
        //{
        //    bool fl = true;

        //    try
        //    {
        //        OnNotifyMessage_ex($"-- Processo IDPARK '{IDPARK}' singolo giorno {dateFrom.ToString("yyyy-MM-dd")}");

        //        if(client == null)
        //        {
        //            client = new HuaweiAPIClient(config);
        //            client.OnNotifyMessage += Client_OnNotifyMessage;
        //        }
                
        //        HuaweiRestClientResponse response = new HuaweiRestClientResponse();
        //        //controllo se esiste il record nella tabella huawei_devices_mapping
        //        HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
        //        List<HuaweiAPIDeviceMapping> plantsMapping = hwMan.GetByIDPARK(IDPARK);
        //        plantsMapping = plantsMapping.Where(m => m.ENABLED == true).ToList();



        //        //DEBUG
        //        if (config.DEBUG_MODE)
        //        {
        //            plantsMapping = plantsMapping.Where(m => m.DEVID == "1000000035059649").ToList();
        //        }
                
                
        //        //END DEBUG


        //        //if (!string.IsNullOrEmpty(IDDEVICE))
        //        //    plantsMapping = plantsMapping.Where(m => m.NOTE.ToUpper() == IDDEVICE.ToUpper()).ToList();

        //        if (plantsMapping.Count == 0)
        //            throw new Exception($"Non esiste alcun record {HuaweiAPIDeviceMappingManager.tableName} per IDPARK {IDPARK}");

        //        //faccio il login alle API se non ero già passato per il login in precedenza
        //        if(string.IsNullOrEmpty(client.XSRF_TOKEN))
        //        {
        //            if (!client.LoginWithRetry(ref response))
        //                throw new Exception($"Errore facendo il login API {client.GetLastExceptionMessage()}");

        //            if (response == null)
        //                throw new Exception($"Errore facendo il login API - response null {client.GetLastExceptionMessage()}");

        //            if (response != null)
        //            {
        //                if (!response.success)
        //                    throw new Exception($"Errore facendo il login API {client.GetLastExceptionMessage()}");
        //            }
        //        }

        //        ProcessaDevices(client, plantsMapping, dateFrom);
        //    }
        //    catch (Exception ce)
        //    {
        //        SetLastException(ce);
        //        fl = false;
        //    }
        //    return fl;
        //}

        private void Client_OnNotifyMessage(string msg)
        {
            OnNotifyMessage_ex(msg);
        }

        public bool ProcessaDevices(IOMeterAPIClient client, List<HuaweiAPIDeviceMapping> plantsMapping, DateTime dateFrom)
        {
            bool fl = true;

            try
            {
                HuaweiAPIDimensionManager dimMan = new HuaweiAPIDimensionManager(config.cloudenergyConnection);
                List<string> MAPIDs = plantsMapping.Select(m => m.MAPID).Distinct().ToList();
                List<HuaweiAPIDimension> allMapDims = dimMan.GetByIDMAPIDList(MAPIDs);

                foreach (HuaweiAPIDeviceMapping map in plantsMapping)
                {
                    //Aggiorno il record di mapping in modo da aggiornare il campo LAST_DATE_CALLED con l'ultima data in cui ho tentato un'operazione
                    //se ho lanciato il programma manualmente allora non aggiorno il campo in modo che indichi sempre e solo i dati del servizio
                    if (!config.skip_update_LAST_DATE_CALLED)
                    {
                        AggiornaLastDatecalled(map.MAPID);
                    }
                    

                    OnNotifyMessage_ex($"Richiedo dati per il dispositivo IDPARK '{map.IDPARK}' NOTE '{map.NOTE}' giorno '{dateFrom.ToString("dd/MM/yyyy")}'");
                    List<GetDevFiveMinutesRequest> allTimeItems = new List<GetDevFiveMinutesRequest>();
                    //creo la richiesta da passare alle api
                    DataItemRequest req = new DataItemRequest() { devIds = map.DEVID, devTypeId = map.DEVTYPEID};
                    req.collectTime = EILib.Utilities.Dates.UtilityDates.DateTimeToUnixTimestamp(dateFrom, false);
                    //devo moltiplicare per mille perche' l'api vuole il time cosi'
                    req.collectTime = req.collectTime * 1000;

                    string jsonResponse = "";
                    HuaweiRestClientResponse devFiveMinutesResponse = null;
                    List<GetDevFiveMinutesRequest> items = null;

                    //tenta di leggere fino a n volte con uno sleep in mezzo
                    int retryCount = 0;
                    bool callAPiOK = false;
                    bool needRelogin = false;
                    //for(int i=0; i< config.RETRY_COUNT;i++)
                    //{
                        client.ClearLastException();
                        if (!client.GetDevFiveMinutes(req, ref devFiveMinutesResponse, ref items, ref needRelogin, ref jsonResponse))
                        {
                            //se il token e' scaduto e devo rifare il login, lo eseguo ora
                            if(needRelogin)
                            {
                                client.XSRF_TOKEN = null;
                                HuaweiRestClientResponse loginResp = null;
                                client.LoginWithRetry(client.util_USERNAME, client.util_PASSWORD, ref loginResp);
                            }
                            else
                            {
                                OnNotifyMessage_ex($"Errore nella chiamata API GetDevFiveMinutes, attendo prima di riprovare - {client.GetLastExceptionMessage()}");
                                LoggerBase.MyLogger.Error($"Errore in GetDevFiveMinutes {client.GetLastExceptionMessage()}");
                                retryCount++;
                                if (config.RETRY_SLEEP_MILLIS > 0)
                                    System.Threading.Thread.Sleep(config.RETRY_SLEEP_MILLIS);
                            }
                            
                        }
                        else
                        {
                            callAPiOK = true;
                            //break;
                        }
                    //}
                    
                    if(callAPiOK)
                    {
                        //salvo su file il responso json dell'api. NB: visto che richiedo sempre tutto il giorno non vado in append ma sovrascrivo altrimenti ottengo dei file da 100 MB ogni giorno
                        if((!string.IsNullOrEmpty(jsonResponse)) &&(items.Count > 0))
                            SaveApiResponse(map.API_JSON_FOLDER, map.IDPARK, map.NOTE, dateFrom, jsonResponse);

                        OnNotifyMessage_ex($"Richiesta dati a 5 minuti per IDPARK '{map.IDPARK}' NOTE '{map.NOTE}' ha ritornato {items.Count} records");
                        //LoggerBase.MyLogger.Error($"Richiesta dati a 5 minuti per IDPARK '{map.IDPARK}' NOTE '{map.NOTE}' ha ritornato {items.Count} records");
                        if (items.Count >0)
                        {
                            //mappo i dati ricevuti sulla configurazione dispositivi e scrivo su db i dati
                            List<HuaweiAPIDimension> deviceDimMap = allMapDims.Where(m => m.MAPID == map.MAPID).OrderBy(m => m.ORDER_DIM).ToList();
                            EseguiMappaturaDevices(map, deviceDimMap, dateFrom, items);
                        }
                        else
                        {
                            //LoggerBase.MyLogger.Error($"Nessun record orario da processare");
                            OnNotifyMessage_ex($"Nessun record orario da processare");
                        }
                    }

                    //Aggiorno il record di mapping in modo da aggiornare il campo LAST_DATE_CALLED con l'ultima data in cui ho tenmtato un'operazione
                    //se hoi lanciato il programma manualmente allora non aggiorno il campo in modo che indichi sempre e solo i dati del servizio
                    if (!config.skip_update_LAST_DATE_CALLED)
                    {
                        AggiornaLastDatecalled(map.MAPID);
                    }
                }
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }



        


        bool EseguiMappaturaDevices(HuaweiAPIDeviceMapping map, List<HuaweiAPIDimension> deviceDimMap, DateTime processingDate, List<GetDevFiveMinutesRequest> items, bool writeToDB = true)
        {
            bool fl = true;
            //string lastDevice = "";
            //string lastGrandezza = "";
            //string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            try
            {
                OnNotifyMessage_ex("Eseguo mappatura");

                List<GeneralInputsDataQuarantine> returnedGID = new List<GeneralInputsDataQuarantine>();
                List<WeatherStationsDataQuarantine> returnedWSD = new List<WeatherStationsDataQuarantine>();
                List<StringBoxDataQuarantine> returnedISTR = new List<StringBoxDataQuarantine>();
                List<BatteryData> returnedBAT = new List<BatteryData>();

                //controllo quati dispositivi `general inputs` genera la mappatura (es un meter genera sempre almeno 2 dispositivi per immessa e prelevata)
                List<string> dispGI = deviceDimMap.Select(m => m.NAME).Distinct().ToList();
                foreach(string dispo in dispGI)
                {
                    OnNotifyMessage_ex($"Processo il dispositivo '{dispo}'");

                    List<HuaweiAPIDimension> singleDeviceParams = deviceDimMap.Where(m => m.NAME == dispo).ToList();
                    HuaweiAPIDimension DimIDDEVICE = singleDeviceParams.Where(m => m.DIMENSION == "IDDEVICE").FirstOrDefault();
                    HuaweiAPIDimension DevType = singleDeviceParams.Where(m => m.DIMENSION == "DEVICE_TYPE").FirstOrDefault();
                    string IDDEVICE = "";
                    
                    if (DevType == null)
                    {
                        OnNotifyMessage_ex($"Impossibile trovare la dimension DEVICE_TYPE, impossibile eseguire la mappatura delle grandezze per IDPARK '{map.IDPARK}', NOTE'{map.NOTE}'");
                        //throw new Exception($"Impossibile trovare la dimension DEVICE_TYPE, impossibile eseguire la mappatura delle grandezze per IDPARK '{map.IDPARK}', IDDEVICE'{map.IDDEVICE}'");
                    }
                    if (DimIDDEVICE == null)
                    {
                        OnNotifyMessage_ex($"Impossibile trovare la dimension IDDEVICE, impossibile eseguire la mappatura delle grandezze per IDPARK '{map.IDPARK}', NOTE'{map.NOTE}'");
                        //throw new Exception($"Impossibile trovare la dimension IDDEVICE, impossibile eseguire la mappatura delle grandezze per IDPARK '{map.IDPARK}', IDDEVICE'{map.IDDEVICE}'");
                    }
                    else
                    {
                        IDDEVICE = DimIDDEVICE.FORMULA;
                    }

                    if((!string.IsNullOrEmpty(IDDEVICE)) && (DevType!= null))
                    {
                        SingleDeviceConfigBase deviceConfig = null;
                        switch (DevType.FORMULA)
                        {
                            case HuaweiAPIDimension_DEVICE_TYPE_Enum.INVERTER:
                            case HuaweiAPIDimension_DEVICE_TYPE_Enum.METER:
                                {
                                    deviceConfig = new GIDDeviceConfig();
                                    break;
                                }
                            case HuaweiAPIDimension_DEVICE_TYPE_Enum.STRINGBOX:
                                {
                                    deviceConfig = new ISTRDeviceConfiguration();
                                    break;
                                }
                            case HuaweiAPIDimension_DEVICE_TYPE_Enum.METEO:
                                {
                                    deviceConfig = new WSDDeviceConfiguration();
                                    break;
                                }
                            case HuaweiAPIDimension_DEVICE_TYPE_Enum.BATTERY:
                                {
                                    deviceConfig = new BatteryDeviceConfig();
                                    break;
                                }
                        }

                        if(deviceConfig!= null)
                        {
                            deviceConfig.parameters = singleDeviceParams;
                            deviceConfig.IDPARK = map.IDPARK;
                            deviceConfig.IDCUSTOMER = map.IDCUSTOMER;
                            deviceConfig.IDDEVICE = IDDEVICE;

                            List<GeneralInputsDataQuarantine> returnedGID_s = new List<GeneralInputsDataQuarantine>();
                            List<WeatherStationsDataQuarantine> returnedWSD_s = new List<WeatherStationsDataQuarantine>();
                            List<StringBoxDataQuarantine> returnedISTR_s = new List<StringBoxDataQuarantine>();
                            List<BatteryData> returnedBAT_s = new List<BatteryData>();
                            MappaVariabili(deviceConfig, items, ref returnedGID_s, ref returnedWSD_s, ref returnedISTR_s, ref returnedBAT_s);
                            if (returnedGID_s.Count > 0)
                                returnedGID.AddRange(returnedGID_s);
                            if (returnedWSD_s.Count > 0)
                                returnedWSD.AddRange(returnedWSD_s);
                            if (returnedISTR_s.Count > 0)
                                returnedISTR.AddRange(returnedISTR_s);
                            if (returnedBAT_s.Count > 0)
                                returnedBAT.AddRange(returnedBAT_s);
                        }
                        else
                        {
                            OnNotifyMessage_ex($"Non sono riuscito a mappare il dispositivo  {dispo}");
                        }
                    }
                }


                OnNotifyMessage_ex("Mappatura di tutti i dispositivi terminata");
                if (writeToDB)
                {
                    OnNotifyMessage_ex("Scrivo i dati su db");
                    if (returnedGID.Count>0)
                        ManageInsertToDB_GID(processingDate, map, returnedGID);
                    if (returnedWSD.Count > 0)
                        ManageInsertToDB_WSD(processingDate, map, returnedWSD);
                    if (returnedISTR.Count > 0)
                        ManageInsertToDB_ISTR(processingDate, map,  returnedISTR);
                    if (returnedBAT.Count > 0)
                        ManageInsertToDB_BAT(processingDate, map, returnedBAT);
                    OnNotifyMessage_ex("Scrittura su db terminata");
                }
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        

        bool MappaVariabili(SingleDeviceConfigBase deviceConfig, 
                            List<GetDevFiveMinutesRequest> items,
                            ref List<GeneralInputsDataQuarantine> returnedGID,
                            ref List<WeatherStationsDataQuarantine> returnedWSD,
                            ref List<StringBoxDataQuarantine> returnedISTR,
                            ref List<BatteryData> returnedBAT)
        {
            bool fl = true;
            string lastDevice = "";
            string lastGrandezza = "";
            string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            try
            {
                FormulaResolver formulaResolver = new FormulaResolver();
                foreach (GetDevFiveMinutesRequest item in items)
                {
                    Dictionary<string, object> logs = TimeRecordToDictionary(item);
                    //aggiunto dati di mapping
                    if (!logs.ContainsKey("IDCUSTOMER"))
                        logs.Add("IDCUSTOMER", deviceConfig.IDCUSTOMER);
                    if (!logs.ContainsKey("IDPARK"))
                        logs.Add("IDPARK", deviceConfig.IDPARK);
                    if (!logs.ContainsKey("IDDEVICE"))
                        logs.Add("IDDEVICE", deviceConfig.IDDEVICE);

                    bool mapped = true;
                    lastDevice = deviceConfig.IDDEVICE;
                    foreach (HuaweiAPIDimension par in deviceConfig.parameters)
                    {
                        string result = "";
                        lastGrandezza = par.DIMENSION;
                        if (!formulaResolver.ResolveFormula(logs, par, ref result))
                        {
                            mapped = false;
                            break;
                        }
                        else
                            par.util_realValue = result;

                        //associo questa variabile dai parametri al device, in modo da non dover poi riscorrere ogni volta tutti i parametri quando ciclo più sotto
                        //if ((par.DIMENSION == "PAC_SIGN") || (par.DIMENSION == "RUN TIME_SIGN"))
                        //{
                        //    if (deviceConfig.GetType() == typeof(GIDDeviceConfig))
                        //    {
                        //        GIDDeviceConfig gid = (GIDDeviceConfig)deviceConfig;
                        //        if (par.DIMENSION == "PAC_SIGN")
                        //            gid.PAC_SIGN = par.FORMULA;
                        //        if (par.DIMENSION == "RUN TIME_SIGN")
                        //            gid.RUN_TIME_SIGN = par.FORMULA;
                        //    }
                        //}
                    }

                    if (mapped)
                    {
                        if (deviceConfig.GetType() == typeof(WSDDeviceConfiguration))
                        {
                            //curDev = (WSDDeviceConfiguration)config.devices[i];
                            //WSDDeviceConfiguration tempDev = new WSDDeviceConfiguration();
                            WSDDeviceConfiguration tempDev = (WSDDeviceConfiguration)deviceConfig;
                            WeatherStationsDataQuarantine curWSD = null;
                            if (ManageWSDMapping(tempDev, dateTimeFormat, ref curWSD))
                            {
                                if (curWSD != null)
                                    returnedWSD.Add(curWSD);
                            }
                        }
                        else if (deviceConfig.GetType() == typeof(ISTRDeviceConfiguration))
                        {
                            //curDev = (ISTRDeviceConfiguration)config.devices[i];
                            //ISTRDeviceConfiguration tempDev = new ISTRDeviceConfiguration();
                            ISTRDeviceConfiguration tempDev = (ISTRDeviceConfiguration)deviceConfig;
                            StringBoxDataQuarantine curISTR = null;
                            if (ManageISTRMapping( tempDev, dateTimeFormat, ref curISTR))
                            {
                                if (curISTR != null)
                                    returnedISTR.Add(curISTR);
                            }
                        }
                        else if (deviceConfig.GetType() == typeof(GIDDeviceConfig))
                        {
                            //GIDDeviceConfig tempDev = new GIDDeviceConfig();
                            GIDDeviceConfig tempDev = (GIDDeviceConfig)deviceConfig;
                            List<GeneralInputsDataQuarantine> curGids = new List<GeneralInputsDataQuarantine>();
                            if (ManageGIDMapping( tempDev, dateTimeFormat, ref curGids))
                            {
                                if ((curGids != null) && (curGids.Count > 0))
                                    returnedGID.AddRange(curGids);
                            }
                        }
                        else if (deviceConfig.GetType() == typeof(BatteryDeviceConfig))
                        {
                            //BatteryDeviceConfig tempDev = new BatteryDeviceConfig();
                            //tempDev.IDPARK = map.IDPARK;
                            //tempDev.IDCUSTOMER = map.IDCUSTOMER;
                            //tempDev.IDDEVICE = map.IDDEVICE;

                            BatteryDeviceConfig tempDev = (BatteryDeviceConfig)deviceConfig;
                            BatteryData curBAT = null;
                            if (ManageBATMapping( tempDev, dateTimeFormat, ref curBAT))
                            {
                                if (curBAT != null)
                                    returnedBAT.Add(curBAT);
                            }
                        }

                       
                    }
                }
            }
            catch(Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }

        bool ManageInsertToDB_GID(DateTime processingDate, HuaweiAPIDeviceMapping map, List<GeneralInputsDataQuarantine> returnedGID)
        {
            bool fl = true;
            if (returnedGID.Count == 0)
                return fl;
            //record filtrati che non sono già presenti sul db e che devo inserire in questo ciclo
            List<GeneralInputsDataQuarantine> insertItems = new List<GeneralInputsDataQuarantine>();
            try
            {
                //ottengo tutti i record della giornata già inseriti per controlloare se devo inserire le righe processate ora o no
                GeneralInputsDataQuarantineManager gidQMan = new GeneralInputsDataQuarantineManager(config.cloudenergyConnection);
                DateTime endDate = processingDate.AddDays(1);
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);

                List<string> iddevices = returnedGID.Select(m => m.IDDEVICE).Distinct().ToList();
                string whereDevices = "";
                if (iddevices.Count == 1)
                    whereDevices = $" IDDEVICE = '{iddevices[0]}'";
                else
                    whereDevices = $" IDDEVICE in ({EILib.Utilities.Utilities.ListToString(iddevices,',','\'')})";

                List<GeneralInputsDataQuarantine> gids = gidQMan.GetByRawQueryMulti($"select * from {GeneralInputsDataQuarantineManager.tableName} where IDPARK='{map.IDPARK}' and {whereDevices} and DATETIME >='{processingDate.ToString("yyyy-MM-dd")}' and DATETIME <='{endDate.ToString("yyyy-MM-dd")}' order by DATETIME");

                foreach(GeneralInputsDataQuarantine gg in returnedGID)
                {
                    bool insert = true;
                    GeneralInputsDataQuarantine lastItem = gids.Where(m => m.IDCUSTOMER == gg.IDCUSTOMER
                                                                           && m.IDPARK == gg.IDPARK
                                                                           && m.IDDEVICE == gg.IDDEVICE
                                                                           && m.DATETIME == gg.DATETIME).
                                                                           OrderByDescending(m => m.DATETIME).LastOrDefault();
                    if(lastItem!= null)
                    {
                        if (lastItem.ENERGY_YELDED <= gg.ENERGY_YELDED)
                            insert = false;
                    }
                    if (insert)
                        insertItems.Add(gg);
                }
                LoggerBase.MyLogger.Error($"{insertItems.Count}/{returnedGID.Count} records GID da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                OnNotifyMessage($"{insertItems.Count}/{returnedGID.Count} records GID da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                if (insertItems.Count > 0)
                {
                    insertItems = insertItems.OrderBy(m => m.DATETIME).ToList();
                    WriteDatabaseGID(insertItems);
                    DateTime maxDateITems = default(DateTime);
                    GeneralInputsDataQuarantine lastDato = insertItems.LastOrDefault();

                    if((lastDato!= null) &&(lastDato.DATETIME!= null))
                        maxDateITems = lastDato.DATETIME.Value;
                    //se sto inserendo un dato piu' recente rispetto a quello rpesente su db, aggiorno il dato
                    if ((map.LAST_DATE_ACQUIRED == null) || (map.LAST_DATE_ACQUIRED < maxDateITems))
                        AggiornaLastDateAcquired(map.MAPID, maxDateITems);
                }
            }
            catch(Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        bool ManageInsertToDB_BAT(DateTime processingDate, HuaweiAPIDeviceMapping map, List<BatteryData> returnedBAT)
        {
            bool fl = true;
            if (returnedBAT.Count == 0)
                return fl;
            //record filtrati che non sono già presenti sul db e che devo inserire in questo ciclo
            List<BatteryData> insertItems = new List<BatteryData>();
            try
            {
                //ottengo tutti i record della giornata già inseriti per controlloare se devo inserire le righe processate ora o no
                BatteryDataManager gidQMan = new BatteryDataManager(config.cloudenergyConnection);
                DateTime endDate = processingDate.AddDays(1);
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
                List<string> iddevices = returnedBAT.Select(m => m.IDDEVICE).Distinct().ToList();
                string whereDevices = "";
                if (iddevices.Count == 1)
                    whereDevices = $" IDDEVICE = '{iddevices[0]}'";
                else
                    whereDevices = $" IDDEVICE in ({EILib.Utilities.Utilities.ListToString(iddevices, ',', '\'')})";

                List<BatteryData> items = gidQMan.GetByRawQueryMulti($"select * from {BatteryDataManager.tableName} where IDPARK='{map.IDPARK}' and {whereDevices} and DATETIME >='{processingDate.ToString("yyyy-MM-dd")}' and DATETIME <='{endDate.ToString("yyyy-MM-dd")}' order by DATETIME");

                foreach (BatteryData gg in returnedBAT)
                {
                    bool insert = true;
                    BatteryData lastItem = items.Where(m => m.IDCUSTOMER == gg.IDCUSTOMER
                                                                           && m.IDPARK == gg.IDPARK
                                                                           && m.IDDEVICE == gg.IDDEVICE
                                                                           && m.DATETIME == gg.DATETIME).
                                                                           OrderByDescending(m => m.DATETIME).FirstOrDefault();
                    if (lastItem != null)
                    {
                        if (lastItem.CHARGE_PERC <= gg.CHARGE_PERC)
                            insert = false;
                    }
                    if (insert)
                        insertItems.Add(gg);
                }
                LoggerBase.MyLogger.Error($"{insertItems.Count}/{returnedBAT.Count} records -BAT da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                OnNotifyMessage($"{insertItems.Count}/{returnedBAT.Count} records BAT da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                if (insertItems.Count > 0)
                {
                    insertItems = insertItems.OrderBy(m => m.DATETIME).ToList();
                    WriteDatabaseBAT(insertItems);
                    DateTime maxDateITems = default(DateTime);
                    BatteryData lastDato = insertItems.LastOrDefault();

                    if ((lastDato != null) && (lastDato.DATETIME != null))
                        maxDateITems = lastDato.DATETIME.Value;
                    //se sto inserendo un dato piu' recente rispetto a quello presente su db, aggiorno il dato
                    if ((map.LAST_DATE_ACQUIRED == null) || (map.LAST_DATE_ACQUIRED < maxDateITems))
                        AggiornaLastDateAcquired(map.MAPID, maxDateITems);
                }

            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        bool ManageInsertToDB_ISTR(DateTime processingDate, HuaweiAPIDeviceMapping map, List<StringBoxDataQuarantine> returnedISTR)
        {
            bool fl = true;
            if (returnedISTR.Count == 0)
                return fl;
            //record filtrati che non sono già presenti sul db e che devo inserire in questo ciclo
            List<StringBoxDataQuarantine> insertItems = new List<StringBoxDataQuarantine>();
            try
            {
                //ottengo tutti i record della giornata già inseriti per controlloare se devo inserire le righe processate ora o no
                StringBoxDataQuarantineManager EnergyMan = new StringBoxDataQuarantineManager(config.cloudenergyConnection);
                DateTime endDate = processingDate.AddDays(1);
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
                List<string> iddevices = returnedISTR.Select(m => m.IDDEVICE).Distinct().ToList();
                string whereDevices = "";
                if (iddevices.Count == 1)
                    whereDevices = $" IDDEVICE = '{iddevices[0]}'";
                else
                    whereDevices = $" IDDEVICE in ({EILib.Utilities.Utilities.ListToString(iddevices, ',', '\'')})";

                List<StringBoxDataQuarantine> items = EnergyMan.GetByRawQueryMulti($"select * from {StringBoxDataQuarantineManager.tableName} where IDPARK='{map.IDPARK}' and {whereDevices} and DATETIME >='{processingDate.ToString("yyyy-MM-dd")}' and DATETIME <='{endDate.ToString("yyyy-MM-dd")}' order by DATETIME");

                foreach (StringBoxDataQuarantine gg in returnedISTR)
                {
                    bool insert = false;
                    StringBoxDataQuarantine lastItem = items.Where(m => m.IDCUSTOMER == gg.IDCUSTOMER
                                                                           && m.IDPARK == gg.IDPARK
                                                                           && m.IDDEVICE == gg.IDDEVICE
                                                                           && m.DATETIME == gg.DATETIME).
                                                                           OrderByDescending(m => m.DATETIME).FirstOrDefault();
                    if (lastItem == null)
                        insert = true;
                    else
                    {

                        if (gg.TOTALI != lastItem.TOTALI  || gg.ALARM != lastItem.ALARM || gg.VDC != lastItem.VDC)
                            insert = true;
                        if (gg.ISTR1 != lastItem.ISTR1 || gg.ISTR2 != lastItem.ISTR2 || gg.ISTR3 != lastItem.ISTR3 || gg.ISTR4 != lastItem.ISTR4
                                    || gg.ISTR5 != lastItem.ISTR5 || gg.ISTR6 != lastItem.ISTR6
                                    || gg.ISTR7 != lastItem.ISTR7 || gg.ISTR8 != lastItem.ISTR8
                                    || gg.ISTR9 != lastItem.ISTR9 || gg.ISTR10 != lastItem.ISTR10)
                            insert = true;
                    }
                        
                    if (insert)
                        insertItems.Add(gg);
                }
                LoggerBase.MyLogger.Error($"{insertItems.Count} records ISTR da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                OnNotifyMessage($"{insertItems.Count}/{returnedISTR.Count} records ISTR  da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                if (insertItems.Count > 0)
                {
                    insertItems = insertItems.OrderBy(m => m.DATETIME).ToList();
                    WriteDatabaseISTR(insertItems);
                    DateTime maxDateITems = default(DateTime);
                    StringBoxDataQuarantine lastDato = insertItems.LastOrDefault();

                    if ((lastDato != null) && (lastDato.DATETIME != null))
                        maxDateITems = lastDato.DATETIME.Value;
                    //se sto inserendo un dato piu' recente rispetto a quello rpesente su db, aggiorno il dato
                    if ((map.LAST_DATE_ACQUIRED == null) || (map.LAST_DATE_ACQUIRED < maxDateITems))
                        AggiornaLastDateAcquired(map.MAPID, maxDateITems);
                }

            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }


        bool ManageInsertToDB_WSD(DateTime processingDate, HuaweiAPIDeviceMapping map, List<WeatherStationsDataQuarantine> returnedWSD)
        {
            bool fl = true;
            if (returnedWSD.Count == 0)
                return fl;
            //record filtrati che non sono già presenti sul db e che devo inserire in questo ciclo
            List<WeatherStationsDataQuarantine> insertItems = new List<WeatherStationsDataQuarantine>();
            try
            {
                //ottengo tutti i record della giornata già inseriti per controlloare se devo inserire le righe processate ora o no
                WeatherStationsDataQuarantineManager EnergyMan = new WeatherStationsDataQuarantineManager(config.cloudenergyConnection);
                DateTime endDate = processingDate.AddDays(1);
                endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
                List<string> iddevices = returnedWSD.Select(m => m.IDDEVICE).Distinct().ToList();
                string whereDevices = "";
                if (iddevices.Count == 1)
                    whereDevices = $" IDDEVICE = '{iddevices[0]}'";
                else
                    whereDevices = $" IDDEVICE in ({EILib.Utilities.Utilities.ListToString(iddevices, ',', '\'')})";

                List<WeatherStationsDataQuarantine> items = EnergyMan.GetByRawQueryMulti($"select * from {WeatherStationsDataQuarantineManager.tableName} where IDPARK='{map.IDPARK}' and {whereDevices} and DATETIME >='{processingDate.ToString("yyyy-MM-dd")}' and DATETIME <='{endDate.ToString("yyyy-MM-dd")}' order by DATETIME");

                foreach (WeatherStationsDataQuarantine gg in returnedWSD)
                {
                    bool insert = false;
                    WeatherStationsDataQuarantine lastItem = items.Where(m => m.IDCUSTOMER == gg.IDCUSTOMER
                                                                           && m.IDPARK == gg.IDPARK
                                                                           && m.IDDEVICE == gg.IDDEVICE
                                                                           && m.DATETIME == gg.DATETIME).
                                                                           OrderByDescending(m => m.DATETIME).FirstOrDefault();
                    if (lastItem == null)
                        insert = true;
                    else
                    {

                        if (gg.INCLINED_DIRECT_RADIATION != lastItem.INCLINED_DIRECT_RADIATION || gg.PLANE_DIRECT_RADIATION != lastItem.PLANE_DIRECT_RADIATION)
                            insert = true;
                    }

                    if (insert)
                        insertItems.Add(gg);
                }
                LoggerBase.MyLogger.Error($"{insertItems.Count} records WSD da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                OnNotifyMessage($"{insertItems.Count}/{returnedWSD.Count} records WSD  da inserire per IDPARK '{map.IDPARK}' IDDEVICE '{map.NOTE}' ");
                if (insertItems.Count > 0)
                {
                    insertItems = insertItems.OrderBy(m => m.DATETIME).ToList();
                    WriteDatabaseWSD(insertItems);
                    DateTime maxDateITems = default(DateTime);
                    WeatherStationsDataQuarantine lastDato = insertItems.LastOrDefault();

                    if ((lastDato != null) && (lastDato.DATETIME != null))
                        maxDateITems = lastDato.DATETIME.Value;
                    //se sto inserendo un dato piu' recente rispetto a quello rpesente su db, aggiorno il dato
                    if ((map.LAST_DATE_ACQUIRED == null) || (map.LAST_DATE_ACQUIRED < maxDateITems))
                        AggiornaLastDateAcquired(map.MAPID, maxDateITems);
                }
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            return fl;
        }



        bool WriteDatabaseGID(List<GeneralInputsDataQuarantine> returnedITems)
        {
            bool fl = true;
            int insertCC = 0;
            int maxQQ = 300;

            if (returnedITems.Count == 0)
                return true;

            try
            {
                //trasformo in dati in comando mysql insert e le accorpo in modo da lanciarlee massivamente n alla volta
                GeneralInputsDataQuarantineManager dqGIDManager = new GeneralInputsDataQuarantineManager(config.cloudenergyConnection);
                List<string> query = new List<string>();
                dqGIDManager.PrepareInsert(returnedITems, maxQQ, ref query);
                if(query.Count>0)
                {
                    for(int i=0; i< query.Count;i++)
                    {
                        int insertSingle = 0;
                        dqGIDManager.ExecuteNonQuery(query[i], ref insertSingle);
                        insertCC += insertSingle;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            OnNotifyMessage_ex($"Record GID inseriti : {insertCC}");
            return fl;
        }


        bool WriteDatabaseBAT(List<BatteryData> returnedITems)
        {
            bool fl = true;
            int insertCC = 0;
            int maxQQ = 300;

            if (returnedITems.Count == 0)
                return true;

            try
            {
                //trasformo in dati in comando mysql insert e le accorpo in modo da lanciarlee massivamente n alla volta
                BatteryDataManager dqGIDManager = new BatteryDataManager(config.cloudenergyConnection);
                List<string> query = new List<string>();
                dqGIDManager.PrepareInsert(returnedITems, maxQQ, ref query);
                if (query.Count > 0)
                {
                    for (int i = 0; i < query.Count; i++)
                    {
                        int insertSingle = 0;
                        dqGIDManager.ExecuteNonQuery(query[i], ref insertSingle);
                        insertCC += insertSingle;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            OnNotifyMessage_ex($"Record BAT inseriti : {insertCC}");
            return fl;
        }

        //bool WriteDatabaseGID(List<GeneralInputsDataQuarantine> returnedGID)
        //{
        //    bool fl = true;
        //    int inserted = 0;

        //    try
        //    {   //tento l'inserimento dei dati nel db
        //        GeneralInputsDataQuarantineManager dqGIDManager = new GeneralInputsDataQuarantineManager(config.cloudenergyConnection);
        //        foreach (GeneralInputsDataQuarantine item in returnedGID)
        //        {
        //            if (dqGIDManager.Insert(item))
        //            {
        //                inserted++;
        //                //ok record inserito
        //                //aggiungo il record alla lista che mi servirà per scrivere nel file data_done gli elementi processati correttamente
        //                // insertedGID.Add(item);
        //            }
        //            else
        //            {
        //                //record non inserito
        //                LoggerBase.MyLogger.Error($"Errore inserendo il record dqGID {LoggerBase.SerializeObject(item)}");
        //            }
        //        }
        //    }
        //    catch (Exception ce)
        //    {
        //        LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
        //    }
        //    OnNotifyMessage_ex($"Record inseriti : {inserted}");
        //    return fl;
        //}


        bool WriteDatabaseWSD(List<WeatherStationsDataQuarantine> returnedITems)
        {
            bool fl = true;
            int insertCC = 0;
            int maxQQ = 300;

            if (returnedITems.Count == 0)
                return true;

            try
            {
                //trasformo in dati in comando mysql insert e le accorpo in modo da lanciarlee massivamente n alla volta
                WeatherStationsDataQuarantineManager manager = new WeatherStationsDataQuarantineManager(config.cloudenergyConnection);
                List<string> query = new List<string>();
                manager.PrepareInsert(returnedITems, maxQQ, ref query);
                if (query.Count > 0)
                {
                    for (int i = 0; i < query.Count; i++)
                    {
                        int insertSingle = 0;
                        manager.ExecuteNonQuery(query[i], ref insertSingle);
                        insertCC += insertSingle;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            OnNotifyMessage_ex($"Record WSD inseriti : {insertCC}");
            return fl;
        }

        //public bool WriteDatabaseWSD(List<WeatherStationsDataQuarantine> returnedWSD)
        //{
        //    bool fl = true;
        //    int insertCC = 0;

        //    try
        //    {   //tento l'inserimento dei dati nel db
        //        WeatherStationsDataQuarantineManager dqManager = new WeatherStationsDataQuarantineManager(config.cloudenergyConnection);
        //        foreach (WeatherStationsDataQuarantine item in returnedWSD)
        //        {
        //            if (dqManager.Insert(item))
        //            {
        //                insertCC++;
        //                //ok record inserito
        //                //aggiungo il record alla lista che mi servirà per scrivere nel file data_done gli elementi processati correttamente
        //                // insertedGID.Add(item);
        //            }
        //            else
        //            {
        //                //record non inserito
        //                LoggerBase.MyLogger.Error($"Errore inserendo il record dqWSD {LoggerBase.SerializeObject(item)}");
        //            }
        //        }
        //    }
        //    catch (Exception ce)
        //    {
        //        LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
        //    }
        //    OnNotifyMessage_ex($"Record WSD inseriti : {insertCC}");
        //    return fl;
        //}


        bool WriteDatabaseISTR(List<StringBoxDataQuarantine> returnedITems)
        {
            bool fl = true;
            int insertCC = 0;
            int maxQQ = 300;

            if (returnedITems.Count == 0)
                return true;

            try
            {
                //trasformo in dati in comando mysql insert e le accorpo in modo da lanciarlee massivamente n alla volta
                StringBoxDataQuarantineManager manager = new StringBoxDataQuarantineManager(config.cloudenergyConnection);
                List<string> query = new List<string>();
                manager.PrepareInsert(returnedITems, maxQQ, ref query);
                if (query.Count > 0)
                {
                    for (int i = 0; i < query.Count; i++)
                    {
                        int insertSingle = 0;
                        manager.ExecuteNonQuery(query[i], ref insertSingle);
                        insertCC += insertSingle;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
            }
            OnNotifyMessage_ex($"Record ISTR inseriti : {insertCC}");
            return fl;
        }

        //public bool WriteDatabaseISTR(List<StringBoxDataQuarantine> returnedISTR)
        //{
        //    bool fl = true;
        //    int insertCC = 0;

        //    try
        //    {   //tento l'inserimento dei dati nel db
        //        StringBoxDataQuarantineManager dqManager = new StringBoxDataQuarantineManager(config.cloudenergyConnection);
        //        foreach (StringBoxDataQuarantine item in returnedISTR)
        //        {
        //            if (dqManager.Insert(item))
        //            {
        //                insertCC++;
        //                //ok record inserito
        //                //aggiungo il record alla lista che mi servirà per scrivere nel file data_done gli elementi processati correttamente
        //                // insertedGID.Add(item);
        //            }
        //            else
        //            {
        //                //record non inserito
        //                LoggerBase.MyLogger.Error($"Errore inserendo il record dqISTR {LoggerBase.SerializeObject(item)}");
        //            }
        //        }
        //    }
        //    catch (Exception ce)
        //    {
        //        LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
        //    }
        //    OnNotifyMessage_ex($"Record ISTR inseriti : {insertCC}");
        //    return fl;
        //}



        bool ManageGIDMapping(GIDDeviceConfig dev, string datetimeFormat, ref List<GeneralInputsDataQuarantine> gidList)
        {
            bool fl = true;

            try
            {
                //if (string.IsNullOrEmpty(datetimeFormat))
                //datetimeFormat = "yyyy-MM-dd HH:mm";
                gidList = new List<GeneralInputsDataQuarantine>();
                //posso controllare tutti i parametri e trovare il valore sostituito in util_realValue

                //numero di dispositivi presenti nella configurazione, per es. il multimetro e' sempre legato a un dispositivo di immessa ed uno di prelevata
                //mi aspetto quindi due liste di parametri con NAME differenti
                
                GeneralInputsDataQuarantine curGid = new GeneralInputsDataQuarantine();
                //List<HuaweiAPIDimension> parameters = dev.parameters.Where(m => m.NAME == dispo).ToList();
                if (dev.MappaParametriSuVariabili(/*settings, */dev.parameters))
                {
                    curGid.IDCUSTOMER = dev.IDCUSTOMER;
                    curGid.IDPARK = dev.IDPARK;
                    curGid.IDDEVICE = dev.IDDEVICE;

                    System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                    CultureInfo cu = CultureInfo.CreateSpecificCulture("en-GB");

                    curGid.ENERGY_YELDED = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ENERGY_YELDED, null, styles, cu);
                    curGid.VAC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.VAC, null, styles, cu);
                    curGid.IAC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.IAC, null, styles, cu);
                    curGid.PAC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.PAC, null, styles, cu);
                    curGid.VDC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.VDC, null, styles, cu);
                    curGid.IDC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.IDC, null, styles, cu);
                    curGid.PDC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.PDC, null, styles, cu);
                    curGid.STATUS = dev.STATUS;
                    curGid.ALARM = dev.ALARM;
                    curGid.RUN_TIME = dev.RUN_TIME;

                    //se la PAC non appartiane al segno specificato per il devices, la metto a null. Questo perche' se ho un solo devices (multimetro) che misura
                    //sia l'immessa che la prelevata, e' appunto il segno che rappresenta la direzione e devo scrivere nel contatore giusto a seconda del caso

                    if ((!string.IsNullOrEmpty(dev.PAC_SIGN)) && (curGid.PAC != null))
                    {
                        if (dev.PAC_SIGN == "-")
                        {
                            if (curGid.PAC > 0)
                                curGid.PAC = 0;     // era null
                            else
                            {
                                if (curGid.PAC == -1)
                                    curGid.PAC = 0; // solo per il caso cattelwash che ha un bug il multimetro per cui ogni tanto manda -1 costante // era null
                                else
                                    curGid.PAC = -1 * curGid.PAC.Value;
                            }

                        }
                        else if (dev.PAC_SIGN == "+")
                        {
                            if (curGid.PAC < 0)
                                curGid.PAC = 0; // era null
                                                //else
                                                //curGid.PAC = -1 * curGid.PAC.Value;
                        }
                    }


                    if (curGid.RUN_TIME != null)
                    {
                        decimal? run_time = null;
                        if (!string.IsNullOrEmpty(curGid.RUN_TIME))
                        {
                            decimal run_timeV = 0;
                            if (decimal.TryParse(curGid.RUN_TIME, out run_timeV))
                                run_time = run_timeV;
                        }

                        if (run_time != null)
                        {
                            if (!string.IsNullOrEmpty(dev.RUN_TIME_SIGN))
                            {

                                if (dev.RUN_TIME_SIGN == "-")
                                {
                                    if (run_time.Value > 0)
                                        curGid.RUN_TIME = "0";     // era null
                                    else
                                    {
                                        if (run_time == -1)
                                            curGid.RUN_TIME = "0"; // solo per il caso cattelwash che ha un bug il multimetro per cui ogni tanto manda -1 costante // era null
                                        else
                                            curGid.RUN_TIME = (-1 * run_time).ToString();
                                    }
                                }
                                else if (dev.RUN_TIME_SIGN == "+")
                                {
                                    if (run_time < 0)
                                        curGid.RUN_TIME = "0"; // era null
                                                                //else
                                                                //curGid.PAC = -1 * curGid.PAC.Value;
                                }
                            }
                            else
                            {
                                if (run_time < 0)
                                {
                                    run_time = run_time * -1;
                                    curGid.RUN_TIME = run_time.ToString();
                                }
                            }
                        }
                    }

                    //mi assicuro che il separatore decimale del cosphi sia un punto
                    if (!string.IsNullOrEmpty(curGid.RUN_TIME))
                        curGid.RUN_TIME = curGid.RUN_TIME.Replace(",", ".");

                    DateTime datetime = default(DateTime);
                    if (DateTime.TryParseExact(dev.DATETIME, datetimeFormat, null, System.Globalization.DateTimeStyles.None, out datetime))
                    {
                        curGid.DATETIME = datetime;
                        curGid.LAST_UPDATE = DateTime.Now;
                        //returnedGID.Add(curGid);

                        gidList.Add(curGid);
                    }
                    else
                        LoggerBase.MyLogger.Error($"Errore mappando la data del record {dev.DATETIME}");
                }
                else
                {
                    //errore estraendo i valori dalla lista parametrica e poplando le variabili di classe
                    LoggerBase.MyLogger.Error($"errore estraendo i valori dalla lista parametrica e poplando le variabili di classe  dev is {LoggerBase.SerializeObject(dev)}");
                }
                


            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }



        bool ManageBATMapping(/*HuaweiAPIDeviceMapping settings, */BatteryDeviceConfig dev, string datetimeFormat, ref BatteryData curGid)
        {
            bool fl = true;

            try
            {
                //if (string.IsNullOrEmpty(datetimeFormat))
                //datetimeFormat = "yyyy-MM-dd HH:mm";
                curGid = new BatteryData();
                //posso controllare tutti i parametri e trovare il valore sostituito in util_realValue
                if (dev.MappaParametriSuVariabili(/*settings, */dev.parameters))
                {
                    curGid.IDCUSTOMER = dev.IDCUSTOMER;
                    curGid.IDPARK = dev.IDPARK;
                    curGid.IDDEVICE = dev.IDDEVICE;

                    System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                    CultureInfo cu = CultureInfo.CreateSpecificCulture("en-GB");
                    curGid.STATUS = dev.STATUS;
                    curGid.ALARM = dev.ALARM;
                    curGid.DEVICE_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.DEVICE_TEMPERATURE, null, styles, cu);
                    curGid.MODULE_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.MODULE_TEMPERATURE, null, styles, cu);
                    curGid.HEALTH = EILib.Utilities.Utilities.StringToDecimalNullable(dev.HEALTH, null, styles, cu);
                    curGid.CHARGE_PERC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.CHARGE_PERC, null, styles, cu);
                    curGid.POWER = EILib.Utilities.Utilities.StringToDecimalNullable(dev.POWER, null, styles, cu);
                    curGid.CHARGE_CAP = EILib.Utilities.Utilities.StringToDecimalNullable(dev.CHARGE_CAP, null, styles, cu);
                    curGid.DISCHARGE_CAP = EILib.Utilities.Utilities.StringToDecimalNullable(dev.DISCHARGE_CAP, null, styles, cu);
                    curGid.VOLTAGE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.VOLTAGE, null, styles, cu);
                    curGid.CURRENT = EILib.Utilities.Utilities.StringToDecimalNullable(dev.CURRENT, null, styles, cu);

                    DateTime datetime = default(DateTime);
                    if (DateTime.TryParseExact(dev.DATETIME, datetimeFormat, null, System.Globalization.DateTimeStyles.None, out datetime))
                    {
                        curGid.DATETIME = datetime;
                        curGid.LAST_UPDATE = DateTime.Now;
                        //returnedGID.Add(curGid);
                    }
                    else
                        LoggerBase.MyLogger.Error($"Errore mappando la data del record {dev.DATETIME}");
                }
                else
                {
                    //errore estraendo i valori dalla lista parametrica e poplando le variabili di classe
                    LoggerBase.MyLogger.Error($"errore estraendo i valori dalla lista parametrica e poplando le variabili di classe  dev is {LoggerBase.SerializeObject(dev)}");
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }



        bool ManageWSDMapping( WSDDeviceConfiguration dev, string datetimeFormat, ref WeatherStationsDataQuarantine curWSD)
        {
            bool fl = true;

            try
            {
                curWSD = new WeatherStationsDataQuarantine();
                //posso controllare tutti i parametri e trovare il valore sostituito in util_realValue
                if (dev.MappaParametriSuVariabili(/*settings, */dev.parameters))
                {
                    curWSD.IDCUSTOMER = dev.IDCUSTOMER;
                    curWSD.IDPARK = dev.IDPARK;
                    curWSD.IDDEVICE = dev.IDDEVICE;

                    System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                    CultureInfo cu = CultureInfo.CreateSpecificCulture("en-GB");

                    curWSD.DEVICE_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.DEVICE_TEMPERATURE, null, styles, cu);
                    curWSD.PLANE_DIRECT_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.PLANE_DIRECT_RADIATION, null, styles, cu);
                    curWSD.INCLINED_DIRECT_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.INCLINED_DIRECT_RADIATION, null, styles, cu);
                    curWSD.TRACKER_DIRECT_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.TRACKER_DIRECT_RADIATION, null, styles, cu);
                    curWSD.PLANE_DIFFUSE_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.PLANE_DIFFUSE_RADIATION, null, styles, cu);
                    curWSD.INCLINED_DIFFUSE_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.INCLINED_DIFFUSE_RADIATION, null, styles, cu);
                    curWSD.TRACKER_DIFFUSE_RADIATION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.TRACKER_DIFFUSE_RADIATION, null, styles, cu);
                    curWSD.WIND_DIRECTION = EILib.Utilities.Utilities.StringToDecimalNullable(dev.WIND_DIRECTION, null, styles, cu);
                    curWSD.WIND_SPEED = EILib.Utilities.Utilities.StringToDecimalNullable(dev.WIND_SPEED, null, styles, cu);
                    curWSD.ENVIRONMENTAL_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ENVIRONMENTAL_TEMPERATURE, null, styles, cu);
                    curWSD.MODULE_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.MODULE_TEMPERATURE, null, styles, cu);
                    curWSD.STATUS = dev.STATUS;
                    curWSD.ALARM = dev.ALARM;

                    DateTime datetime = default(DateTime);
                    if (DateTime.TryParseExact(dev.DATETIME, datetimeFormat, null, System.Globalization.DateTimeStyles.None, out datetime))
                    {
                        curWSD.DATETIME = datetime;
                        curWSD.LAST_UPDATE = DateTime.Now;
                        //returnedGID.Add(curGid);
                    }
                    else
                        LoggerBase.MyLogger.Error($"Errore mappando la data del record {dev.DATETIME}");
                }
                else
                {
                    //errore estraendo i valori dalla lista parametrica e poplando le variabili di classe
                    LoggerBase.MyLogger.Error($"errore estraendo i valori dalla lista parametrica e poplando le variabili di classe  dev is {LoggerBase.SerializeObject(dev)}");
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }


        bool ManageISTRMapping(/*HuaweiAPIDeviceMapping settings, */ISTRDeviceConfiguration dev, string datetimeFormat, ref StringBoxDataQuarantine curISTR)
        {
            bool fl = true;

            try
            {
                curISTR = new StringBoxDataQuarantine();
                //posso controllare tutti i parametri e trovare il valore sostituito in util_realValue
                if (dev.MappaParametriSuVariabili(/*settings, */dev.parameters))
                {
                    curISTR.IDCUSTOMER = dev.IDCUSTOMER;
                    curISTR.IDPARK = dev.IDPARK;
                    curISTR.IDDEVICE = dev.IDDEVICE;

                    System.Globalization.NumberStyles styles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign;
                    CultureInfo cu = CultureInfo.CreateSpecificCulture("en-GB");

                    curISTR.DEVICE_TEMPERATURE = EILib.Utilities.Utilities.StringToDecimalNullable(dev.DEVICE_TEMPERATURE, null, styles, cu);
                    curISTR.TOTALI = EILib.Utilities.Utilities.StringToDecimalNullable(dev.TOTALI, null, styles, cu);
                    curISTR.VDC = EILib.Utilities.Utilities.StringToDecimalNullable(dev.VDC, null, styles, cu);
                    curISTR.ISTR1 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR1, null, styles, cu);
                    curISTR.ISTR2 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR2, null, styles, cu);
                    curISTR.ISTR3 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR3, null, styles, cu);
                    curISTR.ISTR4 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR4, null, styles, cu);
                    curISTR.ISTR5 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR5, null, styles, cu);
                    curISTR.ISTR6 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR6, null, styles, cu);
                    curISTR.ISTR7 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR7, null, styles, cu);
                    curISTR.ISTR8 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR8, null, styles, cu);
                    curISTR.ISTR9 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR9, null, styles, cu);
                    curISTR.ISTR10 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR10, null, styles, cu);
                    curISTR.ISTR11 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR11, null, styles, cu);
                    curISTR.ISTR12 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR12, null, styles, cu);
                    curISTR.ISTR13 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR13, null, styles, cu);
                    curISTR.ISTR14 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR14, null, styles, cu);
                    curISTR.ISTR15 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR15, null, styles, cu);
                    curISTR.ISTR16 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR16, null, styles, cu);
                    curISTR.ISTR17 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR17, null, styles, cu);
                    curISTR.ISTR18 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR18, null, styles, cu);
                    curISTR.ISTR19 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR19, null, styles, cu);
                    curISTR.ISTR20 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR20, null, styles, cu);
                    curISTR.ISTR21 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR21, null, styles, cu);
                    curISTR.ISTR22 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR22, null, styles, cu);
                    curISTR.ISTR23 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR23, null, styles, cu);
                    curISTR.ISTR24 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR24, null, styles, cu);
                    curISTR.ISTR25 = EILib.Utilities.Utilities.StringToDecimalNullable(dev.ISTR25, null, styles, cu);
                    curISTR.STATUS = dev.STATUS;
                    curISTR.ALARM = dev.ALARM;

                    DateTime datetime = default(DateTime);
                    if (DateTime.TryParseExact(dev.DATETIME, datetimeFormat, null, System.Globalization.DateTimeStyles.None, out datetime))
                    {
                        curISTR.DATETIME = datetime;
                        curISTR.LAST_UPDATE = DateTime.Now;
                        //returnedGID.Add(curGid);
                    }
                    else
                        LoggerBase.MyLogger.Error($"Errore mappando la data del record {dev.DATETIME}");
                }
                else
                {
                    //errore estraendo i valori dalla lista parametrica e poplando le variabili di classe
                    LoggerBase.MyLogger.Error($"errore estraendo i valori dalla lista parametrica e poplando le variabili di classe  dev is {LoggerBase.SerializeObject(dev)}");
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }

        Dictionary<string, object> TimeRecordToDictionary(GetDevFiveMinutesRequest item)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (item != null)
                {
                    //serializzo e trasformo in un dictionary
                    string serialData = Newtonsoft.Json.JsonConvert.SerializeObject(item.dataItemMap);
                    res = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(serialData);
                    //aggiungo la data
                    if ((!res.ContainsKey("DATETIME")) &&(item.util_date!= null))
                        res.Add("DATETIME", item.util_date.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch(Exception ce)
            {
                SetLastException(ce);
                //fl = false;
                LoggerBase.MyLogger.Error($"Errore in TimeRecordToDictionary deserializzando per ottenere un dictionary", ce);
            }
            return res;
        }

        string GetJsonLocalFilePath(string API_JSON_FOLDER, string IDPARK, string NOTE, DateTime dateProcess)
        {
            if (string.IsNullOrEmpty(NOTE))
                NOTE = "unknow";

            NOTE = NOTE.Replace(" ", "_");

            string fullDir = API_JSON_FOLDER;
            if (!fullDir.EndsWith("\\"))
                fullDir += "\\";

            fullDir += $"{dateProcess.Year}\\{dateProcess.Month}\\{dateProcess.Day}\\";
            fullDir += dateProcess.ToString("yyyy_MM_dd") + "__" + $"{IDPARK}__{NOTE}" + ".json";

            /***************************************************************/
            //solo per debug locale, devo trasformar eil path nella mia condivisione
            if (config.DEBUG_MODE)
            {
                string localMapp = @"c:\inetpub";
                fullDir = fullDir.Replace(@"\\EI-SRV-MONITOR\inetpub", localMapp);
            }

            return fullDir;
        }



        bool SaveApiResponse(string API_JSON_FOLDER, string IDPARK, string NOTE, DateTime dateProcess, string jsonResponse)
        {
            bool fl = true;

            try
            {
                //System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(API_JSON_FOLDER);
                //DirectorySecurity ds = di.GetAccessControl();

                //se la risposta era vuoto evito di scriverla nel file e di creare il folder del giorno
                if (jsonResponse == "[]")
                {
                    OnNotifyMessage_ex("La risposta json era vuota, non la scrivo nel file");
                    return true;
                }


                string fullPath = GetJsonLocalFilePath(API_JSON_FOLDER, IDPARK, NOTE, dateProcess);
                LoggerBase.MyLogger.Info($"fullpath is '{fullPath}'");

                string dirPath = System.IO.Path.GetDirectoryName(fullPath);
                
                System.IO.DirectoryInfo dd = new System.IO.DirectoryInfo(dirPath);
                if (!dd.Exists)
                {
                    LoggerBase.MyLogger.Error($"La cartella '{dirPath}' non esiste, la creo");
                    dd.Create();
                    dd = new System.IO.DirectoryInfo(dirPath);
                    if (!dd.Exists)
                        throw new Exception($"Cartella non trovata dopo averla creata?!? - {dirPath}");
                }
                //System.IO.FileInfo fs = new System.IO.FileInfo(fullDir + "\\" + dateProcess.ToString("yyyy_MM_dd") + "__" +$"{IDPARK}__{IDDEVICE}__{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}"  + ".json");

               

                LoggerBase.MyLogger.Info($"Scrivo il json sul file '{fullPath}'");
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fullPath, false);
                sw.WriteLine(jsonResponse);
                sw.Close();
            }
            catch(Exception ce)
            {
                SetLastException(ce);
                fl = false;
                LoggerBase.MyLogger.Error($"Errore scrivendo il file json su disco in {MethodBase.GetCurrentMethod().Name}: {ce.Message}", ce);
                OnNotifyMessage_ex($"Errore scrivendo il file json su disco in {MethodBase.GetCurrentMethod().Name}: {ce.Message}");
                //LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);

            }
            return fl;
        }

        //public bool ProcessaDevices(HuaweiAPIClient client, List<HuaweiAPIDeviceMapping> plantsMapping, DateTime dateFrom)
        //{
        //    bool fl = true;

        //    try
        //    {
        //        List<GetDevFiveMinutesRequest> allTimeItems = new List<GetDevFiveMinutesRequest>();
        //        //accorpo i devices dello stesso tipo per fare meno chiamate API
        //        List<int> deviceTypes = plantsMapping.Select(m => m.DEVTYPEID).ToList();
        //        foreach (int tipo in deviceTypes)
        //        {
        //            List<HuaweiAPIDeviceMapping> devPerTipo = plantsMapping.Where(m => m.DEVTYPEID == tipo).ToList();
        //            //creo la richiesta da passare alle api
        //            DataItemRequest req = new DataItemRequest() { devIds = "", devTypeId = tipo };
        //            req.collectTime = EILib.Utilities.Dates.UtilityDates.DateTimeToUnixTimestamp(dateFrom, false);
        //            //devo moltiplicare per mille perche' l'api vuole il time cosi'
        //            req.collectTime = req.collectTime * 1000;
        //            List<string> idDevices = devPerTipo.Select(m => m.DEVID).ToList();
        //            req.devIds = EILib.Utilities.Utilities.ListToString(idDevices, ',', null);

        //            HuaweiRestClientResponse devFiveMinutesResponse = null;
        //            List<GetDevFiveMinutesRequest> items = null;

        //            string jsonResponse = "";
        //            if (!client.GetDevFiveMinutes(req, ref devFiveMinutesResponse, ref items, ref jsonResponse))
        //            {
        //                LoggerBase.MyLogger.Error($"Errore in GetDevFiveMinutes {client.GetLastExceptionMessage()}");
        //            }
        //            else
        //            {
        //                //salvo su file il responso json dell'api
        //                //jsonResponse

        //                //inserisco tutti i risultati dentro la stessa lista, dividero' dopo per iddeveice
        //                if (items.Count > 0)
        //                    allTimeItems.AddRange(items);

        //            }
        //        }

        //        List<HuaweiDeviceTimeVM> hwDevicemaps = new List<HuaweiDeviceTimeVM>();
        //        //qui ho tutti gli item di tutti i dispositivi dentro la medesima lista   allTimeItems
        //        //scorro gli iddevices per dividere  gli items
        //        foreach (HuaweiAPIDeviceMapping pp in plantsMapping)
        //        {
        //            HuaweiDeviceTimeVM devMap = new HuaweiDeviceTimeVM() { DEVID = pp.DEVID, DEVTYPEID = pp.DEVTYPEID, STATIONCODE = pp.STATIONCODE, IDPARK = pp.IDPARK, IDDEVICE = pp.IDDEVICE };
        //            devMap.items = allTimeItems.Where(m => m.devId.ToString() == pp.DEVID).ToList();
        //            hwDevicemaps.Add(devMap);
        //        }

        //        EseguiMappaturaDevices(hwDevicemaps);

        //    }
        //    catch (Exception ce)
        //    {
        //        SetLastException(ce);
        //        fl = false;
        //    }
        //    return fl;
        //}


        //bool EseguiMappaturaDevices(List<HuaweiDeviceTimeVM>  hwDevicemaps)
        //{
        //    bool fl = true;

        //    try
        //    {
        //        //carico la mappatura variabili
        //        HuaweiAPIDimensionManager dimMapping = new HuaweiAPIDimensionManager(config.cloudenergyConnection);
        //        List<string> MAPIDs = hwDevicemaps.Select(m => m.MAPID).Distinct().ToList();
        //        List<HuaweiAPIDimension> dimensions = dimMapping.GetByIDMAPIDList(MAPIDs);
        //        if(dimensions.Count > 0)
        //        {
        //            foreach(HuaweiDeviceTimeVM dev in hwDevicemaps)
        //            {
        //                List<HuaweiAPIDimension> curDim = dimensions.Where(m => m.MAPID == dev.MAPID).ToList();
        //            }
        //        }
        //        else
        //        {
        //            LoggerBase.MyLogger.Error($"Nessuna dimension per MAPIDs {MAPIDs.ToString()}", dimMapping.GetLastException());
        //        }
        //    }
        //    catch(Exception ce)
        //    {
        //        SetLastException(ce);
        //        fl = false;
        //    }
        //    return fl;
        //}


        //Aggiorno il record di mapping in modo da aggiornare il campo LAST_DATE_CALLED con l'ultima data in cui ho tentato un'operazione
        public bool AggiornaLastDatecalled(string MAPID)
        {
            bool fl = true;

            try
            {
                string sql = $"update {HuaweiAPIDeviceMappingManager.tableName} set LAST_DATE_CALLED='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE MAPID='{MAPID}'";
                HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
                int recCC = 0;
                hwMan.ExecuteNonQuery(sql, ref recCC);
            }
            catch(Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }

            return fl;
        }


        //Aggiorno il record di mapping in modo da aggiornare il campo LAST_DATE_ACQUIRED con l'ultima data scaricata dal server remoto
        public bool AggiornaLastDateAcquired(string MAPID, DateTime LAST_DATE_ACQUIRED)
        {
            bool fl = true;

            try
            {
                string sql = $"update {HuaweiAPIDeviceMappingManager.tableName} set LAST_DATE_ACQUIRED='{LAST_DATE_ACQUIRED.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE MAPID='{MAPID}'";
                HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
                int recCC = 0;
                hwMan.ExecuteNonQuery(sql, ref recCC);
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }



        public bool RecuperaDaFileSingoloGiorno(string IDPARK, DateTime dateFrom)
        {
            bool fl = true;

            try
            {
                OnNotifyMessage_ex($"-- Processo IDPARK '{IDPARK}' singolo giorno {dateFrom.ToString("yyyy-MM-dd")}");
                
                //controllo se esiste il record nella tabella huawei_devices_mapping
                HuaweiAPIDeviceMappingManager hwMan = new HuaweiAPIDeviceMappingManager(config.cloudenergyConnection);
                List<HuaweiAPIDeviceMapping> plantsMapping = hwMan.GetByIDPARK(IDPARK);

                if(config.DEBUG_MODE)
                {
                    //DEBUG, filtro solo inverter viaggi
                    //plantsMapping = plantsMapping.Where(m => m.DEVID == "1000000035059647").ToList();
                }

                if (plantsMapping.Count == 0)
                    throw new Exception($"Non esiste alcun record {HuaweiAPIDeviceMappingManager.tableName} per IDPARK {IDPARK}");

                HuaweiAPIDimensionManager dimMan = new HuaweiAPIDimensionManager(config.cloudenergyConnection);
                List<string> MAPIDs = plantsMapping.Select(m => m.MAPID).Distinct().ToList();
                List<HuaweiAPIDimension> allMapDims = dimMan.GetByIDMAPIDList(MAPIDs);

                if (allMapDims.Count == 0)
                    throw new Exception($"Non esiste alcun record dimension per il mapping per IDPARK {IDPARK}");




                //plantsMapping = plantsMapping.Where(m => m.ENABLED == true).ToList();

                //if (!string.IsNullOrEmpty(IDDEVICE))
                //    plantsMapping = plantsMapping.Where(m => m.NOTE.ToUpper() == IDDEVICE.ToUpper()).ToList();



                List<GetDevFiveMinutesRequest> items = new List<GetDevFiveMinutesRequest>();
                foreach (HuaweiAPIDeviceMapping record in plantsMapping)
                {
                    try
                    {
                        OnNotifyMessage_ex($"Elaboro il record di mapping {record.NOTE} ");

                        RecuperaSingoloDispositivoDaFile(record, dateFrom, ref items);
                        //mappo i dati ricevuti sulla configurazione dispositivi e scrivo su db i dati
                        if(items.Count > 0)
                        {
                            List<HuaweiAPIDimension> deviceDimMap = allMapDims.Where(m => m.MAPID == record.MAPID).ToList();
                            EseguiMappaturaDevices(record, deviceDimMap, dateFrom, items);
                        }
                    }
                    catch(Exception se)
                    {
                        LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {se.Message}", se);
                    }
                }
            }
            catch (Exception ce)
            {
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }

        public bool RecuperaSingoloDispositivoDaFile  (HuaweiAPIDeviceMapping record, DateTime procDate, ref List<GetDevFiveMinutesRequest> items)
        {
            bool fl = true;

            try
            {
                string fullPath = GetJsonLocalFilePath(record.API_JSON_FOLDER, record.IDPARK, record.NOTE, procDate);

                if (string.IsNullOrEmpty(fullPath))
                    throw new Exception("Impossibile trovare il path nullo");

                string curDir = System.IO.Path.GetDirectoryName(fullPath);
                System.IO.DirectoryInfo dd = new System.IO.DirectoryInfo(curDir);
                if(!dd.Exists)
                    throw new Exception($"La cartella {curDir} non esiste");
                System.IO.FileInfo fs = new System.IO.FileInfo (fullPath);
                if(!fs.Exists)
                    throw new Exception($"Il file {fullPath} non esiste");

                string jsonResponse = "";
                try
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(fullPath);
                    jsonResponse = sr.ReadToEnd();
                    sr.Close();
                }
                catch(Exception se)
                {
                    throw new Exception($"Errore tentando di leggere il file {fullPath} ",se);
                }

                //tento di parsare il conenuto del file (che e' il json di risposta dlel'api dei 5 minuti) nella struttura apposita
                items = new List<GetDevFiveMinutesRequest>();

                string rData = jsonResponse.Replace("\r\n", "");
                rData = rData.Replace("[]", "");                  // TODO: commentare, ho messo solo per gestire i primissimi file scaricati che erano scritti in modo non del tutto corretto
                rData = rData.Replace("][", ",");                 // TODO: commentare, ho messo solo per gestire i primissimi file scaricati che erano scritti in modo non del tutto corretto
                //rData = rData.Replace("\r\n", "");
                try
                {
                    items = JsonConvert.DeserializeObject<List<GetDevFiveMinutesRequest>>(rData);
                }
                catch(Exception se)
                {
                    throw new Exception("errore parsando la stringa JSON!", se);
                }
                
                //popolo il campo data parsando il timestamp
                foreach (GetDevFiveMinutesRequest item in items)
                {
                    //nb il dato ritornato da huawei e' moltiplicato per mille, quindi lo devo dividere per avere un vero timestamp 
                    Int32 realtimestamp = (Int32)(item.collectTime / 1000);
                    item.util_date = EILib.Utilities.Dates.UtilityDates.UnixTimestampToDatetime(realtimestamp, true);
                }

                //FILTRO I DATI E TENGO SOLO QUELLI DEI 5 MINUTI
                //questo perche' l'api e' comuqnue ai 5 minuti ma nel caso di multimetri mi aggiunge dei record col 90% di dati null ad orari dopo i 17, i 22 ecc..
                //ed e' meglio che li salti

                List<GetDevFiveMinutesRequest> filtered = new List<GetDevFiveMinutesRequest>();
                for (int i = 0; i < items.Count; i++)
                {
                    int resto = 0;
                    Math.DivRem(items[i].util_date.Value.Minute, 5, out resto);
                    if ((resto == 0) && (items[i].util_date.Value.Second == 0))
                        filtered.Add(items[i]);
                }
                items = filtered;

            }
            catch(Exception ce)
            {
                SetLastException(ce);
                fl = false;
                OnNotifyMessage_ex(ce.Message);
            }
            return fl;
        }




    }
}

