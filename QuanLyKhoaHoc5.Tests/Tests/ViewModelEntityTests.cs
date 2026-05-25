using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Bao phủ các ViewModel và Entity bằng cách khởi tạo và truy cập property.
/// Mục tiêu: đưa tất cả class 0% vào vùng covered để tăng tổng coverage.
/// </summary>
public class ViewModelEntityTests
{
    // ─── HocVienViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void HocVienCreateEditViewModel_Properties_SetAndGet()
    {
        var vm = new HocVienCreateEditViewModel
        {
            Id = 0, HoTen = "Nguyễn A", Email = "a@b.com",
            SoDienThoai = "0901", NgaySinh = new DateOnly(2000, 1, 15),
            GioiTinh = "Nam", DiaChi = "Hà Nội",
            TrinhDoHienTai = "Sơ cấp", NgonNguQuanTam = "Tiếng Anh",
            GhiChu = "ghi chú"
        };
        Assert.Equal("Nguyễn A", vm.HoTen);
        Assert.False(vm.IsEdit);  // Id == 0

        vm.Id = 5;
        Assert.True(vm.IsEdit);  // Id > 0
    }

    [Fact]
    public void HocVienListViewModel_Properties_SetAndGet()
    {
        var vm = new HocVienListViewModel
        {
            Id = 1, MaHocVien = "HV001", HoTen = "Học Viên", Email = "hv@t.com",
            SoDienThoai = "0902", TrinhDoHienTai = "Trung cấp",
            NgonNguQuanTam = "Tiếng Nhật", IsActive = true,
            NgayDangKy = DateTime.Now, SoKhoaDaDangKy = 3
        };
        Assert.Equal("HV001", vm.MaHocVien);
        Assert.Equal(3, vm.SoKhoaDaDangKy);
        Assert.True(vm.IsActive);
    }

    [Fact]
    public void HocVienDetailsViewModel_Properties_SetAndGet()
    {
        var vm = new HocVienDetailsViewModel
        {
            Id = 2, MaHocVien = "HV002", HoTen = "HV Test", Email = "hvt@test.com",
            SoDienThoai = "0903", AnhDaiDien = "/imgs/avatar.jpg",
            NgaySinh = new DateOnly(1999, 5, 20), GioiTinh = "Nữ",
            DiaChi = "TP HCM", TrinhDoHienTai = "Cao cấp",
            NgonNguQuanTam = "Tiếng Pháp", IsActive = true,
            NgayDangKy = DateTime.Now,
            LichSuDangKy = [new DangKyItemViewModel { Id = 1, TenLop = "Lớp A" }]
        };
        Assert.Equal(1, vm.LichSuDangKy.Count);
        Assert.Equal("HV Test", vm.HoTen);
    }

    [Fact]
    public void DangKyItemViewModel_Properties_SetAndGet()
    {
        var vm = new DangKyItemViewModel
        {
            Id = 10, KhoaHocId = 1, TenLop = "Lớp A", TenKhoaHoc = "Tiếng Anh",
            NgonNgu = "Tiếng Anh", TrangThai = "DaDuyet",
            NgayDangKy = DateTime.Now, DiemTongKet = 8.5,
            XepLoai = "Giỏi", LyDoTuChoi = null
        };
        Assert.Equal(8.5, vm.DiemTongKet);
        Assert.Equal("Giỏi", vm.XepLoai);
        Assert.Null(vm.LyDoTuChoi);
    }

    // ─── GiangVienViewModel ───────────────────────────────────────────────────

    [Fact]
    public void GiangVienCreateEditViewModel_Properties_SetAndGet()
    {
        var vm = new GiangVienCreateEditViewModel
        {
            Id = 0, HoTen = "GV Test", Email = "gv@t.com",
            SoDienThoai = "0904", ChuyenMon = "IELTS", BangCap = "Thạc sĩ",
            KinhNghiem = 5, MoTa = "Kinh nghiệm giảng dạy 5 năm"
        };
        Assert.False(vm.IsEdit);

        vm.Id = 1;
        Assert.True(vm.IsEdit);
        Assert.Equal("IELTS", vm.ChuyenMon);
    }

    [Fact]
    public void GiangVienListViewModel_Properties_SetAndGet()
    {
        var vm = new GiangVienListViewModel
        {
            Id = 3, MaGiangVien = "GV003", HoTen = "Trần B", Email = "tb@t.com",
            SoDienThoai = "0905", ChuyenMon = "TOEFL", BangCap = "Tiến sĩ",
            KinhNghiem = 10, IsActive = true, SoLopDangDay = 2
        };
        Assert.Equal("GV003", vm.MaGiangVien);
        Assert.Equal(2, vm.SoLopDangDay);
    }

