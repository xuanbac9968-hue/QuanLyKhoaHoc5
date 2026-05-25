using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho KhoaHocController:
/// - ChangeStatus (rotation DangMo→DaDong→TamDung→DangMo, NotFound)
/// - Delete (có/không có LopHoc, NotFound)
/// - Edit GET (valid, NotFound)
/// - Create POST (invalid model)
/// </summary>
public class KhoaHocControllerTests
{
    // ─── Factory helpers ──────────────────────────────────────────────────────────

    private static Mock<IWebHostEnvironment> MockEnv()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return env;
    }

    private static KhoaHoc SeedKhoaHoc(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        string tenKhoaHoc = "Tiếng Anh Sơ cấp",
        string trangThai = "DangMo")
    {
        var kh = new KhoaHoc
        {
            TenKhoaHoc = tenKhoaHoc,
            NgonNgu = "Tiếng Anh",
            TrinhDo = "Sơ cấp",
            HocPhi = 2_000_000,
            ThoiLuong = 20,
            TrangThai = trangThai
        };
        db.KhoaHocs.Add(kh);
        db.SaveChanges();
        return kh;
    }

    // ─── ChangeStatus ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeStatus_DangMo_ChangesToDaDong()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, trangThai: "DangMo");

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.ChangeStatus(kh.Id);

        var json = Assert.IsType<JsonResult>(result);
        dynamic obj = json.Value!;
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Equal("DaDong", GetProp(json.Value!, "newStatus"));
    }

    [Fact]
    public async Task ChangeStatus_DaDong_ChangesToTamDung()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, trangThai: "DaDong");

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.ChangeStatus(kh.Id);

        Assert.Equal("TamDung", GetProp(((JsonResult)result).Value!, "newStatus"));
    }

    [Fact]
    public async Task ChangeStatus_TamDung_ChangesToDangMo()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, trangThai: "TamDung");

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.ChangeStatus(kh.Id);

        Assert.Equal("DangMo", GetProp(((JsonResult)result).Value!, "newStatus"));
    }

    [Fact]
    public async Task ChangeStatus_InvalidId_ReturnsSuccessFalse()
    {
        using var db = DbContextFactory.Create();

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.ChangeStatus(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task ChangeStatus_PersistsNewStatusToDb()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, trangThai: "DangMo");

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        await ctrl.ChangeStatus(kh.Id);

        var updated = await db.KhoaHocs.FindAsync(kh.Id);
        Assert.Equal("DaDong", updated!.TrangThai);
    }

    // ─── Delete ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_NoLopHoc_DeletesAndRedirectsToIndex()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Delete(kh.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Empty(db.KhoaHocs);
    }

    [Fact]
    public async Task Delete_WithLopHoc_SetsErrorAndDoesNotDelete()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        db.LopHocs.Add(new LopHoc
        {
            TenLop = "Lớp A1",
            KhoaHocId = kh.Id,
            TrangThai = "DangTuyenSinh"
        });
        await db.SaveChangesAsync();

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Delete(kh.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.KhoaHocs); // NOT deleted
        Assert.Contains("Error", ctrl.TempData.Keys);
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Delete(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ─── Edit GET ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EditGet_ValidId_ReturnsViewWithModel()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, tenKhoaHoc: "Tiếng Nhật N5");

        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Edit(kh.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<KhoaHocCreateEditViewModel>(view.Model);
        Assert.Equal("Tiếng Nhật N5", model.TenKhoaHoc);
    }

    [Fact]
    public async Task EditGet_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ─── Create GET ───────────────────────────────────────────────────────────────

    [Fact]
    public void CreateGet_ReturnsViewWithEmptyModel()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();

        var result = ctrl.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<KhoaHocCreateEditViewModel>(view.Model);
    }

    // ─── Create POST ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext();
        ctrl.ModelState.AddModelError("TenKhoaHoc", "Required");

        var result = await ctrl.Create(new KhoaHocCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.KhoaHocs);
    }

    // ─── Utility ─────────────────────────────────────────────────────────────────

    /// Đọc property từ anonymous object (giá trị trả về của JsonResult.Value)
    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
