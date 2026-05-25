#!/usr/bin/env dotnet-script
#r "nuget: ClosedXML, 0.104.2"

using ClosedXML.Excel;
using System.Collections.Generic;

// ── Dữ liệu 55 test case ────────────────────────────────────────────────────
record TC(
    int Stt,
    string MaTC,
    string TenChucNang,
    string MoTa,
    string TienDieuKien,
    string CacBuocThucHien,
    string DuLieuDauVao,
    string KetQuaMongDoi,
    string DoUuTien
);

var data = new List<TC>
{
    // ── MODULE 1: ĐĂNG NHẬP / ĐĂNG XUẤT ──────────────────────────────────
    new(1,"TC-001","Đăng nhập / Đăng xuất",
        "Đăng nhập Admin thành công",
        "Server chạy, DB có seed account Admin",
        "1. Mở trình duyệt, vào /Account/Login\n2. Nhập Email: admin@nnl.com\n3. Nhập MatKhau: Admin@123\n4. Click nút Đăng nhập",
        "Email: admin@nnl.com\nMatKhau: Admin@123",
        "Redirect đến /Admin (dashboard), nav bar hiển thị tên \"Admin\", sidebar Admin màu đỏ",
        "Cao"),

    new(2,"TC-002","Đăng nhập / Đăng xuất",
        "Đăng nhập GiangVien thành công",
        "Server chạy, DB có seed account GiangVien",
        "1. Vào /Account/Login\n2. Nhập Email: gv01@nnl.com\n3. Nhập MatKhau: Gv@123\n4. Click Đăng nhập",
        "Email: gv01@nnl.com\nMatKhau: Gv@123",
        "Redirect đến /GiangVien/Dashboard, sidebar màu xanh lá",
        "Cao"),

    new(3,"TC-003","Đăng nhập / Đăng xuất",
        "Đăng nhập HocVien thành công",
        "Server chạy, DB có seed account HocVien",
        "1. Vào /Account/Login\n2. Nhập Email: hv01@nnl.com\n3. Nhập MatKhau: Hv@123\n4. Click Đăng nhập",
        "Email: hv01@nnl.com\nMatKhau: Hv@123",
        "Redirect đến /HocVien/Dashboard, sidebar màu tím",
        "Cao"),

    new(4,"TC-004","Đăng nhập / Đăng xuất",
        "Đăng nhập sai mật khẩu",
        "Có tài khoản admin@nnl.com hợp lệ trong DB",
        "1. Vào /Account/Login\n2. Nhập Email đúng: admin@nnl.com\n3. Nhập MatKhau sai: wrong123\n4. Click Đăng nhập",
        "Email: admin@nnl.com\nMatKhau: wrong123",
        "Ở lại /Account/Login, hiển thị thông báo lỗi \"Email hoặc mật khẩu không đúng\"",
        "Cao"),

    new(5,"TC-005","Đăng nhập / Đăng xuất",
        "Đăng nhập email không tồn tại trong DB",
        "Email không có trong cơ sở dữ liệu",
        "1. Vào /Account/Login\n2. Nhập Email: khongtontai@abc.com\n3. Nhập MatKhau bất kỳ\n4. Click Đăng nhập",
        "Email: khongtontai@abc.com\nMatKhau: abc123",
        "Hiển thị thông báo lỗi \"Email hoặc mật khẩu không đúng\", ở lại trang Login",
        "Trung bình"),

    new(6,"TC-006","Đăng nhập / Đăng xuất",
        "Đăng nhập tài khoản bị khóa (IsActive=false)",
        "Tài khoản có IsActive=false trong DB",
        "1. Vào /Account/Login\n2. Nhập đúng email và mật khẩu của tài khoản bị khóa\n3. Click Đăng nhập",
        "Email: tài khoản IsActive=false\nMatKhau: đúng",
        "Hiển thị lỗi \"Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin.\", không đăng nhập được",
        "Cao"),

    new(7,"TC-007","Đăng nhập / Đăng xuất",
        "Đăng nhập bỏ trống Email (form validation)",
        "Không có điều kiện đặc biệt",
        "1. Vào /Account/Login\n2. Bỏ trống trường Email\n3. Điền MatKhau bất kỳ\n4. Click Đăng nhập",
        "Email: (rỗng)\nMatKhau: abc123",
        "Form validation hiển thị lỗi required trên trường Email, không submit",
        "Trung bình"),

    new(8,"TC-008","Đăng nhập / Đăng xuất",
        "Ghi nhớ đăng nhập (Remember Me)",
        "Có tài khoản hợp lệ",
        "1. Vào /Account/Login\n2. Nhập đúng thông tin đăng nhập\n3. Tick chọn \"Ghi nhớ đăng nhập\"\n4. Click Đăng nhập",
        "Email: hv01@nnl.com\nMatKhau: Hv@123\nGhiNho: true",
        "Đăng nhập thành công; cookie authentication có ExpiresUtc = hiện tại + 30 ngày",
        "Thấp"),

    new(9,"TC-009","Đăng nhập / Đăng xuất",
        "Đăng xuất thành công",
        "Đang đăng nhập với bất kỳ tài khoản nào",
        "1. Đang ở trang bất kỳ sau khi đăng nhập\n2. Click nút Đăng xuất trên nav bar",
        "(Không có)",
        "Redirect đến /Account/Login, session cookie bị xóa, truy cập /Admin bị redirect về Login",
        "Cao"),

    new(10,"TC-010","Đăng nhập / Đăng xuất",
        "Đổi mật khẩu thành công",
        "Đang đăng nhập với tài khoản hợp lệ",
        "1. Vào /Account/ChangePassword\n2. Nhập MatKhauCu đúng\n3. Nhập MatKhauMoi và XacNhanMatKhauMoi khớp nhau (≥6 ký tự)\n4. Click Đổi mật khẩu",
        "MatKhauCu: Admin@123\nMatKhauMoi: NewPass@456\nXacNhan: NewPass@456",
        "Hiển thị thông báo thành công, redirect về trang Profile",
        "Trung bình"),

    // ── MODULE 2: KHÓA HỌC ───────────────────────────────────────────────
    new(11,"TC-011","Khóa học",
        "Admin xem danh sách tất cả khóa học (mọi trạng thái)",
        "DB có khóa học với trạng thái DangMo, DaDong, TamDung",
        "1. Đăng nhập Admin\n2. Vào /KhoaHoc\n3. Kiểm tra danh sách",
        "(Không có dữ liệu nhập)",
        "Hiển thị tất cả khóa học bao gồm các trạng thái DangMo, DaDong, TamDung",
        "Cao"),

    new(12,"TC-012","Khóa học",
        "HocVien chỉ thấy khóa học DangMo",
        "DB có khóa học DaDong và DangMo",
        "1. Đăng nhập HocVien\n2. Vào /KhoaHoc\n3. Kiểm tra danh sách",
        "(Không có dữ liệu nhập)",
        "Chỉ hiển thị khóa học có TrangThai = DangMo, không thấy DaDong/TamDung",
        "Cao"),

    new(13,"TC-013","Khóa học",
        "Filter danh sách khóa học theo ngôn ngữ",
        "DB có khóa học Tiếng Anh và Tiếng Nhật",
        "1. Đăng nhập Admin, vào /KhoaHoc\n2. Chọn filter NgonNgu = Tiếng Anh\n3. Click Lọc",
        "NgonNgu: Tiếng Anh",
        "Chỉ hiển thị các khóa học có NgonNgu = \"Tiếng Anh\"",
        "Trung bình"),

    new(14,"TC-014","Khóa học",
        "Admin tạo mới khóa học (dữ liệu hợp lệ)",
        "Đang đăng nhập Admin",
        "1. Vào /KhoaHoc/Create\n2. Điền TenKhoaHoc, NgonNgu, TrinhDo, HocPhi, ThoiLuong\n3. Click Tạo khóa học",
        "TenKhoaHoc: IELTS Cơ bản\nNgonNgu: Tiếng Anh\nTrinhDo: Trung cấp\nHocPhi: 3000000\nThoiLuong: 30",
        "Redirect về danh sách, khóa học mới xuất hiện trong bảng với TrangThai = DangMo",
        "Cao"),

    new(15,"TC-015","Khóa học",
        "Tạo khóa học thiếu tên (form validation)",
        "Đang đăng nhập Admin",
        "1. Vào /KhoaHoc/Create\n2. Bỏ trống TenKhoaHoc\n3. Điền các trường còn lại\n4. Click Tạo",
        "TenKhoaHoc: (rỗng)\nCác trường khác hợp lệ",
        "Ở lại trang Create, hiển thị thông báo lỗi required cho TenKhoaHoc, không tạo bản ghi",
        "Cao"),

    new(16,"TC-016","Khóa học",
        "Admin sửa thông tin khóa học",
        "DB có khóa học với Id hợp lệ",
        "1. Vào /KhoaHoc/Edit/{id}\n2. Thay đổi HocPhi\n3. Click Cập nhật",
        "HocPhi: 5000000 (giá trị mới)",
        "Redirect về danh sách, HocPhi được cập nhật, NgayCapNhat được ghi nhận",
        "Cao"),

    new(17,"TC-017","Khóa học",
        "Admin xóa khóa học chưa có lớp học",
        "Khóa học mới tạo, chưa có LopHoc nào",
        "1. Vào /KhoaHoc\n2. Click Xóa trên khóa học mục tiêu\n3. Xác nhận xóa",
        "KhoaHocId: id khóa học chưa có lớp",
        "Khóa học bị xóa khỏi DB, redirect về DS với thông báo thành công",
        "Trung bình"),

    new(18,"TC-018","Khóa học",
        "Admin xóa khóa học đã có lớp học (thất bại)",
        "Khóa học đã có ít nhất 1 LopHoc",
        "1. Vào /KhoaHoc\n2. Click Xóa trên khóa học đã có lớp\n3. Xác nhận xóa",
        "KhoaHocId: id khóa học đã có lớp",
        "Không xóa, hiển thị lỗi \"Không thể xóa khóa học đã có lớp học!\"",
        "Cao"),

    new(19,"TC-019","Khóa học",
        "Admin đổi trạng thái khóa học qua AJAX",
        "Khóa học có TrangThai = DangMo",
        "1. Vào /KhoaHoc\n2. Click nút badge trạng thái (AJAX)\n3. Kiểm tra response",
        "KhoaHocId: id KH đang DangMo",
        "API trả JSON {success: true, newStatus: \"DaDong\"}, badge trên trang cập nhật ngay",
        "Cao"),

    new(20,"TC-020","Khóa học",
        "Xem chi tiết khóa học (lịch học, số HV đăng ký)",
        "KH có ít nhất 1 lớp học và có lịch học",
        "1. Vào /KhoaHoc/Details/{id}\n2. Xem thông tin chi tiết",
        "KhoaHocId: id hợp lệ có lịch học",
        "Hiển thị TenKhoaHoc, danh sách lịch học sắp tới, số HV đã đăng ký DaDuyet",
        "Trung bình"),

    // ── MODULE 3: LỚP HỌC ────────────────────────────────────────────────
    new(21,"TC-021","Lớp học",
        "Admin xem tất cả lớp học",
        "Đăng nhập Admin, DB có lớp của nhiều GV",
        "1. Vào /LopHoc\n2. Xem danh sách",
        "(Không có dữ liệu nhập)",
        "Hiển thị toàn bộ lớp học của tất cả giảng viên",
        "Cao"),

    new(22,"TC-022","Lớp học",
        "GiangVien chỉ thấy lớp học của mình",
        "Đăng nhập GiangVien, DB có lớp của GV khác",
        "1. Đăng nhập GiangVien\n2. Vào /LopHoc\n3. Kiểm tra danh sách",
        "(Không có dữ liệu nhập)",
        "Chỉ hiển thị lớp có GiangVienId = userId hiện tại, không thấy lớp của GV khác",
        "Cao"),

    new(23,"TC-023","Lớp học",
        "Admin tạo mới lớp học (dữ liệu hợp lệ)",
        "Đăng nhập Admin, DB có KhoaHoc hợp lệ",
        "1. Vào /LopHoc/Create\n2. Chọn KhoaHoc, điền TenLop, SiSoToiDa\n3. Click Tạo lớp",
        "TenLop: Lớp A1\nKhoaHocId: id hợp lệ\nSiSoToiDa: 20\nTrangThai: DangTuyenSinh",
        "Lớp mới được tạo, xuất hiện trong danh sách với thông báo thành công",
        "Cao"),

    new(24,"TC-024","Lớp học",
        "Admin sửa thông tin lớp học",
        "DB có lớp học với Id hợp lệ",
        "1. Vào /LopHoc/Edit/{id}\n2. Thay đổi TenLop hoặc SiSoToiDa\n3. Click Cập nhật",
        "TenLop mới: Lớp A1 - Sáng",
        "Thông tin lớp được cập nhật, redirect về danh sách với thông báo thành công",
        "Trung bình"),

    new(25,"TC-025","Lớp học",
        "Admin xóa lớp học chưa có học viên đăng ký",
        "Lớp học chưa có DangKy nào",
        "1. Vào /LopHoc\n2. Click Xóa trên lớp trống\n3. Xác nhận",
        "LopHocId: id lớp chưa có HV",
        "Lớp bị xóa, redirect về DS với thông báo thành công",
        "Trung bình"),

    new(26,"TC-026","Lớp học",
        "Admin xóa lớp học đã có HV đăng ký (thất bại)",
        "Lớp học đã có ít nhất 1 DangKyKhoaHoc",
        "1. Vào /LopHoc\n2. Click Xóa trên lớp đã có HV\n3. Xác nhận",
        "LopHocId: id lớp đã có HV đăng ký",
        "Không xóa, hiển thị lỗi \"Không thể xóa lớp đã có học viên đăng ký!\"",
        "Cao"),

    // ── MODULE 4: ĐĂNG KÝ KHÓA HỌC ─────────────────────────────────────
    new(27,"TC-027","Đăng ký khóa học",
        "HocVien đăng ký lớp còn chỗ thành công",
        "Lớp có TrangThai=DangTuyenSinh, SiSoHienTai < SiSoToiDa",
        "1. Đăng nhập HocVien\n2. Vào /KhoaHoc/Details/{id}\n3. Click Đăng ký\n4. Chọn LopHoc\n5. Xác nhận",
        "LopHocId: id lớp còn chỗ",
        "Đăng ký thành công, TrangThai=ChoDuyet, thông báo \"Đăng ký thành công, chờ xét duyệt\"",
        "Cao"),

    new(28,"TC-028","Đăng ký khóa học",
        "HocVien đăng ký lớp đã đầy (thất bại)",
        "SiSoHienTai >= SiSoToiDa",
        "1. Đăng nhập HocVien\n2. POST /DangKy/DangKy với LopHocId đã đầy",
        "LopHocId: id lớp đã đầy chỗ",
        "Lỗi \"Lớp học đã đủ học viên\", không tạo bản ghi đăng ký",
        "Cao"),

    new(29,"TC-029","Đăng ký khóa học",
        "HocVien đăng ký lớp đã đăng ký trước đó (thất bại)",
        "HV đã có DangKy ChoDuyet hoặc DaDuyet cho lớp này",
        "1. Đăng nhập HocVien đã có đăng ký\n2. POST /DangKy/DangKy cùng LopHocId",
        "LopHocId: id lớp đã đăng ký",
        "Lỗi \"Bạn đã đăng ký lớp này rồi\", không tạo đơn mới",
        "Cao"),

    new(30,"TC-030","Đăng ký khóa học",
        "Admin duyệt đăng ký khóa học",
        "Có đơn đăng ký TrangThai=ChoDuyet",
        "1. Đăng nhập Admin\n2. Vào /DangKy/Index\n3. Click Duyệt trên đơn ChoDuyet",
        "DangKyId: id đơn ChoDuyet",
        "TrangThai chuyển thành DaDuyet, bản ghi Diem tự động được tạo, thông báo thành công",
        "Cao"),

    new(31,"TC-031","Đăng ký khóa học",
        "Admin từ chối đơn đăng ký",
        "Có đơn đăng ký TrangThai=ChoDuyet",
        "1. Đăng nhập Admin\n2. Vào /DangKy/Index\n3. Click Từ chối\n4. Nhập lý do: Hết chỗ\n5. Xác nhận",
        "DangKyId: id đơn\nLyDo: Hết chỗ",
        "TrangThai=TuChoi, lý do được lưu vào DB",
        "Trung bình"),

    new(32,"TC-032","Đăng ký khóa học",
        "HocVien hủy đơn đang ChoDuyet",
        "HV có đơn TrangThai=ChoDuyet",
        "1. Đăng nhập HocVien\n2. Vào /DangKy/CuaToi\n3. Click Hủy đơn",
        "DangKyId: id đơn ChoDuyet của HV",
        "TrangThai chuyển thành DaHuy, đơn biến mất khỏi danh sách đang chờ",
        "Trung bình"),

    new(33,"TC-033","Đăng ký khóa học",
        "HocVien hủy đơn đã được duyệt (thất bại)",
        "HV có đơn TrangThai=DaDuyet",
        "1. Đăng nhập HocVien\n2. Vào /DangKy/CuaToi\n3. Click Hủy trên đơn DaDuyet",
        "DangKyId: id đơn DaDuyet",
        "Lỗi \"Chỉ có thể hủy đơn đang chờ duyệt\", TrangThai không thay đổi",
        "Cao"),

    // ── MODULE 5: ĐIỂM SỐ ────────────────────────────────────────────────
    new(34,"TC-034","Điểm số",
        "GiangVien nhập điểm GK và CK cho học viên",
        "GV được phân công lớp, có HV đăng ký DaDuyet",
        "1. Đăng nhập GiangVien\n2. Vào /Diem/LopHoc?lopHocId=X\n3. Nhập DiemGiuaKy=7.5, DiemCuoiKy=8.0\n4. Click Lưu",
        "DiemGiuaKy: 7.5\nDiemCuoiKy: 8.0",
        "DiemTongKet = (7.5 + 8.0×2)/3 = 7.83, XepLoai=\"Khá\", lưu thành công",
        "Cao"),

    new(35,"TC-035","Điểm số",
        "Kiểm tra công thức tính điểm tổng kết",
        "Có bản ghi Diem với GK và CK đã nhập",
        "1. Nhập DiemGiuaKy=10, DiemCuoiKy=10\n2. Lưu điểm\n3. Kiểm tra DiemTongKet",
        "DiemGiuaKy: 10\nDiemCuoiKy: 10",
        "DiemTongKet = (10 + 10×2)/3 = 10.00, XepLoai=\"Xuất sắc\"",
        "Cao"),

    new(36,"TC-036","Điểm số",
        "GiangVien nhập điểm lớp đã khóa (thất bại)",
        "Bản ghi Diem có IsKhoa=true, đang đăng nhập GiangVien",
        "1. Đăng nhập GiangVien\n2. POST /Diem/NhapDiem với DangKyId của lớp IsKhoa=true",
        "DangKyId: id bản ghi đã khóa",
        "API trả JSON {success: false, message: \"Điểm đã bị khóa\"}, không lưu thay đổi",
        "Cao"),

    new(37,"TC-037","Điểm số",
        "Admin nhập điểm lớp đã khóa (thành công - Admin có quyền)",
        "Bản ghi Diem có IsKhoa=true, đang đăng nhập Admin",
        "1. Đăng nhập Admin\n2. POST /Diem/NhapDiem với DangKyId lớp IsKhoa=true",
        "DangKyId: id bản ghi đã khóa\nRole: Admin",
        "Lưu thành công (Admin override khóa), JSON {success: true}",
        "Trung bình"),

    new(38,"TC-038","Điểm số",
        "Admin khóa toàn bộ bảng điểm của lớp",
        "Lớp có ít nhất 1 bản ghi Diem chưa khóa",
        "1. Đăng nhập Admin\n2. POST /Diem/KhoaDiem?lopHocId=X",
        "LopHocId: id lớp cần khóa",
        "Tất cả bản ghi Diem của lớp có IsKhoa=true, thông báo số HV bị khóa",
        "Cao"),

    new(39,"TC-039","Điểm số",
        "HocVien xem điểm của mình",
        "HV có đăng ký DaDuyet và có bản ghi Diem",
        "1. Đăng nhập HocVien\n2. Vào /Diem/CuaToi",
        "(Không có dữ liệu nhập)",
        "Hiển thị danh sách điểm các khóa học DaDuyet: DiemGK, DiemCK, TongKet, XepLoai",
        "Trung bình"),

    // ── MODULE 6: THANH TOÁN ─────────────────────────────────────────────
    new(40,"TC-040","Thanh toán",
        "HocVien tạo yêu cầu thanh toán học phí",
        "HV đã có đăng ký DaDuyet, chưa có yêu cầu thanh toán",
        "1. Đăng nhập HocVien\n2. Vào /ThanhToan/TaoYeuCau?khoaHocId=X\n3. Chọn PhuongThuc=TienMat\n4. Submit",
        "KhoaHocId: id KH đã duyệt\nPhuongThuc: TienMat",
        "TrangThai=ChoPheduyet, redirect về /ThanhToan/CuaToi với thông báo thành công",
        "Cao"),

    new(41,"TC-041","Thanh toán",
        "HocVien tạo yêu cầu khi chưa có đăng ký được duyệt (thất bại)",
        "HV không có DangKy DaDuyet cho khóa học này",
        "1. Đăng nhập HocVien\n2. GET /ThanhToan/TaoYeuCau?khoaHocId=99",
        "KhoaHocId: 99 (chưa đăng ký hoặc chưa duyệt)",
        "Hiển thị lỗi \"Bạn chưa đăng ký hoặc chưa được duyệt vào khóa học này\"",
        "Cao"),

    new(42,"TC-042","Thanh toán",
        "HocVien tạo yêu cầu trùng (đã có ChoPheduyet)",
        "HV đã có yêu cầu ChoPheduyet cho cùng khóa học",
        "1. Đăng nhập HocVien đã có yêu cầu ChoPheduyet\n2. POST /ThanhToan/TaoYeuCau lần nữa",
        "KhoaHocId: id KH đã có yêu cầu pending",
        "Warning \"Đã có yêu cầu thanh toán đang chờ duyệt\", không tạo bản ghi mới",
        "Trung bình"),

    new(43,"TC-043","Thanh toán",
        "Admin duyệt yêu cầu thanh toán",
        "Có yêu cầu TrangThai=ChoPheduyet",
        "1. Đăng nhập Admin\n2. Vào /ThanhToan/Index\n3. Click Duyệt\n4. Chọn HanhDong=DaThanhToan\n5. Submit",
        "ThanhToanId: id yêu cầu ChoPheduyet\nHanhDong: DaThanhToan",
        "TrangThai=DaThanhToan, NgayDuyet và NguoiDuyetId được ghi nhận",
        "Cao"),

    new(44,"TC-044","Thanh toán",
        "Admin từ chối yêu cầu thanh toán",
        "Có yêu cầu TrangThai=ChoPheduyet",
        "1. Đăng nhập Admin\n2. Vào /ThanhToan/ChiTiet/{id}\n3. Chọn HanhDong=TuChoi\n4. Submit",
        "ThanhToanId: id yêu cầu ChoPheduyet\nHanhDong: TuChoi",
        "TrangThai=TuChoi, thông báo từ chối hiển thị",
        "Trung bình"),

    new(45,"TC-045","Thanh toán",
        "API thống kê thanh toán 6 tháng gần nhất",
        "DB có giao dịch DaThanhToan trong 6 tháng qua",
        "1. Đăng nhập Admin\n2. GET /ThanhToan/ThongKe6Thang",
        "(Không có dữ liệu nhập)",
        "Trả về JSON array 6 phần tử, mỗi phần tử gồm {thang, tong}, tổng > 0 ở tháng có giao dịch",
        "Thấp"),

    // ── MODULE 7: QUẢN LÝ TÀI KHOẢN ─────────────────────────────────────
    new(46,"TC-046","Quản lý tài khoản",
        "Admin tạo tài khoản HocVien mới",
        "Email chưa tồn tại trong DB",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/Create\n3. Điền email mới, vaiTro=HocVien",
        "Email: hvmoi@test.com\nHoTen: Học Viên Mới\nVaiTro: HocVien",
        "JSON {success:true}, tạo bản ghi NguoiDung và HocVien, mật khẩu mặc định Abc@12345",
        "Cao"),

    new(47,"TC-047","Quản lý tài khoản",
        "Admin tạo tài khoản với email đã tồn tại (thất bại)",
        "Email đã có trong DB",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/Create với email đã tồn tại",
        "Email: admin@nnl.com (đã có)\nVaiTro: HocVien",
        "JSON {success:false, message:\"Email này đã tồn tại trong hệ thống\"}",
        "Cao"),

    new(48,"TC-048","Quản lý tài khoản",
        "Admin khóa/mở khóa tài khoản (toggle)",
        "Tài khoản IsActive=true",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/KhoaTaiKhoan/{id}",
        "NguoiDungId: id tài khoản IsActive=true",
        "JSON {success:true, isActive:false}, IsActive đảo ngược trong DB",
        "Cao"),

    new(49,"TC-049","Quản lý tài khoản",
        "Admin không thể khóa tài khoản của chính mình",
        "Đang đăng nhập Admin với userId=X",
        "1. Đăng nhập Admin (userId=X)\n2. POST /TaiKhoan/KhoaTaiKhoan/{X}",
        "NguoiDungId: userId của Admin đang đăng nhập",
        "JSON {success:false, message:\"Không thể khóa tài khoản của chính mình\"}, IsActive không đổi",
        "Cao"),

    new(50,"TC-050","Quản lý tài khoản",
        "Admin reset mật khẩu về mặc định",
        "Có tài khoản bất kỳ trong DB",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/ResetMatKhau/{id}",
        "NguoiDungId: id tài khoản cần reset",
        "MatKhauHash cập nhật = BCrypt(Abc@12345), thông báo reset thành công",
        "Cao"),

    new(51,"TC-051","Quản lý tài khoản",
        "Admin đổi vai trò HocVien → GiangVien",
        "HV không có DangKy đang hoạt động",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/SuaVaiTro, vaiTroMoi=GiangVien",
        "NguoiDungId: id HV không có lớp\nVaiTroMoi: GiangVien",
        "VaiTro=GiangVien, bản ghi GiangVien mới được tạo, HocVien record bị xóa",
        "Trung bình"),

    new(52,"TC-052","Quản lý tài khoản",
        "Admin đổi vai trò GiangVien đang dạy lớp (thất bại)",
        "GiangVien đang phụ trách lớp có TrangThai=DangHoc",
        "1. Đăng nhập Admin\n2. POST /TaiKhoan/SuaVaiTro cho GV đang dạy",
        "NguoiDungId: id GV đang dạy lớp DangHoc",
        "Lỗi \"Giảng viên đang phụ trách lớp học, không thể đổi vai trò\", VaiTro không thay đổi",
        "Cao"),

    // ── MODULE 8: PHÂN CÔNG GIẢNG VIÊN ───────────────────────────────────
    new(53,"TC-053","Phân công giảng viên",
        "Admin phân công GiangVien vào KhoaHoc",
        "Có GiangVien và KhoaHoc DangMo trong DB",
        "1. Đăng nhập Admin\n2. Vào /Admin/PhanCong\n3. Chọn GV, chọn KH\n4. POST /Admin/DoPhanCong",
        "GiangVienId: id GV hợp lệ\nKhoaHocId: id KH DangMo",
        "Bản ghi PhanCongGiangDay mới IsActive=true, LopHoc.GiangVienId được cập nhật",
        "Cao"),

    new(54,"TC-054","Phân công giảng viên",
        "Admin đổi GiangVien đã phân công (GV A → GV B)",
        "KhoaHoc đang có phân công IsActive cho GV A",
        "1. Đăng nhập Admin\n2. POST /Admin/DoPhanCong với KhoaHocId cũ, GiangVienId=B",
        "KhoaHocId: id đang có GV A\nGiangVienId: id GV B (khác A)",
        "PhanCong của GV A có IsActive=false, PhanCong mới của GV B có IsActive=true",
        "Trung bình"),

    new(55,"TC-055","Phân công giảng viên",
        "Admin hủy phân công giảng viên",
        "Có bản ghi PhanCongGiangDay IsActive=true",
        "1. Đăng nhập Admin\n2. POST /Admin/HuyPhanCong/{id}",
        "PhanCongId: id bản ghi IsActive=true",
        "IsActive=false, lịch sử phân công vẫn lưu giữ trong DB",
        "Trung bình"),
};

