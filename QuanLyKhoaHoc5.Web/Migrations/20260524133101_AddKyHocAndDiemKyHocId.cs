using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc5.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddKyHocAndDiemKyHocId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KyHocId",
                table: "Diems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KyHocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyHocs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diems_KyHocId",
                table: "Diems",
                column: "KyHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diems_KyHocs_KyHocId",
                table: "Diems",
                column: "KyHocId",
                principalTable: "KyHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diems_KyHocs_KyHocId",
                table: "Diems");

            migrationBuilder.DropTable(
                name: "KyHocs");

            migrationBuilder.DropIndex(
                name: "IX_Diems_KyHocId",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "KyHocId",
                table: "Diems");
        }
    }
}
