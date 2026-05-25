using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Helpers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;

namespace QuanLyKhoaHoc5.Web.Controllers;

[AuthorizeRole("Admin", "GiangVien")]
public class LopHocController : Controller
{
    private readonly AppDbContext _db;

    public LopHocController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search, string? trangThai)
    {
        var query = _db.LopHocs
            .Include(l => l.KhoaHoc)
            .Include(l => l.GiangVien)
            .Include(l => l.DangKys)
            .AsQueryable();

        if (User.IsInRole("GiangVien"))
        {
            var gvId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            query = query.Where(l => l.GiangVienId == gvId);
        }

        search = search?.Trim();
        trangThai = trangThai?.Trim();

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(l => l.TenLop.ToLower().Contains(s) || l.KhoaHoc.TenKhoaHoc.ToLower().Contains(s));
        }
        if (!string.IsNullOrEmpty(trangThai))
            query = query.Where(l => l.TrangThai == trangThai);

        ViewBag.Search = search;
        ViewBag.TrangThai = trangThai;

        var list = await query.OrderByDescending(l => l.NgayTao)
            .Select(l => new LopHocListViewModel
            {
                Id = l.Id,
                TenLop = l.TenLop,
                TenKhoaHoc = l.KhoaHoc.TenKhoaHoc,
                NgonNgu = l.KhoaHoc.NgonNgu,
                TenGiangVien = l.GiangVien != null ? l.GiangVien.HoTen : null,
                NgayKhaiGiang = l.NgayKhaiGiang,
                NgayKetThuc = l.NgayKetThuc,
                SiSoToiDa = l.SiSoToiDa,
                SiSoHienTai = l.DangKys.Count(d => d.TrangThai == "DaDuyet"),
                PhongHoc = l.PhongHoc,
                TrangThai = l.TrangThai
            }).ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var lop = await _db.LopHocs
            .Include(l => l.KhoaHoc).ThenInclude(k => k.PhanCongs).ThenInclude(p => p.GiangVien)
            .Include(l => l.GiangVien)
            .Include(l => l.LichHocs)
            .Include(l => l.DangKys).ThenInclude(d => d.HocVien).ThenInclude(h => h.NguoiDung)
            .Include(l => l.DangKys).ThenInclude(d => d.Diem)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lop == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var tenGiangVien = lop.GiangVien?.HoTen
            ?? lop.KhoaHoc.PhanCongs.FirstOrDefault(p => p.IsActive)?.GiangVien?.HoTen;

        var vm = new LopHocDetailsViewModel
        {
            Id = lop.Id, TenLop = lop.TenLop,
            TenKhoaHoc = lop.KhoaHoc.TenKhoaHoc,
            NgonNgu = lop.KhoaHoc.NgonNgu, TrinhDo = lop.KhoaHoc.TrinhDo,
            TenGiangVien = lop.GiangVien?.HoTen,
            NgayKhaiGiang = lop.NgayKhaiGiang, NgayKetThuc = lop.NgayKetThuc,
            SiSoToiDa = lop.SiSoToiDa, PhongHoc = lop.PhongHoc,
            TrangThai = lop.TrangThai, GhiChu = lop.GhiChu,
            DanhSachHocVien = lop.DangKys.Select(d => new HocVienTrongLopViewModel
            {
                HocVienId = d.HocVienId, DangKyId = d.Id,
                MaHocVien = d.HocVien.MaHocVien, HoTen = d.HocVien.HoTen,
                Email = d.HocVien.NguoiDung.Email,
                TrangThaiDangKy = d.TrangThai, NgayDangKy = d.NgayDangKy,
                DiemTongKet = d.Diem?.DiemTongKet, XepLoai = d.Diem?.XepLoai
            }).ToList(),
            LichHocs = lop.LichHocs.OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
                .Select(l => new LichHocItemViewModel
                {
                    Id = l.Id, NgayHoc = l.NgayHoc,
                    GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc,
                    PhongHoc = l.PhongHoc, ChuDe = l.ChuDe
                }).ToList()
        };

        var tatCaBuoiHoc = lop.LichHocs
            .Select(l => new BuoiHocViewModel
            {
                NgayHoc = l.NgayHoc, KhoaHocId = lop.KhoaHocId,
                TenKhoaHoc = lop.KhoaHoc.TenKhoaHoc, TenGiangVien = tenGiangVien,
                GioBatDau = l.GioBatDau, GioKetThuc = l.GioKetThuc,
                PhongHoc = l.PhongHoc ?? lop.PhongHoc
            })
            .OrderBy(b => b.NgayHoc).ThenBy(b => b.GioBatDau)
            .ToList();
        ViewBag.TatCaBuoiHoc = tatCaBuoiHoc;
        ViewBag.Today = today;

        return View(vm);
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadSelectListsAsync();
        return View(new LopHocCreateEditViewModel());
    }

    [AuthorizeRole("Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LopHocCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadSelectListsAsync(); return View(vm); }

        _db.LopHocs.Add(new LopHoc
        {
            TenLop = vm.TenLop, KhoaHocId = vm.KhoaHocId,
            GiangVienId = vm.GiangVienId, NgayKhaiGiang = vm.NgayKhaiGiang,
            NgayKetThuc = vm.NgayKetThuc, SiSoToiDa = vm.SiSoToiDa,
            PhongHoc = vm.PhongHoc, TrangThai = vm.TrangThai, GhiChu = vm.GhiChu
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm lớp học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var l = await _db.LopHocs.FindAsync(id);
        if (l == null) return NotFound();
        await LoadSelectListsAsync();
        return View(new LopHocCreateEditViewModel
        {
            Id = l.Id, TenLop = l.TenLop, KhoaHocId = l.KhoaHocId,
            GiangVienId = l.GiangVienId, NgayKhaiGiang = l.NgayKhaiGiang,
            NgayKetThuc = l.NgayKetThuc, SiSoToiDa = l.SiSoToiDa,
            PhongHoc = l.PhongHoc, TrangThai = l.TrangThai, GhiChu = l.GhiChu
        });
    }

    [AuthorizeRole("Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LopHocCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadSelectListsAsync(); return View(vm); }
        var l = await _db.LopHocs.FindAsync(id);
        if (l == null) return NotFound();

        l.TenLop = vm.TenLop; l.KhoaHocId = vm.KhoaHocId;
        l.GiangVienId = vm.GiangVienId; l.NgayKhaiGiang = vm.NgayKhaiGiang;
        l.NgayKetThuc = vm.NgayKetThuc; l.SiSoToiDa = vm.SiSoToiDa;
        l.PhongHoc = vm.PhongHoc; l.TrangThai = vm.TrangThai; l.GhiChu = vm.GhiChu;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật lớp học thành công!";
        return RedirectToAction(nameof(Index));
    }

    [AuthorizeRole("Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var l = await _db.LopHocs.Include(x => x.DangKys).FirstOrDefaultAsync(x => x.Id == id);
        if (l == null) return NotFound();
        if (l.DangKys.Any())
        {
            TempData["Error"] = "Không thể xóa lớp đã có học viên đăng ký!";
            return RedirectToAction(nameof(Index));
        }
        _db.LopHocs.Remove(l);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Xóa lớp học thành công!";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.KhoaHocList = new SelectList(
            await _db.KhoaHocs.Where(k => k.TrangThai != "DaDong").ToListAsync(),
            "Id", "TenKhoaHoc");
        ViewBag.GiangVienList = new SelectList(
            await _db.GiangViens.Include(g => g.NguoiDung)
                .Where(g => g.NguoiDung.IsActive).ToListAsync(),
            "Id", "HoTen");
    }
}
