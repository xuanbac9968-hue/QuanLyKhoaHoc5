using ClosedXML.Excel;

// ─── 55 Test cases (stt, maTC, module, moTa, tienDK, buoc, duLieu, ketQua, uuTien) ──────────
var rows = new List<string[]>();
void Add(string stt, string ma, string mod, string mo, string dk, string buoc, string dl, string kq, string up)
    => rows.Add(new string[] { stt, ma, mod, mo, dk, buoc, dl, kq, up });

// MODULE 1: ĐĂNG NHẬP / ĐĂNG XUẤT
Add("1","TC-001","Đăng nhập / Đăng xuất","Đăng nhập Admin thành công",
    "Server chạy, DB có seed account Admin",
    "1. Mở /Account/Login\n2. Nhập Email: admin@nnl.com\n3. Nhập MatKhau: Admin@123\n4. Click Đăng nhập",
    "Email: admin@nnl.com\nMatKhau: Admin@123",
    "Redirect /Admin, nav bar hiển thị \"Admin\", sidebar màu đỏ","Cao");

Add("2","TC-002","Đăng nhập / Đăng xuất","Đăng nhập GiangVien thành công",
    "Server chạy, DB có seed account GiangVien",
    "1. Vào /Account/Login\n2. Nhập Email: gv01@nnl.com\n3. Nhập MatKhau: Gv@123\n4. Click Đăng nhập",
    "Email: gv01@nnl.com\nMatKhau: Gv@123",
    "Redirect /GiangVien/Dashboard, sidebar màu xanh lá","Cao");

Add("3","TC-003","Đăng nhập / Đăng xuất","Đăng nhập HocVien thành công",
    "Server chạy, DB có seed account HocVien",
    "1. Vào /Account/Login\n2. Nhập Email: hv01@nnl.com\n3. Nhập MatKhau: Hv@123\n4. Click Đăng nhập",
    "Email: hv01@nnl.com\nMatKhau: Hv@123",
    "Redirect /HocVien/Dashboard, sidebar màu tím","Cao");

Add("4","TC-004","Đăng nhập / Đăng xuất","Đăng nhập sai mật khẩu",
    "Có tài khoản admin@nnl.com hợp lệ trong DB",
    "1. Vào /Account/Login\n2. Nhập Email: admin@nnl.com\n3. Nhập MatKhau sai: wrong123\n4. Click Đăng nhập",
    "Email: admin@nnl.com\nMatKhau: wrong123",
    "Ở lại /Account/Login, lỗi \"Email hoặc mật khẩu không đúng\"","Cao");

Add("5","TC-005","Đăng nhập / Đăng xuất","Đăng nhập email không tồn tại",
    "Email không có trong DB",
    "1. Vào /Account/Login\n2. Nhập Email: khongtontai@abc.com\n3. Nhập MatKhau bất kỳ\n4. Click Đăng nhập",
    "Email: khongtontai@abc.com\nMatKhau: abc123",
    "Lỗi \"Email hoặc mật khẩu không đúng\", ở lại trang Login","Trung bình");

Add("6","TC-006","Đăng nhập / Đăng xuất","Đăng nhập tài khoản bị khóa (IsActive=false)",
    "Tài khoản có IsActive=false trong DB",
    "1. Vào /Account/Login\n2. Nhập đúng email và mật khẩu của TK bị khóa\n3. Click Đăng nhập",
    "Email: TK IsActive=false\nMatKhau: đúng",
    "Lỗi \"Tài khoản đã bị khóa. Vui lòng liên hệ admin.\", không đăng nhập được","Cao");

Add("7","TC-007","Đăng nhập / Đăng xuất","Đăng nhập bỏ trống Email (form validation)",
    "Không có điều kiện đặc biệt",
    "1. Vào /Account/Login\n2. Bỏ trống trường Email\n3. Điền MatKhau bất kỳ\n4. Click Đăng nhập",
    "Email: (rỗng)\nMatKhau: abc123",
    "Form validation hiển thị lỗi required trên trường Email, không submit","Trung bình");

