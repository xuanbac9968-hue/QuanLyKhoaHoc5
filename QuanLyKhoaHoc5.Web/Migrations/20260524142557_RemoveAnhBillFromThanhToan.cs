using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKhoaHoc5.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAnhBillFromThanhToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhBill",
                table: "ThanhToans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhBill",
                table: "ThanhToans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
