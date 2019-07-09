using IDPay.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace IDPay.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Payment()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Payment(string Amount)
        {
            string Link = "", Message = "";
            try
            {
                var payment = new Payment();
                var obj = new Payment.Request(null);
                obj.amount = decimal.Parse(Amount);
                obj.name = "مدیر";
                obj.phone = "09124010603";
                obj.mail = "";
                obj.desc = "تست درگاه پرداخت";

                var res = await payment.RequestPayment(obj);
                if (res is Payment.RequestRespons_Success)
                    Link = ((Payment.RequestRespons_Success)res).link;
                else
                    Message = ((Payment.RequestRespons_Fail)res).error_message;
            }
            catch(Exception ex)
            {
                
            }
            return Json(new { PaymentUrl = Link, Message = Message }, JsonRequestBehavior.AllowGet);
        }

        
        public async Task<ActionResult> AfterPayment()
        {
            string Message = "";
            try
            {
                var payment = new Payment();
                var obj = new Payment.ResultPayment();
                obj.status = int.Parse(Request.Params["status"]);
                obj.track_id = Request.Params["track_id"];
                obj.id = Request.Params["id"];
                obj.order_id = Request.Params["order_id"];
                obj.amount = decimal.Parse(Request.Params["amount"]);
                obj.card_no = Request.Params["card_no"];
                obj.hashed_card_no = Request.Params["hashed_card_no"];
                obj.date = double.Parse(Request.Params["date"]);
                
                if (!obj.IsOK)
                {
                    ViewBag.ID = obj.id;
                    ViewBag.OrderID = obj.order_id;
                    Message = obj.Message;
                }
                else
                {
                    // تایید تراکنش
                    var res = await payment.VerifyPayment(obj);
                    if (res is Payment.PaymentInfo)
                    {
                        var tmp = (Payment.PaymentInfo)res;
                        Message = tmp.Message;
                        ViewBag.ID = tmp.id;
                        ViewBag.OrderID = tmp.order_id;
                    }
                    else
                        Message = ((Payment.RequestRespons_Fail)res).error_message;
                }
            }
            catch(Exception ex)
            {
                Message = ex.Message;
            }
            ViewBag.Message = Message;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Inquiry(string ID, string OrderID)
        {
            string Message = "";
            try
            {
                var payment = new Payment();
                var res = await payment.InquiryPayment(ID, OrderID);
                if (res is Payment.PaymentStatus)
                {
                    var tmp = (Payment.PaymentStatus)res;
                    Message = tmp.Message;
                }
                else
                    Message = ((Payment.RequestRespons_Fail)res).error_message;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

            return Json(new { Message = Message }, JsonRequestBehavior.AllowGet);
        }
    }
}