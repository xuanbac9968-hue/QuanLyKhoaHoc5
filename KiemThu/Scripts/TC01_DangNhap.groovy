// ============================================================
// TC01_DangNhap.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Đăng nhập hệ thống
// URL       : http://localhost:5125/Account/Login
// Excel     : KiemThu\TestData\TC01_DangNhap.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0) STT | B(1) MoTa | C(2) Email | D(3) MatKhau
//   E(4) GhiNho (true/false) | F(5) KetQuaMongDoi (ThanhCong/ThatBai)
//   G(6) KetQua  ← script tự ghi Pass/Fail
//
// Selectors xác nhận từ Views/Account/Login.cshtml:
//   - Email   : asp-for="Email"          → id="Email"
//   - Password: explicit id="pwInput"    → id="pwInput"
//   - RememberMe: asp-for="GhiNho"      → id="GhiNho"
//   - Submit  : button[type="submit"]
//
// Tài khoản seed (SeedData.cs):
//   Admin   : admin@nnl.com  / Admin@123
//   GiangVien: gv01@nnl.com / Gv@123
//   HocVien : hv01@nnl.com  / Hv@123
// ============================================================
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.testobject.TestObject
import com.kms.katalon.core.testobject.ConditionType
import com.kms.katalon.core.testobject.SelectorMethod
import com.kms.katalon.core.util.KeywordUtil
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook

// ─── CONSTANTS ───────────────────────────────────────────────────────────────
final String BASE_URL   = 'http://localhost:5125'
final String EXCEL_PATH = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC01_DangNhap.xlsx'
final int    TIMEOUT    = 10

// ─── HELPER: Tạo TestObject từ CSS selector ──────────────────────────────────
def makeBy = { String css ->
    TestObject o = new TestObject('css_el')
    o.addProperty('css', ConditionType.EQUALS, css)
    o.setSelectorMethod(SelectorMethod.CSS)
    return o
}

// ─── HELPER: Đọc giá trị ô Excel an toàn ────────────────────────────────────
def cellStr = { Cell c ->
    if (c == null) return ''
    switch (c.getCellType()) {
        case CellType.STRING:  return c.getStringCellValue().trim()
        case CellType.NUMERIC: return String.valueOf((long) c.getNumericCellValue())
        case CellType.BOOLEAN: return String.valueOf(c.getBooleanCellValue())
        default: return ''
    }
}

// ─── EXCEL SETUP ─────────────────────────────────────────────────────────────
File excelFile = new File(EXCEL_PATH)
FileInputStream fis = new FileInputStream(excelFile)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

// Tìm hoặc tạo cột KetQua
Row headerRow = sheet.getRow(0)
int colKetQua = -1
for (int c = 0; c < headerRow.getLastCellNum(); c++) {
    if (cellStr(headerRow.getCell(c)).equalsIgnoreCase('KetQua')) {
        colKetQua = c; break
    }
}
if (colKetQua == -1) {
    colKetQua = headerRow.getLastCellNum()
    headerRow.createCell(colKetQua).setCellValue('KetQua')
}

// ─── MỞ TRÌNH DUYỆT ──────────────────────────────────────────────────────────
WebUI.openBrowser('')
WebUI.setViewPortSize(1366, 768)
int pass = 0, fail = 0

// ─── TEST LOOP ────────────────────────────────────────────────────────────────
for (int i = 1; i <= sheet.getLastRowNum(); i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    // Cột: 0=STT, 1=MoTa, 2=Email, 3=MatKhau, 4=GhiNho, 5=KetQuaMongDoi
    String email         = cellStr(row.getCell(2))
    String matKhau       = cellStr(row.getCell(3))
    String ghiNho        = cellStr(row.getCell(4))
    String ketQuaMongDoi = cellStr(row.getCell(5))
    if (email.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Mở trang đăng nhập
        WebUI.navigateToUrl("${BASE_URL}/Account/Login")
        WebUI.waitForElementVisible(makeBy('#Email'), TIMEOUT)

        // 2. Nhập Email  → id="Email"  (asp-for="Email")
        WebUI.clearText(makeBy('#Email'))
        WebUI.setText(makeBy('#Email'), email)

        // 3. Nhập mật khẩu  → id="pwInput"  (explicit id trong HTML)
        WebUI.waitForElementVisible(makeBy('#pwInput'), TIMEOUT)
        WebUI.clearText(makeBy('#pwInput'))
        WebUI.setText(makeBy('#pwInput'), matKhau)

        // 4. Checkbox "Ghi nhớ đăng nhập"  → id="GhiNho"
        WebUI.uncheckCheckbox(makeBy('#GhiNho'))
        if ('true'.equalsIgnoreCase(ghiNho)) {
            WebUI.checkCheckbox(makeBy('#GhiNho'))
        }

        // 5. Click nút Đăng nhập
        WebUI.waitForElementVisible(makeBy('button[type="submit"]'), TIMEOUT)
        WebUI.click(makeBy('button[type="submit"]'))
        WebUI.waitForPageLoad(TIMEOUT)

        // 6. Kiểm tra kết quả
        String currentUrl = WebUI.getUrl()
        if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
            // Thành công: redirect khỏi /Account/Login
            ok = !currentUrl.toLowerCase().contains('/account/login')
            if (!ok) note = 'Vẫn còn ở trang Login sau khi submit'
        } else {
            // Thất bại: vẫn ở trang login, hiển thị .alert-danger
            ok = WebUI.waitForElementPresent(makeBy('.alert-danger'), 5)
            if (!ok) note = 'Không tìm thấy thông báo lỗi .alert-danger'
        }
    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
    }

    // 7. Ghi kết quả vào cột KetQua
    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: [${email}] → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU FILE & ĐÓNG TRÌNH DUYỆT ────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC01 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
