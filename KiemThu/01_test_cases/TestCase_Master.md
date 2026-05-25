# TEST CASE MASTER – QuanLyKhoaHoc5

> **Phiên bản:** 1.0 | **Ngày tạo:** 25/05/2026  
> **Tổng số test case:** 55  
> **Phủ sóng module:** Login, KhoaHoc, LopHoc, DangKy, Diem, ThanhToan, TaiKhoan, AI

---

## MODULE 1: ĐĂNG NHẬP / ĐĂNG XUẤT (TC-001 → TC-010)

| TC ID | Tên test | Điều kiện đầu | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|--------------|----------------|-------------------|-----------|
| TC-001 | Đăng nhập Admin thành công | Server chạy, DB có seed | Vào /Account/Login; nhập Email=admin@nnl.com, MatKhau=Admin@123; click Đăng nhập | Redirect /Admin (dashboard), nav hiện "Admin" | Cao |
| TC-002 | Đăng nhập GiangVien thành công | Server chạy, DB có seed | Nhập Email=gv01@nnl.com, MatKhau=Gv@123 | Redirect /GiangVien/Dashboard, sidebar màu xanh lá | Cao |
| TC-003 | Đăng nhập HocVien thành công | Server chạy, DB có seed | Nhập Email=hv01@nnl.com, MatKhau=Hv@123 | Redirect /HocVien/Dashboard, sidebar màu tím | Cao |
| TC-004 | Đăng nhập sai mật khẩu | Có tài khoản hợp lệ | Email đúng, MatKhau sai "wrong123" | Ở lại /Account/Login, hiển thị lỗi "Email hoặc mật khẩu không đúng" | Cao |
| TC-005 | Đăng nhập email không tồn tại | – | Email không có trong DB | Thông báo lỗi "Email hoặc mật khẩu không đúng" | Trung bình |
| TC-006 | Đăng nhập tài khoản bị khóa | Tài khoản IsActive=false | Nhập đúng email/mật khẩu | Lỗi "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin." | Cao |
| TC-007 | Đăng nhập email rỗng | – | Bỏ trống Email, điền MatKhau | Form validation hiện lỗi required | Trung bình |
| TC-008 | Ghi nhớ đăng nhập | Đã đăng nhập | Tick "Ghi nhớ", đăng nhập thành công | Cookie có ExpiresUtc = now+30 ngày | Thấp |
| TC-009 | Đăng xuất | Đang đăng nhập | Click nút Đăng xuất | Redirect /Account/Login, session xóa, /Admin redirect về Login | Cao |
| TC-010 | Đổi mật khẩu thành công | Đang đăng nhập | GET /Account/ChangePassword; nhập mật khẩu cũ đúng, mới + xác nhận khớp | Thông báo thành công, redirect Profile | Trung bình |

---

## MODULE 2: KHÓA HỌC (TC-011 → TC-020)

| TC ID | Tên test | Role | Điều kiện đầu | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|--------------|----------------|-------------------|-----------|
| TC-011 | Admin xem DS khóa học (tất cả TT) | Admin | Có KH nhiều trạng thái | GET /KhoaHoc | Hiển thị cả KH DangMo, DaDong, TamDung | Cao |
| TC-012 | HocVien chỉ thấy KH DangMo | HocVien | Có KH DaDong | GET /KhoaHoc | Chỉ thấy TrangThai = DangMo | Cao |
| TC-013 | Filter khóa học theo ngôn ngữ | Admin | Có KH Tiếng Anh + Tiếng Nhật | Filter NgonNgu=TiengAnh | Chỉ hiện KH Tiếng Anh | Trung bình |
| TC-014 | Tạo khóa học mới (Admin) | Admin | – | POST /KhoaHoc/Create với data hợp lệ | Redirect DS, có KH mới trong bảng | Cao |
| TC-015 | Tạo KH thiếu tên (validation) | Admin | – | POST /KhoaHoc/Create, TenKhoaHoc rỗng | Ở lại trang Create, hiện lỗi required | Cao |
| TC-016 | Sửa khóa học (Admin) | Admin | Có KH ID=1 | GET Edit(1), thay đổi HocPhi, POST | KH cập nhật, ngayCapNhat được set | Cao |
| TC-017 | Xóa KH không có lớp (Admin) | Admin | KH mới tạo chưa có lớp | POST /KhoaHoc/Delete/{id} | KH bị xóa, redirect DS + success message | Trung bình |
| TC-018 | Xóa KH đã có lớp (thất bại) | Admin | KH đã có lớp học | POST /KhoaHoc/Delete/{id} | Lỗi "Không thể xóa khóa học đã có lớp học!" | Cao |
| TC-019 | Đổi trạng thái KH (AJAX) | Admin | KH đang DangMo | POST /KhoaHoc/ChangeStatus/{id} | JSON {success:true, newStatus:"DaDong"} | Cao |
| TC-020 | Xem chi tiết KH (lịch học, số HV) | Không cần đăng nhập | KH có lớp, có LichHoc | GET /KhoaHoc/Details/{id} | Hiện TenKhoaHoc, LichHoc sắp tới, số HV đã đăng ký | Trung bình |

