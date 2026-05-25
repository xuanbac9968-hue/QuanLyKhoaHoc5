using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using System.Security.Claims;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho AccountController:
/// - Login GET / POST (valid, wrong password, locked, non-existent)
/// - Logout
/// - AccessDenied
/// - ChangePassword POST
/// </summary>
public class AccountControllerTests
{
    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static NguoiDung MakeUser(int id, string email, string role, bool isActive = true)
        => new NguoiDung
        {
            Id = id,
            Email = email,
            HoTen = $"User {id}",
            VaiTro = role,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = isActive
        };

    // ─── Login GET ────────────────────────────────────────────────────────────────

    [Fact]
    public void LoginGet_WhenNotAuthenticated_ReturnsViewResult()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(); // anonymous

        var result = ctrl.Login((string?)null);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void LoginGet_WhenAuthenticated_RedirectsToRole()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        var user = ControllerHelper.CreateUser(1, "admin@test.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = ctrl.Login((string?)null);

        // Authenticated → redirect to Admin/Index
        Assert.IsType<RedirectToActionResult>(result);
    }

    // ─── Login POST ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginPost_InvalidModelState_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();
        ctrl.ModelState.AddModelError("Email", "Required");

        var result = await ctrl.Login(new LoginViewModel { Email = "", MatKhau = "" });

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task LoginPost_EmailNotFound_AddsModelErrorAndReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "nobody@test.com",
            MatKhau = "Test@123"
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task LoginPost_WrongPassword_AddsModelErrorAndReturnsView()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(1, "user@test.com", "HocVien"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "user@test.com",
            MatKhau = "WrongPass!"
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task LoginPost_LockedAccount_AddsModelErrorAndReturnsView()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(2, "locked@test.com", "HocVien", isActive: false));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "locked@test.com",
            MatKhau = "Test@123"
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task LoginPost_ValidAdmin_RedirectsToAdminIndex()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(3, "admin@test.com", "Admin"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "admin@test.com",
            MatKhau = "Test@123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);
    }

    [Fact]
    public async Task LoginPost_ValidGiangVien_RedirectsToDashboard()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(4, "gv@test.com", "GiangVien"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "gv@test.com",
            MatKhau = "Test@123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", redirect.ActionName);
        Assert.Equal("GiangVien", redirect.ControllerName);
    }

    [Fact]
    public async Task LoginPost_ValidHocVien_RedirectsToDashboard()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(5, "hv@test.com", "HocVien"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Login(new LoginViewModel
        {
            Email = "hv@test.com",
            MatKhau = "Test@123"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", redirect.ActionName);
        Assert.Equal("HocVien", redirect.ControllerName);
    }

    // ─── Logout ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_RedirectsToLogin()
    {
        using var db = DbContextFactory.Create();
        var mockAuth = new Mock<IAuthenticationService>();
        mockAuth
            .Setup(a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string?>(),
                It.IsAny<AuthenticationProperties?>()))
            .Returns(Task.CompletedTask);

        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(authService: mockAuth);

        var result = await ctrl.Logout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }

    // ─── AccessDenied ────────────────────────────────────────────────────────────

    [Fact]
    public void AccessDenied_ReturnsViewResult()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = ctrl.AccessDenied();

        Assert.IsType<ViewResult>(result);
    }

    // ─── ChangePassword GET ───────────────────────────────────────────────────────

    [Fact]
    public void ChangePasswordGet_ReturnsViewWithModel()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = ctrl.ChangePassword();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ChangePasswordViewModel>(view.Model);
    }

    // ─── ChangePassword POST ──────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new AccountController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext();
        ctrl.ModelState.AddModelError("MatKhauCu", "Required");

        var result = await ctrl.ChangePassword(new ChangePasswordViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ChangePasswordPost_WrongCurrentPassword_AddsModelError()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(10, "me@test.com", "HocVien"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        var user = ControllerHelper.CreateUser(10, "me@test.com", "Me", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.ChangePassword(new ChangePasswordViewModel
        {
            MatKhauCu = "WrongOld!",
            MatKhauMoi = "NewPass@123",
            XacNhanMatKhau = "NewPass@123"
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task ChangePasswordPost_ValidInput_UpdatesHashAndRedirects()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(MakeUser(11, "me2@test.com", "HocVien"));
        await db.SaveChangesAsync();

        var ctrl = new AccountController(db);
        var user = ControllerHelper.CreateUser(11, "me2@test.com", "Me2", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.ChangePassword(new ChangePasswordViewModel
        {
            MatKhauCu = "Test@123",
            MatKhauMoi = "NewSecure@456",
            XacNhanMatKhau = "NewSecure@456"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Profile", redirect.ControllerName);

        // Verify hash changed
        var updated = await db.NguoiDungs.FindAsync(11);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewSecure@456", updated!.MatKhauHash));
    }
}
