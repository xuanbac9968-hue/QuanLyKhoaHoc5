# PROMPT CHO CLAUDE CODE – REVIEW VÀ SỬA LẠI PROJECT
# Đề tài: Quản lý khóa học trung tâm ngoại ngữ tích hợp gợi ý khóa học thông minh
# ASP.NET Core 8 MVC – EF Core – SQL Server – Bootstrap 5

Hãy đọc TOÀN BỘ codebase project hiện tại, đặc biệt: AppDbContext.cs, SeedData.cs, Program.cs, tất cả Controllers/, Models/Entities/, Models/ViewModels/, Views/Shared/_Layout*.cshtml.
Sau đó kiểm tra từng nhóm bên dưới, BỔ SUNG hoặc SỬA những gì còn thiếu/sai. KHÔNG chạm vào bất kỳ thứ gì liên quan đến AI, Chatbot, Groq, Gemini, GoiY.
Sau mỗi nhóm chạy `dotnet build` đạt 0 Error 0 Warning mới tiếp tục nhóm tiếp theo.

---

## NHÓM 1 – QUẢN LÝ KHÓA HỌC VÀ LỊCH HỌC (1,5đ)

### 1.1 – KhoaHocController.cs – kiểm tra và bổ sung:

**GET /KhoaHoc/Index**:
- Phân trang 10 bản ghi/trang, tham số `page` trên URL
- Tìm kiếm theo tên (tham số `search`)
- Lọc theo NgonNgu: Tất cả / Tiếng Anh / Tiếng Nhật
- Lọc theo TrangThai: Tất cả / DangMo / DaDong / TamDung
- Lọc theo TrinhDo
- Bảng hiển thị: Ảnh bìa | Tên khóa học | Ngôn ngữ | Trình độ | Học phí (format N0 VNĐ) | Số buổi | Trạng thái (badge màu) | Hành động (Chi tiết / Sửa / Xóa)

**GET /KhoaHoc/Details/{id}**:
- Thông tin đầy đủ khóa học (ảnh bìa, mô tả, nội dung chương trình, học phí, thời lượng)
- Danh sách các lớp học đang mở của khóa này: tên lớp, ngày khai giảng, sĩ số còn lại (SiSoToiDa - SiSoHienTai), tên GV
- Nút "Đăng ký ngay" cho HocVien (link tới trang chọn lớp đăng ký)

**GET/POST /KhoaHoc/Create** (Admin):
Form đầy đủ với validation:
- TenKhoaHoc (required, max 200 ký tự)
- MoTa (textarea)
- NgonNgu (select: Tiếng Anh / Tiếng Nhật, required)
- TrinhDo (select: Sơ cấp / Trung cấp / Cao cấp / IELTS / TOEIC / N5 / N4 / N3 / N2 / N1, required)
- HocPhi (number min 0, required, hiển thị format tiền)
- ThoiLuong (số buổi, required)
- SoBuoiMoiTuan (số buổi/tuần)
- ThoiGianMoiBuoi (phút/buổi)
- NoiDungChuongTrinh (textarea dài)
- AnhBia (file upload, chỉ nhận jpg/png, max 5MB, lưu vào wwwroot/uploads/courses/)
- TrangThai (select: DangMo / DaDong / TamDung, mặc định DangMo)

**GET/POST /KhoaHoc/Edit/{id}** (Admin):
- Giống Create
- Nếu không upload ảnh mới thì giữ nguyên ảnh cũ
- Hiển thị ảnh hiện tại để tham khảo

**POST /KhoaHoc/Delete/{id}** (Admin):
- Kiểm tra còn LopHoc nào không → nếu có: trả về lỗi "Không thể xóa vì đã có lớp học thuộc khóa này"
- Nếu không có: xóa và redirect về Index với thông báo thành công

**POST /KhoaHoc/ChangeStatus/{id}** (Admin):
- Đổi TrangThai (vòng tròn: DangMo → DaDong → TamDung → DangMo)
- Trả JSON: `{ success: true, newStatus: "DaDong", displayText: "Đã đóng" }`

### 1.2 – LichHocController.cs – kiểm tra và bổ sung:

**GET /LichHoc/GetEvents** (AJAX endpoint, tất cả role):
```
Tham số: month (int), year (int), khoaHocId (int, default 0), giangVienId (int, default 0)
```
Logic lọc theo role:
- Admin: lấy tất cả LichHoc trong tháng/năm, JOIN LopHoc → KhoaHoc → GiangVien. Cho phép lọc thêm theo khoaHocId và giangVienId nếu > 0
- GiangVien: WHERE LopHoc.GiangVienId == Id của GiangVien đang đăng nhập
- HocVien: JOIN DangKyKhoaHoc WHERE HocVienId == Id HocVien đang đăng nhập AND TrangThai == "DaDuyet"

Trả về JSON array:
```json
[{
  "id": 1,
  "lopHocId": 2,
  "tenKhoaHoc": "Tiếng Anh IELTS",
  "khoaHocId": 3,
  "ngayHoc": "2026-05-04",
  "gioBatDau": "18:30",
  "gioKetThuc": "20:30",
  "phongHoc": "P.101",
  "tenGiangVien": "Nguyễn Thị Minh",
  "siSoHienTai": 15,
  "siSoToiDa": 20,
  "trangThaiLop": "DangHoc"
}]
```