Add("8","TC-008","Đăng nhập / Đăng xuất","Ghi nhớ đăng nhập (Remember Me)",
    "Có tài khoản hợp lệ",
    "1. Vào /Account/Login\n2. Nhập đúng email/mật khẩu\n3. Tick \"Ghi nhớ đăng nhập\"\n4. Click Đăng nhập",
    "Email: hv01@nnl.com\nMatKhau: Hv@123\nGhiNho: true",
    "Đăng nhập thành công; cookie ExpiresUtc = now + 30 ngày","Thấp");

Add("9","TC-009","Đăng nhập / Đăng xuất","Đăng xuất thành công",
    "Đang đăng nhập với bất kỳ tài khoản",
    "1. Đang ở trang bất kỳ sau đăng nhập\n2. Click nút Đăng xuất trên nav bar",
    "(Không có)",
    "Redirect /Account/Login, cookie xóa, truy cập /Admin bị redirect về Login","Cao");

Add("10","TC-010","Đăng nhập / Đăng xuất","Đổi mật khẩu thành công",
    "Đang đăng nhập với tài khoản hợp lệ",
    "1. Vào /Account/ChangePassword\n2. Nhập MatKhauCu đúng\n3. Nhập MatKhauMoi và XacNhan khớp\n4. Click Đổi",
    "MatKhauCu: Admin@123\nMatKhauMoi: NewPass@456\nXacNhan: NewPass@456",
    "Thông báo thành công, redirect về Profile","Trung bình");

// MODULE 2: KHÓA HỌC
Add("11","TC-011","Khóa học","Admin xem DS tất cả khóa học (mọi trạng thái)",
    "DB có KH DangMo, DaDong, TamDung",
    "1. Đăng nhập Admin\n2. Vào /KhoaHoc\n3. Kiểm tra danh sách",
    "(Không có)",
    "Hiển thị tất cả KH bao gồm DangMo, DaDong, TamDung","Cao");

Add("12","TC-012","Khóa học","HocVien chỉ thấy KH DangMo",
    "DB có KH DaDong và DangMo",
    "1. Đăng nhập HocVien\n2. Vào /KhoaHoc\n3. Kiểm tra danh sách",
    "(Không có)",
    "Chỉ hiển thị TrangThai=DangMo, không thấy DaDong/TamDung","Cao");

Add("13","TC-013","Khóa học","Filter danh sách khóa học theo ngôn ngữ",
    "DB có KH Tiếng Anh và Tiếng Nhật",
    "1. Đăng nhập Admin\n2. Vào /KhoaHoc\n3. Chọn filter NgonNgu=Tiếng Anh\n4. Click Lọc",
    "NgonNgu: Tiếng Anh",
    "Chỉ hiển thị các KH NgonNgu=\"Tiếng Anh\"","Trung bình");

Add("14","TC-014","Khóa học","Admin tạo mới khóa học (dữ liệu hợp lệ)",
    "Đang đăng nhập Admin",
    "1. Vào /KhoaHoc/Create\n2. Điền TenKhoaHoc, NgonNgu, TrinhDo, HocPhi, ThoiLuong\n3. Click Tạo",
    "TenKhoaHoc: IELTS Cơ bản\nNgonNgu: Tiếng Anh\nHocPhi: 3000000",
    "Redirect DS, KH mới xuất hiện TrangThai=DangMo","Cao");

Add("15","TC-015","Khóa học","Tạo KH thiếu tên (form validation)",
    "Đang đăng nhập Admin",
    "1. Vào /KhoaHoc/Create\n2. Bỏ trống TenKhoaHoc\n3. Điền các trường còn lại\n4. Click Tạo",
    "TenKhoaHoc: (rỗng)\nCác trường khác hợp lệ",
    "Ở lại trang Create, lỗi required cho TenKhoaHoc, không tạo bản ghi","Cao");

Add("16","TC-016","Khóa học","Admin sửa thông tin khóa học",
    "DB có KH với Id hợp lệ",
    "1. Vào /KhoaHoc/Edit/{id}\n2. Thay đổi HocPhi\n3. Click Cập nhật",
    "HocPhi: 5000000 (giá trị mới)",
    "Redirect DS, HocPhi cập nhật, NgayCapNhat được ghi nhận","Cao");

