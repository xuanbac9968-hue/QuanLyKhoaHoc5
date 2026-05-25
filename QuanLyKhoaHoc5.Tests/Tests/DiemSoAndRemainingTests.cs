using Microsoft.AspNetCore.Mvc;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Tăng coverage cho DiemSoController (NhapDiem, BatchSave, KyHocList, TaoKyHoc, DongKyHoc, GanKyHoc, KhoaDiem)
/// và các controller nhỏ còn lại.
/// </summary>
public class DiemSoAndRemainingTests
{
    // ─── Shared helpers ───────────────────────────────────────────────────────

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

    private static KhoaHoc SeedKhoaHoc(QuanLyKhoaHoc5.Web.Data.AppDbContext db)
    {
        var kh = new KhoaHoc { TenKhoaHoc = "KH Test", NgonNgu = "Anh", TrinhDo = "SC", ThoiLuong = 20 };
        db.KhoaHocs.Add(kh); db.SaveChanges(); return kh;
    }

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

    private static DiemSoController MakeDiemSoHVCtrl(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db, int hvId)
    {
        var excel = new ExcelService(db);
        var ctrl = new DiemSoController(db, excel);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(hvId, $"hv{hvId}@t.com", $"HV{hvId}", "HocVien"));
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — NhapDiem
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_NhapDiem_NoLopHocId_ReturnsViewWithNullModel()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.NhapDiem(null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Null(view.Model);
    }

    [Fact]
    public async Task DiemSo_NhapDiem_InvalidLopHocId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.NhapDiem(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DiemSo_NhapDiem_ValidLop_NoStudents_ReturnsBatchViewModel()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp DS", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.NhapDiem(lop.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NhapDiemBatchViewModel>(view.Model);
        Assert.Equal(lop.Id, model.LopHocId);
        Assert.Empty(model.HocViens);
    }

