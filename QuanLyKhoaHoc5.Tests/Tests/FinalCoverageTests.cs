using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Bổ sung unit test tối đa hoá coverage:
/// KhoaHocController (Index, Details, Edit),
/// TaiKhoanController (Index),
/// AdminController (TaiKhoan, TaoTaiKhoan, KhoaTaiKhoan, DoiRole, PhanCong, DoPhanCong, HuyPhanCong, LichSuPhanCong),
/// DiemController (LopHoc),
/// DiemSoController (Index, CuaToi),
/// ProfileController (Edit - HocVien, GiangVien paths),
/// LichHocController (Index, CuaToi, GetLopStudents, Delete/GetEvents),
/// </summary>
public class FinalCoverageTests
{
    // ─── Shared helpers ───────────────────────────────────────────────────────

    private static IConfiguration MakeConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:DefaultPassword"] = "Abc@12345" })
            .Build();

    private static IWebHostEnvironment MakeEnv()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return env.Object;
    }

    private static KhoaHoc SeedKhoaHoc(QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        string ten = "KH", string trangThai = "DangMo")
    {
        var kh = new KhoaHoc
        {
            TenKhoaHoc = ten, NgonNgu = "Tiếng Anh", TrinhDo = "Sơ cấp",
            ThoiLuong = 20, TrangThai = trangThai, HocPhi = 2_000_000m
        };
        db.KhoaHocs.Add(kh);
        db.SaveChanges();
        return kh;
    }

    private static (NguoiDung nd, HocVien hv) SeedHocVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int id, string email)
    {
        var nd = new NguoiDung { Id = id, Email = email, HoTen = $"HV{id}", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        var hv = new HocVien { Id = id, MaHocVien = $"HV{id:D3}", HoTen = $"HV{id}" };
        db.NguoiDungs.Add(nd); db.HocViens.Add(hv);
        db.SaveChanges();
        return (nd, hv);
    }

    private static (NguoiDung nd, GiangVien gv) SeedGiangVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int id, string email)
    {
        var nd = new NguoiDung { Id = id, Email = email, HoTen = $"GV{id}", VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true };
        var gv = new GiangVien { Id = id, MaGiangVien = $"GV{id:D3}", HoTen = $"GV{id}" };
        db.NguoiDungs.Add(nd); db.GiangViens.Add(gv);
        db.SaveChanges();
        return (nd, gv);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // KhoaHocController — Index, Details, Edit
    // ═══════════════════════════════════════════════════════════════════════════

    private static KhoaHocController MakeKhoaHocAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new KhoaHocController(db, MakeEnv());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static KhoaHocController MakeKhoaHocAnonCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db)
    {
        var ctrl = new KhoaHocController(db, MakeEnv());
        ctrl.ControllerContext = ControllerHelper.CreateContext(); // anonymous
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task KhoaHoc_Index_Admin_ReturnsAllCourses()
    {
        using var db = DbContextFactory.Create();
        SeedKhoaHoc(db, "KH1", "DangMo");
        SeedKhoaHoc(db, "KH2", "TamDung");
        var ctrl = MakeKhoaHocAdminCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel());

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<KhoaHocFilterViewModel>(view.Model);
        Assert.Equal(2, model.TotalItems);
    }

    [Fact]
    public async Task KhoaHoc_Index_Anonymous_ShowsOnlyOpenCourses()
    {
        using var db = DbContextFactory.Create();
        SeedKhoaHoc(db, "KH Open", "DangMo");
        SeedKhoaHoc(db, "KH Closed", "TamDung");
        var ctrl = MakeKhoaHocAnonCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel());

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<KhoaHocFilterViewModel>(view.Model);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task KhoaHoc_Index_FilterByNgonNgu_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedKhoaHoc(db, "IELTS", "DangMo");
        var kh2 = new KhoaHoc { TenKhoaHoc = "N3", NgonNgu = "Tiếng Nhật", TrinhDo = "Trung cấp", ThoiLuong = 30, TrangThai = "DangMo" };
        db.KhoaHocs.Add(kh2); db.SaveChanges();
        var ctrl = MakeKhoaHocAnonCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel { NgonNgu = "Tiếng Nhật" });

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<KhoaHocFilterViewModel>(view.Model);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task KhoaHoc_Details_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocAnonCtrl(db);

        var result = await ctrl.Details(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHoc_Details_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeKhoaHocAnonCtrl(db);

        var result = await ctrl.Details(kh.Id);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task KhoaHoc_Details_WithHocVienUser_ChecksDangKy()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        SeedHocVien(db, 200, "hv200@t.com");
        var ctrl = new KhoaHocController(db, MakeEnv());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(200, "hv200@t.com", "HV200", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Details(kh.Id);

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task KhoaHoc_EditGet_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocAdminCtrl(db);

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHoc_EditGet_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeKhoaHocAdminCtrl(db);

        var result = await ctrl.Edit(kh.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<KhoaHocCreateEditViewModel>(view.Model);
    }

    [Fact]
    public async Task KhoaHoc_EditPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeKhoaHocAdminCtrl(db);
        ctrl.ModelState.AddModelError("TenKhoaHoc", "Required");

        var result = await ctrl.Edit(kh.Id, new KhoaHocCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // TaiKhoanController — Index
    // ═══════════════════════════════════════════════════════════════════════════

    private static TaiKhoanController MakeTaiKhoanAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new TaiKhoanController(db, MakeConfig());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task TaiKhoan_Index_NoFilter_ReturnsAllUsers()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 100, "hv100@t.com");
        SeedHocVien(db, 101, "hv101@t.com");
        var ctrl = MakeTaiKhoanAdminCtrl(db);

        var result = await ctrl.Index(null, null);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Equal(2, list.Cast<object>().Count());
    }

    [Fact]
    public async Task TaiKhoan_Index_FilterByVaiTro_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 102, "hv102@t.com");
        SeedGiangVien(db, 103, "gv103@t.com");
        var ctrl = MakeTaiKhoanAdminCtrl(db);

        var result = await ctrl.Index("GiangVien", null);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AdminController — TaiKhoan, TaoTaiKhoan, KhoaTaiKhoan, DoiRole,
    //                   PhanCong, DoPhanCong, HuyPhanCong, LichSuPhanCong
    // ═══════════════════════════════════════════════════════════════════════════

    private static AdminController MakeAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var excel = new ExcelService(db);
        var pdf = new PdfService(db);
        var ctrl = new AdminController(db, excel, pdf, MakeConfig());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task Admin_TaiKhoan_ReturnsAllUsers()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 110, "hv110@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaiKhoan(null, null);

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task Admin_TaiKhoan_FilterBySearch_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 111, "alice@t.com");
        SeedHocVien(db, 112, "bob@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaiKhoan(null, "alice");

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task Admin_TaoTaiKhoan_DuplicateEmail_SetsError()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 113, "dup@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaoTaiKhoan("dup@t.com", "Test", null, "HocVien", "Pass@123");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Error", ctrl.TempData.Keys);
    }

    [Fact]
    public async Task Admin_TaoTaiKhoan_NewHocVien_CreatesUserAndRecord()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaoTaiKhoan("new@t.com", "New User", "0901", "HocVien", "Pass@123");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaiKhoan", redirect.ActionName);
        Assert.Single(db.HocViens);
    }

    [Fact]
    public async Task Admin_TaoTaiKhoan_NewGiangVien_CreatesUserAndRecord()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaoTaiKhoan("gvnew@t.com", "GV Mới", "0902", "GiangVien", "Pass@123");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.GiangViens);
    }

    [Fact]
    public async Task Admin_TaoTaiKhoan_NewAdmin_CreatesUserOnly()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.TaoTaiKhoan("admin2@t.com", "Admin 2", null, "Admin", "Pass@123");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.HocViens);
        Assert.Empty(db.GiangViens);
    }

    [Fact]
    public async Task Admin_KhoaTaiKhoan_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.KhoaTaiKhoan(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Admin_KhoaTaiKhoan_ActiveUser_Toggles()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 114, "hv114@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.KhoaTaiKhoan(114);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.False(db.NguoiDungs.Find(114)!.IsActive);
    }

    [Fact]
    public async Task Admin_DoiRole_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.DoiRole(99999, "GiangVien");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Admin_DoiRole_InvalidRole_ReturnsBadRequest()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 115, "hv115@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.DoiRole(115, "SuperAdmin");

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Admin_DoiRole_Valid_ChangesRole()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 116, "hv116@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.DoiRole(116, "GiangVien");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GiangVien", db.NguoiDungs.Find(116)!.VaiTro);
    }

    [Fact]
    public async Task Admin_ResetMatKhau_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.ResetMatKhau(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Admin_ResetMatKhau_Valid_ResetsPassword()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 117, "hv117@t.com");
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.ResetMatKhau(117);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(BCrypt.Net.BCrypt.Verify("Abc@12345", db.NguoiDungs.Find(117)!.MatKhauHash));
    }

    [Fact]
    public async Task Admin_PhanCong_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedKhoaHoc(db);
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.PhanCong();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Admin_DoPhanCong_AssignsGiangVien()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 120, "gv120@t.com");
        var kh = SeedKhoaHoc(db);
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.DoPhanCong(new PhanCongFormViewModel
            { KhoaHocId = kh.Id, GiangVienId = 120 });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PhanCong", redirect.ActionName);
        Assert.Single(db.PhanCongGiangDays);
    }

    [Fact]
    public async Task Admin_DoPhanCong_WithGiangVienZero_DeactivatesOnly()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 121, "gv121@t.com");
        var kh = SeedKhoaHoc(db);
        // Seed existing assignment
        db.PhanCongGiangDays.Add(new PhanCongGiangDay
            { GiangVienId = 121, KhoaHocId = kh.Id, IsActive = true, NgayPhanCong = DateTime.Now });
        await db.SaveChangesAsync();
        var ctrl = MakeAdminCtrl(db);

        // GiangVienId = 0 means "remove assignment"
        var result = await ctrl.DoPhanCong(new PhanCongFormViewModel
            { KhoaHocId = kh.Id, GiangVienId = 0 });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(db.PhanCongGiangDays.First().IsActive);
    }

    [Fact]
    public async Task Admin_HuyPhanCong_InvalidId_StillRedirects()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeAdminCtrl(db);

        // HuyPhanCong accepts null pc gracefully
        var result = await ctrl.HuyPhanCong(99999);

        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Admin_LichSuPhanCong_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 122, "gv122@t.com");
        var kh = SeedKhoaHoc(db);
        db.PhanCongGiangDays.Add(new PhanCongGiangDay
            { GiangVienId = 122, KhoaHocId = kh.Id, IsActive = false, NgayPhanCong = DateTime.Now });
        await db.SaveChangesAsync();
        var ctrl = MakeAdminCtrl(db);

        var result = await ctrl.LichSuPhanCong(kh.Id);

        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemController — LopHoc
    // ═══════════════════════════════════════════════════════════════════════════

    private static DiemController MakeDiemAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var excel = new ExcelService(db);
        var ctrl = new DiemController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task Diem_LopHoc_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemAdminCtrl(db);

        var result = await ctrl.LopHoc(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Diem_LopHoc_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp Test", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var ctrl = MakeDiemAdminCtrl(db);

        var result = await ctrl.LopHoc(lop.Id);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<BangDiemLopViewModel>(view.Model);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — Index, CuaToi
    // ═══════════════════════════════════════════════════════════════════════════

    private static DiemSoController MakeDiemSoAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var excel = new ExcelService(db);
        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static DiemSoController MakeDiemSoGVCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int gvId)
    {
        var excel = new ExcelService(db);
        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(gvId, $"gv{gvId}@t.com", $"GV{gvId}", "GiangVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task DiemSo_Index_EmptyDb_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.Index(null, null, null, null, 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DiemSoFilterViewModel>(view.Model);
        Assert.Equal(0, model.TotalItems);
    }

    [Fact]
    public async Task DiemSo_Index_WithDiems_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        SeedHocVien(db, 130, "hv130@t.com");
        var lop = new LopHoc { TenLop = "Lớp DS", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { Id = 1, HocVienId = 130, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = 1, DiemGiuaKy = 7.0, DiemCuoiKy = 8.0, DiemTongKet = 7.7 });
        await db.SaveChangesAsync();

        var ctrl = MakeDiemSoAdminCtrl(db);
        var result = await ctrl.Index(null, null, null, null, 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DiemSoFilterViewModel>(view.Model);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task DiemSo_Index_FilterByLopHoc_ReturnsFiltered()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        SeedHocVien(db, 131, "hv131@t.com");
        var lop = new LopHoc { TenLop = "Lớp DS2", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 131, LopHocId = lop.Id, TrangThai = "DaDuyet" });
        db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = db.DangKyKhoaHocs.First().Id });
        await db.SaveChangesAsync();

        var ctrl = MakeDiemSoAdminCtrl(db);
        var result = await ctrl.Index(null, kh.Id, lop.Id, null, 1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DiemSoFilterViewModel>(view.Model);
        Assert.Equal(1, model.TotalItems);
    }

    [Fact]
    public async Task DiemSo_CuaToi_GV_NoLop_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 132, "gv132@t.com");
        var ctrl = MakeDiemSoGVCtrl(db, 132);

        var result = await ctrl.CuaToi();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DiemSo_CuaToi_GV_WithLop_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 133, "gv133@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp GV", KhoaHocId = kh.Id, GiangVienId = 133, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakeDiemSoGVCtrl(db, 133);

        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ProfileController — Edit (POST) for HocVien and GiangVien
    // ═══════════════════════════════════════════════════════════════════════════

    private static ProfileController MakeProfileCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int userId, string role)
    {
        var ctrl = new ProfileController(db, MakeEnv());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(userId, $"u{userId}@t.com", $"User{userId}", role));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task Profile_EditPost_HocVien_UpdatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 140, "hv140@t.com");
        var ctrl = MakeProfileCtrl(db, 140, "HocVien");

        var result = await ctrl.Edit(new ProfileViewModel
        {
            HoTen = "HV Updated", SoDienThoai = "0901",
            NgaySinh = new DateOnly(1998, 3, 10), GioiTinh = "Nam",
            DiaChi = "Hà Nội", TrinhDoHienTai = "Trung cấp",
            NgonNguQuanTam = "Tiếng Nhật"
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("HV Updated", db.NguoiDungs.Find(140)!.HoTen);
    }

    [Fact]
    public async Task Profile_EditPost_GiangVien_UpdatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 141, "gv141@t.com");
        var ctrl = MakeProfileCtrl(db, 141, "GiangVien");

        var result = await ctrl.Edit(new ProfileViewModel
        {
            HoTen = "GV Updated", SoDienThoai = "0902",
            ChuyenMon = "TOEFL Updated", BangCap = "Tiến sĩ", KinhNghiem = 8
        });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GV Updated", db.GiangViens.Find(141)!.HoTen);
    }

    [Fact]
    public async Task Profile_EditPost_UserNotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeProfileCtrl(db, 99999, "HocVien");

        var result = await ctrl.Edit(new ProfileViewModel { HoTen = "Test" });

        Assert.IsType<NotFoundResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LichHocController — Index, CuaToi, GetLopStudents, GetEvents
    // ═══════════════════════════════════════════════════════════════════════════

    private static LichHocController MakeLichHocAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new LichHocController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static LichHocController MakeLichHocGVCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int gvId)
    {
        var ctrl = new LichHocController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(gvId, $"gv{gvId}@t.com", $"GV{gvId}", "GiangVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    private static LichHocController MakeLichHocHVCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int hvId)
    {
        var ctrl = new LichHocController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(hvId, $"hv{hvId}@t.com", $"HV{hvId}", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task LichHoc_Index_Admin_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task LichHoc_CuaToi_GiangVien_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 150, "gv150@t.com");
        var ctrl = MakeLichHocGVCtrl(db, 150);

        var result = await ctrl.CuaToi();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task LichHoc_CuaToi_HocVien_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 151, "hv151@t.com");
        var ctrl = MakeLichHocHVCtrl(db, 151);

        var result = await ctrl.CuaToi();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task LichHoc_GetLopStudents_EmptyLop_ReturnsEmptyJson()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp Empty", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakeLichHocAdminCtrl(db);

        var result = await ctrl.GetLopStudents(lop.Id);

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
    }

    [Fact]
    public async Task LichHoc_GetEvents_Admin_ReturnsJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var result = await ctrl.GetEvents(DateTime.Now.Month, DateTime.Now.Year);

        Assert.IsType<JsonResult>(result);
    }

    [Fact]
    public async Task LichHoc_GetEvents_GiangVien_ReturnsJson()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 152, "gv152@t.com");
        var ctrl = MakeLichHocGVCtrl(db, 152);

        var result = await ctrl.GetEvents(DateTime.Now.Month, DateTime.Now.Year);

        Assert.IsType<JsonResult>(result);
    }

    [Fact]
    public async Task LichHoc_GetEvents_HocVien_ReturnsJson()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 153, "hv153@t.com");
        var ctrl = MakeLichHocHVCtrl(db, 153);

        var result = await ctrl.GetEvents(DateTime.Now.Month, DateTime.Now.Year);

        Assert.IsType<JsonResult>(result);
    }
}
