# CHƯƠNG 2: KẾ HOẠCH VÀ THIẾT KẾ KIỂM THỬ HỆ THỐNG QUANLYKHOAHOC5

---

## 2.1 GIỚI THIỆU HỆ THỐNG

### 2.1.1 Mô tả tổng quan

**QuanLyKhoaHoc5** là hệ thống quản lý trung tâm ngoại ngữ xây dựng bằng ASP.NET Core MVC (.NET 10) với các đặc điểm kỹ thuật:

| Thành phần | Công nghệ |
|-----------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 9 |
| Database | SQL Server (LocalDB cho dev) |
| Authentication | Cookie Authentication ("CookieAuth") |
| Password Hashing | BCrypt.Net-Next |
| Excel Export | ClosedXML |
| PDF Export | iTextSharp |
| AI Integration | Groq API / Gemini API |
| Frontend | Bootstrap 5, Bootstrap Icons |
| Testing | Playwright 1.44+, JMeter 5.6+ |

### 2.1.2 Kiến trúc hệ thống

```
┌──────────────────────────────────────────────────────┐
│                    Client Browser                      │
│                (Bootstrap 5 + Vanilla JS)              │
└──────────────────────┬───────────────────────────────┘
                       │ HTTP/HTTPS
┌──────────────────────▼───────────────────────────────┐
│              ASP.NET Core MVC (.NET 10)               │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────┐  │
│  │ Controllers  │  │    Views     │  │   Filters  │  │
│  │ (19 files)   │  │  (Razor)     │  │ AuthorizeR │  │
│  └──────────────┘  └──────────────┘  └────────────┘  │
│  ┌──────────────────────────────────────────────────┐ │
│  │              Services Layer                       │ │
│  │  ThongBaoService | ExcelService | PdfService     │ │
│  │  LichHocHelper   | GoiYService                  │ │
│  └──────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────┐ │
│  │           Entity Framework Core (AppDbContext)   │ │
│  └──────────────────────────────────────────────────┘ │
└──────────────────────┬───────────────────────────────┘
                       │ SQL
┌──────────────────────▼───────────────────────────────┐
│                    SQL Server                          │
│  Tables: NguoiDung, HocVien, GiangVien, KhoaHoc,     │
│  LopHoc, DangKyKhoaHoc, Diem, LichHoc, ThanhToan,   │
│  ThongBao, PhanCongGiangDay, ChatHistory, GoiYKhoaHoc│
└──────────────────────────────────────────────────────┘
```

### 2.1.3 Phân quyền người dùng

| Role | Mô tả | Tài khoản seed |
|------|--------|---------------|
| Admin | Quản trị viên toàn quyền | admin@nnl.com / Admin@123 |
| GiangVien | Giảng viên – quản lý lớp, điểm | gv01@nnl.com / Gv@123 |
| HocVien | Học viên – đăng ký, xem điểm | hv01@nnl.com / Hv@123 |

---

## 2.2 PHẠM VI KIỂM THỬ

### 2.2.1 Trong phạm vi (In Scope)

