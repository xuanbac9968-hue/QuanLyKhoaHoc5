# CHƯƠNG 1: CƠ SỞ LÝ THUYẾT VỀ KIỂM THỬ PHẦN MỀM

---

## 1.1 TỔNG QUAN VỀ KIỂM THỬ PHẦN MỀM

### 1.1.1 Định nghĩa và vai trò

Kiểm thử phần mềm (Software Testing) là quá trình thực thi một chương trình hoặc hệ thống với mục tiêu phát hiện các lỗi, đảm bảo rằng sản phẩm đáp ứng các yêu cầu đã đặt ra và hoạt động đúng như mong đợi. Theo tiêu chuẩn IEEE 829, kiểm thử là "quá trình vận hành một hệ thống hoặc thành phần trong những điều kiện xác định, quan sát hoặc ghi lại kết quả, và đánh giá một số khía cạnh của hệ thống hoặc thành phần đó".

Trong vòng đời phát triển phần mềm (SDLC), kiểm thử đóng vai trò then chốt:

- **Đảm bảo chất lượng (QA):** Phát hiện và ngăn ngừa lỗi trước khi phần mềm đến tay người dùng cuối
- **Xác minh và kiểm định (V&V):** Xác minh rằng hệ thống được xây dựng đúng cách (Verification) và kiểm định rằng hệ thống đúng là thứ người dùng cần (Validation)
- **Giảm thiểu rủi ro:** Chi phí sửa lỗi sau khi phát hành cao hơn 100 lần so với phát hiện trong giai đoạn phát triển (theo NIST)
- **Xây dựng niềm tin:** Cung cấp bằng chứng khách quan về chất lượng hệ thống

### 1.1.2 Nguyên tắc cơ bản của kiểm thử

Bảy nguyên tắc kiểm thử phần mềm được ISTQB (International Software Testing Qualifications Board) công nhận:

1. **Kiểm thử phát hiện lỗi, không chứng minh không có lỗi:** Dù không tìm thấy lỗi, không có nghĩa phần mềm hoàn hảo
2. **Kiểm thử toàn diện là không thể:** Với đầu vào vô hạn, chỉ có thể kiểm thử một tập con đại diện
3. **Kiểm thử sớm tiết kiệm chi phí:** Phát hiện lỗi sớm trong SDLC ít tốn kém hơn
4. **Lỗi phân bố không đều (Defect Clustering):** 80% lỗi thường tập trung trong 20% module
5. **Nghịch lý thuốc trừ sâu:** Lặp lại cùng một bộ test case sẽ mất tác dụng theo thời gian
6. **Kiểm thử phụ thuộc ngữ cảnh:** Không có phương pháp kiểm thử duy nhất phù hợp cho mọi hệ thống
7. **Sự hiểu lầm về "không có lỗi":** Hệ thống không có lỗi không có giá trị nếu không đáp ứng nhu cầu người dùng

---

## 1.2 CÁC CẤP ĐỘ KIỂM THỬ

### 1.2.1 Kiểm thử đơn vị (Unit Testing)

Kiểm thử đơn vị kiểm tra từng thành phần nhỏ nhất của phần mềm – thường là một hàm, một phương thức hoặc một lớp – một cách độc lập. Mục tiêu là xác nhận rằng mỗi đơn vị hoạt động như thiết kế.

