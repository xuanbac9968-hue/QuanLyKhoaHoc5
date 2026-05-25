using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Bổ sung unit test cho các controller chưa được kiểm thử:
/// GiangVienController, HocVienController, DangKyController (Index/CuaToi),
/// ThanhToanController, ThongBaoController, PhanCongGVController.
/// </summary>
public class MoreControllerTests
{
    // ─── Config & Helpers ──────────────────────────────────────────────────────

    private static IConfiguration MakeConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:DefaultPassword"] = "Abc@12345" })
            .Build();

    private static (NguoiDung nd, GiangVien gv) SeedGiangVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int id, string email)
    {
        var nd = new NguoiDung { Id = id, Email = email, HoTen = $"GV {id}", VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true };
        var gv = new GiangVien { Id = id, MaGiangVien = $"GV{id:D3}", HoTen = $"GV {id}", ChuyenMon = "IELTS" };
        db.NguoiDungs.Add(nd); db.GiangViens.Add(gv);
        db.SaveChanges();
        return (nd, gv);
    }

    private static (NguoiDung nd, HocVien hv) SeedHocVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int id, string email)
    {
        var nd = new NguoiDung { Id = id, Email = email, HoTen = $"HV {id}", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        var hv = new HocVien { Id = id, MaHocVien = $"HV{id:D3}", HoTen = $"HV {id}" };
        db.NguoiDungs.Add(nd); db.HocViens.Add(hv);
        db.SaveChanges();
        return (nd, hv);
    }

    private static GiangVienController MakeGiangVienAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new GiangVienController(db, MakeConfig());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static GiangVienController MakeGiangVienCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int gvId)
    {
        var ctrl = new GiangVienController(db, MakeConfig());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(gvId, $"gv{gvId}@t.com", $"GV {gvId}", "GiangVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GiangVienController
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GiangVien_Index_Admin_ReturnsViewWithList()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 10, "gv10@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Index(null, null);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task GiangVien_Index_FilterBySearch_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 11, "alice@t.com");
        SeedGiangVien(db, 12, "bob@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Index("alice", null);

        var view = Assert.IsType<ViewResult>(result);
        var list = view.Model as System.Collections.IEnumerable;
        Assert.Single(list!.Cast<object>());
    }

    [Fact]
    public async Task GiangVien_Details_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 13, "gv13@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Details(13);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task GiangVien_Details_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Details(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GiangVien_CreateGet_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = ctrl.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<GiangVienCreateEditViewModel>(view.Model);
    }

    [Fact]
    public async Task GiangVien_CreatePost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);
        ctrl.ModelState.AddModelError("HoTen", "Required");

        var result = await ctrl.Create(new GiangVienCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.GiangViens);
    }

    [Fact]
    public async Task GiangVien_CreatePost_DuplicateEmail_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 20, "dup@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Create(new GiangVienCreateEditViewModel
            { HoTen = "New GV", Email = "dup@t.com" });

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task GiangVien_CreatePost_Valid_CreatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Create(new GiangVienCreateEditViewModel
        {
            HoTen = "Giảng Viên Mới", Email = "gvmoi@t.com",
            ChuyenMon = "TOEFL", BangCap = "Thạc sĩ", KinhNghiem = 3
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Single(db.GiangViens);
    }

    [Fact]
    public async Task GiangVien_EditGet_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GiangVien_EditGet_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 14, "gv14@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Edit(14);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<GiangVienCreateEditViewModel>(view.Model);
    }

    [Fact]
    public async Task GiangVien_EditPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 15, "gv15@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);
        ctrl.ModelState.AddModelError("HoTen", "Required");

        var result = await ctrl.Edit(15, new GiangVienCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task GiangVien_EditPost_Valid_UpdatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 16, "gv16@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Edit(16, new GiangVienCreateEditViewModel
        {
            HoTen = "GV Updated", Email = "gv16@t.com",
            ChuyenMon = "IELTS Updated", KinhNghiem = 7
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        var updated = db.GiangViens.Find(16)!;
        Assert.Equal("GV Updated", updated.HoTen);
    }

    [Fact]
    public async Task GiangVien_Delete_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Delete(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GiangVien_Delete_NoActiveLop_DeactivatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 17, "gv17@t.com");
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Delete(17);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.False(db.NguoiDungs.Find(17)!.IsActive);
    }

    [Fact]
    public async Task GiangVien_Delete_WithActiveLop_SetsErrorAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 18, "gv18@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "Sơ cấp", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        db.LopHocs.Add(new LopHoc { TenLop = "Lớp A", KhoaHocId = kh.Id, GiangVienId = 18, TrangThai = "DangHoc", SiSoToiDa = 10 });
        await db.SaveChangesAsync();
        var ctrl = MakeGiangVienAdminCtrl(db);

        var result = await ctrl.Delete(18);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Error", ctrl.TempData.Keys);
        Assert.True(db.NguoiDungs.Find(18)!.IsActive); // NOT deactivated
    }

    [Fact]
    public async Task GiangVien_LichDay_ValidGV_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 19, "gv19@t.com");
        var ctrl = MakeGiangVienCtrl(db, 19);

        var result = await ctrl.LichDay();

        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // HocVienController
    // ═══════════════════════════════════════════════════════════════════════════

    private static HocVienController MakeHocVienAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var excel = new ExcelService(db);
        var env = new Moq.Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns("wwwroot");
        var ctrl = new HocVienController(db, excel, env.Object, MakeConfig());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task HocVien_Index_Admin_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 30, "hv30@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Index(null, 1);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task HocVien_Index_FilterBySearch_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 31, "hv31@t.com");
        SeedHocVien(db, 32, "hv32@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Index("hv31", 1);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task HocVien_Details_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 33, "hv33@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Details(33);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HocVienDetailsViewModel>(view.Model);
        Assert.Equal("HV 33", model.HoTen);
    }

    [Fact]
    public async Task HocVien_Details_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Details(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void HocVien_CreateGet_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = ctrl.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task HocVien_CreatePost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);
        ctrl.ModelState.AddModelError("HoTen", "Required");

        var result = await ctrl.Create(new HocVienCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.HocViens);
    }

    [Fact]
    public async Task HocVien_CreatePost_DuplicateEmail_ReturnsViewWithError()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 34, "dup@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Create(new HocVienCreateEditViewModel
            { HoTen = "New HV", Email = "dup@t.com" });

        Assert.IsType<ViewResult>(result);
        Assert.False(ctrl.ModelState.IsValid);
    }

    [Fact]
    public async Task HocVien_CreatePost_Valid_CreatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Create(new HocVienCreateEditViewModel
            { HoTen = "Học Viên Mới", Email = "hvmoi@t.com" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Single(db.HocViens);
    }

    [Fact]
    public async Task HocVien_EditGet_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HocVien_EditGet_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 35, "hv35@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Edit(35);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<HocVienCreateEditViewModel>(view.Model);
        Assert.Equal("HV 35", model.HoTen);
    }

    [Fact]
    public async Task HocVien_EditPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 36, "hv36@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);
        ctrl.ModelState.AddModelError("HoTen", "Required");

        var result = await ctrl.Edit(36, new HocVienCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task HocVien_EditPost_Valid_UpdatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 37, "hv37@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Edit(37, new HocVienCreateEditViewModel
        {
            HoTen = "HV Updated", Email = "hv37@t.com",
            TrinhDoHienTai = "Trung cấp"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("HV Updated", db.HocViens.Find(37)!.HoTen);
    }

    [Fact]
    public async Task HocVien_Delete_ValidId_DeactivatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 38, "hv38@t.com");
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Delete(38);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.False(db.NguoiDungs.Find(38)!.IsActive);
    }

    [Fact]
    public async Task HocVien_Delete_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeHocVienAdminCtrl(db);

        var result = await ctrl.Delete(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DangKyController — Index và CuaToi
    // ═══════════════════════════════════════════════════════════════════════════

    private static DangKyController MakeDangKyAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var thongBao = new ThongBaoService(db);
        var ctrl = new DangKyController(db, thongBao);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static DangKyController MakeDangKyHocVienCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int hvId)
    {
        var thongBao = new ThongBaoService(db);
        var ctrl = new DangKyController(db, thongBao);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(hvId, $"hv{hvId}@t.com", $"HV {hvId}", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task DangKy_Index_EmptyDb_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDangKyAdminCtrl(db);

        var result = await ctrl.Index(new DangKyFilterViewModel());

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DangKy_Index_FilterByTrangThai_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 40, "hv40@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp A", KhoaHocId = kh.Id, TrangThai = "DangTuyenSinh", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 40, LopHocId = lop.Id, TrangThai = "ChoDuyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeDangKyAdminCtrl(db);
        var filter = new DangKyFilterViewModel { TrangThai = "ChoDuyet" };
        var result = await ctrl.Index(filter);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DangKyFilterViewModel>(view.Model);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task DangKy_CuaToi_HocVien_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 41, "hv41@t.com");
        var ctrl = MakeDangKyHocVienCtrl(db, 41);

        var result = await ctrl.CuaToi();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DangKy_CuaToi_WithDangKy_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 42, "hv42@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp B", KhoaHocId = kh.Id, TrangThai = "DangTuyenSinh", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 42, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeDangKyHocVienCtrl(db, 42);
        var result = await ctrl.CuaToi();

        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ThanhToanController
    // ═══════════════════════════════════════════════════════════════════════════

    private static ThanhToanController MakeThanhToanHVCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int hvId)
    {
        var ctrl = new ThanhToanController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(hvId, $"hv{hvId}@t.com", $"HV {hvId}", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static ThanhToanController MakeThanhToanAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new ThanhToanController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task ThanhToan_CuaToi_NoDangKy_ReturnsEmptyView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 50, "hv50@t.com");
        var ctrl = MakeThanhToanHVCtrl(db, 50);

        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Empty(model.Cast<object>());
    }

    [Fact]
    public async Task ThanhToan_CuaToi_WithDangKy_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 51, "hv51@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20, HocPhi = 3_000_000m };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp C", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 51, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeThanhToanHVCtrl(db, 51);
        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(model.Cast<object>());
    }

    [Fact]
    public async Task ThanhToan_TaoYeuCauGet_NotEnrolled_RedirectsToCuaToi()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 52, "hv52@t.com");
        var ctrl = MakeThanhToanHVCtrl(db, 52);

        var result = await ctrl.TaoYeuCau(9999);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ThanhToanController.CuaToi), redirect.ActionName);
        Assert.Contains("Error", ctrl.TempData.Keys);
    }

    [Fact]
    public async Task ThanhToan_TaoYeuCauGet_AlreadyPending_Redirects()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 53, "hv53@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20, HocPhi = 2_000_000m };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp D", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 53, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        db.ThanhToans.Add(new ThanhToan { HocVienId = 53, KhoaHocId = kh.Id, SoTien = 2_000_000m, TrangThai = "ChoPheduyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeThanhToanHVCtrl(db, 53);
        var result = await ctrl.TaoYeuCau(kh.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Warning", ctrl.TempData.Keys);
    }

    [Fact]
    public async Task ThanhToan_TaoYeuCauGet_Valid_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 54, "hv54@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH E", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20, HocPhi = 2_500_000m };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp E", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 54, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        await db.SaveChangesAsync();

        var ctrl = MakeThanhToanHVCtrl(db, 54);
        var result = await ctrl.TaoYeuCau(kh.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ThanhToanCreateViewModel>(view.Model);
    }

    [Fact]
    public async Task ThanhToan_TaoYeuCauPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 55, "hv55@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH F", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20, HocPhi = 1_000_000m };
        db.KhoaHocs.Add(kh); db.SaveChanges();

        var ctrl = MakeThanhToanHVCtrl(db, 55);
        ctrl.ModelState.AddModelError("PhuongThuc", "Required");

        var result = await ctrl.TaoYeuCau(new ThanhToanCreateViewModel { KhoaHocId = kh.Id });

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ThanhToan_TaoYeuCauPost_Valid_CreatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 56, "hv56@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH G", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20, HocPhi = 1_500_000m };
        db.KhoaHocs.Add(kh); db.SaveChanges();

        var ctrl = MakeThanhToanHVCtrl(db, 56);
        var result = await ctrl.TaoYeuCau(new ThanhToanCreateViewModel
        {
            KhoaHocId = kh.Id, PhuongThuc = "TienMat"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ThanhToanController.CuaToi), redirect.ActionName);
        Assert.Single(db.ThanhToans);
    }

    [Fact]
    public async Task ThanhToan_Index_Admin_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeThanhToanAdminCtrl(db);

        var result = await ctrl.Index(null, 1);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task ThanhToan_ChiTiet_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeThanhToanAdminCtrl(db);

        var result = await ctrl.ChiTiet(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ThanhToan_Duyet_DaThanhToan_RedirectsWithSuccess()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 57, "hv57@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH H", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var tt = new ThanhToan { HocVienId = 57, KhoaHocId = kh.Id, SoTien = 1_000_000m, TrangThai = "ChoPheduyet" };
        db.ThanhToans.Add(tt); await db.SaveChangesAsync();

        var ctrl = MakeThanhToanAdminCtrl(db);
        var result = await ctrl.Duyet(new ThanhToanDuyetViewModel { Id = tt.Id, HanhDong = "DaThanhToan" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("DaThanhToan", db.ThanhToans.Find(tt.Id)!.TrangThai);
    }

    [Fact]
    public async Task ThanhToan_ThongKe6Thang_ReturnsJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeThanhToanAdminCtrl(db);

        var result = await ctrl.ThongKe6Thang();

        Assert.IsType<JsonResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ThongBaoController
    // ═══════════════════════════════════════════════════════════════════════════

    private static ThongBaoController MakeThongBaoCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int userId, string role = "HocVien")
    {
        var ctrl = new ThongBaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(userId, $"u{userId}@t.com", $"User {userId}", role));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task ThongBao_Index_NoNotifications_ReturnsEmptyView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeThongBaoCtrl(db, 60);

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Empty(list.Cast<object>());
    }

    [Fact]
    public async Task ThongBao_Index_MarksAllAsRead()
    {
        using var db = DbContextFactory.Create();
        db.ThongBaos.Add(new ThongBao
        {
            NguoiNhanId = 61, TieuDe = "Test", NoiDung = "content",
            LoaiThongBao = "HeThong", DaDoc = false
        });
        await db.SaveChangesAsync();
        var ctrl = MakeThongBaoCtrl(db, 61);

        await ctrl.Index();

        Assert.True(db.ThongBaos.First().DaDoc);
    }

    [Fact]
    public async Task ThongBao_DanhDauDaDoc_ValidId_ReturnsOk()
    {
        using var db = DbContextFactory.Create();
        db.ThongBaos.Add(new ThongBao
        {
            Id = 100, NguoiNhanId = 62, TieuDe = "TB", NoiDung = "nd",
            LoaiThongBao = "HeThong", DaDoc = false
        });
        await db.SaveChangesAsync();
        var ctrl = MakeThongBaoCtrl(db, 62);

        var result = await ctrl.DanhDauDaDoc(100);

        Assert.IsType<OkResult>(result);
        Assert.True(db.ThongBaos.Find(100)!.DaDoc);
    }

    [Fact]
    public async Task ThongBao_DanhDauTatCa_MarksAll()
    {
        using var db = DbContextFactory.Create();
        db.ThongBaos.Add(new ThongBao { NguoiNhanId = 63, TieuDe = "TB1", NoiDung = "nd", LoaiThongBao = "HeThong", DaDoc = false });
        db.ThongBaos.Add(new ThongBao { NguoiNhanId = 63, TieuDe = "TB2", NoiDung = "nd", LoaiThongBao = "HeThong", DaDoc = false });
        await db.SaveChangesAsync();
        var ctrl = MakeThongBaoCtrl(db, 63);

        var result = await ctrl.DanhDauTatCa();

        var json = Assert.IsType<JsonResult>(result);
        Assert.True(db.ThongBaos.Where(t => t.NguoiNhanId == 63).All(t => t.DaDoc));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PhanCongGVController
    // ═══════════════════════════════════════════════════════════════════════════

    private static PhanCongGVController MakePhanCongCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new PhanCongGVController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task PhanCongGV_Index_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task PhanCongGV_PhanCong_InvalidLop_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.PhanCong(new PhanCongRequest { LopHocId = 99999, GiangVienId = 1 });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task PhanCongGV_PhanCong_InvalidGV_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp A", KhoaHocId = kh.Id, TrangThai = "DangTuyenSinh", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.PhanCong(new PhanCongRequest { LopHocId = lop.Id, GiangVienId = 99999 });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task PhanCongGV_PhanCong_Valid_AssignsAndReturnsSuccess()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 70, "gv70@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp F", KhoaHocId = kh.Id, TrangThai = "DangTuyenSinh", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.PhanCong(new PhanCongRequest { LopHocId = lop.Id, GiangVienId = 70 });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Equal(70, db.LopHocs.Find(lop.Id)!.GiangVienId);
    }

    [Fact]
    public async Task PhanCongGV_HuyPhanCong_InvalidLop_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.HuyPhanCong(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task PhanCongGV_HuyPhanCong_Valid_ClearsGiangVien()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 71, "gv71@t.com");
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp G", KhoaHocId = kh.Id, GiangVienId = 71, TrangThai = "DangTuyenSinh", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakePhanCongCtrl(db);

        var result = await ctrl.HuyPhanCong(lop.Id);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Null(db.LopHocs.Find(lop.Id)!.GiangVienId);
    }

    // ─── Utility ─────────────────────────────────────────────────────────────

    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
