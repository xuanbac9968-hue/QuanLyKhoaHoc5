// ============================================================
// TC04_ThemKhoaHoc.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Thêm khóa học mới (Admin)
// URL       : http://localhost:5125/KhoaHoc/Create
// Excel     : KiemThu\TestData\TC04_ThemKhoaHoc.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0)  STT        | B(1)  MoTa
//   C(2)  TenKhoaHoc | D(3)  MoTaKhoaHoc
//   E(4)  NgonNgu    (Tiếng Anh / Tiếng Nhật / Tiếng Hàn)
//   F(5)  TrinhDo    (Sơ cấp / Trung cấp / Cao cấp / IELTS / TOEIC / N5 / N4 / N3 / N2 / N1)
//   G(6)  HocPhi     (VD: 3500000)
//   H(7)  ThoiLuong  (số buổi, VD: 40)
//   I(8)  SoBuoiMoiTuan  (1-7)
//   J(9)  ThoiGianMoiBuoi (phút, VD: 90)
//   K(10) TrangThai   (DangMo / TamDung / DaDong)
//   L(11) KetQuaMongDoi (ThanhCong/ThatBai)
//   M(12) KetQua  ← script tự ghi
//
// Selectors xác nhận từ Views/KhoaHoc/Create.cshtml:
//   - TenKhoaHoc         : asp-for="TenKhoaHoc"         → id="TenKhoaHoc"
//   - MoTa               : asp-for="MoTa"               → id="MoTa"       (textarea)
//   - NgonNgu            : asp-for="NgonNgu"             → id="NgonNgu"    (select)
//   - TrinhDo            : asp-for="TrinhDo"             → id="TrinhDo"    (select)
//   - HocPhi             : asp-for="HocPhi"              → id="HocPhi"
//   - ThoiLuong          : asp-for="ThoiLuong"           → id="ThoiLuong"
//   - SoBuoiMoiTuan      : asp-for="SoBuoiMoiTuan"       → id="SoBuoiMoiTuan"
//   - ThoiGianMoiBuoi    : asp-for="ThoiGianMoiBuoi"     → id="ThoiGianMoiBuoi"
//   - NoiDungChuongTrinh : asp-for="NoiDungChuongTrinh"  → id="NoiDungChuongTrinh" (textarea)
//   - TrangThai          : asp-for="TrangThai"            → id="TrangThai"  (select)
//   - AnhBiaFile         : explicit id="anhBiaInput"     → BỎ QUA (file upload)
//   - Submit             : button[type="submit"]
//
// Layout (Views/Shared/_LayoutAdmin.cshtml):
//   Thành công → TempData["Success"] → div.alert.alert-success
//   Thất bại   → ModelState error    → div[asp-validation-summary] = .alert.alert-danger
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
final String EXCEL_PATH  = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC04_ThemKhoaHoc.xlsx'
final String ADMIN_EMAIL = 'admin@nnl.com'
final String ADMIN_PASS  = 'Admin@123'
final int    TIMEOUT     = 10

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

    String tenKhoaHoc    = cellStr(row.getCell(2))
    String moTa          = cellStr(row.getCell(3))
    String ngonNgu       = cellStr(row.getCell(4))
    String trinhDo       = cellStr(row.getCell(5))
    String hocPhi        = cellStr(row.getCell(6))
    String thoiLuong     = cellStr(row.getCell(7))
    String soBuoi        = cellStr(row.getCell(8))
    String thoiGian      = cellStr(row.getCell(9))
    String trangThai     = cellStr(row.getCell(10))
    String ketQuaMongDoi = cellStr(row.getCell(11))
    if (tenKhoaHoc.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Điều hướng tới form thêm khóa học
        WebUI.navigateToUrl("${BASE_URL}/KhoaHoc/Create")
        WebUI.waitForElementVisible(makeBy('#TenKhoaHoc'), TIMEOUT)

        // 2. Tên khóa học  → id="TenKhoaHoc"
        WebUI.clearText(makeBy('#TenKhoaHoc'))
        WebUI.setText(makeBy('#TenKhoaHoc'), tenKhoaHoc)

        // 3. Mô tả  → id="MoTa"  (textarea)
        WebUI.waitForElementVisible(makeBy('#MoTa'), TIMEOUT)
        WebUI.clearText(makeBy('#MoTa'))
        if (!moTa.isEmpty()) WebUI.setText(makeBy('#MoTa'), moTa)

        // 4. Ngôn ngữ  → id="NgonNgu"  (select)
        if (!ngonNgu.isEmpty()) {
            WebUI.selectOptionByValue(makeBy('#NgonNgu'), ngonNgu, false)
        }

        // 5. Trình độ  → id="TrinhDo"  (select)
        if (!trinhDo.isEmpty()) {
            WebUI.selectOptionByValue(makeBy('#TrinhDo'), trinhDo, false)
        }

        // 6. Học phí  → id="HocPhi"  (number)
        WebUI.clearText(makeBy('#HocPhi'))
        if (!hocPhi.isEmpty()) WebUI.setText(makeBy('#HocPhi'), hocPhi)

        // 7. Thời lượng (số buổi)  → id="ThoiLuong"
        WebUI.clearText(makeBy('#ThoiLuong'))
        if (!thoiLuong.isEmpty()) WebUI.setText(makeBy('#ThoiLuong'), thoiLuong)

        // 8. Số buổi mỗi tuần  → id="SoBuoiMoiTuan"
        WebUI.clearText(makeBy('#SoBuoiMoiTuan'))
        if (!soBuoi.isEmpty()) WebUI.setText(makeBy('#SoBuoiMoiTuan'), soBuoi)

        // 9. Thời gian mỗi buổi (phút)  → id="ThoiGianMoiBuoi"
        WebUI.clearText(makeBy('#ThoiGianMoiBuoi'))
        if (!thoiGian.isEmpty()) WebUI.setText(makeBy('#ThoiGianMoiBuoi'), thoiGian)

        // 10. Trạng thái  → id="TrangThai"  (select)
        if (!trangThai.isEmpty()) {
            WebUI.selectOptionByValue(makeBy('#TrangThai'), trangThai, false)
        }

        // 11. (Bỏ qua upload ảnh bìa → id="anhBiaInput")

        // 12. Submit form
        WebUI.waitForElementVisible(makeBy('button[type="submit"]'), TIMEOUT)
        WebUI.click(makeBy('button[type="submit"]'))
        WebUI.waitForPageLoad(TIMEOUT)

        // 13. Kiểm tra kết quả
        if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
            // Thành công: redirect về /KhoaHoc/Index, layout hiển thị .alert-success
            boolean redirected = WebUI.getUrl().toLowerCase().contains('/khoahoc')
            boolean hasSuccess = WebUI.waitForElementPresent(makeBy('.alert-success'), 5)
            ok = hasSuccess
            if (!ok) note = "Redirect: ${redirected}, AlertSuccess: ${hasSuccess}"
        } else {
            // Thất bại: ở lại form create, .alert-danger từ validation summary
            boolean hasError = WebUI.waitForElementPresent(makeBy('.alert-danger'), 5)
            if (!hasError) hasError = WebUI.waitForElementPresent(makeBy('.text-danger'), 3)
            ok = hasError
            if (!ok) note = 'Không tìm thấy thông báo lỗi'
        }
    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
    }

    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: [${tenKhoaHoc}] → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU & ĐÓNG ──────────────────────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC04 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
