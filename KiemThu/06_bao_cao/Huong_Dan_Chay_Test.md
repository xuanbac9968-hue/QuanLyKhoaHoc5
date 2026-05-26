# HƯỚNG DẪN CHẠY KIỂM THỬ – QuanLyKhoaHoc5

> **Phiên bản:** 1.1 | **Cập nhật:** 26/05/2026

---

## MỤC LỤC

1. [Cài đặt môi trường](#1-cài-đặt-môi-trường)
2. [Khởi động ứng dụng](#2-khởi-động-ứng-dụng)
3. [Chạy Playwright (Automated E2E)](#3-chạy-playwright-automated-e2e)
4. [Chạy JMeter (Load Testing)](#4-chạy-jmeter-load-testing)
5. [Chạy Katalon Studio](#5-chạy-katalon-studio)
6. [Chạy OWASP ZAP (Security)](#6-chạy-owasp-zap-security)
7. [Chạy SonarQube (Code Quality)](#7-chạy-sonarqube-code-quality)
8. [Quản lý Jira + Xray](#8-quản-lý-jira--xray)
9. [CI/CD với GitHub Actions](#9-cicd-với-github-actions)
10. [Troubleshooting](#10-troubleshooting)

---

## 1. CÀI ĐẶT MÔI TRƯỜNG

### 1.1 Yêu cầu hệ thống

| Thành phần | Phiên bản yêu cầu | Kiểm tra |
|-----------|------------------|---------|
| OS | Windows 10/11 hoặc Ubuntu 20.04+ | – |
| .NET SDK | 10.0.x | `dotnet --version` |
| Node.js | 18.x hoặc 20.x | `node --version` |
| Java (JMeter) | 17+ (Temurin/OpenJDK) | `java --version` |
| SQL Server | 2019+ hoặc LocalDB | `SqlLocalDB info` |
| Git | 2.30+ | `git --version` |

### 1.2 Cài đặt .NET 10

```powershell
# Windows (winget)
winget install Microsoft.DotNet.SDK.10

# Ubuntu
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

### 1.3 Cài đặt Node.js

```powershell
# Windows (winget)
winget install OpenJS.NodeJS.LTS

# Ubuntu
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
```

### 1.4 Cài đặt Java (cho JMeter)

```powershell
# Windows
winget install EclipseAdoptium.Temurin.17.JDK

# Ubuntu
sudo apt install openjdk-17-jdk
```

---

## 2. KHỞI ĐỘNG ỨNG DỤNG

### 2.1 Lần đầu tiên

```powershell
# 1. Clone/mở project
cd D:\QuanLyKhoaHoc5\QuanLyKhoaHoc5.Web

# 2. Restore packages
dotnet restore

# 3. Áp dụng migration + seed data
dotnet ef database update
# Nếu chưa có migration:
dotnet ef migrations add InitialCreate
dotnet ef database update

# 4. Chạy ứng dụng
dotnet run
# App sẽ chạy tại: http://localhost:5125
```

### 2.2 Chạy thông thường

```powershell
cd D:\QuanLyKhoaHoc5\QuanLyKhoaHoc5.Web
dotnet run --no-launch-profile
```

### 2.3 Kiểm tra app đã sẵn sàng

```powershell
# Mở trình duyệt hoặc:
curl -o /dev/null -s -w "%{http_code}" http://localhost:5125/Account/Login
# Phải trả về: 200
```

### 2.4 Dừng ứng dụng

```powershell
# Ctrl+C trong terminal đang chạy
# Hoặc tìm và kill process:
Get-Process | Where-Object {$_.Name -like "*QuanLy*"} | Stop-Process -Force
```

---

## 3. CHẠY PLAYWRIGHT (AUTOMATED E2E)

### 3.1 Cài đặt lần đầu

```powershell
cd D:\QuanLyKhoaHoc5\KiemThu\02_selenium_scripts

# Cài npm packages
npm install

# Cài Playwright browsers
npx playwright install chromium
# Nếu cần full browsers:
npx playwright install
```

### 3.2 Chạy toàn bộ test suite

```powershell
# Đảm bảo app đang chạy tại :5125
npm test
# Hoặc:
node tests/run_all.js
```

**Output mẫu:**
```
╔══════════════════════════════════════════════════════════╗
║   QuanLyKhoaHoc5 – Playwright Automated Test Suite       ║
╚══════════════════════════════════════════════════════════╝

▶ Running: 01_login_test.js
══════════ MODULE 1: ĐĂNG NHẬP / ĐĂNG XUẤT ══════════
[TC-001] ✅ Admin đăng nhập → redirect /Admin | URL: http://localhost:5125/Admin
[TC-009] ✅ Đăng xuất → redirect Login
...
✅ PASSED: 01_login_test.js

▶ Running: 07_tai_khoan_test.js
[TC-046a] ✅ Trang /TaiKhoan tải OK
[TC-046b] ✅ Nút "Tạo tài khoản mới" tồn tại
...

FINAL RESULT: ✅ ALL PASSED
Total time: 45.3s
```

### 3.3 Chạy từng module riêng

```powershell
npm run test:login      # Module đăng nhập
npm run test:khoahoc    # Module khóa học
npm run test:lophoc     # Module lớp học
npm run test:dangky     # Module đăng ký
npm run test:diem       # Module điểm số
npm run test:thanhtoan  # Module thanh toán
npm run test:taikhoan   # Module tài khoản
npm run test:phancong   # Module phân công
```

### 3.4 Chạy với browser hiển thị (debug mode)

```powershell
# Sửa config.js: headless: false, slowMo: 500
# Sau đó chạy:
node tests/07_tai_khoan_test.js
```

### 3.5 Chụp screenshot khi thất bại

Thêm vào test file:
```javascript
// Sau khi assert fail:
await page.screenshot({ path: `screenshots/fail_${Date.now()}.png`, fullPage: true });
```

---

## 4. CHẠY JMETER (LOAD TESTING)

### 4.1 Cài đặt JMeter

```powershell
# 1. Download JMeter 5.6.3+
# https://jmeter.apache.org/download_jmeter.cgi

# 2. Giải nén vào D:\tools\jmeter

# 3. Thêm vào PATH (PowerShell):
$env:PATH += ";D:\tools\jmeter\bin"

# 4. Kiểm tra:
jmeter --version
```

### 4.2 Chạy giao diện (GUI Mode) – dùng để thiết kế

```powershell
# GUI mode (chỉ để debug, không dùng cho load test thật)
jmeter -t D:\QuanLyKhoaHoc5\KiemThu\04_jmeter\QuanLyKhoaHoc_LoadTest.jmx
```

### 4.3 Chạy command line (Non-GUI) – production

```powershell
cd D:\QuanLyKhoaHoc5\KiemThu\04_jmeter

# Chạy full test, lưu kết quả:
jmeter -n `
  -t QuanLyKhoaHoc_LoadTest.jmx `
  -l results\result_$(Get-Date -Format 'yyyyMMdd_HHmmss').jtl `
  -e `
  -o results\html_report_$(Get-Date -Format 'yyyyMMdd_HHmmss')

# Xem báo cáo HTML:
Start-Process results\html_report_*\index.html
```

### 4.4 Cấu hình tham số test

Sửa các biến trong Test Plan → User Defined Variables:
```
BASE_URL = localhost
PORT = 5125
PROTOCOL = http
ADMIN_EMAIL = admin@nnl.com
ADMIN_PW = Admin@123
HV_EMAIL = hv01@nnl.com
HV_PW = Hv@123
```

### 4.5 Giải thích 3 Thread Groups

| Thread Group | Users | Ramp-up | Loop | Mục tiêu |
|-------------|-------|---------|------|---------|
| TG1 - Browse Public | 100 | 30s | 5 | Giả lập 100 người duyệt KH |
| TG2 - HV Auth Flow | 150 | 60s | 3 | 150 HV đăng nhập + dùng app |
| TG3 - Admin Heavy | 200 | 90s | 2 | 200 Admin CRUD đồng thời |

### 4.6 Đọc kết quả

**Summary Report:**
- **Average:** Thời gian phản hồi trung bình → mục tiêu < 3000ms
- **90% Line:** 90% request nhanh hơn con số này → mục tiêu < 5000ms
- **Error %:** Tỉ lệ request lỗi → mục tiêu < 1%
- **Throughput:** Số request/giây → mục tiêu > 50 req/s

---

## 5. CHẠY KATALON STUDIO (DATA-DRIVEN)

Dự án Katalon đã có sẵn tại: `KiemThu\03_katalon\QuanLyKhoaHoc5\`  
Script data-driven (Groovy): `KiemThu\Scripts\TC01_DangNhap.groovy` … `TC06_PhanCongGiangVien.groovy`  
Dữ liệu Excel: `KiemThu\TestData\TC01_DangNhap.xlsx` … `TC06_PhanCong.xlsx`

### 5.1 Cài đặt

1. Download **Katalon Studio** (Free) từ https://katalon.com/katalon-studio
2. Giải nén → chạy `katalon.exe`
3. Đăng nhập tài khoản Katalon (free tier)
4. Mở project có sẵn: **File → Open Project** → `D:\QuanLyKhoaHoc5\KiemThu\03_katalon\QuanLyKhoaHoc5`

### 5.2 Chuẩn bị dữ liệu kiểm thử

Dữ liệu Excel đã được sinh sẵn bằng **GenTestData**:

```powershell
# Chỉ cần chạy 1 lần (hoặc chạy lại để reset dữ liệu)
cd D:\QuanLyKhoaHoc5\KiemThu\TestData\GenTestData
dotnet run

# Kết quả: 6 file Excel tại D:\QuanLyKhoaHoc5\KiemThu\TestData\
#   TC01_DangNhap.xlsx        (8 test cases)
#   TC02_DoiMatKhau.xlsx      (6 test cases)
#   TC03_TaoTaiKhoan.xlsx     (7 test cases)
#   TC04_ThemKhoaHoc.xlsx     (6 test cases)
#   TC05_TaoThanhToan.xlsx    (6 test cases)
#   TC06_PhanCong.xlsx        (8 test cases)
```

### 5.3 Chạy từng Test Case script (cách thủ công)

Trong Katalon Studio, mỗi script là một **Test Case** riêng:

| Script | Chức năng | Excel data |
|--------|-----------|-----------|
| TC01_DangNhap | Đăng nhập / Đăng xuất | TC01_DangNhap.xlsx |
| TC02_DoiMatKhau | Đổi mật khẩu | TC02_DoiMatKhau.xlsx |
| TC03_TaoTaiKhoan | Tạo tài khoản (Admin) | TC03_TaoTaiKhoan.xlsx |
| TC04_ThemKhoaHoc | Thêm khóa học (Admin) | TC04_ThemKhoaHoc.xlsx |
| TC05_TaoThanhToan | Tạo yêu cầu thanh toán (HV) | TC05_TaoThanhToan.xlsx |
| TC06_PhanCongGiangVien | Phân công giảng viên (Admin) | TC06_PhanCong.xlsx |

**Cách chạy:**
1. Mở Katalon Studio → Test Explorer → **Test Cases**
2. Import các file `.groovy` từ `KiemThu\Scripts\` vào Katalon project
3. Đảm bảo app đang chạy tại **http://localhost:5125**
4. Click chuột phải vào Test Case → **Run** → chọn **Chrome**
5. Cột **KetQua** trong file Excel sẽ tự động điền `Pass` / `Fail – <note>`

**Chạy tuần tự tất cả (Test Suite):**
1. New → Test Suite
2. Add Test Cases: TC01 → TC02 → TC03 → TC04 → TC05 → TC06
3. **Run** → Chrome

### 5.4 Chạy Groovy script trực tiếp (không cần Katalon GUI)

```powershell
# Yêu cầu: Katalon đã cài và thêm vào PATH
# Hoặc dùng Katalon Runtime Engine (CLI)
katalonc -projectPath="D:\QuanLyKhoaHoc5\KiemThu\03_katalon\QuanLyKhoaHoc5" ^
         -testSuitePath="Test Suites/TS_Login" ^
         -browserType="Chrome" ^
         -executionProfile="default"
```

### 5.5 Đọc kết quả trong Excel

Sau khi chạy, mở file Excel:
- Cột **KetQua** = `Pass` → test case thành công ✅
- Cột **KetQua** = `Fail – <lý do>` → test case thất bại ❌
- Log chi tiết xem trong Katalon Reports → **Reports** folder

### 5.6 Cấu hình quan trọng trong các Scripts

```groovy
// Tất cả script đều dùng:
final String BASE_URL   = 'http://localhost:5125'
final String EXCEL_PATH = 'D:\\QuanLyKhoaHoc5\\KiemThu\\TestData\\TC0X_*.xlsx'
final int    TIMEOUT    = 10  // giây

// Tài khoản admin (TC03, TC04, TC06):
final String ADMIN_EMAIL = 'admin@nnl.com'
final String ADMIN_PASS  = 'Admin@123'
```

**Lưu ý:** Nếu đổi port, sửa `BASE_URL` trong tất cả 6 file `.groovy`.

### 5.7 Ghi chú đặc biệt cho từng TC

| TC | Lưu ý |
|----|-------|
| TC01 | Script bỏ qua hàng có Email rỗng (`continue`). |
| TC02 | Row 4 đổi MK hv01, Row 5 đổi lại → cần chạy theo thứ tự. |
| TC03 | Tạo account mới → chạy lần 2 sẽ fail "email đã tồn tại" ở row 1-2, 6. |
| TC04 | Row 5 có TenKhoaHoc > 200 ký tự để test MaxLength validation. |
| TC05 | Chạy lần 2 có thể fail row 1-3 vì đã tạo ThanhToan (trùng). |
| TC06 | GiangVienId=0 = bỏ phân công (nếu select có option value="0"). |

---

## 6. CHẠY OWASP ZAP (SECURITY TESTING)

### 6.1 Cài đặt ZAP

```powershell
# Windows
winget install OWASP.ZAP

# Ubuntu
sudo apt install zaproxy
```

### 6.2 Chạy Automated Scan

1. Mở ZAP Desktop
2. **Quick Start** → **Automated Scan**
3. URL to attack: `http://localhost:5125`
4. Click **Attack**
5. ZAP sẽ:
   - Spider – khám phá tất cả URL
   - Active Scan – tấn công thử từng endpoint

### 6.3 Chạy từ command line (CI/CD)

```powershell
# Baseline scan (passive, không destructive)
zap-baseline.py -t http://localhost:5125 -r zap_report.html

# Full scan (active + passive)
zap-full-scan.py -t http://localhost:5125 -r zap_full_report.html
```

### 6.4 Đọc báo cáo

ZAP phân loại theo mức độ:
- 🔴 **High:** Cần fix ngay (XSS, SQL Injection, Auth bypass)
- 🟡 **Medium:** Nên fix (Missing headers, weak crypto)
- 🟢 **Low:** Informational (Server version disclosure)

**Expected findings cho QuanLyKhoaHoc5 (acceptable):**
- Server: X-Frame-Options header có thể thiếu
- Server: CSP header chưa cấu hình
- Low: Version disclosure trong headers

**Must NOT have:**
- XSS, CSRF bypass, SQL Injection, Auth bypass

---

## 7. CHẠY SONARQUBE (CODE QUALITY)

### 7.1 Cài đặt SonarQube Community

```powershell
# Download SonarQube Community Edition
# https://www.sonarqube.org/downloads/

# Giải nén vào D:\tools\sonarqube

# Khởi động:
D:\tools\sonarqube\bin\windows-x86-64\StartSonar.bat

# Mở: http://localhost:9000
# Default: admin/admin → đổi mật khẩu
```

### 7.2 Cài đặt SonarScanner

```powershell
# Cài dotnet-sonarscanner
dotnet tool install --global dotnet-sonarscanner

# Kiểm tra:
dotnet sonarscanner --version
```

### 7.3 Chạy phân tích

```powershell
cd D:\QuanLyKhoaHoc5

# 1. Bắt đầu phân tích
dotnet sonarscanner begin `
  /k:"QuanLyKhoaHoc5" `
  /d:sonar.host.url="http://localhost:9000" `
  /d:sonar.token="YOUR_TOKEN_HERE"

# 2. Build project
dotnet build QuanLyKhoaHoc5.Web/QuanLyKhoaHoc5.Web.csproj

# 3. Kết thúc phân tích
dotnet sonarscanner end /d:sonar.token="YOUR_TOKEN_HERE"

# 4. Xem kết quả:
# http://localhost:9000/dashboard?id=QuanLyKhoaHoc5
```

### 7.4 Cấu hình sonar-project.properties

```properties
# File: D:\QuanLyKhoaHoc5\sonar-project.properties
sonar.projectKey=QuanLyKhoaHoc5
sonar.projectName=Quản Lý Khóa Học 5
sonar.projectVersion=1.0
sonar.sources=QuanLyKhoaHoc5.Web
sonar.exclusions=**/wwwroot/lib/**,**/Migrations/**,**/obj/**,**/bin/**
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
sonar.coverage.exclusions=**/Migrations/**,**/Models/Entities/**
```

### 7.5 Quality Gate mục tiêu

| Metric | Mục tiêu |
|--------|---------|
| Bugs | 0 Critical/Blocker |
| Vulnerabilities | 0 High |
| Code Smells | < 50 |
| Duplicated Lines | < 10% |
| Coverage | ≥ 60% |

---

## 8. QUẢN LÝ JIRA + XRAY

### 8.1 Tạo Project trong Jira

1. Create Project → **Scrum** hoặc **Kanban**
2. Project name: `QuanLyKhoaHoc5 QA`
3. Key: `QLKH`

### 8.2 Import Test Cases (Xray)

1. Cài Xray app cho Jira
2. **Xray** → **Import Test Cases** → **CSV format**

**CSV mẫu:**
```csv
Summary,Description,Priority,Labels
"TC-001 Admin đăng nhập","Steps: ...",High,login
"TC-046 Tạo tài khoản","Steps: ...",High,taikhoan
```

### 8.3 Tạo Test Plan

1. **Xray** → **Test Plans** → **Create**
2. Thêm test cases vào plan
3. Assign cho tester

### 8.4 Chạy Test Execution

1. **Xray** → **Test Executions** → **Create**
2. Link với Test Plan
3. Update status cho từng TC: Pass/Fail/Skip/Executing
4. Attach screenshot khi fail

### 8.5 Theo dõi Coverage

**Xray Coverage Matrix** cho thấy:
- Story → TC coverage
- Epic → overall coverage %
- Sprint velocity của testing

---

## 9. CI/CD VỚI GITHUB ACTIONS

### 9.1 Cài đặt repository

```powershell
cd D:\QuanLyKhoaHoc5
git init
git add .
git commit -m "Initial commit with QA infrastructure"
git remote add origin https://github.com/your-org/QuanLyKhoaHoc5.git
git push -u origin main
```

### 9.2 Cấu hình Secrets

Trong GitHub repository → Settings → Secrets:
```
SONAR_TOKEN = sqa_xxxxxxxxxxxx
SONAR_HOST_URL = https://sonarcloud.io
```

### 9.3 Trigger workflow

Workflow chạy tự động khi:
- Push lên `main` hoặc `develop`
- Tạo Pull Request vào `main`
- Manual: Actions tab → **Run workflow** → chọn test suite

### 9.4 Xem kết quả

1. Mở repository trên GitHub
2. Click tab **Actions**
3. Chọn workflow run
4. Xem logs từng step
5. Download artifacts:
   - `published-app` – built app
   - `playwright-results` – test reports

### 9.5 Thông báo Slack (tùy chọn)

Thêm vào cuối workflow:
```yaml
- name: Slack notification
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
  if: always()
```

---

## 10. TROUBLESHOOTING

### 10.1 App không khởi động

```powershell
# Kiểm tra port
netstat -an | findstr 5125

# Kill process nếu port đang bị chiếm
Get-NetTCPConnection -LocalPort 5125 | ForEach-Object {
  Stop-Process -Id $_.OwningProcess -Force
}

# Kiểm tra connection string
cat QuanLyKhoaHoc5.Web/appsettings.json

# Rebuild
dotnet build --no-incremental
```

### 10.2 Playwright lỗi "Target closed"

```javascript
// Thêm waitForLoadState sau mỗi navigation
await page.goto(url, { waitUntil: 'domcontentloaded' });
await page.waitForLoadState('networkidle');

// Hoặc tăng timeout:
const page = await ctx.newPage();
page.setDefaultTimeout(30000);
```

### 10.3 Playwright lỗi CSRF

```javascript
// Đảm bảo lấy token từ trang HIỆN TẠI (sau khi page.goto)
const token = await page.$eval(
  'input[name="__RequestVerificationToken"]',
  el => el.value
);
```

### 10.4 JMeter lỗi "Out of Memory"

```powershell
# Tăng heap size trong jmeter.bat:
set HEAP=-Xms2g -Xmx4g -XX:MaxMetaspaceSize=512m
```

### 10.5 SonarQube không kết nối

```powershell
# Kiểm tra SonarQube đang chạy
curl http://localhost:9000/api/system/status

# Nếu chưa chạy:
D:\tools\sonarqube\bin\windows-x86-64\StartSonar.bat

# Đợi ~60 giây cho server khởi động xong
```

### 10.6 Lỗi thường gặp trong test

| Lỗi | Nguyên nhân | Fix |
|-----|------------|-----|
| `TimeoutError: Waiting for selector` | Element chưa render | Tăng timeout hoặc dùng `waitForSelector` |
| `page.$eval(...) threw` | Element không tồn tại | Thêm `.catch(() => null)` |
| `ERR_CONNECTION_REFUSED` | App chưa chạy | Khởi động app trước |
| `401 Unauthorized` | Chưa đăng nhập | Gọi `loginAs()` trước |
| `403 Forbidden` | Role không đủ quyền | Đăng nhập đúng role |
| `net::ERR_CERT_AUTHORITY_INVALID` | HTTPS self-signed cert | Dùng HTTP cho local test |

---

## PHỤ LỤC: CHECKLIST CHẠY FULL TEST

```
□ App đang chạy tại http://localhost:5125
□ Có thể truy cập /Account/Login (HTTP 200)
□ DB có seed data (3 users, ≥3 KH, ≥2 LH, ≥1 DangKy DaDuyet)
□ Node.js đã install: node --version
□ Playwright đã install: npx playwright --version
□ Playwright browsers đã cài: npx playwright install chromium

▶ Chạy Playwright:
  □ cd KiemThu/02_selenium_scripts
  □ npm test
  □ Kết quả: TẤT CẢ PASS hoặc ghi chép lỗi

▶ Chạy JMeter (optional):
  □ Java đã install: java --version
  □ JMeter đã install: jmeter --version
  □ jmeter -n -t 04_jmeter/QuanLyKhoaHoc_LoadTest.jmx -l results.jtl
  □ Error rate < 1%, avg response < 3000ms

▶ SonarQube (optional):
  □ SonarQube đang chạy tại :9000
  □ dotnet sonarscanner begin/build/end
  □ Quality Gate: Green

▶ OWASP ZAP (optional):
  □ zap-baseline.py -t http://localhost:5125
  □ 0 High alerts, ≤5 Medium alerts
```
