// ============================================================
// TC02_DoiMatKhau.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Đổi mật khẩu
// URL       : http://localhost:5125/Account/ChangePassword
// Excel     : KiemThu\TestData\TC02_DoiMatKhau.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0) STT | B(1) MoTa
//   C(2) EmailLogin | D(3) MatKhauLogin   ← đăng nhập trước
//   E(4) MatKhauCu  | F(5) MatKhauMoi | G(6) XacNhanMatKhau
//   H(7) KetQuaMongDoi (ThanhCong/ThatBai)
//   I(8) KetQua  ← script tự ghi
//
// Selectors xác nhận từ Views/Account/ChangePassword.cshtml:
//   - MatKhauCu      : asp-for="MatKhauCu"       → id="MatKhauCu"
//   - MatKhauMoi     : asp-for="MatKhauMoi"       → id="MatKhauMoi"
//   - XacNhanMatKhau : asp-for="XacNhanMatKhau"   → id="XacNhanMatKhau"
//   - Submit         : button[type="submit"]
//
// Thành công → redirect sang /Profile/Index → layout hiển thị .alert-success
// Thất bại   → ở lại /Account/ChangePassword → .alert-danger
//
// LƯU Ý: Mỗi row đăng nhập riêng. Nếu row ThanhCong đổi MK thành công,
//        mật khẩu account đó thay đổi → thiết kế test data cho phù hợp.
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
final String EXCEL_PATH = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC02_DoiMatKhau.xlsx'
final int    TIMEOUT    = 10

// ─── HELPERS ─────────────────────────────────────────────────────────────────
def makeBy = { String css ->
    TestObject o = new TestObject('css_el')
    o.addProperty('css', ConditionType.EQUALS, css)
    o.setSelectorMethod(SelectorMethod.CSS)
    return o
}

def cellStr = { Cell c ->
    if (c == null) return ''
    switch (c.getCellType()) {
        case CellType.STRING:  return c.getStringCellValue().trim()
        case CellType.NUMERIC: return String.valueOf((long) c.getNumericCellValue())
        case CellType.BOOLEAN: return String.valueOf(c.getBooleanCellValue())
        default: return ''
    }
}

// Đăng nhập với email + matKhau, trả về true nếu thành công
def doLogin = { String email, String pw ->
    WebUI.navigateToUrl("${BASE_URL}/Account/Login")
    WebUI.waitForElementVisible(makeBy('#Email'), TIMEOUT)
    WebUI.clearText(makeBy('#Email'))
    WebUI.setText(makeBy('#Email'), email)
    WebUI.waitForElementVisible(makeBy('#pwInput'), TIMEOUT)
    WebUI.clearText(makeBy('#pwInput'))
    WebUI.setText(makeBy('#pwInput'), pw)
    WebUI.click(makeBy('button[type="submit"]'))
    WebUI.waitForPageLoad(TIMEOUT)
    return !WebUI.getUrl().toLowerCase().contains('/account/login')
}

// ─── EXCEL SETUP ─────────────────────────────────────────────────────────────
FileInputStream fis = new FileInputStream(new File(EXCEL_PATH))
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

Row headerRow = sheet.getRow(0)
int colKetQua = -1
for (int c = 0; c < headerRow.getLastCellNum(); c++) {
    if (cellStr(headerRow.getCell(c)).equalsIgnoreCase('KetQua')) { colKetQua = c; break }
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

    // Cột: 0=STT, 1=MoTa, 2=EmailLogin, 3=MatKhauLogin,
    //       4=MatKhauCu, 5=MatKhauMoi, 6=XacNhanMatKhau, 7=KetQuaMongDoi
    String emailLogin    = cellStr(row.getCell(2))
    String mkLogin       = cellStr(row.getCell(3))
    String matKhauCu     = cellStr(row.getCell(4))
    String matKhauMoi    = cellStr(row.getCell(5))
    String xacNhan       = cellStr(row.getCell(6))
    String ketQuaMongDoi = cellStr(row.getCell(7))
    if (emailLogin.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Đăng nhập
        boolean loggedIn = doLogin(emailLogin, mkLogin)
        if (!loggedIn) {
            note = "Không đăng nhập được với ${emailLogin}"
            throw new Exception(note)
        }

        // 2. Điều hướng tới trang đổi mật khẩu
        WebUI.navigateToUrl("${BASE_URL}/Account/ChangePassword")
        WebUI.waitForElementVisible(makeBy('#MatKhauCu'), TIMEOUT)

        // 3. Nhập mật khẩu hiện tại  → id="MatKhauCu"
        WebUI.clearText(makeBy('#MatKhauCu'))
        WebUI.setText(makeBy('#MatKhauCu'), matKhauCu)

        // 4. Nhập mật khẩu mới  → id="MatKhauMoi"
        WebUI.waitForElementVisible(makeBy('#MatKhauMoi'), TIMEOUT)
        WebUI.clearText(makeBy('#MatKhauMoi'))
        WebUI.setText(makeBy('#MatKhauMoi'), matKhauMoi)

        // 5. Xác nhận mật khẩu mới  → id="XacNhanMatKhau"
        WebUI.waitForElementVisible(makeBy('#XacNhanMatKhau'), TIMEOUT)
        WebUI.clearText(makeBy('#XacNhanMatKhau'))
        WebUI.setText(makeBy('#XacNhanMatKhau'), xacNhan)

        // 6. Click nút Xác nhận đổi mật khẩu
        WebUI.waitForElementVisible(makeBy('button[type="submit"]'), TIMEOUT)
        WebUI.click(makeBy('button[type="submit"]'))
        WebUI.waitForPageLoad(TIMEOUT)

        // 7. Kiểm tra kết quả
        String currentUrl = WebUI.getUrl()
        if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
            // Thành công: redirect sang /Profile/Index → layout hiển thị .alert-success
            boolean redirected = currentUrl.toLowerCase().contains('/profile')
            boolean hasSuccess = WebUI.waitForElementPresent(makeBy('.alert-success'), 5)
            ok = redirected && hasSuccess
            if (!ok) note = "Redirect: ${redirected}, AlertSuccess: ${hasSuccess}"
        } else {
            // Thất bại: ở lại ChangePassword, hiển thị .alert-danger hoặc .text-danger
            boolean hasError = WebUI.waitForElementPresent(makeBy('.alert-danger'), 5)
            if (!hasError) hasError = WebUI.waitForElementPresent(makeBy('.text-danger'), 3)
            ok = hasError
            if (!ok) note = 'Không tìm thấy thông báo lỗi'
        }

        // 8. Đăng xuất sau mỗi row
        WebUI.navigateToUrl("${BASE_URL}/Account/Logout")
        WebUI.waitForPageLoad(TIMEOUT)

    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
        // Cố đăng xuất để sẵn sàng cho row tiếp theo
        try { WebUI.navigateToUrl("${BASE_URL}/Account/Logout") } catch (Exception ignored) {}
    }

    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: [${emailLogin}] → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU & ĐÓNG ──────────────────────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC02 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
