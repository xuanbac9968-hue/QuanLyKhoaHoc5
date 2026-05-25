using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using Newtonsoft.Json;

namespace QuanLyKhoaHoc5.Web.Services;

public class GoiYKhoaHocService
{
    private readonly AppDbContext _db;
    private readonly GeminiService _gemini;
    private readonly ILogger<GoiYKhoaHocService> _logger;

    public GoiYKhoaHocService(AppDbContext db, GeminiService gemini, ILogger<GoiYKhoaHocService> logger)
    {
        _db = db;
        _gemini = gemini;
        _logger = logger;
    }

    public async Task<List<GoiYKetQuaViewModel>> TaoGoiYAsync(int hocVienId)
    {
        var hocVien = await _db.HocViens
            .Include(x => x.NguoiDung)
            .Include(x => x.DangKys)
                .ThenInclude(d => d.LopHoc)
                    .ThenInclude(l => l.KhoaHoc)
            .Include(x => x.DangKys)
                .ThenInclude(d => d.Diem)
            .FirstOrDefaultAsync(x => x.Id == hocVienId);

        if (hocVien == null) return [];

        // Build profile
        var lichSu = hocVien.DangKys
            .Where(d => d.TrangThai == "DaDuyet")
            .Select(d => new LichSuHocTapViewModel
            {
                TenKhoaHoc = d.LopHoc.KhoaHoc.TenKhoaHoc,
                TrinhDo = d.LopHoc.KhoaHoc.TrinhDo,
                DiemTongKet = d.Diem?.DiemTongKet,
                XepLoai = d.Diem?.XepLoai,
                TrangThai = d.LopHoc.TrangThai == "DaKetThuc" ? "Đã hoàn thành" : "Đang học"
            }).ToList();

        var profile = new HocVienProfileViewModel
        {
            HoTen = hocVien.HoTen,
            TrinhDoHienTai = hocVien.TrinhDoHienTai ?? "Chưa xác định",
            NgonNguQuanTam = hocVien.NgonNguQuanTam ?? "Tiếng Anh",
            LichSuHocTap = lichSu
        };

        // Lấy danh sách khóa học đang mở
        var idDaHoc = hocVien.DangKys
            .Where(d => d.TrangThai is "DaDuyet" or "ChoDuyet")
            .Select(d => d.LopHoc.KhoaHocId)
            .Distinct().ToList();

        var khoaHocs = await _db.KhoaHocs
            .Where(k => k.TrangThai == "DangMo" && !idDaHoc.Contains(k.Id))
            .Take(15)
            .ToListAsync();

        if (!khoaHocs.Any()) return [];

        var danhSachInput = khoaHocs.Select(k => new KhoaHocGoiYInputViewModel
        {
            Id = k.Id,
            TenKhoaHoc = k.TenKhoaHoc,
            NgonNgu = k.NgonNgu,
            TrinhDo = k.TrinhDo,
            HocPhi = k.HocPhi,
            MoTa = k.MoTa ?? ""
        }).ToList();

        var promptText = JsonConvert.SerializeObject(new { profile, danhSachInput });
        var goiYItems = await _gemini.GetGoiYKhoaHocAsync(profile, danhSachInput);

        // Lưu vào DB
        var ketQua = new List<GoiYKetQuaViewModel>();
        foreach (var item in goiYItems)
        {
            // Tìm theo ID trước, fallback theo tên
            var kh = khoaHocs.FirstOrDefault(k => k.Id == item.KhoaHocId)
                  ?? khoaHocs.FirstOrDefault(k => k.TenKhoaHoc == item.TenKhoaHoc);
            if (kh == null)
            {
                _logger.LogWarning("Không tìm thấy khóa học với Id={Id}, Tên={Ten}", item.KhoaHocId, item.TenKhoaHoc);
                continue;
            }
            item.KhoaHocId = kh.Id;

            var goiY = new GoiYKhoaHoc
            {
                HocVienId = hocVienId,
                KhoaHocGoiYId = item.KhoaHocId,
                DiemPhuHop = item.DiemPhuHop,
                LyDoGoiY = item.LyDo,
                PromptGuiDi = promptText,
                NgayGoiY = DateTime.Now
            };
            _db.GoiYKhoaHocs.Add(goiY);

            ketQua.Add(new GoiYKetQuaViewModel
            {
                KhoaHocId = kh.Id,
                TenKhoaHoc = kh.TenKhoaHoc,
                DiemPhuHop = item.DiemPhuHop,
                LyDo = item.LyDo,
                HocPhi = kh.HocPhi,
                AnhBia = kh.AnhBia
            });
        }

        await _db.SaveChangesAsync();
        return ketQua;
    }
}
