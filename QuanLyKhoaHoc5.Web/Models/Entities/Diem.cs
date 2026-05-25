using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class Diem
{
    public int Id { get; set; }

    public int DangKyId { get; set; }

    /// <summary>Kỳ học (nullable cho dữ liệu cũ)</summary>
    public int? KyHocId { get; set; }

    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemTongKet { get; set; }

    [MaxLength(20)]
    public string? XepLoai { get; set; } // Xuất sắc | Giỏi | Khá | Trung bình | Yếu

    [MaxLength(500)]
    public string? NhanXetGiangVien { get; set; }

    public bool IsKhoa { get; set; } = false;
    public DateTime? NgayCapNhat { get; set; }

    // Navigation
    public DangKyKhoaHoc DangKy { get; set; } = null!;
    public KyHoc? KyHoc { get; set; }

    // ─── Static helpers ────────────────────────────────────────
    /// <summary>Công thức: GK * 30% + CK * 70%</summary>
    public static double? TinhTongKet(double? giuaKy, double? cuoiKy)
    {
        if (!giuaKy.HasValue || !cuoiKy.HasValue) return null;
        return Math.Round(giuaKy.Value * 0.3 + cuoiKy.Value * 0.7, 2);
    }

    public static string? TinhXepLoai(double? diem) => diem switch
    {
        >= 8.5 => "Xuất sắc",
        >= 7.0 => "Giỏi",
        >= 5.5 => "Khá",
        >= 4.0 => "Trung bình",
        < 4.0  => "Yếu",
        _      => null
    };
}
