using EILib.Dao.Managers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using static EILib.Managers.RestClientManager;

namespace HuaweiAPICore.Managers {

    public sealed class IOMeterAPIClient : ManagerBase {
        private String baseURL = "http://dev1.sgh.snpds.com:8023";

        public bool GetValue() {
            Console.WriteLine("------------- AVVIO SERVIZIO ------------------------------");
            string requestMethod = RestClientMethodEnum.GET;
            string url = baseURL + "/apps/iomtsgdata/v1/obisdescriptor";
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            request.Method = requestMethod;
            request.ContentType = "application/json";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    string responseString = reader.ReadToEnd();
                    Console.WriteLine(responseString);
                }
            }
            return true;
        }
    }
}