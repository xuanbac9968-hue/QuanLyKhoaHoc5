using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class DangKyListViewModel
{
    public int Id { get; set; }
    public string MaHocVien { get; set; } = "";
    public string TenHocVien { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public DateTime NgayDangKy { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? TenNguoiDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }
}

public class TuChoiViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập lý do từ chối")]
    [Display(Name = "Lý do từ chối")]
    public string LyDo { get; set; } = "";
}

public class DangKyFilterViewModel
{
    public string? TrangThai { get; set; }
    public int? KhoaHocId { get; set; }
    public int? LopHocId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalItems { get; set; }
    public List<DangKyListViewModel> Items { get; set; } = [];
}
