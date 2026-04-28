using Demo.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace QL_SAN_THE_THAO.Controllers
{
    public class AdminController : Controller
    {
        private readonly QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();

        public async Task<ActionResult> Index()
        {
            var today = DateTime.Today;
            var now = DateTime.Now;

            var doanhThuThang = await db.ThanhToan
                .Where(t => t.TrangThai == "Đã thanh toán"
                         && t.NgayThanhToan.HasValue
                         && t.NgayThanhToan.Value.Month == now.Month
                         && t.NgayThanhToan.Value.Year == now.Year)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0;

            var donChoDuyet = await db.DatSan
                .CountAsync(d => d.TrangThai == "Chờ xác nhận");

            var donHomNay = await db.DatSan
                .CountAsync(d => DbFunctions.TruncateTime(d.NgayDat) == today);

            var tongThanhVien = await db.TaiKhoanUser.CountAsync();

            var donMoiNhat = await db.DatSan
                .Include(d => d.SanTheThao)
                .Include(d => d.KhungGio)
                .Include(d => d.TaiKhoanUser)
                .OrderByDescending(d => d.NgayDat)
                .ThenByDescending(d => d.MaDat)
                .Take(5)
                .ToListAsync();

            var userIds = donMoiNhat.Select(x => x.MaUser).Distinct().ToList();

            var dictKhachHang = await db.KHACHHANG
                                  .Where(k => userIds.Contains(k.MaKH))
                                  .ToDictionaryAsync(k => k.MaKH, k => k.HoTen);

            ViewBag.TenKhachHang = dictKhachHang;


            ViewBag.DoanhThuThang = doanhThuThang;
            ViewBag.DonChoDuyet = donChoDuyet;
            ViewBag.DonHomNay = donHomNay;
            ViewBag.TongThanhVien = tongThanhVien;
            ViewBag.DonMoiNhat = donMoiNhat;

            return View();
        }



        public async Task<ActionResult> ManageVenues()
        {
            var sanTheThaoList = db.SanTheThao.Include(s => s.KhuTheThao).Include(s => s.LoaiTheThao);
            return View(await sanTheThaoList.ToListAsync());
        }

        public ActionResult CreateVenue()
        {
            ViewBag.MaKhu = new SelectList(db.KhuTheThao, "MaKhu", "TenKhu");
            ViewBag.MaLoai = new SelectList(db.LoaiTheThao, "MaLoai", "TenLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateVenue(
            [Bind(Include = "MaKhu,MaLoai,TenSan,GiaThueTheoGio,TrangThai,MoTa,HinhAnh,IsNoiBat")] SanTheThao sanTheThao,
    HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {   
                var maxId = await db.SanTheThao
                                    .OrderByDescending(s => s.MaSan)
                                    .Select(s => s.MaSan)
                                    .FirstOrDefaultAsync();
                int nextId = 1;
                if (maxId != null && maxId.StartsWith("S"))
                {
                    int.TryParse(maxId.Substring(1), out nextId);
                    nextId++;
                }
                sanTheThao.MaSan = "S" + nextId.ToString("D4");
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string filename = Path.GetFileName(uploadImage.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), filename);
                    uploadImage.SaveAs(path);
                    sanTheThao.HinhAnh = "/Content/images/" + filename;
                }
                else
                {
                    sanTheThao.HinhAnh = "/Content/images/default-san.png";
                }
                db.SanTheThao.Add(sanTheThao);
                await db.SaveChangesAsync();
                return RedirectToAction("ManageVenues");
            }

            ViewBag.MaKhu = new SelectList(db.KhuTheThao, "MaKhu", "TenKhu", sanTheThao.MaKhu);
            ViewBag.MaLoai = new SelectList(db.LoaiTheThao, "MaLoai", "TenLoai", sanTheThao.MaLoai);
            return View(sanTheThao);
        }

        public async Task<ActionResult> EditVenue(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanTheThao sanTheThao = await db.SanTheThao.FindAsync(id);
            if (sanTheThao == null) return HttpNotFound();

            ViewBag.MaKhu = new SelectList(db.KhuTheThao, "MaKhu", "TenKhu", sanTheThao.MaKhu);
            ViewBag.MaLoai = new SelectList(db.LoaiTheThao, "MaLoai", "TenLoai", sanTheThao.MaLoai);
            return View(sanTheThao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditVenue(
            [Bind(Include = "MaSan,MaKhu,MaLoai,TenSan,GiaThueTheoGio,TrangThai,MoTa,HinhAnh,IsNoiBat")] SanTheThao sanTheThao,
            HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string _FileName = System.IO.Path.GetFileName(uploadImage.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/images/"), _FileName);
                    uploadImage.SaveAs(path);
                    sanTheThao.HinhAnh = "/Content/images/" + _FileName;
                }
                else
                {
                }
                db.Entry(sanTheThao).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ManageVenues");
            }

            ViewBag.MaKhu = new SelectList(db.KhuTheThao, "MaKhu", "TenKhu", sanTheThao.MaKhu);
            ViewBag.MaLoai = new SelectList(db.LoaiTheThao, "MaLoai", "TenLoai", sanTheThao.MaLoai);
            return View(sanTheThao);
        }
        public async Task<ActionResult> DeleteVenue(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SanTheThao sanTheThao = await db.SanTheThao
                                            .Include(s => s.KhuTheThao)
                                            .Include(s => s.LoaiTheThao)
                                            .FirstOrDefaultAsync(s => s.MaSan == id);

            if (sanTheThao == null) return HttpNotFound();

            return View(sanTheThao);
        }

        [HttpPost, ActionName("DeleteVenue")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            SanTheThao sanTheThao = await db.SanTheThao.FindAsync(id);


            db.SanTheThao.Remove(sanTheThao);
            await db.SaveChangesAsync();
            return RedirectToAction("ManageVenues");
        }



        public ActionResult ManageBookings()
        {
            var list = db.DatSan
                         .Include("SanTheThao")
                         .Include("KhungGio")
                         .OrderByDescending(d => d.NgayDat)
                         .ToList();
            var userIds = list.Select(x => x.MaUser).Distinct().ToList();
            var dictKhachHang = db.KHACHHANG
                                  .Where(k => userIds.Contains(k.MaKH))
                                  .ToDictionary(k => k.MaKH, k => k.HoTen);
            ViewBag.TenKhachHang = dictKhachHang;

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmBooking(string id)
        {
            var booking = await db.DatSan.FindAsync(id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt sân.";
                return RedirectToAction("ManageBookings");
            }

            if (booking.TrangThai == "Chờ xác nhận")
            {
                booking.TrangThai = "Đã xác nhận";
                db.Entry(booking).State = EntityState.Modified;


                await db.SaveChangesAsync();
                TempData["Success"] = $"Đã xác nhận đơn hàng {id} thành công.";
            }
            else
            {
                TempData["Warning"] = $"Đơn hàng {id} không thể xác nhận (Trạng thái hiện tại: {booking.TrangThai}).";
            }

            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelBooking(string id)
        {
            var booking = await db.DatSan.FindAsync(id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt sân.";
                return RedirectToAction("ManageBookings");
            }

            if (booking.TrangThai != "Đã hủy" && booking.TrangThai != "Đã thanh toán")
            {
                booking.TrangThai = "Đã hủy";
                db.Entry(booking).State = EntityState.Modified;


                await db.SaveChangesAsync();
                TempData["Success"] = $"Đã hủy đơn hàng {id}.";
            }
            else
            {
                TempData["Warning"] = $"Đơn hàng {id} không thể hủy (Trạng thái hiện tại: {booking.TrangThai}).";
            }

            return RedirectToAction("ManageBookings");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsPaid(string id)
        {
            var booking = await db.DatSan.FindAsync(id);

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt sân.";
                return RedirectToAction("ManageBookings");
            }

            if (booking.TrangThai != "Đã thanh toán" && booking.TrangThai != "Đã hủy")
            {
                booking.TrangThai = "Đã thanh toán"; 
                db.Entry(booking).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật đơn {id} sang trạng thái ĐÃ THANH TOÁN.";
            }
            else
            {
                TempData["Warning"] = "Đơn hàng này không thể cập nhật thanh toán.";
            }

            return RedirectToAction("ManageBookings");
        }
        public ActionResult Details(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var booking = db.DatSan
                            .Include("SanTheThao")
                            .Include("KhungGio")
                            .Include("ChiTietDichVu")
                            .Include("ChiTietDichVu.DichVu")
                            .FirstOrDefault(d => d.MaDat == id);

            if (booking == null) return HttpNotFound();

            if (booking.MaUser != null)
            {
                var khach = db.KHACHHANG.Find(booking.MaUser);
                ViewBag.TenKhach = khach?.HoTen ?? "Khách vãng lai";
                ViewBag.SDT = khach?.SDT ?? "Không có";
                ViewBag.Email = khach?.Email ?? "Không có";
                ViewBag.AnhDaiDien = khach?.AnhDaiDien;
            }

            return View(booking);
        }
        public async Task<ActionResult> ManageUsers()
        {
            var khList = db.KHACHHANG
                           .OrderBy(k => k.HoTen)
                           .ToListAsync();

            return View(await khList);
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser([Bind(Include = "HoTen,Email,SDT,AnhDaiDien")] KHACHHANG khach)
        {
            if (ModelState.IsValid)
            {
                khach.NgayTao = DateTime.Now;
                db.KHACHHANG.Add(khach);
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã thêm khách hàng thành công!";
                return RedirectToAction("ManageUsers");
            }
            return View(khach);
        }
        public async Task<ActionResult> EditUser(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            KHACHHANG khach = await db.KHACHHANG.FindAsync(id);
            if (khach == null) return HttpNotFound();

            return View(khach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditUser([Bind(Include = "MaKH,HoTen,Email,SDT,AnhDaiDien,NgayTao")] KHACHHANG khach)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khach).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật thông tin thành công!";
                return RedirectToAction("ManageUsers");
            }
            return View(khach);
        }

        public async Task<ActionResult> DeleteUser(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            KHACHHANG khach = await db.KHACHHANG.FindAsync(id);
            if (khach == null) return HttpNotFound();
            return View(khach);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteUserConfirmed(int id)
        {
            KHACHHANG khach = await db.KHACHHANG.FindAsync(id);
            if (khach != null)
            {
                db.KHACHHANG.Remove(khach);
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa khách hàng thành công!";
            }
            return RedirectToAction("ManageUsers");
        }

        public async Task<ActionResult> ManageNews()
        {
            var newsList = db.TinTuc.OrderByDescending(t => t.NgayDang).ToListAsync();
            return View(await newsList);
        }

        public ActionResult CreateNews()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNews(
            [Bind(Include = "TieuDe,MoTaNgan,NoiDung,HinhAnh,TacGia")] TinTuc tinTuc)
        {
            if (ModelState.IsValid)
            {
                tinTuc.NgayDang = DateTime.Now;
                if (string.IsNullOrEmpty(tinTuc.TacGia))
                {
                    tinTuc.TacGia = "Admin";
                }

                db.TinTuc.Add(tinTuc);
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã thêm tin tức mới thành công!";
                return RedirectToAction("ManageNews");
            }
            return View(tinTuc);
        }

        public async Task<ActionResult> EditNews(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            TinTuc tinTuc = await db.TinTuc.FindAsync(id);
            if (tinTuc == null) return HttpNotFound();

            return View(tinTuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditNews(
            [Bind(Include = "MaTin,TieuDe,MoTaNgan,NoiDung,HinhAnh,NgayDang,TacGia")] TinTuc tinTuc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tinTuc).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật tin tức thành công!";
                return RedirectToAction("ManageNews");
            }
            return View(tinTuc);
        }

        public async Task<ActionResult> DeleteNews(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            TinTuc tinTuc = await db.TinTuc.FindAsync(id);
            if (tinTuc == null) return HttpNotFound();
            return View(tinTuc);
        }

        [HttpPost, ActionName("DeleteNews")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteNewsConfirmed(int id)
        {
            TinTuc tinTuc = await db.TinTuc.FindAsync(id);
            if (tinTuc != null)
            {
                db.TinTuc.Remove(tinTuc);
                await db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa tin tức thành công!";
            }
            return RedirectToAction("ManageNews");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "San");
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        public async Task<ActionResult> ManageServices()
        {
            var list = await db.DichVu.OrderBy(d => d.TenDichVu).ToListAsync();
            return View(list);
        }

        public async Task<ActionResult> CreateService()
        {
            var maxMaDV = await db.DichVu
                                  .OrderByDescending(d => d.MaDV)
                                  .Select(d => d.MaDV)
                                  .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(maxMaDV) && maxMaDV.StartsWith("DV"))
            {
                int.TryParse(maxMaDV.Substring(2), out nextNumber);
                nextNumber++;
            }

            var model = new DichVu
            {
                MaDV = "DV" + nextNumber.ToString("D3")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateService(DichVu dichVu)
        {
            if (ModelState.IsValid)
            {
                var maxMaDV = await db.DichVu
                                      .OrderByDescending(d => d.MaDV)
                                      .Select(d => d.MaDV)
                                      .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (!string.IsNullOrEmpty(maxMaDV) && maxMaDV.StartsWith("DV"))
                {
                    int.TryParse(maxMaDV.Substring(2), out nextNumber);
                    nextNumber++;
                }

                dichVu.MaDV = "DV" + nextNumber.ToString("D3");
                dichVu.SoLuongTon = 0;
                db.DichVu.Add(dichVu);
                await db.SaveChangesAsync();
                return RedirectToAction("ManageServices");
            }

            return View(dichVu);
        }


        public async Task<ActionResult> EditService(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var dv = await db.DichVu.FindAsync(id);
            if (dv == null) return HttpNotFound();


            return View(dv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditService(DichVu dichVu)
        {
            if (ModelState.IsValid)
            {
                var old = await db.DichVu.AsNoTracking()
                                         .FirstOrDefaultAsync(d => d.MaDV == dichVu.MaDV);

                if (old == null) return HttpNotFound();

                dichVu.SoLuongTon = old.SoLuongTon;

                db.Entry(dichVu).State = EntityState.Modified;
                await db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật dịch vụ thành công!";
                return RedirectToAction("ManageServices");
            }
            return View(dichVu);
        }


        public async Task<ActionResult> DeleteService(string id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var dv = await db.DichVu.FindAsync(id);
            if (dv == null) return HttpNotFound();

            return View(dv);
        }

        [HttpPost, ActionName("DeleteService")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteServiceConfirmed(string id)
        {
            var dv = await db.DichVu.FindAsync(id);
            if (dv == null) return HttpNotFound();

            bool daDung = await db.ChiTietDichVu.AnyAsync(c => c.MaDV == id);
            if (daDung)
            {
                TempData["Error"] = "Không thể xóa dịch vụ đã được sử dụng!";
                return RedirectToAction("ManageServices");
            }

            if (dv.SoLuongTon > 0)
            {
                TempData["Error"] = "Không thể xóa dịch vụ còn tồn kho!";
                return RedirectToAction("ManageServices");
            }

            db.DichVu.Remove(dv);
            await db.SaveChangesAsync();

            TempData["Success"] = "Đã xóa dịch vụ!";
            return RedirectToAction("ManageServices");
        }


        public async Task<ActionResult> ManageTrainers()
        {
            var list = await db.Trainer
                               .OrderBy(t => t.HoTen)
                               .ToListAsync();
            return View(list);
        }

        public ActionResult CreateTrainer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateTrainer(Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                db.Trainer.Add(trainer);
                await db.SaveChangesAsync();
                return RedirectToAction("ManageTrainers");
            }
            return View(trainer);
        }

        public async Task<ActionResult> EditTrainer(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var trainer = await db.Trainer.FindAsync(id);
            if (trainer == null) return HttpNotFound();

            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditTrainer(Trainer trainer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trainer).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("ManageTrainers");
            }
            return View(trainer);
        }

        public async Task<ActionResult> DeleteTrainer(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var trainer = await db.Trainer.FindAsync(id);
            if (trainer == null) return HttpNotFound();

            return View(trainer);
        }

        [HttpPost, ActionName("DeleteTrainer")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteTrainerConfirmed(int id)
        {
            var trainer = await db.Trainer.FindAsync(id);
            if (trainer != null)
            {
                db.Trainer.Remove(trainer);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("ManageTrainers");
        }

        public async Task<ActionResult> ManageImport()
        {
            var list = db.PhieuNhapDichVu
                         .OrderByDescending(p => p.NgayNhap)
                         .ToListAsync();
            return View(await list);
        }

        [HttpGet]
        public ActionResult CreateImport()
        {
            ViewBag.DichVuList = new SelectList(db.DichVu, "MaDV", "TenDichVu");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateImport(
     string NguoiNhap,
     string GhiChu,
     string[] MaDV,
     int[] SoLuongNhap
 )
        {
            if (MaDV == null || SoLuongNhap == null)
            {
                TempData["Error"] = "Chưa nhập dịch vụ!";
                return RedirectToAction("CreateImport");
            }

            var max = db.PhieuNhapDichVu
                        .OrderByDescending(p => p.MaPhieuNhap)
                        .Select(p => p.MaPhieuNhap)
                        .FirstOrDefault();

            int next = 1;
            if (!string.IsNullOrEmpty(max))
            {
                int.TryParse(max.Substring(2), out next);
                next++;
            }

            string maPN = "PN" + next.ToString("D3");

            var pn = new PhieuNhapDichVu
            {
                MaPhieuNhap = maPN,
                NgayNhap = DateTime.Now,
                NguoiNhap = string.IsNullOrEmpty(NguoiNhap) ? "Admin" : NguoiNhap,
                GhiChu = GhiChu
            };

            db.PhieuNhapDichVu.Add(pn);

            bool hasRow = false;
            var maxCT = db.ChiTietPhieuNhap
              .OrderByDescending(c => c.MaCTPN)
              .Select(c => c.MaCTPN)
              .FirstOrDefault();

            int nextCT = 1;
            if (!string.IsNullOrEmpty(maxCT))
            {
                int.TryParse(maxCT.Substring(2), out nextCT);
                nextCT++;
            }

            for (int i = 0; i < MaDV.Length; i++)
            {
                if (string.IsNullOrEmpty(MaDV[i]) || SoLuongNhap[i] <= 0)
                    continue;

                db.ChiTietPhieuNhap.Add(new ChiTietPhieuNhap
                {
                    MaCTPN = "CT" + nextCT.ToString("D3"), 
                    MaPhieuNhap = maPN,
                    MaDV = MaDV[i],
                    SoLuongNhap = SoLuongNhap[i]
                });

                nextCT++;

                hasRow = true;
            }

            if (!hasRow)
            {
                TempData["Error"] = "Chưa có dòng nhập hợp lệ!";
                return RedirectToAction("CreateImport");
            }

            await db.SaveChangesAsync();

            TempData["Success"] = "Nhập kho thành công!";
            return RedirectToAction("ManageImport");
        }
        public async Task<ActionResult> ImportDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var phieuNhap = await db.PhieuNhapDichVu
                                    .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);

            if (phieuNhap == null)
                return HttpNotFound();

            var chiTiet = await db.ChiTietPhieuNhap
                                  .Include(c => c.DichVu)
                                  .Where(c => c.MaPhieuNhap == id)
                                  .ToListAsync();

            ViewBag.PhieuNhap = phieuNhap;
            return View(chiTiet);
        }
        public ActionResult RevenueReport(DateTime? fromDate, DateTime? toDate, int? month, int? year)
        {
            var query = db.DatSan.AsQueryable()
                          .Where(d => d.TrangThai == "Đã thanh toán");

            if (fromDate.HasValue)
                query = query.Where(d => d.NgayDat >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.NgayDat <= toDate.Value);

            if (month.HasValue)
                query = query.Where(d => d.NgayDat.Month == month.Value);

            if (year.HasValue)
                query = query.Where(d => d.NgayDat.Year == year.Value);

            var rawData = query.Select(d => new
            {
                Ngay = d.NgayDat,
                TongTienDonHang = d.TongTien ?? 0,
                TienDichVu = d.ChiTietDichVu.Sum(ct => (decimal?)ct.SoLuong * ct.DichVu.DonGia) ?? 0
            }).ToList();

            decimal totalRevenue = rawData.Sum(x => x.TongTienDonHang);
            decimal serviceRevenue = rawData.Sum(x => x.TienDichVu);
            decimal fieldRevenue = totalRevenue - serviceRevenue;

            var chartData = rawData
                .GroupBy(x => x.Ngay)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.TongTienDonHang)
                })
                .OrderBy(x => x.Date)
                .ToList();

            if (!chartData.Any())
            {
                chartData.Add(new { Date = DateTime.Today, Total = 0m });
            }

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.FieldRevenue = fieldRevenue;
            ViewBag.ServiceRevenue = serviceRevenue;

            ViewBag.ChartLabels = chartData.Select(x => x.Date.ToString("dd/MM")).ToList();
            ViewBag.ChartValues = chartData.Select(x => x.Total).ToList();

            var listDatSan = query.OrderByDescending(d => d.NgayDat).ToList();

            var userIds = listDatSan
                            .Where(x => x.MaUser != null)
                            .Select(x => x.MaUser.Value)
                            .Distinct()
                            .ToList();

            var dictKhachHang = db.KHACHHANG
                                  .Where(k => userIds.Contains(k.MaKH))
                                  .ToDictionary(k => k.MaKH, k => k.HoTen);

            ViewBag.TenKhachHang = dictKhachHang;

            return View(listDatSan);
        }







    }
}