// ── Tạo file Excel ─────────────────────────────────────────────────────────
var outputDir = @"D:\QuanLyKhoaHoc5\KiemThu\01_test_cases";
var outputPath = Path.Combine(outputDir, "TestCase_QuanLyKhoaHoc5.xlsx");
Directory.CreateDirectory(outputDir);

using var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Test Cases");

// ── Định nghĩa màu sắc ────────────────────────────────────────────────────
var colorHeader   = XLColor.FromHtml("#1F3864");  // navy xanh đậm
var colorFontHdr  = XLColor.White;
var colorRowEven  = XLColor.FromHtml("#F2F2F2");  // xám nhạt
var colorRowOdd   = XLColor.White;
var colorPass     = XLColor.FromHtml("#E2EFDA");  // xanh lá nhạt
var colorFail     = XLColor.FromHtml("#FFDCE1");  // đỏ nhạt
var colorBorder   = XLColor.FromHtml("#BDD7EE");

// ── Headers ───────────────────────────────────────────────────────────────
string[] headers = {
    "STT", "Mã TC", "Tên chức năng", "Mô tả",
    "Tiền điều kiện", "Các bước thực hiện",
    "Dữ liệu đầu vào", "Kết quả mong đợi",
    "Kết quả thực tế", "Trạng thái", "Ghi chú"
};

