using Demo.Models;
using Demo.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
namespace QL_SAN_THE_THAO.Controllers
{

    public class BookingsController : Controller
    {
        private QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();
        public ActionResult Create(string sanId)
        {
            if (string.IsNullOrEmpty(sanId)) return RedirectToAction("Index", "Venues");

            var san = db.SanTheThao.Find(sanId);
            if (san == null) return HttpNotFound();

            var KhungGioList = db.KhungGio.AsEnumerable().Select(k => new {
                MaKhung = k.MaKhung,
                HienThi = string.Format("{0:hh\\:mm} - {1:hh\\:mm}", k.GioBatDau, k.GioKetThuc)
            }).ToList();

            ViewBag.MaKhungList = new SelectList(KhungGioList, "MaKhung", "HienThi");
            ViewBag.TenSan = san.TenSan;
            ViewBag.GiaThue = san.GiaThueTheoGio;

            ViewBag.DichVuList = db.DichVu
                                    .Where(d => d.SoLuongTon > 0)
                                    .ToList();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DatSan booking, string[] MaDV, int[] SoLuong, string TenKhachHang, string SoDienThoai)
        {
            var san = db.SanTheThao.Include("LoaiTheThao").FirstOrDefault(s => s.MaSan == booking.MaSan);


            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var email = User.Identity.Name;
                    var user = db.KHACHHANG.FirstOrDefault(k => k.Email == email);

                    if (user != null) booking.MaUser = user.MaKH;
                }
                else
                {
                    var existingUser = db.TaiKhoanUser.FirstOrDefault(u => u.SoDienThoai == SoDienThoai);

                    if (existingUser != null)
                    {
                        booking.MaUser = existingUser.MaUser;
                    }
                    else
                    {
                        int maxId = 0;
                        if (db.TaiKhoanUser.Any()) maxId = db.TaiKhoanUser.Max(u => u.MaUser);
                        int newUserInfoId = maxId + 1;

                        var newUser = new TaiKhoanUser
                        {
                            MaUser = newUserInfoId,
                            TenDangNhap = SoDienThoai,
                            MatKhau = "123456",
                            Email = SoDienThoai + "_guest@playo.vn",
                            SoDienThoai = SoDienThoai,
                            TrangThai = "Hoạt động",
                            VaiTro = "KhachHang"
                        };
                        db.TaiKhoanUser.Add(newUser);

                        var newProfile = new HoSoNguoiChoi
                        {
                            MaUser = newUserInfoId,
                            HoTen = TenKhachHang,
                            NgaySinh = DateTime.Now,
                            GioiTinh = "Nam",
                            DiemUyTin = 0,
                            GioiThieu = "Khách vãng lai"
                        };
                        db.HoSoNguoiChoi.Add(newProfile);

                        db.SaveChanges();

                        booking.MaUser = newUserInfoId;
                    }
                }

                string lastId = db.DatSan.OrderByDescending(d => d.MaDat).Select(d => d.MaDat).FirstOrDefault();
                int nextId = lastId != null && lastId.Length > 1 ? int.Parse(lastId.Substring(1)) + 1 : 1;
                booking.MaDat = "D" + nextId.ToString("D4");

                booking.TrangThai = "Chờ xác nhận";

                decimal tongTienDichVu = 0;
                if (MaDV != null && SoLuong != null)
                {
                    for (int i = 0; i < MaDV.Length; i++)
                    {
                        var dv = db.DichVu.Find(MaDV[i]);
                        if (dv != null && SoLuong[i] > 0) tongTienDichVu += (dv.DonGia ?? 0) * SoLuong[i];
                    }
                }
                booking.TongTien = (san?.GiaThueTheoGio ?? 0) + tongTienDichVu;

                // 3. Lưu Đơn DatSan
                db.DatSan.Add(booking);
                db.SaveChanges();

