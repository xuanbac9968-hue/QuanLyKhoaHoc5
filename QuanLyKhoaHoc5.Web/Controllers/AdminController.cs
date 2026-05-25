using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;

namespace QuanLyKhoaHoc5.Web.Controllers;

[AuthorizeRole("Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelService _excel;
    private readonly PdfService _pdf;
    private readonly IConfiguration _config;

    public AdminController(AppDbContext db, ExcelService excel, PdfService pdf, IConfiguration config)
    {
        _db = db; _excel = excel; _pdf = pdf; _config = config;
    }

    // ============ DASHBOARD ============
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var dauThang = new DateTime(now.Year, now.Month, 1);

        var vm = new AdminDashboardViewModel
        {
            TongHocVienDangHoc = await _db.DangKyKhoaHocs
                .CountAsync(d => d.TrangThai == "DaDuyet"),
            TongKhoaHocDangMo  = await _db.KhoaHocs.CountAsync(k => k.TrangThai == "DangMo"),
            TongGiangVien      = await _db.GiangViens.CountAsync(),
        };

        vm.DoanhThuThang = await _db.DangKyKhoaHocs
            .Where(d => d.TrangThai == "DaDuyet" && d.NgayDuyet >= dauThang)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .SumAsync(d => (decimal?)d.LopHoc.KhoaHoc.HocPhi) ?? 0;

        // Thanh toán thực tế đã thu qua module ThanhToan
        vm.TongThuThanhToan = await _db.ThanhToans
            .Where(t => t.TrangThai == "DaThanhToan" && t.NgayDuyet >= dauThang)
            .SumAsync(t => (decimal?)t.SoTien) ?? 0;
        vm.SoThanhToanChoPheduyet = await _db.ThanhToans
            .CountAsync(t => t.TrangThai == "ChoPheduyet");

        // Bar chart: số học viên theo từng khóa học
        vm.SoHocVienTheoKhoa = await _db.KhoaHocs
            .Where(k => k.TrangThai == "DangMo")
            .Select(k => new ChartDataPoint
            {
                Label = k.TenKhoaHoc.Length > 18 ? k.TenKhoaHoc.Substring(0, 18) + "..." : k.TenKhoaHoc,
                Value = k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet")
            })
            .OrderByDescending(x => x.Value)
            .Take(8)
            .ToListAsync();

        // Line chart: doanh thu 6 tháng gần nhất
        vm.DoanhThuTheoThang = Enumerable.Range(0, 6).Select(i =>
        {
            var thang = now.AddMonths(-5 + i);
            var tu    = new DateTime(thang.Year, thang.Month, 1);
            var den   = tu.AddMonths(1);
            var dt    = _db.DangKyKhoaHocs
                .Where(d => d.TrangThai == "DaDuyet" && d.NgayDuyet >= tu && d.NgayDuyet < den)
                .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
                .Sum(d => (double?)d.LopHoc.KhoaHoc.HocPhi) ?? 0;
            return new ChartDataPoint { Label = $"T{thang.Month}/{thang.Year % 100}", Value = dt };
        }).ToList();

        // Top 5 khóa học nhiều học viên nhất
        vm.Top5KhoaHoc = await _db.KhoaHocs
            .Include(k => k.LopHocs).ThenInclude(l => l.DangKys)
            .Include(k => k.PhanCongs).ThenInclude(p => p.GiangVien)
            .OrderByDescending(k => k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet"))
            .Take(5)
            .Select(k => new KhoaHocListViewModel
            {
                Id = k.Id, TenKhoaHoc = k.TenKhoaHoc, NgonNgu = k.NgonNgu,
                TrinhDo = k.TrinhDo, HocPhi = k.HocPhi, TrangThai = k.TrangThai,
                SoLopHoc = k.LopHocs.Count,
                SoHocVien = k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet")
            })
            .ToListAsync();

        vm.DangKyGanDay = await _db.DangKyKhoaHocs
            .Include(d => d.HocVien)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .OrderByDescending(d => d.NgayDangKy)
            .Take(8)
            .Select(d => new DangKyListViewModel
            {
                Id = d.Id, MaHocVien = d.HocVien.MaHocVien, TenHocVien = d.HocVien.HoTen,
                TenLop = d.LopHoc.TenLop, TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc,
                NgonNgu = d.LopHoc.KhoaHoc.NgonNgu, TrangThai = d.TrangThai, NgayDangKy = d.NgayDangKy
            })
            .ToListAsync();

        return View(vm);
    }

    // ============ PHÂN CÔNG GIẢNG VIÊN ============
    public async Task<IActionResult> PhanCong()
    {
        var khoaHocs = await _db.KhoaHocs
            .Include(k => k.PhanCongs.Where(p => p.IsActive))
                .ThenInclude(p => p.GiangVien)
            .Where(k => k.TrangThai == "DangMo")
            .OrderBy(k => k.NgonNgu).ThenBy(k => k.TenKhoaHoc)
            .ToListAsync();

        var vm = khoaHocs.Select(k =>
        {
            var active = k.PhanCongs.FirstOrDefault(p => p.IsActive);
            return new PhanCongViewModel
            {
                KhoaHocId = k.Id, TenKhoaHoc = k.TenKhoaHoc,
                NgonNgu = k.NgonNgu, TrinhDo = k.TrinhDo,
                GiangVienIdHienTai = active?.GiangVienId,
                TenGiangVienHienTai = active?.GiangVien.HoTen,
                NgayPhanCong = active?.NgayPhanCong,
                GhiChu = active?.GhiChu,
                PhanCongId = active?.Id ?? 0
            };
        }).ToList();

        ViewBag.GiangViens = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .Where(g => g.NguoiDung.IsActive)
            .OrderBy(g => g.HoTen)
            .ToListAsync();

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoPhanCong(PhanCongFormViewModel vm)
    {
        // Deactivate existing assignments for this course
        var existing = await _db.PhanCongGiangDays
            .Where(p => p.KhoaHocId == vm.KhoaHocId && p.IsActive)
            .ToListAsync();
        foreach (var p in existing) p.IsActive = false;

        if (vm.GiangVienId > 0)
        {
            _db.PhanCongGiangDays.Add(new PhanCongGiangDay
            {
                GiangVienId = vm.GiangVienId,
                KhoaHocId = vm.KhoaHocId,
                GhiChu = vm.GhiChu,
                NgayPhanCong = DateTime.Now,
                IsActive = true
            });

            // Also update LopHoc.GiangVienId for active classes
            var lops = await _db.LopHocs
                .Where(l => l.KhoaHocId == vm.KhoaHocId && l.TrangThai != "DaKetThuc")
                .ToListAsync();
            foreach (var lop in lops) lop.GiangVienId = vm.GiangVienId;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật phân công giảng viên";
        return RedirectToAction(nameof(PhanCong));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyPhanCong(int phanCongId)
    {
        var pc = await _db.PhanCongGiangDays.FindAsync(phanCongId);
        if (pc != null) { pc.IsActive = false; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Đã hủy phân công";
        return RedirectToAction(nameof(PhanCong));
    }

    // Lịch sử phân công
    public async Task<IActionResult> LichSuPhanCong(int khoaHocId)
    {
        var list = await _db.PhanCongGiangDays
            .Include(p => p.GiangVien)
            .Include(p => p.KhoaHoc)
            .Where(p => p.KhoaHocId == khoaHocId)
            .OrderByDescending(p => p.NgayPhanCong)
            .ToListAsync();
        ViewBag.TenKhoaHoc = list.FirstOrDefault()?.KhoaHoc.TenKhoaHoc ?? "";
        return View(list);
    }

    // ============ QUẢN LÝ TÀI KHOẢN ============
    public async Task<IActionResult> TaiKhoan(string? vaiTro, string? search)
    {
        var query = _db.NguoiDungs.AsQueryable();
        if (!string.IsNullOrEmpty(vaiTro)) query = query.Where(u => u.VaiTro == vaiTro);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.HoTen.Contains(search) || u.Email.Contains(search));

        ViewBag.VaiTro = vaiTro;
        ViewBag.Search = search;
        return View(await query.OrderBy(u => u.VaiTro).ThenBy(u => u.HoTen).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaTaiKhoan(int id)
    {
        var user = await _db.NguoiDungs.FindAsync(id);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        user.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = user.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản";
        return RedirectToAction(nameof(TaiKhoan));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiRole(int id, string vaiTro)
    {
        var user = await _db.NguoiDungs.FindAsync(id);
        if (user == null) return NotFound();
        if (vaiTro is not ("Admin" or "GiangVien" or "HocVien")) return BadRequest();
        user.VaiTro = vaiTro;
        user.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã đổi vai trò thành {vaiTro}";
        return RedirectToAction(nameof(TaiKhoan));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetMatKhau(int id)
    {
        var user = await _db.NguoiDungs.FindAsync(id);
        if (user == null) return NotFound();
        var defaultPw = _config["AppSettings:DefaultPassword"] ?? "Abc@12345";
        user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(defaultPw);
        user.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã reset mật khẩu về: {defaultPw}";
        return RedirectToAction(nameof(TaiKhoan));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoTaiKhoan(string email, string hoTen, string? soDienThoai, string vaiTro, string matKhau)
    {
        if (await _db.NguoiDungs.AnyAsync(u => u.Email == email))
        {
            TempData["Error"] = "Email đã tồn tại";
            return RedirectToAction(nameof(TaiKhoan));
        }

        var nd = new NguoiDung
        {
            Email = email, HoTen = hoTen, SoDienThoai = soDienThoai,
            VaiTro = vaiTro, IsActive = true,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau)
        };
        _db.NguoiDungs.Add(nd);
        await _db.SaveChangesAsync();

        if (vaiTro == "GiangVien")
        {
            var soThuTu = (await _db.GiangViens.CountAsync()) + 1;
            _db.GiangViens.Add(new GiangVien { Id = nd.Id, MaGiangVien = $"GV{soThuTu:D3}", HoTen = hoTen });
            await _db.SaveChangesAsync();
        }
        else if (vaiTro == "HocVien")
        {
            var soThuTu = (await _db.HocViens.CountAsync()) + 1;
            _db.HocViens.Add(new HocVien { Id = nd.Id, MaHocVien = $"HV{soThuTu:D3}", HoTen = hoTen });
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = $"Tạo tài khoản thành công: {email}";
        return RedirectToAction(nameof(TaiKhoan));
    }

    // ============ BÁO CÁO ============
    public IActionResult BaoCao() => View(new BaoCaoFilterViewModel());

    public async Task<IActionResult> ExportExcel(DateTime? tuNgay, DateTime? denNgay)
    {
        var bytes = await _excel.ExportBaoCaoHocVienExcelAsync(tuNgay, denNgay);
        var fileName = $"BaoCao_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportPdf(DateTime? tuNgay, DateTime? denNgay)
    {
        try
        {
            var bytes = await _pdf.ExportBaoCaoHocVienPdfAsync(tuNgay, denNgay);
            var fileName = $"BaoCao_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(bytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi xuất PDF: {ex.Message}";
            return RedirectToAction(nameof(BaoCao));
        }
    }
}