for (int c = 0; c < headers.Length; c++)
{
    var cell = ws.Cell(1, c + 1);
    cell.Value = headers[c];
    cell.Style.Fill.BackgroundColor = colorHeader;
    cell.Style.Font.FontColor = colorFontHdr;
    cell.Style.Font.Bold = true;
    cell.Style.Font.FontSize = 11;
    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    cell.Style.Alignment.WrapText = true;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    cell.Style.Border.OutsideBorderColor = colorBorder;
}

// ── Dữ liệu ───────────────────────────────────────────────────────────────
int row = 2;
foreach (var tc in data)
{
    var bg = (row % 2 == 0) ? colorRowEven : colorRowOdd;

    object[] values = {
        tc.Stt,
        tc.MaTC,
        tc.TenChucNang,
        tc.MoTa,
        tc.TienDieuKien,
        tc.CacBuocThucHien,
        tc.DuLieuDauVao,
        tc.KetQuaMongDoi,
        "",           // Kết quả thực tế (để trống)
        "",           // Trạng thái (để trống)
        tc.DoUuTien   // Ghi chú = Độ ưu tiên
    };

    for (int c = 0; c < values.Length; c++)
    {
        var cell = ws.Cell(row, c + 1);
        if (values[c] is int iv) cell.Value = iv;
        else cell.Value = values[c]?.ToString() ?? "";

        cell.Style.Fill.BackgroundColor = bg;
        cell.Style.Font.FontSize = 10;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        cell.Style.Alignment.WrapText = true;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.OutsideBorderColor = colorBorder;

        // STT + MaTC căn giữa
        if (c < 2)
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        else
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

        // Màu cột Ghi chú theo độ ưu tiên
        if (c == 10)
        {
            var val = values[c]?.ToString() ?? "";
            if (val == "Cao")    { cell.Style.Font.FontColor = XLColor.FromHtml("#C00000"); cell.Style.Font.Bold = true; }
            else if (val == "Trung bình") { cell.Style.Font.FontColor = XLColor.FromHtml("#9C5700"); }
            else if (val == "Thấp")       { cell.Style.Font.FontColor = XLColor.FromHtml("#375623"); }
        }
    }
    row++;
}

