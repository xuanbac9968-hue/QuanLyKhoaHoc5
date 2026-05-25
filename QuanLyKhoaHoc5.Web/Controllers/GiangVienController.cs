using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

public class GiangVienController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public GiangVienController(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

    [AuthorizeRole("GiangVien")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var gv = await _db.GiangViens.Include(g => g.NguoiDung).FirstOrDefaultAsync(g => g.Id == userId);
        if (gv == null) return NotFound();

        var lopList = await _db.LopHocs
            .Include(l => l.KhoaHoc).Include(l => l.DangKys)
            .Where(l => l.GiangVienId == userId && l.TrangThai != "DaKetThuc")
            .Select(l => new LopHocListViewModel
            {
                Id = l.Id, TenLop = l.TenLop, TenKhoaHoc = l.KhoaHoc.TenKhoaHoc, NgonNgu = l.KhoaHoc.NgonNgu,
                NgayKhaiGiang = l.NgayKhaiGiang, NgayKetThuc = l.NgayKetThuc, SiSoToiDa = l.SiSoToiDa,
                SiSoHienTai = l.DangKys.Count(d => d.TrangThai == "DaDuyet"),
                PhongHoc = l.PhongHoc, TrangThai = l.TrangThai
            }).ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var dayNum = (int)today.DayOfWeek;
        var startOfWeek = today.AddDays(-(dayNum == 0 ? 6 : dayNum - 1));
        var endOfWeek = startOfWeek.AddDays(6);

        var lichHocTuan = await _db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Where(l => l.LopHoc.GiangVienId == userId && l.NgayHoc >= startOfWeek && l.NgayHoc <= endOfWeek)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .ToListAsync();

        return View(new GiangVienDashboardViewModel
        {
            HoTen = gv.HoTen, MaGiangVien = gv.MaGiangVien,
            AnhDaiDien = gv.NguoiDung.AnhDaiDien, ChuyenMon = gv.ChuyenMon,
            LopDangDay = lopList,
            LichHomNay = lichHocTuan.Where(l => l.NgayHoc == today).Select(ToItem).ToList(),
            LichTuanNay = lichHocTuan.Select(ToItem).ToList(),
            SoHocVienTong = lopList.Sum(l => l.SiSoHienTai),
            SoThongBaoChuaDoc = await _db.ThongBaos.CountAsync(t => t.NguoiNhanId == userId && !t.DaDoc)
        });
    }

    [AuthorizeRole("GiangVien")]
    public async Task<IActionResult> LichDay()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var lichHocs = await _db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Where(l => l.LopHoc.GiangVienId == userId && l.NgayHoc >= today)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .ToListAsync();

        var lopList = await _db.LopHocs
            .Include(l => l.KhoaHoc).Include(l => l.DangKys)
            .Where(l => l.GiangVienId == userId && l.TrangThai != "DaKetThuc")
            .ToListAsync();

        ViewBag.LopList = lopList;
        return View(lichHocs.Select(ToItem).ToList());
    }

    // ============ Admin CRUD ============
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var gv = await _db.GiangViens
            .Include(g => g.NguoiDung).Include(g => g.LopHocs).ThenInclude(l => l.KhoaHoc)
            .Include(g => g.LopHocs).ThenInclude(l => l.DangKys)
            .Include(g => g.PhanCongs.Where(p => p.IsActive)).ThenInclude(p => p.KhoaHoc)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (gv == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var lichDay = await _db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Where(l => l.LopHoc.GiangVienId == id && l.NgayHoc >= today)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .Take(20)
            .ToListAsync();
        ViewBag.LichDay = lichDay.Select(ToItem).ToList();
        return View(gv);
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index(string? search, string? chuyenMon)
    {
        var query = _db.GiangViens.Include(g => g.NguoiDung).Include(g => g.LopHocs).AsQueryable();
        search = search?.Trim(); chuyenMon = chuyenMon?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(g => g.HoTen.ToLower().Contains(s) || g.MaGiangVien.ToLower().Contains(s) || g.NguoiDung.Email.ToLower().Contains(s));
        }
        if (!string.IsNullOrEmpty(chuyenMon)) query = query.Where(g => g.ChuyenMon == chuyenMon);

        ViewBag.Search = search; ViewBag.ChuyenMon = chuyenMon;
        return View(await query.OrderBy(g => g.MaGiangVien)
            .Select(g => new GiangVienListViewModel
            {
                Id = g.Id, MaGiangVien = g.MaGiangVien, HoTen = g.HoTen, Email = g.NguoiDung.Email,
                SoDienThoai = g.NguoiDung.SoDienThoai, ChuyenMon = g.ChuyenMon, BangCap = g.BangCap,
                KinhNghiem = g.KinhNghiem, IsActive = g.NguoiDung.IsActive,
                SoLopDangDay = g.LopHocs.Count(l => l.TrangThai == "DangHoc" || l.TrangThai == "DangTuyenSinh")
            }).ToListAsync());
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public IActionResult Create() => View(new GiangVienCreateEditViewModel());

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GiangVienCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _db.NguoiDungs.AnyAsync(u => u.Email == vm.Email))
        { ModelState.AddModelError("Email", "Email đã tồn tại"); return View(vm); }

        var defaultPw = _config["AppSettings:DefaultPassword"] ?? "Abc@12345";
        var nd = new NguoiDung { Email = vm.Email, HoTen = vm.HoTen, SoDienThoai = vm.SoDienThoai, MatKhauHash = BCrypt.Net.BCrypt.HashPassword(defaultPw), VaiTro = "GiangVien" };
        _db.NguoiDungs.Add(nd); await _db.SaveChangesAsync();

        var soThuTu = (await _db.GiangViens.CountAsync()) + 1;
        _db.GiangViens.Add(new GiangVien { Id = nd.Id, MaGiangVien = $"GV{soThuTu:D3}", HoTen = vm.HoTen, ChuyenMon = vm.ChuyenMon, BangCap = vm.BangCap, KinhNghiem = vm.KinhNghiem, MoTa = vm.MoTa });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Thêm giảng viên thành công! Mật khẩu mặc định: {defaultPw}";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var gv = await _db.GiangViens.Include(g => g.NguoiDung).FirstOrDefaultAsync(g => g.Id == id);
        if (gv == null) return NotFound();
        return View(new GiangVienCreateEditViewModel { Id = gv.Id, HoTen = gv.HoTen, Email = gv.NguoiDung.Email, SoDienThoai = gv.NguoiDung.SoDienThoai, ChuyenMon = gv.ChuyenMon, BangCap = gv.BangCap, KinhNghiem = gv.KinhNghiem, MoTa = gv.MoTa });
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GiangVienCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var gv = await _db.GiangViens.Include(g => g.NguoiDung).FirstOrDefaultAsync(g => g.Id == id);
        if (gv == null) return NotFound();
        gv.HoTen = vm.HoTen; gv.ChuyenMon = vm.ChuyenMon; gv.BangCap = vm.BangCap; gv.KinhNghiem = vm.KinhNghiem; gv.MoTa = vm.MoTa;
        gv.NguoiDung.HoTen = vm.HoTen; gv.NguoiDung.SoDienThoai = vm.SoDienThoai; gv.NguoiDung.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật giảng viên thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var gv = await _db.GiangViens.Include(g => g.LopHocs).FirstOrDefaultAsync(g => g.Id == id);
        if (gv == null) return NotFound();
        if (gv.LopHocs.Any(l => l.TrangThai is "DangHoc" or "DangTuyenSinh"))
        { TempData["Error"] = "Không thể xóa giảng viên đang phụ trách lớp học!"; return RedirectToAction(nameof(Index)); }
        var nd = await _db.NguoiDungs.FindAsync(id);
        if (nd != null) { nd.IsActive = false; nd.NgayCapNhat = DateTime.Now; }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa giảng viên";
        return RedirectToAction(nameof(Index));
    }

    private static LichHocItemViewModel ToItem(Models.Entities.LichHoc l) => new()
    {
        Id = l.Id, NgayHoc = l.NgayHoc, GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc,
        PhongHoc = l.PhongHoc, TenKhoaHoc = l.LopHoc?.KhoaHoc?.TenKhoaHoc,
        TenLop = l.LopHoc?.TenLop, ChuDe = l.ChuDe
    };
}
