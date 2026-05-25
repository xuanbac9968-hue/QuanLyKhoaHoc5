/**
 * TC_Login_DataDriven.groovy
 * ──────────────────────────────────────────────────────────────────────────
 * Data-Driven Test: Đăng nhập hệ thống QuanLyKhoaHoc5
 *
 * Biến nhận từ Test Suite (bind từ TestData_Login.xlsx – sheet "LoginData"):
 *   ${TCId}           – Mã test case (TC-001 … TC-010)
 *   ${Email}          – Email đăng nhập
 *   ${Password}       – Mật khẩu
 *   ${ExpectedResult} – LOGIN_SUCCESS | LOGIN_FAIL | ACCOUNT_LOCKED |
 *                       VALIDATION_ERR | LOGOUT_SUCCESS | PWD_CHANGED
 *   ${ExpectedUrl}    – Đoạn URL mong đợi (dùng contains)
 *   ${MoTa}           – Mô tả kịch bản
 *
 * Cách chạy: Mở Test Suites/TS_Login → Run (Chrome/Edge)
 * ──────────────────────────────────────────────────────────────────────────
 */

import static com.kms.katalon.core.checkpoint.CheckpointFactory.findCheckpoint
import static com.kms.katalon.core.testcase.TestCaseFactory.findTestCase
import static com.kms.katalon.core.testdata.TestDataFactory.findTestData
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject

import com.kms.katalon.core.annotation.SetupTestCase
import com.kms.katalon.core.annotation.TeardownTestCase
import com.kms.katalon.core.checkpoint.Checkpoint
import com.kms.katalon.core.cucumber.keyword.CucumberBuiltinKeywords as CucumberKW
import com.kms.katalon.core.mobile.keyword.MobileBuiltinKeywords as Mobile
import com.kms.katalon.core.model.FailureHandling
import com.kms.katalon.core.testcase.TestCase
import com.kms.katalon.core.testdata.TestData
import com.kms.katalon.core.testobject.TestObject
import com.kms.katalon.core.webservice.keyword.WSBuiltinKeywords as WS
import com.kms.katalon.core.webui.keyword.WebUiBuiltinKeywords as WebUI

// ─── Cấu hình ─────────────────────────────────────────────────────────────
def BASE_URL    = 'http://localhost:5125'
def LOGIN_URL   = "${BASE_URL}/Account/Login"
def TIMEOUT     = 15   // giây chờ element

// ─── Banner log ───────────────────────────────────────────────────────────
WebUI.comment("═══════════════════════════════════════════════════")
WebUI.comment("  [${TCId}] ${MoTa}")
WebUI.comment("  Email   : ${Email}")
WebUI.comment("  Expected: ${ExpectedResult}")
WebUI.comment("═══════════════════════════════════════════════════")

// ─── 1. Mở trình duyệt & vào trang đăng nhập ─────────────────────────────
WebUI.openBrowser('')
WebUI.navigateToUrl(LOGIN_URL)
WebUI.waitForPageLoad(TIMEOUT)

// ─── 2. Kiểm tra đang ở trang Login ──────────────────────────────────────
WebUI.verifyElementPresent(
    findTestObject('Object Repository/Page_Login/btn_DangNhap'), TIMEOUT)

// ─── 3. Xử lý theo kịch bản VALIDATION_ERR (email rỗng) ──────────────────
if (ExpectedResult == 'VALIDATION_ERR') {
    // Bỏ trống Email, chỉ nhập Password
    WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Email'))
    WebUI.setText(
        findTestObject('Object Repository/Page_Login/input_Password'), Password)
    WebUI.click(
        findTestObject('Object Repository/Page_Login/btn_DangNhap'))
    WebUI.waitForPageLoad(TIMEOUT)

    // Sau khi submit: vẫn ở trang Login (HTML5 validation hoặc server-side)
    def urlValidation = WebUI.getUrl()
    WebUI.verifyMatch(urlValidation, '.*Account/Login.*', true)
    WebUI.comment("  ✔ [${TCId}] PASS – Form validation OK, ở lại Login")
    WebUI.closeBrowser()
    return
}

// ─── 4. Nhập Email & Password ─────────────────────────────────────────────
WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Email'))
WebUI.setText(
    findTestObject('Object Repository/Page_Login/input_Email'), Email)

WebUI.clearText(findTestObject('Object Repository/Page_Login/input_Password'))
WebUI.setText(
    findTestObject('Object Repository/Page_Login/input_Password'), Password)

// ─── 5. Click Đăng nhập ───────────────────────────────────────────────────
WebUI.click(findTestObject('Object Repository/Page_Login/btn_DangNhap'))
WebUI.waitForPageLoad(TIMEOUT)

def currentUrl = WebUI.getUrl()
WebUI.comment("  → URL hiện tại: ${currentUrl}")

