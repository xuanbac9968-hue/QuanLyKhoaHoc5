using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

/// <summary>
/// Buổi học cụ thể (per-session), gắn với LopHoc.
/// </summary>
public class LichHoc
{
    public int Id { get; set; }

    public int LopHocId { get; set; }

    public DateOnly NgayHoc { get; set; }

    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }

    [MaxLength(50)]
    public string? PhongHoc { get; set; }

    [MaxLength(200)]
    public string? ChuDe { get; set; }

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    // Navigation
    public LopHoc LopHoc { get; set; } = null!;
}
