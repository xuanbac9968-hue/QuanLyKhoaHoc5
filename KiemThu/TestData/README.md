# TestData – Dữ liệu kiểm thử Katalon Studio

Thư mục này chứa **6 file Excel** dữ liệu đầu vào cho các script Groovy kiểm thử data-driven.

---

## Cấu trúc

```
KiemThu\TestData\
├── README.md                      ← file này
├── TC01_DangNhap.xlsx             ← 8  test cases: Đăng nhập
├── TC02_DoiMatKhau.xlsx           ← 6  test cases: Đổi mật khẩu
├── TC03_TaoTaiKhoan.xlsx          ← 7  test cases: Tạo tài khoản (Admin)
├── TC04_ThemKhoaHoc.xlsx          ← 6  test cases: Thêm khóa học (Admin)
├── TC05_TaoThanhToan.xlsx         ← 6  test cases: Tạo yêu cầu thanh toán (HV)
├── TC06_PhanCong.xlsx             ← 8  test cases: Phân công giảng viên (Admin)
└── GenTestData\                   ← C# project tạo ra các file Excel trên
    ├── GenTestData.csproj
    └── Program.cs
```

---

## Tái tạo file Excel

```powershell
cd D:\QuanLyKhoaHoc5\KiemThu\TestData\GenTestData
dotnet run
```

**Lưu ý:** Chạy lại sẽ ghi đè toàn bộ dữ liệu, xóa cột KetQua đã ghi trước đó.

---

## Cấu trúc cột từng file

### TC01_DangNhap.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | Email | Email đăng nhập |
| D(3) | Mật khẩu | Mật khẩu |
| E(4) | GhiNho | true/false (Remember Me) |
| F(5) | KetQuaMongDoi | ThanhCong / ThatBai |
| G(6) | KetQua | **Script tự ghi** Pass/Fail |

### TC02_DoiMatKhau.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | EmailLogin | Email để đăng nhập |
| D(3) | MatKhauLogin | Mật khẩu đăng nhập |
| E(4) | MatKhauCu | Mật khẩu cũ (nhập vào form) |
| F(5) | MatKhauMoi | Mật khẩu mới |
| G(6) | XacNhanMatKhau | Xác nhận mật khẩu mới |
| H(7) | KetQuaMongDoi | ThanhCong / ThatBai |
| I(8) | KetQua | **Script tự ghi** |

> ⚠️ Row 4 đổi MK hv01→NewHv@456789, Row 5 đổi lại về Hv@123. Phải chạy theo thứ tự.

### TC03_TaoTaiKhoan.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | HoTen | Họ tên tài khoản mới |
| D(3) | Email | Email tài khoản mới |
| E(4) | VaiTro | HocVien / GiangVien / Admin |
| F(5) | MatKhau | Mật khẩu |
| G(6) | XacNhanMatKhau | Xác nhận mật khẩu |
| H(7) | KetQuaMongDoi | ThanhCong / ThatBai |
| I(8) | KetQua | **Script tự ghi** |

> ⚠️ Chạy lần 2: row 1, 2, 6 sẽ FAIL vì email đã tồn tại. Reset DB hoặc dùng email mới.

### TC04_ThemKhoaHoc.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | TenKhoaHoc | Tên khóa học |
| D(3) | MoTaKhoaHoc | Mô tả khóa học |
| E(4) | NgonNgu | "Tiếng Anh" hoặc "Tiếng Nhật" |
| F(5) | TrinhDo | Sơ cấp/Trung cấp/Cao cấp/IELTS/TOEIC/N5/N4/N3/N2/N1 |
| G(6) | HocPhi | Số tiền VNĐ |
| H(7) | ThoiLuong | Số buổi |
| I(8) | SoBuoiMoiTuan | Số buổi/tuần (1-7) |
| J(9) | ThoiGianMoiBuoi | Thời gian mỗi buổi (phút) |
| K(10) | TrangThai | DangMo / TamDung / DaDong |
| L(11) | KetQuaMongDoi | ThanhCong / ThatBai |
| M(12) | KetQua | **Script tự ghi** |

### TC05_TaoThanhToan.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | EmailHocVien | Email học viên |
| D(3) | MatKhauHocVien | Mật khẩu học viên |
| E(4) | KhoaHocId | ID khóa học trong DB |
| F(5) | PhuongThuc | TienMat / ChuyenKhoan |
| G(6) | GhiChu | Ghi chú tùy chọn |
| H(7) | KetQuaMongDoi | ThanhCong / ThatBai |
| I(8) | KetQua | **Script tự ghi** |

**Mapping HocVien → KhoaHoc (DaDuyet):**
- hv01@nnl.com → KH2 (B1), KH3 (IELTS)
- hv02@nnl.com → KH4 (N5), KH6 (Hàn)
- hv03@nnl.com → KH6 (Hàn, DaDuyet); KH5 (N4, ChoDuyet)

> ⚠️ Chạy lần 2: row 1-3 có thể FAIL vì ThanhToan đã tồn tại (duplicate). Reset DB để test lại.

### TC06_PhanCong.xlsx
| Cột | Tên | Mô tả |
|-----|-----|-------|
| A(0) | STT | Số thứ tự |
| B(1) | Mô tả | Mô tả test case |
| C(2) | KhoaHocId | ID khóa học (1=A1, 2=B1, 3=IELTS, 4=N5, 5=N4, 6=Hàn) |
| D(3) | GiangVienId | ID giảng viên (2=Hương/Anh, 3=Nam/Nhật, 4=Đức/Hàn, 0=Bỏ PC) |
| E(4) | GhiChu | Ghi chú tùy chọn |
| F(5) | KetQuaMongDoi | ThanhCong / ThatBai |
| G(6) | KetQua | **Script tự ghi** |

> ✅ TC06 có thể chạy nhiều lần (phân công là idempotent).

---

## Script Groovy tương ứng

Xem tại: `D:\QuanLyKhoaHoc5\KiemThu\Scripts\`

| Excel | Groovy script |
|-------|--------------|
| TC01_DangNhap.xlsx | TC01_DangNhap.groovy |
| TC02_DoiMatKhau.xlsx | TC02_DoiMatKhau.groovy |
| TC03_TaoTaiKhoan.xlsx | TC03_TaoTaiKhoan.groovy |
| TC04_ThemKhoaHoc.xlsx | TC04_ThemKhoaHoc.groovy |
| TC05_TaoThanhToan.xlsx | TC05_TaoThanhToan.groovy |
| TC06_PhanCong.xlsx | TC06_PhanCongGiangVien.groovy |

App URL: **http://localhost:5125**
