using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUnitAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");
        }
    }
}
