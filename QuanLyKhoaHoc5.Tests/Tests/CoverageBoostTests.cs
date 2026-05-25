using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Bổ sung unit tests để đẩy coverage từ ~62.7% lên ≥70%.
/// Targets:
///   - LichHocController: Create (POST), TaoHangLoat (POST)
///   - GoiYController: Index, LichSu, Admin, TaoGoiY error paths
///   - BaoCaoController: ExportTongHop
///   - ExcelService: ExportHocVienAsync, ExportBangDiemAsync, ExportBaoCaoHocVienExcelAsync
///   - KhoaHocController: Create POST (valid), Edit POST (valid/invalid)
///   - DiemController: ExportExcel
///   - DiemSoController: ExportExcel, ExportExcelByLop
/// </summary>
public class CoverageBoostTests
{
    // ─── Shared seed helpers ──────────────────────────────────────────────────

    private static KhoaHoc SeedKhoaHoc(QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        string ten = "KH Test", string trangThai = "DangMo")
    {
        var kh = new KhoaHoc
        {
            TenKhoaHoc = ten, NgonNgu = "Tiếng Anh", TrinhDo = "Sơ cấp",
            HocPhi = 2_000_000m, ThoiLuong = 20, TrangThai = trangThai
        };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        return kh;
    }

    private static LopHoc SeedLopHoc(QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        int khoaHocId, int? giangVienId = null)
    {
        var lop = new LopHoc
        {
            TenLop = "Lớp Test", KhoaHocId = khoaHocId,
            TrangThai = "DangHoc", SiSoToiDa = 20, GiangVienId = giangVienId
        };
        db.LopHocs.Add(lop); db.SaveChanges();
        return lop;
    }

