# PHÂN TÍCH HỆ THỐNG – QuanLyKhoaHoc5

> **Ngày phân tích:** 25/05/2026  
> **Nguồn:** Đọc toàn bộ Controllers/, Models/, Views/, Services/, Data/  
> **Công nghệ:** ASP.NET Core MVC (.NET 10), EF Core, SQL Server, BCrypt.Net-Next, ClosedXML, iTextSharp

---

## 1. DANH SÁCH ĐẦY ĐỦ URL / ROUTE

### 1.1 Xác thực (AccountController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /Account/Login | Hiển thị form đăng nhập |
| POST | /Account/Login | Xử lý đăng nhập (cookie "CookieAuth") |
| GET | /Account/Logout | Đăng xuất, redirect đến Login |
| GET | /Account/AccessDenied | Trang không có quyền |
| GET | /Account/ChangePassword | Form đổi mật khẩu |
| POST | /Account/ChangePassword | Xử lý đổi mật khẩu |

### 1.2 Admin Dashboard (AdminController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /Admin | Dashboard admin (charts, stats) |
| GET | /Admin/PhanCong | Quản lý phân công GV |
| POST | /Admin/DoPhanCong | Thực hiện phân công |
| POST | /Admin/HuyPhanCong | Hủy phân công |
| GET | /Admin/LichSuPhanCong?khoaHocId= | Lịch sử phân công |
| GET | /Admin/TaiKhoan | DS tài khoản (legacy) |
| POST | /Admin/KhoaTaiKhoan | Khóa/mở tài khoản (legacy) |
| POST | /Admin/DoiRole | Đổi vai trò (legacy) |
| POST | /Admin/ResetMatKhau | Reset mật khẩu (legacy) |
| POST | /Admin/TaoTaiKhoan | Tạo tài khoản (legacy) |
| GET | /Admin/BaoCao | Trang báo cáo |
| GET | /Admin/ExportExcel | Xuất Excel báo cáo |
| GET | /Admin/ExportPdf | Xuất PDF báo cáo |

### 1.3 Quản lý tài khoản (TaiKhoanController – AJAX)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /TaiKhoan | Danh sách + filter vai trò/search |
| GET | /TaiKhoan/Create | Redirect về Index |
| POST | /TaiKhoan/Create | AJAX tạo tài khoản mới |
| POST | /TaiKhoan/KhoaTaiKhoan/{id} | AJAX khóa/mở khóa toggle |
| POST | /TaiKhoan/ResetMatKhau/{id} | AJAX reset mật khẩu mặc định |
| POST | /TaiKhoan/SuaVaiTro | AJAX đổi vai trò (có kiểm tra ràng buộc) |

### 1.4 Khóa học (KhoaHocController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /KhoaHoc | DS khóa học (filter: ngôn ngữ, trình độ, search, paging) |
| GET | /KhoaHoc/Details/{id} | Chi tiết khóa học |
| GET | /KhoaHoc/Create | [Admin] Form tạo mới |
| POST | /KhoaHoc/Create | [Admin] Lưu tạo mới |
| GET | /KhoaHoc/Edit/{id} | [Admin] Form chỉnh sửa |
| POST | /KhoaHoc/Edit/{id} | [Admin] Lưu chỉnh sửa |
| POST | /KhoaHoc/Delete/{id} | [Admin] Xóa (nếu không có lớp) |
| POST | /KhoaHoc/ChangeStatus/{id} | [Admin] Xoay vòng trạng thái AJAX |

### 1.5 Lớp học (LopHocController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /LopHoc | DS lớp (Admin: tất cả, GV: của mình) |
| GET | /LopHoc/Details/{id} | Chi tiết lớp |
| GET | /LopHoc/Create | [Admin] Form tạo lớp |
| POST | /LopHoc/Create | [Admin] Lưu tạo lớp |
| GET | /LopHoc/Edit/{id} | [Admin] Form sửa lớp |
| POST | /LopHoc/Edit/{id} | [Admin] Lưu sửa lớp |
| POST | /LopHoc/Delete/{id} | [Admin] Xóa (nếu không có học viên) |

