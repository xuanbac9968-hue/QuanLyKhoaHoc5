using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db; _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var nd = await _db.NguoiDungs.FindAsync(userId);
        if (nd == null) return NotFound();

        var vm = new ProfileViewModel
        {
            Id = nd.Id, HoTen = nd.HoTen, Email = nd.Email,
            SoDienThoai = nd.SoDienThoai, AnhDaiDien = nd.AnhDaiDien, VaiTro = nd.VaiTro
        };

        if (nd.VaiTro == "HocVien")
        {
            var hv = await _db.HocViens.FindAsync(userId);
            if (hv != null)
            {
                vm.NgaySinh = hv.NgaySinh; vm.GioiTinh = hv.GioiTinh;
                vm.DiaChi = hv.DiaChi; vm.TrinhDoHienTai = hv.TrinhDoHienTai;
                vm.NgonNguQuanTam = hv.NgonNguQuanTam;
            }
        }
        else if (nd.VaiTro == "GiangVien")
        {
            var gv = await _db.GiangViens.FindAsync(userId);
            if (gv != null)
            {
                vm.ChuyenMon = gv.ChuyenMon; vm.BangCap = gv.BangCap;
                vm.KinhNghiem = gv.KinhNghiem; vm.MoTa = gv.MoTa;
            }
        }
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel vm)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var nd = await _db.NguoiDungs.FindAsync(userId);
        if (nd == null) return NotFound();

        nd.HoTen = vm.HoTen; nd.SoDienThoai = vm.SoDienThoai;
        nd.NgayCapNhat = DateTime.Now;

        if (vm.AnhDaiDienFile != null && vm.AnhDaiDienFile.Length > 0)
        {
            var ext = Path.GetExtension(vm.AnhDaiDienFile.FileName).ToLower();
            if (ext is ".jpg" or ".jpeg" or ".png" && vm.AnhDaiDienFile.Length <= 5 * 1024 * 1024)
            {
                var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(folder);
                var fileName = $"{userId}_{Guid.NewGuid()}{ext}";
                var path = Path.Combine(folder, fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await vm.AnhDaiDienFile.CopyToAsync(stream);
                nd.AnhDaiDien = $"/uploads/avatars/{fileName}";
            }
        }

        if (nd.VaiTro == "HocVien")
        {
            var hv = await _db.HocViens.FindAsync(userId);
            if (hv != null)
            {
                hv.HoTen = vm.HoTen; hv.NgaySinh = vm.NgaySinh;
                hv.GioiTinh = vm.GioiTinh; hv.DiaChi = vm.DiaChi;
                hv.TrinhDoHienTai = vm.TrinhDoHienTai; hv.NgonNguQuanTam = vm.NgonNguQuanTam;
            }
        }
        else if (nd.VaiTro == "GiangVien")
        {
            var gv = await _db.GiangViens.FindAsync(userId);
            if (gv != null)
            {
                gv.HoTen = vm.HoTen; gv.ChuyenMon = vm.ChuyenMon;
                gv.BangCap = vm.BangCap; gv.KinhNghiem = vm.KinhNghiem; gv.MoTa = vm.MoTa;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction(nameof(Index));
    }
}
