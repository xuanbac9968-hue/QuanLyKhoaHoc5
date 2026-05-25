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
public class DiemController : Controller
{
    private readonly AppDbContext _db;
    private readonly ExcelService _excel;

    public DiemController(AppDbContext db, ExcelService excel) { _db = db; _excel = excel; }

    [AuthorizeRole("Admin", "GiangVien")]
    public async Task<IActionResult> LopHoc(int lopHocId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var lop = await _db.LopHocs.Include(l => l.KhoaHoc).Include(l => l.GiangVien).FirstOrDefaultAsync(l => l.Id == lopHocId);
        if (lop == null) return NotFound();

        if (User.IsInRole("GiangVien") && lop.GiangVienId != userId) return Forbid();

        var diems = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.HocVien)
            .Where(d => d.DangKy.LopHocId == lopHocId && d.DangKy.TrangThai == "DaDuyet")
            .ToListAsync();

        bool isKhoa = diems.Any(d => d.IsKhoa);
        bool canEdit = User.IsInRole("Admin") || (!isKhoa && User.IsInRole("GiangVien") && lop.GiangVienId == userId);

        return View(new BangDiemLopViewModel
        {
            LopHocId = lopHocId, TenLop = lop.TenLop, TenKhoaHoc = lop.KhoaHoc.TenKhoaHoc,
            TenGiangVien = lop.GiangVien?.HoTen, IsKhoa = isKhoa, CanEdit = canEdit,
            DanhSachDiem = diems.Select(d => new NhapDiemViewModel
            {
                DangKyId = d.DangKyId, HocVienId = d.DangKy.HocVienId, DiemId = d.Id,
                MaHocVien = d.DangKy.HocVien.MaHocVien, TenHocVien = d.DangKy.HocVien.HoTen,
                DiemGiuaKy = d.DiemGiuaKy, DiemCuoiKy = d.DiemCuoiKy, DiemTongKet = d.DiemTongKet,
                XepLoai = d.XepLoai, NhanXetGiangVien = d.NhanXetGiangVien, IsKhoa = d.IsKhoa
            }).ToList()
        });
    }

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> CuaToi()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Where(d => d.DangKy.HocVienId == userId)
            .Select(d => new DiemCuaToiViewModel
            {
                TenLop = d.DangKy.LopHoc.TenLop, TenKhoaHoc = d.DangKy.LopHoc.KhoaHoc.TenKhoaHoc,
                NgonNgu = d.DangKy.LopHoc.KhoaHoc.NgonNgu, DiemGiuaKy = d.DiemGiuaKy,
                DiemCuoiKy = d.DiemCuoiKy, DiemTongKet = d.DiemTongKet, XepLoai = d.XepLoai,
                NhanXetGiangVien = d.NhanXetGiangVien, TrangThaiDangKy = d.DangKy.TrangThai
            }).ToListAsync();
        return View(list);
    }

    [AuthorizeRole("Admin", "GiangVien")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapDiem(int dangKyId, double? diemGiuaKy, double? diemCuoiKy, string? nhanXet)
    {
        var diem = await _db.Diems.FirstOrDefaultAsync(d => d.DangKyId == dangKyId);
        if (diem == null) return NotFound();
        if (diem.IsKhoa && !User.IsInRole("Admin"))
            return Json(new { success = false, message = "Điểm đã bị khóa" });

        diem.DiemGiuaKy = diemGiuaKy;
        diem.DiemCuoiKy = diemCuoiKy;

        // Formula: GK * 30% + CK * 70%
        diem.DiemTongKet = Diem.TinhTongKet(diemGiuaKy, diemCuoiKy);
        diem.XepLoai = Diem.TinhXepLoai(diem.DiemTongKet);
        diem.NhanXetGiangVien = nhanXet;
        diem.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();

        return Json(new { success = true, diemTongKet = diem.DiemTongKet?.ToString("F2"), xepLoai = diem.XepLoai });
    }

    [AuthorizeRole("Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaDiem(int lopHocId)
    {
        var diems = await _db.Diems.Where(d => d.DangKy.LopHocId == lopHocId).ToListAsync();
        diems.ForEach(d => d.IsKhoa = true);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã khóa điểm lớp học";
        return RedirectToAction(nameof(LopHoc), new { lopHocId });
    }

    [AuthorizeRole("Admin", "GiangVien")]
    public async Task<IActionResult> ExportExcel(int lopHocId)
    {
        var bytes = await _excel.ExportBangDiemAsync(lopHocId);
        var lopHoc = await _db.LopHocs.FindAsync(lopHocId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BangDiem_{lopHoc?.TenLop}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // Kept for compatibility — delegates to static Diem helper
    private static string? TinhXepLoai(double? diem) => Diem.TinhXepLoai(diem);
}
