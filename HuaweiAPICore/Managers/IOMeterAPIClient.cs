using EILib.Dao.Managers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using static EILib.Managers.RestClientManager;

namespace HuaweiAPICore.Managers {

    public sealed class IOMeterAPIClient : ManagerBase {
        private String baseURL = "http://dev1.sgh.snpds.com:8023";

        public void DataInizioContratto() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/0-0:94.39.103.255_1,0_2", RestClientMethodEnum.GET);
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

        public void CostanteTrasformazione() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/1-0:0.4.2.255_1,0_2", RestClientMethodEnum.GET);
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

        public void EnergiaTotaleIstantaneaPrelevata() {
            /*  try {
                  HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/1-0:1.8.0.255_3,0_2", RestClientMethodEnum.GET);
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
              catch (Exception ex) {
                  Console.WriteLine(ex.ToString());
              }*/
        }

        public bool GetValue() {
            try {
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
                EnergiaTotaleIstantaneaPrelevata();
                //PotenzaInstantanea();
                //DataInizioContratto();
                PotenzaAttivaPrelevataInstantanea();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            return true;
        }

        public HttpWebRequest initrequest(String urlpass, String requestENUM) {
            string requestMethod = requestENUM;
            string url = baseURL + urlpass;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = requestMethod;
            request.ContentType = "application/json";
            return request;
        }

        public void PotenzaContrattuale() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/0-0:94.39.33.255_3,0_2", RestClientMethodEnum.GET);
            String responseString;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
                using (Stream responseStream = response.GetResponseStream()) {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    responseString = reader.ReadToEnd().ToString();
                }
            }
            if (responseString.Length > 0) {
                int count = 0;
                dynamic json = JsonConvert.DeserializeObject(responseString);
                foreach (var item in json) {
                    if (count == 2) {
                        Console.WriteLine(item);
                    }
                    count++;
                }
            }
        }

        public void PotenzaAttivaPrelevataInstantanea() {
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

        public void CodiceCliente() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/0-0:94.39.100.255_1,0_2", RestClientMethodEnum.GET);
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

        public void CodicePOD() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/1-0:96.1.0.255_1,0_2", RestClientMethodEnum.GET);
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

        public void PotenzaDisponibile() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/1-0:1.31.0.255_3,0_2", RestClientMethodEnum.GET);
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

        public void FasciaOrariaCorrente() {
            HttpWebRequest request = initrequest("/apps/iomtsgdata/v1/tq9p5j-kthtn-edx7d-ashf8-maxma/IT001E56401705/0-0:96.14.0.255_1,0_2", RestClientMethodEnum.GET);
            String responseString;
            try {
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
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}