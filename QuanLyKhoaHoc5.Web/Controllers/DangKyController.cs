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
public class DangKyController : Controller
{
    private readonly AppDbContext _db;
    private readonly ThongBaoService _thongBao;

    public DangKyController(AppDbContext db, ThongBaoService thongBao)
    {
        _db = db; _thongBao = thongBao;
    }

    // Admin: xem tất cả đăng ký
    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Index(DangKyFilterViewModel filter)
    {
        var query = _db.DangKyKhoaHocs
            .Include(d => d.HocVien)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.NguoiDuyet)
            .AsQueryable();

        filter.Search = filter.Search?.Trim();
        filter.TrangThai = filter.TrangThai?.Trim();

        if (!string.IsNullOrEmpty(filter.TrangThai)) query = query.Where(d => d.TrangThai == filter.TrangThai);
        if (filter.LopHocId.HasValue) query = query.Where(d => d.LopHocId == filter.LopHocId);
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(d => d.HocVien.HoTen.ToLower().Contains(s) || d.HocVien.MaHocVien.ToLower().Contains(s));
        }

        filter.TotalItems = await query.CountAsync();
        filter.Items = await query.OrderByDescending(d => d.NgayDangKy)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .Select(d => new DangKyListViewModel
            {
                Id = d.Id, MaHocVien = d.HocVien.MaHocVien, TenHocVien = d.HocVien.HoTen,
                TenLop = d.LopHoc.TenLop, TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc,
                NgonNgu = d.LopHoc.KhoaHoc.NgonNgu, TrangThai = d.TrangThai,
                NgayDangKy = d.NgayDangKy, NgayDuyet = d.NgayDuyet,
                TenNguoiDuyet = d.NguoiDuyet != null ? d.NguoiDuyet.HoTen : null,
                LyDoTuChoi = d.LyDoTuChoi
            }).ToListAsync();

        ViewBag.LopHocList = await _db.LopHocs.Select(l => new { l.Id, l.TenLop }).ToListAsync();
        return View(filter);
    }

    // HocVien: xem đăng ký của mình
    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> CuaToi()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.DangKyKhoaHocs
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Where(d => d.HocVienId == userId)
            .OrderByDescending(d => d.NgayDangKy)
            .Select(d => new DangKyListViewModel
            {
                Id = d.Id, TenLop = d.LopHoc.TenLop,
                TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc, NgonNgu = d.LopHoc.KhoaHoc.NgonNgu,
                TrangThai = d.TrangThai, NgayDangKy = d.NgayDangKy,
                LyDoTuChoi = d.LyDoTuChoi
            }).ToListAsync();

        // Danh sách lớp đang tuyển sinh để đăng ký mới
        ViewBag.LopMoDangKy = await _db.LopHocs
            .Include(l => l.KhoaHoc).Include(l => l.GiangVien).Include(l => l.DangKys)
            .Where(l => l.TrangThai == "DangTuyenSinh"
                && !l.DangKys.Any(d => d.HocVienId == userId && d.TrangThai != "DaHuy" && d.TrangThai != "TuChoi"))
            .Select(l => new LopHocListViewModel
            {
                Id = l.Id, TenLop = l.TenLop,
                TenKhoaHoc = l.KhoaHoc.TenKhoaHoc, NgonNgu = l.KhoaHoc.NgonNgu,
                NgayKhaiGiang = l.NgayKhaiGiang, SiSoToiDa = l.SiSoToiDa,
                SiSoHienTai = l.DangKys.Count(d => d.TrangThai == "DaDuyet"),
                TenGiangVien = l.GiangVien != null ? l.GiangVien.HoTen : null,
                PhongHoc = l.PhongHoc, TrangThai = l.TrangThai
            }).ToListAsync();

        return View(list);
    }

    // HocVien: nộp đơn đăng ký
    [AuthorizeRole("HocVien")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(int lopHocId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var lop = await _db.LopHocs.Include(l => l.DangKys).Include(l => l.KhoaHoc)
            .FirstOrDefaultAsync(l => l.Id == lopHocId);
        if (lop == null) { TempData["Error"] = "Lớp học không tồn tại"; return RedirectToAction("CuaToi"); }
        if (lop.TrangThai != "DangTuyenSinh") { TempData["Error"] = "Lớp học không còn tuyển sinh"; return RedirectToAction("CuaToi"); }

        var soHienTai = lop.DangKys.Count(d => d.TrangThai == "DaDuyet");
        if (soHienTai >= lop.SiSoToiDa) { TempData["Error"] = "Lớp học đã đủ học viên"; return RedirectToAction("CuaToi"); }

        // Check if already registered with an active status
        var existing = await _db.DangKyKhoaHocs
            .FirstOrDefaultAsync(d => d.HocVienId == userId && d.LopHocId == lopHocId);

        if (existing != null)
        {
            if (existing.TrangThai != "DaHuy" && existing.TrangThai != "TuChoi")
            {
                TempData["Error"] = "Bạn đã đăng ký lớp này rồi"; return RedirectToAction("CuaToi");
            }
            // Reuse existing row (avoid unique constraint violation)
            existing.TrangThai = "ChoDuyet";
            existing.NgayDangKy = DateTime.Now;
            existing.NgayDuyet = null;
            existing.NguoiDuyetId = null;
            existing.LyDoTuChoi = null;
        }
        else
        {
            _db.DangKyKhoaHocs.Add(new DangKyKhoaHoc
            {
                HocVienId = userId, LopHocId = lopHocId, TrangThai = "ChoDuyet"
            });
        }
        await _db.SaveChangesAsync();

        // Thông báo cho admin
        var admins = await _db.NguoiDungs.Where(u => u.VaiTro == "Admin" && u.IsActive).ToListAsync();
        var hv = await _db.HocViens.FindAsync(userId);
        foreach (var admin in admins)
            await _thongBao.TaoThongBaoAsync(admin.Id, $"Đăng ký mới: {hv?.HoTen} - {lop.TenLop}",
                loai: "DangKy", duongDan: "/DangKy");

        TempData["Success"] = "Đăng ký thành công! Vui lòng chờ Admin duyệt.";
        return RedirectToAction("CuaToi");
    }

    // Admin: duyệt đơn
    [AuthorizeRole("Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duyet(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dk = await _db.DangKyKhoaHocs.Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dk == null) return NotFound();

        dk.TrangThai = "DaDuyet"; dk.NguoiDuyetId = userId; dk.NgayDuyet = DateTime.Now;

        // Tạo bản ghi điểm trống
        if (!await _db.Diems.AnyAsync(d => d.DangKyId == id))
            _db.Diems.Add(new Diem { DangKyId = id });

        await _db.SaveChangesAsync();
        await _thongBao.TaoThongBaoAsync(dk.HocVienId,
            $"Đăng ký được duyệt: {dk.LopHoc.TenLop}",
            $"Đăng ký lớp {dk.LopHoc.TenLop} - {dk.LopHoc.KhoaHoc.TenKhoaHoc} đã được duyệt.",
            loai: "DangKy", duongDan: "/DangKy/CuaToi");

        TempData["Success"] = "Đã duyệt đơn đăng ký";
        return RedirectToAction(nameof(Index));
    }

    // Admin: từ chối
    [AuthorizeRole("Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TuChoi(int id, string lyDo)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dk = await _db.DangKyKhoaHocs.Include(d => d.LopHoc)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dk == null) return NotFound();

        dk.TrangThai = "TuChoi"; dk.LyDoTuChoi = lyDo;
        dk.NguoiDuyetId = userId; dk.NgayDuyet = DateTime.Now;
        await _db.SaveChangesAsync();

        await _thongBao.TaoThongBaoAsync(dk.HocVienId,
            $"Đăng ký bị từ chối: {dk.LopHoc.TenLop}",
            $"Lý do: {lyDo}", loai: "DangKy", duongDan: "/DangKy/CuaToi");

        TempData["Warning"] = "Đã từ chối đơn đăng ký";
        return RedirectToAction(nameof(Index));
    }

    // Hủy đăng ký (HocVien hoặc Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var dk = await _db.DangKyKhoaHocs.FindAsync(id);
        if (dk == null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool isAdmin = User.IsInRole("Admin");

        if (!isAdmin && dk.HocVienId != userId) return Forbid();
        if (!isAdmin && dk.TrangThai != "ChoDuyet")
        {
            TempData["Error"] = "Chỉ có thể hủy đơn đang chờ duyệt";
            return RedirectToAction("CuaToi");
        }

        dk.TrangThai = "DaHuy";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã hủy đăng ký";
        return isAdmin ? RedirectToAction(nameof(Index)) : RedirectToAction("CuaToi");
    }
}
