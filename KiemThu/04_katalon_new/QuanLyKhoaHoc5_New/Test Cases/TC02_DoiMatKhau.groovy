import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC02 – Đổi mật khẩu
 * Bước 1: Đăng nhập với EmailLogin/MatKhauLogin
 * Bước 2: Vào /Account/ChangePassword, nhập MatKhauCu/MatKhauMoi/XacNhanMatKhau
 * Columns: 0=STT,1=MoTa,2=EmailLogin,3=MatKhauLogin,4=MatKhauCu,
 *          5=MatKhauMoi,6=XacNhanMatKhau,7=KetQuaMongDoi,8=KetQua
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC02_DoiMatKhau.xlsx'
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

    String stt            = fmt.formatCellValue(row.getCell(0))?.trim() ?: ''
    String moTa           = fmt.formatCellValue(row.getCell(1))?.trim() ?: ''
    String emailLogin     = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String matKhauLogin   = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String matKhauCu      = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String matKhauMoi     = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''
    String xacNhanMK      = fmt.formatCellValue(row.getCell(6))?.trim() ?: ''
    String ketQuaMong     = fmt.formatCellValue(row.getCell(7))?.trim() ?: ''

    if (emailLogin.isEmpty()) continue

    WebUI.comment("=== TC02 Row ${stt}: ${moTa} ===")
    String ketQua = 'FAIL'

    try {
        // Bước 1: Đăng nhập
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.waitForElementVisible(findTestObject('Object Repository/Page_Login/input_Email'), timeout)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), emailLogin)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhauLogin)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(timeout)

        // Kiểm tra đăng nhập thành công
        if (WebUI.getUrl().contains('/Account/Login')) {
            ketQua = 'ERROR: Đăng nhập thất bại với ' + emailLogin
            WebUI.comment(ketQua)
        } else {
            // Bước 2: Vào trang đổi mật khẩu
            WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Account/ChangePassword')
            WebUI.waitForElementVisible(
                findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauCu'), timeout)

            WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauCu'))
            WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauCu'), matKhauCu)

            WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauMoi'))
            WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauMoi'), matKhauMoi)

            WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_XacNhanMatKhau'))
            WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_XacNhanMatKhau'), xacNhanMK)

            WebUI.click(findTestObject('Object Repository/Page_DoiMatKhau/btn_XacNhan'))
            WebUI.waitForPageLoad(timeout)

            if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
                boolean hasSuccess = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Common/div_AlertSuccess'),
                    timeout, FailureHandling.OPTIONAL)
                String url = WebUI.getUrl()
                if (hasSuccess || !url.contains('/Account/ChangePassword')) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Đổi mật khẩu thành công")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Không có thông báo thành công và vẫn ở trang ChangePassword")
                }
            } else {
                boolean hasError = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Page_DoiMatKhau/div_AlertDanger'),
                    timeout, FailureHandling.OPTIONAL)
                if (hasError) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Đổi mật khẩu thất bại như mong đợi")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Không có thông báo lỗi")
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
WebUI.comment("=== TC02 Hoàn tất ===")
