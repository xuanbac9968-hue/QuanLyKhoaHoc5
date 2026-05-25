using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc5.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhoaHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgonNgu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrinhDo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HocPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThoiLuong = table.Column<int>(type: "int", nullable: false),
                    SoBuoiMoiTuan = table.Column<int>(type: "int", nullable: true),
                    ThoiGianMoiBuoi = table.Column<int>(type: "int", nullable: true),
                    AnhBia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NoiDungChuongTrinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "HocVien"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiangViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MaGiangVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChuyenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BangCap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KinhNghiem = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiangViens_NguoiDungs_Id",
                        column: x => x.Id,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HocViens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MaHocVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrinhDoHienTai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgonNguQuanTam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocViens_NguoiDungs_Id",
                        column: x => x.Id,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiNhanId = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiThongBao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuongDanLienKet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaos_NguoiDungs_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LopHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    GiangVienId = table.Column<int>(type: "int", nullable: true),
                    NgayKhaiGiang = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: true),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopHocs_GiangViens_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LopHocs_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoiYKhoaHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    KhoaHocGoiYId = table.Column<int>(type: "int", nullable: false),
                    DiemPhuHop = table.Column<double>(type: "float", nullable: true),
                    LyDoGoiY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptGuiDi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhanHoiAI = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayGoiY = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXem = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoiYKhoaHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoiYKhoaHocs_HocViens_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoiYKhoaHocs_KhoaHocs_KhoaHocGoiYId",
                        column: x => x.KhoaHocGoiYId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyKhoaHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiDuyetId = table.Column<int>(type: "int", nullable: true),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyKhoaHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKyKhoaHocs_HocViens_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyKhoaHocs_LopHocs_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyKhoaHocs_NguoiDungs_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LichHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    NgayHoc = table.Column<DateOnly>(type: "date", nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHocs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichHocs_LopHocs_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Diems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyId = table.Column<int>(type: "int", nullable: false),
                    DiemGiuaKy = table.Column<double>(type: "float", nullable: true),
                    DiemCuoiKy = table.Column<double>(type: "float", nullable: true),
                    DiemTongKet = table.Column<double>(type: "float", nullable: true),
                    XepLoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NhanXetGiangVien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsKhoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diems_DangKyKhoaHocs_DangKyId",
                        column: x => x.DangKyId,
                        principalTable: "DangKyKhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKhoaHocs_HocVienId_LopHocId",
                table: "DangKyKhoaHocs",
                columns: new[] { "HocVienId", "LopHocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKhoaHocs_LopHocId",
                table: "DangKyKhoaHocs",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKhoaHocs_NguoiDuyetId",
                table: "DangKyKhoaHocs",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_DangKyId",
                table: "Diems",
                column: "DangKyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_MaGiangVien",
                table: "GiangViens",
                column: "MaGiangVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoiYKhoaHocs_HocVienId",
                table: "GoiYKhoaHocs",
                column: "HocVienId");

            migrationBuilder.CreateIndex(
                name: "IX_GoiYKhoaHocs_KhoaHocGoiYId",
                table: "GoiYKhoaHocs",
                column: "KhoaHocGoiYId");

            migrationBuilder.CreateIndex(
                name: "IX_HocViens_MaHocVien",
                table: "HocViens",
                column: "MaHocVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_LopHocId",
                table: "LichHocs",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_GiangVienId",
                table: "LopHocs",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocs_KhoaHocId",
                table: "LopHocs",
                column: "KhoaHocId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_Email",
                table: "NguoiDungs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaos_NguoiNhanId",
                table: "ThongBaos",
                column: "NguoiNhanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diems");

            migrationBuilder.DropTable(
                name: "GoiYKhoaHocs");

            migrationBuilder.DropTable(
                name: "LichHocs");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "DangKyKhoaHocs");

            migrationBuilder.DropTable(
                name: "HocViens");

            migrationBuilder.DropTable(
                name: "LopHocs");

            migrationBuilder.DropTable(
                name: "GiangViens");

            migrationBuilder.DropTable(
                name: "KhoaHocs");

            migrationBuilder.DropTable(
                name: "NguoiDungs");
        }
    }
}
