# REQUIREMENTS.md
# Hệ thống Quản lý Khóa học Trung tâm Ngoại ngữ tích hợp Gợi ý Khóa học Thông minh

---

## 1. Mô tả tổng quan hệ thống

### 1.1 Thông tin dự án

| Thuộc tính | Nội dung |
|---|---|
| Tên dự án | Xây dựng hệ thống quản lý khóa học cho trung tâm ngoại ngữ tích hợp gợi ý khóa học thông minh |
| Loại sản phẩm | Ứng dụng web (ASP.NET Core 8 MVC) |
| Ngôn ngữ | Tiếng Việt (toàn bộ giao diện và nội dung) |
| Đối tượng khảo sát | Trung tâm Ngoại ngữ ABC, Thái Nguyên |
| Quy mô | ~300 học viên, dạy Tiếng Anh và Tiếng Nhật |
| Hiện trạng thay thế | Excel + sổ tay, đăng ký qua Zalo/điện thoại, thông báo lịch qua nhóm Zalo |

### 1.2 Mục tiêu hệ thống

Xây dựng nền tảng web tập trung để:
- Quản lý toàn bộ khóa học, lớp học, học viên, giảng viên và lịch dạy
- Cho phép học viên đăng ký khóa học trực tuyến thay vì qua Zalo/điện thoại
- Theo dõi điểm số và kết quả học tập của học viên
- Tích hợp AI (Google Gemini API) để gợi ý khóa học phù hợp cho từng học viên
- Đảm bảo bảo mật, phân quyền rõ ràng theo vai trò (Admin / Giảng viên / Học viên)
- Giao diện responsive, thân thiện, hỗ trợ đa trình duyệt

### 1.3 Các Actor (Tác nhân)

| Actor | Mô tả | Quyền hạn |
|---|---|---|
| Admin | Quản trị viên trung tâm | Toàn quyền hệ thống |
| GiangVien | Giảng viên / giáo viên | Quản lý lớp, điểm số, lịch dạy của mình |
| HocVien | Học viên đang học tại trung tâm | Đăng ký, xem lịch, xem điểm, nhận gợi ý |

---

## 2. Công nghệ sử dụng

### 2.1 Backend

```
Framework   : ASP.NET Core 8 MVC (.NET 8 LTS)
ORM         : Entity Framework Core 8 (Code First)
Database    : SQL Server (LocalDB cho dev / SQL Server Express cho production)
Auth        : ASP.NET Core Identity + Cookie Authentication (Role-based)
Password    : BCrypt hashing (BCrypt.Net-Next)
AI          : Google Gemini API (gemini-1.5-flash hoặc gemini-2.0-flash)
HTTP Client : HttpClient (tích hợp sẵn) để gọi Gemini REST API
```

### 2.2 Frontend

```
CSS Framework : Bootstrap 5.3
Icons         : Bootstrap Icons hoặc Font Awesome 6
Charts        : Chart.js (thống kê dashboard)
JS            : Vanilla JavaScript + jQuery (nếu cần)
Template      : Razor Views (.cshtml) + Partial Views + Layout
```

### 2.3 Package NuGet cần cài

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.*" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.*" />
<PackageReference Include="Newtonsoft.Json" Version="13.*" />
<PackageReference Include="ClosedXML" Version="0.102.*" /> <!-- Export Excel -->
```

### 2.4 Cấu hình appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NgoaiNguABC;Trusted_Connection=True;"
  },
  "GeminiAPI": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent",
    "MaxTokens": 1024
  },
  "AppSettings": {
    "TenTrungTam": "Trung tâm Ngoại ngữ ABC",
    "DiaChi": "Thái Nguyên",
    "DefaultPassword": "Abc@12345"
  }
}
```

---

## 3. Danh sách đầy đủ chức năng theo Actor

### 3.1 Admin — Toàn quyền hệ thống

#### 3.1.1 Quản lý Tài khoản & Phân quyền
- Xem danh sách tất cả tài khoản (Admin / Giảng viên / Học viên)
- Tạo tài khoản mới cho giảng viên và học viên
- Sửa thông tin tài khoản
- Khóa / mở khóa tài khoản
- Reset mật khẩu (đặt về mật khẩu mặc định)
- Phân vai trò (Admin / GiangVien / HocVien)

#### 3.1.2 Quản lý Khóa học
- Xem danh sách tất cả khóa học (có phân trang, tìm kiếm, lọc)
- Thêm khóa học mới (tên, mô tả, ngôn ngữ, trình độ, học phí, thời lượng, số buổi, ảnh thumbnail)
- Sửa thông tin khóa học
- Xóa khóa học (chỉ khi chưa có lớp học nào)
- Thay đổi trạng thái khóa học: Đang mở đăng ký / Đã đóng / Tạm dừng
- Lọc theo: ngôn ngữ (Tiếng Anh / Tiếng Nhật), trình độ, trạng thái

#### 3.1.3 Quản lý Lớp học
- Xem danh sách tất cả lớp học
- Tạo lớp học mới (tên lớp, khóa học, giảng viên phụ trách, ngày khai giảng, ngày kết thúc, sĩ số tối đa, phòng học)
- Sửa thông tin lớp học
- Xóa lớp học (chỉ khi chưa có học viên đăng ký)
- Gán / thay đổi giảng viên phụ trách lớp
- Xem danh sách học viên trong lớp
- Mở / đóng đăng ký lớp học

