using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho TaiKhoanController:
/// - Create AJAX (validation: tên rỗng, email trùng, mật khẩu ngắn, vai trò lạ, thành công)
/// - KhoaTaiKhoan (khóa bản thân, không tồn tại, toggle thành công)
/// - ResetMatKhau (không tồn tại, thành công)
/// - SuaVaiTro (bản thân, vai trò lạ, đã có vai trò, thành công)
/// </summary>
public class TaiKhoanControllerTests
{
    // ─── Factory helpers ──────────────────────────────────────────────────────────

    private static IConfiguration MakeConfig(string defaultPw = "Abc@12345")
    {
        var dict = new Dictionary<string, string?> { ["AppSettings:DefaultPassword"] = defaultPw };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static TaiKhoanController MakeAdminController(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        int adminId = 1)
    {
        var ctrl = new TaiKhoanController(db, MakeConfig());
        var adminUser = ControllerHelper.CreateUser(adminId, "admin@test.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(adminUser);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static NguoiDung SeedUser(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        int id, string email, string role = "HocVien", bool isActive = true)
    {
        var nd = new NguoiDung
        {
            Id = id, Email = email, HoTen = $"User {id}", VaiTro = role,
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Test@123"), IsActive = isActive
        };
        db.NguoiDungs.Add(nd);
        db.SaveChanges();
        return nd;
    }

    // ─── Create AJAX ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_EmptyHoTen_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("", "a@b.com", "HocVien", "Pass@1", "Pass@1");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_InvalidEmail_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("Nguyễn A", "notanemail", "HocVien", "Pass@1", "Pass@1");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_ShortPassword_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("User A", "a@b.com", "HocVien", "123", "123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_PasswordMismatch_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("User A", "a@b.com", "HocVien", "Pass@123", "Pass@456");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_InvalidVaiTro_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("User A", "a@b.com", "SuperUser", "Pass@123", "Pass@123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_DuplicateEmail_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 10, "exists@test.com");
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("New User", "exists@test.com", "HocVien", "Pass@123", "Pass@123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task Create_ValidHocVien_CreatesUserAndHocVienRecord()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("Nguyễn Văn B", "nvb@test.com", "HocVien", "Pass@123", "Pass@123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Single(db.NguoiDungs);
        Assert.Single(db.HocViens);
    }

    [Fact]
    public async Task Create_ValidGiangVien_CreatesUserAndGiangVienRecord()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("Trần GV", "tgv@test.com", "GiangVien", "Pass@123", "Pass@123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Single(db.GiangViens);
    }

    [Fact]
    public async Task Create_ValidAdmin_CreatesUserOnly()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.Create("New Admin", "nadmin@test.com", "Admin", "Pass@123", "Pass@123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Single(db.NguoiDungs);
        Assert.Empty(db.HocViens);
        Assert.Empty(db.GiangViens);
    }

    // ─── KhoaTaiKhoan ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task KhoaTaiKhoan_SelfLock_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        // Admin ID = 1, locking own account
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.KhoaTaiKhoan(1);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task KhoaTaiKhoan_NotFound_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.KhoaTaiKhoan(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task KhoaTaiKhoan_ActiveUser_TogglesInactive()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 5, "target@test.com", isActive: true);
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.KhoaTaiKhoan(5);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.False((bool)GetProp(json.Value!, "isActive")!);
    }

    [Fact]
    public async Task KhoaTaiKhoan_InactiveUser_TogglesActive()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 6, "inactive@test.com", isActive: false);
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.KhoaTaiKhoan(6);

        var json = Assert.IsType<JsonResult>(result);
        Assert.True((bool)GetProp(json.Value!, "isActive")!);
    }

    // ─── ResetMatKhau ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetMatKhau_NotFound_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db);

        var result = await ctrl.ResetMatKhau(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task ResetMatKhau_ValidUser_ChangesPasswordHash()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 7, "reset@test.com");
        var ctrl = MakeAdminController(db);

        var result = await ctrl.ResetMatKhau(7);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));

        var updatedUser = await db.NguoiDungs.FindAsync(7);
        Assert.True(BCrypt.Net.BCrypt.Verify("Abc@12345", updatedUser!.MatKhauHash));
    }

    // ─── SuaVaiTro ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task SuaVaiTro_SelfChange_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.SuaVaiTro(1, "GiangVien");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task SuaVaiTro_InvalidRole_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 8, "user8@test.com", "HocVien");
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.SuaVaiTro(8, "SuperAdmin");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task SuaVaiTro_SameRole_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 9, "user9@test.com", "HocVien");
        db.HocViens.Add(new HocVien { Id = 9, MaHocVien = "HV009", HoTen = "User 9" });
        await db.SaveChangesAsync();
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.SuaVaiTro(9, "HocVien"); // same role

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task SuaVaiTro_HocVienToGiangVien_ChangesRoleAndCreatesRecord()
    {
        using var db = DbContextFactory.Create();
        SeedUser(db, 11, "hv11@test.com", "HocVien");
        db.HocViens.Add(new HocVien { Id = 11, MaHocVien = "HV011", HoTen = "User 11" });
        await db.SaveChangesAsync();
        var ctrl = MakeAdminController(db, adminId: 1);

        var result = await ctrl.SuaVaiTro(11, "GiangVien");

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));

        var updated = await db.NguoiDungs.FindAsync(11);
        Assert.Equal("GiangVien", updated!.VaiTro);
        Assert.True(db.GiangViens.Any(gv => gv.Id == 11));
        Assert.False(db.HocViens.Any(hv => hv.Id == 11));
    }

    // ─── Utility ─────────────────────────────────────────────────────────────────

    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
