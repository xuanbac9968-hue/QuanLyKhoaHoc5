using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class DangKyKhoaHoc
{
    public int Id { get; set; }

    public int HocVienId { get; set; }
    public int LopHocId { get; set; }

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet | DaDuyet | TuChoi | DaHuy

    [MaxLength(500)]
    public string? LyDoTuChoi { get; set; }

    public int? NguoiDuyetId { get; set; }
    public DateTime? NgayDuyet { get; set; }

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    // Navigation
    public HocVien HocVien { get; set; } = null!;
    public LopHoc LopHoc { get; set; } = null!;
    public NguoiDung? NguoiDuyet { get; set; }
    public Diem? Diem { get; set; }
}