    // ─── DangKyViewModel ──────────────────────────────────────────────────────

    [Fact]
    public void DangKyFilterViewModel_Properties_SetAndGet()
    {
        var vm = new DangKyFilterViewModel
        {
            TrangThai = "ChoDuyet", LopHocId = 1, Search = "abc",
            Page = 2, PageSize = 15, TotalItems = 30,
            Items = [new DangKyListViewModel { Id = 1 }]
        };
        Assert.Equal("ChoDuyet", vm.TrangThai);
        Assert.Equal(2, vm.Page);
        Assert.Single(vm.Items);
    }

    [Fact]
    public void DangKyListViewModel_Properties_SetAndGet()
    {
        var vm = new DangKyListViewModel
        {
            Id = 5, MaHocVien = "HV005", TenHocVien = "Học Viên 5",
            TenLop = "Lớp B", TenKhoaHoc = "Tiếng Pháp", NgonNgu = "Pháp",
            TrangThai = "DaDuyet", NgayDangKy = DateTime.Now,
            NgayDuyet = DateTime.Now, TenNguoiDuyet = "Admin",
            LyDoTuChoi = null
        };
        Assert.Equal("DaDuyet", vm.TrangThai);
        Assert.Null(vm.LyDoTuChoi);
    }

    // ─── ThanhToanViewModel ───────────────────────────────────────────────────

    [Fact]
    public void ThanhToanCreateViewModel_Properties_SetAndGet()
    {
        var vm = new ThanhToanCreateViewModel
        {
            KhoaHocId = 1, TenKhoaHoc = "Tiếng Anh", HocPhi = 3_000_000m,
            HoTenHocVien = "Học Viên", PhuongThuc = "TienMat", GhiChu = "test"
        };
        Assert.Equal(3_000_000m, vm.HocPhi);
        Assert.Equal("TienMat", vm.PhuongThuc);
    }

    [Fact]
    public void ThanhToanListItemViewModel_Properties_SetAndGet()
    {
        var vm = new ThanhToanListItemViewModel
        {
            Id = 1, HocVienId = 10, MaHocVien = "HV010", TenHocVien = "HV Ten",
            TenKhoaHoc = "KH", SoTien = 5_000_000m, PhuongThuc = "ChuyenKhoan",
            TrangThai = "DaThanhToan", GhiChu = "ok", NgayTao = DateTime.Now,
            NgayDuyet = DateTime.Now, TenNguoiDuyet = "Admin"
        };
        Assert.Equal("DaThanhToan", vm.TrangThai);
        Assert.Equal(5_000_000m, vm.SoTien);
    }

    [Fact]
    public void ThanhToanDuyetViewModel_Properties_SetAndGet()
    {
        var vm = new ThanhToanDuyetViewModel { Id = 7, HanhDong = "DaThanhToan", GhiChu = "xác nhận" };
        Assert.Equal(7, vm.Id);
        Assert.Equal("DaThanhToan", vm.HanhDong);
    }

    [Fact]
    public void HocVienThanhToanViewModel_Properties_SetAndGet()
    {
        var vm = new HocVienThanhToanViewModel
        {
            KhoaHocId = 1, TenKhoaHoc = "KH", NgonNgu = "Anh", TrinhDo = "Sơ cấp",
            HocPhi = 2_000_000m, TrangThaiKhoaHoc = "DangMo",
            TrangThaiThanhToan = "ChoPheduyet", NgayTaoThanhToan = DateTime.Now,
            PhuongThuc = "TienMat", ThanhToanId = 5, CoDangKy = true
        };
        Assert.True(vm.CoDangKy);
        Assert.Equal("ChoPheduyet", vm.TrangThaiThanhToan);
    }

    [Fact]
    public void ThongKeThanhToanViewModel_Properties_SetAndGet()
    {
        var vm = new ThongKeThanhToanViewModel
        {
            TongThuThang = 10_000_000m, SoLuongDaThanhToan = 3,
            SoLuongChoPheduyet = 2, SoLuongTuChoi = 1, Thang = 5, Nam = 2026
        };
        Assert.Equal(3, vm.SoLuongDaThanhToan);
        Assert.Equal(5, vm.Thang);
    }