Add("17","TC-017","Khóa học","Admin xóa KH chưa có lớp",
    "KH mới tạo, chưa có LopHoc",
    "1. Vào /KhoaHoc\n2. Click Xóa trên KH mục tiêu\n3. Xác nhận",
    "KhoaHocId: id chưa có lớp",
    "KH bị xóa, redirect DS với thông báo thành công","Trung bình");

Add("18","TC-018","Khóa học","Admin xóa KH đã có lớp (thất bại)",
    "KH đã có ít nhất 1 LopHoc",
    "1. Vào /KhoaHoc\n2. Click Xóa trên KH đã có lớp\n3. Xác nhận",
    "KhoaHocId: id đã có lớp",
    "Không xóa, lỗi \"Không thể xóa khóa học đã có lớp học!\"","Cao");

Add("19","TC-019","Khóa học","Admin đổi trạng thái KH qua AJAX",
    "KH có TrangThai=DangMo",
    "1. Vào /KhoaHoc\n2. Click badge TrangThai (AJAX)\n3. Kiểm tra JSON response",
    "KhoaHocId: id đang DangMo",
    "JSON {success:true, newStatus:\"DaDong\"}, badge cập nhật ngay","Cao");

Add("20","TC-020","Khóa học","Xem chi tiết KH (lịch học, số HV)",
    "KH có lớp và lịch học",
    "1. Vào /KhoaHoc/Details/{id}\n2. Kiểm tra thông tin",
    "KhoaHocId: id có lịch học",
    "Hiển thị TenKhoaHoc, lịch học sắp tới, số HV đăng ký DaDuyet","Trung bình");

// MODULE 3: LỚP HỌC
Add("21","TC-021","Lớp học","Admin xem tất cả lớp học",
    "Đăng nhập Admin, DB có lớp nhiều GV",
    "1. Đăng nhập Admin\n2. Vào /LopHoc\n3. Xem DS",
    "(Không có)",
    "Hiển thị toàn bộ lớp của tất cả giảng viên","Cao");

Add("22","TC-022","Lớp học","GiangVien chỉ thấy lớp của mình",
    "Đăng nhập GiangVien, DB có lớp của GV khác",
    "1. Đăng nhập GiangVien\n2. Vào /LopHoc\n3. Kiểm tra DS",
    "(Không có)",
    "Chỉ hiển thị lớp GiangVienId=userId, không thấy lớp GV khác","Cao");

Add("23","TC-023","Lớp học","Admin tạo mới lớp học (hợp lệ)",
    "Đăng nhập Admin, DB có KhoaHoc",
    "1. Vào /LopHoc/Create\n2. Chọn KhoaHoc, điền TenLop, SiSoToiDa\n3. Click Tạo",
    "TenLop: Lớp A1\nKhoaHocId: hợp lệ\nSiSoToiDa: 20",
    "Lớp mới tạo, xuất hiện DS với thông báo thành công","Cao");

Add("24","TC-024","Lớp học","Admin sửa thông tin lớp học",
    "DB có lớp Id hợp lệ",
    "1. Vào /LopHoc/Edit/{id}\n2. Thay đổi TenLop\n3. Click Cập nhật",
    "TenLop mới: Lớp A1 - Sáng",
    "Lớp cập nhật, redirect DS với thông báo thành công","Trung bình");

Add("25","TC-025","Lớp học","Admin xóa lớp chưa có HV",
    "Lớp chưa có DangKy nào",
    "1. Vào /LopHoc\n2. Click Xóa lớp trống\n3. Xác nhận",
    "LopHocId: id chưa có HV",
    "Lớp bị xóa, redirect DS thành công","Trung bình");

Add("26","TC-026","Lớp học","Admin xóa lớp đã có HV đăng ký (thất bại)",
    "Lớp đã có ít nhất 1 DangKy",
    "1. Vào /LopHoc\n2. Click Xóa lớp có HV\n3. Xác nhận",
    "LopHocId: id đã có HV",
    "Không xóa, lỗi \"Không thể xóa lớp đã có học viên đăng ký!\"","Cao");

