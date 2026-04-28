using Demo.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using System.Collections.Generic;

namespace QL_SAN_THE_THAO.Controllers
{
    public class GamesIndexViewModel
    {
        public List<TranDau> TranDauMoi { get; set; }
        public List<LoaiTheThao> LoaiTheThao { get; set; }
        public List<TranDau> Collections { get; set; }

        public string SortBy { get; set; }
        public string TimeFilter { get; set; }
        public string SkillFilter { get; set; }
        public string SportFilter { get; set; }
        public string DateFilter { get; set; }
    }

    public class GamesController : Controller
    {
        private readonly QL_SAN_THE_THAOEntities db = new QL_SAN_THE_THAOEntities();

        public async Task<ActionResult> Index(string sortBy, string timeFilter, string skillFilter, string sportFilter, string dateFilter)
        {
            IQueryable<TranDau> gamesQuery = db.TranDau
                .Include(t => t.SanTheThao)
                .Include(t => t.TaiKhoanUser.HoSoNguoiChoi)
                .Include(t => t.KhungGio)
                .Include(t => t.LoaiTheThao)
                .Where(t => t.TrangThai == "Công khai")
                .OrderBy(t => t.Ngay);

            var tranDauList = await gamesQuery.Take(20).ToListAsync();

            ViewBag.LoaiTheThao = await db.LoaiTheThao.Take(5).ToListAsync();

            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentTime = timeFilter;
            ViewBag.CurrentSkill = skillFilter;
            ViewBag.CurrentSport = sportFilter;
            ViewBag.CurrentDate = dateFilter;

            return View(tranDauList);
        }

        public ActionResult FilterSortPartial()
        {
            return PartialView("_FilterSortModalPartial");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}