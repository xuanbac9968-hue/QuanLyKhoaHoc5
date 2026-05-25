using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyKhoaHoc5.Web.Models;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account");

        return User.FindFirstValue(ClaimTypes.Role) switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "GiangVien" => RedirectToAction("Dashboard", "GiangVien"),
            "HocVien" => RedirectToAction("Dashboard", "HocVien"),
            _ => RedirectToAction("Login", "Account")
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