// MODULE 4: ĐĂNG KÝ
Add("27","TC-027","Đăng ký khóa học","HV đăng ký lớp còn chỗ (thành công)",
    "Lớp DangTuyenSinh, còn chỗ trống",
    "1. Đăng nhập HocVien\n2. Vào /KhoaHoc/Details/{id}\n3. Click Đăng ký\n4. Chọn lớp\n5. Xác nhận",
    "LopHocId: id lớp còn chỗ",
    "TrangThai=ChoDuyet, thông báo \"Đăng ký thành công, chờ xét duyệt\"","Cao");

Add("28","TC-028","Đăng ký khóa học","HV đăng ký lớp đã đầy (thất bại)",
    "SiSoHienTai >= SiSoToiDa",
    "1. Đăng nhập HocVien\n2. POST /DangKy/DangKy với LopHocId đã đầy",
    "LopHocId: id lớp đầy chỗ",
    "Lỗi \"Lớp học đã đủ học viên\", không tạo bản ghi","Cao");

Add("29","TC-029","Đăng ký khóa học","HV đăng ký lớp đã đăng ký rồi (thất bại)",
    "HV đã có DangKy ChoDuyet/DaDuyet",
    "1. Đăng nhập HocVien đã có đăng ký\n2. POST /DangKy/DangKy cùng LopHocId",
    "LopHocId: id đã đăng ký",
    "Lỗi \"Bạn đã đăng ký lớp này rồi\", không tạo đơn mới","Cao");

Add("30","TC-030","Đăng ký khóa học","Admin duyệt đăng ký khóa học",
    "Có đơn TrangThai=ChoDuyet",
    "1. Đăng nhập Admin\n2. Vào /DangKy/Index\n3. Click Duyệt",
    "DangKyId: id đơn ChoDuyet",
    "TrangThai=DaDuyet, bản ghi Diem tự động tạo, thông báo thành công","Cao");

Add("31","TC-031","Đăng ký khóa học","Admin từ chối đăng ký",
    "Có đơn TrangThai=ChoDuyet",
    "1. Đăng nhập Admin\n2. Vào /DangKy/Index\n3. Click Từ chối\n4. Nhập lý do: Hết chỗ",
    "DangKyId: id đơn\nLyDo: Hết chỗ",
    "TrangThai=TuChoi, lý do được lưu vào DB","Trung bình");

Add("32","TC-032","Đăng ký khóa học","HV hủy đơn ChoDuyet",
    "HV có đơn TrangThai=ChoDuyet",
    "1. Đăng nhập HocVien\n2. Vào /DangKy/CuaToi\n3. Click Hủy đơn",
    "DangKyId: id đơn ChoDuyet",
    "TrangThai=DaHuy, đơn biến mất khỏi DS chờ","Trung bình");

Add("33","TC-033","Đăng ký khóa học","HV hủy đơn DaDuyet (thất bại)",
    "HV có đơn TrangThai=DaDuyet",
    "1. Đăng nhập HocVien\n2. Vào /DangKy/CuaToi\n3. Click Hủy đơn DaDuyet",
    "DangKyId: id đơn DaDuyet",
    "Lỗi \"Chỉ có thể hủy đơn đang chờ duyệt\", TrangThai không đổi","Cao");

// MODULE 5: ĐIỂM SỐ
Add("34","TC-034","Điểm số","GV nhập điểm GK và CK",
    "GV phân công lớp, HV đăng ký DaDuyet",
    "1. Đăng nhập GiangVien\n2. Vào /Diem/LopHoc?lopHocId=X\n3. Nhập GK=7.5, CK=8.0\n4. Lưu",
    "DiemGiuaKy: 7.5\nDiemCuoiKy: 8.0",
    "TongKet=(7.5+8×2)/3=7.83, XepLoai=Khá, lưu thành công","Cao");

Add("35","TC-035","Điểm số","Kiểm tra công thức điểm tổng kết",
    "Có bản ghi Diem",
    "1. Nhập DiemGiuaKy=10\n2. Nhập DiemCuoiKy=10\n3. Lưu\n4. Kiểm tra DB",
    "DiemGiuaKy: 10\nDiemCuoiKy: 10",
    "DiemTongKet=(10+10×2)/3=10.00, XepLoai=Xuất sắc","Cao");

