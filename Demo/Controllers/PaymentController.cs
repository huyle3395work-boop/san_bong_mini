using Demo.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace QL_SAN_THE_THAO.Controllers
{
    public class PaymentController : Controller
    {
        private QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();

        public ActionResult Index(string maDat)
        {
            if (string.IsNullOrEmpty(maDat)) return RedirectToAction("Index", "San");

            var booking = db.DatSan.FirstOrDefault(d => d.MaDat == maDat);
            if (booking == null) return HttpNotFound();

            return View(booking);
        }

        [HttpPost]
        public ActionResult PayByCash(string maDat)
        {
            var booking = db.DatSan.Find(maDat);
            if (booking == null) return HttpNotFound();

            booking.TrangThai = "Chờ xác nhận";
            db.SaveChanges();

            TempData["Message"] = "Vui lòng thanh toán tiền mặt tại quầy và đợi nhân viên xác nhận.";
            return RedirectToAction("Status", new { maDat = maDat });
        }

        [HttpPost]
        public async Task<ActionResult> PayByQR(string maDat)
        {
            var booking = db.DatSan.FirstOrDefault(d => d.MaDat == maDat);
            if (booking == null) return HttpNotFound();

            decimal total = booking.TongTien.GetValueOrDefault();

            if (total < 2000m)
            {
                return Content("Lỗi: Số tiền thanh toán phải từ 2,000 VNĐ trở lên.");
            }

            long orderCode = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));

            var orderData = new
            {
                orderCode = orderCode,
                amount = (int)total,
                description = "THANHTOAN " + maDat,
                cancelUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payment/Status?maDat=" + maDat,
                returnUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/Payment/Success?maDat=" + maDat
            };

            string checksumKey = ConfigurationManager.AppSettings["PAYOS_CHECKSUM_KEY"];
            string signature = CreateSignature(checksumKey, orderData);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-client-id", ConfigurationManager.AppSettings["PAYOS_CLIENT_ID"]);
                client.DefaultRequestHeaders.Add("x-api-key", ConfigurationManager.AppSettings["PAYOS_API_KEY"]);

                var body = new
                {
                    orderCode = orderData.orderCode,
                    amount = orderData.amount,
                    description = orderData.description,
                    cancelUrl = orderData.cancelUrl,
                    returnUrl = orderData.returnUrl,
                    signature = signature
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api-merchant.payos.vn/v2/payment-requests", content);
                var resultText = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(resultText);

                if (result.code == "00")//thanh cong
                {
                    return Redirect(result.data.checkoutUrl.ToString());
                }
                else
                {
                    return Content("Lỗi từ PayOS: " + result.desc + " (Mã lỗi: " + result.code + ")");
                }
            }
        }

        public ActionResult Status(string maDat)
        {
            var booking = db.DatSan.Find(maDat);
            return View(booking);
        }

        public ActionResult Success(string maDat)
        {
            var booking = db.DatSan.Find(maDat);
            if (booking == null) return HttpNotFound();

            if (booking.TrangThai != "Đã thanh toán")
            {
                booking.TrangThai = "Đã thanh toán";
                db.SaveChanges();
            }

            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng đã được xác nhận.";

            return View("Status", booking);
        }

        private string CreateSignature(string checksumKey, object data)
        {
            var properties = data.GetType().GetProperties()
                .Where(p => p.GetValue(data) != null && p.Name != "signature")
                .OrderBy(p => p.Name)
                .Select(p => $"{p.Name}={p.GetValue(data)}");

            string rawData = string.Join("&", properties);

            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(checksumKey)))
            {
                byte[] hashPayload = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hashPayload).Replace("-", "").ToLower();
            }
        }
    }
}