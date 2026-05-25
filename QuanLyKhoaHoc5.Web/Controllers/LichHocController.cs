using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Helpers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class LichHocController : Controller
{
    private readonly AppDbContext _db;

    public LichHocController(AppDbContext db) => _db = db;

    // ── Admin calendar view ─────────────────────────────────────────────────
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index()
    {
        ViewBag.KhoaHocs   = await _db.KhoaHocs.OrderBy(k => k.TenKhoaHoc).ToListAsync();
        ViewBag.GiangViens = await _db.GiangViens.OrderBy(g => g.HoTen).ToListAsync();
        ViewBag.LopHocs    = await _db.LopHocs.Include(l => l.KhoaHoc).OrderBy(l => l.TenLop).ToListAsync();
        ViewBag.Role       = "Admin";
        return View("CuaToi");
    }

    // ── GiangVien / HocVien calendar view ───────────────────────────────────
    public async Task<IActionResult> CuaToi()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "HocVien";
        if (role == "Admin") return RedirectToAction(nameof(Index));

        if (role == "GiangVien")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ViewBag.LopHocs = await _db.LopHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.GiangVienId == userId)
                .OrderBy(l => l.TenLop)
                .ToListAsync();
        }

        ViewBag.Role = role;
        return View();
    }

    // ── JSON API: Events for calendar AJAX ─────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetEvents(int month, int year,
        int khoaHocId = 0, int giangVienId = 0)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role   = User.FindFirstValue(ClaimTypes.Role) ?? "HocVien";

        // Lấy đầu tháng và cuối tháng + đệm thêm 1 tuần mỗi phía cho Month view
        var startDate = new DateOnly(year, month, 1).AddDays(-7);
        var endDate   = new DateOnly(year, month, 1).AddMonths(1).AddDays(7);

        var query = _db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Include(l => l.LopHoc).ThenInclude(lh => lh.GiangVien)
            .Where(l => l.NgayHoc >= startDate && l.NgayHoc <= endDate)
            .AsQueryable();

        if (role == "GiangVien")
        {
            query = query.Where(l => l.LopHoc.GiangVienId == userId);
        }
        else if (role == "HocVien")
        {
            var enrolledLopIds = await _db.DangKyKhoaHocs
                .Where(d => d.HocVienId == userId && d.TrangThai == "DaDuyet")
                .Select(d => d.LopHocId)
                .ToListAsync();
            query = query.Where(l => enrolledLopIds.Contains(l.LopHocId));
        }
        else // Admin
        {
            if (khoaHocId > 0)
                query = query.Where(l => l.LopHoc.KhoaHocId == khoaHocId);
            if (giangVienId > 0)
                query = query.Where(l => l.LopHoc.GiangVienId == giangVienId);
        }

        var lichHocs = await query.ToListAsync();

        // Lấy sĩ số cho các lớp liên quan
        var lopIds = lichHocs.Select(l => l.LopHocId).Distinct().ToList();
        var siSoMap = await _db.DangKyKhoaHocs
            .Where(dk => lopIds.Contains(dk.LopHocId) && dk.TrangThai == "DaDuyet")
            .GroupBy(dk => dk.LopHocId)
            .Select(g => new { LopHocId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.LopHocId, g => g.Count);

        var events = lichHocs.Select(l => new
        {
            id           = l.Id,
            lopHocId     = l.LopHocId,
            khoaHocId    = l.LopHoc.KhoaHocId,
            tenKhoaHoc   = l.LopHoc.KhoaHoc?.TenKhoaHoc ?? "",
            ngayHoc      = l.NgayHoc.ToString("yyyy-MM-dd"),
            gioBatDau    = l.GioBatDau.ToString("HH:mm"),
            gioKetThuc   = l.GioKetThuc.ToString("HH:mm"),
            startMin     = l.GioBatDau.Hour * 60 + l.GioBatDau.Minute,
            endMin       = l.GioKetThuc.Hour * 60 + l.GioKetThuc.Minute,
            phongHoc     = l.PhongHoc ?? l.LopHoc?.PhongHoc ?? "—",
            tenGiangVien = l.LopHoc?.GiangVien?.HoTen ?? "—",
            tenLop       = l.LopHoc?.TenLop ?? "",
            siSoHienTai  = siSoMap.GetValueOrDefault(l.LopHocId, 0),
            siSoToiDa    = l.LopHoc?.SiSoToiDa ?? 0,
            trangThaiLop = l.LopHoc?.TrangThai ?? "ChuaMo",
            chuDe        = l.ChuDe
        })
        .OrderBy(e => e.ngayHoc).ThenBy(e => e.gioBatDau)
        .ToList();

        return Json(events);
    }

    // ── Create single session (Admin, AJAX or form) ─────────────────────────
    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] LichHocCreateViewModel vm)
    {
        if (!DateOnly.TryParse(vm.NgayHoc, out var ngayHoc) ||
            !TimeOnly.TryParse(vm.GioBatDau, out var gioBD) ||
            !TimeOnly.TryParse(vm.GioKetThuc, out var gioKT))
        {
            return Json(new { success = false, message = "Ngày giờ không hợp lệ" });
        }

        // Kiểm tra trùng phòng
        if (!string.IsNullOrWhiteSpace(vm.PhongHoc))
        {
            var roomConflict = await _db.LichHocs.AnyAsync(l =>
                l.PhongHoc == vm.PhongHoc &&
                l.NgayHoc == ngayHoc &&
                l.GioBatDau < gioKT &&
                l.GioKetThuc > gioBD);
            if (roomConflict)
                return Json(new { success = false, message = $"Phòng {vm.PhongHoc} đã có lớp khác vào giờ này" });
        }

        // Kiểm tra trùng giảng viên
        var lopHoc = await _db.LopHocs.FindAsync(vm.LopHocId);
        if (lopHoc?.GiangVienId != null)
        {
            var gvId = lopHoc.GiangVienId.Value;
            var gvConflict = await _db.LichHocs.AnyAsync(l =>
                l.LopHoc.GiangVienId == gvId &&
                l.NgayHoc == ngayHoc &&
                l.GioBatDau < gioKT &&
                l.GioKetThuc > gioBD);
            if (gvConflict)
                return Json(new { success = false, message = "Giảng viên đã có lịch dạy vào giờ này" });
        }

        _db.LichHocs.Add(new LichHoc
        {
            LopHocId  = vm.LopHocId,
            NgayHoc   = ngayHoc,
            GioBatDau = gioBD,
            GioKetThuc = gioKT,
            PhongHoc  = vm.PhongHoc,
            ChuDe     = vm.ChuDe,
            GhiChu    = vm.GhiChu
        });
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Đã thêm buổi học thành công" });
    }

    // ── Delete single session (Admin, AJAX) ────────────────────────────────
    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lh = await _db.LichHocs.FindAsync(id);
        if (lh == null) return Json(new { success = false, message = "Không tìm thấy buổi học" });
        _db.LichHocs.Remove(lh);
        await _db.SaveChangesAsync();
        return Json(new { success = true, message = "Đã xóa buổi học" });
    }

    // ── Bulk session creation (Admin, AJAX) ────────────────────────────────
    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoHangLoat([FromBody] TaoHangLoatViewModel vm)
    {
        if (!DateOnly.TryParse(vm.NgayBatDau, out var startDate) ||
            !DateOnly.TryParse(vm.NgayKetThuc, out var endDate) ||
            !TimeOnly.TryParse(vm.GioBatDau, out var gioBD) ||
            !TimeOnly.TryParse(vm.GioKetThuc, out var gioKT) ||
            !vm.ThuTrongTuan.Any())
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        if (endDate < startDate)
            return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu" });

        // Convert vm.ThuTrongTuan (1=Mon,2=Tue,...,7=Sun) → DayOfWeek
        // DayOfWeek: Sun=0,Mon=1,Tue=2,Wed=3,Thu=4,Fri=5,Sat=6
        var dayMap = new Dictionary<int, DayOfWeek>
        {
            { 1, DayOfWeek.Monday }, { 2, DayOfWeek.Tuesday }, { 3, DayOfWeek.Wednesday },
            { 4, DayOfWeek.Thursday }, { 5, DayOfWeek.Friday }, { 6, DayOfWeek.Saturday },
            { 7, DayOfWeek.Sunday }
        };
        var selectedDays = vm.ThuTrongTuan
            .Where(dayMap.ContainsKey)
            .Select(d => dayMap[d])
            .ToList();

        // Lấy GiangVienId của lớp để kiểm tra trùng GV
        var lopHoc = await _db.LopHocs.FindAsync(vm.LopHocId);
        if (lopHoc == null)
            return Json(new { success = false, message = "Không tìm thấy lớp học" });

        // Load existing sessions for conflict check
        var existingInRange = await _db.LichHocs
            .Where(l => l.NgayHoc >= startDate && l.NgayHoc <= endDate &&
                        l.GioBatDau < gioKT && l.GioKetThuc > gioBD)
            .Select(l => new { l.NgayHoc, l.PhongHoc, GvId = (int?)l.LopHoc.GiangVienId })
            .ToListAsync();

        int created = 0, skipped = 0;
        var newSessions = new List<LichHoc>();

        foreach (var day in LichHocHelper.GenerateDates(startDate, endDate, selectedDays))
        {
            // Check room conflict
            bool roomConflict = !string.IsNullOrWhiteSpace(vm.PhongHoc) &&
                existingInRange.Any(e => e.NgayHoc == day && e.PhongHoc == vm.PhongHoc);

            // Check teacher conflict
            bool gvConflict = lopHoc.GiangVienId.HasValue &&
                existingInRange.Any(e => e.NgayHoc == day && e.GvId == lopHoc.GiangVienId);

            if (roomConflict || gvConflict)
            {
                skipped++;
                continue;
            }

            newSessions.Add(new LichHoc
            {
                LopHocId   = vm.LopHocId,
                NgayHoc    = day,
                GioBatDau  = gioBD,
                GioKetThuc = gioKT,
                PhongHoc   = vm.PhongHoc
            });

            // Add to existingInRange to prevent intra-batch conflicts
            existingInRange.Add(new { NgayHoc = day, PhongHoc = vm.PhongHoc, GvId = lopHoc.GiangVienId });
            created++;
        }

        if (newSessions.Any())
        {
            _db.LichHocs.AddRange(newSessions);
            await _db.SaveChangesAsync();
        }

        return Json(new
        {
            success = true,
            message = $"Đã tạo {created} buổi học. Bỏ qua {skipped} buổi bị trùng lịch."
        });
    }

    // ── Student list for a LopHoc (modal) ─────────────────────────────────
    [HttpGet]
    [AuthorizeRole("Admin", "GiangVien")]
    public async Task<IActionResult> GetLopStudents(int lopHocId)
    {
        var students = await _db.DangKyKhoaHocs
            .Where(dk => dk.LopHocId == lopHocId && dk.TrangThai == "DaDuyet")
            .Select(dk => new { hoTen = dk.HocVien.HoTen, maHocVien = dk.HocVien.MaHocVien })
            .OrderBy(s => s.hoTen)
            .ToListAsync();
        return Json(students);
    }
}
