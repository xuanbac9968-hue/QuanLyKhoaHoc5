# Hướng Dẫn Import và Chạy Project Katalon QuanLyKhoaHoc5_New

## Yêu Cầu Hệ Thống

| Thành phần | Phiên bản |
|---|---|
| Katalon Studio | Free Edition ≥ 9.x |
| Java | ≥ 11 (bundled với Katalon) |
| Chrome | Phiên bản mới nhất |
| ChromeDriver | Tự động quản lý bởi Katalon |
| .NET Web App | Đang chạy tại http://localhost:5125 |

---

## Bước 1 – Mở Project Trong Katalon Studio

1. Mở **Katalon Studio**
2. Chọn **File → Open Project**
3. Duyệt đến thư mục:
   ```
   D:\QuanLyKhoaHoc5\KiemThu\04_katalon_new\QuanLyKhoaHoc5_New
   ```
4. Chọn file **`QuanLyKhoaHoc5_New.prj`** → Click **OK**

> ✅ Katalon sẽ tự nhận diện cấu trúc thư mục: Object Repository, Test Cases, Test Suites, Profiles.

---

## Bước 2 – Kiểm Tra Web Application Đang Chạy

Đảm bảo web app đang chạy trước khi chạy test:

```powershell
# Trong thư mục QuanLyKhoaHoc5.Web
dotnet run
# Hoặc trong Visual Studio: nhấn F5
```

Kiểm tra: mở trình duyệt → truy cập `http://localhost:5125/Account/Login`

---

## Bước 3 – Kiểm Tra Dữ Liệu Test (Excel)

Các file Excel test data nằm tại:
```
D:\QuanLyKhoaHoc5\KiemThu\TestData\
├── TC01_DangNhap.xlsx
├── TC02_DoiMatKhau.xlsx
├── TC03_TaoTaiKhoan.xlsx
├── TC04_ThemKhoaHoc.xlsx
├── TC05_TaoThanhToan.xlsx
└── TC06_PhanCong.xlsx
```

Nếu chưa có file Excel, chạy tool sinh dữ liệu:
```powershell
cd D:\QuanLyKhoaHoc5\KiemThu\TestData\GenTestData
dotnet run
```

---

## Bước 4 – Cấu Hình Global Variables (Nếu Cần)

Mở **`Profiles/default.glbl`** trong Katalon để kiểm tra:

| Variable | Giá trị mặc định |
|---|---|
| `BASE_URL` | `http://localhost:5125` |
| `ADMIN_EMAIL` | `admin@nnl.com` |
| `ADMIN_PASS` | `Admin@123` |
| `TEST_DATA_DIR` | `D:\QuanLyKhoaHoc5\KiemThu\TestData` |
| `TIMEOUT` | `10` |

Nếu cần thay đổi (ví dụ port khác), double-click vào **`default`** profile trong panel Profiles.

---

## Bước 5 – Thêm Apache POI vào Classpath

Các Test Case dùng Apache POI để đọc/ghi Excel. Cần thêm thư viện:

### Cách A: Dùng External Library của Katalon (Khuyên dùng)