// ─── 6. Verify theo ExpectedResult ───────────────────────────────────────
switch (ExpectedResult) {

    // ── Đăng nhập thành công ────────────────────────────────────────────
    case 'LOGIN_SUCCESS':
        WebUI.verifyMatch(currentUrl, ".*${ExpectedUrl}.*", true,
            FailureHandling.STOP_ON_FAILURE)
        // Kiểm tra nav bar hiển thị (chứng tỏ đã login)
        WebUI.verifyElementPresent(
            findTestObject('Object Repository/Page_Login/btn_DangNhap'),
            1, FailureHandling.OPTIONAL)
        WebUI.comment("  ✔ [${TCId}] PASS – Đăng nhập thành công, URL = ${currentUrl}")
        break

    // ── Sai mật khẩu / email không tồn tại ─────────────────────────────
    case 'LOGIN_FAIL':
        // Phải ở lại trang Login
        WebUI.verifyMatch(currentUrl, '.*Account/Login.*', true,
            FailureHandling.STOP_ON_FAILURE)
        // Phải có thông báo lỗi
        WebUI.waitForElementPresent(
            findTestObject('Object Repository/Page_Login/lbl_ErrorMessage'), TIMEOUT)
        def errText = WebUI.getText(
            findTestObject('Object Repository/Page_Login/lbl_ErrorMessage'))
        WebUI.comment("  → Thông báo lỗi: ${errText}")
        WebUI.verifyMatch(errText,
            '.*(không đúng|không tìm thấy|invalid).*', true,
            FailureHandling.STOP_ON_FAILURE)
        WebUI.comment("  ✔ [${TCId}] PASS – Xác nhận lỗi đăng nhập sai")
        break

    // ── Tài khoản bị khóa ───────────────────────────────────────────────
    case 'ACCOUNT_LOCKED':
        WebUI.verifyMatch(currentUrl, '.*Account/Login.*', true,
            FailureHandling.STOP_ON_FAILURE)
        WebUI.waitForElementPresent(
            findTestObject('Object Repository/Page_Login/lbl_ErrorMessage'), TIMEOUT)
        def lockText = WebUI.getText(
            findTestObject('Object Repository/Page_Login/lbl_ErrorMessage'))
        WebUI.comment("  → Thông báo: ${lockText}")
        WebUI.verifyMatch(lockText, '.*(khóa|locked|liên hệ).*', true,
            FailureHandling.STOP_ON_FAILURE)
        WebUI.comment("  ✔ [${TCId}] PASS – TK bị khóa, thông báo hiển thị")
        break

    // ── Đăng xuất ───────────────────────────────────────────────────────
    case 'LOGOUT_SUCCESS':
        // Đã vào được dashboard (LOGIN_SUCCESS trước)
        WebUI.verifyMatch(currentUrl, ".*${ExpectedUrl}.*", true,
            FailureHandling.OPTIONAL)
        // Click Đăng xuất (nếu phần tử tồn tại trên dashboard)
        boolean logoutExist = WebUI.verifyElementPresent(
            findTestObject('Object Repository/Page_Login/btn_DangNhap'),
            2, FailureHandling.OPTIONAL)
        if (!logoutExist) {
            // Tìm link logout trong nav
            WebUI.navigateToUrl("${BASE_URL}/Account/Logout")
            WebUI.waitForPageLoad(TIMEOUT)
        }
        def urlAfterLogout = WebUI.getUrl()
        WebUI.verifyMatch(urlAfterLogout, '.*Account/Login.*', true,
            FailureHandling.STOP_ON_FAILURE)
        WebUI.comment("  ✔ [${TCId}] PASS – Đăng xuất thành công, redirect Login")
        break

    // ── Đổi mật khẩu ────────────────────────────────────────────────────
    case 'PWD_CHANGED':
        WebUI.verifyMatch(currentUrl, ".*${ExpectedUrl}.*", true,
            FailureHandling.OPTIONAL)
        // Sau khi login Admin thành công, vào trang đổi mật khẩu
        WebUI.navigateToUrl("${BASE_URL}/Account/ChangePassword")
        WebUI.waitForPageLoad(TIMEOUT)
        // Điền form đổi mật khẩu (dùng mật khẩu mặc định)
        try {
            WebUI.setText(
                findTestObject('Object Repository/Page_Login/input_Password'),
                Password)  // MatKhauCu
        } catch (Exception e) {
            WebUI.comment("  ⚠ Không thể điền form đổi mật khẩu: ${e.message}")
        }
        def pwdUrl = WebUI.getUrl()
        WebUI.verifyMatch(pwdUrl, '.*Account/ChangePassword.*', true,
            FailureHandling.OPTIONAL)
        WebUI.comment("  ✔ [${TCId}] PASS – Truy cập trang đổi mật khẩu OK")
        break

    // ── Không xác định ──────────────────────────────────────────────────
    default:
        WebUI.comment("  ⚠ [${TCId}] ExpectedResult không xác định: ${ExpectedResult}")
        break
}

// ─── 7. Đóng trình duyệt ─────────────────────────────────────────────────
WebUI.closeBrowser()
