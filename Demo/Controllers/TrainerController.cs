using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Demo.Models;
using System.Data.Entity;
using System.Net;

public class TrainerController : Controller
{
    private readonly QL_SAN_THE_THAOEntities _context = new QL_SAN_THE_THAOEntities();

    public async Task<ActionResult> Index(string searchTuKhoa, string mon,
                                          bool? isAcademy, string ageGroup, string service)
    {
        IQueryable<Trainer> trainersQuery = _context.Trainer.Include("LoaiTheThao").AsNoTracking();

        if (!string.IsNullOrEmpty(searchTuKhoa))
            trainersQuery = trainersQuery.Where(t => t.HoTen.Contains(searchTuKhoa) || t.KhuVuc.Contains(searchTuKhoa));

        if (!string.IsNullOrEmpty(mon))
            trainersQuery = trainersQuery.Where(t => t.LoaiTheThao.Any(l => l.TenLoai == mon));

        if (isAcademy.HasValue)
            trainersQuery = trainersQuery.Where(t => t.IsAcademy == isAcademy.Value);

        if (!string.IsNullOrEmpty(ageGroup))
            trainersQuery = trainersQuery.Where(t => t.AgeGroup == ageGroup);

        if (!string.IsNullOrEmpty(service))
            trainersQuery = trainersQuery.Where(t => t.Services.Contains(service));

        var viewModel = new Demo.ViewModels.TrainerSearchViewModel
        {
            SearchTuKhoa = searchTuKhoa,
            MonHienTai = mon,
            IsAcademy = isAcademy,
            AgeGroup = ageGroup,
            Service = service,
            Trainers = await trainersQuery.ToListAsync(),
            TatCaMonTheThao = await _context.LoaiTheThao.AsNoTracking().ToListAsync()
        };

        return View(viewModel);
    }


    public async Task<ActionResult> Details(int? id)
    {
        if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        var trainer = await _context.Trainer
            .Include(t => t.LoaiTheThao)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TrainerId == id);

        if (trainer == null) return HttpNotFound();

        ViewBag.AllSportsCategories = await _context.LoaiTheThao.AsNoTracking().ToListAsync();

        ViewBag.SimilarTrainers = await _context.Trainer
            .Where(t => t.TrainerId != id.Value)
            .OrderByDescending(t => t.SoSaoDanhGia).Take(5).Include(t => t.LoaiTheThao).AsNoTracking().ToListAsync();

        return View(trainer);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _context.Dispose();
        base.Dispose(disposing);
    }
}
