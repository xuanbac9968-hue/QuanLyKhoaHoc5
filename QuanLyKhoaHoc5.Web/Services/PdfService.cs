using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;

namespace QuanLyKhoaHoc5.Web.Services;

public class PdfService
{
    private readonly AppDbContext _db;

    public PdfService(AppDbContext db) => _db = db;

    public async Task<byte[]> ExportBaoCaoHocVienPdfAsync(DateTime? tuNgay, DateTime? denNgay)
    {
        var query = _db.DangKyKhoaHocs
            .Include(d => d.HocVien)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.Diem)
            .Where(d => d.TrangThai == "DaDuyet");

        if (tuNgay.HasValue)  query = query.Where(d => d.NgayDuyet >= tuNgay);
        if (denNgay.HasValue) query = query.Where(d => d.NgayDuyet <= denNgay.Value.AddDays(1));

        var data = await query.OrderBy(d => d.HocVien.HoTen).ToListAsync();

        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4.Rotate(), 30, 30, 40, 30);
        var writer = PdfWriter.GetInstance(doc, ms);
        doc.Open();

        // Register Vietnamese font (use built-in base font)
        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        var titleFont  = new Font(baseFont, 14, Font.BOLD);
        var headerFont = new Font(baseFont, 9,  Font.BOLD);
        var cellFont   = new Font(baseFont, 8,  Font.NORMAL);
        var subFont    = new Font(baseFont, 10, Font.NORMAL);

        // Title
        var title = new Paragraph("BAO CAO KET QUA HOC TAP", titleFont) { Alignment = Element.ALIGN_CENTER };
        doc.Add(title);

        var centerName = new Paragraph("Trung tam Ngoai ngu NNL", subFont) { Alignment = Element.ALIGN_CENTER };
        doc.Add(centerName);

        var period = tuNgay.HasValue || denNgay.HasValue
            ? $"Tu: {tuNgay?.ToString("dd/MM/yyyy") ?? "..."} - Den: {denNgay?.ToString("dd/MM/yyyy") ?? "..."}"
            : "Tat ca thoi gian";
        doc.Add(new Paragraph(period, subFont) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph($"Ngay xuat: {DateTime.Now:dd/MM/yyyy HH:mm}", cellFont) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph(" "));

        // Table
        var table = new PdfPTable(8) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 4f, 12f, 12f, 8f, 8f, 8f, 8f, 8f });

        void AddHeader(string text)
        {
            var cell = new PdfPCell(new Phrase(text, headerFont))
            {
                BackgroundColor = new BaseColor(13, 110, 253),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            cell.Phrase.Font.Color = new BaseColor(255, 255, 255);
            table.AddCell(cell);
        }

        AddHeader("STT");
        AddHeader("Ma HV");
        AddHeader("Ho ten");
        AddHeader("Khoa hoc");
        AddHeader("Diem GK");
        AddHeader("Diem CK");
        AddHeader("Tong ket");
        AddHeader("Xep loai");

        int stt = 1;
        foreach (var dk in data)
        {
            var bg = stt % 2 == 0 ? new BaseColor(240, 248, 255) : new BaseColor(255, 255, 255);
            void AddCell(string text, int align = Element.ALIGN_LEFT)
            {
                var c = new PdfPCell(new Phrase(text, cellFont)) { BackgroundColor = bg, HorizontalAlignment = align, Padding = 4 };
                table.AddCell(c);
            }

            AddCell(stt.ToString(), Element.ALIGN_CENTER);
            AddCell(dk.HocVien.MaHocVien);
            AddCell(dk.HocVien.HoTen);
            AddCell(dk.LopHoc.KhoaHoc.TenKhoaHoc);
            AddCell(dk.Diem?.DiemGiuaKy?.ToString("F1") ?? "-", Element.ALIGN_CENTER);
            AddCell(dk.Diem?.DiemCuoiKy?.ToString("F1")  ?? "-", Element.ALIGN_CENTER);
            AddCell(dk.Diem?.DiemTongKet?.ToString("F2") ?? "-", Element.ALIGN_CENTER);
            AddCell(dk.Diem?.XepLoai ?? "-", Element.ALIGN_CENTER);
            stt++;
        }

        doc.Add(table);
        doc.Add(new Paragraph(" "));
        doc.Add(new Paragraph($"Tong so: {data.Count} hoc vien", cellFont));

        doc.Close();
        return ms.ToArray();
    }
}
