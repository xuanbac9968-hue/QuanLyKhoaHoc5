namespace QuanLyKhoaHoc5.Web.Models.Entities;

public class GoiYKhoaHoc
{
    public int Id { get; set; }

    public int HocVienId { get; set; }
    public int KhoaHocGoiYId { get; set; }

    public double? DiemPhuHop { get; set; }
    public string? LyDoGoiY { get; set; }
    public string? PromptGuiDi { get; set; }
    public string? PhanHoiAI { get; set; }

    public DateTime NgayGoiY { get; set; } = DateTime.Now;
    public bool DaXem { get; set; } = false;

    // Navigation
    public HocVien HocVien { get; set; } = null!;
    public KhoaHoc KhoaHocGoiY { get; set; } = null!;
}
