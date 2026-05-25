using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Last-mile tests to push coverage from 65.7% to ≥70%.
/// Targets: BuoiHocViewModel (TenThu/CaHoc), BaoCaoController.Index,
/// ThanhToanController.ChiTiet, LopHocController.Details,
/// PhanCongFormViewModel, LichHocCreateViewModel, TaoHangLoatViewModel,
/// ChatMessageViewModel, ProfileViewModel.
/// </summary>
public class LastMileTests
{
    // ─── BuoiHocViewModel & LichHocChiTietViewModel ───────────────────────────

    [Theory]
    [InlineData(2026, 5, 25, "Thứ Hai")]    // Monday
    [InlineData(2026, 5, 26, "Thứ Ba")]     // Tuesday
    [InlineData(2026, 5, 27, "Thứ Tư")]    // Wednesday
    [InlineData(2026, 5, 28, "Thứ Năm")]   // Thursday
    [InlineData(2026, 5, 29, "Thứ Sáu")]   // Friday
    [InlineData(2026, 5, 30, "Thứ Bảy")]   // Saturday
    [InlineData(2026, 5, 31, "Chủ Nhật")]  // Sunday
    public void BuoiHocViewModel_TenThu_ReturnsVietnameseName(int y, int m, int d, string expected)
    {
        var vm = new BuoiHocViewModel { NgayHoc = new DateOnly(y, m, d) };
        Assert.Equal(expected, vm.TenThu);
    }

    [Theory]
    [InlineData(7, "Sáng")]    // 7 AM → morning
    [InlineData(13, "Chiều")]  // 1 PM → afternoon
    [InlineData(19, "Tối")]    // 7 PM → evening
    public void BuoiHocViewModel_CaHoc_ReturnsCorrectShift(int hour, string expected)
    {
        var vm = new BuoiHocViewModel { GioBatDau = new TimeOnly(hour, 0) };
        Assert.Equal(expected, vm.CaHoc);
    }

    [Fact]
    public void BuoiHocViewModel_AllProperties_SetAndGet()
    {
        var vm = new BuoiHocViewModel
        {
            NgayHoc = new DateOnly(2026, 6, 1),
            KhoaHocId = 1, TenKhoaHoc = "Tiếng Nhật",
            TenGiangVien = "GV Test", GioBatDau = new TimeOnly(9, 0),
            GioKetThuc = new TimeOnly(11, 0), PhongHoc = "P101"
        };
        Assert.Equal("P101", vm.PhongHoc);
        Assert.Equal("Tiếng Nhật", vm.TenKhoaHoc);
    }

    // ─── LichHocChiTietViewModel ──────────────────────────────────────────────

    [Theory]
    [InlineData(2026, 5, 25, "Thứ Hai")]
    [InlineData(2026, 5, 31, "Chủ Nhật")]
    public void LichHocChiTietViewModel_TenThu_Correct(int y, int m, int d, string expected)
    {
        var vm = new LichHocChiTietViewModel { NgayHoc = new DateOnly(y, m, d) };
        Assert.Equal(expected, vm.TenThu);
    }

    [Theory]
    [InlineData(8, "Sáng")]
    [InlineData(15, "Chiều")]
    [InlineData(20, "Tối")]
    public void LichHocChiTietViewModel_CaHoc_Correct(int hour, string expected)
    {
        var vm = new LichHocChiTietViewModel { GioBatDau = new TimeOnly(hour, 0) };
        Assert.Equal(expected, vm.CaHoc);
    }

    [Fact]
    public void LichHocChiTietViewModel_AllProperties_SetAndGet()
    {
        var vm = new LichHocChiTietViewModel
        {
            Id = 5, LopHocId = 1, KhoaHocId = 2, TenKhoaHoc = "IELTS",
            TenLop = "Lớp A", TenGiangVien = "GV A",
            NgayHoc = new DateOnly(2026, 7, 1), GioBatDau = new TimeOnly(8, 0),
            GioKetThuc = new TimeOnly(10, 0), PhongHoc = "P201",
            ChuDe = "Grammar", GhiChu = "Note"
        };
        Assert.Equal(5, vm.Id);
        Assert.Equal("IELTS", vm.TenKhoaHoc);
    }

    // ─── PhanCongFormViewModel ─────────────────────────────────────────────────

