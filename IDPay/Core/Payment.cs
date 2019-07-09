using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IDPay.Core
{
    public class Payment
    {
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        private static string GetStatus(int status)
        {
            switch (status)
            {
                case 1: return "پرداخت انجام نشده است";
                case 2: return "پرداخت ناموفق بوده است";
                case 3: return "خطا رخ داده است";
                case 4: return "بلوکه شده";
                case 5: return "برگشت به پرداخت کننده";
                case 6: return "برگشت خورده سیستمی";
                case 10: return "در انتظار تایید پرداخت";
                case 100: return "پرداخت تایید شده است";
                case 101: return "پرداخت قبلا تایید شده است";
                case 200: return "به دریافت کننده واریز شد";
                default: return "";
            }
        }

        public class Request
        {
            private string OrderID;
            public string order_id
            {
                get
                {
                    return OrderID;
                }
            }
            public decimal amount { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public string mail { get; set; }
            public string desc { get; set; }
            public string callback
            {
                get
                {
                    return "http://localhost:7885/Home/AfterPayment";
                }
            }

            public Request(string _OrderID)
            {
                if (string.IsNullOrEmpty(_OrderID))
                    OrderID = Guid.NewGuid().ToString();
                else
                    OrderID = _OrderID;
            }
        }

        public class RequestRespons_Success
        {
            public string id { get; set; }

            public string link { get; set; }
        }
        public class RequestRespons_Fail
        {
            public int error_code { get; set; }

            public string error_message { get; set; }
        }
        public class ResultPayment
        {
            public int status { get; set; }
            public string track_id { get; set; }
            public string id { get; set; }
            public string order_id { get; set; }
            public decimal amount { get; set; }
            public string card_no { get; set; }
            public string hashed_card_no { get; set; }
            public double date { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
            public bool IsOK
            {
                get
                {
                    return status == 10;
                }
            }
            public string Message
            {
                get
                {
                    return GetStatus(status);
                }
            }
        }

        public class PaymentInfo
        {
            public int status { get; set; }
            public string track_id { get; set; }
            public string id { get; set; }
            public string order_id { get; set; }
            public decimal amount { get; set; }
            public double date { get; set; }
            public PaymentDetail payment { get; set; }
            public VerifyDate verify { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
            public bool IsOK
            {
                get
                {
                    return status == 100;
                }
            }
            public string Message
            {
                get
                {
                    return GetStatus(status);
                }
            }
        }
        public class PaymentStatus
        {
            public int status { get; set; }
            public string track_id { get; set; }
            public string id { get; set; }
            public string order_id { get; set; }
            public decimal amount { get; set; }
            public Wage wage { get; set; }
            public double date { get; set; }
            public Payer payer { get; set; }
            public PaymentDetail payment { get; set; }
            public VerifyDate verify { get; set; }
            public Settlement settlement { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
            public bool IsOK
            {
                get
                {
                    return status == 100;
                }
            }
            public string Message
            {
                get
                {
                    return GetStatus(status);
                }
            }
        }
        public class PaymentDetail
        {
            public string track_id { get; set; }
            public decimal amount { get; set; }
            public string card_no { get; set; }
            public string hashed_card_no { get; set; }
            public double date { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
        }
        public class VerifyDate
        {
            public double date { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
        }
        public class Wage
        {
            public string by { get; set; }
            public string type { get; set; }
            public decimal amount { get; set; }
        }
        public class Payer
        {
            public string name { get; set; }
            public string phone { get; set; }
            public string mail { get; set; }
            public string desc { get; set; }
        }
        public class Settlement
        {
            public string track_id { get; set; }
            public decimal amount { get; set; }
            public double date { get; set; }

            public DateTime Date
            {
                get
                {
                    return UnixTimeStampToDateTime(date);
                }
            }
        }

        private HttpClient client = null;
        public Payment()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("https://api.idpay.ir/v1.1/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Add("X-API-KEY", "6a7f99eb-7c20-4412-a972-6dfb7cd253a4");
            client.DefaultRequestHeaders.Add("X-SANDBOX", "1");
        }

        public async Task<object> RequestPayment(Request obj)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress + "payment"),
                    Method = HttpMethod.Post
                };

                // ذخیره اطلاعات پرداخت در دیتابیس

                request.Content = new StringContent(JsonConvert.SerializeObject(obj),
                    Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RequestRespons_Success>(_data);
                }
                else
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RequestRespons_Fail>(_data);
                }
            }
            catch(Exception ex)
            {
                return new RequestRespons_Fail
                {
                    error_code = 0,
                    error_message = ex.Message
                };
            }
        }

        public async Task<object> VerifyPayment(ResultPayment obj)
        {
            try
            {
                // ذخیره نتیجه پرداخت در دیتابیس

                if (!obj.IsOK)
                    return null;

                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress + "payment/verify"),
                    Method = HttpMethod.Post
                };                

                request.Content = new StringContent(JsonConvert.SerializeObject(
                    new { id = obj.id, order_id = obj.order_id }
                    ),
                    Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PaymentInfo>(_data);
                }
                else
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RequestRespons_Fail>(_data);
                }
            }
            catch (Exception ex)
            {
                return new RequestRespons_Fail
                {
                    error_code = 0,
                    error_message = ex.Message
                };
            }
        }

        public async Task<object> InquiryPayment(string id, string orderId)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(client.BaseAddress + "payment/inquiry"),
                    Method = HttpMethod.Post
                };

                request.Content = new StringContent(JsonConvert.SerializeObject(
                    new { id = id, order_id = orderId }
                    ),
                    Encoding.UTF8, "application/json");

                HttpResponseMessage response = null;
                response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PaymentStatus>(_data);
                }
                else
                {
                    string _data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RequestRespons_Fail>(_data);
                }
            }
            catch (Exception ex)
            {
                return new RequestRespons_Fail
                {
                    error_code = 0,
                    error_message = ex.Message
                };
            }
        }
    }
}