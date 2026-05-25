using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class KyHoc
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string TenKy { get; set; } = ""; // VD: "Kỳ 1 – 2026"

    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }

    /// <summary>DangMo | DaDong</summary>
    [MaxLength(20)]
    public string TrangThai { get; set; } = "DangMo";

    public DateTime NgayTao { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Diem> Diems { get; set; } = [];
}
