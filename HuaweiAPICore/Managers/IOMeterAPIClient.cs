using EILib.Dao.Managers;
using EILib.Managers;
using EILib.Models.JSON;
using HuaweiAPICore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static EILib.Managers.RestClientManager;

namespace HuaweiAPICore.Managers
{
    public sealed class IOMeterAPIClient : ManagerBase
    {
        private ConfigCFG config = null;

        public string XSRF_TOKEN { get; set; }
        public string util_USERNAME { get; set; }
        public string util_PASSWORD { get; set; }

        public IOMeterAPIClient(ConfigCFG _config)
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


        public bool GetDevFiveMinutes(DataItemRequest dtRequest, 
                                    ref HuaweiRestClientResponse response, 
                                    ref List<GetDevFiveMinutesRequest> items, 
                                    ref bool needRelogin, 
                                    ref string jsonResponse)
        {
            bool fl = true;
            int retryCC = 0;
                                                 
            try
            {
                bool esciTentativi = false;
                string fullURL = config.API_BASE_URL + config.getDevFiveMinutes;
                DateTime initProc = DateTime.Now;

                while(esciTentativi == false)
                {
                    response = PostWithHeaderToken(fullURL, dtRequest, config.ADD_TLS_SECURITY);
                    retryCC++;

                    if (response != null)
                    {
                        if (response.success)
                            esciTentativi = true;
                        if (response.failCode == HuaweiConstants.FailCodeEnum.Need_to_login)
                            esciTentativi = true;
                        else if (response.failCode == HuaweiConstants.FailCodeEnum.InterfaceFrequentlyAccessed)
                        {
                            System.Threading.Thread.Sleep(config.RETRY_SLEEP_MILLIS);
                            LoggerBase.MyLogger.Error($"Troppi tentativi, attendo {config.RETRY_SLEEP_MILLIS} millis e ritento");
                        }
                            
                    }

                    if(retryCC >= config.RETRY_COUNT)
                        esciTentativi = true;

                    //se comunque l'operazione impiega piu' di un minuto forzo l'uscita
                    if((DateTime.Now- initProc).TotalSeconds > 60)
                        esciTentativi = true;
                }


                if (response.success)
                {
                    items = null;
                    //tento il cast alla classe corretta
                    try
                    {
                        jsonResponse = response.data.ToString();
                        string rData = jsonResponse.Replace("\r\n", "");
                        items = JsonConvert.DeserializeObject<List<GetDevFiveMinutesRequest>>(rData);
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
                    catch (Exception ce)
                    {
                        LoggerBase.MyLogger.Error("Errore eseguendo il cast a GetDevFiveMinutesResponse dentro GetDevList");
                        throw new Exception("Errore eseguendo il cast a GetDevFiveMinutesResponse dentro GetDevList", ce);
                    }
                }
                else
                {
                    if (response.failCode == HuaweiConstants.FailCodeEnum.Need_to_login)
                    {
                        LoggerBase.MyLogger.Error("GetDevFiveMinutes response unsuccess Need to relogin, we need try to relogin to get new token...");
                        needRelogin = true;
                    }
                    if (response.failCode == HuaweiConstants.FailCodeEnum.InterfaceFrequentlyAccessed)
                    {
                        LoggerBase.MyLogger.Error("GetDevFiveMinutes  - troppe connessioni in poco tempo, errore di frequenza di accesso");
                    }
                    else
                    {
                        if(string.IsNullOrEmpty(response.FullResponse))
                            throw new Exception($"GetDevFiveMinutes - chiamata avvenuta con successo ma non ha ritornato alcun dato ne stato di errore", GetLastException());
                        else
                            throw new Exception($"GetDevFiveMinutes response unsuccess with error '{response.Error}' failcode {response.failCode}", GetLastException());
                    }
                    ClearLastException();
                }

                if (response.failCode != 0)
                {
                    string bbk_failcode = "";
                    bbk_failcode += "";
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public bool GetDevList( string stationCodes, ref HuaweiRestClientResponse response, ref List<GetDevListResponse> devices)
        {
            bool fl = true;

            try
            {
                string fullURL = config.API_BASE_URL + config.getDevList;
                StationCodesRequest req = new StationCodesRequest() { stationCodes = stationCodes, collectTime = null };
                response = PostWithHeaderToken(fullURL, req, config.ADD_TLS_SECURITY);
                if (response == null)
                    throw new Exception("GetDevList response is null", GetLastException());

                if (response.success)
                {
                    devices = null;
                    //tento il cast alla classe corretta
                    try
                    {
                        string rData = response.data.ToString();
                        rData = rData.Replace("\r\n", "");
                        devices = JsonConvert.DeserializeObject<List<GetDevListResponse>>(rData);
                        //stations = response.data;
                    }
                    catch (Exception ce)
                    {
                        LoggerBase.MyLogger.Error("Errore eseguendo il cast a GetDevListResponse dentro GetDevList");
                        throw new Exception("Errore eseguendo il cast a GetDevListResponse dentro GetDevList", ce);
                    }
                }
                else
                {
                    throw new Exception("GetDevList response is null", GetLastException());
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public bool GetStationList(ref HuaweiRestClientResponse response, ref List<GetStationListResponse> stations)
        {
            bool fl = true;

            try
            {
                string fullURL = config.API_BASE_URL + config.getStationList;
                response = PostWithHeaderToken(fullURL, null, config.ADD_TLS_SECURITY);
                if (response == null)
                    throw new Exception("GetStationList response is null", GetLastException());

                if (response.success)
                {
                    stations = null;
                    //tento il cast alla classe corretta
                    try
                    {
                        string rData = response.data.ToString();
                        rData=rData.Replace("\r\n", "");
                        stations = JsonConvert.DeserializeObject<List<GetStationListResponse>>(rData);
                        //stations = response.data;
                    }
                    catch(Exception ce)
                    {
                        LoggerBase.MyLogger.Error("Errore eseguendo il cast a GetStationListResponse dentro GetStationList");
                        throw new Exception("Errore eseguendo il cast a GetStationListResponse dentro GetStationList",ce);
                    }
                }
                else
                {
                    throw new Exception("GetStationList response is null", GetLastException());
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }

        public bool LoginWithRetry(string USERNAME, string PASSWORD, ref HuaweiRestClientResponse response)
        {
            bool fl = true;

            try
            {
                int retryCount = 0;

                for(int i=0; i< config.RETRY_COUNT; i++)
                {
                    if(!LoginSingle(USERNAME, PASSWORD, ref response))
                    {
                        fl = false;
                        retryCount++;
                        if (config.RETRY_SLEEP_MILLIS > 0)
                            System.Threading.Thread.Sleep(config.RETRY_SLEEP_MILLIS);
                        LoggerBase.MyLogger.Error($" Tentativo di login numero {retryCount+1}");
                    }
                    else
                    {
                        fl = true;
                        break;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public bool LoginSingle(string USERNAME, string PASSWORD, ref HuaweiRestClientResponse response)
        {
            bool fl = true;

            try
            {
                Login_Request data = new Login_Request();
                data.userName = USERNAME;
                data.systemCode = PASSWORD;
                string fullURL = config.API_BASE_URL + config.login;


                response = GenericPost(fullURL, data, config.ADD_TLS_SECURITY , true);
                if (response == null)
                    throw new Exception("Login response is null", GetLastException());

                if(response.success)
                {
                    if (string.IsNullOrEmpty(XSRF_TOKEN))
                    {
                        OnNotifyMessage_ex("Login success but XSRF-TOKEN is null!");
                        throw new Exception("Login success but XSRF-TOKEN is null!", GetLastException());
                    }
                    else
                    {
                        OnNotifyMessage_ex("Login success and XSRF-TOKEN returned");
                    }
                }
                else
                {
                    OnNotifyMessage_ex("Login response unsucces");
                    throw new Exception("Login response unsucces", GetLastException());
                }
            }
            catch(Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }

        public bool GetStationsList(ref HuaweiRestClientResponse response)
        {
            bool fl = true;

            try
            {
                string fullURL = config.API_BASE_URL + config.getStationList;
                response = GenericPost(fullURL, null, config.ADD_TLS_SECURITY);
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public bool GetStationRealKpi(string stationCodes, ref ApiResponse response)
        {
            bool fl = true;

            try
            {
                RestClientManager client = new RestClientManager();
                string fullURL = config.API_BASE_URL + config.getStationList;
                StationCodes_API data = new StationCodes_API();
                data.stationCodes = stationCodes;
                response = client.GetApiPostCall(fullURL, data, RestClientMethodEnum.POST, config.ADD_TLS_SECURITY);
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                SetLastException(ce);
                fl = false;
            }
            return fl;
        }


        public HuaweiRestClientResponse GenericPost(string apiFullUrl, object postInputArguments, bool AddSecurity = false, bool isLogin=false)
        {
            HuaweiRestClientResponse apiResponse = new HuaweiRestClientResponse();
            bool fl = true;
            string postStr = null;
            string requestMethod = RestClientMethodEnum.POST;

            if (requestMethod == RestClientMethodEnum.POST)
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    if (postInputArguments != null)
                        postStr = JsonConvert.SerializeObject(postInputArguments, settings);
                }
                catch (Exception ce)
                {
                    fl = false;
                    LogError($"Exception in {MethodBase.GetCurrentMethod().Name} serializing input argument : {ce.Message}", ce);
                    apiResponse.Error = ce.Message;
                    SetLastException(ce);
                }
            }

            if (fl)
            {
                try
                {
                    if (AddSecurity == true)
                    {
                        LogInfo($"Adding security...");

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls  
                                                            | SecurityProtocolType.Tls11
                                                            | SecurityProtocolType.Tls12
                                                            | SecurityProtocolType.Ssl3;

                        ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, errors) => {
                                            return true;
                                        };
                        LogInfo($"End adding security...");
                    }

                    HttpWebRequest request = WebRequest.Create(apiFullUrl) as HttpWebRequest;
                    if (request == null)
                        return new HuaweiRestClientResponse { Error = "No response from api" };
                    request.Method = requestMethod;
                    request.ContentType = "application/json";

                    if (requestMethod == RestClientMethodEnum.POST)
                    {
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postStr);
                        request.ContentLength = bytes.Length;
                        using (Stream requestStream = request.GetRequestStream())
                        {
                            // Send the data.
                            requestStream.Write(bytes, 0, bytes.Length);
                        }
                    }


                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        //apiResponse.webResponse = response;
                        if (response == null)
                        {
                            apiResponse.Error = $"Server api not provide any answer.";
                            return apiResponse;
                        }
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            apiResponse.Error = $"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).";
                            return apiResponse;
                        }
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                            string responseString = reader.ReadToEnd();

                            

                            if(isLogin)
                            {
                                //tento di estrarre il token   XSRF-TOKEN dal coockie ritornato,
                                //dovro' passare questo valore come header di tutte le prossime chiamate
                                if ((response.Headers != null) && (response.Headers.Count > 0))
                                {
                                    if (response.Headers["XSRF-TOKEN"] != null)
                                    {
                                        XSRF_TOKEN = response.Headers.Get("XSRF-TOKEN");
                                    }
                                }
                            }
                            
                            apiResponse = JsonConvert.DeserializeObject<HuaweiRestClientResponse>(responseString);
                            apiResponse.FullResponse = responseString;
                        }
                    }
                }
                catch (Exception ce)
                {
                    // Log.Fatal("Exception", ce);
                    // catch exception and log it
                    //LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                    LogError($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                    apiResponse.Error = ce.Message;
                    SetLastException(ce);
                }
            }
            return apiResponse;
        }



        public HuaweiRestClientResponse PostWithHeaderToken(string apiFullUrl, object postInputArguments, bool AddSecurity = false)
        {
            HuaweiRestClientResponse apiResponse = new HuaweiRestClientResponse();
            bool fl = true;
            string postStr = null;
            string requestMethod = RestClientMethodEnum.POST;

            if (requestMethod == RestClientMethodEnum.POST)
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    if (postInputArguments != null)
                        postStr = JsonConvert.SerializeObject(postInputArguments, settings);
                }
                catch (Exception ce)
                {
                    fl = false;
                    //LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} serializing input argument : {ce.Message}", ce);
                    LogError($"Exception in {MethodBase.GetCurrentMethod().Name} serializing input argument : {ce.Message}", ce);
                    apiResponse.Error = ce.Message;
                    SetLastException(ce);
                }
            }

            if (fl)
            {
                try
                {
                    if (AddSecurity == true)
                    {
                        LogInfo($"Adding security...");

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls  
                                                             | SecurityProtocolType.Tls11
                                                             | SecurityProtocolType.Tls12
                                                             | SecurityProtocolType.Ssl3;

                        ServicePointManager.ServerCertificateValidationCallback +=
                                        (sender, certificate, chain, errors) => {
                                            return true;
                                        };
                        LogInfo($"End adding security...");
                    }

                    HttpWebRequest request = WebRequest.Create(apiFullUrl) as HttpWebRequest;
                    if (request == null)
                        return new HuaweiRestClientResponse { Error = "No response from api" };
                    request.Method = requestMethod;
                    request.ContentType = "application/json";

                    //aggiungo il token all'header
                    request.Headers.Add("XSRF-TOKEN", XSRF_TOKEN);

                    if ((requestMethod == RestClientMethodEnum.POST) && (postStr!= null))
                    {
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                        byte[] bytes = encoding.GetBytes(postStr);
                        request.ContentLength = bytes.Length;
                        using (Stream requestStream = request.GetRequestStream())
                        {
                            // Send the data.
                            requestStream.Write(bytes, 0, bytes.Length);
                        }
                    }


                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        //apiResponse.webResponse = response;
                        if (response == null)
                        {
                            apiResponse.Error = $"Server api not provide any answer.";
                            return apiResponse;
                        }
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            apiResponse.Error = $"Server error (HTTP {response.StatusCode}: {response.StatusDescription}).";
                            return apiResponse;
                        }
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                            string responseString = reader.ReadToEnd();
                            //apiResponse.FullResponse = responseString;

                            if(!string.IsNullOrEmpty(responseString))
                            {
                                apiResponse = JsonConvert.DeserializeObject<HuaweiRestClientResponse>(responseString);
                                if (apiResponse != null)
                                    apiResponse.FullResponse = responseString;
                            }
                        }
                        
                    }
                }
                catch (Exception ce)
                {
                    LogError($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                    apiResponse.Error = ce.Message;
                    SetLastException(ce);
                }
            }
            return apiResponse;
        }


    }
}
