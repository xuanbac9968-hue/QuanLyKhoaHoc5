using ClosedXML.Excel;

// ============================================================
// GenTestData – Tạo 6 file Excel dữ liệu kiểm thử
// Đầu ra: D:\QuanLyKhoaHoc5\KiemThu\TestData\TC0X_*.xlsx
//
// Hệ thống: QuanLyKhoaHoc5 – http://localhost:5125
// Seed accounts:
//   admin@nnl.com / Admin@123  (Admin, NguoiDungId=1)
//   gv01@nnl.com  / Gv@123    (GiangVien, Id=2, Hương - Tiếng Anh)
//   gv02@nnl.com  / Gv@123    (GiangVien, Id=3, Nam   - Tiếng Nhật)
//   gv03@nnl.com  / Gv@123    (GiangVien, Id=4, Đức   - Tiếng Hàn)
//   hv01@nnl.com  / Hv@123    (HocVien – DaDuyet KH2=B1, KH3=IELTS)
//   hv02@nnl.com  / Hv@123    (HocVien – DaDuyet KH4=N5, KH6=Hàn)
//   hv03@nnl.com  / Hv@123    (HocVien – DaDuyet KH6=Hàn; ChoDuyet KH5=N4)
// KhoaHoc IDs: 1=A1, 2=B1, 3=IELTS, 4=N5, 5=N4, 6=Hàn Sơ Cấp
// ============================================================

const string OUT_DIR = @"D:\QuanLyKhoaHoc5\KiemThu\TestData";
Directory.CreateDirectory(OUT_DIR);

// ─── Styles ──────────────────────────────────────────────────────────────────
var cHeader = XLColor.FromHtml("#1F3864");
var cFontH  = XLColor.White;
var cEven   = XLColor.FromHtml("#EEF3FA");
var cOdd    = XLColor.White;
var cBorder = XLColor.FromHtml("#BDD7EE");
var cPass   = XLColor.FromHtml("#375623");
var cFail   = XLColor.FromHtml("#C00000");
var cGray   = XLColor.FromHtml("#595959");

IXLWorksheet MakeSheet(XLWorkbook wb, string name, string[] headers, int[] colWidths)
{
    var ws = wb.Worksheets.Add(name);
    ws.TabColor = cHeader;
    for (int c = 0; c < headers.Length; c++)
    {
        var cell = ws.Cell(1, c + 1);
        cell.Value = headers[c];
        cell.Style.Fill.BackgroundColor = cHeader;
        cell.Style.Font.FontColor       = cFontH;
        cell.Style.Font.Bold            = true;
        cell.Style.Font.FontSize        = 11;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText   = true;
        cell.Style.Border.OutsideBorder      = XLBorderStyleValues.Medium;
        cell.Style.Border.OutsideBorderColor = cBorder;
    }
    ws.Row(1).Height = 26;
    for (int c = 0; c < colWidths.Length; c++) ws.Column(c + 1).Width = colWidths[c];
    ws.SheetView.FreezeRows(1);
    return ws;
}