### 1.6 Đăng ký khóa học (DangKyController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /DangKy | [Admin] DS tất cả đăng ký + filter |
| GET | /DangKy/CuaToi | [HocVien] Đăng ký của tôi + DS lớp mở |
| POST | /DangKy/DangKy | [HocVien] Nộp đơn đăng ký |
| POST | /DangKy/Duyet/{id} | [Admin] Duyệt đơn |
| POST | /DangKy/TuChoi/{id} | [Admin] Từ chối đơn |
| POST | /DangKy/Huy/{id} | [HocVien/Admin] Hủy đơn |

### 1.7 Điểm số (DiemController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /Diem/LopHoc?lopHocId= | [Admin/GV] Bảng điểm lớp |
| GET | /Diem/CuaToi | [HocVien] Điểm của tôi |
| POST | /Diem/NhapDiem | [Admin/GV] AJAX nhập điểm |
| POST | /Diem/KhoaDiem | [Admin] Khóa bảng điểm |
| GET | /Diem/ExportExcel?lopHocId= | [Admin/GV] Xuất Excel bảng điểm |

### 1.8 Thanh toán (ThanhToanController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /ThanhToan/CuaToi | [HocVien] Danh sách học phí |
| GET | /ThanhToan/TaoYeuCau?khoaHocId= | [HocVien] Form tạo yêu cầu thanh toán |
| POST | /ThanhToan/TaoYeuCau | [HocVien] Gửi yêu cầu thanh toán |
| GET | /ThanhToan | [Admin] DS thanh toán + filter |
| GET | /ThanhToan/ChiTiet/{id} | [Admin] Chi tiết thanh toán |
| POST | /ThanhToan/Duyet | [Admin] Duyệt/từ chối thanh toán |
| GET | /ThanhToan/ThongKe6Thang | [Admin] API thống kê 6 tháng (JSON) |

### 1.9 Lịch học (LichHocController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /LichHoc | [HocVien] Lịch học của tôi |
| GET | /LichHoc/Them?lopHocId= | [Admin/GV] Form thêm buổi học |
| POST | /LichHoc/Them | [Admin/GV] Lưu thêm buổi học |
| POST | /LichHoc/Xoa/{id} | [Admin/GV] Xóa buổi học |

### 1.10 Giảng viên (GiangVienController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /GiangVien/Dashboard | [GiangVien] Dashboard cá nhân |
| GET | /GiangVien/LichDay | [GiangVien] Lịch dạy |
| GET | /GiangVien | [Admin] DS giảng viên |
| GET | /GiangVien/Details/{id} | [Admin] Chi tiết GV |
| GET | /GiangVien/Create | [Admin] Form tạo GV |
| POST | /GiangVien/Create | [Admin] Lưu tạo GV |
| GET | /GiangVien/Edit/{id} | [Admin] Form sửa GV |
| POST | /GiangVien/Edit/{id} | [Admin] Lưu sửa GV |
| POST | /GiangVien/Delete/{id} | [Admin] Xóa GV (soft delete) |

### 1.11 Hồ sơ (ProfileController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /Profile | Hồ sơ cá nhân |
| POST | /Profile/Edit | Cập nhật hồ sơ |
| POST | /Profile/UploadAvatar | Upload ảnh đại diện |

### 1.12 Thông báo (ThongBaoController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /ThongBao | DS thông báo |
| POST | /ThongBao/DocTatCa | Đánh dấu tất cả đã đọc |
| GET | /ThongBao/Count | API lấy số chưa đọc (JSON) |

### 1.13 AI (GoiYController, ChatController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /GoiY | [HocVien] Gợi ý khóa học AI |
| POST | /GoiY/Generate | [HocVien] Tạo gợi ý (Groq/Gemini) |
| GET | /Chat | [Authenticated] Chat widget |
| POST | /Chat/Send | [Authenticated] Gửi tin nhắn |

### 1.14 Báo cáo (BaoCaoController)
| Method | URL | Mô tả |
|--------|-----|--------|
| GET | /BaoCao | [Admin] Tổng quan báo cáo |

---

## 2. CHỨC NĂNG THEO TỪNG ROLE

