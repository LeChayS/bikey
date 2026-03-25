using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bikey.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBangVaiTroAndUpdateStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDung_VaiTro_VaiTroMaVaiTro",
                table: "NguoiDung");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropIndex(
                name: "IX_NguoiDung_VaiTroMaVaiTro",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "VaiTroMaVaiTro",
                table: "NguoiDung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VaiTroMaVaiTro",
                table: "NguoiDung",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_VaiTroMaVaiTro",
                table: "NguoiDung",
                column: "VaiTroMaVaiTro");

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDung_VaiTro_VaiTroMaVaiTro",
                table: "NguoiDung",
                column: "VaiTroMaVaiTro",
                principalTable: "VaiTro",
                principalColumn: "MaVaiTro");
        }
    }
}
