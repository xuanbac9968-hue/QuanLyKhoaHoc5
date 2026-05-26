import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC03 – Tạo tài khoản (Admin tạo qua modal)
 * Data-Driven: đọc từ TestData/TC03_TaoTaiKhoan.xlsx
 * Columns: 0=STT, 1=MoTa, 2=HoTen, 3=Email, 4=VaiTro, 5=MatKhau,
 *          6=XacNhanMatKhau, 7=KetQuaMongDoi, 8=KetQua
 *
 * Lưu ý: Script đăng nhập admin → vào /Admin/TaiKhoan → mở modal → điền form
 *        Thành công: toast bg-success xuất hiện
 *        Thất bại: #create-alert hiện (không có class d-none)
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC03_TaoTaiKhoan.xlsx'
FileInputStream fis = new FileInputStream(testDataPath)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

int totalRows = sheet.getLastRowNum()

for (int i = 1; i <= totalRows; i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String stt           = row.getCell(0)?.toString()?.trim() ?: ''
    String moTa          = row.getCell(1)?.toString()?.trim() ?: ''
    String hoTen         = row.getCell(2)?.toString()?.trim() ?: ''
    String email         = row.getCell(3)?.toString()?.trim() ?: ''
    String vaiTro        = row.getCell(4)?.toString()?.trim() ?: ''
    String matKhau       = row.getCell(5)?.toString()?.trim() ?: ''
    String xacNhanMK     = row.getCell(6)?.toString()?.trim() ?: ''
    String ketQuaMong    = row.getCell(7)?.toString()?.trim() ?: ''

    if (hoTen.isEmpty() && email.isEmpty()) continue

    WebUI.comment("=== TC03 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Đăng nhập admin
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), GlobalVariable.ADMIN_EMAIL)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), GlobalVariable.ADMIN_PASS)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Vào trang quản lý tài khoản
        WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Admin/TaiKhoan')
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Mở modal tạo tài khoản
        WebUI.click(findTestObject('Object Repository/Page_TaoTaiKhoan/btn_MoModal'))
        WebUI.waitForElementVisible(
            findTestObject('Object Repository/Page_TaoTaiKhoan/input_crHoTen'),
            GlobalVariable.TIMEOUT as int
        )

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
                vaiTro,
                false
            )
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

        // Chờ phản hồi AJAX
        WebUI.delay(2)

        if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
            boolean hasToast = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_TaoTaiKhoan/div_ToastSuccess'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            ketQua = hasToast ? 'PASS' : 'FAIL'
            WebUI.comment(hasToast ? "PASS: Toast thành công xuất hiện" : "FAIL: Không có toast thành công")
        } else {
            // Kỳ vọng thất bại: #create-alert không có class d-none (hiện)
            boolean alertVisible = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_TaoTaiKhoan/div_CreateAlert'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            if (alertVisible) {
                // Kiểm tra không có class d-none
                String alertClass = WebUI.getAttribute(
                    findTestObject('Object Repository/Page_TaoTaiKhoan/div_CreateAlert'),
                    'class'
                )
                ketQua = !alertClass.contains('d-none') ? 'PASS' : 'FAIL'
                WebUI.comment(ketQua == 'PASS' ? "PASS: Alert lỗi hiển thị" : "FAIL: Alert lỗi vẫn bị ẩn")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Không tìm thấy alert element")
            }
        }
    } catch (Exception e) {
        ketQua = 'ERROR: ' + e.getMessage()
        WebUI.comment("ERROR tại row ${stt}: ${e.getMessage()}")
    } finally {
        try { WebUI.closeBrowser() } catch (Exception ignored) {}
    }

    // Ghi kết quả
    FileInputStream fisW = new FileInputStream(testDataPath)
    Workbook wbW = new XSSFWorkbook(fisW)
    Sheet sheetW = wbW.getSheetAt(0)
    fisW.close()
    sheetW.getRow(i).createCell(8).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC03 Hoàn tất ===")
