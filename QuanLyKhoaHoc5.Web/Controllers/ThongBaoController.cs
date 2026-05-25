using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class ThongBaoController : Controller
{
    private readonly AppDbContext _db;

    public ThongBaoController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.ThongBaos
            .Where(t => t.NguoiNhanId == userId)
            .OrderByDescending(t => t.NgayTao)
            .ToListAsync();

        // Đánh dấu tất cả là đã đọc khi mở trang
        var chuaDoc = list.Where(t => !t.DaDoc).ToList();
        chuaDoc.ForEach(t => t.DaDoc = true);
        if (chuaDoc.Any()) await _db.SaveChangesAsync();

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DanhDauDaDoc(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tb = await _db.ThongBaos.FirstOrDefaultAsync(t => t.Id == id && t.NguoiNhanId == userId);
        if (tb != null) { tb.DaDoc = true; await _db.SaveChangesAsync(); }
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DanhDauTatCa()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var chuaDoc = await _db.ThongBaos.Where(t => t.NguoiNhanId == userId && !t.DaDoc).ToListAsync();
        chuaDoc.ForEach(t => t.DaDoc = true);
        if (chuaDoc.Any()) await _db.SaveChangesAsync();
        return Json(new { success = true, message = $"Đã đánh dấu {chuaDoc.Count} thông báo là đã đọc" });
    }

    public async Task<IActionResult> SoChuaDoc()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _db.ThongBaos.CountAsync(t => t.NguoiNhanId == userId && !t.DaDoc);
        return Json(count);
    }
}
