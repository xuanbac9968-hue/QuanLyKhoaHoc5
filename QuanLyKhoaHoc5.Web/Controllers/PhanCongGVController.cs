using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;

namespace QuanLyKhoaHoc5.Web.Controllers;

[AuthorizeRole("Admin")]
public class PhanCongGVController : Controller
{
    private readonly AppDbContext _db;

    public PhanCongGVController(AppDbContext db) { _db = db; }

    // GET /PhanCongGV/Index
    public async Task<IActionResult> Index()
    {
        var lopHocs = await _db.LopHocs
            .Include(l => l.KhoaHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.DangKys)
            .Where(l => l.TrangThai != "DaKetThuc")
            .OrderBy(l => l.TrangThai).ThenBy(l => l.TenLop)
            .ToListAsync();

        var giangViens = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .Include(g => g.LopHocs)
            .Where(g => g.NguoiDung.IsActive)
            .OrderBy(g => g.HoTen)
            .ToListAsync();

        ViewBag.GiangViens = giangViens;
        return View(lopHocs);
    }

    // POST /PhanCongGV/PhanCong  (AJAX)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanCong([FromBody] PhanCongRequest req)
    {
        var lop = await _db.LopHocs
            .Include(l => l.KhoaHoc)
            .FirstOrDefaultAsync(l => l.Id == req.LopHocId);
        if (lop == null) return Json(new { success = false, message = "Lớp học không tồn tại" });

        var gv = await _db.GiangViens
            .Include(g => g.NguoiDung)
            .FirstOrDefaultAsync(g => g.Id == req.GiangVienId);
        if (gv == null) return Json(new { success = false, message = "Giảng viên không tồn tại" });

        lop.GiangVienId = gv.Id;
        await _db.SaveChangesAsync();

        // Tạo thông báo cho giảng viên
        var ngayKG = lop.NgayKhaiGiang.HasValue
            ? lop.NgayKhaiGiang.Value.ToString("dd/MM/yyyy")
            : "chưa xác định";
        _db.ThongBaos.Add(new ThongBao
        {
            NguoiNhanId = gv.Id,
            TieuDe = $"Phân công giảng dạy: {lop.TenLop}",
            NoiDung = $"Bạn được phân công giảng dạy lớp {lop.TenLop} ({lop.KhoaHoc?.TenKhoaHoc}), khai giảng {ngayKG}",
            LoaiThongBao = "HeThong",
            DuongDanLienKet = $"/LopHoc/Details/{lop.Id}"
        });
        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"Đã phân công giảng viên {gv.HoTen} cho lớp {lop.TenLop}",
            tenGiangVien = gv.HoTen,
            giangVienId = gv.Id
        });
    }

    // POST /PhanCongGV/HuyPhanCong/{lopHocId}  (AJAX)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyPhanCong(int lopHocId)
    {
        var lop = await _db.LopHocs.FindAsync(lopHocId);
        if (lop == null) return Json(new { success = false, message = "Lớp học không tồn tại" });

        lop.GiangVienId = null;
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Đã hủy phân công giảng viên" });
    }
}

public class PhanCongRequest
{
    public int LopHocId { get; set; }
    public int GiangVienId { get; set; }
}