// ── Độ rộng cột (pixel-equivalent) ───────────────────────────────────────
ws.Column(1).Width  = 5;   // STT
ws.Column(2).Width  = 10;  // Mã TC
ws.Column(3).Width  = 20;  // Tên chức năng
ws.Column(4).Width  = 30;  // Mô tả
ws.Column(5).Width  = 28;  // Tiền điều kiện
ws.Column(6).Width  = 45;  // Các bước thực hiện
ws.Column(7).Width  = 35;  // Dữ liệu đầu vào
ws.Column(8).Width  = 45;  // Kết quả mong đợi
ws.Column(9).Width  = 30;  // Kết quả thực tế
ws.Column(10).Width = 14;  // Trạng thái
ws.Column(11).Width = 13;  // Ghi chú

// ── Row height – header ────────────────────────────────────────────────────
ws.Row(1).Height = 30;

// ── Freeze row đầu tiên ───────────────────────────────────────────────────
ws.SheetView.FreezeRows(1);

// ── AutoFilter ────────────────────────────────────────────────────────────
ws.RangeUsed()!.SetAutoFilter();

// ── Tab màu ───────────────────────────────────────────────────────────────
ws.TabColor = XLColor.FromHtml("#1F3864");

// ── Sheet tóm tắt ─────────────────────────────────────────────────────────
var wsSummary = wb.Worksheets.Add("Tổng hợp");
wsSummary.TabColor = XLColor.FromHtml("#70AD47");

