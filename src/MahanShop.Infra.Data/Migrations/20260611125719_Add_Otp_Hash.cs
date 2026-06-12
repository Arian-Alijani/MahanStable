using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MahanShop.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Otp_Hash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "OtpCodes");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "OtpCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "OtpCodes",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "OtpCodes");

            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "OtpCodes");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "OtpCodes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }
    }
}
