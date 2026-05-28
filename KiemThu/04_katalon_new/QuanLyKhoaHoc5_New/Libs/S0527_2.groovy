import com.kms.katalon.core.logging.KeywordLogger
import com.kms.katalon.core.exception.StepFailedException
import com.kms.katalon.core.reporting.ReportUtil
import com.kms.katalon.core.main.TestCaseMain
import com.kms.katalon.core.testdata.TestDataColumn
import com.kms.katalon.core.testcase.TestCaseBinding
import com.kms.katalon.core.driver.internal.DriverCleanerCollector
import com.kms.katalon.core.model.FailureHandling
import com.kms.katalon.core.configuration.RunConfiguration
import static com.kms.katalon.core.testcase.TestCaseFactory.findTestCase
import static com.kms.katalon.core.testobject.ObjectRepository.findTestObject
import static com.kms.katalon.core.testdata.TestDataFactory.findTestData


import com.katalon.execution.application.ExecutionMain

Map<String, String> suiteProperties = new HashMap<String, String>();

suiteProperties.put('rerunTestFailImmediately', 'false')
suiteProperties.put('retryCount', '0')
suiteProperties.put('name', 'TS_All')
suiteProperties.put('description', 'Test Suite ch\u1EA1y to\u00E0n b\u1ED9 6 Test Case ki\u1EC3m th\u1EED t\u1EF1 \u0111\u1ED9ng h\u1EC7 th\u1ED1ng QuanLyKhoaHoc5')
suiteProperties.put('id', 'Test Suites/TS_All')
 

DriverCleanerCollector.getInstance().addDriverCleaner(new com.kms.katalon.core.webui.contribution.WebUiDriverCleaner())
DriverCleanerCollector.getInstance().addDriverCleaner(new com.kms.katalon.core.mobile.contribution.MobileDriverCleaner())
DriverCleanerCollector.getInstance().addDriverCleaner(new com.kms.katalon.core.cucumber.keyword.internal.CucumberDriverCleaner())
DriverCleanerCollector.getInstance().addDriverCleaner(new com.kms.katalon.core.windows.keyword.contribution.WindowsDriverCleaner())
DriverCleanerCollector.getInstance().addDriverCleaner(new com.kms.katalon.core.testng.keyword.internal.TestNGDriverCleaner())



RunConfiguration.setExecutionSettingFile("D:\\QuanLyKhoaHoc5\\KiemThu\\04_katalon_new\\QuanLyKhoaHoc5_New\\Reports\\20260527_100308\\TS_All\\20260527_100308\\execution.properties")

TestCaseMain.beforeStart()

new ExecutionMain().init();

TestCaseMain.startTestSuite('Test Suites/TS_All', suiteProperties, new File("D:\\QuanLyKhoaHoc5\\KiemThu\\04_katalon_new\\QuanLyKhoaHoc5_New\\Reports\\20260527_100308\\TS_All\\20260527_100308\\testCaseBinding"))