### 2.1 Admin (toàn quyền)
- **Dashboard:** Xem KPI (HV đang học, KH đang mở, GV, doanh thu), biểu đồ thanh/đường, top 5 KH, đăng ký gần đây
- **Khóa học:** CRUD + đổi trạng thái (DangMo/DaDong/TamDung), upload ảnh bìa, xem tất cả trạng thái
- **Lớp học:** CRUD + filter, xem DS học viên trong lớp
- **Giảng viên:** CRUD + soft delete, phân công GV vào KH
- **Đăng ký:** Xem tất cả, duyệt/từ chối kèm lý do, hủy
- **Điểm:** Nhập điểm, khóa bảng điểm, xuất Excel
- **Thanh toán:** Duyệt/từ chối yêu cầu, thống kê thu theo tháng, xuất báo cáo Excel/PDF
- **Tài khoản:** Tạo/khóa/reset/đổi vai trò (có kiểm tra ràng buộc), badge "Bạn" cho chính mình
- **Thông báo:** Nhận + đọc
- **Báo cáo:** Xuất Excel, xuất PDF

### 2.2 GiangVien
- **Dashboard:** Lịch hôm nay, lịch tuần, DS lớp đang dạy, số HV
- **Lịch dạy:** Xem toàn bộ lịch học từ hôm nay
- **Lớp học:** Chỉ xem lớp của mình, xem chi tiết DS HV
- **Điểm:** Nhập/sửa điểm lớp của mình (nếu chưa khóa), xuất Excel
- **Lịch học:** Thêm/xóa buổi học trong lớp của mình
- **Thông báo:** Nhận + đọc
- **Hồ sơ:** Xem/sửa thông tin cá nhân, đổi mật khẩu, upload ảnh
- **AI Chat:** Sử dụng chat widget

### 2.3 HocVien
- **Dashboard:** Xem kết quả học tập, lịch học sắp tới, thông báo
- **Khóa học:** Xem danh sách (chỉ DangMo), xem chi tiết + lịch học + đăng ký
- **Đăng ký:** Đăng ký mới/hủy (ChoDuyet), xem trạng thái
- **Điểm:** Xem điểm các môn đã đăng ký
- **Thanh toán:** Tạo yêu cầu thanh toán, xem trạng thái
- **Thông báo:** Nhận + đọc
- **Hồ sơ:** Xem/sửa, đổi mật khẩu, upload ảnh
- **AI Gợi ý:** Nhận gợi ý khóa học từ AI
- **AI Chat:** Sử dụng chat widget

---

## 3. CÁC TRƯỜNG INPUT THEO TỪNG FORM

### 3.1 Đăng nhập
| Field | Name attribute | Type | Validation |
|-------|---------------|------|-----------|
| Email | Email | email | Required |
| Mật khẩu | MatKhau | password | Required |
| Ghi nhớ | GhiNho | checkbox | Optional |

### 3.2 Đổi mật khẩu
| Field | Name attribute | Validation |
|-------|---------------|-----------|
| Mật khẩu hiện tại | MatKhauCu | Required, BCrypt verify |
| Mật khẩu mới | MatKhauMoi | Required, ≥6 ký tự |
| Xác nhận | XacNhanMatKhau | Must match MatKhauMoi |

### 3.3 Tạo tài khoản mới (TaiKhoan/Create AJAX)
| Field | ID | Validation |
|-------|-----|-----------|
| Họ tên | cr-hoTen | Required, not whitespace |
| Email | cr-email | Required, contains @, contains . |
| Vai trò | cr-vaiTro | Admin/GiangVien/HocVien |
| Mật khẩu | cr-matKhau | ≥6 ký tự |
| Xác nhận | cr-xacNhan | == matKhau |

### 3.4 Khóa học Create/Edit
| Field | Validation |
|-------|-----------|
| TenKhoaHoc | Required |
| NgonNgu | Required |
| TrinhDo | Required |
| HocPhi | Required, decimal ≥ 0 |
| SoChoToiDa | Required, int ≥ 1 |
| ThoiLuong | Required (giờ) |
| SoBuoiMoiTuan | Required |
| ThoiGianMoiBuoi | Required (phút) |
| TrangThai | DangMo/DaDong/TamDung |
| AnhBia | Optional, jpg/jpeg/png, max 5MB |

### 3.5 Lớp học Create/Edit
| Field | Validation |
|-------|-----------|
| TenLop | Required |
| KhoaHocId | Required, FK |
| GiangVienId | Optional, FK |
| NgayKhaiGiang | Required, DateOnly |
| NgayKetThuc | Required, DateOnly ≥ NgayKhaiGiang |
| SiSoToiDa | Required, int ≥ 1 |
| PhongHoc | Optional |
| TrangThai | DangTuyenSinh/DangHoc/DaKetThuc |

