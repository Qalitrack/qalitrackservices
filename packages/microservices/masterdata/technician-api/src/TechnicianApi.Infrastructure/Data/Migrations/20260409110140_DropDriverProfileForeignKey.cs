using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnicianApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropDriverProfileForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverProfiles_Drivers_DriverId",
                table: "DriverProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_DriverProfiles_Drivers_DriverId",
                table: "DriverProfiles",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
