using Microsoft.EntityFrameworkCore;
using QuanLyKhoaHoc5.Web.Helpers;
using QuanLyKhoaHoc5.Web.Models.Entities;

namespace QuanLyKhoaHoc5.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, IConfiguration config)
    {
        if (await db.NguoiDungs.AnyAsync()) return;

        // ============ TÀI KHOẢN MẶC ĐỊNH ============
        var admin = new NguoiDung
        {
            Email = "admin@nnl.com",
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            VaiTro = "Admin",
            HoTen = "Quản trị viên",
            IsActive = true
        };
        db.NguoiDungs.Add(admin);
        await db.SaveChangesAsync();

        // ============ GIẢNG VIÊN ============
        var gv1ND = new NguoiDung { Email = "gv01@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Gv@123"), VaiTro = "GiangVien", HoTen = "Nguyễn Thị Hương", SoDienThoai = "0912345678", IsActive = true };
        var gv2ND = new NguoiDung { Email = "gv02@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Gv@123"), VaiTro = "GiangVien", HoTen = "Trần Thanh Nam",   SoDienThoai = "0987654321", IsActive = true };
        var gv3ND = new NguoiDung { Email = "gv03@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Gv@123"), VaiTro = "GiangVien", HoTen = "Lê Minh Đức",      SoDienThoai = "0978123456", IsActive = true };
        db.NguoiDungs.AddRange(gv1ND, gv2ND, gv3ND);
        await db.SaveChangesAsync();

        var gv1 = new GiangVien { Id = gv1ND.Id, MaGiangVien = "GV001", HoTen = "Nguyễn Thị Hương", ChuyenMon = "Tiếng Anh",       BangCap = "Thạc sĩ",  KinhNghiem = 8, MoTa = "Chuyên gia tiếng Anh giao tiếp và luyện thi IELTS" };
        var gv2 = new GiangVien { Id = gv2ND.Id, MaGiangVien = "GV002", HoTen = "Trần Thanh Nam",   ChuyenMon = "Tiếng Nhật",      BangCap = "Cử nhân",  KinhNghiem = 5, MoTa = "JLPT N1, kinh nghiệm dạy tiếng Nhật thương mại" };
        var gv3 = new GiangVien { Id = gv3ND.Id, MaGiangVien = "GV003", HoTen = "Lê Minh Đức",      ChuyenMon = "Tiếng Hàn",       BangCap = "Thạc sĩ",  KinhNghiem = 6, MoTa = "TOPIK cấp 6, từng học và làm việc tại Hàn Quốc" };
        db.GiangViens.AddRange(gv1, gv2, gv3);
        await db.SaveChangesAsync();

        // ============ HỌC VIÊN MẶC ĐỊNH ============
        var hv1ND = new NguoiDung { Email = "hv01@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Hv@123"), VaiTro = "HocVien", HoTen = "Nguyễn Văn An",   IsActive = true };
        var hv2ND = new NguoiDung { Email = "hv02@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Hv@123"), VaiTro = "HocVien", HoTen = "Lê Thị Bích",     IsActive = true };
        var hv3ND = new NguoiDung { Email = "hv03@nnl.com", MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Hv@123"), VaiTro = "HocVien", HoTen = "Phạm Văn Cường",  IsActive = true };
        db.NguoiDungs.AddRange(hv1ND, hv2ND, hv3ND);
        await db.SaveChangesAsync();

        var hv1 = new HocVien { Id = hv1ND.Id, MaHocVien = "HV001", HoTen = "Nguyễn Văn An",  GioiTinh = "Nam", TrinhDoHienTai = "Sơ cấp",    NgonNguQuanTam = "Tiếng Anh", NgaySinh = new DateOnly(2000, 5, 15), NgayDangKy = DateTime.Now.AddMonths(-3) };
        var hv2 = new HocVien { Id = hv2ND.Id, MaHocVien = "HV002", HoTen = "Lê Thị Bích",    GioiTinh = "Nữ",  TrinhDoHienTai = "Trung cấp", NgonNguQuanTam = "Tiếng Nhật", NgaySinh = new DateOnly(2001, 8, 20), NgayDangKy = DateTime.Now.AddMonths(-2) };
        var hv3 = new HocVien { Id = hv3ND.Id, MaHocVien = "HV003", HoTen = "Phạm Văn Cường", GioiTinh = "Nam", TrinhDoHienTai = "Sơ cấp",    NgonNguQuanTam = "Tiếng Hàn", NgaySinh = new DateOnly(2002, 3, 10), NgayDangKy = DateTime.Now.AddMonths(-1) };
        db.HocViens.AddRange(hv1, hv2, hv3);
        await db.SaveChangesAsync();

        // ============ 6 KHÓA HỌC ============
        var today = DateOnly.FromDateTime(DateTime.Now);
        var khoaHocs = new List<KhoaHoc>
        {
            new() { TenKhoaHoc = "Tiếng Anh A1",        MoTa = "Khóa tiếng Anh cơ bản nhất dành cho người mới bắt đầu hoàn toàn.",                 NgonNgu = "Tiếng Anh",  TrinhDo = "A1",      HocPhi = 3500000,  SoChoToiDa = 20, ThoiLuong = 40, SoBuoiMoiTuan = 3, ThoiGianMoiBuoi = 90, TrangThai = "DangMo", NoiDungChuongTrinh = "Phát âm, từ vựng cơ bản, ngữ pháp nền tảng, hội thoại đơn giản" },
            new() { TenKhoaHoc = "Tiếng Anh B1",        MoTa = "Luyện tiếng Anh B1 tự tin giao tiếp trong môi trường quốc tế.",                    NgonNgu = "Tiếng Anh",  TrinhDo = "B1",      HocPhi = 4500000,  SoChoToiDa = 15, ThoiLuong = 48, SoBuoiMoiTuan = 3, ThoiGianMoiBuoi = 90, TrangThai = "DangMo", NoiDungChuongTrinh = "Ngữ pháp nâng cao, kỹ năng thuyết trình, viết email, hội thoại chuyên nghiệp" },
            new() { TenKhoaHoc = "Tiếng Anh IELTS",     MoTa = "Luyện thi IELTS mục tiêu 6.0–7.5, bao gồm đầy đủ 4 kỹ năng.",                     NgonNgu = "Tiếng Anh",  TrinhDo = "IELTS",   HocPhi = 6000000,  SoChoToiDa = 12, ThoiLuong = 80, SoBuoiMoiTuan = 3, ThoiGianMoiBuoi = 120, TrangThai = "DangMo", NoiDungChuongTrinh = "Listening, Reading, Writing, Speaking, luyện đề thật IELTS" },
            new() { TenKhoaHoc = "Tiếng Nhật N5",       MoTa = "Học tiếng Nhật từ cơ bản, thi đạt JLPT N5.",                                       NgonNgu = "Tiếng Nhật", TrinhDo = "N5",      HocPhi = 4000000,  SoChoToiDa = 18, ThoiLuong = 50, SoBuoiMoiTuan = 3, ThoiGianMoiBuoi = 90, TrangThai = "DangMo", NoiDungChuongTrinh = "Hiragana, Katakana, 100 Kanji N5, ngữ pháp, hội thoại cơ bản" },
            new() { TenKhoaHoc = "Tiếng Nhật N4",       MoTa = "Nâng cao tiếng Nhật từ N5 lên N4, đạt chứng chỉ JLPT N4.",                         NgonNgu = "Tiếng Nhật", TrinhDo = "N4",      HocPhi = 5000000,  SoChoToiDa = 15, ThoiLuong = 60, SoBuoiMoiTuan = 2, ThoiGianMoiBuoi = 120, TrangThai = "DangMo", NoiDungChuongTrinh = "300 Kanji N4, ngữ pháp trung cấp, đọc hiểu, nghe" },
            new() { TenKhoaHoc = "Tiếng Hàn Sơ Cấp",   MoTa = "Học tiếng Hàn từ bảng chữ Hangul, thi đạt TOPIK cấp 1–2.",                        NgonNgu = "Tiếng Hàn",  TrinhDo = "Sơ cấp",  HocPhi = 3800000,  SoChoToiDa = 20, ThoiLuong = 48, SoBuoiMoiTuan = 3, ThoiGianMoiBuoi = 90, TrangThai = "DangMo", NoiDungChuongTrinh = "Bảng chữ Hangul, từ vựng, ngữ pháp, hội thoại, luyện đề TOPIK" },
        };
        db.KhoaHocs.AddRange(khoaHocs);
        await db.SaveChangesAsync();

        // ============ Khoảng thời gian cho các lớp ============
        var lichA1Start = today.AddDays(7);
        var lichA1End   = today.AddDays(7 + 90);
        var lichB1Start = today.AddDays(5);
        var lichB1End   = today.AddDays(5 + 90);
        var lichIStart  = today.AddDays(-15);
        var lichIEnd    = today.AddDays(-15 + 120);
        var lichN5Start = today.AddDays(10);
        var lichN5End   = today.AddDays(10 + 90);
        var lichN4Start = today.AddDays(3);
        var lichN4End   = today.AddDays(3 + 90);
        var lichHStart  = today.AddDays(-10);
        var lichHEnd    = today.AddDays(-10 + 90);

        // ============ LỚP HỌC (tạo trước để LichHoc có LopHocId) ============
        var lopHocs = new List<LopHoc>
        {
            new() { TenLop = "Anh A1 K01/2025",     KhoaHocId = khoaHocs[0].Id, GiangVienId = gv1.Id, NgayKhaiGiang = lichA1Start, NgayKetThuc = lichA1End, SiSoToiDa = 20, PhongHoc = "P101", TrangThai = "DangTuyenSinh" },
            new() { TenLop = "Anh B1 K01/2025",     KhoaHocId = khoaHocs[1].Id, GiangVienId = gv1.Id, NgayKhaiGiang = lichB1Start, NgayKetThuc = lichB1End, SiSoToiDa = 15, PhongHoc = "P102", TrangThai = "DangHoc" },
            new() { TenLop = "IELTS K01/2025",       KhoaHocId = khoaHocs[2].Id, GiangVienId = gv1.Id, NgayKhaiGiang = lichIStart,  NgayKetThuc = lichIEnd,  SiSoToiDa = 12, PhongHoc = "P201", TrangThai = "DangHoc" },
            new() { TenLop = "Nhật N5 K01/2025",     KhoaHocId = khoaHocs[3].Id, GiangVienId = gv2.Id, NgayKhaiGiang = lichN5Start, NgayKetThuc = lichN5End, SiSoToiDa = 18, PhongHoc = "P202", TrangThai = "DangTuyenSinh" },
            new() { TenLop = "Nhật N4 K01/2025",     KhoaHocId = khoaHocs[4].Id, GiangVienId = gv2.Id, NgayKhaiGiang = lichN4Start, NgayKetThuc = lichN4End, SiSoToiDa = 15, PhongHoc = "P203", TrangThai = "DangHoc" },
            new() { TenLop = "Hàn Sơ Cấp K01/2025", KhoaHocId = khoaHocs[5].Id, GiangVienId = gv3.Id, NgayKhaiGiang = lichHStart,  NgayKetThuc = lichHEnd,  SiSoToiDa = 20, PhongHoc = "P301", TrangThai = "DangHoc" },
        };
        db.LopHocs.AddRange(lopHocs);
        await db.SaveChangesAsync();

        // ============ LỊCH HỌC (per-session, gắn LopHoc) ============
        // Sinh buổi học cho tháng hiện tại + tháng sau (2 buổi/tuần)
        var sessionStart = today.AddDays(-20); // lùi 20 ngày để IELTS/Hàn có dữ liệu
        var sessionEnd   = today.AddDays(60);

        var lichHocs = new List<LichHoc>();

        // Lớp Anh A1: T2, T4 sáng 8h
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Monday, DayOfWeek.Wednesday]))
            lichHocs.Add(new() { LopHocId = lopHocs[0].Id, NgayHoc = d, GioBatDau = new TimeOnly(8,0), GioKetThuc = new TimeOnly(9,30), PhongHoc = "P101" });

        // Lớp Anh B1: T3, T5 chiều 14h
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Tuesday, DayOfWeek.Thursday]))
            lichHocs.Add(new() { LopHocId = lopHocs[1].Id, NgayHoc = d, GioBatDau = new TimeOnly(14,0), GioKetThuc = new TimeOnly(15,30), PhongHoc = "P102" });

        // Lớp IELTS: T2, T4 tối 18h30
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Monday, DayOfWeek.Wednesday]))
            lichHocs.Add(new() { LopHocId = lopHocs[2].Id, NgayHoc = d, GioBatDau = new TimeOnly(18,30), GioKetThuc = new TimeOnly(20,30), PhongHoc = "P201" });

        // Lớp Nhật N5: T3, T6 sáng 8h
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Tuesday, DayOfWeek.Friday]))
            lichHocs.Add(new() { LopHocId = lopHocs[3].Id, NgayHoc = d, GioBatDau = new TimeOnly(8,0), GioKetThuc = new TimeOnly(9,30), PhongHoc = "P202" });

        // Lớp Nhật N4: T2, T5 chiều 14h
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Monday, DayOfWeek.Thursday]))
            lichHocs.Add(new() { LopHocId = lopHocs[4].Id, NgayHoc = d, GioBatDau = new TimeOnly(14,0), GioKetThuc = new TimeOnly(16,0), PhongHoc = "P203" });

        // Lớp Hàn Sơ Cấp: T3, T5 tối 19h
        foreach (var d in LichHocHelper.GenerateDates(sessionStart, sessionEnd, [DayOfWeek.Tuesday, DayOfWeek.Thursday]))
            lichHocs.Add(new() { LopHocId = lopHocs[5].Id, NgayHoc = d, GioBatDau = new TimeOnly(19,0), GioKetThuc = new TimeOnly(20,30), PhongHoc = "P301" });

        db.LichHocs.AddRange(lichHocs);
        await db.SaveChangesAsync();

        // ============ PHÂN CÔNG GIẢNG VIÊN ============
        var phanCongs = new List<PhanCongGiangDay>
        {
            new() { GiangVienId = gv1.Id, KhoaHocId = khoaHocs[0].Id, NgayPhanCong = DateTime.Now.AddDays(-20), GhiChu = "GV chuyên tiếng Anh", IsActive = true },
            new() { GiangVienId = gv1.Id, KhoaHocId = khoaHocs[1].Id, NgayPhanCong = DateTime.Now.AddDays(-20), GhiChu = "GV chuyên tiếng Anh", IsActive = true },
            new() { GiangVienId = gv1.Id, KhoaHocId = khoaHocs[2].Id, NgayPhanCong = DateTime.Now.AddDays(-20), GhiChu = "GV chuyên IELTS",     IsActive = true },
            new() { GiangVienId = gv2.Id, KhoaHocId = khoaHocs[3].Id, NgayPhanCong = DateTime.Now.AddDays(-15), GhiChu = "GV chuyên tiếng Nhật",IsActive = true },
            new() { GiangVienId = gv2.Id, KhoaHocId = khoaHocs[4].Id, NgayPhanCong = DateTime.Now.AddDays(-15), GhiChu = "GV chuyên tiếng Nhật",IsActive = true },
            new() { GiangVienId = gv3.Id, KhoaHocId = khoaHocs[5].Id, NgayPhanCong = DateTime.Now.AddDays(-10), GhiChu = "GV chuyên tiếng Hàn", IsActive = true },
        };
        db.PhanCongGiangDays.AddRange(phanCongs);
        await db.SaveChangesAsync();

        // ============ ĐĂNG KÝ MẪU ============
        var rnd = new Random(42);
        // hv01 -> Anh B1 lớp 1 (DaDuyet), hv01 -> IELTS lớp 2 (DaDuyet)
        var dk1 = new DangKyKhoaHoc { HocVienId = hv1.Id, LopHocId = lopHocs[1].Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now.AddDays(-40), NgayDuyet = DateTime.Now.AddDays(-38), NguoiDuyetId = admin.Id };
        var dk2 = new DangKyKhoaHoc { HocVienId = hv1.Id, LopHocId = lopHocs[2].Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now.AddDays(-14), NgayDuyet = DateTime.Now.AddDays(-12), NguoiDuyetId = admin.Id };
        // hv02 -> Nhật N5 (DaDuyet), Hàn Sơ Cấp (DaDuyet)
        var dk3 = new DangKyKhoaHoc { HocVienId = hv2.Id, LopHocId = lopHocs[3].Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now.AddDays(-30), NgayDuyet = DateTime.Now.AddDays(-28), NguoiDuyetId = admin.Id };
        var dk4 = new DangKyKhoaHoc { HocVienId = hv2.Id, LopHocId = lopHocs[5].Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now.AddDays(-10), NgayDuyet = DateTime.Now.AddDays(-8),  NguoiDuyetId = admin.Id };
        // hv03 -> Hàn Sơ Cấp (DaDuyet), Nhật N4 (ChoDuyet)
        var dk5 = new DangKyKhoaHoc { HocVienId = hv3.Id, LopHocId = lopHocs[5].Id, TrangThai = "DaDuyet", NgayDangKy = DateTime.Now.AddDays(-9), NgayDuyet = DateTime.Now.AddDays(-7),  NguoiDuyetId = admin.Id };
        var dk6 = new DangKyKhoaHoc { HocVienId = hv3.Id, LopHocId = lopHocs[4].Id, TrangThai = "ChoDuyet", NgayDangKy = DateTime.Now.AddDays(-2) };

        db.DangKyKhoaHocs.AddRange(dk1, dk2, dk3, dk4, dk5, dk6);
        await db.SaveChangesAsync();

        // ============ KỲ HỌC MẪU ============
        var ky1 = new KyHoc { TenKy = "Kỳ 1 – 2024/2025", NgayBatDau = new DateOnly(2024, 9, 1), NgayKetThuc = new DateOnly(2025, 1, 31), TrangThai = "DaDong" };
        var ky2 = new KyHoc { TenKy = "Kỳ 2 – 2024/2025", NgayBatDau = new DateOnly(2025, 2, 1), NgayKetThuc = new DateOnly(2025, 6, 30), TrangThai = "DangMo" };
        db.KyHocs.AddRange(ky1, ky2);
        await db.SaveChangesAsync();

        // ============ ĐIỂM MẪU (dùng công thức mới GK*30%+CK*70%) ============
        // hv01 B1: điểm giữa kỳ + cuối kỳ
        db.Diems.Add(new Diem { DangKyId = dk1.Id, KyHocId = ky1.Id, DiemGiuaKy = 7.5, DiemCuoiKy = 8.0, DiemTongKet = Diem.TinhTongKet(7.5, 8.0), XepLoai = Diem.TinhXepLoai(Diem.TinhTongKet(7.5, 8.0)), NhanXetGiangVien = "Học viên tiến bộ tốt, phát âm chuẩn.", NgayCapNhat = DateTime.Now.AddDays(-5) });
        // hv01 IELTS: chỉ điểm giữa kỳ
        db.Diems.Add(new Diem { DangKyId = dk2.Id, KyHocId = ky2.Id, DiemGiuaKy = 6.5, NhanXetGiangVien = "Writing cần luyện thêm.", NgayCapNhat = DateTime.Now.AddDays(-2) });
        // hv02 Nhật N5: điểm đầy đủ
        db.Diems.Add(new Diem { DangKyId = dk3.Id, KyHocId = ky1.Id, DiemGiuaKy = 9.0, DiemCuoiKy = 9.5, DiemTongKet = Diem.TinhTongKet(9.0, 9.5), XepLoai = Diem.TinhXepLoai(Diem.TinhTongKet(9.0, 9.5)), NhanXetGiangVien = "Học viên xuất sắc, nắm vững ngữ pháp N5.", NgayCapNhat = DateTime.Now.AddDays(-3) });
        // hv02 Hàn Sơ Cấp: điểm đầy đủ
        db.Diems.Add(new Diem { DangKyId = dk4.Id, KyHocId = ky2.Id, DiemGiuaKy = 8.0, DiemCuoiKy = 8.5, DiemTongKet = Diem.TinhTongKet(8.0, 8.5), XepLoai = Diem.TinhXepLoai(Diem.TinhTongKet(8.0, 8.5)), NhanXetGiangVien = "Phát âm Hàn rất tốt.", NgayCapNhat = DateTime.Now.AddDays(-1) });
        // hv03 Hàn Sơ Cấp: điểm giữa kỳ
        db.Diems.Add(new Diem { DangKyId = dk5.Id, KyHocId = ky2.Id, DiemGiuaKy = 5.5, NhanXetGiangVien = "Cần ôn thêm từ vựng.", NgayCapNhat = DateTime.Now.AddDays(-1) });

        await db.SaveChangesAsync();

        // Thêm đăng ký thêm từ hv khác để dashboard có số liệu
        var hvExtraData = new[]
        {
            ("hv04@nnl.com", "Trần Thị Dung",     "HV004", "Nữ",  new DateOnly(1999, 6, 25),  "Trung cấp", "Tiếng Anh"),
            ("hv05@nnl.com", "Hoàng Minh Em",      "HV005", "Nam", new DateOnly(2001, 9, 12),  "Sơ cấp",    "Tiếng Nhật"),
            ("hv06@nnl.com", "Ngô Thị Phương",     "HV006", "Nữ",  new DateOnly(2002, 4, 3),   "Sơ cấp",    "Tiếng Hàn"),
            ("hv07@nnl.com", "Vũ Minh Giang",      "HV007", "Nam", new DateOnly(2000, 11, 18), "Cao cấp",   "Tiếng Anh"),
            ("hv08@nnl.com", "Đặng Thị Hà",        "HV008", "Nữ",  new DateOnly(2003, 1, 7),   "Sơ cấp",    "Tiếng Anh"),
        };

        var extraNDs = new List<NguoiDung>();
        foreach (var d in hvExtraData)
            extraNDs.Add(new NguoiDung { Email = d.Item1, MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Hv@123"), VaiTro = "HocVien", HoTen = d.Item2, IsActive = true });
        db.NguoiDungs.AddRange(extraNDs);
        await db.SaveChangesAsync();

        var extraHVs = new List<HocVien>();
        for (int i = 0; i < hvExtraData.Length; i++)
        {
            var d = hvExtraData[i];
            extraHVs.Add(new HocVien { Id = extraNDs[i].Id, MaHocVien = d.Item3, HoTen = d.Item2, GioiTinh = d.Item4, NgaySinh = d.Item5, TrinhDoHienTai = d.Item6, NgonNguQuanTam = d.Item7, NgayDangKy = DateTime.Now.AddDays(-rnd.Next(5, 60)) });
        }
        db.HocViens.AddRange(extraHVs);
        await db.SaveChangesAsync();

        // Đăng ký cho học viên phụ (để dashboard có số liệu)
        var extraDKs = new[]
        {
            (extraHVs[0].Id, lopHocs[1].Id, "DaDuyet", DateTime.Now.AddDays(-35), DateTime.Now.AddDays(-33), 8.5, 9.0),
            (extraHVs[1].Id, lopHocs[3].Id, "DaDuyet", DateTime.Now.AddDays(-28), DateTime.Now.AddDays(-26), 7.0, 7.5),
            (extraHVs[2].Id, lopHocs[5].Id, "DaDuyet", DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-18), 6.5, 7.0),
            (extraHVs[3].Id, lopHocs[2].Id, "DaDuyet", DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-13), 9.0, 9.5),
            (extraHVs[4].Id, lopHocs[0].Id, "ChoDuyet", DateTime.Now.AddDays(-3), (DateTime?)null, (double?)null, (double?)null),
        };

        foreach (var (hvId, lopId, tt, ngDK, ngDuyet, gk, ck) in extraDKs)
        {
            var dk = new DangKyKhoaHoc { HocVienId = hvId, LopHocId = lopId, TrangThai = tt, NgayDangKy = ngDK, NgayDuyet = ngDuyet, NguoiDuyetId = tt == "DaDuyet" ? admin.Id : null };
            db.DangKyKhoaHocs.Add(dk);
            await db.SaveChangesAsync();
            if (tt == "DaDuyet" && gk.HasValue)
            {
                double? tk = Diem.TinhTongKet(gk, ck);
                string? xl = Diem.TinhXepLoai(tk);
                db.Diems.Add(new Diem { DangKyId = dk.Id, KyHocId = ky1.Id, DiemGiuaKy = gk, DiemCuoiKy = ck, DiemTongKet = tk, XepLoai = xl, NgayCapNhat = DateTime.Now.AddDays(-2) });
                await db.SaveChangesAsync();
            }
        }

        // ============ SEED THANH TOÁN MẪU ============
        await SeedThanhToanAsync(db, admin.Id);
    }

    /// <summary>
    /// Seed kỳ học mẫu. An toàn: chỉ thêm nếu chưa có kỳ học nào.
    /// </summary>
    public static async Task SeedKyHocAsync(AppDbContext db)
    {
        if (await db.KyHocs.AnyAsync()) return;

        db.KyHocs.AddRange(
            new KyHoc { TenKy = "Kỳ 1 – 2024/2025", NgayBatDau = new DateOnly(2024, 9, 1), NgayKetThuc = new DateOnly(2025, 1, 31), TrangThai = "DaDong" },
            new KyHoc { TenKy = "Kỳ 2 – 2024/2025", NgayBatDau = new DateOnly(2025, 2, 1), NgayKetThuc = new DateOnly(2025, 6, 30), TrangThai = "DangMo" }
        );
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seed dữ liệu thanh toán mẫu. Có thể gọi riêng trên DB đã có dữ liệu.
    /// </summary>
    public static async Task SeedThanhToanAsync(AppDbContext db, int adminId = 0)
    {
        if (await db.ThanhToans.AnyAsync()) return;

        // Lấy một số HocVien và KhoaHoc để tạo mẫu
        var hocViens = await db.HocViens.Take(5).ToListAsync();
        var khoaHocs = await db.KhoaHocs.Take(3).ToListAsync();

        if (!hocViens.Any() || !khoaHocs.Any()) return;

        // Nếu không truyền adminId, lấy admin đầu tiên
        if (adminId == 0)
        {
            var adminUser = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTro == "Admin");
            adminId = adminUser?.Id ?? 0;
        }

        var thanhToans = new List<ThanhToan>
        {
            // Đã thanh toán (chuyển khoản)
            new() {
                HocVienId = hocViens[0].Id,
                KhoaHocId = khoaHocs[0].Id,
                SoTien = khoaHocs[0].HocPhi,
                PhuongThuc = "ChuyenKhoan",
                TrangThai = "DaThanhToan",
                GhiChu = "Đã xác nhận chuyển khoản",
                NgayTao = DateTime.Now.AddDays(-15),
                NgayDuyet = DateTime.Now.AddDays(-14),
                NguoiDuyetId = adminId > 0 ? adminId : null
            },
            // Chờ phê duyệt (chuyển khoản)
            new() {
                HocVienId = hocViens[1].Id,
                KhoaHocId = khoaHocs[1].Id,
                SoTien = khoaHocs[1].HocPhi,
                PhuongThuc = "ChuyenKhoan",
                TrangThai = "ChoPheduyet",
                GhiChu = null,
                NgayTao = DateTime.Now.AddDays(-2)
            },
            // Đã thanh toán (tiền mặt)
            new() {
                HocVienId = hocViens[2 < hocViens.Count ? 2 : 0].Id,
                KhoaHocId = khoaHocs[2 < khoaHocs.Count ? 2 : 0].Id,
                SoTien = khoaHocs[2 < khoaHocs.Count ? 2 : 0].HocPhi,
                PhuongThuc = "TienMat",
                TrangThai = "DaThanhToan",
                GhiChu = "Nộp tiền mặt tại văn phòng",
                NgayTao = DateTime.Now.AddDays(-10),
                NgayDuyet = DateTime.Now.AddDays(-10),
                NguoiDuyetId = adminId > 0 ? adminId : null
            },
            // Từ chối
            new() {
                HocVienId = hocViens[1 < hocViens.Count ? 1 : 0].Id,
                KhoaHocId = khoaHocs[0].Id,
                SoTien = khoaHocs[0].HocPhi,
                PhuongThuc = "ChuyenKhoan",
                TrangThai = "TuChoi",
                GhiChu = "Ảnh bill không rõ, vui lòng gửi lại",
                NgayTao = DateTime.Now.AddDays(-5),
                NgayDuyet = DateTime.Now.AddDays(-4),
                NguoiDuyetId = adminId > 0 ? adminId : null
            }
        };

        db.ThanhToans.AddRange(thanhToans);
        await db.SaveChangesAsync();
    }
}