**POST /LichHoc/Create** (Admin):
- Tham số: LopHocId, NgayHoc, GioBatDau, GioKetThuc, PhongHoc, ChuDe, GhiChu
- Kiểm tra trùng lịch: cùng PhongHoc + cùng ngày + giờ giao nhau → báo lỗi "Phòng {PhongHoc} đã có lớp khác vào giờ này"
- Kiểm tra trùng GV: cùng GiangVienId + cùng ngày + giờ giao nhau → báo lỗi "Giảng viên đã có lịch dạy vào giờ này"

**POST /LichHoc/Edit/{id}** (Admin): Sửa thông tin buổi học, validate trùng lịch tương tự Create

**POST /LichHoc/Delete/{id}** (Admin): Xóa buổi học

**POST /LichHoc/TaoHangLoat** (Admin):
- Form: LopHocId, NgayBatDau, NgayKetThuc, các thứ trong tuần (checkbox T2-CN), GioBatDau, GioKetThuc, PhongHoc
- Tự tạo tất cả buổi học trong khoảng thời gian theo các thứ đã chọn
- Bỏ qua các ngày đã có lịch trùng, báo cáo số buổi đã tạo và số buổi bị bỏ qua

### 1.3 – Giao diện lịch học – XÓA TOÀN BỘ code View cũ, viết lại từ đầu:

**CSS màu event** (gán tự động theo khoaHocId % 8):
```javascript
const COLOR_MAP = [
  { bg: '#dbeafe', border: '#3b82f6', text: '#1e40af' },
  { bg: '#fef3c7', border: '#f59e0b', text: '#92400e' },
  { bg: '#ede9fe', border: '#8b5cf6', text: '#4c1d95' },
  { bg: '#dcfce7', border: '#22c55e', text: '#14532d' },
  { bg: '#fce7f3', border: '#ec4899', text: '#831843' },
  { bg: '#ffedd5', border: '#f97316', text: '#7c2d12' },
  { bg: '#cffafe', border: '#06b6d4', text: '#164e63' },
  { bg: '#fee2e2', border: '#ef4444', text: '#7f1d1d' }
];
function getColor(khoaHocId) { return COLOR_MAP[khoaHocId % 8]; }
```

**Toolbar** (1 hàng, flex, space-between):
- Nhóm trái: nút "← Trước" | text "Tháng M/YYYY" (fw-semibold) | nút "Sau →" | nút "Hôm nay" (btn-primary)
- Nhóm giữa (chỉ Admin): select lọc khóa học + select lọc giảng viên
- Nhóm phải: btn-group [Tháng][Tuần][Danh sách]