| STT | Module | URL Pattern | Ưu tiên |
|-----|--------|------------|---------|
| 1 | Xác thực | /Account/* | Cao |
| 2 | Quản lý tài khoản | /TaiKhoan/* | Cao |
| 3 | Khóa học | /KhoaHoc/* | Cao |
| 4 | Lớp học | /LopHoc/* | Cao |
| 5 | Đăng ký khóa học | /DangKy/* | Cao |
| 6 | Điểm số | /Diem/* | Cao |
| 7 | Thanh toán | /ThanhToan/* | Cao |
| 8 | Phân công GV | /Admin/PhanCong* | Trung bình |
| 9 | Thông báo | /ThongBao/* | Thấp |
| 10 | AI Gợi ý | /GoiY/* | Thấp |

### 2.2.2 Ngoài phạm vi (Out of Scope)

- Kiểm thử tích hợp API bên ngoài (Groq/Gemini) – cần API key thật
- Kiểm thử xuất PDF chi tiết (iTextSharp)
- Kiểm thử Upload file quy mô lớn (>100MB)
- Kiểm thử multi-language/i18n (hệ thống chỉ tiếng Việt)
- Kiểm thử trên mobile/tablet (chỉ desktop browser)

---

## 2.3 KẾ HOẠCH KIỂM THỬ

### 2.3.1 Môi trường kiểm thử

| Môi trường | Cấu hình |
|-----------|---------|
| OS | Windows 10/11 hoặc Ubuntu 20.04+ |
| .NET Runtime | .NET 10.0.x |
| SQL Server | LocalDB (dev) / SQL Server 2019+ (prod) |
| Node.js | 18.x hoặc 20.x |
| Playwright | 1.44.0+ |
| JMeter | 5.6.3+ |
| Browser | Chromium (headless) |
| App URL | http://localhost:5299 |

### 2.3.2 Lịch kiểm thử

| Giai đoạn | Thời gian | Hoạt động |
|-----------|-----------|-----------|
| Chuẩn bị | Tuần 1 | Phân tích yêu cầu, thiết kế TC, setup môi trường |
| Kiểm thử | Tuần 2-3 | Chạy manual tests, chạy Playwright, review kết quả |
| Kiểm thử tải | Tuần 3 | Chạy JMeter, phân tích hiệu năng |
| Báo cáo | Tuần 4 | Tổng hợp kết quả, viết báo cáo, đề xuất cải tiến |

### 2.3.3 Nguồn lực

| Vai trò | Trách nhiệm |
|---------|------------|
| Test Lead | Lập kế hoạch, phân công, review báo cáo |
| QA Engineer | Viết test case, chạy automated tests |
| Dev | Fix bug, hỗ trợ môi trường |
| Performance Tester | JMeter setup và phân tích |

---

## 2.4 THIẾT KẾ TEST CASE CHI TIẾT

### 2.4.1 Module Xác thực (TC-001 → TC-010)

#### TC-001: Đăng nhập Admin thành công
```
Tiền điều kiện: Server đang chạy, DB có dữ liệu seed
Bước 1: Mở trình duyệt, điều hướng đến http://localhost:5299/Account/Login
Bước 2: Nhập Email = "admin@nnl.com"
Bước 3: Nhập MatKhau = "Admin@123"
Bước 4: Click nút "Đăng nhập"
Kết quả mong đợi:
  - HTTP 302 redirect đến /Admin
  - Sidebar màu navy blue (#0d1b3e)
  - Thanh navigation hiện tên "Admin"
  - Cookie "CookieAuth" được set với TTL 8 giờ
```

#### TC-004: Đăng nhập sai mật khẩu
```
Tiền điều kiện: Tài khoản admin@nnl.com tồn tại và IsActive=true
Bước 1: Nhập Email = "admin@nnl.com"
Bước 2: Nhập MatKhau = "wrongpassword"
Bước 3: Click "Đăng nhập"
Kết quả mong đợi:
  - Ở lại trang /Account/Login (không redirect)
  - Hiển thị thông báo lỗi: "Email hoặc mật khẩu không đúng"
  - Không có cookie authentication được set
  - Field mật khẩu được clear
```

#### TC-006: Đăng nhập tài khoản bị khóa
```
Tiền điều kiện: Có tài khoản với IsActive=false
Bước 1-4: Nhập đúng email/mật khẩu của tài khoản đã khóa
Kết quả mong đợi:
  - Ở lại trang Login
  - Hiển thị: "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin."
  - Không set cookie
```

### 2.4.2 Module Khóa học (TC-011 → TC-020)

#### TC-018: Xóa khóa học đã có lớp (thất bại có kiểm soát)
```
Tiền điều kiện: Đăng nhập Admin, có KH ID=1 đã có LopHoc
Bước 1: POST /KhoaHoc/Delete/1 với AntiForgery token
Kết quả mong đợi:
  - Redirect về /KhoaHoc
  - TempData["Error"] = "Không thể xóa khóa học đã có lớp học!"
  - KH vẫn còn trong DB (không bị xóa)
  - HTTP 302 → 200
```

#### TC-019: Đổi trạng thái khóa học (AJAX circular rotation)
```
Tiền điều kiện: KH ID=X đang có TrangThai="DangMo"
Bước 1: POST /KhoaHoc/ChangeStatus/X với AntiForgery token
Kết quả mong đợi lần 1:
  - JSON: {success: true, newStatus: "DaDong", displayText: "Đã đóng"}
  - DB: KH.TrangThai = "DaDong", NgayCapNhat cập nhật
Bước 2: POST lần 2 cùng endpoint
  - JSON: {success: true, newStatus: "TamDung"}
Bước 3: POST lần 3
  - JSON: {success: true, newStatus: "DangMo"}
```

### 2.4.3 Module Đăng ký (TC-027 → TC-033)

#### TC-027: HocVien đăng ký lớp thành công
```
Tiền điều kiện:
  - Đăng nhập HocVien (hv01)
  - Có lớp L1 với TrangThai="DangTuyenSinh"
  - L1.SiSoHienTai < L1.SiSoToiDa
  - HV chưa có DangKy cho L1
Bước 1: POST /DangKy/DangKy với lopHocId=L1.Id + AntiForgery token
Kết quả mong đợi:
  - Redirect đến /DangKy/CuaToi
  - TempData["Success"] = "Đăng ký thành công! Vui lòng chờ Admin duyệt."
  - DB: Bản ghi DangKyKhoaHoc mới với TrangThai="ChoDuyet"
  - ThongBao mới tạo cho Admin
```

#### TC-033: HocVien hủy đơn đã duyệt (thất bại)
```
Tiền điều kiện: HV có DangKy với TrangThai="DaDuyet"
Bước 1: POST /DangKy/Huy/{id} với id của đơn DaDuyet
Kết quả mong đợi:
  - Redirect /DangKy/CuaToi
  - TempData["Error"] = "Chỉ có thể hủy đơn đang chờ duyệt"
  - DB: Bản ghi KHÔNG thay đổi TrangThai
```

### 2.4.4 Module Điểm số (TC-034 → TC-039)

#### TC-034: Nhập điểm với công thức GK×30% + CK×70%
```
Tiền điều kiện: Admin/GV, có DangKy DaDuyet, Diem chưa khóa
Input: dangKyId=D1, diemGiuaKy=7.5, diemCuoiKy=8.0
POST /Diem/NhapDiem
Kết quả mong đợi:
  - JSON: {success: true, diemTongKet: "7.85", xepLoai: "Khá"}
  - Tính toán: 7.5×0.3 + 8.0×0.7 = 2.25 + 5.60 = 7.85
  - DB: Diem.DiemGiuaKy=7.5, DiemCuoiKy=8.0, DiemTongKet=7.85, XepLoai="Khá"
```

**Bảng giá trị biên cho XepLoai:**
| DiemTongKet | XepLoai mong đợi |
|------------|-----------------|
| 10.0 | Giỏi |
| 8.5 | Giỏi |
| 8.49 | Khá |
| 7.0 | Khá |
| 6.99 | Trung bình |
| 5.0 | Trung bình |
| 4.99 | Yếu |
| 0.0 | Yếu |

### 2.4.5 Module Tài khoản (TC-046 → TC-052)

#### TC-046: Tạo tài khoản HocVien qua AJAX modal
```
Tiền điều kiện: Đăng nhập Admin, /TaiKhoan load xong
Bước 1: Click nút "Tạo tài khoản mới" → modal #modalCreateTK hiện
Bước 2: Điền form:
  - cr-hoTen: "Nguyễn Văn Test"
  - cr-email: "test.unique.{timestamp}@example.com"
  - cr-vaiTro: "HocVien"
  - cr-matKhau: "Test@123"
  - cr-xacNhan: "Test@123"
Bước 3: Click nút "Tạo tài khoản" → AJAX POST /TaiKhoan/Create
Kết quả mong đợi:
  - JSON: {success: true, user: {id, hoTen, email, vaiTro: "HocVien", roleTxt: "Học viên", roleClass: "bg-success"}}
  - Toast thông báo thành công hiện
  - Dòng mới thêm vào #tk-tbody KHÔNG reload trang
  - DB: NguoiDung mới + HocVien mới với MaHocVien = "HVxxx"
```

#### TC-049: Admin không tự khóa mình
```
Tiền điều kiện: Admin đang đăng nhập với ID=X
Bước 1: POST /TaiKhoan/KhoaTaiKhoan/X (cùng ID với admin hiện tại)
Kết quả mong đợi:
  - JSON: {success: false, message: "Không thể khóa tài khoản của chính mình."}
  - DB: IsActive KHÔNG thay đổi
  - Trên UI: Button Khóa KHÔNG hiển thị cho row có badge "Bạn"
```

### 2.4.6 Module Thanh toán (TC-040 → TC-045)

#### TC-043: Thanh toán quy trình đầy đủ
```
Tiền điều kiện: 
  - HV đã có DangKy DaDuyet cho KH_ID=K1
  - Chưa có ThanhToan ChoPheduyet cho K1

Bước A (HocVien): POST /ThanhToan/TaoYeuCau
  - KhoaHocId: K1
  - PhuongThuc: "ChuyenKhoan"
  - GhiChu: "Chuyển khoản qua ngân hàng ABC"
  Kết quả A:
  - Redirect /ThanhToan/CuaToi
  - TempData["Success"] = "Đã gửi yêu cầu..."
  - DB: ThanhToan mới TrangThai="ChoPheduyet"

Bước B (Admin): POST /ThanhToan/Duyet
  - Id: (ThanhToan ID vừa tạo)
  - HanhDong: "DaThanhToan"
  Kết quả B:
  - Redirect /ThanhToan
  - TempData["Success"] = "Đã xác nhận thanh toán thành công."
  - DB: ThanhToan.TrangThai="DaThanhToan", NgayDuyet=Now, NguoiDuyetId=Admin.Id
```

---

## 2.5 TEST DATA VÀ ĐIỀU KIỆN ĐẦU

### 2.5.1 Seed Data cần có

| Entity | Tối thiểu | Lý do |
|--------|-----------|-------|
| NguoiDung | 3 (admin, gv, hv) | Seed accounts |
| HocVien | 1 (hv01) | Test đăng ký, điểm, thanh toán |
| GiangVien | 1 (gv01) | Test lớp học, điểm |
| KhoaHoc | ≥3 (DangMo, DaDong, TamDung) | Test filter, trạng thái |
| LopHoc | ≥2 (DangTuyenSinh, DangHoc) | Test đăng ký |
| DangKyKhoaHoc | ≥1 DaDuyet | Test điểm, thanh toán |
| Diem | ≥1 | Test nhập điểm |
| PhanCongGiangDay | ≥1 | Test phân công |

### 2.5.2 Dữ liệu test động

Để tránh conflict khi chạy test lặp lại, các test tạo dữ liệu mới dùng timestamp:
```javascript
const testEmail = `test.auto.${Date.now()}@example.com`;
const testName = `AutoTest_${Date.now()}`;
```

---

## 2.6 TIÊU CHÍ CHẤP NHẬN VÀ METRICS

### 2.6.1 Yêu cầu phi chức năng

| Chỉ số | Ngưỡng chấp nhận |
|--------|-----------------|
| Page Load Time (thường) | < 3000ms |
| Page Load Time (tải nặng) | < 5000ms |
| AJAX Response Time | < 2000ms |
| Login Time | < 5000ms |
| Error Rate (load test) | < 1% |
| Concurrent Users | 200 users |
| Throughput | > 50 req/s |

### 2.6.2 Tiêu chí PASS/FAIL

**Module PASS** khi:
- Tất cả TC Cao ưu tiên: PASS
- TC Trung bình: ≥80% PASS
- 0 lỗi Critical (crash app, data loss, security breach)

**Project PASS** khi:
- ≥90% tổng TC: PASS
- 0 lỗi Blocker mở
- Load test: Error rate < 1% ở 200 users
- Tất cả business rules quan trọng được verify

### 2.6.3 Phân loại lỗi (Defect Severity)

| Severity | Định nghĩa | Ví dụ |
|---------|-----------|-------|
| Blocker | App crash, không thể tiếp tục | Exception 500, infinite loop |
| Critical | Chức năng chính không hoạt động | Không đăng nhập được, mất dữ liệu |
| Major | Chức năng quan trọng sai kết quả | Công thức điểm sai, phân quyền sai |
| Minor | UI lỗi, tiện ích không hoạt động | Nút style sai, toast không hiện |
| Trivial | Lỗi nhỏ, không ảnh hưởng | Typo, alignment lệch nhẹ |

---

## 2.7 CẤU TRÚC THƯ MỤC KIEMTHU

```
D:\QuanLyKhoaHoc5\KiemThu\
├── 00_phan_tich_he_thong.md       ← Phân tích routes, roles, forms, business rules
├── 01_test_cases\
│   └── TestCase_Master.md          ← 55 test case + ma trận coverage
├── 02_selenium_scripts\
│   ├── package.json                ← npm dependencies (playwright)
│   ├── config.js                   ← BASE_URL, users, timeouts
│   └── tests\
│       ├── helpers.js              ← Hàm tiện ích dùng chung
│       ├── 01_login_test.js        ← TC-001 đến TC-010
│       ├── 02_khoa_hoc_test.js     ← TC-011 đến TC-020
│       ├── 03_lop_hoc_test.js      ← TC-021 đến TC-026
│       ├── 04_dang_ky_test.js      ← TC-027 đến TC-033
│       ├── 05_diem_test.js         ← TC-034 đến TC-039
│       ├── 06_thanh_toan_test.js   ← TC-040 đến TC-045
│       ├── 07_tai_khoan_test.js    ← TC-046 đến TC-052
│       ├── 08_phan_cong_test.js    ← TC-053 đến TC-055
│       └── run_all.js              ← Runner tổng hợp
├── 04_jmeter\
│   └── QuanLyKhoaHoc_LoadTest.jmx  ← 3 Thread Groups (100/150/200 users)
├── 06_bao_cao\
│   ├── Chuong1_Co_So_Ly_Thuyet.md  ← Lý thuyết kiểm thử (~3500 từ)
│   ├── Chuong2_Ke_Hoach_Va_Thiet_Ke.md  ← File này
│   └── Huong_Dan_Chay_Test.md      ← Hướng dẫn chạy tất cả công cụ
└── README.md                        ← Tổng quan và quick start
```

---

## 2.8 RỦI RO VÀ BIỆN PHÁP GIẢM THIỂU

| Rủi ro | Mức độ | Biện pháp |
|--------|--------|----------|
| App không khởi động được | Cao | Kiểm tra port 5299, logs startup, kill process cũ |
| DB không có seed data | Cao | Chạy `dotnet run` từ đầu để EF tạo và seed |
| Test flaky do timing | Trung bình | Dùng `waitForFunction` thay `waitForTimeout` |
| CSRF token lỗi | Trung bình | Extract từ form trước mỗi POST request |
| JMeter không có Java | Thấp | `choco install temurin17` hoặc download JDK |
| Playwright browser miss | Thấp | Chạy `npx playwright install chromium` |
| Email test trùng | Thấp | Dùng `Date.now()` trong email để unique |

---

## TÓM TẮT CHƯƠNG 2

Chương này đã trình bày:

1. **Mô tả hệ thống:** Stack kỹ thuật, kiến trúc 3-tier, phân quyền 3 roles
2. **Phạm vi kiểm thử:** 10 module trong phạm vi, xác định rõ out-of-scope
3. **Kế hoạch kiểm thử:** Môi trường, lịch trình 4 tuần, phân công vai trò
4. **Thiết kế 55 test case:** Bao phủ 8 module với mức độ ưu tiên rõ ràng
5. **Test data:** Seed data tối thiểu và chiến lược dữ liệu động
6. **Tiêu chí chấp nhận:** Response time, error rate, coverage metrics
7. **Phân loại lỗi:** 5 mức severity từ Trivial đến Blocker
8. **Cấu trúc thư mục:** Tổ chức file test rõ ràng, dễ maintai

Các kết quả kiểm thử thực tế và phân tích chi tiết được trình bày trong Hướng dẫn chạy test và báo cáo kết quả riêng.
