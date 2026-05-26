// ============================================================
// TC06_PhanCongGiangVien.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Phân công giảng viên vào khóa học (Admin)
// URL       : http://localhost:5125/Admin/PhanCong
// Excel     : KiemThu\TestData\TC06_PhanCong.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0) STT | B(1) MoTa
//   C(2) KhoaHocId   (ID khóa học trong DB)
//   D(3) GiangVienId (ID giảng viên trong DB, "0" = bỏ phân công)
//   E(4) GhiChu      (optional)
//   F(5) KetQuaMongDoi (ThanhCong / ThatBai)
//   G(6) KetQua  ← script tự ghi
//
// Dữ liệu ID từ seed (thứ tự tạo):
//   KhoaHoc IDs: 1=Anh A1, 2=Anh B1, 3=IELTS, 4=Nhật N5, 5=Nhật N4, 6=Hàn SC
//   GiangVien IDs: 2=Nguyễn Thị Hương (Anh), 3=Trần Thanh Nam (Nhật), 4=Lê Minh Đức (Hàn)
//   GiangVienId=0 → bỏ phân công (Bỏ phân công)
//
// Selectors xác nhận từ Views/Admin/PhanCong.cshtml:
//   Mỗi khóa học có 1 form riêng chứa:
//     - input[name="KhoaHocId"] (hidden, value = ID khóa)
//     - select[name="GiangVienId"]
//     - input[name="GhiChu"]
//     - button[type="submit"]
//   Dùng XPath để tìm theo KhoaHocId:
//     GiangVienId : //form[.//input[@name='KhoaHocId' and @value='X']]//select[@name='GiangVienId']
//     GhiChu      : //form[.//input[@name='KhoaHocId' and @value='X']]//input[@name='GhiChu']
//     Submit      : //form[.//input[@name='KhoaHocId' and @value='X']]//button[@type='submit']
//
// Kết quả:
//   Thành công → redirect /Admin/PhanCong → .alert-success
//   Thất bại   → ở lại trang, .alert-danger hoặc .alert-warning
//
// Admin seed: admin@nnl.com / Admin@123
// ============================================================
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.testobject.TestObject
import com.kms.katalon.core.testobject.ConditionType
import com.kms.katalon.core.testobject.SelectorMethod
import com.kms.katalon.core.util.KeywordUtil
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook

// ─── CONSTANTS ───────────────────────────────────────────────────────────────
final String BASE_URL    = 'http://localhost:5125'
final String EXCEL_PATH  = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC06_PhanCong.xlsx'
final String ADMIN_EMAIL = 'admin@nnl.com'
final String ADMIN_PASS  = 'Admin@123'
final int    TIMEOUT     = 10

// ─── HELPER: CSS selector ────────────────────────────────────────────────────
def makeBy = { String css ->
    TestObject o = new TestObject('css_el')
    o.addProperty('css', ConditionType.EQUALS, css)
    o.setSelectorMethod(SelectorMethod.CSS)
    return o
}

// ─── HELPER: XPath selector ──────────────────────────────────────────────────
def makeByXPath = { String xpath ->
    TestObject o = new TestObject('xpath_el')
    o.addProperty('xpath', ConditionType.EQUALS, xpath)
    o.setSelectorMethod(SelectorMethod.XPATH)
    return o
}

// ─── HELPER: Đọc giá trị ô Excel ─────────────────────────────────────────────
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

// ─── MỞ TRÌNH DUYỆT & ĐĂNG NHẬP ADMIN (một lần) ─────────────────────────────
WebUI.openBrowser('')
WebUI.setViewPortSize(1366, 768)

WebUI.navigateToUrl("${BASE_URL}/Account/Login")
WebUI.waitForElementVisible(makeBy('#Email'), TIMEOUT)
WebUI.setText(makeBy('#Email'), ADMIN_EMAIL)
WebUI.waitForElementVisible(makeBy('#pwInput'), TIMEOUT)
WebUI.setText(makeBy('#pwInput'), ADMIN_PASS)
WebUI.click(makeBy('button[type="submit"]'))
WebUI.waitForPageLoad(TIMEOUT)
KeywordUtil.logInfo("Đăng nhập Admin thành công")

int pass = 0, fail = 0

// ─── TEST LOOP ────────────────────────────────────────────────────────────────
for (int i = 1; i <= sheet.getLastRowNum(); i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    // Cột: 0=STT, 1=MoTa, 2=KhoaHocId, 3=GiangVienId, 4=GhiChu, 5=KetQuaMongDoi
    String khoaHocId     = cellStr(row.getCell(2))
    String giangVienId   = cellStr(row.getCell(3))
    String ghiChu        = cellStr(row.getCell(4))
    String ketQuaMongDoi = cellStr(row.getCell(5))
    if (khoaHocId.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Điều hướng tới trang Phân công giảng viên
        WebUI.navigateToUrl("${BASE_URL}/Admin/PhanCong")
        WebUI.waitForPageLoad(TIMEOUT)

        // 2. Xây dựng XPath để tìm select GiangVienId trong form của KhoaHoc tương ứng
        //    Form chứa: input[name='KhoaHocId' and @value='khoaHocId']
        String xpSelect  = "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//select[@name='GiangVienId']"
        String xpGhiChu  = "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//input[@name='GhiChu']"
        String xpSubmit  = "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//button[@type='submit']"

        // 3. Chờ select GiangVienId xuất hiện
        WebUI.waitForElementVisible(makeByXPath(xpSelect), TIMEOUT)

        // 4. Chọn giảng viên  → select[name="GiangVienId"]
        WebUI.selectOptionByValue(makeByXPath(xpSelect), giangVienId, false)

        // 5. Nhập ghi chú (nếu có)  → input[name="GhiChu"]
        WebUI.waitForElementVisible(makeByXPath(xpGhiChu), TIMEOUT)
        WebUI.clearText(makeByXPath(xpGhiChu))
        if (!ghiChu.isEmpty()) {
            WebUI.setText(makeByXPath(xpGhiChu), ghiChu)
        }

        // 6. Click nút Lưu trong form tương ứng
        WebUI.waitForElementVisible(makeByXPath(xpSubmit), TIMEOUT)
        WebUI.click(makeByXPath(xpSubmit))
        WebUI.waitForPageLoad(TIMEOUT)

        // 7. Kiểm tra kết quả
        if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
            // Thành công: redirect về /Admin/PhanCong, .alert-success
            boolean onPhanCong = WebUI.getUrl().toLowerCase().contains('/admin/phancong')
            boolean hasSuccess = WebUI.waitForElementPresent(makeBy('.alert-success'), 5)
            ok = onPhanCong && hasSuccess
            if (!ok) note = "OnPage: ${onPhanCong}, AlertSuccess: ${hasSuccess}"
        } else {
            // Thất bại: cảnh báo
            boolean hasErr  = WebUI.waitForElementPresent(makeBy('.alert-danger'), 4)
            boolean hasWarn = WebUI.waitForElementPresent(makeBy('.alert-warning'), 3)
            ok = hasErr || hasWarn
            if (!ok) note = 'Không tìm thấy thông báo lỗi'
        }
    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
    }

    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: KhoaHoc=${khoaHocId} GV=${giangVienId} → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU & ĐÓNG ──────────────────────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC06 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