    [Fact]
    public void AdminThanhToanFilterViewModel_Properties_SetAndGet()
    {
        var vm = new AdminThanhToanFilterViewModel
        {
            TrangThai = "ChoPheduyet", Page = 1, PageSize = 20, TotalItems = 5,
            Items = [new ThanhToanListItemViewModel { Id = 1 }]
        };
        Assert.Equal("ChoPheduyet", vm.TrangThai);
        Assert.Single(vm.Items);
    }

    // ─── DiemSoViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void DiemSoFilterViewModel_Properties_SetAndGet()
    {
        var vm = new DiemSoFilterViewModel
        {
            KyHocId = 1, KhoaHocId = 2, LopHocId = 3,
            TimKiem = "test", Page = 1, PageSize = 20, TotalItems = 10
        };
        vm.Items.Add(new DiemSoRowViewModel { DiemId = 1 });
        vm.ThongKe.TongHocVien = 10;
        Assert.Single(vm.Items);
        Assert.Equal(10, vm.ThongKe.TongHocVien);
    }

    [Fact]
    public void DiemSoRowViewModel_Properties_SetAndGet()
    {
        var vm = new DiemSoRowViewModel
        {
            DiemId = 1, DangKyId = 5, MaHocVien = "HV001", TenHocVien = "Test",
            TenKhoaHoc = "KH", TenLop = "Lớp A", TenGiangVien = "GV",
            TenKyHoc = "Kỳ 1", DiemGiuaKy = 7.0, DiemCuoiKy = 8.0,
            DiemTongKet = 7.6, XepLoai = "Khá", IsKhoa = false,
            NgayCapNhat = DateTime.Now
        };
        Assert.Equal(7.6, vm.DiemTongKet);
        Assert.Equal("Khá", vm.XepLoai);
        Assert.False(vm.IsKhoa);
    }

    [Fact]
    public void DiemSoThongKeViewModel_TiLeDat_Computed()
    {
        var vm = new DiemSoThongKeViewModel
        {
            DiemTrungBinh = 7.5, SoHocVienDat = 8,
            SoHocVienChuaDat = 2, TongHocVien = 10,
            Histogram = new int[10]
        };
        Assert.Equal(80.0, vm.TiLeDat);

        // Zero total guard
        var vm2 = new DiemSoThongKeViewModel { TongHocVien = 0 };
        Assert.Equal(0, vm2.TiLeDat);
    }

    [Fact]
    public void NhapDiemBatchViewModel_Properties_SetAndGet()
    {
        var vm = new NhapDiemBatchViewModel
        {
            LopHocId = 1, TenLop = "Lớp A", TenKhoaHoc = "KH", IsKhoa = false,
            TenKyHoc = "Kỳ 1", KyDaDong = false,
            HocViens = [new NhapDiemHangViewModel { DangKyId = 1 }]
        };
        Assert.Single(vm.HocViens);
        Assert.False(vm.IsKhoa);
    }

    [Fact]
    public void NhapDiemHangViewModel_Properties_SetAndGet()
    {
        var vm = new NhapDiemHangViewModel
        {
            DangKyId = 3, DiemId = 7, MaHocVien = "HV003", TenHocVien = "Test",
            DiemGiuaKy = 6.5, DiemCuoiKy = 7.0, DiemTongKet = 6.8,
            XepLoai = "Trung bình khá", NhanXet = "Cần cố gắng", IsKhoa = false
        };
        Assert.Equal(6.5, vm.DiemGiuaKy);
        Assert.Equal("Trung bình khá", vm.XepLoai);
    }

    [Fact]
    public void DiemSoKyHocViewModel_DiemTrungBinhKy_Computed()
    {
        var vm = new DiemSoKyHocViewModel
        {
            KyHocId = 1, TenKy = "Kỳ 1",
            Mons = [
                new DiemSoMonViewModel { DiemTongKet = 8.0 },
                new DiemSoMonViewModel { DiemTongKet = 6.0 }
            ]
        };
        Assert.Equal(7.0, vm.DiemTrungBinhKy);

        // No grades
        var vm2 = new DiemSoKyHocViewModel { Mons = [new DiemSoMonViewModel { DiemTongKet = null }] };
        Assert.Null(vm2.DiemTrungBinhKy);
    }