**Chế độ THÁNG:**
- CSS Grid 7 cột, header row: Thứ Hai → Chủ Nhật (Thứ Bảy, Chủ Nhật class text-danger)
- Mỗi ô day-cell: min-height 120px, padding 4px, border 0.5px solid #e5e7eb
- Ngày hôm nay: số ngày trong `<span>` tròn (background #3b82f6, color white, border-radius 50%, width height 24px, inline-flex center)
- Ô hôm nay: background #eff6ff
- Ngày tháng khác: opacity 0.45, background #f9fafb
- Event card: `background:{bg}; border-left:3px solid {border}; color:{text}; padding:2px 6px; border-radius:4px; margin-bottom:2px; font-size:11px; font-weight:500; cursor:pointer; white-space:nowrap; overflow:hidden; text-overflow:ellipsis`
  - Dòng 1: giờ bắt đầu + tên khóa (truncate nếu dài)
  - Dòng 2 (font-size 10px, opacity 0.8): phòng học + họ tắt GV (VD: "P.101 · N.Minh")
  - hover: opacity 0.8
- Nếu ngày có >3 event: hiển thị 3 cái đầu + `<div class="text-muted" style="font-size:10px;cursor:pointer">+N buổi nữa</div>` (click: show popover danh sách đầy đủ)
- Click event → mở Bootstrap Modal chi tiết

**Modal chi tiết buổi học** (Bootstrap modal size md):
```
Header: tên khóa học + badge trạng thái lớp
Body (bảng 2 cột):
  📅 Ngày học    | Thứ X, dd/MM/yyyy
  ⏰ Giờ học     | HH:mm – HH:mm
  🏫 Phòng học   | {phongHoc}
  👨‍🏫 Giảng viên | {tenGiangVien}
  👥 Sĩ số       | {siSoHienTai}/{siSoToiDa} (chỉ Admin và GiangVien thấy)
Footer: nút Đóng
```

**Chế độ TUẦN:**
- Bảng: cột đầu = khung giờ 07:00–21:00 (mỗi hàng 60px = 1 tiếng)
- 7 cột tiếp theo = Thứ Hai → Chủ Nhật
- Header cột: "T2\n26/5" – ngày hôm nay in đậm + background #eff6ff
- Event: position absolute trong ô ngày, top = (phút từ 07:00)/60*60px, height = durationPhút/60*60px, màu theo COLOR_MAP, border-radius 4px, padding 2px 4px, font-size 11px, overflow hidden

**Chế độ DANH SÁCH:**
- Group theo ngày, mỗi ngày là 1 block
- Header ngày: "Thứ Hai, 26/05/2026" – ngày hôm nay dùng text-primary fw-bold
- Mỗi buổi học: 1 row flex gồm badge giờ (màu theo khóa) + tên khóa + phòng + GV + badge trạng thái lớp
- Chỉ hiện 30 ngày tới từ hôm nay, ẩn ngày không có buổi nào
- Mobile: mặc định dùng chế độ này, ẩn nút [Tháng] và [Tuần]

**Legend màu** cố định dưới calendar: flex-wrap, mỗi item = ô màu 12×12px (border-left 3px solid màu đậm, bg màu nhạt) + tên khóa học. Chỉ hiện khóa có lịch trong tháng đang xem.

**AJAX**: load data khi đổi tháng/tuần/filter. Spinner loading khi đang fetch. Không reload trang.

**3 role khác nhau**:
- Admin (/LichHoc/Index): tiêu đề "Lịch học toàn hệ thống", hiển thị filter
- GiangVien (/LichHoc/CuaToi): tiêu đề "Lịch dạy của tôi", ẩn filter
- HocVien (/LichHoc/CuaToi): tiêu đề "Lịch học của tôi", ẩn filter

---

## NHÓM 2 – QUẢN LÝ HỌC VIÊN VÀ ĐĂNG KÝ KHÓA HỌC (1,5đ)

### 2.1 – HocVienController.cs (Admin) – kiểm tra và bổ sung:

**GET /HocVien/Index**:
- Phân trang 10/trang
- Tìm kiếm theo tên / mã HV / email (tham số `search`)
- Bảng: STT | Mã HV | Họ tên | Email | SĐT | Trình độ | Ngày đăng ký | Trạng thái (badge Hoạt động/Bị khóa) | Hành động

**GET /HocVien/Details/{id}**:
- Thông tin cá nhân: ảnh đại diện, họ tên, email, SĐT, ngày sinh, giới tính, địa chỉ, trình độ, ngôn ngữ quan tâm
- Lịch sử đăng ký: bảng gồm tên lớp, tên khóa, ngày đăng ký, trạng thái, điểm tổng kết (nếu có)

**GET/POST /HocVien/Create** (Admin):
- Tạo NguoiDung mới (VaiTro=HocVien) + bản ghi HocVien
- Tự sinh MaHocVien theo format HV + 3 số (VD: HV001, HV002...)
- Mật khẩu mặc định lấy từ appsettings["AppSettings:DefaultPassword"], hash BCrypt

**GET/POST /HocVien/Edit/{id}** (Admin): Sửa thông tin HocVien và NguoiDung tương ứng

**POST /HocVien/Delete/{id}** (Admin): Soft delete – set NguoiDung.IsActive = false (KHÔNG xóa DB)

**GET /HocVien/ExportExcel** (Admin):
- ClosedXML tạo file .xlsx
- Cột: STT | Mã HV | Họ tên | Email | Số điện thoại | Ngày sinh | Giới tính | Trình độ | Ngày đăng ký
- Header row: background màu #4472C4, chữ trắng, bold
- Auto-fit tất cả cột
- Tên file: `DanhSachHocVien_{yyyyMMdd}.xlsx`

### 2.2 – DangKyController.cs – kiểm tra và bổ sung:

**GET /DangKy/Index** (Admin):
- Danh sách tất cả đơn, phân trang
- Lọc theo: TrangThai (select) / KhoaHoc (select) / LopHoc (select) / khoảng NgayDangKy
- Bảng: STT | Học viên | Lớp học | Khóa học | Ngày đăng ký | Trạng thái | Hành động

**GET /DangKy/CuaToi** (HocVien):
- Danh sách đơn của HocVien đang đăng nhập
- Badge màu: ChoDuyet=bg-warning text-dark | DaDuyet=bg-success | TuChoi=bg-danger (kèm lý do) | DaHuy=bg-secondary

**POST /DangKy/DangKy** (HocVien, AJAX):
1. Kiểm tra HocVien chưa có đơn active (ChoDuyet/DaDuyet) cho lớp này
2. Kiểm tra lớp còn chỗ: đếm DangKyKhoaHoc DaDuyet của lớp < SiSoToiDa
3. Lưu DangKyKhoaHoc mới TrangThai="ChoDuyet"
4. Tạo ThongBao cho Admin (LoaiThongBao="DangKy"): "Học viên {HoTen} vừa đăng ký lớp {TenLop}"
5. Trả JSON: `{success: true, message: "Đã gửi đơn đăng ký, vui lòng chờ admin duyệt"}`

**POST /DangKy/Duyet/{id}** (Admin):
- Cập nhật TrangThai="DaDuyet", NgayDuyet=now, NguoiDuyetId=currentUserId
- Tạo ThongBao cho HocVien: "Đơn đăng ký lớp {TenLop} của bạn đã được duyệt"
- Trả JSON `{success, message}`

**POST /DangKy/TuChoi/{id}** (Admin):
- Body: `{lyDo: "..."}`
- Cập nhật TrangThai="TuChoi", LyDoTuChoi=lyDo
- Tạo ThongBao cho HocVien: "Đơn đăng ký lớp {TenLop} bị từ chối. Lý do: {lyDo}"
- Trả JSON `{success, message}`

**POST /DangKy/Huy/{id}**:
- HocVien: chỉ hủy được đơn của mình khi TrangThai=ChoDuyet
- Admin: hủy bất kỳ đơn nào
- Cập nhật TrangThai="DaHuy"

---

## NHÓM 3 – QUẢN LÝ GIẢNG VIÊN VÀ PHÂN CÔNG GIẢNG DẠY (1,5đ)

### 3.1 – GiangVienController.cs (Admin) – kiểm tra và bổ sung:

**GET /GiangVien/Index**:
- Bảng: STT | Mã GV | Họ tên | Email | Chuyên môn | Bằng cấp | Kinh nghiệm (năm) | Số lớp đang dạy | Hành động

**GET /GiangVien/Details/{id}**:
- Thông tin cá nhân đầy đủ
- Danh sách lớp đang dạy và đã dạy: tên lớp, tên khóa, thời gian, số HV, trạng thái

**GET/POST /GiangVien/Create** (Admin):
- Tạo NguoiDung (VaiTro=GiangVien) + bản ghi GiangVien
- Tự sinh MaGiangVien: GV + 3 số (GV001, GV002...)
- Mật khẩu mặc định BCrypt hash

**GET/POST /GiangVien/Edit/{id}**: Sửa thông tin GiangVien và NguoiDung

**POST /GiangVien/Delete/{id}**:
- Kiểm tra còn lớp đang dạy (TrangThai=DangHoc) không → nếu có: báo lỗi "Không thể xóa vì giảng viên đang phụ trách lớp học"
- Nếu không: soft delete IsActive=false

### 3.2 – PhanCongGVController.cs – tạo mới nếu chưa có:

**GET /PhanCongGV/Index** (Admin):
Layout 2 cột:
- Cột trái (danh sách lớp): tên lớp | khóa học | ngày khai giảng | GV hiện tại (hoặc badge "Chưa phân công" màu đỏ) | nút chọn GV (dropdown)
- Cột phải (danh sách GV): tên GV | chuyên môn | số lớp đang dạy | badge chuyên môn
- Mỗi lớp có dropdown chọn GV + nút "Phân công" (AJAX, không reload trang)

**POST /PhanCongGV/PhanCong** (Admin, AJAX):
- Body: `{lopHocId: 1, giangVienId: 2}`
- Cập nhật LopHoc.GiangVienId
- Tạo ThongBao cho GiangVien (LoaiThongBao="HeThong"): "Bạn được phân công giảng dạy lớp {TenLop}, khai giảng {NgayKhaiGiang}"
- Trả JSON: `{success: true, message: "Đã phân công giảng viên {HoTen} cho lớp {TenLop}"}`

**POST /PhanCongGV/HuyPhanCong/{lopHocId}** (Admin, AJAX):
- Set LopHoc.GiangVienId = null
- Trả JSON `{success, message}`

### 3.3 – Dashboard GiangVien – GET /GiangVien/Dashboard:

3 metric cards hàng đầu:
- Số lớp đang dạy (đếm LopHoc WHERE GiangVienId==mine AND TrangThai==DangHoc)
- Tổng học viên (tổng DangKyKhoaHoc DaDuyet của tất cả lớp mình)
- Buổi dạy hôm nay (đếm LichHoc WHERE ngày hôm nay)

Danh sách lớp đang phụ trách: card mỗi lớp gồm tên lớp, tên khóa, ngày giờ buổi tiếp theo, phòng học, sĩ số HV.

Lịch dạy tuần này: bảng ngang 7 cột (T2–CN), mỗi ô hiển thị tên lớp + giờ nếu có buổi.

---

## NHÓM 4 – QUẢN LÝ ĐIỂM VÀ KẾT QUẢ HỌC TẬP (1,0đ)

### 4.1 – Kiểm tra Entity Diem có đủ các trường:
```csharp
public class Diem {
    public int Id { get; set; }
    public int DangKyId { get; set; }              // FK → DangKyKhoaHoc.Id
    public float? DiemGiuaKy { get; set; }         // 0–10, nullable
    public float? DiemCuoiKy { get; set; }         // 0–10, nullable
    public float? DiemTongKet { get; set; }        // tự tính = GK*0.3 + CK*0.7
    public string XepLoai { get; set; }            // tự tính theo thang
    public string NhanXetGiangVien { get; set; }
    public bool IsKhoa { get; set; } = false;      // Admin khóa = không cho sửa
    public DateTime? NgayCapNhat { get; set; }
    public virtual DangKyKhoaHoc DangKy { get; set; }
}
```
Nếu thiếu trường nào → migration + `dotnet ef database update`.

### 4.2 – DiemController.cs – viết lại hoàn toàn:

**Hàm tính xếp loại** (private):
```csharp
private string TinhXepLoai(float diem) => diem switch {
    >= 8.5f => "Xuất sắc",
    >= 7.0f => "Giỏi",
    >= 5.5f => "Khá",
    >= 4.0f => "Trung bình",
    _ => "Yếu"
};
```

**GET /DiemSo/QuanLy** (Admin):
- Filter: dropdown LopHoc, dropdown KhoaHoc, input tìm tên HV
- Bảng: STT | Học viên | Lớp học | Khóa học | Điểm GK | Điểm CK | Tổng kết | Xếp loại | Trạng thái (badge "Đã khóa"/"Chưa khóa")
- Thống kê tóm tắt cuối trang: Tổng HV | Điểm TB | Tỷ lệ đạt ≥5.0 | Tỷ lệ khá ≥5.5
- Nút "Export Excel" + nút "Khóa điểm" per lớp

**GET /DiemSo/NhapDiem** (GiangVien & Admin):
- Dropdown chọn LopHoc (GiangVien chỉ thấy lớp mình; Admin thấy tất cả)
- Sau khi chọn lớp: AJAX `/DiemSo/GetHocVienCuaLop?lopHocId=X` load danh sách HV kèm điểm hiện tại
- Bảng nhập inline:

| STT | Họ tên | Điểm GK (0–10) | Điểm CK (0–10) | Tổng kết | Xếp loại | Nhận xét |
|-----|--------|----------------|----------------|----------|-----------|----------|
| 1   | Nguyễn A | `<input class="diem-gk" data-id="1">` | `<input class="diem-ck">` | `<span class="tong-ket">` | `<span class="xep-loai">` | `<input class="nhan-xet">` |

- JavaScript realtime: khi thay đổi GK hoặc CK → tính `TK = GK*0.3 + CK*0.7` → cập nhật cột Tổng kết và Xếp loại ngay lập tức
- Input validation: min=0, max=10, step=0.1
- Nút "💾 Lưu tất cả điểm" → POST AJAX BatchSave → toast "Đã lưu thành công"
- Nếu lớp IsKhoa=true: tất cả input disabled, hiển thị banner cảnh báo "🔒 Điểm lớp này đã được khóa bởi Admin"

**POST /DiemSo/BatchSave** (AJAX, Admin & GiangVien):
```csharp
[HttpPost]
[Authorize(Roles = "Admin,GiangVien")]
public async Task<JsonResult> BatchSave([FromBody] List<DiemBatchDto> danhSach)
{
    foreach (var item in danhSach) {
        var diem = await _db.Diems.FirstOrDefaultAsync(x => x.DangKyId == item.DangKyId);
        if (diem == null) {
            diem = new Diem { DangKyId = item.DangKyId };
            _db.Diems.Add(diem);
        }
        if (diem.IsKhoa) continue;
        diem.DiemGiuaKy = item.DiemGiuaKy;
        diem.DiemCuoiKy = item.DiemCuoiKy;
        if (item.DiemGiuaKy.HasValue && item.DiemCuoiKy.HasValue)
            diem.DiemTongKet = (float)Math.Round(item.DiemGiuaKy.Value * 0.3 + item.DiemCuoiKy.Value * 0.7, 1);
        diem.XepLoai = diem.DiemTongKet.HasValue ? TinhXepLoai(diem.DiemTongKet.Value) : null;
        diem.NhanXetGiangVien = item.NhanXet;
        diem.NgayCapNhat = DateTime.Now;
    }
    await _db.SaveChangesAsync();
    return Json(new { success = true, message = "Đã lưu điểm thành công!" });
}
```

**POST /DiemSo/KhoaDiem/{lopHocId}** (Admin):
- Set IsKhoa=true cho tất cả Diem của các DangKyKhoaHoc thuộc lớp này
- Trả JSON `{success, message}`

**GET /DiemSo/CuaToi** (HocVien):
- Lấy tất cả DangKyKhoaHoc WHERE HocVienId==mine AND TrangThai==DaDuyet, JOIN Diem, JOIN LopHoc, JOIN KhoaHoc
- Hiển thị accordion Bootstrap 5:
  - Header: tên khóa học + badge xếp loại màu (Xuất sắc=success, Giỏi=primary, Khá=info, Trung bình=warning, Yếu=danger)
  - Body: bảng 2 cột (label | giá trị): Điểm GK | Điểm CK | Điểm tổng kết | Xếp loại | Nhận xét của GV
  - Nếu chưa có bản ghi Diem: "⏳ Giảng viên chưa nhập điểm cho khóa học này"

**GET /DiemSo/ExportExcel/{lopHocId}** (Admin & GiangVien):
- ClosedXML, các cột: STT | Mã HV | Họ tên | Điểm GK | Điểm CK | Điểm tổng kết | Xếp loại | Nhận xét
- Row 1: tiêu đề "BẢNG ĐIỂM – {TenLop}" merge toàn bộ cột, căn giữa, font 14 bold
- Row 2: header cột, background #4472C4 chữ trắng bold
- Data rows: căn giữa cột điểm, auto-fit tất cả cột
- Tên file: `BangDiem_{TenLop}_{yyyyMMdd}.xlsx`

### 4.3 – Seed data điểm mẫu (SeedData.cs):
Bổ sung điểm cho ít nhất 5 bản ghi DangKyKhoaHoc đã DaDuyet, đa dạng: 1 Xuất sắc (GK=9, CK=9.5), 1 Giỏi (GK=7.5, CK=8), 1 Khá (GK=6, CK=6.5), 1 Trung bình (GK=4.5, CK=5), 1 Yếu (GK=3, CK=3.5).

---

## NHÓM 5 – HỆ THỐNG ĐĂNG NHẬP VÀ PHÂN QUYỀN (1,0đ)

### 5.1 – AccountController.cs – kiểm tra và bổ sung:

**GET/POST /Account/Login**:
- Form: Email (input email) + Password (input password) + Remember me (checkbox)
- Client-side validation: jQuery Validate
- Server-side: kiểm tra email tồn tại, verify BCrypt.Verify(password, hash), kiểm tra IsActive==true
- Nếu IsActive==false: ModelState.AddModelError → "Tài khoản đã bị khóa, vui lòng liên hệ Admin"
- Nếu sai email/mật khẩu: "Email hoặc mật khẩu không chính xác"
- Đăng nhập thành công: Claims gồm NameIdentifier=Id, Name=HoTen, Role=VaiTro
- Redirect theo VaiTro: Admin→/Admin | GiangVien→/GiangVien/Dashboard | HocVien→/HocVien/Dashboard

**GET /Account/Logout**: SignOut cookie → redirect /Account/Login

**GET/POST /Account/ChangePassword** (tất cả role, [Authorize]):
- Form: Mật khẩu hiện tại | Mật khẩu mới | Xác nhận mật khẩu mới
- Validation mật khẩu mới: >=8 ký tự, có ít nhất 1 chữ hoa, 1 chữ số, 1 ký tự đặc biệt
- BCrypt.Verify(matKhauHienTai, hash) trước khi cho đổi
- Nếu sai mật khẩu hiện tại: "Mật khẩu hiện tại không đúng"

### 5.2 – Kiểm tra Program.cs:
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
```
Thêm nếu chưa có. Đảm bảo `app.UseAuthentication()` và `app.UseAuthorization()` đúng thứ tự trong pipeline.

### 5.3 – Kiểm tra [Authorize] toàn bộ Controller:
- KhoaHocController Create/Edit/Delete/ChangeStatus: [Authorize(Roles="Admin")]
- HocVienController: [Authorize(Roles="Admin")]
- GiangVienController: [Authorize(Roles="Admin")]
- PhanCongGVController: [Authorize(Roles="Admin")]
- DangKyController.DangKy: [Authorize(Roles="HocVien")]
- DangKyController.Duyet/TuChoi: [Authorize(Roles="Admin")]
- DiemController.NhapDiem/BatchSave/KhoaDiem: [Authorize(Roles="Admin,GiangVien")]
- DiemController.CuaToi: [Authorize(Roles="HocVien")]
- LichHocController Create/Edit/Delete/TaoHangLoat: [Authorize(Roles="Admin")]

### 5.4 – Quản lý tài khoản – TaiKhoanController.cs (Admin):

**GET /TaiKhoan/Index**:
- Danh sách tất cả NguoiDung, lọc theo VaiTro (select), tìm theo tên/email
- Bảng: Họ tên | Email | Vai trò (badge) | Ngày tạo | Trạng thái | Hành động

**POST /TaiKhoan/KhoaTaiKhoan/{id}**:
- Toggle IsActive (true→false hoặc false→true)
- Không được khóa chính mình (kiểm tra id != currentUserId)
- Trả JSON `{success, newStatus, message}`

**POST /TaiKhoan/ResetMatKhau/{id}**:
- Đặt MatKhauHash = BCrypt.HashPassword(defaultPassword)
- Trả JSON `{success, message: "Đã reset về mật khẩu mặc định: {defaultPassword}"}`

### 5.5 – Bảo mật:
- Kiểm tra tất cả form POST có `@Html.AntiForgeryToken()` và action có `[ValidateAntiForgeryToken]`
- Tất cả AJAX POST phải gửi header: `headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }`
- Trang AccessDenied: tạo Views/Account/AccessDenied.cshtml với thông báo "Bạn không có quyền truy cập trang này"

---

## NHÓM 6 – GIAO DIỆN WEB RESPONSIVE BOOTSTRAP 5 (1,0đ)

### 6.1 – Kiểm tra 3 Layout file:

**_LayoutAdmin.cshtml** – sidebar phải có đầy đủ:
```
TỔNG QUAN
  └─ Dashboard (/Admin)
