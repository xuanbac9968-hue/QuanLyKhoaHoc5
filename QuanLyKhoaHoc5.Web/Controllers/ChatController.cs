using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using System.Security.Claims;

namespace QuanLyKhoaHoc5.Web.Controllers;

[Authorize(AuthenticationSchemes = "CookieAuth")]
public class ChatController : Controller
{
    private readonly AppDbContext _db;
    private readonly ChatService _chat;
    private const int MaxDisplayMessages = 50;

    public ChatController(AppDbContext db, ChatService chat) { _db = db; _chat = chat; }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return Json(new { success = false, message = "Nội dung không được để trống." });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role   = User.FindFirstValue(ClaimTypes.Role) ?? "HocVien";

        // Load last 16 messages for context (lightweight)
        var recentHistory = await _db.ChatHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(16)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new ChatMessageViewModel { Role = h.Role, Content = h.Content })
            .ToListAsync();

        string reply;
        try
        {
            reply = await _chat.SendMessageAsync(userId, request.Message, recentHistory, role, _db);
        }
        catch (Exception)
        {
            reply = "Xin lỗi, đã xảy ra lỗi khi kết nối AI. Vui lòng thử lại.";
        }

        // Save both user message and assistant reply to DB
        _db.ChatHistories.AddRange(
            new ChatHistory { UserId = userId, Role = "user",      Content = request.Message, CreatedAt = DateTime.Now },
            new ChatHistory { UserId = userId, Role = "assistant", Content = reply,            CreatedAt = DateTime.Now.AddMilliseconds(1) }
        );
        await _db.SaveChangesAsync();

        return Json(new { success = true, reply });
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var history = await _db.ChatHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(MaxDisplayMessages)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new ChatMessageViewModel { Role = h.Role, Content = h.Content })
            .ToListAsync();

        return Json(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var records = await _db.ChatHistories
            .Where(h => h.UserId == userId)
            .ToListAsync();

        _db.ChatHistories.RemoveRange(records);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }
}
