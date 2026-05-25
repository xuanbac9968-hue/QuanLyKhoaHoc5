using Microsoft.AspNetCore.Mvc;
using Moq;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Services;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Unit test cho DangKyController:
/// - DangKy (lớp không tồn tại, lớp đóng, lớp đầy, đã đăng ký, thành công)
/// - Huy (không phải chủ, trạng thái không hợp lệ, thành công)
/// - Duyet (Admin duyệt đơn)
/// - TuChoi (Admin từ chối đơn)
/// </summary>
public class DangKyControllerTests
{
    // ─── Setup: tạo DB với 1 HocVien + admin ──────────────────────────────────────

    private record TestFixture(
        AppDbContext Db,
        DangKyController Ctrl,
        int HocVienId = 10,
        int AdminId = 1);

    private static TestFixture SetupHocVien()
    {
        var db = DbContextFactory.Create();

        // Admin
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = 1, Email = "admin@test.com", HoTen = "Admin", VaiTro = "Admin",
            MatKhauHash = "x", IsActive = true
        });
        // HocVien
        db.NguoiDungs.Add(new NguoiDung
        {
            Id = 10, Email = "hv@test.com", HoTen = "Học Viên Test", VaiTro = "HocVien",
            MatKhauHash = "x", IsActive = true
        });
        db.HocViens.Add(new HocVien { Id = 10, MaHocVien = "HV001", HoTen = "Học Viên Test" });
        db.SaveChanges();

        var thongBao = new ThongBaoService(db);
        var ctrl = new DangKyController(db, thongBao);
        var hvUser = ControllerHelper.CreateUser(10, "hv@test.com", "Học Viên Test", "HocVien");
        ctrl.ControllerContext = ControllerHelper.CreateContext(hvUser);
        ctrl.TempData = ControllerHelper.CreateTempData();

        return new TestFixture(db, ctrl);
    }

    private static (KhoaHoc kh, LopHoc lop) SeedLop(
        AppDbContext db,
        string trangThaiLop = "DangTuyenSinh",
        int siSoToiDa = 20)
    {
        var kh = new KhoaHoc
        {
            TenKhoaHoc = "KH Test", NgonNgu = "Tiếng Anh", TrinhDo = "Sơ cấp", ThoiLuong = 10
        };
        db.KhoaHocs.Add(kh);
        var lop = new LopHoc
        {
            TenLop = "Lớp Test", KhoaHocId = kh.Id,
            TrangThai = trangThaiLop, SiSoToiDa = siSoToiDa
        };
        db.LopHocs.Add(lop);
        db.SaveChanges();
        return (kh, lop);
    }

    // ─── DangKy ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DangKy_NonExistentLop_SetsErrorAndRedirects()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var result = await f.Ctrl.DangKy(99999);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CuaToi", redirect.ActionName);
            Assert.True(f.Ctrl.TempData.ContainsKey("Error"));
        }
    }

    [Fact]
    public async Task DangKy_ClosedLop_SetsErrorAndRedirects()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db, trangThaiLop: "DaKetThuc");

            var result = await f.Ctrl.DangKy(lop.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CuaToi", redirect.ActionName);
            Assert.True(f.Ctrl.TempData.ContainsKey("Error"));
        }
    }

    [Fact]
    public async Task DangKy_FullClass_SetsErrorAndRedirects()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            // Tạo học viên thứ 2 để lấp đầy lớp
            f.Db.NguoiDungs.Add(new NguoiDung { Id = 20, Email = "hv2@test.com", HoTen = "HV2", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true });
            f.Db.HocViens.Add(new HocVien { Id = 20, MaHocVien = "HV002", HoTen = "HV2" });
            var (_, lop) = SeedLop(f.Db, trangThaiLop: "DangTuyenSinh", siSoToiDa: 1);
            f.Db.DangKyKhoaHocs.Add(new DangKyKhoaHoc { HocVienId = 20, LopHocId = lop.Id, TrangThai = "DaDuyet" });
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.DangKy(lop.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(f.Ctrl.TempData.ContainsKey("Error"));
        }
    }

    [Fact]
    public async Task DangKy_AlreadyRegisteredActive_SetsErrorAndRedirects()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            f.Db.DangKyKhoaHocs.Add(new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "ChoDuyet"
            });
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.DangKy(lop.Id);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(f.Ctrl.TempData.ContainsKey("Error"));
        }
    }

    [Fact]
    public async Task DangKy_ReusesCancelledRegistration()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            // Đăng ký cũ đã bị hủy
            var existingDk = new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "DaHuy"
            };
            f.Db.DangKyKhoaHocs.Add(existingDk);
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.DangKy(lop.Id);

            // Phải redirect thành công
            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(f.Ctrl.TempData.ContainsKey("Success"));

            // Row cũ được tái sử dụng (vẫn 1 row, không phải 2)
            var dks = f.Db.DangKyKhoaHocs.Where(d => d.HocVienId == f.HocVienId).ToList();
            Assert.Single(dks);
            Assert.Equal("ChoDuyet", dks[0].TrangThai);
        }
    }

    [Fact]
    public async Task DangKy_NewValidRegistration_CreatesRecord()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);

            var result = await f.Ctrl.DangKy(lop.Id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(f.Ctrl.TempData.ContainsKey("Success"));
            Assert.Single(f.Db.DangKyKhoaHocs);
            Assert.Equal("ChoDuyet", f.Db.DangKyKhoaHocs.First().TrangThai);
        }
    }

    // ─── Huy ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Huy_NotFound_ReturnsNotFound()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var result = await f.Ctrl.Huy(99999);
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    public async Task Huy_NotOwner_ReturnsForbid()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            f.Db.NguoiDungs.Add(new NguoiDung { Id = 99, Email = "other@test.com", HoTen = "Other", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true });
            f.Db.HocViens.Add(new HocVien { Id = 99, MaHocVien = "HV099", HoTen = "Other" });
            var (_, lop) = SeedLop(f.Db);
            var dk = new DangKyKhoaHoc { HocVienId = 99, LopHocId = lop.Id, TrangThai = "ChoDuyet" };
            f.Db.DangKyKhoaHocs.Add(dk);
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.Huy(dk.Id);

            Assert.IsType<ForbidResult>(result);
        }
    }

    [Fact]
    public async Task Huy_AlreadyApproved_SetsErrorTempData()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            var dk = new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "DaDuyet"
            };
            f.Db.DangKyKhoaHocs.Add(dk);
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.Huy(dk.Id);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(f.Ctrl.TempData.ContainsKey("Error"));
        }
    }

    [Fact]
    public async Task Huy_OwnPendingRegistration_SetsStatusDaHuy()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            var dk = new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "ChoDuyet"
            };
            f.Db.DangKyKhoaHocs.Add(dk);
            await f.Db.SaveChangesAsync();

            var result = await f.Ctrl.Huy(dk.Id);

            Assert.IsType<RedirectToActionResult>(result);
            var updated = f.Db.DangKyKhoaHocs.Find(dk.Id)!;
            Assert.Equal("DaHuy", updated.TrangThai);
        }
    }

    // ─── Duyet (Admin) ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Duyet_ValidId_SetsDaDuyetAndCreatesDiemRecord()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            var dk = new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "ChoDuyet"
            };
            f.Db.DangKyKhoaHocs.Add(dk);
            await f.Db.SaveChangesAsync();

            // Switch to Admin context
            var adminUser = ControllerHelper.CreateUser(f.AdminId, "admin@test.com", "Admin", "Admin");
            var thongBao = new ThongBaoService(f.Db);
            var adminCtrl = new DangKyController(f.Db, thongBao);
            adminCtrl.ControllerContext = ControllerHelper.CreateContext(adminUser);
            adminCtrl.TempData = ControllerHelper.CreateTempData();

            var result = await adminCtrl.Duyet(dk.Id);

            Assert.IsType<RedirectToActionResult>(result);
            var updatedDk = f.Db.DangKyKhoaHocs.Find(dk.Id)!;
            Assert.Equal("DaDuyet", updatedDk.TrangThai);
            Assert.True(f.Db.Diems.Any(d => d.DangKyId == dk.Id));
        }
    }

    // ─── TuChoi (Admin) ──────────────────────────────────────────────────────────

    [Fact]
    public async Task TuChoi_ValidId_SetsTuChoiAndSavesLyDo()
    {
        var f = SetupHocVien();
        using (f.Db)
        {
            var (_, lop) = SeedLop(f.Db);
            var dk = new DangKyKhoaHoc
            {
                HocVienId = f.HocVienId, LopHocId = lop.Id, TrangThai = "ChoDuyet"
            };
            f.Db.DangKyKhoaHocs.Add(dk);
            await f.Db.SaveChangesAsync();

            var adminUser = ControllerHelper.CreateUser(f.AdminId, "admin@test.com", "Admin", "Admin");
            var thongBao = new ThongBaoService(f.Db);
            var adminCtrl = new DangKyController(f.Db, thongBao);
            adminCtrl.ControllerContext = ControllerHelper.CreateContext(adminUser);
            adminCtrl.TempData = ControllerHelper.CreateTempData();

            const string lyDo = "Không đủ điều kiện đầu vào";
            var result = await adminCtrl.TuChoi(dk.Id, lyDo);

            Assert.IsType<RedirectToActionResult>(result);
            var updatedDk = f.Db.DangKyKhoaHocs.Find(dk.Id)!;
            Assert.Equal("TuChoi", updatedDk.TrangThai);
            Assert.Equal(lyDo, updatedDk.LyDoTuChoi);
        }
    }
}
