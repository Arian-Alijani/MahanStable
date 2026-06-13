using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahanShop.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Admin_VariantKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "VariantAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "VariantAttributeValues",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "VariantAttributes");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "VariantAttributeValues");
        }
    }
}
