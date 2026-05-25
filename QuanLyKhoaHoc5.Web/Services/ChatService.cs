using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.ViewModels;

namespace QuanLyKhoaHoc5.Web.Services;

public class ChatService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ChatService> _logger;

    public ChatService(HttpClient http, IConfiguration config, ILogger<ChatService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(
        int userId, string userMessage,
        List<ChatMessageViewModel> history, string role,
        AppDbContext db)
    {
        var apiKey   = _config["GrokApi:ApiKey"]   ?? "";
        var endpoint = _config["GrokApi:Endpoint"] ?? "https://api.x.ai/v1/chat/completions";
        var model    = _config["GrokApi:Model"]    ?? "grok-3-latest";

        var systemPrompt = await BuildSystemPromptAsync(userId, role, db);

        // Build messages: system + conversation history + current user message
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Include up to last 16 history messages for context
        var historySlice = history.Count > 16 ? history[^16..] : history;
        foreach (var h in historySlice)
            messages.Add(new { role = h.Role == "assistant" ? "assistant" : "user", content = h.Content });

        messages.Add(new { role = "user", content = userMessage });

        var requestBody = new
        {
            model,
            messages = messages.ToArray(),
            temperature = 0.7,
            max_tokens = 900
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            var json = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Grok API lỗi {Status}: {Body}", response.StatusCode, err[..Math.Min(300, err.Length)]);
                if (apiKey.StartsWith("xai-placeholder"))
                    return await FallbackGroqAsync(systemPrompt, messages);
                return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
            }

            return ExtractContent(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChatService lỗi khi gọi Grok API");
            return await FallbackGroqAsync(systemPrompt, messages);
        }
    }

    private async Task<string> FallbackGroqAsync(string systemPrompt, List<object> messages)
    {
        var apiKey   = _config["GroqAPI:ApiKey"]   ?? "";
        var endpoint = _config["GroqAPI:Endpoint"] ?? "https://api.groq.com/openai/v1/chat/completions";
        var model    = _config["GroqAPI:Model"]    ?? "llama-3.3-70b-versatile";

        if (string.IsNullOrWhiteSpace(apiKey))
            return "Xin lỗi, dịch vụ AI chưa được cấu hình. Vui lòng liên hệ admin.";

        var requestBody = new
        {
            model,
            messages = messages.ToArray(),
            temperature = 0.7,
            max_tokens = 900
        };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            req.Content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return "Xin lỗi, không thể kết nối đến AI. Vui lòng thử lại.";
            return ExtractContent(await resp.Content.ReadAsStringAsync());
        }
        catch
        {
            return "Xin lỗi, dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau.";
        }
    }

    private async Task<string> BuildSystemPromptAsync(int userId, string role, AppDbContext db)
    {
        return role switch
        {
            "Admin"     => await BuildAdminPromptAsync(db),
            "GiangVien" => await BuildGiangVienPromptAsync(userId, db),
            _           => await BuildHocVienPromptAsync(userId, db)
        };
    }

    // ─── Admin ────────────────────────────────────────────────────────────────
    private static async Task<string> BuildAdminPromptAsync(AppDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth = new DateTime(today.Year, today.Month, 1);

        var tongKhoaHocMo    = await db.KhoaHocs.CountAsync(k => k.TrangThai == "DangMo");
        var tongHocVienDangHoc = await db.DangKyKhoaHocs.CountAsync(d => d.TrangThai == "DaDuyet");
        var tongGiangVien    = await db.GiangViens.CountAsync();
        var doanhThuThang    = await db.DangKyKhoaHocs
            .Where(d => d.TrangThai == "DaDuyet" && d.NgayDuyet >= firstOfMonth)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .SumAsync(d => (decimal?)d.LopHoc.KhoaHoc.HocPhi) ?? 0;

        var topKhoa = await db.KhoaHocs
            .Include(k => k.LopHocs).ThenInclude(l => l.DangKys)
            .OrderByDescending(k => k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet"))
            .Take(5)
            .Select(k => new {
                k.TenKhoaHoc, k.NgonNgu, k.TrinhDo,
                SoHV = k.LopHocs.SelectMany(l => l.DangKys).Count(d => d.TrangThai == "DaDuyet")
            })
            .ToListAsync();

        var topLines = topKhoa.Select(k => $"- {k.TenKhoaHoc} ({k.NgonNgu} - {k.TrinhDo}): {k.SoHV} học viên");

        var dangKyChoduyet = await db.DangKyKhoaHocs.CountAsync(d => d.TrangThai == "ChoDuyet");

        return $"""
            Bạn là trợ lý AI quản lý của Trung tâm Ngoại ngữ NNL. Hỗ trợ Admin phân tích dữ liệu và tư vấn quản lý.

            THỐNG KÊ TỔNG QUAN (cập nhật hôm nay {today:dd/MM/yyyy}):
            - Khóa học đang mở: {tongKhoaHocMo}
            - Học viên đang theo học (đã duyệt): {tongHocVienDangHoc}
            - Tổng giảng viên: {tongGiangVien}
            - Doanh thu tháng {today.Month}/{today.Year}: {doanhThuThang:#,##0} VNĐ
            - Đơn đăng ký chờ duyệt: {dangKyChoduyet}

            TOP 5 KHÓA HỌC (theo số học viên):
            {string.Join("\n", topLines)}

            Hỗ trợ Admin về: phân tích số liệu, tình trạng tuyển sinh, doanh thu, quản lý nhân sự, đề xuất chiến lược phát triển.
            Trả lời ngắn gọn, chính xác. Nếu Admin hỏi dữ liệu ngoài phạm vi trên, hướng dẫn họ vào trang báo cáo.
            """;
    }

    // ─── GiangVien ────────────────────────────────────────────────────────────
    private static async Task<string> BuildGiangVienPromptAsync(int userId, AppDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var gv = await db.GiangViens
            .Include(g => g.NguoiDung)
            .FirstOrDefaultAsync(g => g.Id == userId);

        // Upcoming sessions for this teacher
        var upcomingEntitiesGV = await db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Where(l => l.LopHoc.GiangVienId == userId && l.NgayHoc >= today)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .Take(20)
            .ToListAsync();

        var upcomingSessions = upcomingEntitiesGV;

        var sessionLines = upcomingEntitiesGV.Select(l =>
            $"- {l.NgayHoc:dd/MM/yyyy} ({l.LopHoc?.TenLop}) {l.GioBatDau:HH:mm}-{l.GioKetThuc:HH:mm} | {l.LopHoc?.KhoaHoc?.TenKhoaHoc} | Phòng: {l.PhongHoc ?? "—"}");

        // Classes managed by this teacher
        var lopHocs = await db.LopHocs
            .Include(l => l.KhoaHoc)
            .Include(l => l.DangKys)
            .Where(l => l.GiangVienId == userId && l.TrangThai != "DaKetThuc")
            .ToListAsync();

        var lopLines = lopHocs.Select(l =>
            $"- {l.TenLop} ({l.KhoaHoc?.TenKhoaHoc}) | {l.DangKys.Count(d => d.TrangThai == "DaDuyet")}/{l.SiSoToiDa} HV | TT: {l.TrangThai}");

        return $"""
            Bạn là trợ lý AI hỗ trợ giảng viên của Trung tâm Ngoại ngữ NNL.

            THÔNG TIN GIẢNG VIÊN:
            - Họ tên: {gv?.HoTen ?? "Chưa xác định"}
            - Chuyên môn: {gv?.ChuyenMon ?? "Chưa cập nhật"}
            - Bằng cấp: {gv?.BangCap ?? "Chưa cập nhật"}
            - Kinh nghiệm: {(gv?.KinhNghiem.HasValue == true ? $"{gv.KinhNghiem} năm" : "Chưa cập nhật")}

            LỊCH DẠY SẮP TỚI (20 buổi gần nhất từ {today:dd/MM/yyyy}):
            {(upcomingEntitiesGV.Any() ? string.Join("\n", sessionLines) : "Chưa có lịch dạy sắp tới.")}

            LỚP ĐANG PHỤ TRÁCH:
            {(lopHocs.Any() ? string.Join("\n", lopLines) : "Chưa phụ trách lớp nào.")}

            Hỗ trợ giảng viên về: tra cứu lịch dạy, thông tin lớp học, danh sách học viên, điểm số.
            Trả lời ngắn gọn, chuyên nghiệp bằng tiếng Việt.
            """;
    }

    // ─── HocVien ──────────────────────────────────────────────────────────────
    private static async Task<string> BuildHocVienPromptAsync(int userId, AppDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var hv = await db.HocViens
            .Include(h => h.NguoiDung)
            .FirstOrDefaultAsync(h => h.Id == userId);

        // Enrolled registrations with grade info
        var dangKys = await db.DangKyKhoaHocs
            .Where(d => d.HocVienId == userId)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc)
            .Include(d => d.LopHoc).ThenInclude(l => l.KhoaHoc).ThenInclude(k => k.PhanCongs).ThenInclude(p => p.GiangVien)
            .Include(d => d.Diem)
            .ToListAsync();

        var approvedDks = dangKys.Where(d => d.TrangThai == "DaDuyet").ToList();
        var enrolledKhoaHocIds = approvedDks.Select(d => d.LopHoc.KhoaHocId).Distinct().ToList();
        var enrolledLopIdsHV = approvedDks.Select(d => d.LopHocId).Distinct().ToList();

        // Upcoming sessions from enrolled lop hocs
        var upcomingEntitiesHV = await db.LichHocs
            .Include(l => l.LopHoc).ThenInclude(lh => lh.KhoaHoc)
            .Include(l => l.LopHoc).ThenInclude(lh => lh.GiangVien)
            .Where(l => enrolledLopIdsHV.Contains(l.LopHocId) && l.NgayHoc >= today)
            .OrderBy(l => l.NgayHoc).ThenBy(l => l.GioBatDau)
            .Take(15)
            .ToListAsync();

        var sessionLines = upcomingEntitiesHV.Select(l =>
            $"- {l.NgayHoc:dd/MM/yyyy} ({l.LopHoc?.TenLop}) {l.GioBatDau:HH:mm}-{l.GioKetThuc:HH:mm} | {l.LopHoc?.KhoaHoc?.TenKhoaHoc} | Phòng: {l.PhongHoc ?? "—"} | GV: {l.LopHoc?.GiangVien?.HoTen ?? "—"}");

        // Grade summary
        var gradeLines = approvedDks
            .Where(d => d.Diem != null)
            .Select(d => $"- {d.LopHoc.KhoaHoc.TenKhoaHoc}: Điểm TK = {d.Diem!.DiemTongKet?.ToString("F1") ?? "Chưa có"} | Xếp loại: {d.Diem.XepLoai ?? "—"}");

        // Registration status summary
        var dangKyLines = dangKys.Select(d =>
        {
            var tts = d.TrangThai switch
            {
                "DaDuyet" => "Đã duyệt", "ChoDuyet" => "Chờ duyệt",
                "TuChoi" => "Từ chối", "DaHuy" => "Đã hủy", _ => d.TrangThai
            };
            return $"- {d.LopHoc.KhoaHoc.TenKhoaHoc} ({d.LopHoc.TenLop}): {tts}";
        });

        // Available courses
        var availableCourses = await db.KhoaHocs
            .Where(k => k.TrangThai == "DangMo" && !enrolledKhoaHocIds.Contains(k.Id))
            .Include(k => k.PhanCongs).ThenInclude(p => p.GiangVien)
            .Include(k => k.LopHocs).ThenInclude(l => l.DangKys)
            .Take(10)
            .ToListAsync();

        var courseInfoLines = availableCourses.Select(k =>
        {
            var gvTen = k.PhanCongs.FirstOrDefault(p => p.IsActive)?.GiangVien?.HoTen ?? "Chưa phân công";
            var soHv = k.LopHocs.Sum(l => l.DangKys.Count(d => d.TrangThai == "DaDuyet"));
            var conCho = k.LopHocs.Sum(l => l.SiSoToiDa) - soHv;
            return $"- {k.TenKhoaHoc} | {k.NgonNgu} - {k.TrinhDo} | Học phí: {k.HocPhi:#,##0}đ | GV: {gvTen} | Còn chỗ: {Math.Max(0, conCho)}";
        });

        return $"""
            Bạn là tư vấn viên AI của Trung tâm Ngoại ngữ NNL. Tư vấn thân thiện, ngắn gọn bằng tiếng Việt.

            THÔNG TIN HỌC VIÊN:
            - Họ tên: {hv?.HoTen ?? "Chưa xác định"}
            - Trình độ hiện tại: {hv?.TrinhDoHienTai ?? "Chưa xác định"}
            - Ngôn ngữ quan tâm: {hv?.NgonNguQuanTam ?? "Chưa xác định"}

            TÌNH TRẠNG ĐĂNG KÝ:
            {(dangKys.Any() ? string.Join("\n", dangKyLines) : "Chưa có đăng ký nào.")}

            LỊCH HỌC SẮP TỚI (15 buổi gần nhất từ {today:dd/MM/yyyy}):
            {(upcomingEntitiesHV.Any() ? string.Join("\n", sessionLines) : "Chưa có lịch học sắp tới.")}

            ĐIỂM SỐ:
            {(gradeLines.Any() ? string.Join("\n", gradeLines) : "Chưa có điểm số nào.")}

            KHÓA HỌC CÒN CÓ THỂ ĐĂNG KÝ:
            {(courseInfoLines.Any() ? string.Join("\n", courseInfoLines) : "Hiện tất cả khóa học đã đầy hoặc bạn đã đăng ký hết.")}

            Khi tư vấn: dựa vào trình độ và ngôn ngữ quan tâm để gợi ý đúng khóa. Trả lời ngắn gọn (3-5 câu). Nhớ ngữ cảnh các câu hỏi trước.
            """;
    }

    private static string ExtractContent(string raw)
    {
        try
        {
            dynamic? obj = JsonConvert.DeserializeObject(raw);
            return (string?)obj?.choices[0]?.message?.content ?? "Xin lỗi, không thể nhận phản hồi từ AI.";
        }
        catch
        {
            return "Xin lỗi, đã xảy ra lỗi khi xử lý phản hồi.";
        }
    }
}