QUẢN LÝ
  ├─ Khóa học (/KhoaHoc)
  ├─ Lớp học (/LopHoc)
  ├─ Học viên (/HocVien)
  ├─ Giảng viên (/GiangVien)
  ├─ Đăng ký (/DangKy)
  ├─ Lịch học (/LichHoc)
  ├─ Quản lý điểm (/DiemSo/QuanLy)
  └─ Phân công GV (/PhanCongGV)
AI & BÁO CÁO
  ├─ Báo cáo (/BaoCao)
  └─ Log AI Gợi ý (/GoiY/Admin)
TÀI KHOẢN
  ├─ Quản lý tài khoản (/TaiKhoan)
  ├─ Hồ sơ (/Profile)
  ├─ Thông báo (/ThongBao)
  ├─ Đổi mật khẩu (/Account/ChangePassword)
  └─ Đăng xuất (/Account/Logout)
```

**_LayoutGiangVien.cshtml**:
```
TỔNG QUAN
  └─ Dashboard (/GiangVien/Dashboard)
GIẢNG DẠY
  ├─ Lớp & Học viên (/LopHoc)
  ├─ Nhập điểm (/DiemSo/NhapDiem)
  └─ Lịch dạy (/LichHoc/CuaToi)
TÀI KHOẢN
  ├─ Hồ sơ (/Profile)
  ├─ Thông báo (/ThongBao)
  ├─ Đổi mật khẩu (/Account/ChangePassword)
  └─ Đăng xuất (/Account/Logout)
