namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TongHocVienDangHoc { get; set; }
    public int TongKhoaHocDangMo { get; set; }
    public int TongGiangVien { get; set; }
    public decimal DoanhThuThang { get; set; }
    public int SoThanhToanChoPheduyet { get; set; }
    public decimal TongThuThanhToan { get; set; } // thu thực tế từ ThanhToan
    public List<ChartDataPoint> SoHocVienTheoKhoa { get; set; } = [];
    public List<ChartDataPoint> DoanhThuTheoThang { get; set; } = [];
    public List<KhoaHocListViewModel> Top5KhoaHoc { get; set; } = [];
    public List<DangKyListViewModel> DangKyGanDay { get; set; } = [];
}

public class ChartDataPoint
{
    public string Label { get; set; } = "";
    public double Value { get; set; }
}

public class ProfileViewModel
{
    public int Id { get; set; }
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string? AnhDaiDien { get; set; }
    public string VaiTro { get; set; } = "";

    public DateOnly? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? TrinhDoHienTai { get; set; }
    public string? NgonNguQuanTam { get; set; }

    public string? ChuyenMon { get; set; }
    public string? BangCap { get; set; }
    public int? KinhNghiem { get; set; }
    public string? MoTa { get; set; }

    public IFormFile? AnhDaiDienFile { get; set; }
}

public class LichHocTuanViewModel
{
    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public List<LichHocChiTietViewModel> Items { get; set; } = [];
}

// Used for displaying a LichHoc item (per-session)
public class LichHocChiTietViewModel
{
    public int Id { get; set; }
    public int LopHocId { get; set; }
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public DateOnly NgayHoc { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? PhongHoc { get; set; }
    public string? ChuDe { get; set; }
    public string? GhiChu { get; set; }

    public string TenThu => NgayHoc.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ Hai", DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư", DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu", DayOfWeek.Saturday => "Thứ Bảy",
        DayOfWeek.Sunday => "Chủ Nhật", _ => ""
    };

    public string CaHoc => GioBatDau.Hour < 12 ? "Sáng" : GioBatDau.Hour < 18 ? "Chiều" : "Tối";
}

// For creating/editing a LichHoc (Admin form)
public class LichHocCreateViewModel
{
    public int LopHocId { get; set; }
    public string NgayHoc { get; set; } = "";
    public string GioBatDau { get; set; } = "08:00";
    public string GioKetThuc { get; set; } = "09:30";
    public string? PhongHoc { get; set; }
    public string? ChuDe { get; set; }
    public string? GhiChu { get; set; }
}

// For bulk session creation
public class TaoHangLoatViewModel
{
    public int LopHocId { get; set; }
    public string NgayBatDau { get; set; } = "";
    public string NgayKetThuc { get; set; } = "";
    public List<int> ThuTrongTuan { get; set; } = []; // 1=Mon,...,7=Sun (DayOfWeek)
    public string GioBatDau { get; set; } = "08:00";
    public string GioKetThuc { get; set; } = "09:30";
    public string? PhongHoc { get; set; }
}

// Phân công giảng viên
public class PhanCongViewModel
{
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrinhDo { get; set; } = "";
    public int? GiangVienIdHienTai { get; set; }
    public string? TenGiangVienHienTai { get; set; }
    public DateTime? NgayPhanCong { get; set; }
    public string? GhiChu { get; set; }
    public int PhanCongId { get; set; }
}

public class PhanCongFormViewModel
{
    public int KhoaHocId { get; set; }
    public int GiangVienId { get; set; }
    public string? GhiChu { get; set; }
}

// Chat
public class ChatMessageViewModel
{
    public string Role { get; set; } = "user"; // "user" | "assistant"
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class ChatRequest
{
    public string Message { get; set; } = "";
}

public class BuoiHocViewModel
{
    public DateOnly NgayHoc { get; set; }
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? PhongHoc { get; set; }

    public string TenThu => NgayHoc.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ Hai", DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư", DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu", DayOfWeek.Saturday => "Thứ Bảy",
        DayOfWeek.Sunday => "Chủ Nhật", _ => ""
    };

    public string CaHoc => GioBatDau.Hour < 12 ? "Sáng" : GioBatDau.Hour < 18 ? "Chiều" : "Tối";
}

// BaoCao
public class BaoCaoFilterViewModel
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
}