Add("36","TC-036","Điểm số","GV nhập điểm lớp đã khóa (thất bại)",
    "Diem.IsKhoa=true, đăng nhập GiangVien",
    "1. Đăng nhập GiangVien\n2. POST /Diem/NhapDiem với DangKyId IsKhoa=true",
    "DangKyId: bản ghi IsKhoa=true\nRole: GiangVien",
    "JSON {success:false, message:\"Điểm đã bị khóa\"}, không lưu","Cao");

Add("37","TC-037","Điểm số","Admin nhập điểm lớp đã khóa (thành công)",
    "Diem.IsKhoa=true, đăng nhập Admin",
    "1. Đăng nhập Admin\n2. POST /Diem/NhapDiem với DangKyId IsKhoa=true",
    "DangKyId: bản ghi IsKhoa=true\nRole: Admin",
    "Lưu thành công (Admin override), JSON {success:true}","Trung bình");

Add("38","TC-038","Điểm số","Admin khóa toàn bộ bảng điểm lớp",
    "Lớp có Diem chưa khóa",
    "1. Đăng nhập Admin\n2. POST /Diem/KhoaDiem?lopHocId=X",
    "LopHocId: id lớp cần khóa",
    "Tất cả Diem.IsKhoa=true, thông báo số HV bị khóa","Cao");

Add("39","TC-039","Điểm số","HV xem điểm của mình",
    "HV có đăng ký DaDuyet và bản ghi Diem",
    "1. Đăng nhập HocVien\n2. Vào /Diem/CuaToi",
    "(Không có)",
    "Hiển thị DS điểm: DiemGK, DiemCK, TongKet, XepLoai","Trung bình");

// MODULE 6: THANH TOÁN
Add("40","TC-040","Thanh toán","HV tạo yêu cầu thanh toán học phí",
    "HV đã DaDuyet, chưa có yêu cầu TT",
    "1. Đăng nhập HocVien\n2. Vào /ThanhToan/TaoYeuCau?khoaHocId=X\n3. Chọn TienMat\n4. Submit",
    "KhoaHocId: KH đã duyệt\nPhuongThuc: TienMat",
    "TrangThai=ChoPheduyet, redirect /ThanhToan/CuaToi thành công","Cao");

Add("41","TC-041","Thanh toán","HV tạo yêu cầu khi chưa DaDuyet (thất bại)",
    "HV không có DangKy DaDuyet",
    "1. Đăng nhập HocVien\n2. GET /ThanhToan/TaoYeuCau?khoaHocId=99",
    "KhoaHocId: 99 (chưa đăng ký)",
    "Lỗi \"Bạn chưa đăng ký hoặc chưa được duyệt vào khóa học này\"","Cao");

Add("42","TC-042","Thanh toán","HV tạo yêu cầu trùng (đang ChoPheduyet)",
    "HV đã có yêu cầu ChoPheduyet cùng KH",
    "1. Đăng nhập HocVien đã có yêu cầu\n2. POST /ThanhToan/TaoYeuCau lần nữa",
    "KhoaHocId: id đã có pending",
    "Warning \"Đã có yêu cầu đang chờ duyệt\", không tạo bản ghi mới","Trung bình");

Add("43","TC-043","Thanh toán","Admin duyệt yêu cầu thanh toán",
    "Có yêu cầu ChoPheduyet",
    "1. Đăng nhập Admin\n2. Vào /ThanhToan/Index\n3. Click Duyệt\n4. HanhDong=DaThanhToan",
    "ThanhToanId: id ChoPheduyet\nHanhDong: DaThanhToan",
    "TrangThai=DaThanhToan, NgayDuyet + NguoiDuyetId ghi nhận","Cao");

