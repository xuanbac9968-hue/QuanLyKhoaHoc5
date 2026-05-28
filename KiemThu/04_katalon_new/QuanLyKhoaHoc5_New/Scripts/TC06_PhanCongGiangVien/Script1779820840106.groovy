import internal.GlobalVariable
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import com.kms.katalon.core.testobject.ConditionType
import com.kms.katalon.core.testobject.TestObject
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC06 – Phân công giảng viên (Admin)
 * URL: /Admin/PhanCong ✓ (AdminController.PhanCong)
 * XPath động: //form[.//input[@name='KhoaHocId' and @value='X']]//select[@name='GiangVienId']
 * GiangVienId=0 → "Bỏ phân công"
 * Kết quả: controller luôn redirect về PhanCong với TempData["Success"] nếu submit được form
 * Columns: 0=STT,1=MoTa,2=KhoaHocId,3=GiangVienId,4=GhiChu,5=KetQuaMongDoi,6=KetQua
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC06_PhanCong.xlsx'
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

    String stt         = fmt.formatCellValue(row.getCell(0))?.trim() ?: ''
    String moTa        = fmt.formatCellValue(row.getCell(1))?.trim() ?: ''
    String khoaHocId   = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String giangVienId = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String ghiChu      = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String ketQuaMong  = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''

    // Bỏ .0 phòng trường hợp DataFormatter vẫn trả decimal
    khoaHocId   = khoaHocId.replace('.0','')
    giangVienId = giangVienId.replace('.0','')

    if (khoaHocId.isEmpty()) continue

    WebUI.comment("=== TC06 Row ${stt}: ${moTa} (KH${khoaHocId} → GV${giangVienId}) ===")
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
            // Vào trang phân công
            WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Admin/PhanCong')
            WebUI.waitForPageLoad(timeout)

            // Tạo TestObject động: select giảng viên trong form của KhoaHocId cụ thể
            TestObject selectGV = new TestObject("select_GV_KH${khoaHocId}")
            selectGV.addProperty('xpath', ConditionType.EQUALS,
                "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//select[@name='GiangVienId']")

            WebUI.waitForElementVisible(selectGV, timeout)
            WebUI.selectOptionByValue(selectGV, giangVienId, false)

            // Tạo TestObject động: nút submit của form đó
            TestObject btnSubmit = new TestObject("btn_Submit_KH${khoaHocId}")
            btnSubmit.addProperty('xpath', ConditionType.EQUALS,
                "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//button[@type='submit']")

            WebUI.click(btnSubmit)
            WebUI.waitForPageLoad(timeout)

            // Controller luôn redirect về PhanCong với TempData["Success"]
            boolean hasSuccess = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_PhanCong/div_AlertSuccess'),
                timeout, FailureHandling.OPTIONAL)

            ketQua = hasSuccess ? 'PASS' : 'FAIL'
            WebUI.comment(ketQua == 'PASS'
                ? "PASS: Phân công KH${khoaHocId} → GV${giangVienId} thành công"
                : "FAIL: Không thấy alert thành công tại ${WebUI.getUrl()}")
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
    wbW.getSheetAt(0).getRow(i).createCell(6).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC06 Hoàn tất ===")
