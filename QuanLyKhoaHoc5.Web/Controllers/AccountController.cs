using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToRole();
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.NguoiDungs
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.MatKhau, user.MatKhauHash))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.HoTen),
            new(ClaimTypes.Role, user.VaiTro),
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = model.GhiNho,
            ExpiresUtc = model.GhiNho ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("CookieAuth", principal, props);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToRole(user.VaiTro);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.NguoiDungs.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(model.MatKhauCu, user.MatKhauHash))
        {
            ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không đúng");
            return View(model);
        }

        user.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
        user.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Index", "Profile");
    }

    private IActionResult RedirectToRole(string? role = null)
    {
        role ??= User.FindFirstValue(ClaimTypes.Role);
        return role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "GiangVien" => RedirectToAction("Dashboard", "GiangVien"),
            "HocVien" => RedirectToAction("Dashboard", "HocVien"),
            _ => RedirectToAction("Login")
        };
    }
}
