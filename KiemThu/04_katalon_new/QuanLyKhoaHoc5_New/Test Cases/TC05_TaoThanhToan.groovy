import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC05 – Tạo yêu cầu thanh toán (HocVien)
 * FIX: URL đúng là /ThanhToan/TaoYeuCau/{khoaHocId}, không phải /HocVien/ThanhToan/...
 * Logic server: Sau khi submit thành công → redirect về /ThanhToan/CuaToi với TempData["Success"]
 *               Không hợp lệ (chưa duyệt, không tìm thấy KH) → redirect /ThanhToan/CuaToi với TempData["Error"]
 * Columns: 0=STT,1=MoTa,2=EmailHocVien,3=MatKhauHocVien,4=KhoaHocId,
 *          5=PhuongThuc,6=GhiChu,7=KetQuaMongDoi,8=KetQua
 * PhuongThuc: "TienMat" → radio#ptTienMat; "ChuyenKhoan" → radio#ptChuyenKhoan
 * Seed hợp lệ: hv01→KH2,KH3 | hv02→KH4,KH6 | hv03→KH6 (đều DaDuyet)
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC05_TaoThanhToan.xlsx'
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
    String emailHV    = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String matKhauHV  = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String khoaHocId  = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String phuongThuc = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''
    String ghiChu     = fmt.formatCellValue(row.getCell(6))?.trim() ?: ''
    String ketQuaMong = fmt.formatCellValue(row.getCell(7))?.trim() ?: ''

    // DataFormatter trả "2" cho số nguyên → không cần replace .0
    // Nhưng đề phòng: bỏ .0 nếu có
    khoaHocId = khoaHocId.replace('.0', '')

    if (emailHV.isEmpty()) continue

    WebUI.comment("=== TC05 Row ${stt}: ${moTa} ===")
    String ketQua = 'FAIL'

    try {
        // Đăng nhập học viên
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.waitForElementVisible(findTestObject('Object Repository/Page_Login/input_Email'), timeout)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), emailHV)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), matKhauHV)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(timeout)

        if (WebUI.getUrl().contains('/Account/Login')) {
            ketQua = 'ERROR: Đăng nhập thất bại với ' + emailHV
            WebUI.comment(ketQua)
        } else {
            // FIX: URL đúng là /ThanhToan/TaoYeuCau/{id}
            WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/ThanhToan/TaoYeuCau/' + khoaHocId)
            WebUI.waitForPageLoad(timeout)

            String currentUrl = WebUI.getUrl()
            boolean onTaoYeuCauPage = currentUrl.contains('/TaoYeuCau')

            if (!onTaoYeuCauPage) {
                // Server redirect về CuaToi – kiểm tra TempData["Error"] hoặc TempData["Warning"]
                if ('ThatBai'.equalsIgnoreCase(ketQuaMong)) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Redirect về ${currentUrl} như mong đợi (KH không hợp lệ/chưa duyệt)")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Bị redirect về ${currentUrl} khi nên vào được TaoYeuCau")
                }
            } else {
                // Đang ở trang TaoYeuCau → điền form
                WebUI.waitForElementVisible(
                    findTestObject('Object Repository/Page_ThanhToan/btn_GuiYeuCau'), timeout)

                if ('TienMat'.equalsIgnoreCase(phuongThuc)) {
                    WebUI.click(findTestObject('Object Repository/Page_ThanhToan/radio_TienMat'))
                } else if ('ChuyenKhoan'.equalsIgnoreCase(phuongThuc)) {
                    WebUI.click(findTestObject('Object Repository/Page_ThanhToan/radio_ChuyenKhoan'))
                }

                WebUI.clearText(findTestObject('Object Repository/Page_ThanhToan/textarea_GhiChu'))
                if (!ghiChu.isEmpty()) {
                    WebUI.setText(findTestObject('Object Repository/Page_ThanhToan/textarea_GhiChu'), ghiChu)
                }

                WebUI.click(findTestObject('Object Repository/Page_ThanhToan/btn_GuiYeuCau'))
                WebUI.waitForPageLoad(timeout)

                // Sau khi submit → redirect về /ThanhToan/CuaToi
                // ThanhCong: TempData["Success"] → .alert.alert-success
                // ThatBai: TempData["Error"]/"Warning" → .alert-danger hoặc không có alert-success
                if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
                    boolean hasSuccess = WebUI.verifyElementPresent(
                        findTestObject('Object Repository/Common/div_AlertSuccess'),
                        timeout, FailureHandling.OPTIONAL)
                    ketQua = hasSuccess ? 'PASS' : 'FAIL'
                    WebUI.comment(ketQua == 'PASS'
                        ? "PASS: Gửi yêu cầu thanh toán thành công"
                        : "FAIL: Không có alert thành công tại ${WebUI.getUrl()}")
                } else {
                    boolean hasError = WebUI.verifyElementPresent(
                        findTestObject('Object Repository/Common/div_AlertDanger'),
                        timeout, FailureHandling.OPTIONAL)
                    ketQua = hasError ? 'PASS' : 'FAIL'
                    WebUI.comment(ketQua == 'PASS'
                        ? "PASS: Yêu cầu thất bại như mong đợi"
                        : "FAIL: Không có alert lỗi")
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
WebUI.comment("=== TC05 Hoàn tất ===")
