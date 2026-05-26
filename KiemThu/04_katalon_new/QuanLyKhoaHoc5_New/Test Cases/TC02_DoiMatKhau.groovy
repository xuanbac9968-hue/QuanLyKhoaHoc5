import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC02 – Đổi mật khẩu
 * Data-Driven: đọc từ TestData/TC02_DoiMatKhau.xlsx
 * Columns: 0=STT, 1=MoTa, 2=EmailLogin, 3=MatKhauLogin, 4=MatKhauCu,
 *          5=MatKhauMoi, 6=XacNhanMatKhau, 7=KetQuaMongDoi, 8=KetQua
 *
 * Lưu ý: Script tự đăng nhập trước khi đổi mật khẩu
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC02_DoiMatKhau.xlsx'
FileInputStream fis = new FileInputStream(testDataPath)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

int totalRows = sheet.getLastRowNum()

for (int i = 1; i <= totalRows; i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String stt             = row.getCell(0)?.toString()?.trim() ?: ''
    String moTa            = row.getCell(1)?.toString()?.trim() ?: ''
    String emailLogin      = row.getCell(2)?.toString()?.trim() ?: ''
    String matKhauLogin    = row.getCell(3)?.toString()?.trim() ?: ''
    String matKhauCu       = row.getCell(4)?.toString()?.trim() ?: ''
    String matKhauMoi      = row.getCell(5)?.toString()?.trim() ?: ''
    String xacNhanMatKhau  = row.getCell(6)?.toString()?.trim() ?: ''
    String ketQuaMong      = row.getCell(7)?.toString()?.trim() ?: ''

    if (emailLogin.isEmpty()) continue

    WebUI.comment("=== TC02 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Bước 1: Đăng nhập
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Email'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), emailLogin)
        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_MatKhau'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhauLogin)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Bước 2: Vào trang đổi mật khẩu
        WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Account/ChangePassword')
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Bước 3: Nhập thông tin đổi mật khẩu
        WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauCu'))
        WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauCu'), matKhauCu)

        WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauMoi'))
        WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_MatKhauMoi'), matKhauMoi)

        WebUI.clearText(findTestObject('Object Repository/Page_DoiMatKhau/input_XacNhanMatKhau'))
        WebUI.setText(findTestObject('Object Repository/Page_DoiMatKhau/input_XacNhanMatKhau'), xacNhanMatKhau)

        WebUI.click(findTestObject('Object Repository/Page_DoiMatKhau/btn_XacNhan'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
            // Kỳ vọng thành công: có alert-success hoặc chuyển trang
            boolean hasSuccess = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Common/div_AlertSuccess'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            if (hasSuccess) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đổi mật khẩu thành công")
            } else {
                // Có thể redirect về Dashboard
                String url = WebUI.getUrl()
                if (!url.contains('/Account/ChangePassword')) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Đổi mật khẩu thành công, chuyển tới ${url}")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Không có thông báo thành công")
                }
            }
        } else {
            // Kỳ vọng thất bại: phải có alert-danger
            boolean hasError = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_DoiMatKhau/div_AlertDanger'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            if (hasError) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đổi mật khẩu thất bại như mong đợi")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Không có thông báo lỗi")
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
    Row rowW = sheetW.getRow(i)
    rowW.createCell(8).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC02 Hoàn tất ===")