#### 3.1.4 Quản lý Lịch học
- Xem lịch học dạng bảng và dạng calendar
- Thêm buổi học (lớp, phòng, ngày, giờ bắt đầu, giờ kết thúc, ghi chú)
- Sửa / xóa buổi học
- Nhân lịch học (tạo hàng loạt buổi học theo tuần)
- Cảnh báo trùng lịch phòng học hoặc giảng viên

#### 3.1.5 Quản lý Học viên
- Xem danh sách học viên (phân trang, tìm kiếm theo tên/mã SV/email)
- Thêm học viên mới
- Sửa thông tin học viên
- Xóa học viên (soft delete)
- Xem lịch sử đăng ký khóa học của học viên
- Xem điểm và kết quả học tập
- Xuất danh sách học viên ra file Excel

#### 3.1.6 Quản lý Giảng viên
- Xem danh sách giảng viên
- Thêm / sửa / xóa giảng viên
- Xem lịch dạy của giảng viên
- Xem các lớp giảng viên đang phụ trách

#### 3.1.7 Quản lý Đăng ký Khóa học
- Xem toàn bộ đơn đăng ký (tất cả trạng thái)
- Duyệt / từ chối đơn đăng ký
- Đăng ký thay cho học viên
- Hủy đăng ký
- Lọc theo: trạng thái, khóa học, lớp học, thời gian

#### 3.1.8 Quản lý Điểm & Kết quả
- Xem điểm của tất cả học viên theo lớp
- Nhập / sửa điểm cho học viên
- Xem thống kê điểm theo lớp / khóa học
- Xuất bảng điểm ra Excel

#### 3.1.9 Dashboard & Thống kê
- Tổng quan: số học viên, số khóa học, số lớp đang hoạt động, doanh thu tháng
- Biểu đồ học viên đăng ký theo tháng (Chart.js)
- Biểu đồ tỷ lệ khóa học theo ngôn ngữ (Tiếng Anh / Tiếng Nhật)
- Danh sách lớp sắp khai giảng
- Danh sách đăng ký chờ duyệt gần đây
- Thông báo hệ thống

#### 3.1.10 Gợi ý Khóa học AI (Admin view)
- Xem log lịch sử các lần AI gợi ý
- Xem thống kê: khóa học được gợi ý nhiều nhất
- Cấu hình tham số gợi ý AI

---

### 3.2 Giảng viên

#### 3.2.1 Dashboard Giảng viên
- Tổng quan: số lớp đang dạy, số học viên, lịch dạy hôm nay / tuần này
- Danh sách lớp đang phụ trách
- Lịch dạy tuần hiện tại (dạng calendar hoặc bảng)

#### 3.2.2 Quản lý Lớp của mình
- Xem danh sách lớp đang và đã dạy
- Xem chi tiết lớp: thông tin, danh sách học viên, lịch học
- Xem điểm danh từng buổi học
- Cập nhật ghi chú lớp học

#### 3.2.3 Quản lý Điểm
- Nhập điểm cho học viên trong lớp mình phụ trách
- Sửa điểm (khi chưa được Admin khóa)
- Xem bảng điểm tổng hợp của lớp
- Xuất bảng điểm ra Excel

#### 3.2.4 Quản lý Lịch dạy
- Xem lịch dạy cá nhân theo tuần / tháng
- Xem chi tiết từng buổi học

#### 3.2.5 Thông tin cá nhân
- Xem và cập nhật thông tin cá nhân (họ tên, số điện thoại, ảnh đại diện)
- Đổi mật khẩu

---

### 3.3 Học viên

#### 3.3.1 Dashboard Học viên
- Thông tin cá nhân tóm tắt
- Khóa học đang học, tiến độ
- Lịch học tuần này
- Thông báo mới nhất
- Gợi ý khóa học từ AI (hiển thị nổi bật)

#### 3.3.2 Xem Khóa học
- Xem danh sách tất cả khóa học đang mở đăng ký
- Lọc theo: ngôn ngữ, trình độ, học phí, lịch học
- Xem chi tiết khóa học: mô tả, nội dung chương trình, giảng viên, lịch học mẫu, học phí
- Xem các lớp đang tuyển sinh của khóa học

#### 3.3.3 Đăng ký Khóa học
- Chọn lớp học và gửi đơn đăng ký
- Xem trạng thái đơn đăng ký: Chờ duyệt / Đã duyệt / Từ chối / Đã hủy
- Hủy đơn đăng ký (khi còn ở trạng thái Chờ duyệt)
- Xem lý do từ chối (nếu bị từ chối)

#### 3.3.4 Lịch học
- Xem lịch học cá nhân theo tuần (các lớp đang theo học)
- Nhận thông báo khi lịch học thay đổi

#### 3.3.5 Điểm & Kết quả
- Xem điểm của từng lớp đang / đã học
- Xem bảng điểm chi tiết
- Xem nhận xét từ giảng viên

#### 3.3.6 Gợi ý Khóa học AI
- Nhận gợi ý khóa học phù hợp dựa trên:
  - Lịch sử đăng ký và các khóa đã học
  - Trình độ hiện tại (được xác định qua kết quả học)
  - Ngôn ngữ quan tâm
