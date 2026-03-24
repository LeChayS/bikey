using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bikey.Migrations
{
    /// <inheritdoc />
    public partial class RestructurePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanCreateHoaDon",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanCreateHopDong",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanDeleteHinhAnhXe",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanDeleteHoaDon",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanDeleteHopDong",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanEditHinhAnhXe",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanEditHoaDon",
                table: "PhanQuyen");

            migrationBuilder.DropColumn(
                name: "CanEditHopDong",
                table: "PhanQuyen");

            migrationBuilder.RenameColumn(
                name: "CanViewHinhAnhXe",
                table: "PhanQuyen",
                newName: "CanReturnVehicle");

            migrationBuilder.RenameColumn(
                name: "CanUploadHinhAnhXe",
                table: "PhanQuyen",
                newName: "CanProcessBooking");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanReturnVehicle",
                table: "PhanQuyen",
                newName: "CanViewHinhAnhXe");

            migrationBuilder.RenameColumn(
                name: "CanProcessBooking",
                table: "PhanQuyen",
                newName: "CanUploadHinhAnhXe");

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateHoaDon",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateHopDong",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteHinhAnhXe",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteHoaDon",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteHopDong",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEditHinhAnhXe",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEditHoaDon",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEditHopDong",
                table: "PhanQuyen",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