    [Fact]
    public void PhanCongFormViewModel_Properties_SetAndGet()
    {
        var vm = new PhanCongFormViewModel { KhoaHocId = 1, GiangVienId = 5, GhiChu = "test" };
        Assert.Equal(1, vm.KhoaHocId);
        Assert.Equal(5, vm.GiangVienId);
    }

    // ─── LichHocCreateViewModel ────────────────────────────────────────────────

    [Fact]
    public void LichHocCreateViewModel_Properties_SetAndGet()
    {
        var vm = new LichHocCreateViewModel
        {
            LopHocId = 1, NgayHoc = "2026-06-01", GioBatDau = "08:00",
            GioKetThuc = "09:30", PhongHoc = "P101", ChuDe = "Test", GhiChu = "note"
        };
        Assert.Equal("2026-06-01", vm.NgayHoc);
        Assert.Equal("P101", vm.PhongHoc);
    }

    // ─── TaoHangLoatViewModel ─────────────────────────────────────────────────

    [Fact]
    public void TaoHangLoatViewModel_Properties_SetAndGet()
    {
        var vm = new TaoHangLoatViewModel
        {
            LopHocId = 2, NgayBatDau = "2026-06-01", NgayKetThuc = "2026-08-31",
            ThuTrongTuan = [1, 3, 5], GioBatDau = "08:00", GioKetThuc = "09:30",
            PhongHoc = "P102"
        };
        Assert.Equal(3, vm.ThuTrongTuan.Count);
        Assert.Contains(3, vm.ThuTrongTuan);
    }

    // ─── ChatMessageViewModel & ChatRequest ───────────────────────────────────

    [Fact]
    public void ChatMessageViewModel_Properties_SetAndGet()
    {
        var vm = new ChatMessageViewModel
        {
            Role = "assistant", Content = "Xin chào!", Timestamp = DateTime.Now
        };
        Assert.Equal("assistant", vm.Role);
        Assert.Equal("Xin chào!", vm.Content);
    }

    [Fact]
    public void ChatRequest_Properties_SetAndGet()
    {
        var vm = new ChatRequest { Message = "Hello World" };
        Assert.Equal("Hello World", vm.Message);
    }

    // ─── ProfileViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void ProfileViewModel_HocVienFields_SetAndGet()
    {
        var vm = new ProfileViewModel
        {
            Id = 1, HoTen = "Test HV", Email = "hv@t.com", SoDienThoai = "0901",
            VaiTro = "HocVien", NgaySinh = new DateOnly(2000, 1, 1),
            GioiTinh = "Nam", DiaChi = "HN", TrinhDoHienTai = "Sơ cấp",
            NgonNguQuanTam = "Tiếng Anh"
        };
        Assert.Equal("HocVien", vm.VaiTro);
        Assert.Equal("Sơ cấp", vm.TrinhDoHienTai);
    }

    [Fact]
    public void ProfileViewModel_GiangVienFields_SetAndGet()
    {
        var vm = new ProfileViewModel
        {
            Id = 2, HoTen = "GV Test", Email = "gv@t.com", VaiTro = "GiangVien",
            ChuyenMon = "IELTS", BangCap = "Thạc sĩ", KinhNghiem = 5, MoTa = "Mô tả"
        };
        Assert.Equal("GiangVien", vm.VaiTro);
        Assert.Equal("IELTS", vm.ChuyenMon);
        Assert.Equal(5, vm.KinhNghiem);
    }

    // ─── BaoCaoFilterViewModel ────────────────────────────────────────────────

    [Fact]
    public void BaoCaoFilterViewModel_Properties_SetAndGet()
    {
        var vm = new BaoCaoFilterViewModel
        {
            TuNgay = DateTime.Now.AddMonths(-1), DenNgay = DateTime.Now
        };
        Assert.NotNull(vm.TuNgay);
        Assert.NotNull(vm.DenNgay);
    }

    // ─── BaoCaoController.Index ────────────────────────────────────────────────

    [Fact]
    public async Task BaoCao_Index_EmptyDb_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ctrl = new BaoCaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task BaoCao_Index_WithData_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        // Add some data so the chart query runs through real paths
        var nd = new NguoiDung { Email = "hv@t.com", HoTen = "HV1", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        db.HocViens.Add(new HocVien { Id = nd.Id, MaHocVien = "HV001", HoTen = "HV1" });
        db.SaveChanges();
        var kh = new KhoaHoc { TenKhoaHoc = "KH1", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp A", KhoaHocId = kh.Id, TrangThai = "DaKetThuc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        db.DangKyKhoaHocs.Add(new DangKyKhoaHoc
            { HocVienId = nd.Id, LopHocId = lop.Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now });
        await db.SaveChangesAsync();

        var ctrl = new BaoCaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Index();

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, ctrl.ViewBag.TongHocVien);
    }