- Xem lý do AI gợi ý (giải thích ngắn gọn)
- Nhấn "Đăng ký ngay" từ kết quả gợi ý

#### 3.3.7 Thông tin cá nhân
- Xem và cập nhật thông tin cá nhân
- Đổi mật khẩu
- Xem lịch sử đăng ký khóa học

---

### 3.4 Chức năng chung (tất cả Actor)

- Đăng nhập bằng email + mật khẩu
- Đăng xuất
- Xem thông báo hệ thống
- Đổi mật khẩu
- Cập nhật ảnh đại diện

---

## 4. Cấu trúc Database

### 4.1 Danh sách bảng

| STT | Tên bảng | Mô tả |
|---|---|---|
| 1 | NguoiDung | Tài khoản đăng nhập tất cả người dùng |
| 2 | HocVien | Thông tin chi tiết học viên |
| 3 | GiangVien | Thông tin chi tiết giảng viên |
| 4 | KhoaHoc | Thông tin các khóa học |
| 5 | LopHoc | Các lớp học thuộc khóa học |
| 6 | LichHoc | Lịch học chi tiết từng buổi |
| 7 | DangKyKhoaHoc | Đơn đăng ký lớp học của học viên |
| 8 | Diem | Điểm số từng học viên trong lớp |
| 9 | GoiYKhoaHoc | Lưu kết quả gợi ý AI cho học viên |
| 10 | ThongBao | Thông báo hệ thống |

### 4.2 Chi tiết từng bảng

---

#### Bảng NguoiDung

```sql
CREATE TABLE NguoiDung (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    Email           NVARCHAR(256)   UNIQUE NOT NULL,
    MatKhauHash     NVARCHAR(512)   NOT NULL,              -- BCrypt hash
    VaiTro          NVARCHAR(20)    NOT NULL               -- 'Admin' | 'GiangVien' | 'HocVien'
                    CHECK (VaiTro IN ('Admin','GiangVien','HocVien')),
    HoTen           NVARCHAR(100)   NOT NULL,
    SoDienThoai     NVARCHAR(15)    NULL,
    AnhDaiDien      NVARCHAR(500)   NULL,                  -- đường dẫn ảnh
    IsActive        BIT             NOT NULL DEFAULT 1,    -- 1: hoạt động, 0: bị khóa
    NgayTao         DATETIME        NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME        NULL
)
```

---

#### Bảng HocVien

```sql
CREATE TABLE HocVien (
    Id              INT             PRIMARY KEY
                    FOREIGN KEY REFERENCES NguoiDung(Id),  -- 1-1 với NguoiDung
    MaHocVien       NVARCHAR(20)    UNIQUE NOT NULL,        -- VD: HV001, HV002
    HoTen           NVARCHAR(100)   NOT NULL,
    NgaySinh        DATE            NULL,
    GioiTinh        NVARCHAR(10)    NULL,                   -- 'Nam' | 'Nữ' | 'Khác'
    DiaChi          NVARCHAR(300)   NULL,
    TrinhDoHienTai  NVARCHAR(50)    NULL,                   -- VD: 'Sơ cấp', 'Trung cấp', 'Cao cấp'
    NgonNguQuanTam  NVARCHAR(100)   NULL,                   -- VD: 'Tiếng Anh', 'Tiếng Nhật', 'Cả hai'
    NgayDangKy      DATETIME        NOT NULL DEFAULT GETDATE(),
    GhiChu          NVARCHAR(500)   NULL
)
```

---

#### Bảng GiangVien

```sql
CREATE TABLE GiangVien (
    Id              INT             PRIMARY KEY
                    FOREIGN KEY REFERENCES NguoiDung(Id),  -- 1-1 với NguoiDung
    MaGiangVien     NVARCHAR(20)    UNIQUE NOT NULL,        -- VD: GV001
    HoTen           NVARCHAR(100)   NOT NULL,
    ChuyenMon       NVARCHAR(100)   NULL,                   -- VD: 'Tiếng Anh', 'Tiếng Nhật'
    BangCap         NVARCHAR(100)   NULL,                   -- VD: 'Thạc sĩ', 'Cử nhân'
    KinhNghiem      INT             NULL,                   -- số năm kinh nghiệm
    MoTa            NVARCHAR(500)   NULL
)
```

---

#### Bảng KhoaHoc

```sql
CREATE TABLE KhoaHoc (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    TenKhoaHoc      NVARCHAR(200)   NOT NULL,
    MoTa            NVARCHAR(MAX)   NULL,
    NgonNgu         NVARCHAR(50)    NOT NULL,               -- 'Tiếng Anh' | 'Tiếng Nhật'
    TrinhDo         NVARCHAR(50)    NOT NULL,               -- 'Sơ cấp' | 'Trung cấp' | 'Cao cấp' | 'IELTS' | 'TOEIC' | 'N5' | 'N4' | 'N3' | 'N2' | 'N1'
    HocPhi          DECIMAL(18,2)   NOT NULL DEFAULT 0,
    ThoiLuong       INT             NOT NULL,               -- số buổi học
    SoBuoiMoiTuan   INT             NULL,                   -- VD: 3 buổi/tuần
    ThoiGianMoiBuoi INT             NULL,                   -- phút/buổi
    AnhBia          NVARCHAR(500)   NULL,                   -- đường dẫn ảnh
    NoiDungChuongTrinh NVARCHAR(MAX) NULL,                  -- mô tả chi tiết nội dung
    TrangThai       NVARCHAR(50)    NOT NULL DEFAULT 'DangMo'  -- 'DangMo' | 'DaDong' | 'TamDung'
                    CHECK (TrangThai IN ('DangMo','DaDong','TamDung')),
    NgayTao         DATETIME        NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME        NULL
)
```

