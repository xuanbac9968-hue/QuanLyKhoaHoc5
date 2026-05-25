using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class PhanCongGiangDay
{
    public int Id { get; set; }

    public int GiangVienId { get; set; }
    public int KhoaHocId { get; set; }

    public DateTime NgayPhanCong { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? GhiChu { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public GiangVien GiangVien { get; set; } = null!;
    public KhoaHoc KhoaHoc { get; set; } = null!;
}