Add("44","TC-044","Thanh toán","Admin từ chối thanh toán",
    "Có yêu cầu ChoPheduyet",
    "1. Đăng nhập Admin\n2. Vào /ThanhToan/ChiTiet/{id}\n3. HanhDong=TuChoi\n4. Submit",
    "ThanhToanId: id ChoPheduyet\nHanhDong: TuChoi",
    "TrangThai=TuChoi, thông báo từ chối","Trung bình");

Add("45","TC-045","Thanh toán","API thống kê thanh toán 6 tháng",
    "DB có giao dịch DaThanhToan 6 tháng qua",
    "1. Đăng nhập Admin\n2. GET /ThanhToan/ThongKe6Thang",
    "(Không có)",
    "JSON array 6 phần tử [{thang, tong}], tổng > 0 tháng có giao dịch","Thấp");

// MODULE 7: QUẢN LÝ TÀI KHOẢN
Add("46","TC-046","Quản lý tài khoản","Admin tạo TK HocVien mới",
    "Email chưa tồn tại trong DB",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/Create, vaiTro=HocVien",
    "Email: hvmoi@test.com\nHoTen: HV Mới\nVaiTro: HocVien",
    "JSON {success:true}, tạo NguoiDung + HocVien, pass mặc định Abc@12345","Cao");

Add("47","TC-047","Quản lý tài khoản","Admin tạo TK email đã tồn tại (thất bại)",
    "Email đã có trong DB",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/Create với email đã tồn tại",
    "Email: admin@nnl.com (đã có)\nVaiTro: HocVien",
    "JSON {success:false, message:\"Email này đã tồn tại trong hệ thống\"}","Cao");

Add("48","TC-048","Quản lý tài khoản","Admin khóa/mở TK (toggle IsActive)",
    "TK IsActive=true",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/KhoaTaiKhoan/{id}",
    "NguoiDungId: id TK IsActive=true",
    "JSON {success:true, isActive:false}, IsActive đảo ngược trong DB","Cao");

Add("49","TC-049","Quản lý tài khoản","Admin không khóa được chính mình",
    "Đang đăng nhập Admin userId=X",
    "1. Đăng nhập Admin (userId=X)\n2. POST /TaiKhoan/KhoaTaiKhoan/{X}",
    "NguoiDungId: userId Admin đang đăng nhập",
    "JSON {success:false, \"Không thể khóa tài khoản của chính mình\"}, IsActive không đổi","Cao");

Add("50","TC-050","Quản lý tài khoản","Admin reset mật khẩu về mặc định",
    "Có tài khoản bất kỳ trong DB",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/ResetMatKhau/{id}",
    "NguoiDungId: id cần reset",
    "MatKhauHash = BCrypt(Abc@12345), thông báo thành công","Cao");

Add("51","TC-051","Quản lý tài khoản","Admin đổi vai trò HV → GV",
    "HV không có DangKy hoạt động",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/SuaVaiTro, vaiTroMoi=GiangVien",
    "NguoiDungId: id HV\nVaiTroMoi: GiangVien",
    "VaiTro=GiangVien, GiangVien record tạo, HocVien record xóa","Trung bình");

Add("52","TC-052","Quản lý tài khoản","Admin đổi vai trò GV đang dạy (thất bại)",
    "GV đang phụ trách lớp DangHoc",
    "1. Đăng nhập Admin\n2. POST /TaiKhoan/SuaVaiTro cho GV đang dạy",
    "NguoiDungId: id GV dạy DangHoc",
    "Lỗi \"Giảng viên đang phụ trách lớp học, không thể đổi vai trò\"","Cao");

// MODULE 8: PHÂN CÔNG
Add("53","TC-053","Phân công giảng viên","Admin phân công GV vào KH",
    "Có GV và KH DangMo trong DB",
    "1. Đăng nhập Admin\n2. Vào /Admin/PhanCong\n3. Chọn GV + KH\n4. POST /Admin/DoPhanCong",
    "GiangVienId: id GV\nKhoaHocId: id KH DangMo",
    "PhanCongGiangDay mới IsActive=true, LopHoc.GiangVienId cập nhật","Cao");

