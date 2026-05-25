using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class GiangVienCreateEditViewModel
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

    [Display(Name = "Chuyên môn")]
    public string? ChuyenMon { get; set; }

    [Display(Name = "Bằng cấp")]
    public string? BangCap { get; set; }

    [Display(Name = "Số năm kinh nghiệm")]
    [Range(0, 50)]
    public int? KinhNghiem { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }
}

public class GiangVienListViewModel
{
    public int Id { get; set; }
    public string MaGiangVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SoDienThoai { get; set; }
    public string? ChuyenMon { get; set; }
    public string? BangCap { get; set; }
    public int? KinhNghiem { get; set; }
    public bool IsActive { get; set; }
    public int SoLopDangDay { get; set; }
}

public class GiangVienDashboardViewModel
{
    public string HoTen { get; set; } = "";
    public string MaGiangVien { get; set; } = "";
    public string? AnhDaiDien { get; set; }
    public string? ChuyenMon { get; set; }
    public List<LopHocListViewModel> LopDangDay { get; set; } = [];
    public List<LichHocItemViewModel> LichHomNay { get; set; } = [];
    public List<LichHocItemViewModel> LichTuanNay { get; set; } = [];
    public int SoHocVienTong { get; set; }
    public int SoThongBaoChuaDoc { get; set; }
}
