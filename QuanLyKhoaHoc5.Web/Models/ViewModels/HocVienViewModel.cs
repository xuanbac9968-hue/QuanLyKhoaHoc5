using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class HocVienCreateEditViewModel
{
    public int Id { get; set; }
    public bool IsEdit => Id > 0;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Ngày sinh")]
    [DataType(DataType.Date)]
    public DateOnly? NgaySinh { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [Display(Name = "Trình độ hiện tại")]
    public string? TrinhDoHienTai { get; set; }

    [Display(Name = "Ngôn ngữ quan tâm")]
    public string? NgonNguQuanTam { get; set; }

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class HocVienListViewModel
{
    public int Id { get; set; }
    public string MaHocVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string? TrinhDoHienTai { get; set; }
    public string? NgonNguQuanTam { get; set; }
    public bool IsActive { get; set; }
    public DateTime NgayDangKy { get; set; }
    public int SoKhoaDaDangKy { get; set; }
}

public class HocVienDetailsViewModel
{
    public int Id { get; set; }
    public string MaHocVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string? AnhDaiDien { get; set; }
    public DateOnly? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? DiaChi { get; set; }
    public string? TrinhDoHienTai { get; set; }
    public string? NgonNguQuanTam { get; set; }
    public bool IsActive { get; set; }
    public DateTime NgayDangKy { get; set; }
    public List<DangKyItemViewModel> LichSuDangKy { get; set; } = [];
}

public class DangKyItemViewModel
{
    public int Id { get; set; }
    public int KhoaHocId { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public DateTime NgayDangKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
    public string? LyDoTuChoi { get; set; }
}

public class HocVienDashboardViewModel
{
    public string HoTen { get; set; } = "";
    public string MaHocVien { get; set; } = "";
    public string? AnhDaiDien { get; set; }
    public string? TrinhDoHienTai { get; set; }
    public string? NgonNguQuanTam { get; set; }
    public List<DangKyItemViewModel> KhoaHocDangHoc { get; set; } = [];
    public List<BuoiHocViewModel> LichHocTuanNay { get; set; } = [];
    public int SoThongBaoChuaDoc { get; set; }
    public List<GoiYKetQuaViewModel> GoiYGanNhat { get; set; } = [];
}
