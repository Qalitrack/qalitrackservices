using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDriverCreatedAtIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CreatedAt",
                schema: "masterdata",
                table: "Vehicles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedAt",
                schema: "masterdata",
                table: "Drivers",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CreatedAt",
                schema: "masterdata",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CreatedAt",
                schema: "masterdata",
                table: "Drivers");
        }
    }
}
