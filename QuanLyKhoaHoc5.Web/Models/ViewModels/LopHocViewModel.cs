using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class LopHocCreateEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
    [Display(Name = "Tên lớp")]
    public string TenLop { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn khóa học")]
    [Display(Name = "Khóa học")]
    public int KhoaHocId { get; set; }

    [Display(Name = "Giảng viên phụ trách")]
    public int? GiangVienId { get; set; }

    [Display(Name = "Ngày khai giảng")]
    [DataType(DataType.Date)]
    public DateOnly? NgayKhaiGiang { get; set; }

    [Display(Name = "Ngày kết thúc")]
    [DataType(DataType.Date)]
    public DateOnly? NgayKetThuc { get; set; }

    [Required]
    [Range(1, 100)]
    [Display(Name = "Sĩ số tối đa")]
    public int SiSoToiDa { get; set; } = 20;

    [Display(Name = "Phòng học")]
    public string? PhongHoc { get; set; }

    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "ChuaMo";

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class LopHocListViewModel
{
    public int Id { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public DateOnly? NgayKhaiGiang { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public int SiSoToiDa { get; set; }
    public int SiSoHienTai { get; set; }
    public string? PhongHoc { get; set; }
    public string TrangThai { get; set; } = "";
}

public class LopHocDetailsViewModel
{
    public int Id { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrinhDo { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public DateOnly? NgayKhaiGiang { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public int SiSoToiDa { get; set; }
    public string? PhongHoc { get; set; }
    public string TrangThai { get; set; } = "";
    public string? GhiChu { get; set; }
    public List<HocVienTrongLopViewModel> DanhSachHocVien { get; set; } = [];
    public List<LichHocItemViewModel> LichHocs { get; set; } = [];
}

public class HocVienTrongLopViewModel
{
    public int HocVienId { get; set; }
    public int DangKyId { get; set; }
    public string MaHocVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string TrangThaiDangKy { get; set; } = "";
    public DateTime NgayDangKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
}

public class LichHocItemViewModel
{
    public int Id { get; set; }
    public DateOnly NgayHoc { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? PhongHoc { get; set; }
    public string? TenKhoaHoc { get; set; }
    public string? TenLop { get; set; }
    public string? ChuDe { get; set; }

    public string TenThu => NgayHoc.DayOfWeek switch
    {
        DayOfWeek.Monday => "Thứ Hai", DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư", DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu", DayOfWeek.Saturday => "Thứ Bảy",
        DayOfWeek.Sunday => "Chủ Nhật", _ => ""
    };

    public string CaHoc => GioBatDau.Hour < 12 ? "Sáng" : GioBatDau.Hour < 18 ? "Chiều" : "Tối";
}
