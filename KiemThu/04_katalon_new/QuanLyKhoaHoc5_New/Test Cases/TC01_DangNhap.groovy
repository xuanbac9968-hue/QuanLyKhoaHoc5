import static com.kms.katalon.core.checkpoint.CheckpointFactory.findCheckpoint
import static com.kms.katalon.core.testcase.TestCaseFactory.findTestCase
import static com.kms.katalon.core.testdata.TestDataFactory.findTestData
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import com.kms.katalon.core.configuration.RunConfiguration
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC01 – Đăng nhập
 * Data-Driven: đọc từ TestData/TC01_DangNhap.xlsx
 * Columns: 0=STT, 1=MoTa, 2=Email, 3=MatKhau, 4=GhiNho, 5=KetQuaMongDoi, 6=KetQua
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC01_DangNhap.xlsx'
FileInputStream fis = new FileInputStream(testDataPath)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

int totalRows = sheet.getLastRowNum()  // 1-based last index (row 0 = header)

for (int i = 1; i <= totalRows; i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String stt          = row.getCell(0)?.toString()?.trim() ?: ''
    String moTa         = row.getCell(1)?.toString()?.trim() ?: ''
    String email        = row.getCell(2)?.toString()?.trim() ?: ''
    String matKhau      = row.getCell(3)?.toString()?.trim() ?: ''
    String ghiNho       = row.getCell(4)?.toString()?.trim() ?: ''
    String ketQuaMong   = row.getCell(5)?.toString()?.trim() ?: ''

    if (email.isEmpty()) continue

    WebUI.comment("=== TC01 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Mở trang đăng nhập
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()

        // Nhập thông tin
        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Email'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), email)

        WebUI.clearText(findTestObject('Object Repository/Page_Login/input_MatKhau'))
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhau)

        if ('true'.equalsIgnoreCase(ghiNho)) {
            boolean isChecked = WebUI.verifyElementChecked(
                findTestObject('Object Repository/Page_Login/chk_GhiNho'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            if (!isChecked) {
                WebUI.click(findTestObject('Object Repository/Page_Login/chk_GhiNho'))
            }
        }

        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
            // Kiểm tra đã chuyển khỏi trang Login
            String currentUrl = WebUI.getUrl()
            if (!currentUrl.contains('/Account/Login')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đăng nhập thành công, URL = ${currentUrl}")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Vẫn ở trang Login")
            }
        } else {
            // Kỳ vọng thất bại – phải còn ở trang Login và có alert-danger
            boolean hasError = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_Login/div_AlertDanger'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            String currentUrl = WebUI.getUrl()
            if (hasError || currentUrl.contains('/Account/Login')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Đăng nhập thất bại như mong đợi")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Đăng nhập thành công khi không nên")
            }
        }
    } catch (Exception e) {
        ketQua = 'ERROR: ' + e.getMessage()
        WebUI.comment("ERROR tại row ${stt}: ${e.getMessage()}")
    } finally {
        WebUI.closeBrowser()
    }

    // Ghi kết quả vào Excel
    FileInputStream fisW = new FileInputStream(testDataPath)
    Workbook wbW = new XSSFWorkbook(fisW)
    Sheet sheetW = wbW.getSheetAt(0)
    fisW.close()

    Row rowW = sheetW.getRow(i)
    Cell cell = rowW.createCell(6)
    cell.setCellValue(ketQua)

    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC01 Hoàn tất ===")
