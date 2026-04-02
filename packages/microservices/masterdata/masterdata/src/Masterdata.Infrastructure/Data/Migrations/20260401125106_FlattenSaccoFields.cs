using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlattenSaccoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Saccos");

            migrationBuilder.AlterColumn<string>(
                name: "OtherDetails",
                table: "Saccos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Saccos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Saccos");

            migrationBuilder.AlterColumn<string>(
                name: "OtherDetails",
                table: "Saccos",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Saccos",
                type: "jsonb",
                nullable: true);
        }
    }
}
