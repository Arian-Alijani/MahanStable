using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahanShop.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Admin_ValueParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentValueId",
                table: "VariantAttributeValues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributeValues_ParentValueId",
                table: "VariantAttributeValues",
                column: "ParentValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantAttributeValues_VariantAttributeValues_ParentValueId",
                table: "VariantAttributeValues",
                column: "ParentValueId",
                principalTable: "VariantAttributeValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantAttributeValues_VariantAttributeValues_ParentValueId",
                table: "VariantAttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_VariantAttributeValues_ParentValueId",
                table: "VariantAttributeValues");

            migrationBuilder.DropColumn(
                name: "ParentValueId",
                table: "VariantAttributeValues");
        }
    }
}
