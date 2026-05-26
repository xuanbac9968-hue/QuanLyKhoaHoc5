// ============================================================
// TC05_TaoThanhToan.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Tạo yêu cầu thanh toán học phí (Học viên)
// URL       : http://localhost:5125/ThanhToan/TaoYeuCau?khoaHocId={id}
// Excel     : KiemThu\TestData\TC05_TaoThanhToan.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0) STT  | B(1) MoTa
//   C(2) EmailHocVien  | D(3) MatKhauHocVien
//   E(4) KhoaHocId     (ID khóa học - xem DB sau khi seed)
//   F(5) PhuongThuc    (TienMat / ChuyenKhoan)
//   G(6) GhiChu        (optional)
//   H(7) KetQuaMongDoi (ThanhCong / ThatBai)
//   I(8) KetQua  ← script tự ghi
//
// Điều kiện: HocVien phải được duyệt vào KhoaHoc đó (DaDuyet)
// Dữ liệu seed mẫu:
//   hv01@nnl.com / Hv@123  → KhoaHocId 2 (Tiếng Anh B1), KhoaHocId 3 (IELTS)
//   hv02@nnl.com / Hv@123  → KhoaHocId 4 (Nhật N5),    KhoaHocId 6 (Hàn Sơ Cấp)
//   hv03@nnl.com / Hv@123  → KhoaHocId 6 (Hàn Sơ Cấp)
//
// Selectors xác nhận từ Views/ThanhToan/TaoYeuCau.cshtml:
//   - PhuongThuc TienMat   : id="ptTienMat"      (radio, value="TienMat")
//   - PhuongThuc CK        : id="ptChuyenKhoan"  (radio, value="ChuyenKhoan")
//   - GhiChu               : asp-for="GhiChu"    → id="GhiChu" (textarea)
//   - Submit               : button[type="submit"]
//
// Kết quả:
//   Thành công → redirect /ThanhToan/CuaToi → .alert-success
//   Thất bại   → ở lại form, .alert-danger hoặc validation message
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
final String EXCEL_PATH = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC05_TaoThanhToan.xlsx'
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

String lastEmail = ''  // Theo dõi email đang đăng nhập để tránh re-login không cần thiết
int pass = 0, fail = 0

// ─── TEST LOOP ────────────────────────────────────────────────────────────────
for (int i = 1; i <= sheet.getLastRowNum(); i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String emailHV       = cellStr(row.getCell(2))
    String matKhauHV     = cellStr(row.getCell(3))
    String khoaHocId     = cellStr(row.getCell(4))
    String phuongThuc    = cellStr(row.getCell(5))
    String ghiChu        = cellStr(row.getCell(6))
    String ketQuaMongDoi = cellStr(row.getCell(7))
    if (emailHV.isEmpty() || khoaHocId.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Đăng nhập nếu cần (đổi user hoặc chưa đăng nhập)
        if (!emailHV.equalsIgnoreCase(lastEmail)) {
            boolean loggedIn = doLogin(emailHV, matKhauHV)
            if (!loggedIn) {
                note = "Không đăng nhập được với ${emailHV}"
                throw new Exception(note)
            }
            lastEmail = emailHV
        }

        // 2. Điều hướng tới form tạo yêu cầu thanh toán
        WebUI.navigateToUrl("${BASE_URL}/ThanhToan/TaoYeuCau?khoaHocId=${khoaHocId}")
        WebUI.waitForPageLoad(TIMEOUT)

        // Kiểm tra có bị redirect không (ví dụ chưa đăng ký KhoaHoc)
        String url = WebUI.getUrl()
        if (!url.contains('/ThanhToan/TaoYeuCau')) {
            if (ketQuaMongDoi.equalsIgnoreCase('ThatBai')) {
                // Bị redirect = đúng với case thất bại (không có quyền)
                ok   = true
                note = "Redirect đúng kỳ vọng: ${url}"
            } else {
                note = "Bị redirect tới ${url}, không vào được form"
                throw new Exception(note)
            }
        } else {
            // 3. Chọn phương thức thanh toán
            if (phuongThuc.equalsIgnoreCase('ChuyenKhoan')) {
                // Radio ChuyenKhoan  → id="ptChuyenKhoan"
                WebUI.waitForElementVisible(makeBy('#ptChuyenKhoan'), TIMEOUT)
                WebUI.click(makeBy('#ptChuyenKhoan'))
            } else {
                // Radio TienMat (default)  → id="ptTienMat"
                WebUI.waitForElementVisible(makeBy('#ptTienMat'), TIMEOUT)
                WebUI.click(makeBy('#ptTienMat'))
            }

            // 4. Ghi chú (optional)  → id="GhiChu"  (textarea)
            if (!ghiChu.isEmpty()) {
                WebUI.waitForElementVisible(makeBy('#GhiChu'), TIMEOUT)
                WebUI.clearText(makeBy('#GhiChu'))
                WebUI.setText(makeBy('#GhiChu'), ghiChu)
            }

            // 5. Submit
            WebUI.waitForElementVisible(makeBy('button[type="submit"]'), TIMEOUT)
            WebUI.click(makeBy('button[type="submit"]'))
            WebUI.waitForPageLoad(TIMEOUT)

            // 6. Kiểm tra kết quả
            if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
                // Thành công: redirect về /ThanhToan/CuaToi, .alert-success
                boolean redirected = WebUI.getUrl().toLowerCase().contains('/thanhtoan/cuatoi')
                boolean hasSuccess = WebUI.waitForElementPresent(makeBy('.alert-success'), 5)
                ok = redirected && hasSuccess
                if (!ok) note = "Redirect: ${redirected}, AlertSuccess: ${hasSuccess}"
            } else {
                // Thất bại: ở lại form hoặc redirect kèm cảnh báo
                boolean hasWarn = WebUI.waitForElementPresent(makeBy('.alert-warning'), 4)
                boolean hasErr  = WebUI.waitForElementPresent(makeBy('.alert-danger'), 3)
                ok = hasWarn || hasErr
                if (!ok) note = 'Không tìm thấy thông báo lỗi / cảnh báo'
            }
        }

    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
        // Cố đăng xuất để reset trạng thái
        try {
            WebUI.navigateToUrl("${BASE_URL}/Account/Logout")
            lastEmail = ''
        } catch (Exception ignored) {}
    }

    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: [${emailHV}] KhoaHoc=${khoaHocId} → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU & ĐÓNG ──────────────────────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC05 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
