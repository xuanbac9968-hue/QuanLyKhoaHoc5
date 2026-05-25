using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Filters;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class GoiYController : Controller
{
    private readonly AppDbContext _db;
    private readonly GoiYKhoaHocService _goiYService;

    public GoiYController(AppDbContext db, GoiYKhoaHocService goiYService)
    {
        _db = db; _goiYService = goiYService;
    }

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Lấy gợi ý gần nhất (nếu có)
        var goiYGanNhat = await _db.GoiYKhoaHocs
            .Include(g => g.KhoaHocGoiY)
            .Where(g => g.HocVienId == userId)
            .OrderByDescending(g => g.NgayGoiY)
            .Take(3)
            .ToListAsync();

        var vm = new GoiYTrangViewModel
        {
            DaGoiY = goiYGanNhat.Any(),
            KetQua = goiYGanNhat.Select(g => new GoiYKetQuaViewModel
            {
                KhoaHocId = g.KhoaHocGoiYId,
                TenKhoaHoc = g.KhoaHocGoiY.TenKhoaHoc,
                DiemPhuHop = g.DiemPhuHop ?? 0,
                LyDo = g.LyDoGoiY ?? "",
                HocPhi = g.KhoaHocGoiY.HocPhi,
                AnhBia = g.KhoaHocGoiY.AnhBia
            }).ToList()
        };
        return View(vm);
    }

    [AuthorizeRole("HocVien")]
    [HttpPost]
    public async Task<IActionResult> TaoGoiY([FromServices] ILogger<GoiYController> logger)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            var ketQua = await _goiYService.TaoGoiYAsync(userId);
            if (!ketQua.Any())
                return Json(new { success = false, message = "Hiện chưa có gợi ý phù hợp. Hãy tiếp tục học và thử lại sau." });

            return Json(new { success = true, goiY = ketQua });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("GoiY AI lỗi: {Msg}", ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi không xác định khi tạo gợi ý AI");
            return Json(new { success = false, message = "Đã xảy ra lỗi khi kết nối AI, vui lòng thử lại sau." });
        }
    }

    [AuthorizeRole("HocVien")]
    public async Task<IActionResult> LichSu()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.GoiYKhoaHocs
            .Include(g => g.KhoaHocGoiY)
            .Where(g => g.HocVienId == userId)
            .OrderByDescending(g => g.NgayGoiY)
            .ToListAsync();
        return View(list);
    }

    [AuthorizeRole("Admin")]
    public async Task<IActionResult> Admin(int page = 1)
    {
        int pageSize = 20;
        var query = _db.GoiYKhoaHocs
            .Include(g => g.HocVien)
            .Include(g => g.KhoaHocGoiY)
            .OrderByDescending(g => g.NgayGoiY);

        ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);
        ViewBag.Page = page;
        return View(await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
    }
}