---

#### Bảng LopHoc

```sql
CREATE TABLE LopHoc (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    TenLop          NVARCHAR(100)   NOT NULL,               -- VD: 'Anh Văn Sơ Cấp K01/2025'
    KhoaHocId       INT             NOT NULL
                    FOREIGN KEY REFERENCES KhoaHoc(Id),
    GiangVienId     INT             NULL
                    FOREIGN KEY REFERENCES GiangVien(Id),
    NgayKhaiGiang   DATE            NULL,
    NgayKetThuc     DATE            NULL,
    SiSoToiDa       INT             NOT NULL DEFAULT 20,
    PhongHoc        NVARCHAR(50)    NULL,
    TrangThai       NVARCHAR(50)    NOT NULL DEFAULT 'ChuaMo'  -- 'ChuaMo' | 'DangTuyenSinh' | 'DangHoc' | 'DaKetThuc'
                    CHECK (TrangThai IN ('ChuaMo','DangTuyenSinh','DangHoc','DaKetThuc')),
    GhiChu          NVARCHAR(500)   NULL,
    NgayTao         DATETIME        NOT NULL DEFAULT GETDATE()
)
```

---

#### Bảng LichHoc

```sql
CREATE TABLE LichHoc (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    LopHocId        INT             NOT NULL
                    FOREIGN KEY REFERENCES LopHoc(Id),
    NgayHoc         DATE            NOT NULL,
    GioBatDau       TIME            NOT NULL,
    GioKetThuc      TIME            NOT NULL,
    PhongHoc        NVARCHAR(50)    NULL,
    ChuDe           NVARCHAR(200)   NULL,                   -- chủ đề buổi học
    GhiChu          NVARCHAR(500)   NULL,
    TrangThai       NVARCHAR(30)    NOT NULL DEFAULT 'ChuaDienRa' -- 'ChuaDienRa' | 'DaDienRa' | 'HuyBo'
                    CHECK (TrangThai IN ('ChuaDienRa','DaDienRa','HuyBo'))
)
```

---

#### Bảng DangKyKhoaHoc

```sql
CREATE TABLE DangKyKhoaHoc (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    HocVienId       INT             NOT NULL
                    FOREIGN KEY REFERENCES HocVien(Id),
    LopHocId        INT             NOT NULL
                    FOREIGN KEY REFERENCES LopHoc(Id),
    NgayDangKy      DATETIME        NOT NULL DEFAULT GETDATE(),
    TrangThai       NVARCHAR(50)    NOT NULL DEFAULT 'ChoDuyet'  -- 'ChoDuyet' | 'DaDuyet' | 'TuChoi' | 'DaHuy'
                    CHECK (TrangThai IN ('ChoDuyet','DaDuyet','TuChoi','DaHuy')),
    LyDoTuChoi      NVARCHAR(500)   NULL,
    NguoiDuyetId    INT             NULL
                    FOREIGN KEY REFERENCES NguoiDung(Id),
    NgayDuyet       DATETIME        NULL,
    GhiChu          NVARCHAR(500)   NULL,
    -- Ràng buộc: mỗi học viên chỉ có 1 đăng ký active (ChoDuyet hoặc DaDuyet) cho mỗi lớp
    CONSTRAINT UQ_DangKy_HocVien_Lop UNIQUE (HocVienId, LopHocId)
)
```

---

#### Bảng Diem

```sql
CREATE TABLE Diem (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    DangKyId        INT             NOT NULL
                    FOREIGN KEY REFERENCES DangKyKhoaHoc(Id),
    DiemGiuaKy      FLOAT           NULL  CHECK (DiemGiuaKy BETWEEN 0 AND 10),
    DiemCuoiKy      FLOAT           NULL  CHECK (DiemCuoiKy BETWEEN 0 AND 10),
    DiemTongKet     FLOAT           NULL  CHECK (DiemTongKet BETWEEN 0 AND 10),
    XepLoai         NVARCHAR(20)    NULL,   -- 'Xuất sắc' | 'Giỏi' | 'Khá' | 'Trung bình' | 'Yếu'
    NhanXetGiangVien NVARCHAR(500)  NULL,
    IsKhoa          BIT             NOT NULL DEFAULT 0,   -- Admin khóa = không cho sửa
    NgayCapNhat     DATETIME        NULL
)
```

---

#### Bảng GoiYKhoaHoc

```sql
CREATE TABLE GoiYKhoaHoc (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    HocVienId       INT             NOT NULL
                    FOREIGN KEY REFERENCES HocVien(Id),
    KhoaHocGoiYId   INT             NOT NULL
                    FOREIGN KEY REFERENCES KhoaHoc(Id),
    DiemPhuHop      FLOAT           NULL,   -- điểm phù hợp (0-100) do AI trả về
    LyDoGoiY        NVARCHAR(MAX)   NULL,   -- giải thích của AI
    PromptGuiDi     NVARCHAR(MAX)   NULL,   -- lưu prompt đã gửi cho Gemini (debug)
    PhanHoiAI       NVARCHAR(MAX)   NULL,   -- lưu raw response từ Gemini (debug)
    NgayGoiY        DATETIME        NOT NULL DEFAULT GETDATE(),
    DaXem           BIT             NOT NULL DEFAULT 0
)
```

