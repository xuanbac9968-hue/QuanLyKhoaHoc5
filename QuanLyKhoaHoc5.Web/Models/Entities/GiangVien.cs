using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class GiangVien
{
    public int Id { get; set; } // FK -> NguoiDung.Id

    [Required, MaxLength(20)]
    public string MaGiangVien { get; set; } = "";

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = "";

    [MaxLength(100)]
    public string? ChuyenMon { get; set; }

    [MaxLength(100)]
    public string? BangCap { get; set; }

    public int? KinhNghiem { get; set; }

    [MaxLength(500)]
    public string? MoTa { get; set; }

    // Navigation
    public NguoiDung NguoiDung { get; set; } = null!;
    public ICollection<LopHoc> LopHocs { get; set; } = [];
    public ICollection<PhanCongGiangDay> PhanCongs { get; set; } = [];
}