---

## MODULE 3: LỚP HỌC (TC-021 → TC-026)

| TC ID | Tên test | Role | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|----------------|-------------------|-----------|
| TC-021 | Admin xem tất cả lớp | Admin | GET /LopHoc | Tất cả lớp của mọi GV | Cao |
| TC-022 | GV chỉ xem lớp của mình | GiangVien | GET /LopHoc | Chỉ lớp có GiangVienId = userId | Cao |
| TC-023 | Tạo lớp học mới (Admin) | Admin | POST /LopHoc/Create hợp lệ | Lớp mới tạo, có trong DS | Cao |
| TC-024 | Sửa lớp học | Admin | POST /LopHoc/Edit/{id} | Lớp cập nhật thành công | Trung bình |
| TC-025 | Xóa lớp chưa có HV | Admin | POST /LopHoc/Delete/{id} – lớp trống | Xóa thành công | Trung bình |
| TC-026 | Xóa lớp đã có HV (thất bại) | Admin | POST /LopHoc/Delete/{id} – có DangKy | Lỗi "Không thể xóa lớp đã có học viên đăng ký!" | Cao |

---

## MODULE 4: ĐĂNG KÝ KHÓA HỌC (TC-027 → TC-033)

| TC ID | Tên test | Role | Điều kiện đầu | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|--------------|----------------|-------------------|-----------|
| TC-027 | HV đăng ký lớp mở (thành công) | HocVien | Lớp DangTuyenSinh, còn chỗ | POST /DangKy/DangKy, lopHocId hợp lệ | TrangThai=ChoDuyet, thông báo thành công | Cao |
| TC-028 | HV đăng ký lớp đã đầy | HocVien | SiSoHienTai >= SiSoToiDa | POST /DangKy/DangKy | Lỗi "Lớp học đã đủ học viên" | Cao |
| TC-029 | HV đăng ký lớp đã đăng ký rồi | HocVien | Đã có DangKy ChoDuyet/DaDuyet | POST /DangKy/DangKy cùng lopHocId | Lỗi "Bạn đã đăng ký lớp này rồi" | Cao |
| TC-030 | Admin duyệt đăng ký | Admin | Có đơn ChoDuyet | POST /DangKy/Duyet/{id} | TrangThai=DaDuyet, Diem record tạo tự động | Cao |
| TC-031 | Admin từ chối đăng ký | Admin | Có đơn ChoDuyet | POST /DangKy/TuChoi/{id}, lyDo="Hết chỗ" | TrangThai=TuChoi, lưu lý do | Trung bình |
| TC-032 | HV hủy đơn ChoDuyet | HocVien | Đơn đang ChoDuyet | POST /DangKy/Huy/{id} | TrangThai=DaHuy | Trung bình |
| TC-033 | HV hủy đơn DaDuyet (thất bại) | HocVien | Đơn đã DaDuyet | POST /DangKy/Huy/{id} | Lỗi "Chỉ có thể hủy đơn đang chờ duyệt" | Cao |

---

## MODULE 5: ĐIỂM SỐ (TC-034 → TC-039)

| TC ID | Tên test | Role | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|----------------|-------------------|-----------|
| TC-034 | GV nhập điểm (GK + CK) | GiangVien | POST /Diem/NhapDiem, GK=7.5, CK=8.0 | DiemTongKet = 7.85, XepLoai=Khá | Cao |
| TC-035 | Công thức điểm tổng kết | Admin | Nhập GK=10, CK=10 | DiemTongKet=10.0 | Cao |
| TC-036 | Nhập điểm lớp đã khóa (GV thất bại) | GiangVien | POST /Diem/NhapDiem, IsKhoa=true | JSON {success:false, message:"Điểm đã bị khóa"} | Cao |
| TC-037 | Admin nhập điểm lớp đã khóa (thành công) | Admin | POST /Diem/NhapDiem, IsKhoa=true | Lưu thành công | Trung bình |
| TC-038 | Admin khóa bảng điểm | Admin | POST /Diem/KhoaDiem?lopHocId= | Tất cả Diem.IsKhoa=true | Cao |
| TC-039 | HV xem điểm của mình | HocVien | GET /Diem/CuaToi | DS điểm các khóa đã đăng ký DaDuyet | Trung bình |

---

## MODULE 6: THANH TOÁN (TC-040 → TC-045)