**Đặc điểm:**
- Thực hiện bởi lập trình viên
- Chạy nhanh, tự động hoá 100%
- Sử dụng framework: xUnit, NUnit (C#), JUnit (Java), Pytest (Python)
- Dùng mock/stub để cô lập dependency

**Ứng dụng trong QuanLyKhoaHoc5:**
- Kiểm thử công thức tính điểm: `Diem.TinhTongKet(giuaKy, cuoiKy)` = GK×30% + CK×70%
- Kiểm thử logic phân loại: `Diem.TinhXepLoai(tongKet)` → Giỏi/Khá/Trung bình/Yếu
- Kiểm thử helper tạo mã: `GenerateMaHocVienAsync()` → "HV001", "HV002",...

### 1.2.2 Kiểm thử tích hợp (Integration Testing)

Kiểm thử tích hợp xác minh rằng các module hoặc component hoạt động đúng khi kết hợp với nhau.

**Các chiến lược:**
- **Big Bang:** Tích hợp tất cả cùng lúc rồi test
- **Top-down:** Test từ module cấp cao nhất xuống
- **Bottom-up:** Test từ module cơ bản nhất lên
- **Sandwich:** Kết hợp top-down và bottom-up

**Ứng dụng trong QuanLyKhoaHoc5:**
- Kiểm thử luồng Đăng ký → Duyệt → Tạo Điểm: Đảm bảo khi Admin duyệt DangKy, bản ghi Diem được tạo tự động
- Kiểm thử luồng TaoTaiKhoan → TaoHocVien/GiangVien: Khi tạo tài khoản với vaiTro=HocVien, record HocVien được tạo
- Kiểm thử tích hợp Controller-Service-Database: ThongBaoService.TaoThongBaoAsync() được gọi đúng lúc

### 1.2.3 Kiểm thử hệ thống (System Testing)

Kiểm thử hệ thống kiểm tra toàn bộ hệ thống đã tích hợp đầy đủ, so sánh với các yêu cầu đặc tả.

**Bao gồm:**
- Kiểm thử chức năng (Functional Testing)
- Kiểm thử phi chức năng (Performance, Security, Usability)
- Kiểm thử hồi quy (Regression Testing)

### 1.2.4 Kiểm thử chấp nhận (Acceptance Testing)

Kiểm thử chấp nhận do người dùng cuối hoặc khách hàng thực hiện, xác nhận hệ thống đáp ứng yêu cầu nghiệp vụ.

- **UAT (User Acceptance Testing):** Người dùng thực tế test trực tiếp
- **Alpha/Beta Testing:** Test nội bộ/mở rộng trước khi phát hành chính thức

---

## 1.3 CÁC LOẠI KIỂM THỬ THEO KỸ THUẬT

### 1.3.1 Kiểm thử hộp đen (Black-box Testing)

Kiểm thử hộp đen không quan tâm đến cấu trúc bên trong của hệ thống, chỉ kiểm tra đầu vào và đầu ra dựa trên đặc tả yêu cầu.

**Kỹ thuật:**
- **Phân lớp tương đương (Equivalence Partitioning):** Chia dữ liệu đầu vào thành các nhóm hành xử giống nhau. Ví dụ: mật khẩu ≥6 ký tự (hợp lệ) và <6 ký tự (không hợp lệ)
- **Phân tích giá trị biên (Boundary Value Analysis):** Test tại ranh giới của các lớp. Ví dụ: điểm = 0, 5, 6.9, 7.0, 8.4, 8.5, 10
- **Bảng quyết định (Decision Table):** Mô hình hoá các kết hợp điều kiện phức tạp
- **Kiểm thử trạng thái (State Transition Testing):** Test các chuyển đổi trạng thái hợp lệ và không hợp lệ. Ví dụ: KhoaHoc: DangMo→DaDong→TamDung→DangMo

**Ứng dụng trong hệ thống:**
- Đăng nhập: Test đúng/sai email, đúng/sai mật khẩu, tài khoản bị khóa
- Tạo tài khoản: Validation email format, mật khẩu ≥6 ký tự, email unique
- Đăng ký khóa học: Lớp đầy/còn chỗ, đã đăng ký/chưa, lớp không mở

### 1.3.2 Kiểm thử hộp trắng (White-box Testing)

Kiểm thử hộp trắng (còn gọi là glass-box hay structural testing) dựa trên kiến thức về cấu trúc code bên trong.

**Tiêu chí phủ sóng:**
- **Phủ sóng câu lệnh (Statement Coverage):** Mỗi dòng code phải được thực thi ít nhất 1 lần
- **Phủ sóng nhánh (Branch Coverage):** Mỗi nhánh if/else phải được kiểm tra
- **Phủ sóng điều kiện (Condition Coverage):** Mỗi điều kiện boolean phải có cả giá trị true và false
- **Phủ sóng đường dẫn (Path Coverage):** Mọi đường dẫn logic phải được thực thi

**Ứng dụng trong QuanLyKhoaHoc5:**
- Kiểm tra nhánh trong `KhoaTaiKhoan`: id==me → trả về lỗi; id≠me → toggle IsActive
- Kiểm tra nhánh trong `SuaVaiTro`: GV có lớp → lỗi; HV có DangKy → lỗi; không có ràng buộc → đổi thành công
- Kiểm tra điều kiện trong `KhoaHoc/Index`: Admin → tất cả TT; non-Admin → chỉ DangMo

### 1.3.3 Kiểm thử hộp xám (Grey-box Testing)

Kết hợp black-box và white-box, kiểm thử viên có kiến thức một phần về cấu trúc nội bộ.

---

## 1.4 KIỂM THỬ TỰ ĐỘNG (AUTOMATED TESTING)

### 1.4.1 Tổng quan

Kiểm thử tự động sử dụng công cụ để thực thi test script và so sánh kết quả thực tế với kết quả mong đợi mà không cần can thiệp thủ công. Phù hợp nhất cho:

- **Regression Testing:** Tái chạy test sau mỗi thay đổi code
- **Performance Testing:** Giả lập nhiều người dùng đồng thời
- **Repetitive Tests:** Test cần chạy nhiều lần với dữ liệu khác nhau

### 1.4.2 Framework Playwright

**Playwright** là framework kiểm thử E2E (End-to-End) hiện đại do Microsoft phát triển, hỗ trợ Chrome, Firefox, Safari trên Windows/macOS/Linux.

**Ưu điểm so với Selenium WebDriver:**
| Tiêu chí | Playwright | Selenium |
|---------|-----------|---------|
| Tốc độ | Nhanh hơn (native browser API) | Chậm hơn (WebDriver protocol) |
| Độ ổn định | Auto-wait, ít flaky | Cần explicit wait |
| Setup | `npm install playwright` | Driver riêng mỗi browser |
| Network intercept | Built-in | Cần proxy ngoài |
| Screenshot | Dễ, multi-step | Phức tạp hơn |
| Ngôn ngữ | JS/TS/Python/C#/.NET | Java/Python/C#/Ruby/JS |

**Các tính năng Playwright sử dụng trong dự án:**
- `page.goto()` – Điều hướng trang
- `page.fill()` – Điền form
- `page.click()` – Click element
- `page.waitForSelector()` – Chờ element xuất hiện
- `page.waitForResponse()` – Chờ HTTP response
- `page.waitForFunction()` – Chờ điều kiện JS
- `page.evaluate()` – Chạy JavaScript trong browser
- `page.on('dialog', ...)` – Xử lý confirm/alert
- `ctx.newPage()` – Tạo page mới trong context

**Cấu trúc test trong dự án:**
```javascript
// Pattern chuẩn: Login → Navigate → Interact → Assert
const { browser, ctx } = await createBrowser();
await loginAs(ctx, 'admin');
const page = await ctx.newPage();
await page.goto(BASE + '/TaiKhoan');
const btn = await page.$('button:has-text("Tạo tài khoản mới")');
report('TC-046b', 'Nút tạo TK tồn tại', !!btn);
```

### 1.4.3 Công cụ JMeter cho kiểm thử tải

Apache JMeter là công cụ mã nguồn mở để kiểm thử hiệu năng và tải.

**Khái niệm cơ bản:**
- **Thread Group:** Nhóm luồng ảo, mô phỏng người dùng đồng thời
- **Ramp-up Time:** Thời gian để khởi động đủ số thread
- **Sampler:** Đơn vị yêu cầu (HTTP Request Sampler)
- **Listener:** Thu thập và hiển thị kết quả (Summary Report, View Results Tree)
- **Assertion:** Kiểm tra response (Status Code, Response Time, Content)
- **Timer:** Thêm độ trễ giữa các request (mô phỏng think time)

**Cấu hình trong dự án QuanLyKhoaHoc5:**
- TG1: 100 users, ramp 30s – Browse public pages
- TG2: 150 users, ramp 60s – HocVien authenticated flow
- TG3: 200 users, ramp 90s – Admin heavy operations

**Các chỉ số cần theo dõi:**
| Chỉ số | Định nghĩa | Mục tiêu (Non-functional) |
|--------|-----------|--------------------------|
| Response Time (Avg) | Thời gian phản hồi trung bình | < 3000ms |
| Throughput | Số request/giây | > 50 req/s |
| Error Rate | % request lỗi | < 1% |
| 90th Percentile | 90% request < X ms | < 5000ms |
| Latency | Thời gian đến byte đầu tiên | < 1000ms |

---

## 1.5 KIỂM THỬ BẢO MẬT (SECURITY TESTING)

### 1.5.1 Các lỗ hổng OWASP Top 10 liên quan

**A01 – Broken Access Control:** Hệ thống kiểm soát truy cập không đúng, người dùng truy cập resource không được phép. Trong QuanLyKhoaHoc5, attribute `[AuthorizeRole]` custom thực hiện kiểm soát này.

**A02 – Cryptographic Failures:** Dữ liệu nhạy cảm không được bảo vệ đúng cách. Hệ thống sử dụng BCrypt.Net-Next để hash mật khẩu, đây là phương pháp được khuyến nghị.

**A03 – SQL Injection:** Dữ liệu từ người dùng được nhúng trực tiếp vào câu truy vấn SQL. Sử dụng EF Core với LINQ queries loại bỏ hoàn toàn nguy cơ này.

**A05 – Security Misconfiguration:** Cấu hình bảo mật sai. Cần kiểm tra: HTTPS, cookie HttpOnly/Secure, CORS settings.

**A07 – Identification and Authentication Failures:** Xác thực yếu. Cần kiểm tra: brute force protection, session fixation, CSRF protection.

### 1.5.2 Anti-forgery Token (CSRF Protection)

ASP.NET Core MVC mặc định có cơ chế bảo vệ CSRF thông qua `[ValidateAntiForgeryToken]`. Tất cả các POST action trong QuanLyKhoaHoc5 đều sử dụng attribute này. Playwright test cần extract token từ form trước khi POST.

---

## 1.6 CI/CD VÀ TÍCH HỢP KIỂM THỬ

### 1.6.1 GitHub Actions

GitHub Actions là nền tảng CI/CD tích hợp vào GitHub, cho phép tự động hoá build, test, và deploy khi có code thay đổi.

**Cấu trúc workflow:**
```yaml
on: [push, pull_request]
jobs:
  build:
    steps:
      - checkout
      - setup dotnet
      - dotnet build
  test:
    needs: build
    steps:
      - start app
      - run playwright tests
```

**Pipeline của dự án:**
1. Push code → trigger workflow
2. Build .NET app → artifact
3. Start app (localhost:5299) + SQL Server LocalDB
4. Run 8 Playwright test files tuần tự
5. Upload kết quả, notify
6. (Optional) SonarQube analysis trên main branch

### 1.6.2 SonarQube

SonarQube là công cụ phân tích chất lượng code tĩnh (static analysis), phát hiện:
- **Bugs:** Code có thể gây ra lỗi runtime
- **Vulnerabilities:** Lỗ hổng bảo mật
- **Code Smells:** Code không dễ bảo trì
- **Duplications:** Code trùng lặp
- **Coverage:** Tỉ lệ code được test bao phủ

**Chỉ số Quality Gate của dự án:**
- Bugs: A
- Vulnerabilities: A  
- Code Smells: B hoặc tốt hơn
- Coverage: ≥ 70%
- Duplications: < 10%

---

## 1.7 CÁC CÔNG CỤ QUẢN LÝ KIỂM THỬ

### 1.7.1 Jira + Xray

Jira là công cụ quản lý dự án và theo dõi vấn đề (issue tracker). Xray là plugin quản lý kiểm thử trong Jira, cho phép:
- Tạo và quản lý Test Case
- Liên kết TC với Story/Bug
- Theo dõi Test Execution
- Báo cáo coverage

### 1.7.2 Katalon Studio

Katalon Studio là nền tảng kiểm thử tự động toàn diện, hỗ trợ Web, Mobile, API, Desktop. Đặc điểm:
- Record & Playback cho người mới bắt đầu
- Viết script Groovy/Java cho người chuyên sâu
- Tích hợp sẵn Selenium và Appium
- Báo cáo HTML/PDF tự động

### 1.7.3 OWASP ZAP

OWASP ZAP (Zed Attack Proxy) là công cụ kiểm thử bảo mật web mã nguồn mở. Trong kiểm thử QuanLyKhoaHoc5:
- Scan tự động tìm XSS, SQL Injection, CSRF
- Tích hợp vào CI/CD pipeline
- Báo cáo lỗ hổng theo mức độ nghiêm trọng

---

## 1.8 MÔ HÌNH KIỂM THỬ TRONG DỰ ÁN

### 1.8.1 Chiến lược kiểm thử tổng thể

Dự án QuanLyKhoaHoc5 áp dụng mô hình kiểm thử đa tầng:

```
                    ┌─────────────────────┐
                    │   Manual Testing     │  ← Exploratory, UAT
                    └─────────────────────┘
                  ┌───────────────────────────┐
                  │    Playwright E2E Tests    │  ← 55+ test cases, 8 modules
                  └───────────────────────────┘
                ┌─────────────────────────────────┐
                │     JMeter Load/Perf Tests       │  ← 100/150/200 users
                └─────────────────────────────────┘
              ┌───────────────────────────────────────┐
              │         SonarQube Static Analysis      │  ← Code quality
              └───────────────────────────────────────┘
```

### 1.8.2 Tiêu chí hoàn thành kiểm thử (Definition of Done)

Một Test Case được xem là **PASS** khi:
1. Kết quả thực tế khớp với kết quả mong đợi trong spec
2. Không có JavaScript error trong console
3. Response time < 3000ms (non-loaded)
4. Không có lỗi mạng (4xx/5xx ngoài dự kiến)

Toàn bộ Sprint được xem là **DONE** khi:
1. ≥95% test case Cao-ưu tiên PASS
2. ≥80% tổng test case PASS
3. 0 lỗi nghiêm trọng (Critical/Blocker) còn mở
4. Load test: Error rate < 1% ở 200 concurrent users
5. SonarQube Quality Gate: Green

---

## TÓM TẮT CHƯƠNG 1

Chương này đã trình bày:
- Định nghĩa, vai trò và 7 nguyên tắc cơ bản của kiểm thử phần mềm
- Bốn cấp độ kiểm thử: Unit → Integration → System → Acceptance
- Ba loại kiểm thử theo kỹ thuật: Black-box, White-box, Grey-box
- Framework Playwright và JMeter với ứng dụng cụ thể trong dự án
- Kiểm thử bảo mật theo chuẩn OWASP Top 10
- CI/CD pipeline với GitHub Actions và SonarQube
- Chiến lược kiểm thử đa tầng và tiêu chí hoàn thành

Chương 2 sẽ trình bày cụ thể kế hoạch và thiết kế kiểm thử cho hệ thống QuanLyKhoaHoc5, bao gồm đặc tả 55+ test case chi tiết và tổ chức các file script tự động.
