using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc5.Web.Migrations
{
    /// <inheritdoc />
    public partial class RestructureLichHocToPerSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichHocs_KhoaHocs_KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropIndex(
                name: "IX_LichHocs_KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "KhoaHocId",
                table: "LichHocs");

            migrationBuilder.DropColumn(
                name: "NgayBatDau",
                table: "LichHocs");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_LichHocs_KhoaHocId",
                table: "LichHocs",
                column: "KhoaHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_LichHocs_KhoaHocs_KhoaHocId",
                table: "LichHocs",
                column: "KhoaHocId",
                principalTable: "KhoaHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