    private static (NguoiDung nd, HocVien hv) SeedHocVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, string email = "hv@t.com")
    {
        var nd = new NguoiDung
        {
            Email = email, HoTen = "HV Test", VaiTro = "HocVien",
            MatKhauHash = "x", IsActive = true
        };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        var hv = new HocVien { Id = nd.Id, MaHocVien = $"HV{nd.Id:000}", HoTen = nd.HoTen };
        db.HocViens.Add(hv); db.SaveChanges();
        return (nd, hv);
    }

    private static (NguoiDung nd, GiangVien gv) SeedGiangVien(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, string email = "gv@t.com")
    {
        var nd = new NguoiDung
        {
            Email = email, HoTen = "GV Test", VaiTro = "GiangVien",
            MatKhauHash = "x", IsActive = true
        };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        var gv = new GiangVien
        {
            Id = nd.Id, MaGiangVien = $"GV{nd.Id:000}", HoTen = nd.HoTen,
            ChuyenMon = "Tiếng Anh"
        };
        db.GiangViens.Add(gv); db.SaveChanges();
        return (nd, gv);
    }

    private static LichHocController MakeLichHocAdminCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int adminId = 1)
    {
        var ctrl = new LichHocController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(adminId, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LichHocController — Create (POST)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task LichHoc_Create_InvalidDate_ReturnsFailureJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new LichHocCreateViewModel
        {
            LopHocId = 1,
            NgayHoc = "not-a-date",   // invalid
            GioBatDau = "08:00",
            GioKetThuc = "10:00"
        };

        var result = await ctrl.Create(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_Create_InvalidTime_ReturnsFailureJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new LichHocCreateViewModel
        {
            LopHocId = 1,
            NgayHoc = "2026-06-01",
            GioBatDau = "not-time",   // invalid
            GioKetThuc = "10:00"
        };

        var result = await ctrl.Create(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_Create_ValidData_NoConflict_ReturnsSuccess()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new LichHocCreateViewModel
        {
            LopHocId = lop.Id,
            NgayHoc = "2026-08-01",
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            PhongHoc = "P101",
            ChuDe = "Lesson 1",
            GhiChu = "Ghi chú test"
        };

        var result = await ctrl.Create(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Equal(1, db.LichHocs.Count());
    }

    [Fact]
    public async Task LichHoc_Create_RoomConflict_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);

        // Seed existing session in same room/time
        db.LichHocs.Add(new LichHoc
        {
            LopHocId = lop.Id,
            NgayHoc = DateOnly.Parse("2026-08-02"),
            GioBatDau = TimeOnly.Parse("08:00"),
            GioKetThuc = TimeOnly.Parse("10:00"),
            PhongHoc = "P201"
        });
        await db.SaveChangesAsync();

        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new LichHocCreateViewModel
        {
            LopHocId = lop.Id,
            NgayHoc = "2026-08-02",
            GioBatDau = "08:30",
            GioKetThuc = "09:30",
            PhongHoc = "P201"   // same room, overlapping time
        };

        var result = await ctrl.Create(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_Create_GiangVienConflict_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var (ndGv, gv) = SeedGiangVien(db, "gv_conflict@t.com");
        var kh = SeedKhoaHoc(db);
        var lop1 = SeedLopHoc(db, kh.Id, gv.Id);
        var lop2 = SeedLopHoc(db, kh.Id, gv.Id);

        // Seed existing session for lop1 on 2026-08-03 08:00-10:00
        db.LichHocs.Add(new LichHoc
        {
            LopHocId = lop1.Id,
            NgayHoc = DateOnly.Parse("2026-08-03"),
            GioBatDau = TimeOnly.Parse("08:00"),
            GioKetThuc = TimeOnly.Parse("10:00"),
            PhongHoc = "P300"
        });
        await db.SaveChangesAsync();

        var ctrl = MakeLichHocAdminCtrl(db);

        // Try to schedule lop2 for same GiangVien at overlapping time
        var vm = new LichHocCreateViewModel
        {
            LopHocId = lop2.Id,
            NgayHoc = "2026-08-03",
            GioBatDau = "08:30",
            GioKetThuc = "09:30",
            PhongHoc = "P400"
        };

        var result = await ctrl.Create(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LichHocController — TaoHangLoat (POST)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task LichHoc_TaoHangLoat_InvalidDate_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new TaoHangLoatViewModel
        {
            LopHocId = 1,
            NgayBatDau = "bad-date",
            NgayKetThuc = "2026-09-30",
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            ThuTrongTuan = new List<int> { 1 }
        };

        var result = await ctrl.TaoHangLoat(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_TaoHangLoat_EmptyDays_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new TaoHangLoatViewModel
        {
            LopHocId = 1,
            NgayBatDau = "2026-09-01",
            NgayKetThuc = "2026-09-30",
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            ThuTrongTuan = new List<int>()   // empty
        };

        var result = await ctrl.TaoHangLoat(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_TaoHangLoat_EndBeforeStart_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new TaoHangLoatViewModel
        {
            LopHocId = 1,
            NgayBatDau = "2026-09-30",
            NgayKetThuc = "2026-09-01",   // before start
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            ThuTrongTuan = new List<int> { 1 }
        };

        var result = await ctrl.TaoHangLoat(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_TaoHangLoat_LopNotFound_ReturnsFailure()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var vm = new TaoHangLoatViewModel
        {
            LopHocId = 99999,   // not found
            NgayBatDau = "2026-09-01",
            NgayKetThuc = "2026-09-30",
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            ThuTrongTuan = new List<int> { 1 }
        };

        var result = await ctrl.TaoHangLoat(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_TaoHangLoat_ValidRequest_CreatesSessions()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var ctrl = MakeLichHocAdminCtrl(db);

        // Week of Sep 7 2026 = Monday=7, no pre-existing sessions
        var vm = new TaoHangLoatViewModel
        {
            LopHocId = lop.Id,
            NgayBatDau = "2026-09-01",
            NgayKetThuc = "2026-09-07",   // 1 week
            GioBatDau = "08:00",
            GioKetThuc = "10:00",
            ThuTrongTuan = new List<int> { 1, 3 },   // Mon + Wed
            PhongHoc = "P501"
        };

        var result = await ctrl.TaoHangLoat(vm);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        // Sep 1 (Mon) + Sep 3 (Wed) = 2 sessions
        Assert.Equal(2, db.LichHocs.Count());
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GoiYController — LichSu, Admin
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GoiY_LichSu_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var (nd, hv) = SeedHocVien(db, "goiy_lichsu@t.com");
        var goiYService = new GoiYKhoaHocService(db, null!, NullLogger<GoiYKhoaHocService>.Instance);

        var ctrl = new GoiYController(db, goiYService);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(nd.Id, nd.Email, nd.HoTen, "HocVien"));

        var result = await ctrl.LichSu();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<GoiYKhoaHoc>>(view.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task GoiY_Admin_ReturnsView_WithPaging()
    {
        using var db = DbContextFactory.Create();
        var goiYService = new GoiYKhoaHocService(db, null!, NullLogger<GoiYKhoaHocService>.Instance);

        var ctrl = new GoiYController(db, goiYService);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.Admin(page: 1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.Model);
    }

    [Fact]
    public async Task GoiY_TaoGoiY_WhenServiceThrows_ReturnsJsonError()
    {
        using var db = DbContextFactory.Create();
        var (nd, hv) = SeedHocVien(db, "goiy_tao@t.com");
        // GeminiService = null! → TaoGoiYAsync will throw an exception
        var goiYService = new GoiYKhoaHocService(db, null!, NullLogger<GoiYKhoaHocService>.Instance);

        var ctrl = new GoiYController(db, goiYService);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(nd.Id, nd.Email, nd.HoTen, "HocVien"));

        // TaoGoiY receives logger from [FromServices]; we can call it directly
        // using a NullLogger to avoid DI dependency
        var logger = NullLogger<GoiYController>.Instance;
        var result = await ctrl.TaoGoiY(logger);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // BaoCaoController — ExportTongHop
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task BaoCao_ExportTongHop_ReturnsFileResult()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new BaoCaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.ExportTongHop();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileResult.ContentType);
        Assert.NotEmpty(fileResult.FileContents);
    }

    [Fact]
    public async Task BaoCao_ExportTongHop_WithData_ReturnsNonEmptyFile()
    {
        using var db = DbContextFactory.Create();
        var (nd, hv) = SeedHocVien(db, "bch_export@t.com");
        var kh = SeedKhoaHoc(db, "KH Export");
        var lop = SeedLopHoc(db, kh.Id);

        var ctrl = new BaoCaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.ExportTongHop();

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.True(fileResult.FileContents.Length > 0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ExcelService — direct service tests
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExcelService_ExportHocVienAsync_ReturnsBytes()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, "excel_hv1@t.com");
        SeedHocVien(db, "excel_hv2@t.com");
        var svc = new ExcelService(db);

        var bytes = await svc.ExportHocVienAsync();

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportHocVienAsync_EmptyDb_ReturnsBytes()
    {
        using var db = DbContextFactory.Create();
        var svc = new ExcelService(db);

        var bytes = await svc.ExportHocVienAsync();

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);  // still returns Excel with headers
    }

    [Fact]
    public async Task ExcelService_ExportBangDiemAsync_ReturnsBytes()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var svc = new ExcelService(db);

        var bytes = await svc.ExportBangDiemAsync(lop.Id);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBaoCaoHocVienExcelAsync_NoFilter_ReturnsBytes()
    {
        using var db = DbContextFactory.Create();
        var svc = new ExcelService(db);

        var bytes = await svc.ExportBaoCaoHocVienExcelAsync(null, null);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBaoCaoHocVienExcelAsync_WithDateFilter_ReturnsBytes()
    {
        using var db = DbContextFactory.Create();
        var svc = new ExcelService(db);

        var bytes = await svc.ExportBaoCaoHocVienExcelAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 12, 31));

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // KhoaHocController — Create POST (valid) and Edit POST
    // ═══════════════════════════════════════════════════════════════════════════

    private static Mock<IWebHostEnvironment> MockEnv()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return env;
    }

    [Fact]
    public async Task KhoaHoc_CreatePost_ValidModel_RedirectsToIndex()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var vm = new KhoaHocCreateEditViewModel
        {
            TenKhoaHoc = "Khóa Mới",
            NgonNgu = "Tiếng Anh",
            TrinhDo = "Trung cấp",
            HocPhi = 3_000_000m,
            ThoiLuong = 30,
            TrangThai = "DangMo"
        };

        var result = await ctrl.Create(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Single(db.KhoaHocs);
        Assert.Equal("Khóa Mới", db.KhoaHocs.First().TenKhoaHoc);
    }

    [Fact]
    public async Task KhoaHoc_EditPost_ValidModel_RedirectsToIndex()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db, "KH Cũ");
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var vm = new KhoaHocCreateEditViewModel
        {
            Id = kh.Id,
            TenKhoaHoc = "KH Mới",
            NgonNgu = "Tiếng Nhật",
            TrinhDo = "Nâng cao",
            HocPhi = 5_000_000m,
            ThoiLuong = 40,
            TrangThai = "DangMo"
        };

        var result = await ctrl.Edit(kh.Id, vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        var updated = await db.KhoaHocs.FindAsync(kh.Id);
        Assert.Equal("KH Mới", updated!.TenKhoaHoc);
        Assert.Equal("Tiếng Nhật", updated.NgonNgu);
    }

    [Fact]
    public async Task KhoaHoc_EditPost_InvalidId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var vm = new KhoaHocCreateEditViewModel
        {
            TenKhoaHoc = "X", NgonNgu = "Y", TrinhDo = "Z",
            HocPhi = 1m, ThoiLuong = 10, TrangThai = "DangMo"
        };

        var result = await ctrl.Edit(99999, vm);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHoc_EditPost_InvalidModel_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var ctrl = new KhoaHocController(db, MockEnv().Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.ModelState.AddModelError("TenKhoaHoc", "Required");

        var result = await ctrl.Edit(kh.Id, new KhoaHocCreateEditViewModel());

        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemController — ExportExcel
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Diem_ExportExcel_ReturnsFileResult()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var excel = new ExcelService(db);

        var ctrl = new DiemController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.ExportExcel(lop.Id);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileResult.ContentType);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — ExportExcel, ExportExcelByLop
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_ExportExcel_WithLopId_ReturnsFile()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var excel = new ExcelService(db);

        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.ExportExcel(null, null, lop.Id, null);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileResult.ContentType);
    }

    [Fact]
    public async Task DiemSo_ExportExcel_NoData_RedirectsWithWarning()
    {
        using var db = DbContextFactory.Create();
        var excel = new ExcelService(db);

        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        // No lopHocId, no data in DB → fallback path → RedirectToAction
        var result = await ctrl.ExportExcel(null, null, null, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(ctrl.TempData.ContainsKey("Warning"));
    }

    [Fact]
    public async Task DiemSo_ExportExcelByLop_Admin_ReturnsFile()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = SeedLopHoc(db, kh.Id);
        var excel = new ExcelService(db);

        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.ExportExcelByLop(lop.Id);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileResult.ContentType);
    }

    [Fact]
    public async Task DiemSo_ExportExcelByLop_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var excel = new ExcelService(db);

        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.ExportExcelByLop(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    // ─── Utility ─────────────────────────────────────────────────────────────
    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