    [Fact]
    public void DiemSoMonViewModel_Properties_SetAndGet()
    {
        var vm = new DiemSoMonViewModel
        {
            TenKhoaHoc = "Tiếng Nhật", TenLop = "N3", NgonNgu = "Nhật",
            DiemGiuaKy = 7.5, DiemCuoiKy = 8.0, DiemTongKet = 7.8,
            XepLoai = "Khá", NhanXet = "Tốt"
        };
        Assert.Equal("Tiếng Nhật", vm.TenKhoaHoc);
        Assert.Equal(7.8, vm.DiemTongKet);
    }

    // ─── KhoaHocViewModel ────────────────────────────────────────────────────

    [Fact]
    public void KhoaHocFilterViewModel_Properties_SetAndGet()
    {
        var vm = new KhoaHocFilterViewModel
        {
            NgonNgu = "Tiếng Nhật", TrinhDo = "Sơ cấp", TrangThai = "DangMo",
            Search = "abc", Page = 1, PageSize = 10, TotalItems = 25,
            Items = [new KhoaHocListViewModel { Id = 1 }]
        };
        Assert.Equal("DangMo", vm.TrangThai);
        Assert.Single(vm.Items);
    }

    // ─── LopHocViewModel ─────────────────────────────────────────────────────

    [Fact]
    public void LopHocDetailsViewModel_Properties_SetAndGet()
    {
        var vm = new LopHocDetailsViewModel
        {
            Id = 1, TenLop = "Lớp A", TenKhoaHoc = "Tiếng Anh", NgonNgu = "Anh",
            TrinhDo = "Sơ cấp", TenGiangVien = "GV A",
            NgayKhaiGiang = new DateOnly(2026, 6, 1), NgayKetThuc = new DateOnly(2026, 12, 1),
            SiSoToiDa = 20, PhongHoc = "P101", TrangThai = "DangTuyenSinh",
            GhiChu = "ghi chú",
            DanhSachHocVien = [new HocVienTrongLopViewModel { HocVienId = 1 }],
            LichHocs = [new LichHocItemViewModel { Id = 1 }]
        };
        Assert.Single(vm.DanhSachHocVien);
        Assert.Single(vm.LichHocs);
        Assert.Equal("P101", vm.PhongHoc);
    }

    [Fact]
    public void HocVienTrongLopViewModel_Properties_SetAndGet()
    {
        var vm = new HocVienTrongLopViewModel
        {
            HocVienId = 5, DangKyId = 10, MaHocVien = "HV005", HoTen = "Test",
            Email = "t@t.com", TrangThaiDangKy = "DaDuyet",
            NgayDangKy = DateTime.Now, DiemTongKet = 9.0, XepLoai = "Xuất sắc"
        };
        Assert.Equal(9.0, vm.DiemTongKet);
        Assert.Equal("Xuất sắc", vm.XepLoai);
    }

    // ─── GoiYViewModels ──────────────────────────────────────────────────────

    [Fact]
    public void GoiYKetQuaViewModel_Properties_SetAndGet()
    {
        var vm = new GoiYKetQuaViewModel
        {
            KhoaHocId = 2, TenKhoaHoc = "Tiếng Hàn", DiemPhuHop = 0.95,
            LyDo = "Phù hợp với trình độ", HocPhi = 4_000_000m, AnhBia = "/img/kh.jpg"
        };
        Assert.Equal(0.95, vm.DiemPhuHop);
        Assert.Equal(4_000_000m, vm.HocPhi);
    }

    [Fact]
    public void KhoaHocGoiYInputViewModel_Properties_SetAndGet()
    {
        var vm = new KhoaHocGoiYInputViewModel
        {
            Id = 3, TenKhoaHoc = "TOEFL", NgonNgu = "Tiếng Anh",
            TrinhDo = "Cao cấp", HocPhi = 6_000_000m, MoTa = "Luyện thi TOEFL"
        };
        Assert.Equal(6_000_000m, vm.HocPhi);
        Assert.Equal("TOEFL", vm.TenKhoaHoc);
    }

    // ─── DiemViewModel ────────────────────────────────────────────────────────

    [Fact]
    public void NhapDiemViewModel_Properties_SetAndGet()
    {
        var vm = new NhapDiemViewModel
        {
            DangKyId = 1, HocVienId = 5, MaHocVien = "HV005", TenHocVien = "Test",
            DiemId = 3, DiemGiuaKy = 6.0, DiemCuoiKy = 7.0, DiemTongKet = 6.6,
            XepLoai = "Trung bình", NhanXetGiangVien = "Cần cố gắng thêm", IsKhoa = false
        };
        Assert.Equal(6.6, vm.DiemTongKet);
        Assert.False(vm.IsKhoa);
    }