```

**_LayoutHocVien.cshtml**:
```
TỔNG QUAN
  └─ Dashboard (/HocVien/Dashboard)
HỌC TẬP
  ├─ Khóa học (/KhoaHoc)
  ├─ Đăng ký của tôi (/DangKy/CuaToi)
  ├─ Lịch học (/LichHoc/CuaToi)
  └─ Điểm của tôi (/DiemSo/CuaToi)
AI
  └─ Gợi ý AI (/GoiY)
TÀI KHOẢN
  ├─ Hồ sơ (/Profile)
  ├─ Thông báo (/ThongBao)
  ├─ Đổi mật khẩu (/Account/ChangePassword)
  └─ Đăng xuất (/Account/Logout)
```

Navbar top (tất cả layout): tên trung tâm bên trái | bên phải: bell icon thông báo (badge đỏ số chưa đọc) + ảnh đại diện + họ tên + badge vai trò.

Hamburger button hiện trên mobile (≤991px), click toggle class `.show` cho sidebar. Overlay mờ khi sidebar mở trên mobile.

### 6.2 – Dashboard Admin (/Admin/Index):

**Hàng 1 – 4 metric cards**:
- Tổng học viên (đếm HocVien IsActive=true) – icon bi-people – màu xanh (#3b82f6)
- Khóa học đang mở (đếm KhoaHoc TrangThai=DangMo) – icon bi-book – màu xanh lá (#22c55e)
- Lớp đang hoạt động (đếm LopHoc TrangThai=DangHoc) – icon bi-building – màu cam (#f59e0b)
- Tổng thu tháng này (tính từ số HV DaDuyet × học phí KhoaHoc tương ứng) – icon bi-cash – màu tím (#8b5cf6)

**Hàng 2 – 2 biểu đồ Chart.js**:
- Biểu đồ cột (bar): Số lượng đăng ký theo 6 tháng gần nhất (group by tháng của NgayDangKy). Label = "Tháng M/YYYY". Dataset màu xanh.
- Biểu đồ tròn (doughnut): Tỷ lệ khóa học Tiếng Anh vs Tiếng Nhật (đếm theo NgonNgu). 2 màu xanh dương và đỏ cam.

**Hàng 3 – 2 bảng**:
- Lớp sắp khai giảng: 5 lớp LopHoc ORDER BY NgayKhaiGiang ASC, chỉ lấy ngày trong tương lai. Cột: Tên lớp | Khóa học | Ngày khai giảng | Giảng viên | Sĩ số đăng ký
- Đơn đăng ký chờ duyệt: 5 DangKyKhoaHoc TrangThai=ChoDuyet ORDER BY NgayDangKy DESC. Cột: Học viên | Lớp | Khóa | Ngày đăng ký | nút Duyệt nhanh

### 6.3 – Dashboard HocVien (/HocVien/Dashboard):
- Card thông tin: ảnh đại diện + họ tên + email + trình độ + ngôn ngữ quan tâm
- Danh sách khóa đang học (DaDuyet): tên lớp, tên khóa, buổi học tiếp theo (ngày + giờ + phòng)
- Lịch học tuần này: bảng mini 7 cột, highlight ngày có lịch
- Gợi ý AI nổi bật: nếu có GoiYKhoaHoc gần nhất, hiển thị card gợi ý đầu tiên
- 3 thông báo chưa đọc gần nhất

### 6.4 – Responsive CSS (thêm vào wwwroot/css/site.css):
```css
/* Mobile sidebar */
@media (max-width: 991px) {
    .sidebar { position: fixed; top: 0; left: 0; height: 100vh; z-index: 1050; transform: translateX(-100%); transition: transform 0.3s ease; }
    .sidebar.show { transform: translateX(0); }
    .sidebar-overlay { display: none; position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 1040; }
    .sidebar-overlay.show { display: block; }
    .main-content { margin-left: 0 !important; }
}
/* Calendar mobile */
@media (max-width: 768px) {
    .view-btn[data-view="month"], .view-btn[data-view="week"] { display: none; }
    .day-cell { min-height: 70px; }
    .event-detail { display: none; }
}
/* Tables */
.table-responsive { overflow-x: auto; -webkit-overflow-scrolling: touch; }
```

---

## NHÓM 7 – HOÀN THIỆN VÀ SEED DATA

### 7.1 – Seed Data (SeedData.cs) – kiểm tra đủ dữ liệu mẫu:
```
Tài khoản (NguoiDung + bảng con):
  admin@nnl.com / Admin@123  → VaiTro=Admin
  gv01@nnl.com  / Gv@123    → VaiTro=GiangVien, ChuyenMon=Tiếng Anh, KinhNghiem=5
  gv02@nnl.com  / Gv@123    → VaiTro=GiangVien, ChuyenMon=Tiếng Nhật, KinhNghiem=3
  hv01@nnl.com  / Hv@123    → VaiTro=HocVien, TrinhDo=Sơ cấp, NgonNguQuanTam=Tiếng Anh
  hv02@nnl.com  / Hv@123    → VaiTro=HocVien, TrinhDo=Trung cấp, NgonNguQuanTam=Tiếng Anh
  hv03@nnl.com  / Hv@123    → VaiTro=HocVien, TrinhDo=Sơ cấp, NgonNguQuanTam=Tiếng Nhật

