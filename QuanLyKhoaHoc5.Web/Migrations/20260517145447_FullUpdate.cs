using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc5.Web.Migrations
{
    /// <inheritdoc />
    public partial class FullUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHocs_LopHocs_LopHocId",
                table: "LichHocs");

            migrationBuilder.DropIndex(
                name: "IX_LichHocs_LopHocId",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "ChuDe",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "LichHocs");

            migrationBuilder.RenameColumn(
                name: "NgayHoc",
                table: "LichHocs",
                newName: "NgayKetThuc");

            migrationBuilder.RenameColumn(
                name: "LopHocId",
                table: "LichHocs",
                newName: "ThuTrongTuan");

            migrationBuilder.AddColumn<int>(
                name: "KhoaHocId",
                table: "LichHocs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NgayBatDau",
                table: "LichHocs",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "SoChoToiDa",
                table: "KhoaHocs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PhanCongGiangDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiangVienId = table.Column<int>(type: "int", nullable: false),
                    KhoaHocId = table.Column<int>(type: "int", nullable: false),
                    NgayPhanCong = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongGiangDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDays_GiangViens_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongGiangDays_KhoaHocs_KhoaHocId",
                        column: x => x.KhoaHocId,
                        principalTable: "KhoaHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_KhoaHocId",
                table: "LichHocs",
                column: "KhoaHocId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_GiangVienId",
                table: "PhanCongGiangDays",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongGiangDays_KhoaHocId",
                table: "PhanCongGiangDays",
                column: "KhoaHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHocs_KhoaHocs_KhoaHocId",
                table: "LichHocs",
                column: "KhoaHocId",
                principalTable: "KhoaHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHocs_KhoaHocs_KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropTable(
                name: "PhanCongGiangDays");

            migrationBuilder.DropIndex(
                name: "IX_LichHocs_KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "SoChoToiDa",
                table: "KhoaHocs");

            migrationBuilder.RenameColumn(
                name: "ThuTrongTuan",
                table: "LichHocs",
                newName: "LopHocId");

            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "LichHocs",
                newName: "NgayHoc");

            migrationBuilder.AddColumn<string>(
                name: "ChuDe",
                table: "LichHocs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "LichHocs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "LichHocs",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_LopHocId",
                table: "LichHocs",
                column: "LopHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHocs_LopHocs_LopHocId",
                table: "LichHocs",
                column: "LopHocId",
                principalTable: "LopHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
