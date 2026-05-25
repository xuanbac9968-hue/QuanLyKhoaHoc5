using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class NguoiDung
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = "";

    [Required, MaxLength(512)]
    public string MatKhauHash { get; set; } = "";

    [Required, MaxLength(20)]
    public string VaiTro { get; set; } = "HocVien"; // Admin | GiangVien | HocVien

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = "";

    [MaxLength(15)]
    public string? SoDienThoai { get; set; }

    [MaxLength(500)]
    public string? AnhDaiDien { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }

    // Navigation
    public HocVien? HocVien { get; set; }
    public GiangVien? GiangVien { get; set; }
    public ICollection<ThongBao> ThongBaos { get; set; } = [];
}