    // ─── ThanhToanController.ChiTiet ─────────────────────────────────────────

    [Fact]
    public async Task ThanhToan_ChiTiet_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var ndHV = new NguoiDung { Email = "hv@t.com", HoTen = "HV1", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(ndHV); db.SaveChanges();
        db.HocViens.Add(new HocVien { Id = ndHV.Id, MaHocVien = "HV001", HoTen = "HV1" });
        db.SaveChanges();
        var kh = new KhoaHoc { TenKhoaHoc = "KH1", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var tt = new ThanhToan { HocVienId = ndHV.Id, KhoaHocId = kh.Id, SoTien = 2_000_000m, TrangThai = "ChoPheduyet" };
        db.ThanhToans.Add(tt); await db.SaveChangesAsync();

        var ctrl = new ThanhToanController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.ChiTiet(tt.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ThanhToanListItemViewModel>(view.Model);
        Assert.Equal(tt.Id, model.Id);
    }

    // ─── LopHocController.Details ─────────────────────────────────────────────

    [Fact]
    public async Task LopHoc_Details_ValidId_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var kh = new KhoaHoc { TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        var lop = new LopHoc { TenLop = "Lớp Test", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();

        // Add a HocVien with a DangKy
        var nd = new NguoiDung { Email = "hv@t.com", HoTen = "HV1", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        db.HocViens.Add(new HocVien { Id = nd.Id, MaHocVien = "HV001", HoTen = "HV1" });
        db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = nd.Id, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = dk.Id, DiemTongKet = 8.0 });
        await db.SaveChangesAsync();

        var ctrl = new LopHocController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Details(lop.Id);

        var view = Assert.IsType<ViewResult>(result);
    }

    // ─── DiemSoController.QuanLy redirect ─────────────────────────────────────

    [Fact]
    public void DiemSo_QuanLy_RedirectsToIndex()
    {
        using var db = DbContextFactory.Create();
        var excel = new ExcelService(db);
        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = ctrl.QuanLy(null, null, null, null, 1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    // ─── AdminController.LichSuPhanCong - empty ──────────────────────────────

    [Fact]
    public async Task Admin_LichSuPhanCong_EmptyList_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var excel = new ExcelService(db);
        var pdf = new PdfService(db);
        var ctrl = new AdminController(db, excel, pdf,
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["AppSettings:DefaultPassword"] = "Abc@12345" })
                .Build());
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.LichSuPhanCong(99999);

        var view = Assert.IsType<ViewResult>(result);
    }

    // ─── GoiYController — Index (without service calls) ─────────────────────

    [Fact]
    public async Task GoiY_Index_HocVien_NoExistingGoiY_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        var nd = new NguoiDung { Email = "hv@t.com", HoTen = "HV", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        db.HocViens.Add(new HocVien { Id = nd.Id, MaHocVien = "HV001", HoTen = "HV" });
        await db.SaveChangesAsync();

        // GoiYKhoaHocService needs GeminiService — stub with null values via a real DB context
        // We only test the path where goiYGanNhat is empty (no prior recommendations)
        var goiYService = new GoiYKhoaHocService(db, null!, NullLogger<GoiYKhoaHocService>.Instance);
        var ctrl = new GoiYController(db, goiYService);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(nd.Id, "hv@t.com", "HV", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<GoiYTrangViewModel>(view.Model);
        Assert.False(model.DaGoiY);
    }

    // ─── ThongBaoController.SoChuaDoc ────────────────────────────────────────

    [Fact]
    public async Task ThongBao_SoChuaDoc_ReturnsCount()
    {
        using var db = DbContextFactory.Create();
        db.ThongBaos.Add(new ThongBao { NguoiNhanId = 80, TieuDe = "TB", NoiDung = "nd", LoaiThongBao = "HeThong", DaDoc = false });
        db.ThongBaos.Add(new ThongBao { NguoiNhanId = 80, TieuDe = "TB2", NoiDung = "nd", LoaiThongBao = "HeThong", DaDoc = true });
        await db.SaveChangesAsync();
        var ctrl = new ThongBaoController(db);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(80, "u80@t.com", "User80", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();

        var result = await ctrl.SoChuaDoc();

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(1, json.Value);
    }
}
