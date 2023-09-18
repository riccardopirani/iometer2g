using EILib.Dao.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using static EILib.Managers.RestClientManager;

namespace HuaweiAPICore.Managers {

    public sealed class IOMeterAPIClient : ManagerBase {
        private String baseURL = "http://dev1.sgh.snpds.com:8023";

        public bool GetValue() {
            Console.WriteLine("------------- AVVIO SERVIZIO ------------------------------");
            string requestMethod = RestClientMethodEnum.GET;
            string url = baseURL + "/apps/iomtsgdata/v1/obisdescriptor";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            String responseString;
            request.Method = requestMethod;
            request.ContentType = "application/json";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    responseString = reader.ReadToEnd().ToString();
                }
            }
            if (responseString.Length > 0) {
                dynamic json = JsonConvert.DeserializeObject(responseString);
                foreach( var item in json) {
                    Console.WriteLine(item);
                }
               
            }


            return true;
        }
    }
}