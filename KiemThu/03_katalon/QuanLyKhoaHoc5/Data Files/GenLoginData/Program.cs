using ClosedXML.Excel;

// TestData_Login.xlsx — dùng cho Katalon Data-Driven
// Cột: TCId | Email | Password | ExpectedResult | ExpectedUrl | MoTa
var OUT = @"D:\QuanLyKhoaHoc5\KiemThu\03_katalon\QuanLyKhoaHoc5\Data Files\TestData_Login.xlsx";

using var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("LoginData");

// Header
string[] headers = { "TCId", "Email", "Password", "ExpectedResult", "ExpectedUrl", "MoTa" };
for (int i = 0; i < headers.Length; i++)
{
    var c = ws.Cell(1, i + 1);
    c.Value = headers[i];
    c.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F3864");
    c.Style.Font.FontColor = XLColor.White;
    c.Style.Font.Bold = true;
    c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    c.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
}
ws.Row(1).Height = 22;

// Data rows: TCId, Email, Password, ExpectedResult, ExpectedUrl, MoTa
var data = new string[,]
{
    { "TC-001", "admin@nnl.com",           "Admin@123",  "LOGIN_SUCCESS", "/Admin",               "Admin đăng nhập thành công" },
    { "TC-002", "gv01@nnl.com",            "Gv@123",     "LOGIN_SUCCESS", "/GiangVien/Dashboard", "GiangVien đăng nhập thành công" },
    { "TC-003", "hv01@nnl.com",            "Hv@123",     "LOGIN_SUCCESS", "/HocVien/Dashboard",   "HocVien đăng nhập thành công" },
    { "TC-004", "admin@nnl.com",           "wrong123",   "LOGIN_FAIL",    "/Account/Login",        "Sai mật khẩu → thông báo lỗi" },
    { "TC-005", "khongtontai@abc.com",     "abc123",     "LOGIN_FAIL",    "/Account/Login",        "Email không tồn tại trong DB" },
    { "TC-006", "locked@nnl.com",          "Admin@123",  "ACCOUNT_LOCKED","/Account/Login",        "TK bị khóa IsActive=false" },
    { "TC-007", "",                         "abc123",     "VALIDATION_ERR","/Account/Login",        "Email rỗng → form validation" },
    { "TC-008", "hv01@nnl.com",            "Hv@123",     "LOGIN_SUCCESS", "/HocVien/Dashboard",   "Ghi nhớ đăng nhập 30 ngày" },
    { "TC-009", "admin@nnl.com",           "Admin@123",  "LOGOUT_SUCCESS","/Account/Login",        "Đăng xuất thành công" },
    { "TC-010", "admin@nnl.com",           "Admin@123",  "PWD_CHANGED",   "/Account/Login",        "Đổi mật khẩu thành công" },
};

for (int r = 0; r < data.GetLength(0); r++)
{
    var bg = (r % 2 == 0) ? XLColor.White : XLColor.FromHtml("#EEF3FA");
    for (int c = 0; c < data.GetLength(1); c++)
    {
        var cell = ws.Cell(r + 2, c + 1);
        cell.Value = data[r, c];
        cell.Style.Fill.BackgroundColor = bg;
        cell.Style.Font.FontSize = 10;
        cell.Style.Alignment.WrapText = false;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#BDD7EE");
        cell.Style.Alignment.Horizontal = (c == 0 || c == 3)
            ? XLAlignmentHorizontalValues.Center
            : XLAlignmentHorizontalValues.Left;

        // Màu ExpectedResult
        if (c == 3)
        {
            var v = data[r, c];
            if (v == "LOGIN_SUCCESS" || v == "LOGOUT_SUCCESS" || v == "PWD_CHANGED")
                cell.Style.Font.FontColor = XLColor.FromHtml("#375623");
            else if (v == "LOGIN_FAIL" || v == "ACCOUNT_LOCKED")
                cell.Style.Font.FontColor = XLColor.FromHtml("#C00000");
            else
                cell.Style.Font.FontColor = XLColor.FromHtml("#9C5700");
            cell.Style.Font.Bold = true;
        }
    }
}

int[] widths = { 10, 28, 16, 18, 26, 34 };
for (int i = 0; i < widths.Length; i++) ws.Column(i + 1).Width = widths[i];

ws.SheetView.FreezeRows(1);
ws.RangeUsed()!.SetAutoFilter();

wb.SaveAs(OUT);
Console.WriteLine($"OK  {OUT}");
Console.WriteLine($"    {data.GetLength(0)} hàng dữ liệu, 6 cột (TCId|Email|Password|ExpectedResult|ExpectedUrl|MoTa)");
