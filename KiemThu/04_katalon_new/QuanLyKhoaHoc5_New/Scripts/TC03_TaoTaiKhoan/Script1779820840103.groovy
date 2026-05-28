import internal.GlobalVariable
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC03 – Tạo tài khoản (Admin – modal tại /TaiKhoan)
 * FIX: URL đúng là /TaiKhoan (TaiKhoanController.Index), không phải /Admin/TaiKhoan
 * Modal: button[onclick="openCreateModal()"] → #cr-hoTen, #cr-email, #cr-vaiTro, #cr-matKhau, #cr-xacNhan
 * Submit AJAX: #btn-create-submit
 * Thành công: #toast-container .toast.bg-success xuất hiện
 * Thất bại:   #create-alert hiện (không có class d-none)
 * Columns: 0=STT,1=MoTa,2=HoTen,3=Email,4=VaiTro,5=MatKhau,6=XacNhanMatKhau,7=KetQuaMongDoi,8=KetQua
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC03_TaoTaiKhoan.xlsx'
DataFormatter fmt = new DataFormatter()

FileInputStream fis = new FileInputStream(testDataPath)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

int totalRows = sheet.getLastRowNum()
int timeout = GlobalVariable.TIMEOUT as int

for (int i = 1; i <= totalRows; i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String stt        = fmt.formatCellValue(row.getCell(0))?.trim() ?: ''
    String moTa       = fmt.formatCellValue(row.getCell(1))?.trim() ?: ''
    String hoTen      = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String email      = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String vaiTro     = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String matKhau    = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''
    String xacNhanMK  = fmt.formatCellValue(row.getCell(6))?.trim() ?: ''
    String ketQuaMong = fmt.formatCellValue(row.getCell(7))?.trim() ?: ''

    if (hoTen.isEmpty() && email.isEmpty()) continue

    WebUI.comment("=== TC03 Row ${stt}: ${moTa} ===")
    String ketQua = 'FAIL'

    try {
        // Đăng nhập admin
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.waitForElementVisible(findTestObject('Object Repository/Page_Login/input_Email'), timeout)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), GlobalVariable.ADMIN_EMAIL)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), GlobalVariable.ADMIN_PASS)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(timeout)

        if (WebUI.getUrl().contains('/Account/Login')) {
            ketQua = 'ERROR: Đăng nhập admin thất bại'
            WebUI.comment(ketQua)
        } else {
            // FIX: URL đúng là /TaiKhoan (TaiKhoanController.Index có modal)
            WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/TaiKhoan')
            WebUI.waitForElementVisible(
                findTestObject('Object Repository/Page_TaoTaiKhoan/btn_MoModal'), timeout)

            // Mở modal
            WebUI.click(findTestObject('Object Repository/Page_TaoTaiKhoan/btn_MoModal'))
            WebUI.waitForElementVisible(
                findTestObject('Object Repository/Page_TaoTaiKhoan/input_crHoTen'), timeout)

            // Điền form
            WebUI.clearText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crHoTen'))
            if (!hoTen.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crHoTen'), hoTen)
            }

            WebUI.clearText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crEmail'))
            if (!email.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crEmail'), email)
            }

            if (!vaiTro.isEmpty()) {
                WebUI.selectOptionByValue(
                    findTestObject('Object Repository/Page_TaoTaiKhoan/select_crVaiTro'),
                    vaiTro, false)
            }

            WebUI.clearText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crMatKhau'))
            if (!matKhau.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crMatKhau'), matKhau)
            }

            WebUI.clearText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crXacNhan'))
            if (!xacNhanMK.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_TaoTaiKhoan/input_crXacNhan'), xacNhanMK)
            }

            WebUI.click(findTestObject('Object Repository/Page_TaoTaiKhoan/btn_CreateSubmit'))
            WebUI.delay(2)  // Chờ AJAX phản hồi

            if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
                boolean hasToast = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Page_TaoTaiKhoan/div_ToastSuccess'),
                    timeout, FailureHandling.OPTIONAL)
                ketQua = hasToast ? 'PASS' : 'FAIL'
                WebUI.comment(hasToast ? "PASS: Toast thành công xuất hiện" : "FAIL: Không có toast thành công")
            } else {
                // Kiểm tra #create-alert không có class d-none (đang hiện)
                boolean alertExists = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Page_TaoTaiKhoan/div_CreateAlert'),
                    timeout, FailureHandling.OPTIONAL)
                if (alertExists) {
                    String cls = WebUI.getAttribute(
                        findTestObject('Object Repository/Page_TaoTaiKhoan/div_CreateAlert'), 'class')
                    ketQua = !cls.contains('d-none') ? 'PASS' : 'FAIL'
                    WebUI.comment(ketQua == 'PASS'
                        ? "PASS: Alert lỗi đang hiển thị (class=${cls})"
                        : "FAIL: Alert lỗi vẫn bị ẩn (class=${cls})")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Không tìm thấy #create-alert element")
                }
            }
        }
    } catch (Exception e) {
        ketQua = 'ERROR: ' + e.getMessage()
        WebUI.comment("ERROR row ${stt}: ${e.getMessage()}")
    } finally {
        try { WebUI.closeBrowser() } catch (Exception ignored) {}
    }

    FileInputStream fisW = new FileInputStream(testDataPath)
    Workbook wbW = new XSSFWorkbook(fisW)
    fisW.close()
    wbW.getSheetAt(0).getRow(i).createCell(8).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC03 Hoàn tất ===")
