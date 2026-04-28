using Demo.Models;
using Demo.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Net;

namespace QL_SAN_THE_THAO.Controllers
{
    public class VenuesController : Controller
    {
        private QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();


        public ActionResult Index(string search, string maLoai)
        {
            var query = db.SanTheThao
                            .Include(s => s.KhuTheThao)
                            .Include(s => s.LoaiTheThao)
                            .Where(s => s.TrangThai != "Bảo trì");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.TenSan.Contains(search));
            }

            if (!string.IsNullOrEmpty(maLoai) && maLoai != "All")
            {
                query = query.Where(s => s.MaLoai == maLoai);
            }

            var venues = query.OrderByDescending(s => s.IsNoiBat)
                            .ThenBy(s => s.TenSan)
                            .ToList();

            ViewBag.DanhSachLoai = db.LoaiTheThao.ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentMaLoai = maLoai;

            var model = new San
            {
                SanNoiBat = venues
            };

            ViewBag.Title = "Danh sách sân thể thao";
            return View(model);
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var san = db.SanTheThao
                        .Include(s => s.KhuTheThao)
                        .Include(s => s.LoaiTheThao)
                        .FirstOrDefault(s => s.MaSan == id);

            if (san == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/Venues/Details.cshtml", san);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}