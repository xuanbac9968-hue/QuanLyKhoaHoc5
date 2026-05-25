using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class ThanhToan
{
    public int Id { get; set; }

    public int HocVienId { get; set; }

    public int KhoaHocId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SoTien { get; set; }

    /// <summary>ChuyenKhoan | TienMat</summary>
    [Required, MaxLength(30)]
    public string PhuongThuc { get; set; } = "TienMat";

    /// <summary>ChoPheduyet | DaThanhToan | TuChoi</summary>
    [Required, MaxLength(20)]
    public string TrangThai { get; set; } = "ChoPheduyet";

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public DateTime? NgayDuyet { get; set; }

    public int? NguoiDuyetId { get; set; }

    // Navigation
    public HocVien HocVien { get; set; } = null!;
    public KhoaHoc KhoaHoc { get; set; } = null!;
    public NguoiDung? NguoiDuyet { get; set; }
}
