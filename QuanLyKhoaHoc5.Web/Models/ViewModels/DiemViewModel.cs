using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class NhapDiemViewModel
{
    public int DangKyId { get; set; }
    public int HocVienId { get; set; }
    public string MaHocVien { get; set; } = "";
    public string TenHocVien { get; set; } = "";
    public int? DiemId { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm giữa kỳ")]
    public double? DiemGiuaKy { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm cuối kỳ")]
    public double? DiemCuoiKy { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm tổng kết")]
    public double? DiemTongKet { get; set; }

    [Display(Name = "Xếp loại")]
    public string? XepLoai { get; set; }

    [Display(Name = "Nhận xét giảng viên")]
    public string? NhanXetGiangVien { get; set; }

    public bool IsKhoa { get; set; }
}

public class BangDiemLopViewModel
{
    public int LopHocId { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public bool IsKhoa { get; set; }
    public bool CanEdit { get; set; }
    public List<NhapDiemViewModel> DanhSachDiem { get; set; } = [];
}

public class DiemCuaToiViewModel
{
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
    public string? NhanXetGiangVien { get; set; }
    public string TrangThaiDangKy { get; set; } = "";
}