                // 4. Lưu Chi Tiết Dịch Vụ (Copy lại đoạn cũ của bạn)
                if (MaDV != null && SoLuong != null)
                {
                    string lastCT = db.ChiTietDichVu.OrderByDescending(c => c.MaCTDV).Select(c => c.MaCTDV).FirstOrDefault();
                    int nextCTNum = 1;
                    if (lastCT != null && lastCT.Length > 1) { int.TryParse(lastCT.Substring(1), out nextCTNum); nextCTNum++; }

                    for (int i = 0; i < MaDV.Length; i++)
                    {
                        if (SoLuong[i] > 0)
                        {
                            var ct = new ChiTietDichVu
                            {
                                MaCTDV = "C" + nextCTNum.ToString("D4"),
                                MaDat = booking.MaDat,
                                MaDV = MaDV[i],
                                SoLuong = SoLuong[i]
                            };
                            db.ChiTietDichVu.Add(ct);
                            nextCTNum++;
                        }
                    }
                    db.SaveChanges();
                }

                return RedirectToAction("Index", "Payment", new { maDat = booking.MaDat });
            }

            // ... (GIỮ NGUYÊN ĐOẠN NẠP LẠI VIEW KHI CÓ LỖI) ...
            var queryKG = db.KhungGio.OrderBy(k => k.GioBatDau).AsQueryable();
            bool isFootball = (san != null && san.TenSan != null && san.TenSan.ToLower().Contains("bóng"))
                           || (san != null && san.LoaiTheThao != null && san.LoaiTheThao.TenLoai.ToLower().Contains("bóng"));
            if (isFootball) queryKG = queryKG.Where(k => k.MaKhung.Length == 4);
            else queryKG = queryKG.Where(k => k.MaKhung.Length == 5);

            if (booking.NgayDat.Date == DateTime.Now.Date)
            {
                TimeSpan timeNow = DateTime.Now.TimeOfDay;
                queryKG = queryKG.Where(k => k.GioBatDau > timeNow);
            }

            ViewBag.MaKhungList = new SelectList(queryKG.ToList().Select(x => new
            {
                MaKhung = x.MaKhung,
                ThoiGian = x.GioBatDau.ToString(@"hh\:mm") + " - " + x.GioKetThuc.ToString(@"hh\:mm")
            }), "MaKhung", "ThoiGian", booking.MaKhung);
            ViewBag.DichVuList = db.DichVu.Where(d => d.SoLuongTon > 0).ToList();
            if (san != null) { ViewBag.GiaThue = san.GiaThueTheoGio; ViewBag.TenSan = san.TenSan; }

            return View(booking);
        }

        public ActionResult MyBookings()
        {
            if (Session["MaUser"] == null) return RedirectToAction("Login", "Account");

            int userId = (int)Session["MaUser"];

            var allBookings = db.DatSan
                                .Include(b => b.KhungGio) 
                                .Include(b => b.SanTheThao) 
                                .Where(b => b.MaUser == userId)
                                .OrderByDescending(b => b.NgayDat)
                                .ToList();

            var viewModel = new MyBookingsViewModel
            {
                UpcomingBookings = allBookings.Where(b => b.NgayDat >= DateTime.Today).ToList(),
                PastBookings = allBookings.Where(b => b.NgayDat < DateTime.Today).ToList(),
                TotalBookingsCount = allBookings.Count
            };

            var user = db.KHACHHANG.Find(userId);
            ViewBag.AvatarUrl = user?.AnhDaiDien;
            ViewBag.HoTen = user?.HoTen;

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult CancelBooking(string maDat)
        {
            try
            {
                var booking = db.DatSan
                                .Include(d => d.KhungGio) 
                                .Include(d => d.SanTheThao)
                                .FirstOrDefault(d => d.MaDat == maDat);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Đơn đặt sân không tồn tại!" });
                }

                if (Session["MaUser"] == null || booking.MaUser != (int)Session["MaUser"])
                {
                    return Json(new { success = false, message = "Bạn không có quyền hủy đơn này!" });
                }

                if (booking.TrangThai == "Đã hủy")
                {
                    return Json(new { success = false, message = "Đơn này đã được hủy trước đó." });
                }

                DateTime gioDa = booking.NgayDat.Add(booking.KhungGio.GioBatDau);
                if ((gioDa - DateTime.Now).TotalHours < 2)
                {
                    return Json(new { success = false, message = "Chỉ được hủy sân trước giờ đá ít nhất 2 tiếng!" });
                }

                booking.TrangThai = "Đã hủy";


                db.SaveChanges();

                return Json(new { success = true, message = "Hủy đơn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}