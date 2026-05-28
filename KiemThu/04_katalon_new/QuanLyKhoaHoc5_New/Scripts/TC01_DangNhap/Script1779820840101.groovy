import internal.GlobalVariable
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.ss.util.CellReference
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC01 – Đăng nhập
 * URL: http://localhost:5125/Account/Login
 * Columns: 0=STT, 1=MoTa, 2=Email, 3=MatKhau, 4=GhiNho, 5=KetQuaMongDoi, 6=KetQua
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC01_DangNhap.xlsx'
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
    String email      = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String matKhau    = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String ghiNho     = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String ketQuaMong = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''

    if (email.isEmpty()) continue

    WebUI.comment("=== TC01 Row ${stt}: ${moTa} ===")
    String ketQua = 'FAIL'

    try {
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.waitForElementVisible(findTestObject('Object Repository/Page_Login/input_Email'), timeout)

        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Email'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), email)

        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_MatKhau'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhau)

        if ('true'.equalsIgnoreCase(ghiNho)) {
            boolean checked = WebUI.verifyElementChecked(
                findTestObject('Object Repository/Page_Login/chk_GhiNho'),
                timeout, FailureHandling.OPTIONAL)
            if (!checked) WebUI.click(findTestObject('Object Repository/Page_Login/chk_GhiNho'))
        }

        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(timeout)

        String url = WebUI.getUrl()
        if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
            if (!url.contains('/Account/Login')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đăng nhập thành công → ${url}")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Vẫn ở trang Login")
            }
        } else {
            boolean hasError = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_Login/div_AlertDanger'),
                timeout, FailureHandling.OPTIONAL)
            if (hasError || url.contains('/Account/Login')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đăng nhập thất bại như mong đợi")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Đăng nhập thành công khi không nên")
            }
        }
    } catch (Exception e) {
        ketQua = 'ERROR: ' + e.getMessage()
        WebUI.comment("ERROR row ${stt}: ${e.getMessage()}")
    } finally {
        try { WebUI.closeBrowser() } catch (Exception ignored) {}
    }

    // Ghi KetQua vào Excel
    FileInputStream fisW = new FileInputStream(testDataPath)
    Workbook wbW = new XSSFWorkbook(fisW)
    fisW.close()
    wbW.getSheetAt(0).getRow(i).createCell(6).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC01 Hoàn tất ===")
