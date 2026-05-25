using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

// ─── Dùng cho Admin Index ──────────────────────────────────────────────────
public class DiemSoFilterViewModel
{
    public int? KyHocId { get; set; }
    public int? KhoaHocId { get; set; }
    public int? LopHocId { get; set; }
    public string? TimKiem { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public List<DiemSoRowViewModel> Items { get; set; } = [];
    public DiemSoThongKeViewModel ThongKe { get; set; } = new();
}

public class DiemSoRowViewModel
{
    public int DiemId { get; set; }
    public int DangKyId { get; set; }
    public string MaHocVien { get; set; } = "";
    public string TenHocVien { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string? TenGiangVien { get; set; }
    public string? TenKyHoc { get; set; }
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
    public bool IsKhoa { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}

public class DiemSoThongKeViewModel
{
    public double DiemTrungBinh { get; set; }
    public int SoHocVienDat { get; set; }    // >= 4.0
    public int SoHocVienChuaDat { get; set; }
    public int TongHocVien { get; set; }
    public double TiLeDat => TongHocVien > 0 ? Math.Round((double)SoHocVienDat / TongHocVien * 100, 1) : 0;
    // Histogram data: 0-1, 1-2, ..., 9-10
    public int[] Histogram { get; set; } = new int[10];
}

// ─── Dùng cho GiangVien NhapDiem ──────────────────────────────────────────
public class NhapDiemBatchViewModel
{
    public int LopHocId { get; set; }
    public string TenLop { get; set; } = "";
    public string TenKhoaHoc { get; set; } = "";
    public bool IsKhoa { get; set; }
    public string? TenKyHoc { get; set; }
    public bool KyDaDong { get; set; }
    public List<NhapDiemHangViewModel> HocViens { get; set; } = [];
}

public class NhapDiemHangViewModel
{
    public int DangKyId { get; set; }
    public int DiemId { get; set; }
    public string MaHocVien { get; set; } = "";
    public string TenHocVien { get; set; } = "";
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
    public string? NhanXet { get; set; }
    public bool IsKhoa { get; set; }
}

// ─── Dùng cho HocVien CuaToi ──────────────────────────────────────────────
public class DiemSoHocVienViewModel
{
    public List<DiemSoKyHocViewModel> CacKy { get; set; } = [];
    // Radar chart: tên môn + điểm tổng kết gần nhất
    public List<string> RadarLabels { get; set; } = [];
    public List<double> RadarData { get; set; } = [];
}

public class DiemSoKyHocViewModel
{
    public int? KyHocId { get; set; }
    public string TenKy { get; set; } = "";
    public List<DiemSoMonViewModel> Mons { get; set; } = [];
    public double? DiemTrungBinhKy =>
        Mons.Any(m => m.DiemTongKet.HasValue)
            ? Math.Round(Mons.Where(m => m.DiemTongKet.HasValue).Average(m => m.DiemTongKet!.Value), 2)
            : null;
}

public class DiemSoMonViewModel
{
    public string TenKhoaHoc { get; set; } = "";
    public string TenLop { get; set; } = "";
    public string NgonNgu { get; set; } = "";
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemTongKet { get; set; }
    public string? XepLoai { get; set; }
    public string? NhanXet { get; set; }
}

// ─── AJAX Batch Save ──────────────────────────────────────────────────────
public class BatchSaveDiemRequest
{
    public List<SingleDiemRequest> Items { get; set; } = [];
}

public class SingleDiemRequest
{
    public int DangKyId { get; set; }
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public string? NhanXet { get; set; }
}
