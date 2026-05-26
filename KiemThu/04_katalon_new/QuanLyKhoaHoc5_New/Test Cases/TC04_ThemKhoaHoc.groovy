import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC04 – Thêm khóa học (Admin)
 * FIX: URL đúng là /KhoaHoc/Create (KhoaHocController), không phải /Admin/KhoaHoc/Create
 * Columns: 0=STT,1=MoTa,2=TenKhoaHoc,3=MoTaKhoaHoc,4=NgonNgu,5=TrinhDo,
 *          6=HocPhi,7=ThoiLuong,8=SoBuoiMoiTuan,9=ThoiGianMoiBuoi,10=TrangThai,
 *          11=KetQuaMongDoi,12=KetQua
 * NgonNgu chỉ có: "Tiếng Anh", "Tiếng Nhật"
 * TrangThai: DangMo / TamDung / DaDong
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC04_ThemKhoaHoc.xlsx'
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

    String stt             = fmt.formatCellValue(row.getCell(0))?.trim() ?: ''
    String moTa            = fmt.formatCellValue(row.getCell(1))?.trim() ?: ''
    String tenKhoaHoc      = fmt.formatCellValue(row.getCell(2))?.trim() ?: ''
    String moTaKH          = fmt.formatCellValue(row.getCell(3))?.trim() ?: ''
    String ngonNgu         = fmt.formatCellValue(row.getCell(4))?.trim() ?: ''
    String trinhDo         = fmt.formatCellValue(row.getCell(5))?.trim() ?: ''
    String hocPhi          = fmt.formatCellValue(row.getCell(6))?.trim() ?: ''
    String thoiLuong       = fmt.formatCellValue(row.getCell(7))?.trim() ?: ''
    String soBuoiMoiTuan   = fmt.formatCellValue(row.getCell(8))?.trim() ?: ''
    String thoiGianMoiBuoi = fmt.formatCellValue(row.getCell(9))?.trim() ?: ''
    String trangThai       = fmt.formatCellValue(row.getCell(10))?.trim() ?: ''
    String ketQuaMong      = fmt.formatCellValue(row.getCell(11))?.trim() ?: ''

    if (stt.isEmpty() && moTa.isEmpty()) continue

    WebUI.comment("=== TC04 Row ${stt}: ${moTa} ===")
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
            // FIX: URL đúng là /KhoaHoc/Create
            WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/KhoaHoc/Create')
            WebUI.waitForElementVisible(
                findTestObject('Object Repository/Page_ThemKhoaHoc/input_TenKhoaHoc'), timeout)

            // Điền form
            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_TenKhoaHoc'))
            if (!tenKhoaHoc.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_TenKhoaHoc'), tenKhoaHoc)
            }

            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/textarea_MoTa'))
            if (!moTaKH.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/textarea_MoTa'), moTaKH)
            }

            if (!ngonNgu.isEmpty()) {
                WebUI.selectOptionByLabel(
                    findTestObject('Object Repository/Page_ThemKhoaHoc/select_NgonNgu'),
                    ngonNgu, false)
            }

            if (!trinhDo.isEmpty()) {
                WebUI.selectOptionByValue(
                    findTestObject('Object Repository/Page_ThemKhoaHoc/select_TrinhDo'),
                    trinhDo, false)
            }

            // Clear số trước khi nhập (triple-click để select all rồi gõ)
            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_HocPhi'))
            if (!hocPhi.isEmpty()) {
                // Loại bỏ .0 từ DataFormatter cho số nguyên
                String hocPhiVal = hocPhi.replace('.0','').replace(',','')
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_HocPhi'), hocPhiVal)
            }

            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiLuong'))
            if (!thoiLuong.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiLuong'),
                    thoiLuong.replace('.0',''))
            }

            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_SoBuoiMoiTuan'))
            if (!soBuoiMoiTuan.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_SoBuoiMoiTuan'),
                    soBuoiMoiTuan.replace('.0',''))
            }

            WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiGianMoiBuoi'))
            if (!thoiGianMoiBuoi.isEmpty()) {
                WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiGianMoiBuoi'),
                    thoiGianMoiBuoi.replace('.0',''))
            }

            if (!trangThai.isEmpty()) {
                WebUI.selectOptionByValue(
                    findTestObject('Object Repository/Page_ThemKhoaHoc/select_TrangThai'),
                    trangThai, false)
            }

            WebUI.click(findTestObject('Object Repository/Page_ThemKhoaHoc/btn_TaoKhoaHoc'))
            WebUI.waitForPageLoad(timeout)

            String url = WebUI.getUrl()
            if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
                boolean hasSuccess = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Common/div_AlertSuccess'),
                    timeout, FailureHandling.OPTIONAL)
                if (hasSuccess || !url.contains('/Create')) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Tạo khóa học thành công → ${url}")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Không thấy thông báo thành công, vẫn ở ${url}")
                }
            } else {
                boolean hasError = WebUI.verifyElementPresent(
                    findTestObject('Object Repository/Page_ThemKhoaHoc/div_AlertDanger'),
                    timeout, FailureHandling.OPTIONAL)
                if (hasError || url.contains('/Create')) {
                    ketQua = 'PASS'
                    WebUI.comment("PASS: Validation thất bại như mong đợi")
                } else {
                    ketQua = 'FAIL'
                    WebUI.comment("FAIL: Tạo thành công khi không nên")
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
    wbW.getSheetAt(0).getRow(i).createCell(12).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC04 Hoàn tất ===")
