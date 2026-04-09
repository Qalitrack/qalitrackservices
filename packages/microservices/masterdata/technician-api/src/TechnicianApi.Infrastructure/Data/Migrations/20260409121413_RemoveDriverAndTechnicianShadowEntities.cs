using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnicianApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDriverAndTechnicianShadowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceReturnForms_Technician_TechnicianId",
                table: "AdvanceReturnForms");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentBalanceSummaries_Technician_TechnicianId",
                table: "AssignmentBalanceSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Technician_TechnicianId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentTechnician_Technician_TechnicianId",
                table: "AssignmentTechnician");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Technician_TechnicianId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_DailySummaries_Technician_TechnicianId",
                table: "DailySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverActivities_Drivers_DriverId",
                table: "DriverActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_PerDiemReturnForms_Technician_TechnicianId",
                table: "PerDiemReturnForms");

            migrationBuilder.DropForeignKey(
                name: "FK_PerformanceMetrics_Technician_TechnicianId",
                table: "PerformanceMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_Refunds_Technician_TechnicianId",
                table: "Refunds");

            migrationBuilder.DropForeignKey(
                name: "FK_Requisitions_Technician_TechnicianId",
                table: "Requisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceReports_Technician_TechnicianId",
                table: "ServiceReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Drivers_DriverId",
                table: "Trips");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                table: "Trucks");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleMileages_Drivers_DriverId",
                table: "VehicleMileages");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Technician");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentTechnician_TechnicianId",
                table: "AssignmentTechnician");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_TechnicianId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "Assignments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TechnicianId",
                table: "Assignments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technician",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technician", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTechnician_TechnicianId",
                table: "AssignmentTechnician",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TechnicianId",
                table: "Assignments",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceReturnForms_Technician_TechnicianId",
                table: "AdvanceReturnForms",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentBalanceSummaries_Technician_TechnicianId",
                table: "AssignmentBalanceSummaries",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Technician_TechnicianId",
                table: "Assignments",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentTechnician_Technician_TechnicianId",
                table: "AssignmentTechnician",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Technician_TechnicianId",
                table: "Claims",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySummaries_Technician_TechnicianId",
                table: "DailySummaries",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverActivities_Drivers_DriverId",
                table: "DriverActivities",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerDiemReturnForms_Technician_TechnicianId",
                table: "PerDiemReturnForms",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerformanceMetrics_Technician_TechnicianId",
                table: "PerformanceMetrics",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Refunds_Technician_TechnicianId",
                table: "Refunds",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requisitions_Technician_TechnicianId",
                table: "Requisitions",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceReports_Technician_TechnicianId",
                table: "ServiceReports",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Drivers_DriverId",
                table: "Trips",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Drivers_DriverId",
                table: "Trucks",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleMileages_Drivers_DriverId",
                table: "VehicleMileages",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
