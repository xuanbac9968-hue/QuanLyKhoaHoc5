using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class DiemSoController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelService _excel;

    public DiemSoController(AppDbContext db, ExcelService excel)
    {
        _db = db;
        _excel = excel;
    }

    // ═══════════════════════════════════════════════════════════════
    // ADMIN: Bảng điểm toàn hệ thống
    // ═══════════════════════════════════════════════════════════════

    // Route alias: /DiemSo/QuanLy → Index
    [AuthorizeRole("Admin")]
    [Route("DiemSo/QuanLy")]
    public IActionResult QuanLy(int? kyHocId, int? khoaHocId, int? lopHocId, string? timKiem, int page = 1)
        => RedirectToAction(nameof(Index), new { kyHocId, khoaHocId, lopHocId, timKiem, page });

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index(int? kyHocId, int? khoaHocId, int? lopHocId, string? timKiem, int page = 1)
    {
        const int pageSize = 20;

        var query = _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.HocVien)
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.GiangVien)
            .Include(d => d.KyHoc)
            .Where(d => d.DangKy.TrangThai == "DaDuyet")
            .AsQueryable();

        if (kyHocId.HasValue) query = query.Where(d => d.KyHocId == kyHocId);
        if (khoaHocId.HasValue) query = query.Where(d => d.DangKy.LopHoc.KhoaHocId == khoaHocId);
        if (lopHocId.HasValue) query = query.Where(d => d.DangKy.LopHocId == lopHocId);
        if (!string.IsNullOrWhiteSpace(timKiem))
            query = query.Where(d =>
                d.DangKy.HocVien.HoTen.Contains(timKiem) ||
                d.DangKy.HocVien.MaHocVien.Contains(timKiem));

        // Thống kê trên toàn bộ kết quả (không phân trang)
        var allDiems = await query.Select(d => d.DiemTongKet).ToListAsync();
        var histogram = new int[10];
        foreach (var diem in allDiems.Where(d => d.HasValue))
        {
            int bucket = Math.Min((int)Math.Floor(diem!.Value), 9);
            histogram[bucket]++;
        }

        var thongKe = new DiemSoThongKeViewModel
        {
            TongHocVien  = allDiems.Count,
            SoHocVienDat = allDiems.Count(d => d.HasValue && d.Value >= 4.0),
            SoHocVienChuaDat = allDiems.Count(d => d.HasValue && d.Value < 4.0),
            DiemTrungBinh = allDiems.Any(d => d.HasValue)
                ? Math.Round(allDiems.Where(d => d.HasValue).Average(d => d!.Value), 2)
                : 0,
            Histogram = histogram
        };

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.NgayCapNhat ?? DateTime.MinValue)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(d => new DiemSoRowViewModel
            {
                DiemId        = d.Id,
                DangKyId      = d.DangKyId,
                MaHocVien     = d.DangKy.HocVien.MaHocVien,
                TenHocVien    = d.DangKy.HocVien.HoTen,
                TenKhoaHoc    = d.DangKy.LopHoc.KhoaHoc.TenKhoaHoc,
                TenLop        = d.DangKy.LopHoc.TenLop,
                TenGiangVien  = d.DangKy.LopHoc.GiangVien != null ? d.DangKy.LopHoc.GiangVien.HoTen : null,
                TenKyHoc      = d.KyHoc != null ? d.KyHoc.TenKy : null,
                DiemGiuaKy    = d.DiemGiuaKy,
                DiemCuoiKy    = d.DiemCuoiKy,
                DiemTongKet   = d.DiemTongKet,
                XepLoai       = d.XepLoai,
                IsKhoa        = d.IsKhoa,
                NgayCapNhat   = d.NgayCapNhat
            })
            .ToListAsync();

        ViewBag.KyHocs   = await _db.KyHocs.OrderByDescending(k => k.NgayBatDau).ToListAsync();
        ViewBag.KhoaHocs = await _db.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync();
        ViewBag.LopHocs  = await _db.LopHocs.OrderBy(l => l.TenLop).ToListAsync();

        return View(new DiemSoFilterViewModel
        {
            KyHocId = kyHocId, KhoaHocId = khoaHocId, LopHocId = lopHocId,
            TimKiem = timKiem, Page = page, PageSize = pageSize,
            TotalItems = total, Items = items, ThongKe = thongKe
        });
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> ExportExcel(int? kyHocId, int? khoaHocId, int? lopHocId, string? timKiem)
    {
        var query = _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.HocVien)
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.GiangVien)
            .Include(d => d.KyHoc)
            .Where(d => d.DangKy.TrangThai == "DaDuyet")
            .AsQueryable();

        if (kyHocId.HasValue) query = query.Where(d => d.KyHocId == kyHocId);
        if (khoaHocId.HasValue) query = query.Where(d => d.DangKy.LopHoc.KhoaHocId == khoaHocId);
        if (lopHocId.HasValue) query = query.Where(d => d.DangKy.LopHocId == lopHocId);
        if (!string.IsNullOrWhiteSpace(timKiem))
            query = query.Where(d =>
                d.DangKy.HocVien.HoTen.Contains(timKiem) ||
                d.DangKy.HocVien.MaHocVien.Contains(timKiem));

        // Re-use existing ExcelService (by lopHocId) if single class, else export first class
        if (lopHocId.HasValue)
        {
            var bytes = await _excel.ExportBangDiemAsync(lopHocId.Value);
            var lop = await _db.LopHocs.FindAsync(lopHocId.Value);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BangDiem_{lop?.TenLop}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // Fallback: export first class in query
        var firstLopId = await query.Select(d => d.DangKy.LopHocId).FirstOrDefaultAsync();
        if (firstLopId > 0)
        {
            var bytes = await _excel.ExportBangDiemAsync(firstLopId);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BangDiem_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        TempData["Warning"] = "Không có dữ liệu để xuất.";
        return RedirectToAction(nameof(Index));
    }

    // POST /DiemSo/KhoaDiem/{lopHocId} – Admin khóa điểm cả lớp
    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaDiem(int lopHocId)
    {
        var diems = await _db.Diems
            .Where(d => d.DangKy.LopHocId == lopHocId)
            .ToListAsync();

        if (!diems.Any())
            return Json(new { success = false, message = "Không tìm thấy dữ liệu điểm của lớp này" });

        diems.ForEach(d => { d.IsKhoa = true; d.NgayCapNhat = DateTime.Now; });
        await _db.SaveChangesAsync();

        var tenLop = await _db.LopHocs.Where(l => l.Id == lopHocId).Select(l => l.TenLop).FirstOrDefaultAsync();
        return Json(new { success = true, message = $"Đã khóa điểm lớp {tenLop} ({diems.Count} học viên)" });
    }

    // GET /DiemSo/ExportExcel/{lopHocId} – xuất bảng điểm theo lớp
    [AuthorizeRole("Admin", "GiangVien")]
    [HttpGet("DiemSo/ExportExcel/{lopHocId:int}")]
    public async Task<IActionResult> ExportExcelByLop(int lopHocId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var lop = await _db.LopHocs.FirstOrDefaultAsync(l => l.Id == lopHocId);
        if (lop == null) return NotFound();
        if (User.IsInRole("GiangVien") && lop.GiangVienId != userId) return Forbid();

        var bytes = await _excel.ExportBangDiemAsync(lopHocId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"BangDiem_{lop.TenLop}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // ═══════════════════════════════════════════════════════════════
    // GIẢNG VIÊN: Nhập điểm inline
    // ═══════════════════════════════════════════════════════════════

    [AuthorizeRole("Admin", "GiangVien")]
    public async Task<IActionResult> NhapDiem(int? lopHocId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // List lớp học để chọn
        IQueryable<LopHoc> lopQuery = _db.LopHocs
            .Include(l => l.KhoaHoc)
            .Where(l => l.TrangThai != "DaKetThuc");

        if (User.IsInRole("GiangVien"))
            lopQuery = lopQuery.Where(l => l.GiangVienId == userId);

        ViewBag.LopHocList = await lopQuery.OrderBy(l => l.TenLop).ToListAsync();
        ViewBag.KyHocList  = await _db.KyHocs.OrderByDescending(k => k.NgayBatDau).ToListAsync();
        ViewBag.SelectedLopHocId = lopHocId;

        if (!lopHocId.HasValue) return View(null as NhapDiemBatchViewModel);

        var lop = await _db.LopHocs
            .Include(l => l.KhoaHoc)
            .FirstOrDefaultAsync(l => l.Id == lopHocId);
        if (lop == null) return NotFound();
        if (User.IsInRole("GiangVien") && lop.GiangVienId != userId) return Forbid();

        // Lấy đăng ký + điểm
        var dangKys = await _db.DangKyKhoaHocs
            .Include(dk => dk.HocVien)
            .Include(dk => dk.Diem)
            .Where(dk => dk.LopHocId == lopHocId && dk.TrangThai == "DaDuyet")
            .OrderBy(dk => dk.HocVien.HoTen)
            .ToListAsync();

        bool isKhoa = dangKys.Any(dk => dk.Diem?.IsKhoa == true);

        // Kiểm tra kỳ học đã đóng chưa
        var kyHoc = dangKys.FirstOrDefault(dk => dk.Diem?.KyHocId != null)?.Diem?.KyHoc;
        bool kyDaDong = kyHoc?.TrangThai == "DaDong";

        var vm = new NhapDiemBatchViewModel
        {
            LopHocId   = lop.Id,
            TenLop     = lop.TenLop,
            TenKhoaHoc = lop.KhoaHoc.TenKhoaHoc,
            IsKhoa     = isKhoa,
            TenKyHoc   = kyHoc?.TenKy,
            KyDaDong   = kyDaDong,
            HocViens   = dangKys.Select(dk => new NhapDiemHangViewModel
            {
                DangKyId  = dk.Id,
                DiemId    = dk.Diem?.Id ?? 0,
                MaHocVien = dk.HocVien.MaHocVien,
                TenHocVien = dk.HocVien.HoTen,
                DiemGiuaKy = dk.Diem?.DiemGiuaKy,
                DiemCuoiKy = dk.Diem?.DiemCuoiKy,
                DiemTongKet = dk.Diem?.DiemTongKet,
                XepLoai   = dk.Diem?.XepLoai,
                NhanXet   = dk.Diem?.NhanXetGiangVien,
                IsKhoa    = dk.Diem?.IsKhoa ?? false
            }).ToList()
        };

        return View(vm);
    }

    // AJAX: Lưu tất cả điểm một lần
    [AuthorizeRole("Admin", "GiangVien")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchSave([FromBody] BatchSaveDiemRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var savedCount = 0;
        var errors = new List<string>();

        foreach (var item in req.Items)
        {
            var diem = await _db.Diems
                .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc)
                .FirstOrDefaultAsync(d => d.DangKyId == item.DangKyId);

            if (diem == null) { errors.Add($"Không tìm thấy điểm DangKyId={item.DangKyId}"); continue; }
            if (diem.IsKhoa && !User.IsInRole("Admin")) { errors.Add($"Điểm đã khóa: {item.DangKyId}"); continue; }
            if (User.IsInRole("GiangVien") && diem.DangKy.LopHoc.GiangVienId != userId)
            { errors.Add($"Không có quyền: {item.DangKyId}"); continue; }

            diem.DiemGiuaKy = item.DiemGiuaKy;
            diem.DiemCuoiKy = item.DiemCuoiKy;
            diem.DiemTongKet = Diem.TinhTongKet(item.DiemGiuaKy, item.DiemCuoiKy);
            diem.XepLoai = Diem.TinhXepLoai(diem.DiemTongKet);
            diem.NhanXetGiangVien = item.NhanXet;
            diem.NgayCapNhat = DateTime.Now;
            savedCount++;
        }

        await _db.SaveChangesAsync();

        return Json(new
        {
            success = errors.Count == 0,
            savedCount,
            errors,
            message = errors.Count == 0
                ? $"Đã lưu {savedCount} điểm thành công."
                : $"Lưu {savedCount}/{req.Items.Count} điểm. {errors.Count} lỗi."
        });
    }

    // Gán KyHoc cho lớp học
    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GanKyHoc(int lopHocId, int kyHocId)
    {
        var diems = await _db.Diems
            .Where(d => d.DangKy.LopHocId == lopHocId)
            .ToListAsync();

        diems.ForEach(d => d.KyHocId = kyHocId);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã gán kỳ học cho {diems.Count} điểm trong lớp.";
        return RedirectToAction(nameof(NhapDiem), new { lopHocId });
    }

    // ═══════════════════════════════════════════════════════════════
    // HỌC VIÊN: Xem điểm theo kỳ học
    // ═══════════════════════════════════════════════════════════════

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> CuaToi()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var diems = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.KyHoc)
            .Where(d => d.DangKy.HocVienId == userId && d.DangKy.TrangThai == "DaDuyet")
            .OrderBy(d => d.KyHoc != null ? d.KyHoc.NgayBatDau.Year : 0)
                .ThenBy(d => d.KyHoc != null ? d.KyHoc.NgayBatDau.Month : 0)
            .ToListAsync();

        // Group theo kỳ học (null = "Chưa phân kỳ")
        var groups = diems
            .GroupBy(d => d.KyHocId)
            .OrderBy(g => g.FirstOrDefault()?.KyHoc?.NgayBatDau ?? DateOnly.MaxValue)
            .Select(g =>
            {
                var ky = g.FirstOrDefault()?.KyHoc;
                return new DiemSoKyHocViewModel
                {
                    KyHocId = g.Key,
                    TenKy = ky?.TenKy ?? "Chưa phân kỳ",
                    Mons = g.Select(d => new DiemSoMonViewModel
                    {
                        TenKhoaHoc = d.DangKy.LopHoc.KhoaHoc.TenKhoaHoc,
                        TenLop     = d.DangKy.LopHoc.TenLop,
                        NgonNgu    = d.DangKy.LopHoc.KhoaHoc.NgonNgu,
                        DiemGiuaKy = d.DiemGiuaKy,
                        DiemCuoiKy = d.DiemCuoiKy,
                        DiemTongKet = d.DiemTongKet,
                        XepLoai    = d.XepLoai,
                        NhanXet    = d.NhanXetGiangVien
                    }).ToList()
                };
            }).ToList();

        // Radar chart: tên khóa + điểm tổng kết (lấy từ tất cả kỳ, deduplicate theo tên khóa)
        var radarData = diems
            .Where(d => d.DiemTongKet.HasValue)
            .GroupBy(d => d.DangKy.LopHoc.KhoaHoc.TenKhoaHoc)
            .Select(g => new { Label = g.Key, Diem = g.OrderByDescending(d => d.NgayCapNhat).First().DiemTongKet!.Value })
            .ToList();

        return View(new DiemSoHocVienViewModel
        {
            CacKy = groups,
            RadarLabels = radarData.Select(r => r.Label).ToList(),
            RadarData = radarData.Select(r => r.Diem).ToList()
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // ADMIN: Quản lý KyHoc
    // ═══════════════════════════════════════════════════════════════

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> KyHocList()
    {
        var list = await _db.KyHocs
            .OrderByDescending(k => k.NgayBatDau)
            .ToListAsync();
        return View(list);
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoKyHoc(string tenKy, DateOnly ngayBatDau, DateOnly ngayKetThuc)
    {
        if (string.IsNullOrWhiteSpace(tenKy))
        {
            TempData["Error"] = "Tên kỳ học không được để trống.";
            return RedirectToAction(nameof(KyHocList));
        }

        _db.KyHocs.Add(new KyHoc
        {
            TenKy       = tenKy.Trim(),
            NgayBatDau  = ngayBatDau,
            NgayKetThuc = ngayKetThuc,
            TrangThai   = "DangMo"
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã tạo kỳ học '{tenKy}'.";
        return RedirectToAction(nameof(KyHocList));
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DongKyHoc(int id)
    {
        var ky = await _db.KyHocs.FindAsync(id);
        if (ky == null) return NotFound();
        ky.TrangThai = "DaDong";
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Đã đóng kỳ học '{ky.TenKy}'.";
        return RedirectToAction(nameof(KyHocList));
    }
}
