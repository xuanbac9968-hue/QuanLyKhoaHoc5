// ============================================================
// TC03_TaoTaiKhoan.groovy  –  Katalon Studio  –  Data Driven
// Chức năng : Tạo tài khoản mới (Admin)
// URL       : http://localhost:5125/TaiKhoan
// Excel     : KiemThu\TestData\TC03_TaoTaiKhoan.xlsx
//
// Cấu trúc cột Excel (header row 0):
//   A(0) STT | B(1) MoTa
//   C(2) HoTen | D(3) Email | E(4) VaiTro (HocVien/GiangVien/Admin)
//   F(5) MatKhau | G(6) XacNhanMatKhau
//   H(7) KetQuaMongDoi (ThanhCong/ThatBai)
//   I(8) KetQua  ← script tự ghi
//
// Selectors xác nhận từ Views/TaiKhoan/Index.cshtml:
//   - Open modal btn : button[onclick="openCreateModal()"]
//   - Modal          : #modalCreateTK
//   - HoTen          : id="cr-hoTen"
//   - Email          : id="cr-email"
//   - VaiTro         : id="cr-vaiTro"   (select)
//   - MatKhau        : id="cr-matKhau"
//   - XacNhan        : id="cr-xacNhan"
//   - Submit         : id="btn-create-submit"  (AJAX, không phải form submit)
//
// Kết quả:
//   Thành công → Toast div.toast.bg-success xuất hiện trong #toast-container
//   Thất bại   → #create-alert mất class d-none (hiển thị lỗi trong modal)
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
final String BASE_URL      = 'http://localhost:5125'
final String EXCEL_PATH    = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC03_TaoTaiKhoan.xlsx'
final String ADMIN_EMAIL   = 'admin@nnl.com'
final String ADMIN_PASS    = 'Admin@123'
final int    TIMEOUT       = 10

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

    // Cột: 0=STT, 1=MoTa, 2=HoTen, 3=Email, 4=VaiTro, 5=MatKhau, 6=XacNhan, 7=KetQuaMongDoi
    String hoTen         = cellStr(row.getCell(2))
    String email         = cellStr(row.getCell(3))
    String vaiTro        = cellStr(row.getCell(4))
    String matKhau       = cellStr(row.getCell(5))
    String xacNhan       = cellStr(row.getCell(6))
    String ketQuaMongDoi = cellStr(row.getCell(7))
    if (hoTen.isEmpty() && email.isEmpty()) continue

    boolean ok   = false
    String  note = ''

    try {
        // 1. Điều hướng tới trang Quản lý tài khoản
        WebUI.navigateToUrl("${BASE_URL}/TaiKhoan")
        WebUI.waitForElementVisible(makeBy('button[onclick="openCreateModal()"]'), TIMEOUT)

        // 2. Click mở modal "Tạo tài khoản mới"
        WebUI.click(makeBy('button[onclick="openCreateModal()"]'))

        // 3. Chờ modal hiện và input đầu tiên khả dụng
        WebUI.waitForElementVisible(makeBy('#cr-hoTen'), TIMEOUT)

        // 4. Nhập thông tin vào modal  → id="cr-hoTen"
        WebUI.clearText(makeBy('#cr-hoTen'))
        WebUI.setText(makeBy('#cr-hoTen'), hoTen)

        // 5. Nhập Email  → id="cr-email"
        WebUI.clearText(makeBy('#cr-email'))
        WebUI.setText(makeBy('#cr-email'), email)

        // 6. Chọn Vai trò  → id="cr-vaiTro"  (select)
        if (!vaiTro.isEmpty()) {
            WebUI.selectOptionByValue(makeBy('#cr-vaiTro'), vaiTro, false)
        }

        // 7. Nhập Mật khẩu  → id="cr-matKhau"
        WebUI.clearText(makeBy('#cr-matKhau'))
        WebUI.setText(makeBy('#cr-matKhau'), matKhau)

        // 8. Xác nhận mật khẩu  → id="cr-xacNhan"
        WebUI.clearText(makeBy('#cr-xacNhan'))
        WebUI.setText(makeBy('#cr-xacNhan'), xacNhan)

        // 9. Click nút "Tạo tài khoản"  → id="btn-create-submit"  (AJAX)
        WebUI.waitForElementVisible(makeBy('#btn-create-submit'), TIMEOUT)
        WebUI.click(makeBy('#btn-create-submit'))

        // 10. Chờ phản hồi AJAX (tối đa 8 giây)
        Thread.sleep(1500)

        // 11. Kiểm tra kết quả
        if (ketQuaMongDoi.equalsIgnoreCase('ThanhCong')) {
            // Thành công: Toast div.toast.bg-success xuất hiện trong #toast-container
            ok = WebUI.waitForElementPresent(makeBy('#toast-container .toast.bg-success'), 6)
            if (!ok) note = 'Không tìm thấy toast thành công'
        } else {
            // Thất bại: #create-alert hiển thị lỗi (mất class d-none)
            // Kiểm tra element hiển thị: alert visible và không có class d-none
            ok = WebUI.waitForElementPresent(makeBy('#create-alert:not(.d-none)'), 5)
            if (!ok) note = 'Không tìm thấy thông báo lỗi trong modal'
        }
    } catch (Exception e) {
        note = (e.getMessage() ?: 'Unknown error').take(120)
        ok   = false
    }

    Cell resultCell = row.getCell(colKetQua) ?: row.createCell(colKetQua)
    resultCell.setCellValue(ok ? 'Pass' : "Fail – ${note}")
    if (ok) pass++ else fail++
    KeywordUtil.logInfo("Row ${i}: [${email}] vaiTro=${vaiTro} → ${ok ? 'PASS' : 'FAIL'}" + (ok ? '' : " | ${note}"))
}

// ─── LƯU & ĐÓNG ──────────────────────────────────────────────────────────────
FileOutputStream fos = new FileOutputStream(new File(EXCEL_PATH))
wb.write(fos); fos.close(); wb.close()
WebUI.closeBrowser()
KeywordUtil.logInfo("=== TC03 HOÀN THÀNH: ${pass} Pass | ${fail} Fail ===")