Khóa học (5 khóa, đủ 2 ngôn ngữ):
  Tiếng Anh Giao tiếp Sơ cấp  – HocPhi=2.000.000 – TrangThai=DangMo
  Tiếng Anh IELTS              – HocPhi=4.500.000 – TrangThai=DangMo
  Tiếng Anh B1                 – HocPhi=3.500.000 – TrangThai=DangMo
  Tiếng Nhật N5 Sơ cấp        – HocPhi=2.500.000 – TrangThai=DangMo
  Tiếng Nhật N4                – HocPhi=3.000.000 – TrangThai=DangMo

Lớp học (ít nhất 3 lớp TrangThai=DangHoc, có GiangVienId):
  Lớp Anh Sơ Cấp K01/2026  → gv01, SiSoToiDa=20
  Lớp IELTS K01/2026        → gv01, SiSoToiDa=15
  Lớp Nhật N5 K01/2026     → gv02, SiSoToiDa=15

LichHoc: mỗi lớp có lịch học tháng hiện tại + tháng sau (2 buổi/tuần, cách ngày)

DangKyKhoaHoc:
  hv01 → Lớp Anh Sơ Cấp K01/2026 – TrangThai=DaDuyet
  hv02 → Lớp IELTS K01/2026 – TrangThai=DaDuyet
  hv03 → Lớp Nhật N5 K01/2026 – TrangThai=DaDuyet
  hv01 → Lớp IELTS K01/2026 – TrangThai=ChoDuyet (chờ duyệt)