### 3.6 Đăng ký (DangKy/DangKy POST)
| Field | Validation |
|-------|-----------|
| lopHocId | Required, lớp đang DangTuyenSinh, còn chỗ |

### 3.7 Nhập điểm (Diem/NhapDiem AJAX)
| Field | Validation |
|-------|-----------|
| dangKyId | Required |
| diemGiuaKy | Optional, 0–10 |
| diemCuoiKy | Optional, 0–10 |
| nhanXet | Optional |

### 3.8 Thanh toán (ThanhToan/TaoYeuCau)
| Field | Validation |
|-------|-----------|
| KhoaHocId | Required, đã đăng ký DaDuyet |
| PhuongThuc | Required (TienMat/ChuyenKhoan/MoMo/VNPay) |
| GhiChu | Optional |

---

## 4. BUSINESS RULES

### BR-01: Xác thực & Phân quyền
- Chỉ tài khoản **IsActive = true** mới được đăng nhập
- Cookie auth có TTL: 8 giờ (thường), 30 ngày (ghi nhớ)
- `[AuthorizeRole]` custom attribute kiểm tra role từ ClaimTypes.Role
- Admin truy cập tất cả; GV chỉ xem lớp/điểm của mình; HV chỉ xem dữ liệu của mình

### BR-02: Đăng ký khóa học
- Chỉ HocVien mới được đăng ký
- Lớp phải có TrangThai = "DangTuyenSinh"
- Số HV đã DaDuyet < SiSoToiDa của lớp
- Không đăng ký lại lớp đang có status ≠ DaHuy/TuChoi
- Hủy chỉ được khi TrangThai = "ChoDuyet" (với HV); Admin hủy bất kỳ lúc

### BR-03: Điểm số
- Công thức: DiemTongKet = GiuaKy × 30% + CuoiKy × 70%
- XepLoai: Giỏi (≥8.5), Khá (7.0–8.4), Trung bình (5.0–6.9), Yếu (<5.0)
- Khi bảng điểm đã **Khóa** (IsKhoa=true), chỉ Admin mới sửa được
- Bản ghi Diem tạo tự động khi Admin duyệt đăng ký

### BR-04: Thanh toán
- Chỉ HocVien đã DaDuyet mới tạo được yêu cầu
- Không tạo yêu cầu mới khi đã có yêu cầu ChoPheduyet cho cùng khóa học
- Admin duyệt: trạng thái → DaThanhToan hoặc TuChoi

### BR-05: Quản lý tài khoản
- Admin không thể tự khóa/đổi vai trò chính mình (badge "Bạn" + ẩn nút)
- GiangVien đang phụ trách lớp → không đổi vai trò
- HocVien có DangKy đang tồn tại → không đổi vai trò
- Reset mật khẩu về DefaultPassword (appsettings: "Abc@12345")
- Email phải unique trong hệ thống

### BR-06: Khóa học
- Không xóa KH đã có lớp học
- Không xóa LH đã có học viên đăng ký
- Trạng thái xoay vòng: DangMo → DaDong → TamDung → DangMo (AJAX)
- HocVien chỉ thấy KH có TrangThai = "DangMo"

### BR-07: Giảng viên
- Không xóa GV đang phụ trách lớp TrangThai DangHoc/DangTuyenSinh
- Khi xóa: soft delete (IsActive=false), không xóa record
- Khi tạo GV qua TaiKhoan/Create: tự động tạo record GiangVien với MaGiangVien (GVxxx)
- Khi tạo HV qua TaiKhoan/Create: tự động tạo record HocVien với MaHocVien (HVxxx)

### BR-08: Phân công giảng viên
- Mỗi khóa học chỉ có 1 phân công IsActive tại một thời điểm
- Khi phân công mới: deactivate tất cả phân công cũ của cùng khóa học
- Cập nhật luôn LopHoc.GiangVienId cho các lớp chưa kết thúc

### BR-09: AI Features
- GoiY: dùng API Groq/Gemini để gợi ý khóa học phù hợp với HocVien
- Chat: lưu lịch sử ChatHistory theo user
- Chỉ authenticated users mới dùng Chat

### BR-10: Thông báo
- Hệ thống tự tạo ThongBao khi: đăng ký mới (→ Admin), được duyệt/từ chối (→ HocVien)
- API GET /ThongBao/Count trả về số chưa đọc (dùng cho badge trên nav)