// Header tổng hợp
var summaryHeaders = new[] { "Module", "Số TC", "Cao", "Trung bình", "Thấp" };
for (int c = 0; c < summaryHeaders.Length; c++)
{
    var cell = wsSummary.Cell(1, c + 1);
    cell.Value = summaryHeaders[c];
    cell.Style.Fill.BackgroundColor = colorHeader;
    cell.Style.Font.FontColor = colorFontHdr;
    cell.Style.Font.Bold = true;
    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
}

var summaryData = new (string Module, int Total, int Cao, int TrungBinh, int Thap)[]
{
    ("Đăng nhập / Đăng xuất", 10, 6, 3, 1),
    ("Khóa học", 10, 6, 4, 0),
    ("Lớp học", 6, 4, 2, 0),
    ("Đăng ký khóa học", 7, 5, 2, 0),
    ("Điểm số", 6, 4, 2, 0),
    ("Thanh toán", 6, 3, 2, 1),
    ("Quản lý tài khoản", 7, 5, 2, 0),
    ("Phân công giảng viên", 3, 1, 2, 0),
};

int sr = 2;
foreach (var s in summaryData)
{
    var bg2 = (sr % 2 == 0) ? colorRowEven : colorRowOdd;
    wsSummary.Cell(sr, 1).Value = s.Module;
    wsSummary.Cell(sr, 2).Value = s.Total;
    wsSummary.Cell(sr, 3).Value = s.Cao;
    wsSummary.Cell(sr, 4).Value = s.TrungBinh;
    wsSummary.Cell(sr, 5).Value = s.Thap;
    for (int c = 1; c <= 5; c++)
    {
        var cell = wsSummary.Cell(sr, c);
        cell.Style.Fill.BackgroundColor = bg2;
        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        if (c > 1) cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
    sr++;
}
// Tổng cộng
wsSummary.Cell(sr, 1).Value = "TỔNG CỘNG";
wsSummary.Cell(sr, 2).Value = 55;
wsSummary.Cell(sr, 3).Value = 34;
wsSummary.Cell(sr, 4).Value = 19;
wsSummary.Cell(sr, 5).Value = 2;
for (int c = 1; c <= 5; c++)
{
    var cell = wsSummary.Cell(sr, c);
    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#BDD7EE");
    cell.Style.Font.Bold = true;
    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
    if (c > 1) cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
}
wsSummary.Columns().AdjustToContents();
wsSummary.SheetView.FreezeRows(1);

// ── Lưu file ──────────────────────────────────────────────────────────────
wb.SaveAs(outputPath);
Console.WriteLine($"✅ Đã tạo: {outputPath}");
Console.WriteLine($"   - Sheet 'Test Cases': 55 test case");
Console.WriteLine($"   - Sheet 'Tổng hợp': ma trận module × độ ưu tiên");
