using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bikey.Migrations
{
    /// <inheritdoc />
    public partial class deleteManage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanCheckout",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageCart",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageHinhAnhXe",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageHoaDon",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageHopDong",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageLoaiXe",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageUser",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanManageXe",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanViewCart",
                table: "PhanQuyen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanCheckout",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageCart",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageHinhAnhXe",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageHoaDon",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageHopDong",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageLoaiXe",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageUser",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageXe",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewCart",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
