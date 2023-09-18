using EILib.Dao.Managers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using static EILib.Managers.RestClientManager;

namespace HuaweiAPICore.Managers {

    public sealed class IOMeterAPIClient : ManagerBase {
        private String baseURL = "http://dev1.sgh.snpds.com:8023";

        public HttpWebRequest initrequest(String urlpass, String requestENUM) {
            string requestMethod = requestENUM;
            string url = baseURL + urlpass;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = requestMethod;
            request.ContentType = "application/json";
            return request;
        }

        public bool GetValue() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/obisdescriptor", RestClientMethodEnum.GET);
            String responseString;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    responseString = reader.ReadToEnd().ToString();
                }
            }
            if (responseString.Length > 0) {
                dynamic json = JsonConvert.DeserializeObject(responseString);
                foreach (var item in json) {
                    Console.WriteLine(item);
                }
            }
            PotenzaInstantanea();
            return true;
        }

        public void PotenzaInstantanea() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/1-0:1.7.0.255_3,0_2", RestClientMethodEnum.GET);
            String responseString;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    responseString = reader.ReadToEnd().ToString();
                }
            }
            if (responseString.Length > 0) {
                dynamic json = JsonConvert.DeserializeObject(responseString);
                foreach (var item in json) {
                    Console.WriteLine(item);
                }
            }
        }
    }
}