using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.Entities;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[AuthorizeRole("Admin")]
public class TaiKhoanController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public TaiKhoanController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // GET /TaiKhoan
    public async Task<IActionResult> Index(string? vaiTro, string? search)
    {
        var query = _db.NguoiDungs.AsQueryable();
        if (!string.IsNullOrEmpty(vaiTro)) query = query.Where(u => u.VaiTro == vaiTro);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(u => u.HoTen.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
        }
        ViewBag.VaiTro      = vaiTro;
        ViewBag.Search      = search;
        ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return View(await query.OrderBy(u => u.VaiTro).ThenBy(u => u.HoTen).ToListAsync());
    }

    // GET /TaiKhoan/Create — redirect to Index (modal-based flow)
    [HttpGet]
    public IActionResult Create() => RedirectToAction(nameof(Index));

    // POST /TaiKhoan/Create (AJAX)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string hoTen, string email, string vaiTro,
        string matKhau, string xacNhan)
    {
        // ---------- validation ----------
        if (string.IsNullOrWhiteSpace(hoTen))
            return Json(new { success = false, message = "Họ tên không được để trống." });

        email = (email ?? "").Trim().ToLower();
        if (string.IsNullOrEmpty(email) || !email.Contains('@') || !email.Contains('.'))
            return Json(new { success = false, message = "Email không hợp lệ." });

        if (string.IsNullOrEmpty(matKhau) || matKhau.Length < 6)
            return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự." });

        if (matKhau != xacNhan)
            return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." });

        string[] valid = ["Admin", "GiangVien", "HocVien"];
        if (!valid.Contains(vaiTro))
            return Json(new { success = false, message = "Vai trò không hợp lệ." });

        if (await _db.NguoiDungs.AnyAsync(u => u.Email.ToLower() == email))
            return Json(new { success = false, message = "Email này đã tồn tại trong hệ thống." });

        // ---------- create NguoiDung ----------
        var user = new NguoiDung
        {
            HoTen        = hoTen.Trim(),
            Email        = email,
            MatKhauHash  = BCrypt.Net.BCrypt.HashPassword(matKhau),
            VaiTro       = vaiTro,
            IsActive     = true,
            NgayTao      = DateTime.Now
        };
        _db.NguoiDungs.Add(user);
        await _db.SaveChangesAsync();

        // ---------- create role-specific record ----------
        if (vaiTro == "GiangVien")
        {
            _db.GiangViens.Add(new GiangVien
            {
                Id           = user.Id,
                MaGiangVien  = await GenerateMaGiangVienAsync(),
                HoTen        = user.HoTen
            });
            await _db.SaveChangesAsync();
        }
        else if (vaiTro == "HocVien")
        {
            _db.HocViens.Add(new HocVien
            {
                Id         = user.Id,
                MaHocVien  = await GenerateMaHocVienAsync(),
                HoTen      = user.HoTen
            });
            await _db.SaveChangesAsync();
        }

        var (roleTxt, roleClass) = RoleDisplay(vaiTro);
        return Json(new
        {
            success = true,
            message = $"Đã tạo tài khoản <strong>{user.HoTen}</strong> ({roleTxt}) thành công.",
            user    = new
            {
                id       = user.Id,
                hoTen    = user.HoTen,
                email    = user.Email,
                vaiTro,
                roleTxt,
                roleClass,
                ngayTao  = user.NgayTao.ToString("dd/MM/yyyy")
            }
        });
    }

    // POST /TaiKhoan/KhoaTaiKhoan/{id}  (AJAX — toggle)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaTaiKhoan(int id)
    {
        var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == me)
            return Json(new { success = false, message = "Không thể khóa tài khoản của chính mình." });

        var user = await _db.NguoiDungs.FindAsync(id);
        if (user == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });

        user.IsActive      = !user.IsActive;
        user.NgayCapNhat   = DateTime.Now;
        await _db.SaveChangesAsync();

        var msg = user.IsActive ? "Đã <strong>mở khóa</strong> tài khoản thành công." : "Đã <strong>khóa</strong> tài khoản thành công.";
        return Json(new { success = true, message = msg, isActive = user.IsActive });
    }

    // POST /TaiKhoan/ResetMatKhau/{id}  (AJAX)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetMatKhau(int id)
    {
        var user = await _db.NguoiDungs.FindAsync(id);
        if (user == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });

        var defaultPw = _config["AppSettings:DefaultPassword"] ?? "Abc@12345";
        user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(defaultPw);
        user.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = $"Đã reset về mật khẩu mặc định: <code>{defaultPw}</code>" });
    }

    // POST /TaiKhoan/SuaVaiTro (AJAX)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaVaiTro(int id, string vaiTroMoi)
    {
        var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == me)
            return Json(new { success = false, message = "Không thể thay đổi vai trò của chính mình." });

        string[] valid = ["Admin", "GiangVien", "HocVien"];
        if (!valid.Contains(vaiTroMoi))
            return Json(new { success = false, message = "Vai trò không hợp lệ." });

        var user = await _db.NguoiDungs
            .Include(u => u.HocVien).ThenInclude(hv => hv!.DangKys)
            .Include(u => u.GiangVien).ThenInclude(gv => gv!.LopHocs)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return Json(new { success = false, message = "Tài khoản không tồn tại." });
        if (user.VaiTro == vaiTroMoi)
            return Json(new { success = false, message = "Tài khoản đã có vai trò này rồi." });

        // Ràng buộc dữ liệu liên quan
        if (user.VaiTro == "GiangVien" && user.GiangVien?.LopHocs.Any() == true)
            return Json(new { success = false, message = "Giảng viên đang phụ trách lớp học, không thể đổi vai trò." });

        if (user.VaiTro == "HocVien" && user.HocVien?.DangKys.Any() == true)
            return Json(new { success = false, message = "Học viên đã có đăng ký khóa học, không thể đổi vai trò." });

        // Xóa bản ghi vai trò cũ
        if (user.VaiTro == "GiangVien" && user.GiangVien != null) _db.GiangViens.Remove(user.GiangVien);
        else if (user.VaiTro == "HocVien"  && user.HocVien  != null) _db.HocViens.Remove(user.HocVien);

        user.VaiTro      = vaiTroMoi;
        user.NgayCapNhat = DateTime.Now;

        // Tạo bản ghi vai trò mới
        if (vaiTroMoi == "GiangVien")
            _db.GiangViens.Add(new GiangVien { Id = user.Id, MaGiangVien = await GenerateMaGiangVienAsync(), HoTen = user.HoTen });
        else if (vaiTroMoi == "HocVien")
            _db.HocViens.Add(new HocVien { Id = user.Id, MaHocVien = await GenerateMaHocVienAsync(), HoTen = user.HoTen });

        await _db.SaveChangesAsync();

        var (roleTxt, roleClass) = RoleDisplay(vaiTroMoi);
        return Json(new { success = true, message = $"Đã đổi vai trò thành <strong>{roleTxt}</strong>.", vaiTroMoi, roleTxt, roleClass });
    }

    // ─── helpers ───────────────────────────────────────────────────

    private static (string txt, string cls) RoleDisplay(string vaiTro) => vaiTro switch
    {
        "Admin"      => ("Admin",       "bg-danger"),
        "GiangVien"  => ("Giảng viên",  "bg-primary"),
        _            => ("Học viên",    "bg-success")
    };

    private async Task<string> GenerateMaHocVienAsync()
    {
        var codes = await _db.HocViens.Select(h => h.MaHocVien).ToListAsync();
        var maxNum = codes
            .Where(c => c.Length > 2 && c.StartsWith("HV") && int.TryParse(c.AsSpan(2), out _))
            .Select(c => int.Parse(c[2..]))
            .DefaultIfEmpty(0).Max();
        return $"HV{maxNum + 1:D3}";
    }

    private async Task<string> GenerateMaGiangVienAsync()
    {
        var codes = await _db.GiangViens.Select(g => g.MaGiangVien).ToListAsync();
        var maxNum = codes
            .Where(c => c.Length > 2 && c.StartsWith("GV") && int.TryParse(c.AsSpan(2), out _))
            .Select(c => int.Parse(c[2..]))
            .DefaultIfEmpty(0).Max();
        return $"GV{maxNum + 1:D3}";
    }
}