    [Fact]
    public async Task DiemSo_NhapDiem_ValidLop_WithStudents_ReturnsBatchViewModel()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 300, "hv300@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp DS2", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = 300, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = dk.Id, DiemGiuaKy = 7.0, DiemCuoiKy = 8.0 });
        await db.SaveChangesAsync();

        var ctrl = MakeDiemSoAdminCtrl(db);
        var result = await ctrl.NhapDiem(lop.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<NhapDiemBatchViewModel>(view.Model);
        Assert.Single(model.HocViens);
    }

    [Fact]
    public async Task DiemSo_NhapDiem_GV_OnlySeesOwnLop()
    {
        using var db = DbContextFactory.Create();
        SeedGiangVien(db, 301, "gv301@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp GV", KhoaHocId = kh.Id, GiangVienId = 301, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); await db.SaveChangesAsync();

        var ctrl = MakeDiemSoGVCtrl(db, 301);
        var result = await ctrl.NhapDiem(lop.Id);

        // GV is assigned to this lop → should succeed
        Assert.IsType<ViewResult>(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — BatchSave
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_BatchSave_ValidItems_SavesAndReturnsSuccess()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 302, "hv302@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp BS", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = 302, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        var diem = new Diem { DangKyId = dk.Id };
        db.Diems.Add(diem); await db.SaveChangesAsync();

        var ctrl = MakeDiemSoAdminCtrl(db);
        var result = await ctrl.BatchSave(new BatchSaveDiemRequest
        {
            Items = [new SingleDiemRequest { DangKyId = dk.Id, DiemGiuaKy = 7.0, DiemCuoiKy = 8.0, NhanXet = "Tốt" }]
        });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Equal(1, GetProp(json.Value!, "savedCount"));
    }

    [Fact]
    public async Task DiemSo_BatchSave_InvalidDangKyId_ReturnsPartialSuccess()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.BatchSave(new BatchSaveDiemRequest
        {
            Items = [new SingleDiemRequest { DangKyId = 99999, DiemGiuaKy = 5.0, DiemCuoiKy = 6.0 }]
        });

        var json = Assert.IsType<JsonResult>(result);
        // success=false because there are errors
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task DiemSo_BatchSave_EmptyList_ReturnsSuccess()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.BatchSave(new BatchSaveDiemRequest { Items = [] });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — KhoaDiem (Admin POST)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_KhoaDiem_NoDiems_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.KhoaDiem(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task DiemSo_KhoaDiem_WithDiems_LockAll()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 303, "hv303@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp Khoa", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = 303, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = dk.Id }); await db.SaveChangesAsync();

        var ctrl = MakeDiemSoAdminCtrl(db);
        var result = await ctrl.KhoaDiem(lop.Id);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.True(db.Diems.First().IsKhoa);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — KyHocList, TaoKyHoc, DongKyHoc, GanKyHoc
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_KyHocList_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        db.KyHocs.Add(new KyHoc { TenKy = "Kỳ 1 – 2026", NgayBatDau = new DateOnly(2026, 1, 1), NgayKetThuc = new DateOnly(2026, 6, 30) });
        await db.SaveChangesAsync();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.KyHocList();

        var view = Assert.IsType<ViewResult>(result);
        var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(view.Model);
        Assert.Single(list.Cast<object>());
    }

    [Fact]
    public async Task DiemSo_TaoKyHoc_EmptyTen_SetsError()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.TaoKyHoc("  ", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30));

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Contains("Error", ctrl.TempData.Keys);
        Assert.Empty(db.KyHocs);
    }

    [Fact]
    public async Task DiemSo_TaoKyHoc_Valid_CreatesAndRedirects()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.TaoKyHoc("Kỳ 2 – 2026", new DateOnly(2026, 7, 1), new DateOnly(2026, 12, 31));

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("KyHocList", redirect.ActionName);
        Assert.Single(db.KyHocs);
        Assert.Equal("DangMo", db.KyHocs.First().TrangThai);
    }

    [Fact]
    public async Task DiemSo_DongKyHoc_NotFound_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.DongKyHoc(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DiemSo_DongKyHoc_Valid_ClosesKyHoc()
    {
        using var db = DbContextFactory.Create();
        var ky = new KyHoc { TenKy = "Kỳ 1", NgayBatDau = new DateOnly(2026, 1, 1), NgayKetThuc = new DateOnly(2026, 6, 30), TrangThai = "DangMo" };
        db.KyHocs.Add(ky); await db.SaveChangesAsync();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.DongKyHoc(ky.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("DaDong", db.KyHocs.Find(ky.Id)!.TrangThai);
    }

    [Fact]
    public async Task DiemSo_GanKyHoc_NoMatchingDiems_RedirectsWithNoChange()
    {
        using var db = DbContextFactory.Create();
        var ky = new KyHoc { TenKy = "Kỳ 1", NgayBatDau = new DateOnly(2026, 1, 1), NgayKetThuc = new DateOnly(2026, 6, 30) };
        db.KyHocs.Add(ky); await db.SaveChangesAsync();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.GanKyHoc(99999, ky.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NhapDiem", redirect.ActionName);
    }

    [Fact]
    public async Task DiemSo_GanKyHoc_WithDiems_AssignsKyHoc()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 304, "hv304@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp Gan", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = 304, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = dk.Id });
        var ky = new KyHoc { TenKy = "Kỳ G", NgayBatDau = new DateOnly(2026, 1, 1), NgayKetThuc = new DateOnly(2026, 6, 30) };
        db.KyHocs.Add(ky); await db.SaveChangesAsync();
        var ctrl = MakeDiemSoAdminCtrl(db);

        var result = await ctrl.GanKyHoc(lop.Id, ky.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(ky.Id, db.Diems.First().KyHocId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DiemSoController — CuaToi (HocVien)
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DiemSo_CuaToi_HocVien_NoDiems_ReturnsView()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 305, "hv305@t.com");
        var ctrl = MakeDiemSoHVCtrl(db, 305);

        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DiemSoHocVienViewModel>(view.Model);
        Assert.Empty(model.CacKy);
    }

    [Fact]
    public async Task DiemSo_CuaToi_HocVien_WithDiems_ReturnsViewModel()
    {
        using var db = DbContextFactory.Create();
        SeedHocVien(db, 306, "hv306@t.com");
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp CT", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var dk = new DangKyKhoaHoc { HocVienId = 306, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        db.Diems.Add(new Diem { DangKyId = dk.Id, DiemGiuaKy = 8.0, DiemCuoiKy = 9.0, DiemTongKet = 8.7 });
        await db.SaveChangesAsync();

        var ctrl = MakeDiemSoHVCtrl(db, 306);
        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DiemSoHocVienViewModel>(view.Model);
        Assert.Single(model.CacKy); // 1 group with no KyHoc
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // LichHocController — Delete (POST)
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

    [Fact]
    public async Task LichHoc_Delete_InvalidId_ReturnsFailJson()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeLichHocAdminCtrl(db);

        var result = await ctrl.Delete(99999);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(false, GetProp(json.Value!, "success"));
    }

    [Fact]
    public async Task LichHoc_Delete_ValidId_DeletesAndReturnsJson()
    {
        using var db = DbContextFactory.Create();
        var kh = SeedKhoaHoc(db);
        var lop = new LopHoc { TenLop = "Lớp LH", KhoaHocId = kh.Id, TrangThai = "DangHoc", SiSoToiDa = 10 };
        db.LopHocs.Add(lop); db.SaveChanges();
        var lh = new LichHoc { LopHocId = lop.Id, NgayHoc = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), GioBatDau = new TimeOnly(8, 0), GioKetThuc = new TimeOnly(10, 0) };
        db.LichHocs.Add(lh); await db.SaveChangesAsync();
        var ctrl = MakeLichHocAdminCtrl(db);

        var result = await ctrl.Delete(lh.Id);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(true, GetProp(json.Value!, "success"));
        Assert.Empty(db.LichHocs);
    }

    // ─── Utility ─────────────────────────────────────────────────────────────

    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
