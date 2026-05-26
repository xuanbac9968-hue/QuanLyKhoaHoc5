import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import com.kms.katalon.core.testobject.ConditionType
import com.kms.katalon.core.testobject.TestObjectProperty
import com.kms.katalon.core.testobject.TestObject
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC06 – Phân công giảng viên
 * Data-Driven: đọc từ TestData/TC06_PhanCong.xlsx
 * Columns: 0=STT, 1=MoTa, 2=KhoaHocId, 3=GiangVienId, 4=GhiChu, 5=KetQuaMongDoi, 6=KetQua
 *
 * URL: /Admin/PhanCong
 * GiangVienId = 0 → "Bỏ phân công"
 * Select giảng viên: XPath động //form[.//input[@name='KhoaHocId' and @value='X']]//select[@name='GiangVienId']
 * Kết quả: luôn ThanhCong nếu submit được (controller redirect về PhanCong với TempData["Success"])
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC06_PhanCong.xlsx'
FileInputStream fis = new FileInputStream(testDataPath)
Workbook wb = new XSSFWorkbook(fis)
Sheet sheet = wb.getSheetAt(0)
fis.close()

int totalRows = sheet.getLastRowNum()

for (int i = 1; i <= totalRows; i++) {
    Row row = sheet.getRow(i)
    if (row == null) continue

    String stt          = row.getCell(0)?.toString()?.trim() ?: ''
    String moTa         = row.getCell(1)?.toString()?.trim() ?: ''
    String khoaHocId    = row.getCell(2)?.toString()?.trim() ?: ''
    String giangVienId  = row.getCell(3)?.toString()?.trim() ?: ''
    String ghiChu       = row.getCell(4)?.toString()?.trim() ?: ''
    String ketQuaMong   = row.getCell(5)?.toString()?.trim() ?: ''

    // Loại bỏ .0 nếu Excel đọc số thành decimal
    if (khoaHocId.endsWith('.0')) khoaHocId = khoaHocId.replace('.0', '')
    if (giangVienId.endsWith('.0')) giangVienId = giangVienId.replace('.0', '')

    if (khoaHocId.isEmpty()) continue

    WebUI.comment("=== TC06 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Đăng nhập admin
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), GlobalVariable.ADMIN_EMAIL)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), GlobalVariable.ADMIN_PASS)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Vào trang phân công
        WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Admin/PhanCong')
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Tạo TestObject động cho select giảng viên của khóa học cụ thể
        TestObject selectGV = new TestObject('select_GiangVien_KH' + khoaHocId)
        selectGV.addProperty(
            'xpath',
            ConditionType.EQUALS,
            "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//select[@name='GiangVienId']"
        )

        // Chọn giảng viên
        WebUI.selectOptionByValue(selectGV, giangVienId, false)

        // Tạo TestObject động cho nút submit của form khóa học cụ thể
        TestObject btnSubmit = new TestObject('btn_Submit_KH' + khoaHocId)
        btnSubmit.addProperty(
            'xpath',
            ConditionType.EQUALS,
            "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//button[@type='submit']"
        )

        WebUI.click(btnSubmit)
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Kết quả: luôn có TempData["Success"] nếu submit thành công
        boolean hasSuccess = WebUI.verifyElementPresent(
            findTestObject('Object Repository/Page_PhanCong/div_AlertSuccess'),
            GlobalVariable.TIMEOUT as int,
            FailureHandling.OPTIONAL
        )

        ketQua = hasSuccess ? 'PASS' : 'FAIL'
        WebUI.comment(ketQua == 'PASS'
            ? "PASS: Phân công KH${khoaHocId} → GV${giangVienId} thành công"
            : "FAIL: Không thấy thông báo thành công")

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
    sheetW.getRow(i).createCell(6).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC06 Hoàn tất ===")
