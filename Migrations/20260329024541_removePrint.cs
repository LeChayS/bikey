using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bikey.Migrations
{
    /// <inheritdoc />
    public partial class removePrint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanPrintHoaDon",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanPrintHopDong",
                table: "PhanQuyen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanPrintHoaDon",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanPrintHopDong",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
