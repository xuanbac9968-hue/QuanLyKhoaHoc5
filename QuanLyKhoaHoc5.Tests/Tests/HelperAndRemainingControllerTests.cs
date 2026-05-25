using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Helpers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho các controller + helper còn lại:
/// - HomeController (Index redirect logic)
/// - GiangVienController (Dashboard)
/// - HocVienController (Dashboard)
/// - AdminController (Index dashboard)
/// - ProfileController (Index)
/// - LichHocHelper (GenerateDates, ToViewModel)
/// - LichHocItemViewModel computed properties (TenThu, CaHoc)
/// </summary>
public class HelperAndRemainingControllerTests
{
    private static IConfiguration EmptyConfig()
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

    private static Mock<IWebHostEnvironment> MockEnv()
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        return env;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HomeController
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Home_Index_Anonymous_RedirectsToLogin()
    {
        var ctrl = new HomeController();
        ctrl.ControllerContext = ControllerHelper.CreateContext(); // no user

        var result = ctrl.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public void Home_Index_AuthAdmin_RedirectsToAdminIndex()
    {
        var ctrl = new HomeController();
        var user = ControllerHelper.CreateUser(1, "a@a.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = ctrl.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Admin", redirect.ControllerName);
    }

    [Fact]
    public void Home_Index_AuthGiangVien_RedirectsToDashboard()
    {
        var ctrl = new HomeController();
        var user = ControllerHelper.CreateUser(2, "gv@a.com", "GV", "GiangVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = ctrl.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", redirect.ActionName);
        Assert.Equal("GiangVien", redirect.ControllerName);
    }

    [Fact]
    public void Home_Index_AuthHocVien_RedirectsToDashboard()
    {
        var ctrl = new HomeController();
        var user = ControllerHelper.CreateUser(3, "hv@a.com", "HV", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = ctrl.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", redirect.ActionName);
        Assert.Equal("HocVien", redirect.ControllerName);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GiangVienController
    // ══════════════════════════════════════════════════════════════════════════

    private static (QuanLyKhoaHoc5.Web.Data.AppDbContext db, GiangVienController ctrl) SetupGiangVien(int gvId = 5)
    {
        var db = DbContextFactory.Create();
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = gvId, Email = $"gv{gvId}@test.com", HoTen = $"GV {gvId}",
            VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true
        });
        db.GiangViens.Add(new GiangVien
        {
            Id = gvId, MaGiangVien = $"GV00{gvId}", HoTen = $"GV {gvId}", ChuyenMon = "Tiếng Anh"
        });
        db.SaveChanges();

        var ctrl = new GiangVienController(db, EmptyConfig());
        var user = ControllerHelper.CreateUser(gvId, $"gv{gvId}@test.com", $"GV {gvId}", "GiangVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return (db, ctrl);
    }

    [Fact]
    public async Task GiangVien_Dashboard_ValidGV_ReturnsView()
    {
        var (db, ctrl) = SetupGiangVien();
        using (db)
        {
            var result = await ctrl.Dashboard();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GiangVienDashboardViewModel>(view.Model);
            Assert.Equal("GV 5", model.HoTen);
        }
    }

    [Fact]
    public async Task GiangVien_Dashboard_GVNotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new GiangVienController(db, EmptyConfig());
        var user = ControllerHelper.CreateUser(99, "nobody@test.com", "Nobody", "GiangVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.Dashboard();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GiangVien_Dashboard_WithLopHoc_ShowsLopList()
    {
        var (db, ctrl) = SetupGiangVien(gvId: 6);
        using (db)
        {
            var kh = new KhoaHoc { TenKhoaHoc = "KH Test", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 10 };
            db.KhoaHocs.Add(kh);
            db.LopHocs.Add(new LopHoc { TenLop = "Lớp GV", KhoaHocId = kh.Id, GiangVienId = 6, TrangThai = "DangHoc" });
            await db.SaveChangesAsync();

            var result = await ctrl.Dashboard();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GiangVienDashboardViewModel>(view.Model);
            Assert.Single(model.LopDangDay);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // HocVienController
    // ══════════════════════════════════════════════════════════════════════════

    private static (QuanLyKhoaHoc5.Web.Data.AppDbContext db, HocVienController ctrl) SetupHocVien(int hvId = 7)
    {
        var db = DbContextFactory.Create();
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = hvId, Email = $"hv{hvId}@test.com", HoTen = $"HV {hvId}",
            VaiTro = "HocVien", MatKhauHash = "x", IsActive = true
        });
        db.HocViens.Add(new HocVien { Id = hvId, MaHocVien = $"HV00{hvId}", HoTen = $"HV {hvId}" });
        db.SaveChanges();

        var excelSvc = new ExcelService(db);
        var ctrl = new HocVienController(db, excelSvc, MockEnv().Object, EmptyConfig());
        var user = ControllerHelper.CreateUser(hvId, $"hv{hvId}@test.com", $"HV {hvId}", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return (db, ctrl);
    }

    [Fact]
    public async Task HocVien_Dashboard_ValidHV_ReturnsView()
    {
        var (db, ctrl) = SetupHocVien();
        using (db)
        {
            var result = await ctrl.Dashboard();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HocVienDashboardViewModel>(view.Model);
            Assert.Equal("HV 7", model.HoTen);
        }
    }

    [Fact]
    public async Task HocVien_Dashboard_HVNotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var excelSvc = new ExcelService(db);
        var ctrl = new HocVienController(db, excelSvc, MockEnv().Object, EmptyConfig());
        var user = ControllerHelper.CreateUser(999, "nobody@test.com", "Nobody", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.Dashboard();

        Assert.IsType<NotFoundResult>(result);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AdminController
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Admin_Index_EmptyDb_ReturnsViewWithZeroStats()
    {
        using var db = DbContextFactory.Create();
        var excelSvc = new ExcelService(db);
        var pdfSvc = new PdfService(db);
        var ctrl = new AdminController(db, excelSvc, pdfSvc, EmptyConfig());
        var adminUser = ControllerHelper.CreateUser(1, "admin@test.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(adminUser);

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(0, model.TongHocVienDangHoc);
        Assert.Equal(0, model.TongKhoaHocDangMo);
    }

    [Fact]
    public async Task Admin_Index_WithData_ReturnsCorrectCounts()
    {
        using var db = DbContextFactory.Create();
        // Seed: 2 KhoaHoc DangMo, 1 NguoiDung HocVien, 1 GiangVien, 1 DangKy DaDuyet
        var kh1 = new KhoaHoc { TenKhoaHoc = "KH1", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 10, TrangThai = "DangMo" };
        var kh2 = new KhoaHoc { TenKhoaHoc = "KH2", NgonNgu = "Nhật", TrinhDo = "SC", ThoiLuong = 10, TrangThai = "DangMo" };
        db.KhoaHocs.AddRange(kh1, kh2);
        db.NguoiDungs.Add(new NguoiDung { Id = 10, Email = "hv@t.com", HoTen = "HV", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true });
        db.HocViens.Add(new HocVien { Id = 10, MaHocVien = "HV001", HoTen = "HV" });
        db.NguoiDungs.Add(new NguoiDung { Id = 11, Email = "gv@t.com", HoTen = "GV", VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true });
        db.GiangViens.Add(new GiangVien { Id = 11, MaGiangVien = "GV001", HoTen = "GV" });
        await db.SaveChangesAsync();
        var lop = new LopHoc { TenLop = "Lop A", KhoaHocId = kh1.Id, TrangThai = "DangHoc" };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 10, LopHocId = lop.Id, TrangThai = "DaDuyet", NgayDuyet = DateTime.Now });
        await db.SaveChangesAsync();

        var excelSvc = new ExcelService(db);
        var pdfSvc = new PdfService(db);
        var ctrl = new AdminController(db, excelSvc, pdfSvc, EmptyConfig());
        var adminUser = ControllerHelper.CreateUser(1, "admin@test.com", "Admin", "Admin");
        ctrl.ControllerContext = ControllerHelper.CreateContext(adminUser);

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(1, model.TongHocVienDangHoc);
        Assert.Equal(2, model.TongKhoaHocDangMo);
        Assert.Equal(1, model.TongGiangVien);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ProfileController
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Profile_Index_UserNotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new ProfileController(db, MockEnv().Object);
        var user = ControllerHelper.CreateUser(999, "n@t.com", "N", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.Index();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Profile_Index_HocVien_ReturnsViewWithProfile()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = 20, Email = "hv20@test.com", HoTen = "HV 20",
            VaiTro = "HocVien", MatKhauHash = "x", IsActive = true
        });
        db.HocViens.Add(new HocVien
        {
            Id = 20, MaHocVien = "HV020", HoTen = "HV 20",
            GioiTinh = "Nam", TrinhDoHienTai = "Cơ bản"
        });
        await db.SaveChangesAsync();

        var ctrl = new ProfileController(db, MockEnv().Object);
        var user = ControllerHelper.CreateUser(20, "hv20@test.com", "HV 20", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(view.Model);
        Assert.Equal("HV 20", model.HoTen);
        Assert.Equal("Nam", model.GioiTinh);
    }

    [Fact]
    public async Task Profile_Index_GiangVien_ReturnsViewWithChuyenMon()
    {
        using var db = DbContextFactory.Create();
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = 21, Email = "gv21@test.com", HoTen = "GV 21",
            VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true
        });
        db.GiangViens.Add(new GiangVien
        {
            Id = 21, MaGiangVien = "GV021", HoTen = "GV 21",
            ChuyenMon = "Tiếng Nhật", BangCap = "Thạc sĩ", KinhNghiem = 5
        });
        await db.SaveChangesAsync();

        var ctrl = new ProfileController(db, MockEnv().Object);
        var user = ControllerHelper.CreateUser(21, "gv21@test.com", "GV 21", "GiangVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(view.Model);
        Assert.Equal("Tiếng Nhật", model.ChuyenMon);
        Assert.Equal("Thạc sĩ", model.BangCap);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LichHocHelper — GenerateDates
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void LichHocHelper_GenerateDates_ReturnsCorrectWeekdays()
    {
        var start = new DateOnly(2026, 1, 5);  // Monday
        var end   = new DateOnly(2026, 1, 16); // Friday of next week
        var days  = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday };

        var result = LichHocHelper.GenerateDates(start, end, days).ToList();

        // Expect: Mon 5, Wed 7, Mon 12, Wed 14
        Assert.Equal(4, result.Count);
        Assert.All(result, d => Assert.True(d.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Wednesday));
    }

    [Fact]
    public void LichHocHelper_GenerateDates_EmptyRange_ReturnsEmpty()
    {
        var start = new DateOnly(2026, 1, 10);
        var end   = new DateOnly(2026, 1, 9); // end before start

        var result = LichHocHelper.GenerateDates(start, end, [DayOfWeek.Monday]).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void LichHocHelper_GenerateDates_NoDayMatch_ReturnsEmpty()
    {
        var start = new DateOnly(2026, 1, 5);  // Monday
        var end   = new DateOnly(2026, 1, 6);  // Tuesday
        // Only generate Friday — no match in Mon-Tue range
        var result = LichHocHelper.GenerateDates(start, end, [DayOfWeek.Friday]).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void LichHocHelper_ToViewModel_MapsCorrectly()
    {
        var lichHoc = new LichHoc
        {
            Id = 42, LopHocId = 7,
            NgayHoc = new DateOnly(2026, 6, 1),
            GioBatDau = new TimeOnly(8, 0),
            GioKetThuc = new TimeOnly(10, 0),
            PhongHoc = "P.201", ChuDe = "Greetings"
        };

        var vm = LichHocHelper.ToViewModel(lichHoc);

        Assert.Equal(42, vm.Id);
        Assert.Equal(7, vm.LopHocId);
        Assert.Equal(new DateOnly(2026, 6, 1), vm.NgayHoc);
        Assert.Equal("P.201", vm.PhongHoc);
        Assert.Equal("Greetings", vm.ChuDe);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LichHocItemViewModel — computed properties
    // ══════════════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData(DayOfWeek.Monday,    "Thứ Hai")]
    [InlineData(DayOfWeek.Tuesday,   "Thứ Ba")]
    [InlineData(DayOfWeek.Wednesday, "Thứ Tư")]
    [InlineData(DayOfWeek.Thursday,  "Thứ Năm")]
    [InlineData(DayOfWeek.Friday,    "Thứ Sáu")]
    [InlineData(DayOfWeek.Saturday,  "Thứ Bảy")]
    [InlineData(DayOfWeek.Sunday,    "Chủ Nhật")]
    public void LichHocItemViewModel_TenThu_MapsCorrectly(DayOfWeek dow, string expected)
    {
        // Find a DateOnly with that DayOfWeek
        var baseDate = new DateOnly(2026, 1, 5); // Monday
        var offset = ((int)dow - (int)DayOfWeek.Monday + 7) % 7;
        var date = baseDate.AddDays(offset);

        var vm = new LichHocItemViewModel { NgayHoc = date };

        Assert.Equal(expected, vm.TenThu);
    }

    [Theory]
    [InlineData(7,  "Sáng")]
    [InlineData(13, "Chiều")]
    [InlineData(19, "Tối")]
    public void LichHocItemViewModel_CaHoc_MapsCorrectly(int hour, string expected)
    {
        var vm = new LichHocItemViewModel
        {
            NgayHoc = new DateOnly(2026, 1, 5),
            GioBatDau = new TimeOnly(hour, 0)
        };

        Assert.Equal(expected, vm.CaHoc);
    }
}
