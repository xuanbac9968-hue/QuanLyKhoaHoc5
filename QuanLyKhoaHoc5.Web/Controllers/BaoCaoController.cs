using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;

namespace QuanLyKhoaHoc5.Web.Controllers;

[AuthorizeRole("Admin")]
public class BaoCaoController : Controller
{
    private readonly AppDbContext _db;

    public BaoCaoController(AppDbContext db) { _db = db; }

    // GET /BaoCao/Index
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var dauNam = new DateTime(now.Year, 1, 1);

        ViewBag.TongHocVien    = await _db.HocViens.CountAsync();
        ViewBag.TongGiangVien  = await _db.GiangViens.CountAsync();
        ViewBag.TongKhoaHoc    = await _db.KhoaHocs.CountAsync();
        ViewBag.TongLopKetThuc = await _db.LopHocs.CountAsync(l => l.TrangThai == "DaKetThuc");

        // Biểu đồ: số HV đăng ký theo từng tháng trong năm hiện tại
        var dangKyNam = await _db.DangKyKhoaHocs
            .Where(d => d.NgayDangKy.Year == now.Year)
            .GroupBy(d => d.NgayDangKy.Month)
            .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        var chartLabels = Enumerable.Range(1, 12).Select(m => $"T{m}").ToList();
        var chartData   = Enumerable.Range(1, 12)
            .Select(m => dangKyNam.FirstOrDefault(x => x.Thang == m)?.SoLuong ?? 0)
            .ToList();

        ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(chartLabels);
        ViewBag.ChartData   = System.Text.Json.JsonSerializer.Serialize(chartData);

        return View();
    }

    // GET /BaoCao/ExportTongHop
    public async Task<IActionResult> ExportTongHop()
    {
        using var wb = new XLWorkbook();

        // ─── Sheet 1: Học Viên ────────────────────────────────────
        var wsHV = wb.Worksheets.Add("Hoc Vien");
        var headers1 = new[] { "STT", "Mã HV", "Họ tên", "Email", "Trình độ", "Ngày đăng ký" };
        for (int i = 0; i < headers1.Length; i++)
        {
            wsHV.Cell(1, i + 1).Value = headers1[i];
            wsHV.Cell(1, i + 1).Style.Font.Bold = true;
            wsHV.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            wsHV.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        var hocViens = await _db.HocViens.Include(h => h.NguoiDung).OrderBy(h => h.MaHocVien).ToListAsync();
        for (int i = 0; i < hocViens.Count; i++)
        {
            var hv = hocViens[i];
            wsHV.Cell(i + 2, 1).Value = i + 1;
            wsHV.Cell(i + 2, 2).Value = hv.MaHocVien;
            wsHV.Cell(i + 2, 3).Value = hv.HoTen;
            wsHV.Cell(i + 2, 4).Value = hv.NguoiDung.Email;
            wsHV.Cell(i + 2, 5).Value = hv.TrinhDoHienTai ?? "";
            wsHV.Cell(i + 2, 6).Value = hv.NgayDangKy.ToString("dd/MM/yyyy");
        }
        wsHV.Columns().AdjustToContents();

        // ─── Sheet 2: Khóa Học ────────────────────────────────────
        var wsKH = wb.Worksheets.Add("Khoa Hoc");
        var headers2 = new[] { "STT", "Tên khóa học", "Ngôn ngữ", "Trình độ", "Học phí", "Số lớp", "Số HV đăng ký" };
        for (int i = 0; i < headers2.Length; i++)
        {
            wsKH.Cell(1, i + 1).Value = headers2[i];
            wsKH.Cell(1, i + 1).Style.Font.Bold = true;
            wsKH.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            wsKH.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        var khoaHocs = await _db.KhoaHocs
            .Include(k => k.LopHocs).ThenInclude(l => l.DangKys)
            .OrderBy(k => k.TenKhoaHoc).ToListAsync();
        for (int i = 0; i < khoaHocs.Count; i++)
        {
            var kh = khoaHocs[i];
            var soHV = kh.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet");
            wsKH.Cell(i + 2, 1).Value = i + 1;
            wsKH.Cell(i + 2, 2).Value = kh.TenKhoaHoc;
            wsKH.Cell(i + 2, 3).Value = kh.NgonNgu;
            wsKH.Cell(i + 2, 4).Value = kh.TrinhDo;
            wsKH.Cell(i + 2, 5).Value = (double)kh.HocPhi;
            wsKH.Cell(i + 2, 5).Style.NumberFormat.Format = "#,##0";
            wsKH.Cell(i + 2, 6).Value = kh.LopHocs.Count;
            wsKH.Cell(i + 2, 7).Value = soHV;
        }
        wsKH.Columns().AdjustToContents();

        // ─── Sheet 3: Bảng Điểm ──────────────────────────────────
        var wsBD = wb.Worksheets.Add("Bang Diem");
        var headers3 = new[] { "STT", "Học viên", "Lớp học", "Điểm GK", "Điểm CK", "Tổng kết", "Xếp loại" };
        for (int i = 0; i < headers3.Length; i++)
        {
            wsBD.Cell(1, i + 1).Value = headers3[i];
            wsBD.Cell(1, i + 1).Style.Font.Bold = true;
            wsBD.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            wsBD.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }
        var diems = await _db.Diems
            .Include(d => d.DangKy).ThenInclude(dk => dk.HocVien)
            .Include(d => d.DangKy).ThenInclude(dk => dk.LopHoc)
            .Where(d => d.DangKy.TrangThai == "DaDuyet")
            .OrderBy(d => d.DangKy.LopHoc.TenLop).ThenBy(d => d.DangKy.HocVien.HoTen)
            .ToListAsync();
        for (int i = 0; i < diems.Count; i++)
        {
            var d = diems[i];
            wsBD.Cell(i + 2, 1).Value = i + 1;
            wsBD.Cell(i + 2, 2).Value = d.DangKy.HocVien.HoTen;
            wsBD.Cell(i + 2, 3).Value = d.DangKy.LopHoc.TenLop;
            if (d.DiemGiuaKy.HasValue) wsBD.Cell(i + 2, 4).Value = d.DiemGiuaKy.Value;
            if (d.DiemCuoiKy.HasValue) wsBD.Cell(i + 2, 5).Value = d.DiemCuoiKy.Value;
            if (d.DiemTongKet.HasValue) wsBD.Cell(i + 2, 6).Value = d.DiemTongKet.Value;
            wsBD.Cell(i + 2, 7).Value = d.XepLoai ?? "";
        }
        wsBD.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var fileName = $"BaoCaoTongHop_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