Diem (đa dạng xếp loại):
  hv01 + Lớp Anh Sơ Cấp: GK=9.0, CK=9.5 → TK=9.35 → Xuất sắc
  hv02 + Lớp IELTS:       GK=7.5, CK=8.0 → TK=7.85 → Giỏi
  hv03 + Lớp Nhật N5:    GK=6.0, CK=6.5 → TK=6.35 → Khá
```

### 7.2 – Profile – ProfileController.cs (tất cả role):

**GET/POST /Profile/Index**:
- Xem + sửa: HoTen, SoDienThoai
- HocVien thêm: NgaySinh, GioiTinh (select Nam/Nữ/Khác), DiaChi, TrinhDoHienTai, NgonNguQuanTam
- Upload ảnh đại diện: chỉ jpg/png, max 5MB, lưu `wwwroot/uploads/avatars/{userId}.jpg`, xóa ảnh cũ trước khi lưu ảnh mới
- Nếu chưa có ảnh: hiển thị avatar mặc định (tạo file `wwwroot/images/default-avatar.png`)

### 7.3 – Thông báo – ThongBaoController.cs:

**Bell icon navbar**: AJAX GET `/ThongBao/SoChuaDoc` mỗi 30 giây, cập nhật badge số. Nếu count=0 thì ẩn badge.

**GET /ThongBao/Index**: Danh sách thông báo sắp xếp mới nhất trên. Icon theo LoaiThongBao: DangKy=📋, LichHoc=📅, Diem=📊, HeThong=🔔. Chưa đọc: background xanh nhạt.

**POST /ThongBao/DanhDauDaDoc/{id}**: Set DaDoc=true, trả JSON `{success}`.

**POST /ThongBao/DanhDauTatCa**: Set DaDoc=true cho tất cả thông báo của currentUser.

**GET /ThongBao/SoChuaDoc** (AJAX): Trả `{count: N}`.

### 7.4 – Báo cáo – BaoCaoController.cs (Admin):

**GET /BaoCao/Index**:
- 4 số liệu tổng hợp: tổng HV, tổng GV, tổng khóa học, tổng lớp đã kết thúc
- Biểu đồ Chart.js cột: số HV đăng ký theo từng tháng trong năm hiện tại

**GET /BaoCao/ExportTongHop** (Admin):
- ClosedXML tạo file nhiều sheet:
  - Sheet "Hoc Vien": STT, Mã HV, Họ tên, Email, Trình độ, Ngày đăng ký
  - Sheet "Khoa Hoc": STT, Tên, Ngôn ngữ, Trình độ, Học phí, Số lớp, Số HV đã đăng ký
  - Sheet "Bang Diem": STT, Học viên, Lớp, GK, CK, Tổng kết, Xếp loại
- Tên file: `BaoCaoTongHop_{yyyyMMdd}.xlsx`

---

## YÊU CẦU BẮT BUỘC

1. Đọc kỹ code hiện tại TRƯỚC khi sửa, không làm hỏng chức năng đang chạy tốt
2. Sau mỗi nhóm: `dotnet build` → 0 Error 0 Warning
3. Thay đổi DB schema: `dotnet ef migrations add {TenMigration}` → `dotnet ef database update`
4. KHÔNG chạm vào bất kỳ code nào liên quan đến: AI, Chatbot, Groq, Gemini, GoiY, ChatHistory
5. Toàn bộ text hiển thị phải bằng tiếng Việt
6. Bootstrap 5 nhất quán, async/await cho tất cả DB và HTTP call
7. Server-side DataAnnotations + Client-side jQuery Validate cho mọi form có input
8. Cuối cùng: chạy tests, liệt kê đầy đủ URL tất cả chức năng đã hoàn thành theo từng nhóm
