//qwezdrbqdahmvolp

using Demo.Models;
using Demo.ViewModels;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security; 

namespace Demo.Controllers
{
    public class AccountController : Controller
    {
        private readonly QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();
        [HttpPost]
        public JsonResult Register(string email, string fullName, string password)
        {
            try
            {
                if (db.KHACHHANG.Any(x => x.Email == email))
                    return Json(new { success = false, message = "Email này đã tồn tại!" });

                var kh = new KHACHHANG
                {
                    HoTen = fullName,
                    Email = email,
                    MatKhau = password,
                    NgayTao = DateTime.Now
                };

                db.KHACHHANG.Add(kh);
                db.SaveChanges();

                FormsAuthentication.SetAuthCookie(email, false);
                Session["MaUser"] = kh.MaKH;
                Session["UserEmail"] = email;
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi lưu Database: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public JsonResult Login(string username, string password, string returnUrl)
        {
            var admin = db.TaiKhoanUser.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhau == password);
            if (admin != null)
            {
                FormsAuthentication.SetAuthCookie(admin.TenDangNhap, false);
                Session["MaUser"] = admin.MaUser;
                Session["UserEmail"] = admin.Email;
                Session["VaiTro"] = admin.VaiTro;

                return Json(new { success = true, returnUrl = "/Admin/Dashboard" });
            }

            var khach = db.KHACHHANG.FirstOrDefault(k => k.Email == username && k.MatKhau == password);
            if (khach != null)
            {
                FormsAuthentication.SetAuthCookie(khach.Email, false);
                Session["MaUser"] = khach.MaKH;
                Session["UserEmail"] = khach.Email;
                Session["VaiTro"] = khach.VaiTro;

                return Json(new { success = true, returnUrl = returnUrl });
            }

            return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không đúng!" });
        }

        [HttpPost]
        public JsonResult SendOTP(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Vui lòng nhập email!" });

                var otp = new Random().Next(100000, 999999).ToString();
                Session["OTP_Code"] = otp;
                Session["OTP_Email"] = email;

                var from = "playootp3395@gmail.com";
                var pass = "qwezdrbqdahmvolp";
                var msg = new MailMessage(from, email, "Mã OTP xác thực đăng nhập – PLAYO VN",
                    $"Mã OTP đăng nhập của bạn là: {otp}\nVui lòng không chia sẻ mã này cho bất kỳ ai.");

                var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(from, pass),
                    EnableSsl = true
                };
                smtp.Send(msg);

                return Json(new { success = true, message = "OTP đã được gửi đến email của bạn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi email: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult VerifyOTP(string otp, string email)
        {
            var otpSession = Session["OTP_Code"]?.ToString();
            var otpEmail = Session["OTP_Email"]?.ToString();

            if (otpSession != null && otp == otpSession && email == otpEmail)
            {
                Session.Remove("OTP_Code");
                var existingUser = db.KHACHHANG.FirstOrDefault(k => k.Email == email);
                if (existingUser != null)
                {
                    FormsAuthentication.SetAuthCookie(email, false);
                    Session["MaUser"] = existingUser.MaKH;
                    Session["UserEmail"] = email;
                    return Json(new { success = true, isNewUser = false });
                }
                return Json(new { success = true, isNewUser = true });
            }
            return Json(new { success = false, message = "Mã OTP không đúng!" });
        }

        [HttpPost]
        public JsonResult CreateProfile()
        {
            try
            {
                var email = Request.Form["Email"];
                var fullName = Request.Form["FullName"];
                var mobile = Request.Form["Mobile"];
                var password = Request.Form["Password"]; 
                if (db.KHACHHANG.Any(x => x.Email == email))
                    return Json(new { success = false, message = "Email đã tồn tại!" });

                var kh = new KHACHHANG
                {
                    HoTen = fullName,
                    SDT = mobile,
                    Email = email,
                    MatKhau = password,
                    NgayTao = DateTime.Now,
                    AnhDaiDien = "/Content/images/default-avatar.png"
                };

                db.KHACHHANG.Add(kh);
                db.SaveChanges();
                FormsAuthentication.SetAuthCookie(email, false);
                Session["MaUser"] = kh.MaKH;
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
        public ActionResult Profile()
        {
            var email = User.Identity.Name;

            var kh = db.KHACHHANG.FirstOrDefault(x => x.Email == email);

            if (kh == null) return HttpNotFound();

            return View(kh);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        private int? GetCurrentUserId()
        {
            if (Session["MaUser"] != null) return (int)Session["MaUser"];

            var email = User.Identity.Name;
            var user = db.KHACHHANG.FirstOrDefault(k => k.Email == email);
            if (user != null) Session["MaUser"] = user.MaKH;
            return user?.MaKH;
        }
        public ActionResult EditProfile()
        {
            var email = User.Identity.Name;
            var kh = db.KHACHHANG.FirstOrDefault(x => x.Email == email);
            if (kh == null) return HttpNotFound();
            return View(kh);
        }
        [HttpPost]
        public JsonResult UpdateProfile(string HoTen, string SDT, HttpPostedFileBase Photo)
        {
            try
            {
                var email = User.Identity.Name;
                var kh = db.KHACHHANG.FirstOrDefault(x => x.Email == email);
                kh.HoTen = HoTen;
                kh.SDT = SDT;

                if (Photo != null && Photo.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(Photo.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                    Photo.SaveAs(path);
                    kh.AnhDaiDien = "/Content/images/" + fileName;
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
        public ActionResult Feedback()
        {
            var email = User.Identity.Name;
            var kh = db.KHACHHANG.FirstOrDefault(x => x.Email == email);
            return View(kh);
        }
        [HttpPost]
        public JsonResult SubmitGeneralFeedback(string noiDung)
        {
            try
            {
                var email = User.Identity.Name;
                var user = db.KHACHHANG.FirstOrDefault(k => k.Email == email);
                var dg = new DanhGia
                {
                    MaDanhGia = "FB" + DateTime.Now.Ticks.ToString().Substring(10),
                    MaUser = user.MaKH,
                    NoiDung = noiDung,
                    NgayDanhGia = DateTime.Now,
                    Diem = 5,
                    MaSan = "SYSTEM"
                };
                db.DanhGia.Add(dg);
                db.SaveChanges();
                return Json(new { success = true, message = "Cảm ơn bạn đã góp ý!" });
            }
            catch { return Json(new { success = false }); }
        }
    }
}