---

#### Bảng ThongBao

```sql
CREATE TABLE ThongBao (
    Id              INT             PRIMARY KEY IDENTITY(1,1),
    NguoiNhanId     INT             NOT NULL
                    FOREIGN KEY REFERENCES NguoiDung(Id),
    TieuDe          NVARCHAR(200)   NOT NULL,
    NoiDung         NVARCHAR(MAX)   NULL,
    LoaiThongBao    NVARCHAR(50)    NULL,   -- 'DangKy' | 'LichHoc' | 'Diem' | 'HeThong'
    DaDoc           BIT             NOT NULL DEFAULT 0,
    NgayTao         DATETIME        NOT NULL DEFAULT GETDATE(),
    DuongDanLienKet NVARCHAR(500)   NULL    -- URL điều hướng khi click thông báo
)
```

---

### 4.3 Quan hệ giữa các bảng

```
NguoiDung  1──1  HocVien
NguoiDung  1──1  GiangVien
KhoaHoc    1──n  LopHoc
LopHoc     1──n  LichHoc
GiangVien  1──n  LopHoc
HocVien    1──n  DangKyKhoaHoc
LopHoc     1──n  DangKyKhoaHoc
DangKyKhoaHoc 1──1 Diem
HocVien    1──n  GoiYKhoaHoc
KhoaHoc    1──n  GoiYKhoaHoc
NguoiDung  1──n  ThongBao
```

---

## 5. Yêu cầu AI Gợi ý Khóa học

### 5.1 Mô tả tính năng

Hệ thống sử dụng **Google Gemini API** để phân tích hồ sơ học viên và danh sách khóa học hiện có, từ đó đưa ra danh sách tối đa **3 khóa học phù hợp nhất** kèm lý do giải thích.

### 5.2 Điểm tích hợp trong hệ thống

| Vị trí | Mô tả |
|---|---|
| Dashboard học viên | Hiển thị gợi ý nổi bật ngay khi đăng nhập |
| Trang danh sách khóa học | Nút "Gợi ý cho tôi" → gọi AI và filter danh sách |
| Trang profile học viên | Xem lại lịch sử gợi ý AI |
| Admin: xem log | Admin xem lịch sử tất cả gợi ý đã thực hiện |

### 5.3 Input (Dữ liệu đầu vào cho Gemini)

```json
{
  "hocVien": {
    "hoTen": "Nguyễn Văn A",
    "trinhDoHienTai": "Sơ cấp",
    "ngonNguQuanTam": "Tiếng Anh",
    "lichSuHocTap": [
      {
        "tenKhoaHoc": "Tiếng Anh Giao tiếp Cơ bản",
        "trinhDo": "Sơ cấp",
        "diemTongKet": 8.5,
        "xepLoai": "Giỏi",
        "trangThai": "Đã hoàn thành"
      }
    ]
  },
  "danhSachKhoaHocHienCo": [
    {
      "id": 1,
      "tenKhoaHoc": "Tiếng Anh Giao tiếp Trung cấp",
      "ngonNgu": "Tiếng Anh",
      "trinhDo": "Trung cấp",
      "hocPhi": 2500000,
      "moTa": "Khóa học phát triển kỹ năng giao tiếp trình độ B1..."
    },
    {
      "id": 2,
      "tenKhoaHoc": "TOEIC 450+",
      "ngonNgu": "Tiếng Anh",
      "trinhDo": "Trung cấp",
      "hocPhi": 3200000,
      "moTa": "Luyện thi TOEIC từ 0 lên 450 điểm..."
    }
  ]
}
```

### 5.4 Prompt mẫu gửi cho Gemini

```
Bạn là chuyên gia tư vấn giáo dục ngôn ngữ tại Trung tâm Ngoại ngữ ABC.

Hãy phân tích hồ sơ học viên và danh sách khóa học bên dưới, sau đó đề xuất TỐI ĐA 3 khóa học phù hợp nhất cho học viên này.

THÔNG TIN HỌC VIÊN:
{hocVienJson}

DANH SÁCH KHÓA HỌC HIỆN CÓ:
{danhSachKhoaHocJson}

Hãy trả về KẾT QUẢ DUY NHẤT dưới dạng JSON hợp lệ (không có markdown, không có text thêm), theo đúng cấu trúc sau:
{
  "goiY": [
    {
      "khoaHocId": <id số nguyên>,
      "tenKhoaHoc": "<tên khóa học>",
      "diemPhuHop": <số thực 0-100>,
      "lyDo": "<giải thích ngắn gọn 1-2 câu tại sao khóa học này phù hợp>"
    }
  ]
}

Chỉ đề xuất các khóa học thực sự phù hợp với trình độ và mục tiêu của học viên. Sắp xếp theo mức độ phù hợp giảm dần.
```

### 5.5 Output (Kết quả từ Gemini)

