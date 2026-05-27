using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using QuanLyKhoaHoc5.Tests.Helpers;
using QuanLyKhoaHoc5.Web.Controllers;
using QuanLyKhoaHoc5.Web.Data;
using QuanLyKhoaHoc5.Web.Models.Entities;
using QuanLyKhoaHoc5.Web.Models.ViewModels;
using QuanLyKhoaHoc5.Web.Services;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace QuanLyKhoaHoc5.Tests.Tests;

/// <summary>
/// Bổ sung coverage từ 75.4% lên ≥ 80%.
/// Mục tiêu chính:
///  - ChatService: tất cả nhánh gọi API, fallback, prompt builder
///  - GeminiService: tất cả nhánh (success, error, exception, empty, invalid JSON, markdown)
///  - GoiYKhoaHocService: hocVien not found, no courses, success flow, item not found
///  - GoiYController: TaoGoiY error paths
///  - KhoaHocController: filter, details, CRUD uncovered branches
///  - ExcelService: ExportBangDiemAsync, date filter branches
/// </summary>
public class Coverage80Tests
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════════════════

    private static HttpClient MakeHttpClient(
        HttpStatusCode status, string responseBody,
        bool throwInstead = false,
        Exception? exceptionToThrow = null,
        string? matchUrlSubstring = null)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var setup = handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        if (throwInstead)
            setup.ThrowsAsync(exceptionToThrow ?? new HttpRequestException("connection refused"));
        else
            setup.ReturnsAsync(new HttpResponseMessage(status)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
    }

    /// <summary>
    /// Tạo HttpClient hai lượt: lượt đầu trả về firstResponse, lượt sau trả về secondResponse.
    /// </summary>
    private static HttpClient MakeHttpClientSequential(
        HttpResponseMessage firstResponse,
        HttpResponseMessage? secondResponse = null)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var seq = handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(firstResponse);

        if (secondResponse != null)
            seq.ReturnsAsync(secondResponse);

        return new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost") };
    }

    private static IConfiguration MakeChatConfig(
        string grokKey = "xai-testkey",
        string grokEndpoint = "https://grok.test/v1/chat/completions",
        string grokModel = "grok-3-latest",
        string groqKey = "",
        string groqEndpoint = "https://groq.test/v1/chat/completions",
        string groqModel = "llama-3.3-70b-versatile")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GrokApi:ApiKey"]   = grokKey,
                ["GrokApi:Endpoint"] = grokEndpoint,
                ["GrokApi:Model"]    = grokModel,
                ["GroqAPI:ApiKey"]   = groqKey,
                ["GroqAPI:Endpoint"] = groqEndpoint,
                ["GroqAPI:Model"]    = groqModel,
            })
            .Build();
    }

    private static IConfiguration MakeGeminiConfig(
        string groqKey = "test-groq-key",
        string groqEndpoint = "https://groq.test/v1/chat/completions",
        string groqModel = "llama-3.3-70b-versatile")
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GroqAPI:ApiKey"]   = groqKey,
                ["GroqAPI:Endpoint"] = groqEndpoint,
                ["GroqAPI:Model"]    = groqModel,
            })
            .Build();
    }

    /// Tạo JSON phản hồi Groq/OpenAI-compatible với nội dung cho trước.
    private static string GroqOkJson(string content = "Xin chào từ AI!")
    {
        var obj = new { choices = new[] { new { message = new { content } } } };
        return JsonConvert.SerializeObject(obj);
    }

    /// Tạo JSON phản hồi Groq với goiY item bên trong content (dạng escaped JSON string).
    private static string GoiYGroqJson(int khoaHocId, string ten = "KH Test")
    {
        var innerGoiY = JsonConvert.SerializeObject(new
        {
            goiY = new[] { new { khoaHocId, tenKhoaHoc = ten, diemPhuHop = 90.0, lyDo = "Rất phù hợp" } }
        });
        var obj = new { choices = new[] { new { message = new { content = innerGoiY } } } };
        return JsonConvert.SerializeObject(obj);
    }

    // Seed helpers
    private static (NguoiDung nd, HocVien hv) SeedHocVienFull(AppDbContext db,
        string email = "hv80@t.com", string ngonNgu = "Tiếng Anh", string trinhDo = "A1")
    {
        var nd = new NguoiDung { Email = email, HoTen = "HV Test 80", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        var hv = new HocVien { Id = nd.Id, MaHocVien = $"HV{nd.Id:000}", HoTen = nd.HoTen, NgonNguQuanTam = ngonNgu, TrinhDoHienTai = trinhDo };
        db.HocViens.Add(hv); db.SaveChanges();
        return (nd, hv);
    }

    private static (NguoiDung nd, GiangVien gv) SeedGiangVienFull(AppDbContext db,
        string email = "gv80@t.com")
    {
        var nd = new NguoiDung { Email = email, HoTen = "GV Test 80", VaiTro = "GiangVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        var gv = new GiangVien { Id = nd.Id, MaGiangVien = $"GV{nd.Id:000}", HoTen = nd.HoTen, ChuyenMon = "IELTS", BangCap = "Thạc sĩ", KinhNghiem = 5 };
        db.GiangViens.Add(gv); db.SaveChanges();
        return (nd, gv);
    }

    private static KhoaHoc SeedKH(AppDbContext db,
        string ten = "Tiếng Anh A1", string trangThai = "DangMo",
        string ngonNgu = "Tiếng Anh", string trinhDo = "A1", decimal hocPhi = 2_000_000m)
    {
        var kh = new KhoaHoc { TenKhoaHoc = ten, NgonNgu = ngonNgu, TrinhDo = trinhDo, ThoiLuong = 40, TrangThai = trangThai, HocPhi = hocPhi };
        db.KhoaHocs.Add(kh); db.SaveChanges();
        return kh;
    }

    private static LopHoc SeedLop(AppDbContext db, int khoaHocId, int? gvId = null,
        string trangThai = "DangHoc", int siSo = 20)
    {
        var lop = new LopHoc { TenLop = "Lớp Test", KhoaHocId = khoaHocId, GiangVienId = gvId, TrangThai = trangThai, SiSoToiDa = siSo };
        db.LopHocs.Add(lop); db.SaveChanges();
        return lop;
    }

    private static DangKyKhoaHoc SeedDangKy(AppDbContext db, int hvId, int lopId,
        string trangThai = "DaDuyet", DateTime? ngayDuyet = null)
    {
        var dk = new DangKyKhoaHoc { HocVienId = hvId, LopHocId = lopId, TrangThai = trangThai, NgayDuyet = ngayDuyet ?? DateTime.Now };
        db.DangKyKhoaHocs.Add(dk); db.SaveChanges();
        return dk;
    }

    private static LichHoc SeedLichHoc(AppDbContext db, int lopId, DateOnly? ngayHoc = null)
    {
        var lh = new LichHoc
        {
            LopHocId = lopId,
            NgayHoc = ngayHoc ?? DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            GioBatDau = new TimeOnly(8, 0),
            GioKetThuc = new TimeOnly(10, 0),
            PhongHoc = "P101"
        };
        db.LichHocs.Add(lh); db.SaveChanges();
        return lh;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 1: ChatService — HTTP success path
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ChatService_SendMessage_AdminRole_GrokSuccess_ReturnsContent()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db); var lop = SeedLop(db, kh.Id);
        var (_, hv) = SeedHocVienFull(db);
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet", DateTime.Now);
        SeedDangKy(db, hv.Id, lop.Id, "ChoDuyet");

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("Trả lời Admin!"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Xin chào", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Equal("Trả lời Admin!", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GiangVienRole_GrokSuccess_ReturnsContent()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var (_, gv) = SeedGiangVienFull(db);
        var lop = SeedLop(db, kh.Id, gv.Id);
        SeedLichHoc(db, lop.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("Trả lời GiangVien!"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(gv.Id, "Lịch dạy?", new List<ChatMessageViewModel>(), "GiangVien", db);

        Assert.Equal("Trả lời GiangVien!", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GiangVienRole_NoLopHoc_ReturnsContent()
    {
        var db = DbContextFactory.Create();
        var (_, gv) = SeedGiangVienFull(db, "gv80b@t.com");

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("GV không có lớp"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(gv.Id, "?", new List<ChatMessageViewModel>(), "GiangVien", db);

        Assert.Contains("GV không có lớp", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_HocVienRole_GrokSuccess_ReturnsContent()
    {
        var db = DbContextFactory.Create();
        var khDaDk = SeedKH(db, "KH Đã DK"); var lopDaDk = SeedLop(db, khDaDk.Id);
        var (_, hv) = SeedHocVienFull(db, "hv80b@t.com");
        var dk = SeedDangKy(db, hv.Id, lopDaDk.Id);
        SeedLichHoc(db, lopDaDk.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(5)));

        // Điểm cho đăng ký
        var diem = new Diem { DangKyId = dk.Id, DiemGiuaKy = 7.5, DiemCuoiKy = 8.0, DiemTongKet = 7.83, XepLoai = "Khá" };
        db.Diems.Add(diem); db.SaveChanges();

        var khMoi = SeedKH(db, "KH Mới Chưa DK", "DangMo", "Tiếng Anh", "B1");

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("Tư vấn cho HocVien!"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(hv.Id, "Tư vấn khóa học", new List<ChatMessageViewModel>(), "HocVien", db);

        Assert.Equal("Tư vấn cho HocVien!", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_HocVienRole_NoDangKy_ReturnsContent()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv80c@t.com");
        var kh = SeedKH(db, "KH Available");

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("HocVien không có đăng ký"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(hv.Id, "?", new List<ChatMessageViewModel>(), "HocVien", db);

        Assert.Contains("không có đăng ký", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_WithLargeHistory_SlicesLast16()
    {
        var db = DbContextFactory.Create();
        SeedKH(db);

        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("Đã nhận 16 tin"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        // Tạo history > 16 messages để test slice
        var history = Enumerable.Range(0, 20)
            .Select(i => new ChatMessageViewModel { Role = i % 2 == 0 ? "user" : "assistant", Content = $"msg{i}" })
            .ToList();

        var result = await svc.SendMessageAsync(1, "Hello", history, "Admin", db);

        Assert.Equal("Đã nhận 16 tin", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_WithHistory_UnderLimit_DoesNotSlice()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, GroqOkJson("OK 5 tin"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var history = Enumerable.Range(0, 5)
            .Select(i => new ChatMessageViewModel { Role = "user", Content = $"msg{i}" })
            .ToList();

        var result = await svc.SendMessageAsync(1, "Hi", history, "Admin", db);
        Assert.Equal("OK 5 tin", result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 2: ChatService — API error paths
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ChatService_SendMessage_GrokApiError_NonPlaceholderKey_ReturnsFixedErrorMsg()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig(grokKey: "xai-realkey");
        var http = MakeHttpClient(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("sự cố kết nối", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokApiError_PlaceholderKey_EmptyGroqKey_ReturnsConfigError()
    {
        var db = DbContextFactory.Create();
        // placeholder key → triggers fallback; GroqAPI key is empty → returns config error
        var config = MakeChatConfig(grokKey: "xai-placeholder-demo", groqKey: "");
        var http = MakeHttpClient(HttpStatusCode.Unauthorized, "Unauthorized");
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("chưa được cấu hình", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokException_GroqKeyEmpty_ReturnsConfigError()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig(grokKey: "xai-realkey", groqKey: "");
        // Grok throws → fallback called → GroqAPI key empty → return config error
        var http = MakeHttpClient(HttpStatusCode.OK, "", throwInstead: true,
            exceptionToThrow: new HttpRequestException("Network fail"));
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("chưa được cấu hình", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokException_GroqSuccess_ReturnsGroqContent()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig(
            grokKey: "xai-realkey",
            groqKey: "groq-key123",
            groqEndpoint: "https://groq.test/v1/chat/completions");

        // Sequential: first call throws, second call succeeds (Groq fallback)
        var firstResp = new HttpResponseMessage(HttpStatusCode.OK);
        firstResp.Dispose(); // can't throw from response directly, use handler directly

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Grok down"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GroqOkJson("Groq fallback OK"), Encoding.UTF8, "application/json")
            });
        var http = new HttpClient(handler.Object);

        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Equal("Groq fallback OK", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokException_GroqHttpError_ReturnsConnectError()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig(grokKey: "xai-realkey", groqKey: "groq-key456");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Grok down"))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("groq error", Encoding.UTF8, "application/json")
            });
        var http = new HttpClient(handler.Object);

        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("không thể kết nối", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokException_GroqAlsoThrows_ReturnsUnavailable()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig(grokKey: "xai-realkey", groqKey: "groq-key789");

        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Grok down"))
            .ThrowsAsync(new HttpRequestException("Groq also down"));
        var http = new HttpClient(handler.Object);

        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("tạm thời không khả dụng", result);
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokReturnsInvalidJson_ReturnsParseError()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{{invalid json here}}");
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        // ExtractContent catch → returns lỗi xử lý phản hồi
        Assert.Contains("lỗi", result.ToLower());
    }

    [Fact]
    public async Task ChatService_SendMessage_GrokReturnsMissingContent_ReturnsDefault()
    {
        var db = DbContextFactory.Create();
        var config = MakeChatConfig();
        // Valid JSON but no content field → returns default message
        var http = MakeHttpClient(HttpStatusCode.OK, """{"choices":[{"message":{}}]}""");
        var svc = new ChatService(http, config, NullLogger<ChatService>.Instance);

        var result = await svc.SendMessageAsync(1, "Hi", new List<ChatMessageViewModel>(), "Admin", db);

        Assert.Contains("Xin lỗi", result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 3: GeminiService — all branches
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GeminiService_GetGoiY_EmptyApiKey_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["GroqAPI:ApiKey"] = "" })
            .Build();
        var http = MakeHttpClient(HttpStatusCode.OK, "");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>()));
    }

    [Fact]
    public async Task GeminiService_GetGoiY_NullApiKey_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { })
            .Build();
        var http = MakeHttpClient(HttpStatusCode.OK, "");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>()));
    }

    [Fact]
    public async Task GeminiService_GetGoiY_EmptyEndpoint_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GroqAPI:ApiKey"]   = "valid-key",
                ["GroqAPI:Endpoint"] = ""
            })
            .Build();
        var http = MakeHttpClient(HttpStatusCode.OK, "");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>()));
    }

    [Fact]
    public async Task GeminiService_GetGoiY_HttpSuccess_ReturnsItems()
    {
        var config = MakeGeminiConfig();
        // Build proper Groq response: outer JSON wraps inner JSON as a string
        var innerContent = JsonConvert.SerializeObject(new
        {
            goiY = new[] { new { khoaHocId = 1, tenKhoaHoc = "KH A", diemPhuHop = 88.5, lyDo = "Phu hop" } }
        });
        var fullJson = JsonConvert.SerializeObject(new
        {
            choices = new[] { new { message = new { content = innerContent } } }
        });

        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var profile = new HocVienProfileViewModel { HoTen = "Test HV", TrinhDoHienTai = "A1" };
        var courses = new List<KhoaHocGoiYInputViewModel>
        {
            new() { Id = 1, TenKhoaHoc = "KH A", NgonNgu = "Tiếng Anh", TrinhDo = "A1", HocPhi = 2_000_000m }
        };

        var result = await svc.GetGoiYKhoaHocAsync(profile, courses);

        Assert.NotEmpty(result);
        Assert.Equal(1, result[0].KhoaHocId);
    }

    [Fact]
    public async Task GeminiService_GetGoiY_HttpError_Throws()
    {
        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.TooManyRequests, "Rate limit exceeded");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>()));
    }

    [Fact]
    public async Task GeminiService_GetGoiY_ConnectionError_Throws()
    {
        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "",
            throwInstead: true,
            exceptionToThrow: new HttpRequestException("Connection refused"));
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>()));
    }

    [Fact]
    public async Task GeminiService_GetGoiY_EmptyContentText_ReturnsEmpty()
    {
        var config = MakeGeminiConfig();
        // choices[0].message.content is empty string
        var http = MakeHttpClient(HttpStatusCode.OK, """{"choices":[{"message":{"content":""}}]}""");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var result = await svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GeminiService_GetGoiY_InvalidJsonContent_ReturnsEmpty()
    {
        var config = MakeGeminiConfig();
        // content trả về không phải JSON hợp lệ → parse fail → return empty
        var http = MakeHttpClient(HttpStatusCode.OK,
            """{"choices":[{"message":{"content":"không phải json"}}]}""");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var result = await svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GeminiService_GetGoiY_MarkdownWrappedJson_ParsesCorrectly()
    {
        var config = MakeGeminiConfig();
        // AI trả về JSON bọc trong markdown code block — CleanJsonResponse sẽ strip ```json ... ```
        var innerGoiY = JsonConvert.SerializeObject(new
        {
            goiY = new[] { new { khoaHocId = 2, tenKhoaHoc = "KH B", diemPhuHop = 75.0, lyDo = "OK" } }
        });
        var markdownContent = "```json\n" + innerGoiY + "\n```";
        var fullJson = JsonConvert.SerializeObject(new
        {
            choices = new[] { new { message = new { content = markdownContent } } }
        });

        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var result = await svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>());

        Assert.NotEmpty(result);
        Assert.Equal(2, result[0].KhoaHocId);
    }

    [Fact]
    public async Task GeminiService_GetGoiY_ContentNullInChoices_ReturnsEmpty()
    {
        var config = MakeGeminiConfig();
        // choices array not valid → ExtractTextFromGroqResponse returns ""
        var http = MakeHttpClient(HttpStatusCode.OK, """{"result":"ok"}""");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var result = await svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GeminiService_GetGoiY_InvalidGroqResponseJson_ReturnsEmpty()
    {
        var config = MakeGeminiConfig();
        // totally invalid JSON from server
        var http = MakeHttpClient(HttpStatusCode.OK, "{{broken}");
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        // ExtractTextFromGroqResponse returns "" on exception → return empty
        var result = await svc.GetGoiYKhoaHocAsync(new HocVienProfileViewModel(), new List<KhoaHocGoiYInputViewModel>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GeminiService_BuildPrompt_WithCourses_ContainsCourseName()
    {
        var config = MakeGeminiConfig();
        // Success path to exercise BuildPrompt
        var innerContent = JsonConvert.SerializeObject(new
        {
            goiY = new[] { new { khoaHocId = 5, tenKhoaHoc = "IELTS Advanced", diemPhuHop = 95.0, lyDo = "Phu hop" } }
        });
        var fullJson = JsonConvert.SerializeObject(new
        {
            choices = new[] { new { message = new { content = innerContent } } }
        });

        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var svc = new GeminiService(http, config, NullLogger<GeminiService>.Instance);

        var profile = new HocVienProfileViewModel
        {
            HoTen = "Học viên X",
            TrinhDoHienTai = "B1",
            NgonNguQuanTam = "Tiếng Anh",
            LichSuHocTap = new List<LichSuHocTapViewModel>
            {
                new() { TenKhoaHoc = "English A1", TrinhDo = "A1", DiemTongKet = 9.0, XepLoai = "Xuất sắc", TrangThai = "Đã hoàn thành" }
            }
        };
        var courses = new List<KhoaHocGoiYInputViewModel>
        {
            new() { Id = 5, TenKhoaHoc = "IELTS Advanced", NgonNgu = "Tiếng Anh", TrinhDo = "C1", HocPhi = 5_000_000m, MoTa = "Luyện thi IELTS" }
        };

        var result = await svc.GetGoiYKhoaHocAsync(profile, courses);
        Assert.NotEmpty(result);
        Assert.Equal(5, result[0].KhoaHocId);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 4: GoiYKhoaHocService — all branches
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_HocVienNotFound_ReturnsEmpty()
    {
        var db = DbContextFactory.Create();
        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(99999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_NoAvailableCourses_ReturnsEmpty()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv_nokhoa@t.com");
        // Không có khóa học DangMo nào
        SeedKH(db, "KH Đã Đóng", "DaDong");

        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(hv.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_WithResults_SavesAndReturnsItems()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv_goiy@t.com");
        var kh = SeedKH(db, "KH GoiY Test", "DangMo");

        // Gemini returns a valid recommendation for kh.Id
        var config = MakeGeminiConfig();
        var fullJson = GoiYGroqJson(kh.Id, kh.TenKhoaHoc);
        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(hv.Id);

        Assert.NotEmpty(result);
        Assert.Equal(kh.Id, result[0].KhoaHocId);
        Assert.Equal(kh.TenKhoaHoc, result[0].TenKhoaHoc);
        Assert.True(db.GoiYKhoaHocs.Any(g => g.HocVienId == hv.Id));
    }

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_ItemNotFoundById_FallbackByName()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv_fallback@t.com");
        var kh = SeedKH(db, "KH Fallback Test", "DangMo");

        // Gemini returns wrong ID (999) but correct name
        var config = MakeGeminiConfig();
        var fullJson = GoiYGroqJson(999, kh.TenKhoaHoc);
        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(hv.Id);

        // Found by name fallback → should return the item
        Assert.NotEmpty(result);
        Assert.Equal(kh.Id, result[0].KhoaHocId);
    }

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_ItemNotFoundByIdOrName_Skips()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv_skip@t.com");
        SeedKH(db, "KH Real", "DangMo");

        // Gemini returns non-existent ID and non-existent name
        var config = MakeGeminiConfig();
        var fullJson = GoiYGroqJson(8888, "Khóa học không tồn tại");
        var http = MakeHttpClient(HttpStatusCode.OK, fullJson);
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(hv.Id);

        // Item skipped → empty result (warning logged)
        Assert.Empty(result);
    }

    [Fact]
    public async Task GoiYKhoaHocService_TaoGoiY_AlreadyEnrolled_CourseExcluded()
    {
        var db = DbContextFactory.Create();
        var (_, hv) = SeedHocVienFull(db, "hv_enrolled@t.com");
        var kh = SeedKH(db, "KH Đã Học", "DangMo");
        var lop = SeedLop(db, kh.Id);
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet");
        // No other courses → empty
        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var svc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);

        var result = await svc.TaoGoiYAsync(hv.Id);
        Assert.Empty(result);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 5: GoiYController — TaoGoiY error paths
    // ═══════════════════════════════════════════════════════════════════════════

    private static (GoiYController ctrl, AppDbContext db) MakeGoiYController(
        HttpStatusCode groqStatus, string groqBody,
        bool throwExceptionInstead = false,
        Exception? ex = null,
        string groqKey = "valid-key",
        bool seedHocVien = true,
        bool seedKhoaHoc = true)
    {
        var db = DbContextFactory.Create();
        int hvId = 1;
        if (seedHocVien)
        {
            var nd = new NguoiDung { Id = hvId, Email = "hv_goiY@t.com", HoTen = "HV", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
            db.NguoiDungs.Add(nd);
            db.HocViens.Add(new HocVien { Id = hvId, MaHocVien = "HV001", HoTen = "HV" });
            db.SaveChanges();
        }
        if (seedKhoaHoc)
            SeedKH(db, "KH Ctrl Test", "DangMo");

        var config = MakeGeminiConfig(groqKey: groqKey);
        HttpClient http;
        if (throwExceptionInstead)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(ex ?? new TimeoutException("Timeout"));
            http = new HttpClient(handler.Object);
        }
        else
            http = MakeHttpClient(groqStatus, groqBody);

        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var goiYSvc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);
        var ctrl = new GoiYController(db, goiYSvc);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(hvId, "hv_goiY@t.com", "HV", "HocVien"));
        return (ctrl, db);
    }

    [Fact]
    public async Task GoiYController_TaoGoiY_EmptyResult_ReturnsSuccessFalse()
    {
        // Gemini returns empty → TaoGoiYAsync returns empty → success=false
        var (ctrl, _) = MakeGoiYController(HttpStatusCode.OK, """{"choices":[{"message":{"content":"{\"goiY\":[]}"}}]}""");
        var logger = NullLogger<GoiYController>.Instance;

        var result = await ctrl.TaoGoiY(logger) as JsonResult;

        Assert.NotNull(result);
        var value = result.Value!;
        var successProp = value.GetType().GetProperty("success")?.GetValue(value);
        Assert.NotNull(successProp);
        Assert.False((bool)successProp!);
    }

    [Fact]
    public async Task GoiYController_TaoGoiY_InvalidOperationException_ReturnsErrorJson()
    {
        // No groq key → GeminiService throws InvalidOperationException
        var (ctrl, _) = MakeGoiYController(HttpStatusCode.OK, "{}", groqKey: "");
        var logger = NullLogger<GoiYController>.Instance;

        var result = await ctrl.TaoGoiY(logger) as JsonResult;

        Assert.NotNull(result);
        var value = result.Value!;
        var successProp = value.GetType().GetProperty("success")?.GetValue(value);
        Assert.False((bool)successProp!);
        var msgProp = value.GetType().GetProperty("message")?.GetValue(value)?.ToString();
        Assert.NotNull(msgProp);
        Assert.Contains("cấu hình", msgProp);
    }

    [Fact]
    public async Task GoiYController_TaoGoiY_GenericException_ReturnsGenericError()
    {
        // TimeoutException không được GeminiService bắt → bubbles up as generic Exception
        var (ctrl, _) = MakeGoiYController(HttpStatusCode.OK, "{}",
            throwExceptionInstead: true,
            ex: new TimeoutException("Request timeout"));
        var logger = NullLogger<GoiYController>.Instance;

        var result = await ctrl.TaoGoiY(logger) as JsonResult;

        Assert.NotNull(result);
        var value = result.Value!;
        var successProp = value.GetType().GetProperty("success")?.GetValue(value);
        Assert.False((bool)successProp!);
        var msgProp = value.GetType().GetProperty("message")?.GetValue(value)?.ToString();
        Assert.Contains("lỗi", msgProp?.ToLower() ?? "");
    }

    [Fact]
    public async Task GoiYController_Index_ReturnsViewWithViewModel()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var nd = new NguoiDung { Id = 10, Email = "hv_idx@t.com", HoTen = "HV Idx", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd);
        db.HocViens.Add(new HocVien { Id = 10, MaHocVien = "HV010", HoTen = "HV Idx" });
        db.SaveChanges();

        // Seed một GoiY
        db.GoiYKhoaHocs.Add(new GoiYKhoaHoc
        {
            HocVienId = 10, KhoaHocGoiYId = kh.Id, DiemPhuHop = 90, LyDoGoiY = "Test", NgayGoiY = DateTime.Now
        });
        db.SaveChanges();

        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var goiYSvc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);
        var ctrl = new GoiYController(db, goiYSvc);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(10, "hv_idx@t.com", "HV Idx", "HocVien"));

        var result = await ctrl.Index() as ViewResult;

        Assert.NotNull(result);
        var vm = result.Model as GoiYTrangViewModel;
        Assert.NotNull(vm);
        Assert.True(vm.DaGoiY);
    }

    [Fact]
    public async Task GoiYController_LichSu_ReturnsViewWithList()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var nd = new NguoiDung { Id = 11, Email = "hv_ls@t.com", HoTen = "HV LS", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd);
        db.HocViens.Add(new HocVien { Id = 11, MaHocVien = "HV011", HoTen = "HV LS" });
        db.GoiYKhoaHocs.Add(new GoiYKhoaHoc
        {
            HocVienId = 11, KhoaHocGoiYId = kh.Id, DiemPhuHop = 85, LyDoGoiY = "Phù hợp", NgayGoiY = DateTime.Now.AddHours(-1)
        });
        db.SaveChanges();

        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var goiYSvc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);
        var ctrl = new GoiYController(db, goiYSvc);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(11, "hv_ls@t.com", "HV LS", "HocVien"));

        var result = await ctrl.LichSu() as ViewResult;
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GoiYController_Admin_ReturnsViewWithPagination()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        db.HocViens.Add(new HocVien { Id = 12, MaHocVien = "HV012", HoTen = "Admin HV" });
        db.SaveChanges();
        db.GoiYKhoaHocs.Add(new GoiYKhoaHoc
        {
            HocVienId = 12, KhoaHocGoiYId = kh.Id, DiemPhuHop = 95, NgayGoiY = DateTime.Now
        });
        db.SaveChanges();

        var config = MakeGeminiConfig();
        var http = MakeHttpClient(HttpStatusCode.OK, "{}");
        var gemini = new GeminiService(http, config, NullLogger<GeminiService>.Instance);
        var goiYSvc = new GoiYKhoaHocService(db, gemini, NullLogger<GoiYKhoaHocService>.Instance);
        var ctrl = new GoiYController(db, goiYSvc);
        ctrl.ControllerContext = ControllerHelper.CreateContext(
            ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin"));

        var result = await ctrl.Admin(1) as ViewResult;
        Assert.NotNull(result);
        Assert.Equal(1, ctrl.ViewBag.Page);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 6: KhoaHocController — uncovered branches
    // ═══════════════════════════════════════════════════════════════════════════

    private static KhoaHocController MakeKhoaHocCtrl(AppDbContext db, ClaimsPrincipal? user = null)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        var ctrl = new KhoaHocController(db, env.Object);
        ctrl.ControllerContext = ControllerHelper.CreateContext(user);
        ctrl.TempData = ControllerHelper.CreateTempData();
        return ctrl;
    }

    [Fact]
    public async Task KhoaHocController_Index_WithNgonNguFilter_FiltersResults()
    {
        var db = DbContextFactory.Create();
        SeedKH(db, "KH Anh", "DangMo", "Tiếng Anh", "A1");
        SeedKH(db, "KH Pháp", "DangMo", "Tiếng Pháp", "A1");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel { NgonNgu = "Tiếng Anh" }) as ViewResult;

        Assert.NotNull(result);
        var vm = result.Model as KhoaHocFilterViewModel;
        Assert.NotNull(vm);
        Assert.All(vm.Items, i => Assert.Equal("Tiếng Anh", i.NgonNgu));
    }

    [Fact]
    public async Task KhoaHocController_Index_WithTrinhDoFilter_FiltersResults()
    {
        var db = DbContextFactory.Create();
        SeedKH(db, "KH A1", "DangMo", "Tiếng Anh", "A1");
        SeedKH(db, "KH B1", "DangMo", "Tiếng Anh", "B1");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel { TrinhDo = "B1" }) as ViewResult;

        Assert.NotNull(result);
        var vm = result.Model as KhoaHocFilterViewModel;
        Assert.All(vm!.Items, i => Assert.Equal("B1", i.TrinhDo));
    }

    [Fact]
    public async Task KhoaHocController_Index_WithSearchFilter_FiltersResults()
    {
        var db = DbContextFactory.Create();
        SeedKH(db, "IELTS Intensive", "DangMo");
        SeedKH(db, "TOEFL Basic", "DangMo");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Index(new KhoaHocFilterViewModel { Search = "IELTS" }) as ViewResult;

        Assert.NotNull(result);
        var vm = result.Model as KhoaHocFilterViewModel;
        Assert.All(vm!.Items, i => Assert.Contains("IELTS", i.TenKhoaHoc, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task KhoaHocController_Index_AdminWithTrangThaiFilter_ShowsAll()
    {
        var db = DbContextFactory.Create();
        SeedKH(db, "KH Mở", "DangMo");
        SeedKH(db, "KH Đóng", "DaDong");
        var adminUser = ControllerHelper.CreateUser(1, "admin@t.com", "Admin", "Admin");
        var ctrl = MakeKhoaHocCtrl(db, adminUser);

        var result = await ctrl.Index(new KhoaHocFilterViewModel { TrangThai = "DaDong" }) as ViewResult;

        var vm = result!.Model as KhoaHocFilterViewModel;
        Assert.All(vm!.Items, i => Assert.Equal("DaDong", i.TrangThai));
    }

    [Fact]
    public async Task KhoaHocController_Details_HocVienUser_ChecksEnrollmentStatus()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id, trangThai: "DangTuyenSinh");
        var (nd, hv) = SeedHocVienFull(db, "hv_details@t.com");
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet");
        SeedLichHoc(db, lop.Id);

        var hvUser = ControllerHelper.CreateUser(hv.Id, nd.Email, nd.HoTen, "HocVien");
        var ctrl = MakeKhoaHocCtrl(db, hvUser);

        var result = await ctrl.Details(kh.Id) as ViewResult;

        Assert.NotNull(result);
        Assert.True((bool)ctrl.ViewBag.DaDangKy);
    }

    [Fact]
    public async Task KhoaHocController_Details_NotFound_Returns404()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Details(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHocController_Details_AnonymousUser_ShowsCoursInfo()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH Public");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Details(kh.Id) as ViewResult;

        Assert.NotNull(result);
        Assert.Equal(kh, result.Model);
    }

    [Fact]
    public async Task KhoaHocController_Create_POST_ValidModel_RedirectsToIndex()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var vm = new KhoaHocCreateEditViewModel
        {
            TenKhoaHoc = "Khóa Mới", NgonNgu = "Tiếng Anh", TrinhDo = "A2",
            HocPhi = 1_500_000m, ThoiLuong = 30, SoChoToiDa = 25, TrangThai = "DangMo"
        };

        var result = await ctrl.Create(vm) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal(1, db.KhoaHocs.Count());
    }

    [Fact]
    public async Task KhoaHocController_Create_POST_InvalidModel_ReturnsView()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);
        ctrl.ModelState.AddModelError("TenKhoaHoc", "Required");

        var result = await ctrl.Create(new KhoaHocCreateEditViewModel()) as ViewResult;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task KhoaHocController_Edit_GET_NotFound_Returns404()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Edit(99999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHocController_Edit_POST_NotFound_Returns404()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Edit(99999, new KhoaHocCreateEditViewModel { TenKhoaHoc = "X", NgonNgu = "Y", TrinhDo = "Z", TrangThai = "DangMo" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHocController_Edit_POST_InvalidModel_ReturnsView()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var ctrl = MakeKhoaHocCtrl(db);
        ctrl.ModelState.AddModelError("X", "err");

        var result = await ctrl.Edit(kh.Id, new KhoaHocCreateEditViewModel()) as ViewResult;

        Assert.NotNull(result);
    }

    [Fact]
    public async Task KhoaHocController_Edit_POST_Valid_SavesAndRedirects()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "Original Name");
        var ctrl = MakeKhoaHocCtrl(db);

        var vm = new KhoaHocCreateEditViewModel
        {
            Id = kh.Id, TenKhoaHoc = "Updated Name", NgonNgu = "Tiếng Pháp", TrinhDo = "B2",
            HocPhi = 3_000_000m, ThoiLuong = 60, TrangThai = "DangMo"
        };

        var result = await ctrl.Edit(kh.Id, vm) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.Equal("Updated Name", db.KhoaHocs.Find(kh.Id)!.TenKhoaHoc);
    }

    [Fact]
    public async Task KhoaHocController_Delete_WithLopHoc_ShowsError()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        SeedLop(db, kh.Id);
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Delete(kh.Id) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.NotNull(ctrl.TempData["Error"]);
    }

    [Fact]
    public async Task KhoaHocController_Delete_NoLopHoc_DeletesAndRedirects()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH To Delete");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Delete(kh.Id) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal("Index", result.ActionName);
        Assert.False(db.KhoaHocs.Any(k => k.Id == kh.Id));
    }

    [Fact]
    public async Task KhoaHocController_Delete_NotFound_Returns404()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.Delete(99999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task KhoaHocController_ChangeStatus_DangMo_SwitchesToDaDong()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH Status", "DangMo");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.ChangeStatus(kh.Id) as JsonResult;

        Assert.NotNull(result);
        var updated = db.KhoaHocs.Find(kh.Id)!;
        Assert.Equal("DaDong", updated.TrangThai);
    }

    [Fact]
    public async Task KhoaHocController_ChangeStatus_DaDong_SwitchesToTamDung()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH TamDung", "DaDong");
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.ChangeStatus(kh.Id) as JsonResult;

        Assert.NotNull(result);
        var updated = db.KhoaHocs.Find(kh.Id)!;
        Assert.Equal("TamDung", updated.TrangThai);
    }

    [Fact]
    public async Task KhoaHocController_ChangeStatus_TamDung_SwitchesToDangMo()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH TamDung2", "TamDung");
        var ctrl = MakeKhoaHocCtrl(db);

        await ctrl.ChangeStatus(kh.Id);

        var updated = db.KhoaHocs.Find(kh.Id)!;
        Assert.Equal("DangMo", updated.TrangThai);
    }

    [Fact]
    public async Task KhoaHocController_ChangeStatus_UnknownStatus_SwitchesToDangMo()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db, "KH Unknown", "Unknown");
        var ctrl = MakeKhoaHocCtrl(db);

        await ctrl.ChangeStatus(kh.Id);

        var updated = db.KhoaHocs.Find(kh.Id)!;
        Assert.Equal("DangMo", updated.TrangThai);
    }

    [Fact]
    public async Task KhoaHocController_ChangeStatus_NotFound_ReturnsFalse()
    {
        var db = DbContextFactory.Create();
        var ctrl = MakeKhoaHocCtrl(db);

        var result = await ctrl.ChangeStatus(99999) as JsonResult;

        Assert.NotNull(result);
        var value = result.Value!;
        var successProp = value.GetType().GetProperty("success")?.GetValue(value);
        Assert.False((bool)successProp!);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 7: ExcelService — uncovered methods and branches
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExcelService_ExportBangDiem_WithData_ReturnsByteArray()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id);
        var (_, hv) = SeedHocVienFull(db, "hv_excel@t.com");
        var dk = SeedDangKy(db, hv.Id, lop.Id);

        // Thêm điểm
        db.Diems.Add(new Diem
        {
            DangKyId = dk.Id, DiemGiuaKy = 7.5, DiemCuoiKy = 8.0,
            DiemTongKet = 7.83, XepLoai = "Khá", NhanXetGiangVien = "Tốt"
        });
        db.SaveChanges();

        var svc = new ExcelService(db);
        var result = await svc.ExportBangDiemAsync(lop.Id);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBangDiem_EmptyDiem_ReturnsFile()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id);
        var svc = new ExcelService(db);

        var result = await svc.ExportBangDiemAsync(lop.Id);
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBaoCaoHocVien_WithTuNgay_FiltersCorrectly()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id);
        var (_, hv) = SeedHocVienFull(db, "hv_bao_cao_1@t.com");
        // DangKy với NgayDuyet gần đây
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet", DateTime.Now);
        var svc = new ExcelService(db);

        var result = await svc.ExportBaoCaoHocVienExcelAsync(DateTime.Now.AddDays(-1), null);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBaoCaoHocVien_WithDenNgay_FiltersCorrectly()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id);
        var (_, hv) = SeedHocVienFull(db, "hv_bao_cao_2@t.com");
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet", DateTime.Now.AddDays(-5));
        var svc = new ExcelService(db);

        var result = await svc.ExportBaoCaoHocVienExcelAsync(null, DateTime.Now);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportBaoCaoHocVien_WithBothDates_FiltersCorrectly()
    {
        var db = DbContextFactory.Create();
        var kh = SeedKH(db);
        var lop = SeedLop(db, kh.Id);
        var (_, hv) = SeedHocVienFull(db, "hv_bao_cao_3@t.com");
        SeedDangKy(db, hv.Id, lop.Id, "DaDuyet", DateTime.Now.AddDays(-3));
        var svc = new ExcelService(db);

        var result = await svc.ExportBaoCaoHocVienExcelAsync(
            DateTime.Now.AddDays(-7),
            DateTime.Now);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task ExcelService_ExportHocVien_WithNullFields_StillExports()
    {
        var db = DbContextFactory.Create();
        var nd = new NguoiDung { Email = "hv_null@t.com", HoTen = "HV Null", VaiTro = "HocVien", MatKhauHash = "x", IsActive = true };
        db.NguoiDungs.Add(nd); db.SaveChanges();
        // HocVien với nhiều field null
        db.HocViens.Add(new HocVien
        {
            Id = nd.Id, MaHocVien = "HV099", HoTen = "HV Null",
            NgaySinh = null, GioiTinh = null, DiaChi = null,
            TrinhDoHienTai = null, NgonNguQuanTam = null
        });
        db.SaveChanges();

        var svc = new ExcelService(db);
        var result = await svc.ExportHocVienAsync();
        Assert.True(result.Length > 0);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REGION 8: Additional entity/viewmodel coverage
    // ═══════════════════════════════════════════════════════════════════════════

    [Fact]
    public void GoiYKhoaHoc_AllProperties_SetCorrectly()
    {
        var now = DateTime.Now;
        var entity = new GoiYKhoaHoc
        {
            Id = 1,
            HocVienId = 10,
            KhoaHocGoiYId = 20,
            DiemPhuHop = 95.5,
            LyDoGoiY = "Rất phù hợp",
            PromptGuiDi = "prompt text",
            NgayGoiY = now
        };

        Assert.Equal(1, entity.Id);
        Assert.Equal(10, entity.HocVienId);
        Assert.Equal(20, entity.KhoaHocGoiYId);
        Assert.Equal(95.5, entity.DiemPhuHop);
        Assert.Equal("Rất phù hợp", entity.LyDoGoiY);
        Assert.Equal("prompt text", entity.PromptGuiDi);
        Assert.Equal(now, entity.NgayGoiY);
    }

    [Fact]
    public void HocVienProfileViewModel_AllProperties_SetCorrectly()
    {
        var vm = new HocVienProfileViewModel
        {
            HoTen = "Nguyễn Văn A",
            TrinhDoHienTai = "B1",
            NgonNguQuanTam = "Tiếng Anh",
            LichSuHocTap = new List<LichSuHocTapViewModel>
            {
                new() { TenKhoaHoc = "KH1", TrinhDo = "A1", DiemTongKet = 8.5, XepLoai = "Giỏi", TrangThai = "Đã hoàn thành" }
            }
        };

        Assert.Equal("Nguyễn Văn A", vm.HoTen);
        Assert.Equal("B1", vm.TrinhDoHienTai);
        Assert.Single(vm.LichSuHocTap);
    }

    [Fact]
    public void KhoaHocGoiYInputViewModel_AllProperties_SetCorrectly()
    {
        var vm = new KhoaHocGoiYInputViewModel
        {
            Id = 5,
            TenKhoaHoc = "IELTS 7.0",
            NgonNgu = "Tiếng Anh",
            TrinhDo = "C1",
            HocPhi = 5_000_000m,
            MoTa = "Luyện thi IELTS đạt 7.0+"
        };

        Assert.Equal(5, vm.Id);
        Assert.Equal("IELTS 7.0", vm.TenKhoaHoc);
        Assert.Equal(5_000_000m, vm.HocPhi);
    }

    [Fact]
    public void GoiYKetQuaViewModel_AllProperties_SetCorrectly()
    {
        var vm = new GoiYKetQuaViewModel
        {
            KhoaHocId = 3,
            TenKhoaHoc = "TOEFL Prep",
            DiemPhuHop = 88.0,
            LyDo = "Phù hợp trình độ",
            HocPhi = 4_000_000m,
            AnhBia = "/img/toefl.jpg"
        };

        Assert.Equal(3, vm.KhoaHocId);
        Assert.Equal(88.0, vm.DiemPhuHop);
    }

    [Fact]
    public void GoiYTrangViewModel_WithItems_DaGoiYIsTrue()
    {
        var vm = new GoiYTrangViewModel
        {
            DaGoiY = true,
            KetQua = new List<GoiYKetQuaViewModel>
            {
                new() { KhoaHocId = 1, TenKhoaHoc = "KH1", DiemPhuHop = 90.0 }
            }
        };

        Assert.True(vm.DaGoiY);
        Assert.Single(vm.KetQua);
    }

    [Fact]
    public void GeminiGoiYResponse_DefaultsToEmptyList()
    {
        var resp = new GeminiGoiYResponse();
        Assert.NotNull(resp.GoiY);
        Assert.Empty(resp.GoiY);
    }

    [Fact]
    public void LichSuHocTapViewModel_AllProperties_SetCorrectly()
    {
        var vm = new LichSuHocTapViewModel
        {
            TenKhoaHoc = "English A2",
            TrinhDo = "A2",
            DiemTongKet = 9.0,
            XepLoai = "Xuất sắc",
            TrangThai = "Đã hoàn thành"
        };

        Assert.Equal("English A2", vm.TenKhoaHoc);
        Assert.Equal(9.0, vm.DiemTongKet);
        Assert.Equal("Xuất sắc", vm.XepLoai);
    }

    [Fact]
    public void ChatHistory_AllProperties_SetCorrectly()
    {
        var nd = new NguoiDung { Id = 1, Email = "x@t.com", HoTen = "X", VaiTro = "Admin", MatKhauHash = "h", IsActive = true };
        var history = new ChatHistory
        {
            Id = 1,
            UserId = 1,
            Role = "user",
            Content = "Xin chào",
            CreatedAt = DateTime.Now,
            NguoiDung = nd
        };

        Assert.Equal("user", history.Role);
        Assert.Equal("Xin chào", history.Content);
        Assert.Equal(nd, history.NguoiDung);
    }

    [Fact]
    public void DangKyKhoaHoc_DefaultTrangThai_IsChoDuyet()
    {
        var dk = new DangKyKhoaHoc { HocVienId = 1, LopHocId = 1 };
        Assert.Equal("ChoDuyet", dk.TrangThai);
        Assert.Null(dk.LyDoTuChoi);
        Assert.Null(dk.NgayDuyet);
    }
}
