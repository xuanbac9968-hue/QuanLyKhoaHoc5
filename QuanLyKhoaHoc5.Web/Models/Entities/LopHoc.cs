using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class LopHoc
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TenLop { get; set; } = "";

    public int KhoaHocId { get; set; }
    public int? GiangVienId { get; set; }

    public DateOnly? NgayKhaiGiang { get; set; }
    public DateOnly? NgayKetThuc { get; set; }

    public int SiSoToiDa { get; set; } = 20;

    [MaxLength(50)]
    public string? PhongHoc { get; set; }

    [MaxLength(50)]
    public string TrangThai { get; set; } = "ChuaMo"; // ChuaMo | DangTuyenSinh | DangHoc | DaKetThuc

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    // Navigation
    public KhoaHoc KhoaHoc { get; set; } = null!;
    public GiangVien? GiangVien { get; set; }
    public ICollection<DangKyKhoaHoc> DangKys { get; set; } = [];
    public ICollection<LichHoc> LichHocs { get; set; } = [];
}