```json
{
  "goiY": [
    {
      "khoaHocId": 1,
      "tenKhoaHoc": "Tiếng Anh Giao tiếp Trung cấp",
      "diemPhuHop": 92.5,
      "lyDo": "Học viên đã hoàn thành xuất sắc cấp sơ cấp với điểm 8.5, đây là bước tiếp theo phù hợp nhất để phát triển kỹ năng giao tiếp lên trình độ B1."
    },
    {
      "khoaHocId": 2,
      "tenKhoaHoc": "TOEIC 450+",
      "diemPhuHop": 75.0,
      "lyDo": "Với nền tảng vững chắc ở cấp sơ cấp, học viên có thể bắt đầu luyện thi TOEIC để có chứng chỉ quốc tế hữu ích cho công việc."
    }
  ]
}
```

### 5.6 Xử lý lỗi AI

| Trường hợp | Xử lý |
|---|---|
| Gemini API timeout / lỗi kết nối | Hiển thị thông báo "Không thể kết nối AI, vui lòng thử lại sau", không lưu kết quả |
| Response không phải JSON hợp lệ | Bắt exception, log lỗi, hiển thị thông báo lỗi |
| goiY trả về rỗng | Hiển thị "Hiện chưa có gợi ý phù hợp, hãy khám phá tất cả khóa học" |
| KhoaHocId không tồn tại trong DB | Bỏ qua gợi ý đó, chỉ hiển thị các gợi ý hợp lệ |
| Học viên chưa có lịch sử học | Vẫn gọi AI với lịch sử rỗng, AI gợi ý dựa trên ngôn ngữ quan tâm và trình độ |

### 5.7 Service class mẫu

```csharp
// Services/GeminiService.cs
public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public async Task<List<GoiYKhoaHocDto>> GetGoiYKhoaHoc(HocVienProfileDto profile, List<KhoaHocDto> danhSachKhoaHoc)
    {
        var prompt = BuildPrompt(profile, danhSachKhoaHoc);
        var requestBody = new {
            contents = new[] {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var apiKey = _config["GeminiAPI:ApiKey"];
        var endpoint = $"{_config["GeminiAPI:Endpoint"]}?key={apiKey}";

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody);
        var responseText = ExtractTextFromGeminiResponse(await response.Content.ReadAsStringAsync());

        // Parse JSON từ responseText
        var result = JsonConvert.DeserializeObject<GeminiGoiYResponse>(CleanJsonResponse(responseText));
        return result?.GoiY ?? new List<GoiYKhoaHocDto>();
    }
}
```

---

## 6. Cấu trúc thư mục Project

```
NgoaiNguABC/
├── NgoaiNguABC.sln
└── NgoaiNguABC/
    ├── Controllers/
    │   ├── AccountController.cs        -- Đăng nhập, đăng xuất, đổi mật khẩu
    │   ├── AdminController.cs          -- Dashboard admin
    │   ├── KhoaHocController.cs        -- CRUD khóa học
    │   ├── LopHocController.cs         -- CRUD lớp học
    │   ├── LichHocController.cs        -- CRUD lịch học
    │   ├── HocVienController.cs        -- CRUD học viên (Admin)
    │   ├── GiangVienController.cs      -- CRUD giảng viên (Admin)
    │   ├── DangKyController.cs         -- Đăng ký, duyệt, hủy
    │   ├── DiemController.cs           -- Nhập điểm, xem điểm
    │   ├── GoiYController.cs           -- AI gợi ý khóa học
    │   ├── ThongBaoController.cs       -- Thông báo
    │   └── ProfileController.cs        -- Thông tin cá nhân
    │
    ├── Models/
    │   ├── Entities/                   -- EF Core entity classes (ánh xạ với DB)
    │   │   ├── NguoiDung.cs
    │   │   ├── HocVien.cs
    │   │   ├── GiangVien.cs
    │   │   ├── KhoaHoc.cs
    │   │   ├── LopHoc.cs
    │   │   ├── LichHoc.cs
    │   │   ├── DangKyKhoaHoc.cs
    │   │   ├── Diem.cs
    │   │   ├── GoiYKhoaHoc.cs
    │   │   └── ThongBao.cs
    │   └── ViewModels/                 -- ViewModel cho Views
    │       ├── LoginViewModel.cs
    │       ├── DangKyViewModel.cs
    │       ├── KhoaHocViewModel.cs
    │       ├── DiemViewModel.cs
    │       ├── GoiYViewModel.cs
    │       └── DashboardViewModel.cs
    │
    ├── Data/
    │   ├── AppDbContext.cs             -- DbContext chính
    │   └── SeedData.cs                 -- Dữ liệu mẫu khởi tạo
    │
    ├── Services/
    │   ├── GeminiService.cs            -- Gọi Gemini API và parse kết quả
    │   ├── GoiYKhoaHocService.cs       -- Logic xây dựng prompt và lưu kết quả
    │   ├── ThongBaoService.cs          -- Tạo và gửi thông báo
    │   └── ExcelService.cs             -- Xuất Excel (ClosedXML)
    │
    ├── Filters/
    │   └── AuthorizeRoleAttribute.cs   -- Custom authorization attribute
    │
    ├── Views/
    │   ├── Shared/
    │   │   ├── _Layout.cshtml          -- Layout chung
    │   │   ├── _LayoutAdmin.cshtml     -- Layout dành cho Admin
    │   │   ├── _LayoutGiangVien.cshtml
    │   │   ├── _LayoutHocVien.cshtml
    │   │   ├── _Navbar.cshtml
    │   │   └── _Sidebar.cshtml
    │   ├── Account/
    │   │   ├── Login.cshtml
    │   │   └── ChangePassword.cshtml
    │   ├── Admin/
    │   │   └── Index.cshtml            -- Dashboard admin
    │   ├── KhoaHoc/
    │   │   ├── Index.cshtml            -- Danh sách
    │   │   ├── Create.cshtml
    │   │   ├── Edit.cshtml
    │   │   └── Details.cshtml
    │   ├── LopHoc/
    │   ├── LichHoc/
    │   ├── HocVien/
    │   ├── GiangVien/
    │   ├── DangKy/
    │   ├── Diem/
    │   ├── GoiY/
    │   │   ├── Index.cshtml            -- Hiển thị gợi ý AI
    │   │   └── LichSu.cshtml
    │   └── Profile/
    │
    ├── wwwroot/
    │   ├── css/
    │   │   └── site.css
    │   ├── js/
    │   │   └── site.js
    │   ├── lib/                        -- Bootstrap, jQuery, Chart.js
    │   └── uploads/                    -- Ảnh upload (ảnh đại diện, ảnh bìa khóa học)
    │
    ├── Migrations/                     -- EF Core migrations (tự sinh)
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Program.cs
```

