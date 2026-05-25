using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

public class HocVienController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelService _excel;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public HocVienController(AppDbContext db, ExcelService excel, IWebHostEnvironment env, IConfiguration config)
    { _db = db; _excel = excel; _env = env; _config = config; }

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var hv = await _db.HocViens
            .Include(h => h.NguoiDung)
            .Include(h => h.DangKys).ThenInclude(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(h => h.DangKys).ThenInclude(d => d.Diem)
            .FirstOrDefaultAsync(h => h.Id == userId);
        if (hv == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var enrolledLopIds = hv.DangKys.Where(d => d.TrangThai == "DaDuyet").Select(d => d.LopHocId).Distinct().ToList();
        var dayNum = (int)today.DayOfWeek;
        var startOfWeek = today.AddDays(-(dayNum == 0 ? 6 : dayNum - 1));
        var endOfWeek = startOfWeek.AddDays(6);

        var lichHocEntities = await _db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Include(l => l.LopHoc).ThenInclude(lh => lh.GiangVien)
            .Where(l => enrolledLopIds.Contains(l.LopHocId) && l.NgayHoc >= startOfWeek && l.NgayHoc <= endOfWeek)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .ToListAsync();

        var lichHocTuanNay = lichHocEntities.Select(l => new BuoiHocViewModel
        {
            NgayHoc = l.NgayHoc, KhoaHocId = l.LopHoc.KhoaHocId,
            TenKhoaHoc = l.LopHoc.KhoaHoc?.TenKhoaHoc ?? "",
            TenGiangVien = l.LopHoc.GiangVien?.HoTen,
            GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc,
            PhongHoc = l.PhongHoc ?? l.LopHoc.PhongHoc
        }).ToList();

        var goiYGanNhat = await _db.GoiYKhoaHocs
            .Include(g => g.KhoaHocGoiY)
            .Where(g => g.HocVienId == userId)
            .OrderByDescending(g => g.NgayGoiY)
            .Take(3)
            .Select(g => new GoiYKetQuaViewModel
            {
                KhoaHocId = g.KhoaHocGoiYId, TenKhoaHoc = g.KhoaHocGoiY.TenKhoaHoc,
                DiemPhuHop = g.DiemPhuHop ?? 0, LyDo = g.LyDoGoiY ?? "",
                HocPhi = g.KhoaHocGoiY.HocPhi, AnhBia = g.KhoaHocGoiY.AnhBia
            }).ToListAsync();

        return View(new HocVienDashboardViewModel
        {
            HoTen = hv.HoTen, MaHocVien = hv.MaHocVien,
            AnhDaiDien = hv.NguoiDung.AnhDaiDien,
            TrinhDoHienTai = hv.TrinhDoHienTai, NgonNguQuanTam = hv.NgonNguQuanTam,
            SoThongBaoChuaDoc = await _db.ThongBaos.CountAsync(t => t.NguoiNhanId == userId && !t.DaDoc),
            GoiYGanNhat = goiYGanNhat,
            KhoaHocDangHoc = hv.DangKys.Where(d => d.TrangThai == "DaDuyet" && d.LopHoc.TrangThai != "DaKetThuc")
                .Select(d => new DangKyItemViewModel { Id = d.Id, KhoaHocId = d.LopHoc.KhoaHocId, TenLop = d.LopHoc.TenLop, TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc, NgonNgu = d.LopHoc.KhoaHoc.NgonNgu, TrangThai = d.TrangThai, NgayDangKy = d.NgayDangKy, DiemTongKet = d.Diem?.DiemTongKet, XepLoai = d.Diem?.XepLoai }).ToList(),
            LichHocTuanNay = lichHocTuanNay
        });
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _db.HocViens.Include(h => h.NguoiDung).Include(h => h.DangKys).AsQueryable();
        search = search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(h => h.HoTen.ToLower().Contains(s) || h.MaHocVien.ToLower().Contains(s) || h.NguoiDung.Email.ToLower().Contains(s));
        }
        ViewBag.Search = search; ViewBag.Page = page;
        int pageSize = 10;
        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
        return View(await query.OrderBy(h => h.MaHocVien)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(h => new HocVienListViewModel { Id = h.Id, MaHocVien = h.MaHocVien, HoTen = h.HoTen, Email = h.NguoiDung.Email, SoDienThoai = h.NguoiDung.SoDienThoai, TrinhDoHienTai = h.TrinhDoHienTai, NgonNguQuanTam = h.NgonNguQuanTam, IsActive = h.NguoiDung.IsActive, NgayDangKy = h.NgayDangKy, SoKhoaDaDangKy = h.DangKys.Count(d => d.TrangThai == "DaDuyet") })
            .ToListAsync());
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var hv = await _db.HocViens.Include(h => h.NguoiDung).Include(h => h.DangKys).ThenInclude(d => d.LopHoc).ThenInclude(l => l.KhoaHoc).Include(h => h.DangKys).ThenInclude(d => d.Diem).FirstOrDefaultAsync(h => h.Id == id);
        if (hv == null) return NotFound();
        return View(new HocVienDetailsViewModel { Id = hv.Id, MaHocVien = hv.MaHocVien, HoTen = hv.HoTen, Email = hv.NguoiDung.Email, SoDienThoai = hv.NguoiDung.SoDienThoai, AnhDaiDien = hv.NguoiDung.AnhDaiDien, NgaySinh = hv.NgaySinh, GioiTinh = hv.GioiTinh, DiaChi = hv.DiaChi, TrinhDoHienTai = hv.TrinhDoHienTai, NgonNguQuanTam = hv.NgonNguQuanTam, IsActive = hv.NguoiDung.IsActive, NgayDangKy = hv.NgayDangKy, LichSuDangKy = hv.DangKys.Select(d => new DangKyItemViewModel { Id = d.Id, TenLop = d.LopHoc.TenLop, TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc, NgonNgu = d.LopHoc.KhoaHoc.NgonNgu, TrangThai = d.TrangThai, NgayDangKy = d.NgayDangKy, DiemTongKet = d.Diem?.DiemTongKet, XepLoai = d.Diem?.XepLoai, LyDoTuChoi = d.LyDoTuChoi }).ToList() });
    }

    [AuthorizeRole("Admin")] [HttpGet]
    public IActionResult Create() => View(new HocVienCreateEditViewModel());

    [AuthorizeRole("Admin")] [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HocVienCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (await _db.NguoiDungs.AnyAsync(u => u.Email == vm.Email)) { ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống"); return View(vm); }
        var defaultPw = _config["AppSettings:DefaultPassword"] ?? "Abc@12345";
        var nd = new NguoiDung { Email = vm.Email, HoTen = vm.HoTen, SoDienThoai = vm.SoDienThoai, MatKhauHash = BCrypt.Net.BCrypt.HashPassword(defaultPw), VaiTro = "HocVien" };
        _db.NguoiDungs.Add(nd); await _db.SaveChangesAsync();
        var soThuTu = (await _db.HocViens.CountAsync()) + 1;
        _db.HocViens.Add(new HocVien { Id = nd.Id, MaHocVien = $"HV{soThuTu:D3}", HoTen = vm.HoTen, NgaySinh = vm.NgaySinh, GioiTinh = vm.GioiTinh, DiaChi = vm.DiaChi, TrinhDoHienTai = vm.TrinhDoHienTai, NgonNguQuanTam = vm.NgonNguQuanTam, GhiChu = vm.GhiChu });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Thêm học viên thành công! Mật khẩu mặc định: {defaultPw}";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")] [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var hv = await _db.HocViens.Include(h => h.NguoiDung).FirstOrDefaultAsync(h => h.Id == id);
        if (hv == null) return NotFound();
        return View(new HocVienCreateEditViewModel { Id = hv.Id, HoTen = hv.HoTen, Email = hv.NguoiDung.Email, SoDienThoai = hv.NguoiDung.SoDienThoai, NgaySinh = hv.NgaySinh, GioiTinh = hv.GioiTinh, DiaChi = hv.DiaChi, TrinhDoHienTai = hv.TrinhDoHienTai, NgonNguQuanTam = hv.NgonNguQuanTam, GhiChu = hv.GhiChu });
    }

    [AuthorizeRole("Admin")] [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HocVienCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var hv = await _db.HocViens.Include(h => h.NguoiDung).FirstOrDefaultAsync(h => h.Id == id);
        if (hv == null) return NotFound();
        hv.HoTen = vm.HoTen; hv.NgaySinh = vm.NgaySinh; hv.GioiTinh = vm.GioiTinh; hv.DiaChi = vm.DiaChi; hv.TrinhDoHienTai = vm.TrinhDoHienTai; hv.NgonNguQuanTam = vm.NgonNguQuanTam; hv.GhiChu = vm.GhiChu;
        hv.NguoiDung.HoTen = vm.HoTen; hv.NguoiDung.SoDienThoai = vm.SoDienThoai; hv.NguoiDung.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật thông tin học viên thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")] [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var nd = await _db.NguoiDungs.FindAsync(id);
        if (nd == null) return NotFound();
        nd.IsActive = false; nd.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã vô hiệu hóa học viên";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportExcel()
    {
        var bytes = await _excel.ExportHocVienAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachHocVien_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
