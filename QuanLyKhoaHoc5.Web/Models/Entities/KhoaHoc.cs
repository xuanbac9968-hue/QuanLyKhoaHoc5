using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class KhoaHoc
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string TenKhoaHoc { get; set; } = "";

    public string? MoTa { get; set; }

    [Required, MaxLength(50)]
    public string NgonNgu { get; set; } = "Tiếng Anh";

    [Required, MaxLength(50)]
    public string TrinhDo { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal HocPhi { get; set; } = 0;

    public int SoChoToiDa { get; set; } = 20;

    public int ThoiLuong { get; set; }

    public int? SoBuoiMoiTuan { get; set; }
    public int? ThoiGianMoiBuoi { get; set; }

    [MaxLength(500)]
    public string? AnhBia { get; set; }

    public string? NoiDungChuongTrinh { get; set; }

    [MaxLength(50)]
    public string TrangThai { get; set; } = "DangMo"; // DangMo | DaDong | TamDung

    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }

    // Navigation
    public ICollection<LopHoc> LopHocs { get; set; } = [];
    public ICollection<GoiYKhoaHoc> GoiYs { get; set; } = [];
    public ICollection<PhanCongGiangDay> PhanCongs { get; set; } = [];
}