---

## 7. API Endpoints

### 7.1 Authentication

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/Account/Login` | Hiển thị form đăng nhập | Public |
| POST | `/Account/Login` | Xử lý đăng nhập | Public |
| GET | `/Account/Logout` | Đăng xuất | Any |
| GET | `/Account/ChangePassword` | Form đổi mật khẩu | Any |
| POST | `/Account/ChangePassword` | Xử lý đổi mật khẩu | Any |

---

### 7.2 Admin — Khóa học

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/KhoaHoc` | Danh sách khóa học (có filter/search/paging) | Admin, GiangVien, HocVien |
| GET | `/KhoaHoc/Details/{id}` | Chi tiết khóa học | Any |
| GET | `/KhoaHoc/Create` | Form tạo khóa học | Admin |
| POST | `/KhoaHoc/Create` | Lưu khóa học mới | Admin |
| GET | `/KhoaHoc/Edit/{id}` | Form sửa khóa học | Admin |
| POST | `/KhoaHoc/Edit/{id}` | Cập nhật khóa học | Admin |
| POST | `/KhoaHoc/Delete/{id}` | Xóa khóa học | Admin |
| POST | `/KhoaHoc/ChangeStatus/{id}` | Đổi trạng thái | Admin |

---

### 7.3 Admin — Lớp học

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/LopHoc` | Danh sách lớp học | Admin, GiangVien |
| GET | `/LopHoc/Details/{id}` | Chi tiết + danh sách học viên | Admin, GiangVien |
| GET | `/LopHoc/Create` | Form tạo lớp | Admin |
| POST | `/LopHoc/Create` | Lưu lớp mới | Admin |
| GET | `/LopHoc/Edit/{id}` | Form sửa lớp | Admin |
| POST | `/LopHoc/Edit/{id}` | Cập nhật lớp | Admin |
| POST | `/LopHoc/Delete/{id}` | Xóa lớp | Admin |
| GET | `/LopHoc/DanhSachHocVien/{id}` | Học viên trong lớp | Admin, GiangVien |

---

### 7.4 Admin — Học viên & Giảng viên

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/HocVien` | Danh sách học viên | Admin |
| GET | `/HocVien/Details/{id}` | Chi tiết học viên | Admin |
| GET | `/HocVien/Create` | Form tạo học viên | Admin |
| POST | `/HocVien/Create` | Lưu học viên mới | Admin |
| GET | `/HocVien/Edit/{id}` | Form sửa | Admin |
| POST | `/HocVien/Edit/{id}` | Cập nhật | Admin |
| POST | `/HocVien/Delete/{id}` | Xóa (soft delete) | Admin |
| GET | `/HocVien/ExportExcel` | Xuất Excel | Admin |
| GET | `/GiangVien` | Danh sách giảng viên | Admin |
| GET | `/GiangVien/Create` | Form tạo giảng viên | Admin |
| POST | `/GiangVien/Create` | Lưu giảng viên | Admin |
| GET | `/GiangVien/Edit/{id}` | Form sửa | Admin |
| POST | `/GiangVien/Edit/{id}` | Cập nhật | Admin |
| POST | `/GiangVien/Delete/{id}` | Xóa | Admin |

---

### 7.5 Đăng ký Khóa học

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/DangKy` | Danh sách đơn đăng ký | Admin |
| GET | `/DangKy/CuaToi` | Đơn đăng ký của học viên đang đăng nhập | HocVien |
| POST | `/DangKy/DangKy` | Học viên nộp đơn đăng ký (body: lopHocId) | HocVien |
| POST | `/DangKy/Duyet/{id}` | Admin/GV duyệt đơn | Admin |
| POST | `/DangKy/TuChoi/{id}` | Admin/GV từ chối (body: lyDo) | Admin |
| POST | `/DangKy/Huy/{id}` | Học viên hoặc Admin hủy đơn | Admin, HocVien |

---

### 7.6 Điểm số

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/Diem/LopHoc/{lopHocId}` | Bảng điểm của lớp | Admin, GiangVien |
| GET | `/Diem/CuaToi` | Điểm của học viên đang đăng nhập | HocVien |
| POST | `/Diem/NhapDiem` | Nhập/cập nhật điểm (body: DiemViewModel) | Admin, GiangVien |
| POST | `/Diem/KhoaDiem/{lopHocId}` | Khóa điểm lớp (không cho sửa nữa) | Admin |
| GET | `/Diem/ExportExcel/{lopHocId}` | Xuất bảng điểm Excel | Admin, GiangVien |