    [Fact]
    public void BangDiemLopViewModel_Properties_SetAndGet()
    {
        var vm = new BangDiemLopViewModel
        {
            LopHocId = 1, TenLop = "Lớp A", TenKhoaHoc = "KH", TenGiangVien = "GV",
            IsKhoa = false, CanEdit = true,
            DanhSachDiem = [new NhapDiemViewModel { DangKyId = 1 }]
        };
        Assert.True(vm.CanEdit);
        Assert.Single(vm.DanhSachDiem);
    }

    [Fact]
    public void DiemCuaToiViewModel_Properties_SetAndGet()
    {
        var vm = new DiemCuaToiViewModel
        {
            TenLop = "Lớp B", TenKhoaHoc = "Tiếng Trung", NgonNgu = "Hoa",
            DiemGiuaKy = 8.0, DiemCuoiKy = 9.0, DiemTongKet = 8.6,
            XepLoai = "Giỏi", NhanXetGiangVien = "Tốt lắm", TrangThaiDangKy = "DaDuyet"
        };
        Assert.Equal("Giỏi", vm.XepLoai);
        Assert.Equal(8.6, vm.DiemTongKet);
    }

    // ─── PhanCongViewModel ────────────────────────────────────────────────────

    [Fact]
    public void PhanCongViewModel_Properties_SetAndGet()
    {
        var vm = new PhanCongViewModel
        {
            KhoaHocId = 1, TenKhoaHoc = "Tiếng Anh", NgonNgu = "Anh",
            TrinhDo = "Trung cấp", GiangVienIdHienTai = 5, TenGiangVienHienTai = "GV A",
            NgayPhanCong = DateTime.Now, GhiChu = "test", PhanCongId = 10
        };
        Assert.Equal(5, vm.GiangVienIdHienTai);
        Assert.Equal(10, vm.PhanCongId);
    }

    // ─── Entities ─────────────────────────────────────────────────────────────

    [Fact]
    public void GoiYKhoaHoc_Properties_SetAndGet()
    {
        var e = new GoiYKhoaHoc
        {
            Id = 1, HocVienId = 5, KhoaHocGoiYId = 2, DiemPhuHop = 0.85,
            LyDoGoiY = "Phù hợp", NgayGoiY = DateTime.Now
        };
        Assert.Equal(0.85, e.DiemPhuHop);
        Assert.Equal("Phù hợp", e.LyDoGoiY);
    }

    [Fact]
    public void KyHoc_Properties_SetAndGet()
    {
        var e = new KyHoc
        {
            Id = 1, TenKy = "Kỳ 1 – 2026",
            NgayBatDau = new DateOnly(2026, 1, 1),
            NgayKetThuc = new DateOnly(2026, 6, 30),
            TrangThai = "DangMo"
        };
        Assert.Equal("Kỳ 1 – 2026", e.TenKy);
        Assert.Equal("DangMo", e.TrangThai);
    }

    [Fact]
    public void PhanCongGiangDay_Properties_SetAndGet()
    {
        var e = new PhanCongGiangDay
        {
            Id = 1, GiangVienId = 5, KhoaHocId = 2, GhiChu = "Test",
            NgayPhanCong = DateTime.Now, IsActive = true
        };
        Assert.Equal(5, e.GiangVienId);
        Assert.True(e.IsActive);
    }

    [Fact]
    public void ChatHistory_Properties_SetAndGet()
    {
        var e = new ChatHistory
        {
            Id = 1, UserId = 10, Role = "user", Content = "Xin chào",
            CreatedAt = DateTime.Now
        };
        Assert.Equal("user", e.Role);
        Assert.Equal("Xin chào", e.Content);
    }

    [Fact]
    public void ThanhToan_Properties_SetAndGet()
    {
        var e = new ThanhToan
        {
            Id = 1, HocVienId = 5, KhoaHocId = 2,
            SoTien = 3_000_000m, PhuongThuc = "TienMat",
            TrangThai = "ChoPheduyet", GhiChu = "ghi chú",
            NgayTao = DateTime.Now, NgayDuyet = null, NguoiDuyetId = null
        };
        Assert.Equal(3_000_000m, e.SoTien);
        Assert.Equal("ChoPheduyet", e.TrangThai);
        Assert.Null(e.NgayDuyet);
    }
}
