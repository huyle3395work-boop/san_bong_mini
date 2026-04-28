using Demo.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity.Core.Objects;
using Demo.ViewModels;
namespace QL_SAN_THE_THAO.Controllers
{
    public class SanController : Controller
    {
        private QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();

        public ActionResult Index()
        {
            var model = new San();
            model.SanNoiBat = db.SanTheThao
                                .Include(s => s.KhuTheThao) 
                                .Where(s => s.TrangThai != "Bảo trì")
                                .OrderByDescending(s => s.IsNoiBat)
                                .ThenByDescending(s => s.MaSan)
                                .Take(8)
                                .ToList();
            model.TranDauMoi = db.TranDau
                                 .Include(t => t.SanTheThao)
                                 .Include(t => t.TaiKhoanUser)
                                 .Include(t => t.TaiKhoanUser.HoSoNguoiChoi) 
                                 .Include(t => t.KhungGio)    
                                 .Where(t => t.TrangThai == "Công khai")
                                 .OrderBy(t => t.Ngay)
                                 .Take(8)
                                 .ToList();
            model.TinTucMoi = db.TinTuc
                                .OrderByDescending(t => t.NgayDang)
                                .Take(6)
                                .ToList();

            return View(model);
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