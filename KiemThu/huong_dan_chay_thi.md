# Hướng dẫn chạy kiểm thử tự động – QuanLyKhoaHoc5

**Công cụ:** Katalon Studio 11.1.3 · Chrome · SQL Server · .NET 10  
**Dự án Katalon:** `D:\QuanLyKhoaHoc5\KiemThu\04_katalon_new\QuanLyKhoaHoc5_New`  
**Test Suite:** `Test Suites/TS_All` (6 Test Case: TC01 → TC06)

---

## Quy trình thực hiện (mỗi lần chạy)

### Bước 1 — Khởi động ứng dụng web

1. Mở **Visual Studio** → mở solution `QuanLyKhoaHoc5.sln`
2. Nhấn **F5** (hoặc nút ▶ Run) để chạy ở chế độ Debug
3. Chờ trình duyệt mở hoặc xem Output: xuất hiện `Now listening on: http://localhost:5125`
4. **Giữ VS mở** — đừng tắt trong lúc chạy Katalon

> **Kiểm tra nhanh:** Mở `http://localhost:5125` → thấy trang đăng nhập là OK ✓

---

### Bước 2 — Reset dữ liệu test (chạy trước mỗi lần test)

1. Mở `D:\QuanLyKhoaHoc5\KiemThu\`
2. **Double-click** file `reset_db.bat`
3. Cửa sổ CMD hiện ra, chờ thấy dòng:
   ```
   XONG! Co the chay Katalon TS_All.
   ```
4. Nhấn **Enter** để đóng cửa sổ

> **Mục đích:** Xóa các tài khoản test do TC03 tạo ra (email dạng `hvmoi*`, `gvmoi*`, `admin_moi*`)  
> Nếu không reset, TC03 sẽ báo lỗi "Email đã tồn tại" ở lần chạy thứ 2 trở đi.

---

### Bước 3 — Mở Katalon Studio

1. Mở **Katalon Studio** (shortcut trên Desktop hoặc từ Start Menu)
2. Vào **File → Open Project**
3. Duyệt đến: `D:\QuanLyKhoaHoc5\KiemThu\04_katalon_new\QuanLyKhoaHoc5_New`
4. Chọn file `QuanLyKhoaHoc5_New.prj` → **OK**
5. Chờ Katalon load xong project (thanh progress bar ở dưới hết)

> Nếu project đang mở sẵn rồi: bỏ qua bước này.

---

### Bước 4 — Chạy TS_All

1. Trong **Test Explorer** bên trái → mở **Test Suites** → double-click `TS_All`
2. Click nút **▶ Run** (góc phải trên) hoặc nhấn **Ctrl+Shift+A**  
   (Hoặc chuột phải `TS_All` → **Run**)
3. Hộp thoại **Execute Test Suite** xuất hiện:
   - **Execution Profile:** `default`
   - **Web Browser:** `Chrome`
4. Click **OK**

> Katalon sẽ tự động mở Chrome, chạy lần lượt 6 test case.  
> **Không đóng cửa sổ Chrome** trong lúc chạy.

---

### Bước 5 — Xem kết quả

Sau khi chạy xong (~15–30 phút tùy máy), kết quả hiện ở tab **Log Viewer** và **Report**.

#### Kết quả mong đợi

| Test Case | Mô tả | Số ca | Kỳ vọng |
|-----------|-------|-------|---------|
| TC01 | Đăng nhập | 8 | 8 PASS |
| TC02 | Đổi mật khẩu | 7 | 7 PASS |
| TC03 | Tạo tài khoản | 8 | 8 PASS |
| TC04 | Thêm khóa học | 8 | 8 PASS |
| TC05 | Tạo thanh toán | 6 | 6 PASS |
| TC06 | Phân công GV | 4 | 4 PASS |
| **TỔNG** | | **41** | **41 PASS** |

#### Xem báo cáo chi tiết

- Tab **Log Viewer**: log từng bước theo thời gian thực
- Tab **Result**: bảng PASS/FAIL từng test case
- File báo cáo: `Reports/` → folder theo timestamp → `*.html`

---

## Lưu ý quan trọng

| Tình huống | Xử lý |
|-----------|--------|
| TC03 báo "Email đã tồn tại" | Chạy lại `reset_db.bat` trước khi test |
| TC05 lỗi timeout / treo lâu | Kiểm tra web app có đang chạy không (Bước 1) |
| Chrome không mở được | Tắt Chrome đang mở sẵn, thử lại |
| Katalon báo "Test Cases/TCxx not found" | Trong Katalon: chuột phải project → **Refresh** |
| Toàn bộ 6 TC bị Skipped | Script ở sai vị trí — kiểm tra folder `Scripts/` |
| Lỗi "No such property: GlobalVariable" | Thiếu `import internal.GlobalVariable` trong script |

---

## Cấu trúc project Katalon

```
04_katalon_new/QuanLyKhoaHoc5_New/
├── Test Cases/          ← Metadata (.tc) + bản gốc .groovy
├── Scripts/             ← Script thực thi (Katalon 11 đọc ở đây)
│   ├── TC01_DangNhap/Script1779820840101.groovy
│   ├── TC02_DoiMatKhau/Script1779820840102.groovy
│   ├── TC03_TaoTaiKhoan/Script1779820840103.groovy
│   ├── TC04_ThemKhoaHoc/Script1779820840104.groovy
│   ├── TC05_TaoThanhToan/Script1779820840105.groovy
│   └── TC06_PhanCongGiangVien/Script1779820840106.groovy
├── Object Repository/   ← 34 UI elements (CSS selectors)
├── Data Files/          ← 6 file Excel test data (.xlsx)
├── Profiles/default.glbl ← GlobalVariable: BASE_URL, ADMIN_*, TIMEOUT...
└── Test Suites/TS_All.ts ← Test suite chứa 6 TC
```

---

## Tài khoản seed trong database

| Email | Mật khẩu | Vai trò |
|-------|----------|---------|
| `admin@nnl.com` | `Admin@123` | Admin |
| `hv01@nnl.com` | `Hv01@123` | HocVien |
| `hv02@nnl.com` | `Hv02@123` | HocVien |
| `hv03@nnl.com` | `Hv03@123` | HocVien |
| `gv01@nnl.com` | `Gv01@123` | GiangVien |

> Web chạy tại: **http://localhost:5125**
