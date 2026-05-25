using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class ThanhToanCreateViewModel
{
    [Required]
    public int KhoaHocId { get; set; }

    public string TenKhoaHoc { get; set; } = "";

    public decimal HocPhi { get; set; }

    public string HoTenHocVien { get; set; } = "";

    /// <summary>ChuyenKhoan | TienMat</summary>
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    public string PhuongThuc { get; set; } = "TienMat";

    [MaxLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class ThanhToanListItemViewModel
{
    public int Id { get; set; }
    public int HocVienId { get; set; }
    public string MaHocVien { get; set; } = "";
    public string TenHocVien { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? TenNguoiDuyet { get; set; }
}

public class ThanhToanDuyetViewModel
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string HanhDong { get; set; } = ""; // DaThanhToan | TuChoi

    [MaxLength(500)]
    public string? GhiChu { get; set; }
}

public class HocVienThanhToanViewModel
{
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public string TrinhDo { get; set; } = "";
    public decimal HocPhi { get; set; }
    public string TrangThaiKhoaHoc { get; set; } = "";
    // Trạng thái thanh toán gần nhất
    public string? TrangThaiThanhToan { get; set; }
    public DateTime? NgayTaoThanhToan { get; set; }
    public string? PhuongThuc { get; set; }
    public int? ThanhToanId { get; set; }
    public bool CoDangKy { get; set; }
}

public class AdminThanhToanFilterViewModel
{
    public string? TrangThai { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public List<ThanhToanListItemViewModel> Items { get; set; } = [];
}

public class ThongKeThanhToanViewModel
{
    public decimal TongThuThang { get; set; }
    public int SoLuongDaThanhToan { get; set; }
    public int SoLuongChoPheduyet { get; set; }
    public int SoLuongTuChoi { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }
}
