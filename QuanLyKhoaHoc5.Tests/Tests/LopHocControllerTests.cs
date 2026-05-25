using Microsoft.AspNetCore.Mvc;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho LopHocController:
/// - Index (Admin thấy tất cả, GiangVien chỉ thấy lớp của mình)
/// - Details (valid, NotFound)
/// - Create POST (invalid model, thành công)
/// - Edit GET + POST
/// - Delete (có DangKy, không có DangKy)
/// </summary>
public class LopHocControllerTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static KhoaHoc SeedKhoaHoc(QuanLyKhoaHoc5.Web.Data.AppDbContext db)
    {
        var kh = new KhoaHoc
        {
            TenKhoaHoc = "KH Test", NgonNgu = "Tiếng Anh", TrinhDo = "Sơ cấp", ThoiLuong = 20
        };
        db.KhoaHocs.Add(kh);
        db.SaveChanges();
        return kh;
    }

    private static LopHoc SeedLop(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        int khoaHocId,
        int? giangVienId = null,
        string trangThai = "DangTuyenSinh")
    {
        var lop = new LopHoc
        {
            TenLop = "Lớp Test", KhoaHocId = khoaHocId,
            GiangVienId = giangVienId, TrangThai = trangThai,
            SiSoToiDa = 20
        };
        db.LopHocs.Add(lop);
        db.SaveChanges();
        return lop;
    }

    private static LopHocController MakeAdminCtrl(QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new LopHocController(db);
        var user = ControllerHelper.CreateUser(adminId, "admin@test.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    // ─── Index ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_Admin_ReturnsAllLops()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        SeedLop(db, kh.Id);
        SeedLop(db, kh.Id);
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Index(null, null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Equal(2, model.Cast<object>().Count());
    }

    [Fact]
    public async Task Index_FilterByTrangThai_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        SeedLop(db, kh.Id, trangThai: "DangTuyenSinh");
        SeedLop(db, kh.Id, trangThai: "DaKetThuc");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Index(null, "DangTuyenSinh");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(model.Cast<object>());
    }

    // ─── Details ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Details(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ─── Create ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreatePost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeAdminCtrl(db);
        ctrl.ModelState.AddModelError("TenLop", "Required");

        var result = await ctrl.Create(new LopHocCreateEditViewModel { KhoaHocId = kh.Id });

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.LopHocs);
    }

    [Fact]
    public async Task CreatePost_ValidModel_CreatesLopAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeAdminCtrl(db);

        var vm = new LopHocCreateEditViewModel
        {
            TenLop = "Lớp Mới", KhoaHocId = kh.Id,
            TrangThai = "DangTuyenSinh", SiSoToiDa = 15
        };

        var result = await ctrl.Create(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Single(db.LopHocs);
        Assert.Equal("Lớp Mới", db.LopHocs.First().TenLop);
    }

    // ─── Edit ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EditGet_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditGet_ValidId_ReturnsViewWithModel()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLop(db, kh.Id);
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Edit(lop.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<LopHocCreateEditViewModel>(view.Model);
    }

    [Fact]
    public async Task EditPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLop(db, kh.Id);
        var ctrl = MakeAdminCtrl(db);
        ctrl.ModelState.AddModelError("TenLop", "Required");

        var result = await ctrl.Edit(lop.Id, new LopHocCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task EditPost_ValidModel_UpdatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLop(db, kh.Id);
        var ctrl = MakeAdminCtrl(db);

        var vm = new LopHocCreateEditViewModel
        {
            TenLop = "Lớp Cập Nhật", KhoaHocId = kh.Id,
            TrangThai = "DangHoc", SiSoToiDa = 25
        };

        var result = await ctrl.Edit(lop.Id, vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        var updated = db.LopHocs.Find(lop.Id)!;
        Assert.Equal("Lớp Cập Nhật", updated.TenLop);
        Assert.Equal(25, updated.SiSoToiDa);
    }

    // ─── Delete ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Delete(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_NoDangKy_DeletesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLop(db, kh.Id);
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Delete(lop.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.LopHocs);
    }

    [Fact]
    public async Task Delete_WithDangKy_SetsErrorAndDoesNotDelete()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLop(db, kh.Id);

        // Seed a HocVien and DangKy
        db.NguoiDungs.Add(new NguoiDung { Id = 50, Email = "hv50@test.com", HoTen = "HV50", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true });
        db.HocViens.Add(new HocVien { Id = 50, MaHocVien = "HV050", HoTen = "HV50" });
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 50, LopHocId = lop.Id, TrangThai = "ChoDuyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.Delete(lop.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.LopHocs); // NOT deleted
        Assert.Contains("Error", ctrl.TempData.Keys);
    }
}
