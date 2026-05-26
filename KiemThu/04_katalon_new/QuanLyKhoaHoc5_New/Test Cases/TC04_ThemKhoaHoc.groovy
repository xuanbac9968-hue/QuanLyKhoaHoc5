import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import com.kms.katalon.core.webui.keyword.WebUiBuiltInKeywords as WebUI
import com.kms.katalon.core.model.FailureHandling
import org.apache.poi.ss.usermodel.*
import org.apache.poi.xssf.usermodel.XSSFWorkbook
import java.io.FileInputStream
import java.io.FileOutputStream

/**
 * TC04 – Thêm khóa học
 * Data-Driven: đọc từ TestData/TC04_ThemKhoaHoc.xlsx
 * Columns: 0=STT, 1=MoTa, 2=TenKhoaHoc, 3=MoTaKhoaHoc, 4=NgonNgu, 5=TrinhDo,
 *          6=HocPhi, 7=ThoiLuong, 8=SoBuoiMoiTuan, 9=ThoiGianMoiBuoi,
 *          10=TrangThai, 11=KetQuaMongDoi, 12=KetQua
 *
 * Lưu ý: Script đăng nhập admin → vào /Admin/KhoaHoc/Create → điền form
 *        NgonNgu: "Tiếng Anh" hoặc "Tiếng Nhật" (không có Tiếng Hàn trong select)
 *        TrangThai: DangMo / TamDung / DaDong
 */

String testDataPath = GlobalVariable.TEST_DATA_DIR + '\\TC04_ThemKhoaHoc.xlsx'
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
    String tenKhoaHoc      = row.getCell(2)?.toString()?.trim() ?: ''
    String moTaKH          = row.getCell(3)?.toString()?.trim() ?: ''
    String ngonNgu         = row.getCell(4)?.toString()?.trim() ?: ''
    String trinhDo         = row.getCell(5)?.toString()?.trim() ?: ''
    String hocPhi          = row.getCell(6)?.toString()?.trim() ?: ''
    String thoiLuong       = row.getCell(7)?.toString()?.trim() ?: ''
    String soBuoiMoiTuan   = row.getCell(8)?.toString()?.trim() ?: ''
    String thoiGianMoiBuoi = row.getCell(9)?.toString()?.trim() ?: ''
    String trangThai       = row.getCell(10)?.toString()?.trim() ?: ''
    String ketQuaMong      = row.getCell(11)?.toString()?.trim() ?: ''

    if (tenKhoaHoc.isEmpty() && moTa.isEmpty()) continue

    WebUI.comment("=== TC04 Row ${stt}: ${moTa} ===")

    String ketQua = 'FAIL'
    try {
        // Đăng nhập admin
        WebUI.openBrowser(GlobalVariable.BASE_URL + '/Account/Login')
        WebUI.maximizeWindow()
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_Email'), GlobalVariable.ADMIN_EMAIL)
        WebUI.setText(findTestObject('Object Repository/Page_Login/input_MatKhau'), GlobalVariable.ADMIN_PASS)
        WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        // Vào trang tạo khóa học
        WebUI.navigateToUrl(GlobalVariable.BASE_URL + '/Admin/KhoaHoc/Create')
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

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
                ngonNgu,
                false
            )
        }

        if (!trinhDo.isEmpty()) {
            WebUI.selectOptionByValue(
                findTestObject('Object Repository/Page_ThemKhoaHoc/select_TrinhDo'),
                trinhDo,
                false
            )
        }

        WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_HocPhi'))
        if (!hocPhi.isEmpty()) {
            WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_HocPhi'), hocPhi)
        }

        WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiLuong'))
        if (!thoiLuong.isEmpty()) {
            WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiLuong'), thoiLuong)
        }

        WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_SoBuoiMoiTuan'))
        if (!soBuoiMoiTuan.isEmpty()) {
            WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_SoBuoiMoiTuan'), soBuoiMoiTuan)
        }

        WebUI.clearText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiGianMoiBuoi'))
        if (!thoiGianMoiBuoi.isEmpty()) {
            WebUI.setText(findTestObject('Object Repository/Page_ThemKhoaHoc/input_ThoiGianMoiBuoi'), thoiGianMoiBuoi)
        }

        if (!trangThai.isEmpty()) {
            WebUI.selectOptionByValue(
                findTestObject('Object Repository/Page_ThemKhoaHoc/select_TrangThai'),
                trangThai,
                false
            )
        }

        WebUI.click(findTestObject('Object Repository/Page_ThemKhoaHoc/btn_TaoKhoaHoc'))
        WebUI.waitForPageLoad(GlobalVariable.TIMEOUT as int)

        if ('ThanhCong'.equalsIgnoreCase(ketQuaMong)) {
            // Kỳ vọng thành công: chuyển về danh sách hoặc có alert-success
            String url = WebUI.getUrl()
            boolean hasSuccess = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Common/div_AlertSuccess'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            if (hasSuccess || !url.contains('/Create')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Tạo khóa học thành công")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Không thấy thông báo thành công")
            }
        } else {
            // Kỳ vọng thất bại: còn ở trang Create hoặc có alert-danger
            boolean hasError = WebUI.verifyElementPresent(
                findTestObject('Object Repository/Page_ThemKhoaHoc/div_AlertDanger'),
                GlobalVariable.TIMEOUT as int,
                FailureHandling.OPTIONAL
            )
            String url = WebUI.getUrl()
            if (hasError || url.contains('/Create')) {
                ketQua = 'PASS'
                WebUI.comment("PASS: Tạo khóa học thất bại như mong đợi")
            } else {
                ketQua = 'FAIL'
                WebUI.comment("FAIL: Tạo khóa học thành công khi không nên")
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
    sheetW.getRow(i).createCell(12).setCellValue(ketQua)
    FileOutputStream fos = new FileOutputStream(testDataPath)
    wbW.write(fos)
    fos.close()
    wbW.close()
}

wb.close()
WebUI.comment("=== TC04 Hoàn tất ===")
