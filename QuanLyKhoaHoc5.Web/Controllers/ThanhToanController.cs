using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class ThanhToanController : Controller
{
    private readonly AppDbContext _db;

    public ThanhToanController(AppDbContext db) => _db = db;

    // ═══════════════════════════════════════════════════════════════
    // HỌC VIÊN: Xem học phí + trạng thái thanh toán
    // ═══════════════════════════════════════════════════════════════

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> CuaToi()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Lấy danh sách khóa học đã đăng ký (đã duyệt)
        var dangKys = await _db.DangKyKhoaHocs
            .Where(d => d.HocVienId == userId && d.TrangThai == "DaDuyet")
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .ToListAsync();

        var khoaHocIds = dangKys.Select(d => d.LopHoc.KhoaHocId).Distinct().ToList();

        // Lấy thanh toán gần nhất mỗi khóa
        var thanhToans = await _db.ThanhToans
            .Where(t => t.HocVienId == userId && khoaHocIds.Contains(t.KhoaHocId))
            .OrderByDescending(t => t.NgayTao)
            .ToListAsync();

        var result = khoaHocIds.Select(kId =>
        {
            var dk = dangKys.First(d => d.LopHoc.KhoaHocId == kId);
            var khoa = dk.LopHoc.KhoaHoc;
            var tt = thanhToans.FirstOrDefault(t => t.KhoaHocId == kId);

            return new HocVienThanhToanViewModel
            {
                KhoaHocId      = kId,
                TenKhoaHoc     = khoa.TenKhoaHoc,
                NgonNgu        = khoa.NgonNgu,
                TrinhDo        = khoa.TrinhDo,
                HocPhi         = khoa.HocPhi,
                TrangThaiKhoaHoc = khoa.TrangThai,
                TrangThaiThanhToan = tt?.TrangThai,
                NgayTaoThanhToan   = tt?.NgayTao,
                PhuongThuc         = tt?.PhuongThuc,
                ThanhToanId        = tt?.Id,
                CoDangKy           = true
            };
        }).ToList();

        return View(result);
    }

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> TaoYeuCau(int khoaHocId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Kiểm tra đăng ký hợp lệ
        var isEnrolled = await _db.DangKyKhoaHocs
            .AnyAsync(d => d.HocVienId == userId
                        && d.LopHoc.KhoaHocId == khoaHocId
                        && d.TrangThai == "DaDuyet");
        if (!isEnrolled)
        {
            TempData["Error"] = "Bạn chưa đăng ký hoặc chưa được duyệt vào khóa học này.";
            return RedirectToAction(nameof(CuaToi));
        }

        // Kiểm tra đã có yêu cầu đang chờ duyệt chưa
        var existing = await _db.ThanhToans
            .FirstOrDefaultAsync(t => t.HocVienId == userId
                                   && t.KhoaHocId == khoaHocId
                                   && t.TrangThai == "ChoPheduyet");
        if (existing != null)
        {
            TempData["Warning"] = "Bạn đã có yêu cầu thanh toán đang chờ duyệt cho khóa học này.";
            return RedirectToAction(nameof(CuaToi));
        }

        var khoa = await _db.KhoaHocs.FindAsync(khoaHocId);
        if (khoa == null) return NotFound();

        var hocVien = await _db.HocViens.FindAsync(userId);

        var vm = new ThanhToanCreateViewModel
        {
            KhoaHocId     = khoaHocId,
            TenKhoaHoc    = khoa.TenKhoaHoc,
            HocPhi        = khoa.HocPhi,
            HoTenHocVien  = hocVien?.HoTen ?? ""
        };

        return View(vm);
    }

    [AuthorizeRole("HocVien")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoYeuCau(ThanhToanCreateViewModel vm)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var khoa = await _db.KhoaHocs.FindAsync(vm.KhoaHocId);
        if (khoa == null) return NotFound();

        vm.TenKhoaHoc = khoa.TenKhoaHoc;
        vm.HocPhi = khoa.HocPhi;

        if (!ModelState.IsValid) return View(vm);

        _db.ThanhToans.Add(new ThanhToan
        {
            HocVienId  = userId,
            KhoaHocId  = vm.KhoaHocId,
            SoTien     = khoa.HocPhi,
            PhuongThuc = vm.PhuongThuc,
            TrangThai  = "ChoPheduyet",
            GhiChu     = vm.GhiChu,
            NgayTao    = DateTime.Now
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã gửi yêu cầu thanh toán. Vui lòng chờ admin xác nhận.";
        return RedirectToAction(nameof(CuaToi));
    }

    // ═══════════════════════════════════════════════════════════════
    // ADMIN: Danh sách & duyệt thanh toán
    // ═══════════════════════════════════════════════════════════════

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index(string? trangThai, int page = 1)
    {
        const int pageSize = 20;

        var query = _db.ThanhToans
            .Include(t => t.HocVien).ThenInclude(hv => hv.NguoiDung)
            .Include(t => t.KhoaHoc)
            .Include(t => t.NguoiDuyet)
            .AsQueryable();

        if (!string.IsNullOrEmpty(trangThai))
            query = query.Where(t => t.TrangThai == trangThai);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new ThanhToanListItemViewModel
            {
                Id             = t.Id,
                HocVienId      = t.HocVienId,
                MaHocVien      = t.HocVien.MaHocVien,
                TenHocVien     = t.HocVien.HoTen,
                TenKhoaHoc     = t.KhoaHoc.TenKhoaHoc,
                SoTien         = t.SoTien,
                PhuongThuc     = t.PhuongThuc,
                TrangThai      = t.TrangThai,
                GhiChu         = t.GhiChu,
                NgayTao        = t.NgayTao,
                NgayDuyet      = t.NgayDuyet,
                TenNguoiDuyet  = t.NguoiDuyet != null ? t.NguoiDuyet.HoTen : null
            })
            .ToListAsync();

        // Thống kê tháng hiện tại
        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);
        var tongThuThang = await _db.ThanhToans
            .Where(t => t.TrangThai == "DaThanhToan" && t.NgayDuyet >= firstOfMonth)
            .SumAsync(t => (decimal?)t.SoTien) ?? 0;

        ViewBag.TongThuThang = tongThuThang;
        ViewBag.Thang = now.Month;
        ViewBag.Nam = now.Year;
        ViewBag.SoChoPheduyet = await _db.ThanhToans.CountAsync(t => t.TrangThai == "ChoPheduyet");

        var filter = new AdminThanhToanFilterViewModel
        {
            TrangThai  = trangThai,
            Page       = page,
            PageSize   = pageSize,
            TotalItems = total,
            Items      = items
        };

        return View(filter);
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var tt = await _db.ThanhToans
            .Include(t => t.HocVien).ThenInclude(hv => hv.NguoiDung)
            .Include(t => t.KhoaHoc)
            .Include(t => t.NguoiDuyet)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tt == null) return NotFound();

        var vm = new ThanhToanListItemViewModel
        {
            Id            = tt.Id,
            HocVienId     = tt.HocVienId,
            MaHocVien     = tt.HocVien.MaHocVien,
            TenHocVien    = tt.HocVien.HoTen,
            TenKhoaHoc    = tt.KhoaHoc.TenKhoaHoc,
            SoTien        = tt.SoTien,
            PhuongThuc    = tt.PhuongThuc,
            TrangThai     = tt.TrangThai,
            GhiChu        = tt.GhiChu,
            NgayTao       = tt.NgayTao,
            NgayDuyet     = tt.NgayDuyet,
            TenNguoiDuyet = tt.NguoiDuyet?.HoTen
        };

        return View(vm);
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Duyet(ThanhToanDuyetViewModel vm)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var tt = await _db.ThanhToans.FindAsync(vm.Id);
        if (tt == null) return NotFound();

        if (tt.TrangThai != "ChoPheduyet")
        {
            TempData["Warning"] = "Yêu cầu này đã được xử lý rồi.";
            return RedirectToAction(nameof(Index));
        }

        tt.TrangThai    = vm.HanhDong == "DaThanhToan" ? "DaThanhToan" : "TuChoi";
        tt.NgayDuyet    = DateTime.Now;
        tt.NguoiDuyetId = adminId;
        if (!string.IsNullOrWhiteSpace(vm.GhiChu))
            tt.GhiChu = vm.GhiChu;

        await _db.SaveChangesAsync();

        TempData["Success"] = tt.TrangThai == "DaThanhToan"
            ? "Đã xác nhận thanh toán thành công."
            : "Đã từ chối yêu cầu thanh toán.";

        return RedirectToAction(nameof(Index));
    }

    // API: Thống kê thu theo tháng (cho dashboard)
    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<IActionResult> ThongKe6Thang()
    {
        var now = DateTime.Now;
        var result = new List<object>();
        for (int i = 5; i >= 0; i--)
        {
            var tg = now.AddMonths(-i);
            var start = new DateTime(tg.Year, tg.Month, 1);
            var end = start.AddMonths(1);
            var tong = await _db.ThanhToans
                .Where(t => t.TrangThai == "DaThanhToan" && t.NgayDuyet >= start && t.NgayDuyet < end)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0;
            result.Add(new { thang = $"{tg.Month}/{tg.Year}", tong });
        }
        return Json(result);
    }
}
