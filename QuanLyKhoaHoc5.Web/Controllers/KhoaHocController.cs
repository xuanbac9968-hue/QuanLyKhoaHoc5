using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Helpers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;

namespace QuanLyKhoaHoc5.Web.Controllers;

public class KhoaHocController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public KhoaHocController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    // Public - allow anonymous
    public async Task<IActionResult> Index(KhoaHocFilterViewModel filter)
    {
        var query = _db.KhoaHocs
            .Include(k => k.LopHocs).ThenInclude(l => l.DangKys)
            .AsQueryable();

        // Non-admin only sees open courses
        if (!User.IsInRole("Admin"))
            query = query.Where(k => k.TrangThai == "DangMo");

        filter.Search  = filter.Search?.Trim();
        filter.NgonNgu = filter.NgonNgu?.Trim();
        filter.TrinhDo = filter.TrinhDo?.Trim();

        if (!string.IsNullOrEmpty(filter.NgonNgu))
            query = query.Where(k => k.NgonNgu == filter.NgonNgu);
        if (!string.IsNullOrEmpty(filter.TrinhDo))
            query = query.Where(k => k.TrinhDo == filter.TrinhDo);
        if (!string.IsNullOrEmpty(filter.TrangThai) && User.IsInRole("Admin"))
            query = query.Where(k => k.TrangThai == filter.TrangThai);
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(k => k.TenKhoaHoc.ToLower().Contains(s) || (k.MoTa != null && k.MoTa.ToLower().Contains(s)));
        }

        ViewBag.NgonNguList = await _db.KhoaHocs.Select(k => k.NgonNgu).Distinct().OrderBy(n => n).ToListAsync();

        filter.TotalItems = await query.CountAsync();
        filter.Items = await query
            .OrderByDescending(k => k.NgayTao)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(k => new KhoaHocListViewModel
            {
                Id = k.Id, TenKhoaHoc = k.TenKhoaHoc, NgonNgu = k.NgonNgu, TrinhDo = k.TrinhDo,
                HocPhi = k.HocPhi, SoChoToiDa = k.SoChoToiDa, ThoiLuong = k.ThoiLuong,
                TrangThai = k.TrangThai, AnhBia = k.AnhBia, SoLopHoc = k.LopHocs.Count,
                NgayTao = k.NgayTao,
                SoHocVien = k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet")
            }).ToListAsync();

        return View(filter);
    }

    // Public - allow anonymous for viewing course details and schedule
    public async Task<IActionResult> Details(int id)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var k = await _db.KhoaHocs
            .Include(x => x.LopHocs).ThenInclude(l => l.GiangVien)
            .Include(x => x.LopHocs).ThenInclude(l => l.DangKys)
            .Include(x => x.LopHocs).ThenInclude(l => l.LichHocs)
            .Include(x => x.PhanCongs.Where(p => p.IsActive)).ThenInclude(p => p.GiangVien)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (k == null) return NotFound();

        // Check if logged-in student is already enrolled
        bool daDangKy = false;
        bool coChoTrong = false;
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("HocVien"))
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            daDangKy = await _db.DangKyKhoaHocs
                .AnyAsync(d => d.HocVienId == userId && d.LopHoc.KhoaHocId == id && d.TrangThai != "TuChoi");
        }

        var soHvDaDk = k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet");
        coChoTrong = k.SoChoToiDa > soHvDaDk;

        ViewBag.DaDangKy = daDangKy;
        ViewBag.CoChoTrong = coChoTrong;
        ViewBag.SoHvDaDk = soHvDaDk;

        // Upcoming sessions (next 60 days) across all lop hocs
        var lichHocList = k.LopHocs
            .SelectMany(l => l.LichHocs.Select(lh => LichHocHelper.ToViewModel(lh)))
            .Where(b => b.NgayHoc >= today)
            .OrderBy(b => b.NgayHoc).ThenBy(b => b.GioBatDau)
            .Take(20)
            .ToList();
        ViewBag.LichHocList = lichHocList;

        // Get the open lop for registration
        var lopMo = k.LopHocs
            .Where(l => l.TrangThai is "DangTuyenSinh" or "DangHoc")
            .OrderByDescending(l => l.NgayKhaiGiang)
            .FirstOrDefault();
        ViewBag.LopMoId = lopMo?.Id;

        return View(k);
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public IActionResult Create() => View(new KhoaHocCreateEditViewModel());

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KhoaHocCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var kh = new KhoaHoc
        {
            TenKhoaHoc = vm.TenKhoaHoc, MoTa = vm.MoTa, NgonNgu = vm.NgonNgu, TrinhDo = vm.TrinhDo,
            HocPhi = vm.HocPhi, SoChoToiDa = vm.SoChoToiDa, ThoiLuong = vm.ThoiLuong,
            SoBuoiMoiTuan = vm.SoBuoiMoiTuan, ThoiGianMoiBuoi = vm.ThoiGianMoiBuoi,
            NoiDungChuongTrinh = vm.NoiDungChuongTrinh, TrangThai = vm.TrangThai,
            AnhBia = await SaveAnhAsync(vm.AnhBiaFile)
        };
        _db.KhoaHocs.Add(kh);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm khóa học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var k = await _db.KhoaHocs.FindAsync(id);
        if (k == null) return NotFound();
        return View(new KhoaHocCreateEditViewModel
        {
            Id = k.Id, TenKhoaHoc = k.TenKhoaHoc, MoTa = k.MoTa, NgonNgu = k.NgonNgu, TrinhDo = k.TrinhDo,
            HocPhi = k.HocPhi, SoChoToiDa = k.SoChoToiDa, ThoiLuong = k.ThoiLuong,
            SoBuoiMoiTuan = k.SoBuoiMoiTuan, ThoiGianMoiBuoi = k.ThoiGianMoiBuoi,
            NoiDungChuongTrinh = k.NoiDungChuongTrinh, TrangThai = k.TrangThai, AnhBiaHienTai = k.AnhBia
        });
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KhoaHocCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var k = await _db.KhoaHocs.FindAsync(id);
        if (k == null) return NotFound();

        k.TenKhoaHoc = vm.TenKhoaHoc; k.MoTa = vm.MoTa; k.NgonNgu = vm.NgonNgu; k.TrinhDo = vm.TrinhDo;
        k.HocPhi = vm.HocPhi; k.SoChoToiDa = vm.SoChoToiDa; k.ThoiLuong = vm.ThoiLuong;
        k.SoBuoiMoiTuan = vm.SoBuoiMoiTuan; k.ThoiGianMoiBuoi = vm.ThoiGianMoiBuoi;
        k.NoiDungChuongTrinh = vm.NoiDungChuongTrinh; k.TrangThai = vm.TrangThai;
        k.NgayCapNhat = DateTime.Now;
        if (vm.AnhBiaFile != null) k.AnhBia = await SaveAnhAsync(vm.AnhBiaFile);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật khóa học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var k = await _db.KhoaHocs.Include(x => x.LopHocs).FirstOrDefaultAsync(x => x.Id == id);
        if (k == null) return NotFound();
        if (k.LopHocs.Any()) { TempData["Error"] = "Không thể xóa khóa học đã có lớp học!"; return RedirectToAction(nameof(Index)); }
        _db.KhoaHocs.Remove(k);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Xóa khóa học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var k = await _db.KhoaHocs.FindAsync(id);
        if (k == null) return Json(new { success = false });

        // Circular rotation: DangMo → DaDong → TamDung → DangMo
        k.TrangThai = k.TrangThai switch
        {
            "DangMo"  => "DaDong",
            "DaDong"  => "TamDung",
            "TamDung" => "DangMo",
            _         => "DangMo"
        };
        k.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();

        var displayText = k.TrangThai switch
        {
            "DangMo"  => "Đang mở",
            "DaDong"  => "Đã đóng",
            "TamDung" => "Tạm dừng",
            _         => k.TrangThai
        };

        return Json(new { success = true, newStatus = k.TrangThai, displayText });
    }

    private async Task<string?> SaveAnhAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not ".jpg" and not ".jpeg" and not ".png") return null;
        if (file.Length > 5 * 1024 * 1024) return null;
        var folder = Path.Combine(_env.WebRootPath, "uploads", "khoa-hoc");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(folder, fileName);
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/khoa-hoc/{fileName}";
    }
}
