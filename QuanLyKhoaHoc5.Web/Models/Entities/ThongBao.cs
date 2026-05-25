using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class ThongBao
{
    public int Id { get; set; }

    public int NguoiNhanId { get; set; }

    [Required, MaxLength(200)]
    public string TieuDe { get; set; } = "";

    public string? NoiDung { get; set; }

    [MaxLength(50)]
    public string? LoaiThongBao { get; set; } // DangKy | LichHoc | Diem | HeThong

    public bool DaDoc { get; set; } = false;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? DuongDanLienKet { get; set; }

    // Navigation
    public NguoiDung NguoiNhan { get; set; } = null!;
}
