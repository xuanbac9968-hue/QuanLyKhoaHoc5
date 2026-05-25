using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class KhoaHocCreateEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
    [MaxLength(200)]
    [Display(Name = "Tên khóa học")]
    public string TenKhoaHoc { get; set; } = "";

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngôn ngữ")]
    [Display(Name = "Ngôn ngữ")]
    public string NgonNgu { get; set; } = "Tiếng Anh";

    [Required(ErrorMessage = "Vui lòng chọn trình độ")]
    [Display(Name = "Trình độ")]
    public string TrinhDo { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập học phí")]
    [Range(0, 100000000, ErrorMessage = "Học phí không hợp lệ")]
    [Display(Name = "Học phí (VNĐ)")]
    public decimal HocPhi { get; set; }

    [Range(1, 500)]
    [Display(Name = "Số chỗ tối đa")]
    public int SoChoToiDa { get; set; } = 20;

    [Required(ErrorMessage = "Vui lòng nhập thời lượng")]
    [Range(1, 500)]
    [Display(Name = "Số buổi học")]
    public int ThoiLuong { get; set; }

    [Display(Name = "Số buổi/tuần")]
    public int? SoBuoiMoiTuan { get; set; }

    [Display(Name = "Thời gian/buổi (phút)")]
    public int? ThoiGianMoiBuoi { get; set; }

    [Display(Name = "Nội dung chương trình")]
    public string? NoiDungChuongTrinh { get; set; }

    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "DangMo";

    public IFormFile? AnhBiaFile { get; set; }
    public string? AnhBiaHienTai { get; set; }
}

public class KhoaHocListViewModel
{
    public int Id { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrinhDo { get; set; } = "";
    public decimal HocPhi { get; set; }
    public int SoChoToiDa { get; set; }
    public int ThoiLuong { get; set; }
    public string TrangThai { get; set; } = "";
    public string? AnhBia { get; set; }
    public int SoLopHoc { get; set; }
    public int SoHocVien { get; set; }
    public DateTime NgayTao { get; set; }
}

public class KhoaHocFilterViewModel
{
    public string? NgonNgu { get; set; }
    public string? TrinhDo { get; set; }
    public string? TrangThai { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public List<KhoaHocListViewModel> Items { get; set; } = [];
}
