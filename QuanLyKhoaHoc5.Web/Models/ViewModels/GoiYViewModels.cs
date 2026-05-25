using Newtonsoft.Json;

namespace QuanLyKhoaHoc5.Web.Models.ViewModels;

public class GoiYItemViewModel
{
    [JsonProperty("khoaHocId")]
    public int KhoaHocId { get; set; }

    [JsonProperty("tenKhoaHoc")]
    public string TenKhoaHoc { get; set; } = "";

    [JsonProperty("diemPhuHop")]
    public double DiemPhuHop { get; set; }

    [JsonProperty("lyDo")]
    public string LyDo { get; set; } = "";
}

public class GoiYKetQuaViewModel
{
    public int KhoaHocId { get; set; }
    public string TenKhoaHoc { get; set; } = "";
    public double DiemPhuHop { get; set; }
    public string LyDo { get; set; } = "";
    public decimal HocPhi { get; set; }
    public string? AnhBia { get; set; }
}

public class HocVienProfileViewModel
{
    [JsonProperty("hoTen")]
    public string HoTen { get; set; } = "";

    [JsonProperty("trinhDoHienTai")]
    public string TrinhDoHienTai { get; set; } = "";

    [JsonProperty("ngonNguQuanTam")]
    public string NgonNguQuanTam { get; set; } = "";

    [JsonProperty("lichSuHocTap")]
    public List<LichSuHocTapViewModel> LichSuHocTap { get; set; } = [];
}

public class LichSuHocTapViewModel
{
    [JsonProperty("tenKhoaHoc")]
    public string TenKhoaHoc { get; set; } = "";

    [JsonProperty("trinhDo")]
    public string TrinhDo { get; set; } = "";

    [JsonProperty("diemTongKet")]
    public double? DiemTongKet { get; set; }

    [JsonProperty("xepLoai")]
    public string? XepLoai { get; set; }

    [JsonProperty("trangThai")]
    public string TrangThai { get; set; } = "";
}

public class KhoaHocGoiYInputViewModel
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("tenKhoaHoc")]
    public string TenKhoaHoc { get; set; } = "";

    [JsonProperty("ngonNgu")]
    public string NgonNgu { get; set; } = "";

    [JsonProperty("trinhDo")]
    public string TrinhDo { get; set; } = "";

    [JsonProperty("hocPhi")]
    public decimal HocPhi { get; set; }

    [JsonProperty("moTa")]
    public string MoTa { get; set; } = "";
}

public class GoiYTrangViewModel
{
    public List<GoiYKetQuaViewModel> KetQua { get; set; } = [];
    public bool DaGoiY { get; set; }
    public string? ThongBaoLoi { get; set; }
}
