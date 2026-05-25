using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class HocVien
{
    public int Id { get; set; } // FK -> NguoiDung.Id

    [Required, MaxLength(20)]
    public string MaHocVien { get; set; } = "";

    [Required, MaxLength(100)]
    public string HoTen { get; set; } = "";

    public DateOnly? NgaySinh { get; set; }

    [MaxLength(10)]
    public string? GioiTinh { get; set; }

    [MaxLength(300)]
    public string? DiaChi { get; set; }

    [MaxLength(50)]
    public string? TrinhDoHienTai { get; set; }

    [MaxLength(100)]
    public string? NgonNguQuanTam { get; set; }

    public DateTime NgayDangKy { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    // Navigation
    public NguoiDung NguoiDung { get; set; } = null!;
    public ICollection<DangKyKhoaHoc> DangKys { get; set; } = [];
    public ICollection<GoiYKhoaHoc> GoiYs { get; set; } = [];
}