| TC ID | Tên test | Role | Điều kiện đầu | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|--------------|----------------|-------------------|-----------|
| TC-040 | HV tạo yêu cầu thanh toán | HocVien | Đã DaDuyet 1 KH | POST /ThanhToan/TaoYeuCau, PhuongThuc=TienMat | TrangThai=ChoPheduyet, redirect CuaToi | Cao |
| TC-041 | HV tạo yêu cầu khi chưa có đăng ký duyệt | HocVien | Không có DangKy DaDuyet | GET /ThanhToan/TaoYeuCau?khoaHocId=99 | Lỗi "Bạn chưa đăng ký hoặc chưa được duyệt" | Cao |
| TC-042 | HV tạo yêu cầu trùng (đang chờ duyệt) | HocVien | Đã có ChoPheduyet | POST /ThanhToan/TaoYeuCau | Warning "Đã có yêu cầu đang chờ duyệt" | Trung bình |
| TC-043 | Admin duyệt thanh toán | Admin | Có yêu cầu ChoPheduyet | POST /ThanhToan/Duyet, HanhDong=DaThanhToan | TrangThai=DaThanhToan, có người duyệt + ngày | Cao |
| TC-044 | Admin từ chối thanh toán | Admin | Có yêu cầu ChoPheduyet | POST /ThanhToan/Duyet, HanhDong=TuChoi | TrangThai=TuChoi | Trung bình |
| TC-045 | API thống kê 6 tháng | Admin | Có giao dịch DaThanhToan | GET /ThanhToan/ThongKe6Thang | JSON array 6 phần tử {thang, tong} | Thấp |

---

## MODULE 7: QUẢN LÝ TÀI KHOẢN (TC-046 → TC-052)

| TC ID | Tên test | Role | Điều kiện đầu | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|--------------|----------------|-------------------|-----------|
| TC-046 | Tạo tài khoản HocVien mới | Admin | Email chưa tồn tại | POST /TaiKhoan/Create, vaiTro=HocVien | JSON success=true, record NguoiDung + HocVien tạo | Cao |
| TC-047 | Tạo tài khoản email trùng | Admin | Email đã tồn tại | POST /TaiKhoan/Create, email đã có | JSON {success:false, "Email này đã tồn tại"} | Cao |
| TC-048 | Khóa tài khoản (toggle) | Admin | Có TK IsActive=true | POST /TaiKhoan/KhoaTaiKhoan/{id} | IsActive=false, JSON isActive=false | Cao |
| TC-049 | Admin không thể tự khóa mình | Admin | Đang đăng nhập | POST /TaiKhoan/KhoaTaiKhoan/{adminId} | JSON {success:false, "Không thể khóa tài khoản của chính mình"} | Cao |
| TC-050 | Reset mật khẩu | Admin | Có TK | POST /TaiKhoan/ResetMatKhau/{id} | MatKhauHash = BCrypt(DefaultPassword), toast hiện | Cao |
| TC-051 | Đổi vai trò HV → GV | Admin | HV không có DangKy | POST /TaiKhoan/SuaVaiTro, vaiTroMoi=GiangVien | VaiTro=GiangVien, tạo GiangVien record mới | Trung bình |
| TC-052 | Đổi vai trò GV đang dạy (thất bại) | Admin | GV có lớp DangHoc | POST /TaiKhoan/SuaVaiTro | Lỗi "Giảng viên đang phụ trách lớp học" | Cao |

---

## MODULE 8: PHÂN CÔNG GIẢNG VIÊN (TC-053 → TC-055)

| TC ID | Tên test | Role | Bước thực hiện | Kết quả mong đợi | Độ ưu tiên |
|-------|----------|------|----------------|-------------------|-----------|
| TC-053 | Admin phân công GV vào KH | Admin | Có GV, có KH DangMo | POST /Admin/DoPhanCong, GiangVienId hợp lệ | PhanCongGiangDay mới IsActive=true, LopHoc.GiangVienId cập nhật | Cao |
| TC-054 | Admin đổi GV đã phân công | Admin | KH đang có GV A | POST /Admin/DoPhanCong, GiangVienId=B | GV A.IsActive=false, GV B.IsActive=true | Trung bình |
| TC-055 | Admin hủy phân công | Admin | Có phân công IsActive | POST /Admin/HuyPhanCong/{id} | IsActive=false | Trung bình |

---

## BẢNG TỔNG HỢP XEP LOẠI

| Độ ưu tiên | Số TC |
|-----------|-------|
| Cao | 38 |
| Trung bình | 14 |
| Thấp | 3 |
| **Tổng** | **55** |

## MA TRẬN PHỦ SÓNG

| Module | Số TC | Tỉ lệ |
|--------|-------|--------|
| Đăng nhập | 10 | 18% |
| Khóa học | 10 | 18% |
| Lớp học | 6 | 11% |
| Đăng ký | 7 | 13% |
| Điểm số | 6 | 11% |
| Thanh toán | 6 | 11% |
| Tài khoản | 7 | 13% |
| Phân công | 3 | 5% |
| **Tổng** | **55** | **100%** |