1. Trong Katalon, vào **Project → Settings → Library Management**
2. Click **Add** → duyệt và thêm các file JAR sau (tải từ [Maven Central](https://mvnrepository.com)):
   - `poi-5.x.x.jar`
   - `poi-ooxml-5.x.x.jar`
   - `commons-collections4-4.x.jar`
   - `xmlbeans-5.x.x.jar`
   - `commons-compress-1.x.x.jar`
3. Click **OK** và restart Katalon nếu được yêu cầu

### Cách B: Đặt JAR vào thư mục Drivers

Tạo thư mục `Drivers` trong project và đặt các file JAR vào đó:
```
QuanLyKhoaHoc5_New\
└── Drivers\
    ├── poi-5.3.0.jar
    ├── poi-ooxml-5.3.0.jar
    └── ...
```

> ⚠️ **Lưu ý:** Katalon Free Edition hỗ trợ Apache POI. Nếu bạn gặp lỗi `ClassNotFoundException`, kiểm tra lại phần Library Management.

---

## Bước 6 – Chạy Từng Test Case

### Chạy một Test Case:
1. Trong **Test Explorer** → mở thư mục **Test Cases**
2. Double-click vào test case muốn chạy (ví dụ: `TC01_DangNhap`)
3. Click nút **Run** (▶) ở toolbar
4. Chọn browser: **Chrome** → OK

### Thứ tự chạy khuyên dùng:
```
TC01 → TC02 → TC03 → TC04 → TC05 → TC06
```

> ⚠️ **TC02** thay đổi mật khẩu hv01 rồi khôi phục lại. Chạy TC02 trước TC05 nếu TC05 dùng hv01.

---

## Bước 7 – Chạy Toàn Bộ Test Suite

1. Trong **Test Explorer** → mở thư mục **Test Suites**
2. Double-click vào **`TS_All`**
3. Click nút **Run** (▶)
4. Chọn browser: **Chrome** → OK

Test Suite sẽ chạy lần lượt 6 test case. Kết quả hiển thị trong tab **Log Viewer** và **Reports**.

---

## Bước 8 – Xem Kết Quả

### Trong Katalon Studio:
- **Log Viewer** (panel dưới): Xem log real-time
- **Reports** folder: Báo cáo HTML tự động sau mỗi lần chạy

### Trong File Excel:
Mỗi test case ghi kết quả `PASS` / `FAIL` / `ERROR: ...` vào cột `KetQua` của file Excel tương ứng.

---

## Cấu Trúc Object Repository

```
Object Repository/
├── Common/
│   ├── div_AlertSuccess.rs    ← .alert.alert-success (TempData thành công)
│   └── div_AlertDanger.rs     ← .alert-danger (lỗi chung)
├── Page_Login/
│   ├── input_Email.rs         ← input#Email
│   ├── input_MatKhau.rs       ← input#pwInput (KHÔNG phải #MatKhau!)
│   ├── chk_GhiNho.rs          ← input#GhiNho
│   ├── btn_DangNhap.rs        ← button[type='submit']
│   └── div_AlertDanger.rs     ← .alert-danger
├── Page_DoiMatKhau/
│   ├── input_MatKhauCu.rs     ← input#MatKhauCu
│   ├── input_MatKhauMoi.rs    ← input#MatKhauMoi
│   ├── input_XacNhanMatKhau.rs← input#XacNhanMatKhau
│   ├── btn_XacNhan.rs         ← button[type='submit']
│   └── div_AlertDanger.rs     ← .alert-danger
├── Page_TaoTaiKhoan/
│   ├── btn_MoModal.rs         ← button[onclick="openCreateModal()"]
│   ├── input_crHoTen.rs       ← input#cr-hoTen
│   ├── input_crEmail.rs       ← input#cr-email
│   ├── select_crVaiTro.rs     ← select#cr-vaiTro
│   ├── input_crMatKhau.rs     ← input#cr-matKhau
│   ├── input_crXacNhan.rs     ← input#cr-xacNhan
│   ├── btn_CreateSubmit.rs    ← button#btn-create-submit
│   ├── div_CreateAlert.rs     ← div#create-alert (ẩn bằng d-none)
│   └── div_ToastSuccess.rs    ← #toast-container .toast.bg-success
├── Page_ThemKhoaHoc/
│   ├── input_TenKhoaHoc.rs    ← input#TenKhoaHoc
│   ├── textarea_MoTa.rs       ← textarea#MoTa
│   ├── select_NgonNgu.rs      ← select#NgonNgu
│   ├── select_TrinhDo.rs      ← select#TrinhDo
│   ├── input_HocPhi.rs        ← input#HocPhi
│   ├── input_ThoiLuong.rs     ← input#ThoiLuong
│   ├── input_SoBuoiMoiTuan.rs ← input#SoBuoiMoiTuan
│   ├── input_ThoiGianMoiBuoi.rs←input#ThoiGianMoiBuoi
│   ├── select_TrangThai.rs    ← select#TrangThai
│   ├── btn_TaoKhoaHoc.rs      ← button[type='submit']
│   └── div_AlertDanger.rs     ← .alert-danger
├── Page_ThanhToan/
│   ├── radio_TienMat.rs       ← input#ptTienMat
│   ├── radio_ChuyenKhoan.rs   ← input#ptChuyenKhoan
│   ├── textarea_GhiChu.rs     ← textarea#GhiChu
│   └── btn_GuiYeuCau.rs       ← button[type='submit']
└── Page_PhanCong/
    └── div_AlertSuccess.rs    ← .alert.alert-success
```

---

## Lưu Ý Quan Trọng

### ⚠️ Selector Mật Khẩu Đăng Nhập
Trang Login dùng `id="pwInput"` (KHÔNG phải `id="MatKhau"`):
```groovy
// ĐÚNG:
findTestObject('Object Repository/Page_Login/input_MatKhau')
// → CSS: input#pwInput

// SAI (project cũ 03_katalon):
// CSS: input#MatKhau → KHÔNG TÌM THẤY PHẦN TỬ
```

### ⚠️ TC06 – XPath Động Cho PhanCong
TC06 tạo TestObject tại runtime để chọn đúng `select` giảng viên theo `KhoaHocId`:
```groovy
TestObject selectGV = new TestObject('select_GiangVien_KH' + khoaHocId)
selectGV.addProperty('xpath', ConditionType.EQUALS,
    "//form[.//input[@name='KhoaHocId' and @value='${khoaHocId}']]//select[@name='GiangVienId']"
)
```

### ⚠️ TC03 – Chạy Lần 2 Sẽ Fail Các Row ThanhCong
TC03 tạo tài khoản mới (hvmoi01@test.com, gvmoi01@test.com). Lần chạy thứ 2, email đã tồn tại → các row đó sẽ fail (expected behavior, không phải bug).

### ⚠️ Thứ Tự Database
Chạy test theo thứ tự TC01 → TC06. Nếu database bị thay đổi (ví dụ xóa seed), một số test sẽ fail.

### ⚠️ Reset Database
Nếu muốn chạy lại từ đầu, reset database:
```powershell
# Trong thư mục QuanLyKhoaHoc5.Web
dotnet ef database drop --force
dotnet ef database update
# Hoặc chạy lại app để trigger seed
dotnet run
```

---

## Tài Khoản Seed Mặc Định

| Email | Mật khẩu | Vai trò |
|---|---|---|
| admin@nnl.com | Admin@123 | Admin |
| gv01@nnl.com | Gv@123 | GiangVien |
| hv01@nnl.com | Hv@123 | HocVien |
| hv02@nnl.com | Hv@123 | HocVien |
| hv03@nnl.com | Hv@123 | HocVien |

---

*Tạo bởi Claude Code – Dự án QuanLyKhoaHoc5*
