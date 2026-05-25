using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;

namespace QuanLyKhoaHoc5.Web.Services;

public class ExcelService
{
    private readonly AppDbContext _db;

    public ExcelService(AppDbContext db) => _db = db;

    public async Task<byte[]> ExportHocVienAsync()
    {
        var list = await _db.HocViens.Include(x => x.NguoiDung).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Danh sach hoc vien");

        ws.Cell(1, 1).Value = "Ma HV";
        ws.Cell(1, 2).Value = "Ho ten";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Ngay sinh";
        ws.Cell(1, 5).Value = "Gioi tinh";
        ws.Cell(1, 6).Value = "Dia chi";
        ws.Cell(1, 7).Value = "Trinh do";
        ws.Cell(1, 8).Value = "Ngon ngu";
        ws.Cell(1, 9).Value = "Ngay dang ky";
        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        ws.Row(1).Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < list.Count; i++)
        {
            var hv = list[i];
            ws.Cell(i + 2, 1).Value = hv.MaHocVien;
            ws.Cell(i + 2, 2).Value = hv.HoTen;
            ws.Cell(i + 2, 3).Value = hv.NguoiDung.Email;
            ws.Cell(i + 2, 4).Value = hv.NgaySinh?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(i + 2, 5).Value = hv.GioiTinh ?? "";
            ws.Cell(i + 2, 6).Value = hv.DiaChi ?? "";
            ws.Cell(i + 2, 7).Value = hv.TrinhDoHienTai ?? "";
            ws.Cell(i + 2, 8).Value = hv.NgonNguQuanTam ?? "";
            ws.Cell(i + 2, 9).Value = hv.NgayDangKy.ToString("dd/MM/yyyy");
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportBangDiemAsync(int lopHocId)
    {
        var diems = await _db.Diems
            .Include(x => x.DangKy).ThenInclude(d => d.HocVien)
            .Include(x => x.DangKy).ThenInclude(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Where(x => x.DangKy.LopHocId == lopHocId)
            .ToListAsync();

        var lopHoc = await _db.LopHocs.Include(l => l.KhoaHoc).FirstOrDefaultAsync(l => l.Id == lopHocId);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Bang diem");

        ws.Cell(1, 1).Value = $"BANG DIEM - {lopHoc?.TenLop}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "STT"; ws.Cell(3, 2).Value = "Ma HV"; ws.Cell(3, 3).Value = "Ho ten";
        ws.Cell(3, 4).Value = "Diem GK"; ws.Cell(3, 5).Value = "Diem CK"; ws.Cell(3, 6).Value = "Tong ket";
        ws.Cell(3, 7).Value = "Xep loai"; ws.Cell(3, 8).Value = "Nhan xet";
        ws.Row(3).Style.Font.Bold = true;
        ws.Row(3).Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        ws.Row(3).Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < diems.Count; i++)
        {
            var d = diems[i];
            ws.Cell(i + 4, 1).Value = i + 1;
            ws.Cell(i + 4, 2).Value = d.DangKy.HocVien.MaHocVien;
            ws.Cell(i + 4, 3).Value = d.DangKy.HocVien.HoTen;
            ws.Cell(i + 4, 4).Value = d.DiemGiuaKy?.ToString("F1") ?? "";
            ws.Cell(i + 4, 5).Value = d.DiemCuoiKy?.ToString("F1")  ?? "";
            ws.Cell(i + 4, 6).Value = d.DiemTongKet?.ToString("F2") ?? "";
            ws.Cell(i + 4, 7).Value = d.XepLoai ?? "";
            ws.Cell(i + 4, 8).Value = d.NhanXetGiangVien ?? "";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportBaoCaoHocVienExcelAsync(DateTime? tuNgay, DateTime? denNgay)
    {
        var query = _db.DangKyKhoaHocs
            .Include(d => d.HocVien)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.Diem)
            .Where(d => d.TrangThai == "DaDuyet");

        if (tuNgay.HasValue)  query = query.Where(d => d.NgayDuyet >= tuNgay);
        if (denNgay.HasValue) query = query.Where(d => d.NgayDuyet <= denNgay.Value.AddDays(1));

        var data = await query.OrderBy(d => d.HocVien.HoTen).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Bao cao");

        ws.Cell(1, 1).Value = "BAO CAO KET QUA HOC TAP";
        ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(2, 1).Value = $"Tu: {tuNgay?.ToString("dd/MM/yyyy") ?? "Tat ca"} - Den: {denNgay?.ToString("dd/MM/yyyy") ?? "Tat ca"}";
        ws.Cell(3, 1).Value = $"Ngay xuat: {DateTime.Now:dd/MM/yyyy HH:mm}";

        int startRow = 5;
        ws.Cell(startRow, 1).Value = "STT"; ws.Cell(startRow, 2).Value = "Ma HV"; ws.Cell(startRow, 3).Value = "Ho ten";
        ws.Cell(startRow, 4).Value = "Khoa hoc"; ws.Cell(startRow, 5).Value = "Diem GK";
        ws.Cell(startRow, 6).Value = "Diem CK"; ws.Cell(startRow, 7).Value = "Tong ket"; ws.Cell(startRow, 8).Value = "Xep loai";
        var headerRow = ws.Row(startRow);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        headerRow.Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < data.Count; i++)
        {
            var d = data[i];
            ws.Cell(startRow + 1 + i, 1).Value = i + 1;
            ws.Cell(startRow + 1 + i, 2).Value = d.HocVien.MaHocVien;
            ws.Cell(startRow + 1 + i, 3).Value = d.HocVien.HoTen;
            ws.Cell(startRow + 1 + i, 4).Value = d.LopHoc.KhoaHoc.TenKhoaHoc;
            ws.Cell(startRow + 1 + i, 5).Value = d.Diem?.DiemGiuaKy?.ToString("F1") ?? "";
            ws.Cell(startRow + 1 + i, 6).Value = d.Diem?.DiemCuoiKy?.ToString("F1")  ?? "";
            ws.Cell(startRow + 1 + i, 7).Value = d.Diem?.DiemTongKet?.ToString("F2") ?? "";
            ws.Cell(startRow + 1 + i, 8).Value = d.Diem?.XepLoai ?? "";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