---

### 7.7 AI Gợi ý Khóa học

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/GoiY` | Trang gợi ý khóa học của học viên | HocVien |
| POST | `/GoiY/TaoGoiY` | Gọi Gemini API và lưu kết quả (AJAX) | HocVien |
| GET | `/GoiY/LichSu` | Lịch sử gợi ý AI của học viên | HocVien |
| GET | `/GoiY/Admin` | Admin xem log tất cả gợi ý | Admin |

**Response của `/GoiY/TaoGoiY` (JSON):**
```json
{
  "success": true,
  "goiY": [
    {
      "khoaHocId": 1,
      "tenKhoaHoc": "Tiếng Anh Trung cấp",
      "diemPhuHop": 92.5,
      "lyDo": "Phù hợp với trình độ hiện tại...",
      "hocPhi": 2500000,
      "anhBia": "/uploads/anh-bia-1.jpg"
    }
  ]
}
```

---

### 7.8 Lịch học

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/LichHoc` | Lịch học toàn trung tâm (Admin) | Admin |
| GET | `/LichHoc/CuaToi` | Lịch học cá nhân | GiangVien, HocVien |
| GET | `/LichHoc/LopHoc/{lopHocId}` | Lịch của lớp cụ thể | Admin, GiangVien |
| POST | `/LichHoc/Create` | Thêm buổi học | Admin |
| POST | `/LichHoc/Edit/{id}` | Sửa buổi học | Admin |
| POST | `/LichHoc/Delete/{id}` | Xóa buổi học | Admin |
| POST | `/LichHoc/TaoHangLoat` | Tạo nhiều buổi theo lịch lặp lại | Admin |

---

### 7.9 Thông báo

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/ThongBao` | Danh sách thông báo của người dùng | Any |
| POST | `/ThongBao/DanhDauDaDoc/{id}` | Đánh dấu đã đọc | Any |
| GET | `/ThongBao/SoChuaDoc` | Lấy số thông báo chưa đọc (AJAX) | Any |

---

### 7.10 Dashboard

| Method | URL | Mô tả | Auth |
|---|---|---|---|
| GET | `/` | Redirect theo vai trò | Any |
| GET | `/Admin` | Dashboard Admin | Admin |
| GET | `/GiangVien/Dashboard` | Dashboard Giảng viên | GiangVien |
| GET | `/HocVien/Dashboard` | Dashboard Học viên | HocVien |

---

## 8. Seed Data khởi tạo

Khi chạy lần đầu, hệ thống tự tạo dữ liệu mẫu:

```csharp
// Data/SeedData.cs
// 1 tài khoản Admin: admin@abc.edu.vn / Admin@123456
// 2 giảng viên mẫu với tài khoản
// 5 học viên mẫu
// 4 khóa học mẫu:
//   - Tiếng Anh Giao tiếp Sơ cấp
//   - Tiếng Anh Giao tiếp Trung cấp
//   - TOEIC 450+
//   - Tiếng Nhật N5
// 3 lớp học đang mở tuyển sinh
// Lịch học mẫu cho các lớp
```

---

## 9. Yêu cầu phi chức năng

| Yêu cầu | Chi tiết |
|---|---|
| Bảo mật | Mật khẩu hash BCrypt, Cookie Authentication, phân quyền Role-based, chống CSRF (AntiForgeryToken) |
| Hiệu năng | Phản hồi < 2 giây cho CRUD thông thường. Dùng async/await cho tất cả DB và API call |
| Giao diện | Bootstrap 5, responsive (mobile/tablet/desktop), hỗ trợ Chrome, Firefox, Edge |
| Đa người dùng | Hỗ trợ nhiều người dùng truy cập đồng thời |
| Validation | Server-side validation (DataAnnotations) + Client-side validation (jQuery Validate) |
| Logging | Ghi log lỗi hệ thống và lỗi Gemini API |
| Upload file | Ảnh đại diện, ảnh bìa khóa học (giới hạn 5MB, chấp nhận jpg/png) |

---

## 10. Lưu ý triển khai

```
- Chạy migrations: dotnet ef database update
- Cấu hình Gemini API key trong appsettings.json (không commit key lên git)
- Thư mục wwwroot/uploads cần quyền ghi
- SQL Server LocalDB cho môi trường dev
- Gemini free tier: giới hạn 15 requests/phút — cần cache kết quả gợi ý (lưu vào bảng GoiYKhoaHoc)
- Không gọi Gemini API mỗi lần load trang; chỉ gọi khi học viên nhấn nút "Gợi ý cho tôi"
  hoặc khi dữ liệu học viên thay đổi (đăng ký mới, hoàn thành khóa học)
```
