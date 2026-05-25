# KiemThu – Hạ tầng Kiểm thử QuanLyKhoaHoc5

> Thư mục này chứa toàn bộ tài liệu và script kiểm thử cho hệ thống **Quản Lý Khóa Học 5** (ASP.NET Core MVC, .NET 10).

---

## 📁 Cấu trúc thư mục

```
KiemThu/
├── 00_phan_tich_he_thong.md       ← Phân tích hệ thống (routes, roles, forms, business rules)
├── 01_test_cases/
│   └── TestCase_Master.md          ← 55 test cases + ma trận coverage
├── 02_selenium_scripts/            ← Playwright automated tests
│   ├── package.json
│   ├── config.js                   ← Cấu hình: URL, tài khoản, timeouts
│   └── tests/
│       ├── helpers.js
│       ├── 01_login_test.js        ← TC-001..010
│       ├── 02_khoa_hoc_test.js     ← TC-011..020
│       ├── 03_lop_hoc_test.js      ← TC-021..026
│       ├── 04_dang_ky_test.js      ← TC-027..033
│       ├── 05_diem_test.js         ← TC-034..039
│       ├── 06_thanh_toan_test.js   ← TC-040..045
│       ├── 07_tai_khoan_test.js    ← TC-046..052
│       ├── 08_phan_cong_test.js    ← TC-053..055
│       └── run_all.js              ← Runner tổng hợp
├── 04_jmeter/
│   └── QuanLyKhoaHoc_LoadTest.jmx  ← Load test: 100/150/200 concurrent users
├── 06_bao_cao/
│   ├── Chuong1_Co_So_Ly_Thuyet.md  ← Chương 1 luận văn: Cơ sở lý thuyết (~3500 từ)
│   ├── Chuong2_Ke_Hoach_Va_Thiet_Ke.md ← Chương 2: Kế hoạch & thiết kế test
│   └── Huong_Dan_Chay_Test.md      ← Hướng dẫn chạy Playwright, JMeter, ZAP, SonarQube
└── README.md                        ← File này
```

---

## ⚡ Quick Start

### Bước 1: Khởi động ứng dụng

```powershell
cd D:\QuanLyKhoaHoc5\QuanLyKhoaHoc5.Web
dotnet run
# → http://localhost:5299
```

### Bước 2: Cài đặt test dependencies

```powershell
cd D:\QuanLyKhoaHoc5\KiemThu\02_selenium_scripts
npm install
npx playwright install chromium
```

### Bước 3: Chạy tất cả automated tests

```powershell
npm test
```

### Bước 4: Xem kết quả

```
╔══════════════════════════════════════════════════════════╗
║  FINAL RESULT: ✅ ALL PASSED                              ║
║  Total time: 45.3s                                        ║
╚══════════════════════════════════════════════════════════╝
```

---

## 🎯 Phạm vi kiểm thử

| Module | TC IDs | Số TC | Tool |
|--------|--------|-------|------|
| Đăng nhập / Đăng xuất | TC-001..010 | 10 | Playwright |
| Khóa học | TC-011..020 | 10 | Playwright |
| Lớp học | TC-021..026 | 6 | Playwright |
| Đăng ký khóa học | TC-027..033 | 7 | Playwright |
| Điểm số | TC-034..039 | 6 | Playwright |
| Thanh toán | TC-040..045 | 6 | Playwright |
| Quản lý tài khoản | TC-046..052 | 7 | Playwright |
| Phân công GV | TC-053..055 | 3 | Playwright |
| **Tổng** | | **55** | |

---

## 🔧 Load Testing (JMeter)

```powershell
jmeter -n `
  -t D:\QuanLyKhoaHoc5\KiemThu\04_jmeter\QuanLyKhoaHoc_LoadTest.jmx `
  -l results.jtl `
  -e -o html_report
```

**3 Thread Groups:**
- TG1: 100 users – Browse public pages
- TG2: 150 users – HocVien authenticated flow
- TG3: 200 users – Admin heavy operations

---

## 📊 CI/CD (GitHub Actions)

Workflow `.github/workflows/selenium-tests.yml`:
- Trigger: push/PR vào `main`/`develop`
- Build .NET app → Start server → Run Playwright → Upload results
- Optional: SonarQube analysis trên main

---

## 📚 Tài liệu tham khảo

- [Hướng dẫn chạy test đầy đủ](06_bao_cao/Huong_Dan_Chay_Test.md)
- [Phân tích hệ thống](00_phan_tich_he_thong.md)
- [Toàn bộ test cases](01_test_cases/TestCase_Master.md)
- [Chương 1: Cơ sở lý thuyết](06_bao_cao/Chuong1_Co_So_Ly_Thuyet.md)
- [Chương 2: Kế hoạch & thiết kế](06_bao_cao/Chuong2_Ke_Hoach_Va_Thiet_Ke.md)

---

## 🔐 Tài khoản seed

| Role | Email | Password |
|------|-------|---------|
| Admin | admin@nnl.com | Admin@123 |
| GiangVien | gv01@nnl.com | Gv@123 |
| HocVien | hv01@nnl.com | Hv@123 |

---

## ✅ Tiêu chí hoàn thành

- [ ] ≥90% test case PASS
- [ ] 0 lỗi Blocker/Critical còn mở
- [ ] Load test: Error rate < 1% ở 200 users
- [ ] Response time < 3000ms (trung bình)
- [ ] SonarQube Quality Gate: Green
- [ ] 0 High alerts từ OWASP ZAP

---

*Tạo bởi Claude Code | 25/05/2026*
