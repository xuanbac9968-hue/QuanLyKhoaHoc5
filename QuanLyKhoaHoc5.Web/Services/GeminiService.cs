using Newtonsoft.Json;
using QuanLyKhoaHoc5.Web.Models.ViewModels;

namespace QuanLyKhoaHoc5.Web.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration config, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<List<GoiYItemViewModel>> GetGoiYKhoaHocAsync(HocVienProfileViewModel profile, List<KhoaHocGoiYInputViewModel> danhSachKhoaHoc)
    {
        var apiKey = _config["GroqAPI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Chưa cấu hình Groq API key. Vui lòng liên hệ quản trị viên.");

        var endpoint = _config["GroqAPI:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("Chưa cấu hình Groq API endpoint.");

        var model = _config["GroqAPI:Model"] ?? "llama-3.3-70b-versatile";
        var prompt = BuildPrompt(profile, danhSachKhoaHoc);

        _logger.LogInformation("=== GROQ REQUEST ===");
        _logger.LogInformation("Endpoint: {Endpoint} | Model: {Model}", endpoint, model);
        _logger.LogInformation("Prompt ({Len} chars): {Prompt}", prompt.Length, prompt[..Math.Min(2000, prompt.Length)]);

        // Groq dùng OpenAI-compatible format
        var requestBody = new
        {
            model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 1024
        };

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            var jsonContent = JsonConvert.SerializeObject(requestBody);
            request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            response = await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Không thể kết nối đến Groq API");
            throw new InvalidOperationException("Không thể kết nối đến dịch vụ AI. Vui lòng thử lại sau.");
        }

        _logger.LogInformation("=== GROQ RESPONSE === HTTP {Status}", (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Groq API lỗi {Status}: {Body}", response.StatusCode, errBody[..Math.Min(500, errBody.Length)]);
            throw new InvalidOperationException($"Dịch vụ AI phản hồi lỗi ({(int)response.StatusCode}). Vui lòng thử lại sau.");
        }

        var rawJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Groq raw: {Raw}", rawJson[..Math.Min(500, rawJson.Length)]);

        var responseText = ExtractTextFromGroqResponse(rawJson);
        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogWarning("Groq trả về text rỗng");
            return [];
        }

        var cleanJson = CleanJsonResponse(responseText);
        _logger.LogInformation("Groq clean JSON: {Json}", cleanJson[..Math.Min(500, cleanJson.Length)]);

        try
        {
            var result = JsonConvert.DeserializeObject<GeminiGoiYResponse>(cleanJson);
            return result?.GoiY ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể parse JSON từ Groq: {Json}", cleanJson[..Math.Min(300, cleanJson.Length)]);
            return [];
        }
    }

    private string BuildPrompt(HocVienProfileViewModel profile, List<KhoaHocGoiYInputViewModel> danhSachKhoaHoc)
    {
        var hocVienJson = JsonConvert.SerializeObject(profile, Formatting.Indented);
        var khoaHocJson = JsonConvert.SerializeObject(danhSachKhoaHoc, Formatting.Indented);

        var idList = string.Join(", ", danhSachKhoaHoc.Select(k => $"{k.Id} ({k.TenKhoaHoc})"));
        var jsonMau = """{"goiY": [{"khoaHocId": 1, "tenKhoaHoc": "Tên khóa", "diemPhuHop": 85.5, "lyDo": "Lý do 1-2 câu"}]}""";

        return $"""
            Bạn là chuyên gia tư vấn giáo dục ngôn ngữ tại Trung tâm Ngoại ngữ ABC.

            Hãy phân tích hồ sơ học viên và danh sách khóa học bên dưới, sau đó đề xuất TỐI ĐA 3 khóa học phù hợp nhất.

            THÔNG TIN HỌC VIÊN:
            {hocVienJson}

            DANH SÁCH KHÓA HỌC (mỗi khóa có trường "id" là ID thực trong hệ thống):
            {khoaHocJson}

            Danh sách ID hợp lệ: {idList}

            QUAN TRỌNG: Chỉ trả về JSON thuần túy, KHÔNG có markdown, KHÔNG có code block.
            Giá trị "khoaHocId" PHẢI là một trong các ID hợp lệ ở trên.

            Trả về JSON theo đúng cấu trúc này:
            {jsonMau}

            Sắp xếp theo mức độ phù hợp giảm dần.
            """;
    }

    // Groq/OpenAI response: choices[0].message.content
    private string ExtractTextFromGroqResponse(string rawJson)
    {
        try
        {
            dynamic? obj = JsonConvert.DeserializeObject(rawJson);
            return obj?.choices[0]?.message?.content ?? "";
        }
        catch
        {
            return "";
        }
    }

    private string CleanJsonResponse(string text)
    {
        text = text.Trim();
        if (text.StartsWith("```json")) text = text[7..];
        if (text.StartsWith("```")) text = text[3..];
        if (text.EndsWith("```")) text = text[..^3];
        return text.Trim();
    }
}

public class GeminiGoiYResponse
{
    [JsonProperty("goiY")]
    public List<GoiYItemViewModel> GoiY { get; set; } = [];
}
