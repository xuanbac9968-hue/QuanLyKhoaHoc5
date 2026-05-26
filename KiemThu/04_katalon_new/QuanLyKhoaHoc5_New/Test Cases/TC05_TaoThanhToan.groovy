import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC05 – Tạo yêu cầu thanh toán
 * Data-Driven: đọc từ TestData/TC05_TaoThanhToan.xlsx
 * Columns: 0=STT, 1=MoTa, 2=EmailHocVien, 3=MatKhauHocVien, 4=KhoaHocId,
 *          5=PhuongThuc, 6=GhiChu, 7=KetQuaMongDoi, 8=KetQua
 *
 * PhuongThuc: "TienMat" → chọn radio ptTienMat; "ChuyenKhoan" → chọn radio ptChuyenKhoan
 * URL tạo yêu cầu: /HocVien/ThanhToan/TaoYeuCau/{KhoaHocId}
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC05_TaoThanhToan.xlsx'
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
    String emailHV       = row.getCell(2)?.toString()?.trim() ?: ''
    String matKhauHV     = row.getCell(3)?.toString()?.trim() ?: ''
    String khoaHocId     = row.getCell(4)?.toString()?.trim() ?: ''
    String phuongThuc    = row.getCell(5)?.toString()?.trim() ?: ''
    String ghiChu        = row.getCell(6)?.toString()?.trim() ?: ''
    String ketQuaMong    = row.getCell(7)?.toString()?.trim() ?: ''

    // Loại bỏ .0 nếu Excel đọc số thành decimal (vd: "2.0" → "2")
    if (khoaHocId.endsWith('.0')) khoaHocId = khoaHocId.replace('.0', '')

    if (emailHV.isEmpty()) continue

    WebUI.comment("=== TC05 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Đăng nhập học viên
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), emailHV)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhauHV)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Vào trang tạo yêu cầu thanh toán
        WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/HocVien/ThanhToan/TaoYeuCau/' + khoaHocId)
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Kiểm tra có vào được trang không (nếu KhoaHocId không hợp lệ sẽ redirect)
        String currentUrl = WebUI.getUrl()
        if (!currentUrl.contains('/TaoYeuCau')) {
            if ('ThatBai'.equalsIgnoreCase(ketQuaMong)) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Redirect ra khỏi TaoYeuCau như mong đợi (KhoaHocId không hợp lệ)")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Bị redirect khi không nên")
            }
        } else {
            // Chọn phương thức thanh toán
            if ('TienMat'.equalsIgnoreCase(phuongThuc)) {
                WebUI.click(findTestObject('Object Repository/Page_ThanhToan/radio_TienMat'))
            } else if ('ChuyenKhoan'.equalsIgnoreCase(phuongThuc)) {
                WebUI.click(findTestObject('Object Repository/Page_ThanhToan/radio_ChuyenKhoan'))
            }

            // Ghi chú
            WebUI.clearText(findTestObject('Object Repository/Page_ThanhToan/textarea_GhiChu'))
            if (!ghiChu.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThanhToan/textarea_GhiChu'), ghiChu)
            }

            WebUI.click(findTestObject('Object Repository/Page_ThanhToan/btn_GuiYeuCau'))
            WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

            if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
                boolean hasSuccess = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Common/div_AlertSuccess'),
                    GlobalVariable.TIMEOUT as int,
                    FailureHandling.OPTIONAL
                )
                ketQua = hasSuccess ? 'PASS' : 'FAIL'
                WebUI.comment(ketQua == 'PASS' ? "PASS: Tạo yêu cầu thành công" : "FAIL: Không có thông báo thành công")
            } else {
                boolean hasError = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Common/div_AlertDanger'),
                    GlobalVariable.TIMEOUT as int,
                    FailureHandling.OPTIONAL
                )
                ketQua = hasError ? 'PASS' : 'FAIL'
                WebUI.comment(ketQua == 'PASS' ? "PASS: Tạo yêu cầu thất bại như mong đợi" : "FAIL: Không có thông báo lỗi")
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
WebUI.comment("=== TC05 Hoàn tất ===")
