using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.Entities;

namespace QuanLyKhoaHoc5.Web.Services;

public class ThongBaoService
{
    private readonly AppDbContext _db;

    public ThongBaoService(AppDbContext db) => _db = db;

    public async Task TaoThongBaoAsync(int nguoiNhanId, string tieuDe, string? noiDung = null,
        string? loai = null, string? duongDan = null)
    {
        var tb = new ThongBao
        {
            NguoiNhanId = nguoiNhanId,
            TieuDe = tieuDe,
            NoiDung = noiDung,
            LoaiThongBao = loai,
            DuongDanLienKet = duongDan
        };
        _db.ThongBaos.Add(tb);
        await _db.SaveChangesAsync();
    }
}
