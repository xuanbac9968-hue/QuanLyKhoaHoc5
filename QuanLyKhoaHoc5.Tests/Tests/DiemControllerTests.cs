using Microsoft.AspNetCore.Mvc;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho DiemController:
/// - NhapDiem (không tìm thấy, điểm bị khóa bởi GV, valid)
/// - CuaToi (HocVien xem điểm)
/// - KhoaDiem (Admin khóa điểm lớp)
/// - Công thức tổng kết và xếp loại được lưu đúng
/// </summary>
public class DiemControllerTests
{
    // ─── Setup ───────────────────────────────────────────────────────────────────

    private record DiemFixture(
        QuanLyKhoaHoc5.Web.Data.AppDbContext Db,
        Diem DiemRecord,
        int LopHocId,
        int HocVienId = 1);

    private static DiemFixture Setup()
    {
        var db = DbContextFactory.Create();

        var nd = new NguoiDung { Id = 1, Email = "hv@test.com", HoTen = "HV Test", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd);
        var hv = new HocVien { Id = 1, MaHocVien = "HV001", HoTen = "HV Test" };
        db.HocViens.Add(hv);

        var kh = new KhoaHoc { TenKhoaHoc = "KH Test", NgonNgu = "Tiếng Anh", TrinhDo = "Sơ cấp", ThoiLuong = 10 };
        db.KhoaHocs.Add(kh);
        var lop = new LopHoc { TenLop = "Lớp A", KhoaHocId = kh.Id, TrangThai = "DangHoc" };
        db.LopHocs.Add(lop);
        db.SaveChanges();

        var dk = new DangKyKhoaHoc { HocVienId = 1, LopHocId = lop.Id, TrangThai = "DaDuyet" };
        db.DangKyKhoaHocs.Add(dk);
        db.SaveChanges();

        var diem = new Diem { DangKyId = dk.Id };
        db.Diems.Add(diem);
        db.SaveChanges();

        return new DiemFixture(db, diem, lop.Id);
    }

    private static DiemController MakeController(
        QuanLyKhoaHoc5.Web.Data.AppDbContext db,
        int userId, string role)
    {
        var excelSvc = new ExcelService(db);
        var ctrl = new DiemController(db, excelSvc);
        var user = ControllerHelper.CreateUser(userId, $"{role.ToLower()}@test.com", role, role);
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    // ─── NhapDiem ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NhapDiem_InvalidDangKyId_ReturnsNotFound()
    {
        using var db = DbContextFactory.Create();
        var ctrl = MakeController(db, 99, "Admin");

        var result = await ctrl.NhapDiem(99999, 8.0, 9.0, null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task NhapDiem_LockedDiem_GiangVienNotAdmin_ReturnsFailJson()
    {
        var f = Setup();
        using (f.Db)
        {
            // Khóa điểm
            f.DiemRecord.IsKhoa = true;
            await f.Db.SaveChangesAsync();

            var ctrl = MakeController(f.Db, 5, "GiangVien");

            var result = await ctrl.NhapDiem(f.DiemRecord.DangKyId, 7.0, 7.0, null);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(false, GetProp(json.Value!, "success"));
        }
    }

    [Fact]
    public async Task NhapDiem_ValidScores_CalculatesFormula()
    {
        var f = Setup();
        using (f.Db)
        {
            var ctrl = MakeController(f.Db, 99, "Admin");

            var result = await ctrl.NhapDiem(f.DiemRecord.DangKyId, 8.0, 9.0, "Xuất sắc!");

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(true, GetProp(json.Value!, "success"));

            var updated = f.Db.Diems.Find(f.DiemRecord.Id)!;
            Assert.Equal(8.0, updated.DiemGiuaKy);
            Assert.Equal(9.0, updated.DiemCuoiKy);
            Assert.Equal(8.70, updated.DiemTongKet!.Value, precision: 2);
            Assert.Equal("Xuất sắc", updated.XepLoai);
            Assert.Equal("Xuất sắc!", updated.NhanXetGiangVien);
        }
    }

    [Fact]
    public async Task NhapDiem_NullScores_TongKetIsNull_XepLoaiIsNull()
    {
        var f = Setup();
        using (f.Db)
        {
            var ctrl = MakeController(f.Db, 99, "Admin");

            var result = await ctrl.NhapDiem(f.DiemRecord.DangKyId, null, null, "Chưa thi");

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(true, GetProp(json.Value!, "success"));

            var updated = f.Db.Diems.Find(f.DiemRecord.Id)!;
            Assert.Null(updated.DiemTongKet);
            Assert.Null(updated.XepLoai);
        }
    }

    [Fact]
    public async Task NhapDiem_Scores_YeuXepLoai()
    {
        var f = Setup();
        using (f.Db)
        {
            var ctrl = MakeController(f.Db, 99, "Admin");

            // GK=2, CK=3 → TK = 2*0.3 + 3*0.7 = 0.6 + 2.1 = 2.7 → Yếu
            await ctrl.NhapDiem(f.DiemRecord.DangKyId, 2.0, 3.0, null);

            var updated = f.Db.Diems.Find(f.DiemRecord.Id)!;
            Assert.Equal("Yếu", updated.XepLoai);
        }
    }

    [Fact]
    public async Task NhapDiem_AdminCanEditLockedDiem()
    {
        var f = Setup();
        using (f.Db)
        {
            // Lock
            f.DiemRecord.IsKhoa = true;
            await f.Db.SaveChangesAsync();

            // Admin vẫn được chỉnh
            var ctrl = MakeController(f.Db, 99, "Admin");

            var result = await ctrl.NhapDiem(f.DiemRecord.DangKyId, 10.0, 10.0, null);

            var json = Assert.IsType<JsonResult>(result);
            Assert.Equal(true, GetProp(json.Value!, "success"));
        }
    }

    // ─── CuaToi ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CuaToi_ReturnsViewWithList()
    {
        var f = Setup();
        using (f.Db)
        {
            var ctrl = MakeController(f.Db, f.HocVienId, "HocVien");

            var result = await ctrl.CuaToi();

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model);
        }
    }

    [Fact]
    public async Task CuaToi_EmptyDb_ReturnsEmptyList()
    {
        using var db = DbContextFactory.Create();
        // User 99 không có bản ghi điểm nào
        var ctrl = MakeController(db, 99, "HocVien");

        var result = await ctrl.CuaToi();

        var view = Assert.IsType<ViewResult>(result);
        var list = view.Model as System.Collections.IEnumerable;
        Assert.NotNull(list);
    }

    // ─── KhoaDiem ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task KhoaDiem_SetsIsKhoaTrue_ForAllDiemsInLop()
    {
        var f = Setup();
        using (f.Db)
        {
            var ctrl = MakeController(f.Db, 99, "Admin");

            var result = await ctrl.KhoaDiem(f.LopHocId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LopHoc", redirect.ActionName);

            var diem = f.Db.Diems.Find(f.DiemRecord.Id)!;
            Assert.True(diem.IsKhoa);
        }
    }

    // ─── Utility ─────────────────────────────────────────────────────────────────

    private static object? GetProp(object obj, string propName)
        => obj.GetType().GetProperty(propName)?.GetValue(obj);
}
