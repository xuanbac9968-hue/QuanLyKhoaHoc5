using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;

namespace QuanLyKhoaHoc5.Web.Helpers;

public static class LichHocHelper
{
    /// <summary>
    /// Generate individual session dates for a date range and list of weekdays.
    /// dayOfWeeks: DayOfWeek.Monday, DayOfWeek.Wednesday, etc.
    /// </summary>
    public static IEnumerable<DateOnly> GenerateDates(DateOnly start, DateOnly end, IEnumerable<DayOfWeek> dayOfWeeks)
    {
        var daysSet = new HashSet<DayOfWeek>(dayOfWeeks);
        var cur = start;
        while (cur <= end)
        {
            if (daysSet.Contains(cur.DayOfWeek))
                yield return cur;
            cur = cur.AddDays(1);
        }
    }

    /// <summary>
    /// Convert LichHoc entity to ViewModel.
    /// </summary>
    public static LichHocChiTietViewModel ToViewModel(LichHoc l) => new()
    {
        Id = l.Id,
        LopHocId = l.LopHocId,
        KhoaHocId = l.LopHoc?.KhoaHocId ?? 0,
        TenKhoaHoc = l.LopHoc?.KhoaHoc?.TenKhoaHoc ?? "",
        TenLop = l.LopHoc?.TenLop ?? "",
        TenGiangVien = l.LopHoc?.GiangVien?.HoTen,
        NgayHoc = l.NgayHoc,
        GioBatDau = l.GioBatDau,
        GioKetThuc = l.GioKetThuc,
        PhongHoc = l.PhongHoc ?? l.LopHoc?.PhongHoc,
        ChuDe = l.ChuDe,
        GhiChu = l.GhiChu
    };
}