Add("54","TC-054","Phân công giảng viên","Admin đổi GV phân công (A → B)",
    "KH đang có PhanCong IsActive cho GV A",
    "1. Đăng nhập Admin\n2. POST /Admin/DoPhanCong cùng KH, GiangVienId=B",
    "KhoaHocId: id đang GV A\nGiangVienId: id GV B",
    "PhanCong GV A IsActive=false, PhanCong GV B IsActive=true","Trung bình");

Add("55","TC-055","Phân công giảng viên","Admin hủy phân công GV",
    "Có PhanCongGiangDay IsActive=true",
    "1. Đăng nhập Admin\n2. POST /Admin/HuyPhanCong/{id}",
    "PhanCongId: id IsActive=true",
    "IsActive=false, lịch sử phân công vẫn lưu trong DB","Trung bình");

// ─── Tạo file Excel ─────────────────────────────────────────────────────────
const string OUT = @"D:\QuanLyKhoaHoc5\KiemThu\01_test_cases\TestCase_QuanLyKhoaHoc5.xlsx";
Directory.CreateDirectory(Path.GetDirectoryName(OUT)!);

using var wb = new XLWorkbook();

// Màu sắc
var cHeader = XLColor.FromHtml("#1F3864");
var cFontH  = XLColor.White;
var cEven   = XLColor.FromHtml("#EEF3FA");
var cOdd    = XLColor.White;
var cBorder = XLColor.FromHtml("#BDD7EE");

// ══════════════ Sheet 1: Test Cases ════════════════════════════════════════
var ws = wb.Worksheets.Add("Test Cases");
ws.TabColor = XLColor.FromHtml("#1F3864");

string[] hdrs = { "STT","Mã TC","Tên chức năng","Mô tả","Tiền điều kiện",
                  "Các bước thực hiện","Dữ liệu đầu vào","Kết quả mong đợi",
                  "Kết quả thực tế","Trạng thái","Ghi chú" };

// Header
for (int c = 0; c < hdrs.Length; c++)
{
    var cell = ws.Cell(1, c + 1);
    cell.Value = hdrs[c];
    cell.Style.Fill.BackgroundColor = cHeader;
    cell.Style.Font.FontColor       = cFontH;
    cell.Style.Font.Bold            = true;
    cell.Style.Font.FontSize        = 11;
    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
    cell.Style.Alignment.WrapText   = true;
    cell.Style.Border.OutsideBorder      = XLBorderStyleValues.Medium;
    cell.Style.Border.OutsideBorderColor = cBorder;
}
ws.Row(1).Height = 32;

// Data rows
for (int i = 0; i < rows.Count; i++)
{
    int r   = i + 2;
    var row = rows[i];
    var bg  = (r % 2 == 0) ? cEven : cOdd;

    // Col 1: STT (int)
    ws.Cell(r, 1).Value = int.Parse(row[0]);
    // Col 2-8 từ mảng row[1..7]
    for (int c = 1; c <= 7; c++) ws.Cell(r, c + 1).Value = row[c];
    // Col 9,10: để trống
    ws.Cell(r,  9).Value = "";
    ws.Cell(r, 10).Value = "";
    // Col 11: Ghi chú = độ ưu tiên (row[8])
    ws.Cell(r, 11).Value = row[8];

    // Style cho tất cả cột
    for (int c = 1; c <= 11; c++)
    {
        var cell = ws.Cell(r, c);
        cell.Style.Fill.BackgroundColor = bg;
        cell.Style.Font.FontSize        = 10;
        cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Top;
        cell.Style.Alignment.WrapText   = true;
        cell.Style.Border.OutsideBorder      = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = cBorder;
        cell.Style.Alignment.Horizontal = (c <= 2)
            ? XLAlignmentHorizontalValues.Center
            : XLAlignmentHorizontalValues.Left;
    }

    // Màu cột Ghi chú (độ ưu tiên)
    var nc = ws.Cell(r, 11);
    nc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    if (row[8] == "Cao")
    {
        nc.Style.Font.FontColor = XLColor.FromHtml("#C00000");
        nc.Style.Font.Bold = true;
    }
    else if (row[8] == "Trung bình")
        nc.Style.Font.FontColor = XLColor.FromHtml("#9C5700");
    else
        nc.Style.Font.FontColor = XLColor.FromHtml("#375623");
}