void WriteRow(IXLWorksheet ws, int rowIdx, string[] values, string ketQuaMongDoiCol = "ThanhCong")
{
    var bg = (rowIdx % 2 == 0) ? cEven : cOdd;
    for (int c = 0; c < values.Length; c++)
    {
        var cell = ws.Cell(rowIdx + 1, c + 1); // +1 because row 1 is header
        // Try numeric
        if (long.TryParse(values[c], out long numL))
            cell.Value = numL;
        else
            cell.Value = values[c];

        cell.Style.Fill.BackgroundColor = bg;
        cell.Style.Font.FontSize        = 10;
        cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        cell.Style.Alignment.WrapText   = false;
        cell.Style.Border.OutsideBorder      = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = cBorder;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    }
    // Col 1 (STT) center
    ws.Cell(rowIdx + 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    // Last col with data = KetQuaMongDoi → color
    int lastCol = values.Length;
    var kqCell = ws.Cell(rowIdx + 1, lastCol);
    kqCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    kqCell.Style.Font.Bold = true;
    string kq = values[lastCol - 1];
    if (kq == "ThanhCong") kqCell.Style.Font.FontColor = cPass;
    else if (kq == "ThatBai") kqCell.Style.Font.FontColor = cFail;
    else kqCell.Style.Font.FontColor = cGray;
}

void AddAutoFilter(IXLWorksheet ws, int totalRows, int totalCols)
{
    ws.Range(1, 1, totalRows + 1, totalCols).SetAutoFilter();
}

// ══════════════════════════════════════════════════════════════════════════════
// TC01_DangNhap.xlsx
// Columns: STT | MoTa | Email | MatKhau | GhiNho | KetQuaMongDoi | KetQua
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","Email","Mật khẩu","GhiNho","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   40,     26,      16,          9,        16,            10  };

    // (STT, MoTa, Email, MatKhau, GhiNho, KetQuaMongDoi)
    string[][] data =
    {
        new[]{ "1","Admin đăng nhập thành công",           "admin@nnl.com",        "Admin@123",     "false","ThanhCong" },
        new[]{ "2","GiangVien đăng nhập thành công",       "gv01@nnl.com",         "Gv@123",        "false","ThanhCong" },
        new[]{ "3","HocVien đăng nhập thành công",         "hv01@nnl.com",         "Hv@123",        "false","ThanhCong" },
        new[]{ "4","Đăng nhập sai mật khẩu",               "admin@nnl.com",        "WrongPass@123", "false","ThatBai"   },
        new[]{ "5","Email không tồn tại trong DB",         "khongtontai@test.com", "abc123",        "false","ThatBai"   },
        new[]{ "6","Ghi nhớ đăng nhập (Remember Me)",      "hv01@nnl.com",         "Hv@123",        "true", "ThanhCong" },
        new[]{ "7","HocVien 2 đăng nhập thành công",       "hv02@nnl.com",         "Hv@123",        "false","ThanhCong" },
        new[]{ "8","GiangVien 2 đăng nhập thành công",     "gv02@nnl.com",         "Gv@123",        "false","ThanhCong" },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "DangNhap", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC01_DangNhap.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

// ══════════════════════════════════════════════════════════════════════════════
// TC02_DoiMatKhau.xlsx
// Columns: STT | MoTa | EmailLogin | MatKhauLogin | MatKhauCu | MatKhauMoi | XacNhanMatKhau | KetQuaMongDoi | KetQua
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","EmailLogin","MatKhauLogin","MatKhauCu","MatKhauMoi","XacNhanMatKhau","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   40,      22,           14,            14,          14,            16,              16,            10   };

    string[][] data =
    {
        // ThatBai cases first (tránh ảnh hưởng mật khẩu)
        new[]{ "1","Sai mật khẩu cũ",                       "admin@nnl.com",  "Admin@123",   "WrongOld@123",  "NewPass@456",  "NewPass@456",  "ThatBai"   },
        new[]{ "2","Mật khẩu mới không khớp xác nhận",      "admin@nnl.com",  "Admin@123",   "Admin@123",     "NewPass@456",  "DiffPass@789", "ThatBai"   },
        new[]{ "3","Mật khẩu mới quá yếu (< 6 ký tự)",     "admin@nnl.com",  "Admin@123",   "Admin@123",     "abc",          "abc",          "ThatBai"   },
        // ThanhCong: đổi mật khẩu hv01, sau đó đổi lại
        new[]{ "4","Đổi mật khẩu hv01 thành công",          "hv01@nnl.com",   "Hv@123",      "Hv@123",        "NewHv@456789", "NewHv@456789", "ThanhCong" },
        new[]{ "5","Khôi phục mật khẩu hv01 về ban đầu",    "hv01@nnl.com",   "NewHv@456789","NewHv@456789",  "Hv@123",       "Hv@123",       "ThanhCong" },
        // ThatBai: sai email login (script sẽ throw exception)
        new[]{ "6","Sai mật khẩu login (không vào được)",   "hv02@nnl.com",   "SaiPass@123", "Hv@123",        "NewHv@456",    "NewHv@456",    "ThatBai"   },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "DoiMatKhau", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC02_DoiMatKhau.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

// ══════════════════════════════════════════════════════════════════════════════
// TC03_TaoTaiKhoan.xlsx
// Columns: STT | MoTa | HoTen | Email | VaiTro | MatKhau | XacNhanMatKhau | KetQuaMongDoi | KetQua
// VaiTro values (select option value): HocVien | GiangVien | Admin
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","HoTen","Email","VaiTro","MatKhau","XacNhanMatKhau","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   42,      22,     28,      12,      14,         16,               16,           10   };

    string[][] data =
    {
        new[]{ "1","Tạo tài khoản HocVien mới (hợp lệ)",       "Học Viên Mới 01",      "hvmoi01@test.com",   "HocVien",   "Hv@Abc123",  "Hv@Abc123",  "ThanhCong" },
        new[]{ "2","Tạo tài khoản GiangVien mới (hợp lệ)",     "Giảng Viên Mới 01",    "gvmoi01@test.com",   "GiangVien", "Gv@Abc123",  "Gv@Abc123",  "ThanhCong" },
        new[]{ "3","Email đã tồn tại trong hệ thống",           "Trùng Admin Email",    "admin@nnl.com",      "HocVien",   "Abc@12345",  "Abc@12345",  "ThatBai"   },
        new[]{ "4","Mật khẩu xác nhận không khớp",             "Test Không Khớp",      "nomatch@test.com",   "HocVien",   "Abc@12345",  "Diff@98765", "ThatBai"   },
        new[]{ "5","Mật khẩu quá yếu (không đủ độ phức tạp)",  "Test Mật Khẩu Yếu",   "weakpw@test.com",    "HocVien",   "abc",        "abc",        "ThatBai"   },
        new[]{ "6","Tạo tài khoản Admin mới (hợp lệ)",         "Admin Phụ 01",         "admin2@test.com",    "Admin",     "Ad@Abc123",  "Ad@Abc123",  "ThanhCong" },
        new[]{ "7","HoTen để trống (validation)",               "",                     "emptyname@test.com", "HocVien",   "Abc@12345",  "Abc@12345",  "ThatBai"   },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "TaoTaiKhoan", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC03_TaoTaiKhoan.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

// ══════════════════════════════════════════════════════════════════════════════
// TC04_ThemKhoaHoc.xlsx
// Columns: STT | MoTa | TenKhoaHoc | MoTaKhoaHoc | NgonNgu | TrinhDo | HocPhi |
//          ThoiLuong | SoBuoiMoiTuan | ThoiGianMoiBuoi | TrangThai | KetQuaMongDoi | KetQua
// NgonNgu select values: "Tiếng Anh" | "Tiếng Nhật"
// TrinhDo select values: Sơ cấp/Trung cấp/Cao cấp/IELTS/TOEIC | N5/N4/N3/N2/N1
// TrangThai select values: DangMo | DaDong | TamDung
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","TenKhoaHoc","MoTaKhoaHoc","NgonNgu","TrinhDo","HocPhi",
                         "ThoiLuong","SoBuoiMoiTuan","ThoiGianMoiBuoi","TrangThai","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   42,      28,            32,            12,       12,       12,
                            10,        14,             15,              12,         16,           10       };

    // TenKhoaHoc(200 chars max), TrinhDo required, HocPhi>=0
    // ThatBai case: TrinhDo empty → Required validation fails
    string longName = "Khóa Học Tên Quá Dài " + new string('X', 190); // > 200 chars

    string[][] data =
    {
        // ThanhCong cases
        new[]{ "1","Tạo KH TOEIC hợp lệ",                      "Tiếng Anh TOEIC Luyện Thi",    "Luyện thi TOEIC mục tiêu 600-900 điểm", "Tiếng Anh",  "TOEIC",  "4500000","48","3","90","DangMo","ThanhCong" },
        new[]{ "2","Tạo KH Nhật N3 hợp lệ",                    "Tiếng Nhật N3 Nâng Cao",        "Nâng cao từ N4 lên N3 JLPT",            "Tiếng Nhật", "N3",     "5500000","60","2","120","DangMo","ThanhCong" },
        new[]{ "3","Tạo KH trạng thái TamDung",                 "Tiếng Anh Cao Cấp Test",        "Khóa tiếng Anh cao cấp",                "Tiếng Anh",  "Cao cấp","5000000","50","3","90","TamDung","ThanhCong" },
        // ThatBai: TrinhDo bỏ trống → Required validation
        new[]{ "4","TrinhDo bỏ trống – validation Required",    "Khóa Học Thiếu Trình Độ",       "Mô tả test case thiếu trình độ",        "Tiếng Anh",  "",       "3000000","40","3","90","DangMo","ThatBai"   },
        // ThatBai: TenKhoaHoc > 200 chars → MaxLength(200) validation
        new[]{ "5","TenKhoaHoc vượt 200 ký tự – validation",   longName,                        "Mô tả test case",                       "Tiếng Anh",  "Sơ cấp","2000000","30","2","90","DangMo","ThatBai"   },
        // ThanhCong: học phí = 0 (miễn phí)
        new[]{ "6","KH miễn phí (HocPhi=0)",                    "Tiếng Anh Miễn Phí Test",       "Khóa học thử nghiệm miễn phí",          "Tiếng Anh",  "Sơ cấp","0",      "20","2","60","DangMo","ThanhCong" },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "ThemKhoaHoc", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC04_ThemKhoaHoc.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

// ══════════════════════════════════════════════════════════════════════════════
// TC05_TaoThanhToan.xlsx
// Columns: STT | MoTa | EmailHocVien | MatKhauHocVien | KhoaHocId | PhuongThuc | GhiChu | KetQuaMongDoi | KetQua
// PhuongThuc: TienMat | ChuyenKhoan
// Điều kiện: HocVien phải DaDuyet vào KhoaHoc tương ứng
//   hv01@nnl.com → DaDuyet KH2(B1), KH3(IELTS)
//   hv02@nnl.com → DaDuyet KH4(N5), KH6(Hàn)
//   hv03@nnl.com → DaDuyet KH6(Hàn); ChoDuyet KH5(N4)
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","EmailHocVien","MatKhauHocVien","KhoaHocId","PhuongThuc","GhiChu","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   45,     22,             14,              12,          12,          30,       16,            10    };

    string[][] data =
    {
        // ThanhCong: hv01 thanh toán KH B1 (DaDuyet)
        new[]{ "1","hv01 TT Tiền mặt – KH B1 (ThanhCong)",     "hv01@nnl.com","Hv@123","2","TienMat",    "Nộp học phí kỳ 1 Tiếng Anh B1",       "ThanhCong" },
        // ThanhCong: hv02 thanh toán KH N5 (DaDuyet)
        new[]{ "2","hv02 TT Chuyển khoản – KH N5 (ThanhCong)", "hv02@nnl.com","Hv@123","4","ChuyenKhoan","Chuyển khoản học phí Nhật N5",         "ThanhCong" },
        // ThanhCong: hv03 thanh toán KH Hàn (DaDuyet)
        new[]{ "3","hv03 TT Tiền mặt – KH Hàn (ThanhCong)",    "hv03@nnl.com","Hv@123","6","TienMat",    "Thanh toán học phí Hàn Sơ Cấp",        "ThanhCong" },
        // ThatBai: KhoaHocId không tồn tại → redirect ra ngoài form
        new[]{ "4","hv01 – KhoaHocId không tồn tại (ThatBai)", "hv01@nnl.com","Hv@123","99","TienMat",   "",                                     "ThatBai"   },
        // ThatBai: hv03 + KH5(N4) → chỉ ChoDuyet, chưa DaDuyet
        new[]{ "5","hv03 – KH N4 chỉ ChoDuyet (ThatBai)",      "hv03@nnl.com","Hv@123","5","TienMat",    "",                                     "ThatBai"   },
        // ThatBai: hv01 + KH4 (N5) → hv01 không có DangKy KH4
        new[]{ "6","hv01 – KH N5 không có đăng ký (ThatBai)",  "hv01@nnl.com","Hv@123","4","TienMat",    "",                                     "ThatBai"   },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "TaoThanhToan", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC05_TaoThanhToan.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

// ══════════════════════════════════════════════════════════════════════════════
// TC06_PhanCong.xlsx
// Columns: STT | MoTa | KhoaHocId | GiangVienId | GhiChu | KetQuaMongDoi | KetQua
// GiangVienId: 2=Hương(Anh) | 3=Nam(Nhật) | 4=Đức(Hàn) | 0=Bỏ phân công
// KhoaHocId: 1=A1 | 2=B1 | 3=IELTS | 4=N5 | 5=N4 | 6=Hàn
// ══════════════════════════════════════════════════════════════════════════════
{
    string[] headers = { "STT","Mô tả","KhoaHocId","GiangVienId","GhiChu","KetQuaMongDoi","KetQua" };
    int[]    widths  = {   5,   52,      12,           12,           36,      16,             10   };

    string[][] data =
    {
        // Phân công hợp lệ (ThanhCong)
        new[]{ "1","KH A1 (1) → GV Hương (2) – Tiếng Anh",    "1","2","Phân công GV tiếng Anh vào khóa A1",    "ThanhCong" },
        new[]{ "2","KH B1 (2) → GV Hương (2) – Tiếng Anh",    "2","2","Phân công GV tiếng Anh vào khóa B1",    "ThanhCong" },
        new[]{ "3","KH IELTS (3) → GV Hương (2) – Tiếng Anh", "3","2","Phân công GV tiếng Anh vào khóa IELTS", "ThanhCong" },
        new[]{ "4","KH N5 (4) → GV Nam (3) – Tiếng Nhật",     "4","3","Phân công GV tiếng Nhật vào khóa N5",   "ThanhCong" },
        new[]{ "5","KH N4 (5) → GV Nam (3) – Tiếng Nhật",     "5","3","Phân công GV tiếng Nhật vào khóa N4",   "ThanhCong" },
        new[]{ "6","KH Hàn (6) → GV Đức (4) – Tiếng Hàn",    "6","4","Phân công GV tiếng Hàn vào khóa SC",    "ThanhCong" },
        // Thay đổi phân công (ThanhCong)
        new[]{ "7","KH A1 (1) → Đổi sang GV Nam (3)",         "1","3","Đổi GV Hương sang Nam cho KH A1",       "ThanhCong" },
        // Bỏ phân công nếu có option value=0 (ThanhCong hoặc ThatBai tùy ứng dụng)
        new[]{ "8","KH A1 (1) → Bỏ phân công (GV=0)",         "1","0","Gỡ phân công khỏi KH A1",               "ThanhCong" },
    };

    using var wb = new XLWorkbook();
    var ws = MakeSheet(wb, "PhanCongGV", headers, widths);
    for (int i = 0; i < data.Length; i++)
        WriteRow(ws, i + 1, data[i]);
    AddAutoFilter(ws, data.Length, headers.Length);
    string path = Path.Combine(OUT_DIR, "TC06_PhanCong.xlsx");
    wb.SaveAs(path);
    Console.WriteLine($"✓  {path}  ({data.Length} rows)");
}

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine($"  Hoàn thành! 6 file Excel đã tạo tại: {OUT_DIR}");
Console.WriteLine("═══════════════════════════════════════════════════════════");
Console.WriteLine("  TC01_DangNhap.xlsx       – 8  test cases");
Console.WriteLine("  TC02_DoiMatKhau.xlsx     – 6  test cases");
Console.WriteLine("  TC03_TaoTaiKhoan.xlsx    – 7  test cases");
Console.WriteLine("  TC04_ThemKhoaHoc.xlsx    – 6  test cases");
Console.WriteLine("  TC05_TaoThanhToan.xlsx   – 6  test cases");
Console.WriteLine("  TC06_PhanCong.xlsx        – 8  test cases");
Console.WriteLine("                      Tổng – 41 test cases dữ liệu");