// Độ rộng cột
int[] widths = { 5, 10, 22, 32, 30, 48, 36, 48, 28, 14, 13 };
for (int c = 0; c < widths.Length; c++) ws.Column(c + 1).Width = widths[c];

ws.SheetView.FreezeRows(1);
ws.Range(1, 1, rows.Count + 1, 11).SetAutoFilter();

// ══════════════ Sheet 2: Tổng hợp ══════════════════════════════════════════
var ws2 = wb.Worksheets.Add("Tổng hợp");
ws2.TabColor = XLColor.FromHtml("#70AD47");

// Tiêu đề
ws2.Cell(1, 1).Value = "BẢNG TỔNG HỢP TEST CASE – QuanLyKhoaHoc5";
ws2.Cell(1, 1).Style.Font.Bold = true;
ws2.Cell(1, 1).Style.Font.FontSize = 14;
ws2.Cell(1, 1).Style.Font.FontColor = cHeader;
ws2.Range(1, 1, 1, 6).Merge();
ws2.Row(1).Height = 26;

ws2.Cell(2, 1).Value = "Ngày tạo: 25/05/2026  |  Phiên bản: 1.0  |  Tổng: 55 test case";
ws2.Cell(2, 1).Style.Font.Italic   = true;
ws2.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
ws2.Range(2, 1, 2, 6).Merge();

// Header bảng tổng hợp
string[] h2 = { "Module","Tổng TC","Cao","Trung bình","Thấp","Tỉ lệ" };
for (int c = 0; c < h2.Length; c++)
{
    var cell = ws2.Cell(4, c + 1);
    cell.Value = h2[c];
    cell.Style.Fill.BackgroundColor = cHeader;
    cell.Style.Font.FontColor       = cFontH;
    cell.Style.Font.Bold            = true;
    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
}

// Dữ liệu tổng hợp
string[,] sData =
{
    { "Đăng nhập / Đăng xuất",  "10","6","3","1","18%" },
    { "Khóa học",               "10","6","4","0","18%" },
    { "Lớp học",                 "6","4","2","0","11%" },
    { "Đăng ký khóa học",        "7","5","2","0","13%" },
    { "Điểm số",                 "6","4","2","0","11%" },
    { "Thanh toán",              "6","3","2","1","11%" },
    { "Quản lý tài khoản",       "7","5","2","0","13%" },
    { "Phân công giảng viên",    "3","1","2","0", "5%" },
};

for (int i = 0; i < sData.GetLength(0); i++)
{
    int r   = i + 5;
    var bg2 = (r % 2 == 0) ? cEven : cOdd;
    for (int c = 0; c < 6; c++)
    {
        var cell = ws2.Cell(r, c + 1);
        cell.Value = sData[i, c];
        cell.Style.Fill.BackgroundColor = bg2;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = cBorder;
        cell.Style.Alignment.Horizontal = (c == 0)
            ? XLAlignmentHorizontalValues.Left
            : XLAlignmentHorizontalValues.Center;
    }
}

// Dòng tổng
int tr = sData.GetLength(0) + 5;
string[] tots = { "TỔNG CỘNG","55","34","19","2","100%" };
for (int c = 0; c < tots.Length; c++)
{
    var cell = ws2.Cell(tr, c + 1);
    cell.Value = tots[c];
    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
    cell.Style.Font.Bold            = true;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    cell.Style.Alignment.Horizontal = (c == 0)
        ? XLAlignmentHorizontalValues.Left
        : XLAlignmentHorizontalValues.Center;
}

int[] w2 = { 28, 10, 10, 14, 10, 10 };
for (int c = 0; c < w2.Length; c++) ws2.Column(c + 1).Width = w2[c];
ws2.SheetView.FreezeRows(1);

// Lưu
wb.SaveAs(OUT);
Console.WriteLine($"OK  {OUT}");
Console.WriteLine($"    Sheet 'Test Cases': {rows.Count} test case (11 cột)");
Console.WriteLine($"    Sheet 'Tổng hợp':  8 module × 3 mức ưu tiên");
