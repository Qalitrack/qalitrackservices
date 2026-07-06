using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveToMasterdataSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "masterdata");

            migrationBuilder.RenameTable(
                name: "Weighbridges",
                newName: "Weighbridges",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                newName: "Vehicles",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Transporters",
                newName: "Transporters",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "Suppliers",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Saccos",
                newName: "Saccos",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Routes",
                newName: "Routes",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Owners",
                newName: "Owners",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Organisations",
                newName: "Organisations",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "DriverVehicles",
                newName: "DriverVehicles",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Drivers",
                newName: "Drivers",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "Customers",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "AxleConfigurations",
                newName: "AxleConfigurations",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "AuditLogs",
                newSchema: "masterdata");

            migrationBuilder.RenameTable(
                name: "Affiliations",
                newName: "Affiliations",
                newSchema: "masterdata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Weighbridges",
                schema: "masterdata",
                newName: "Weighbridges");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                schema: "masterdata",
                newName: "Vehicles");

            migrationBuilder.RenameTable(
                name: "Transporters",
                schema: "masterdata",
                newName: "Transporters");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                schema: "masterdata",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "Saccos",
                schema: "masterdata",
                newName: "Saccos");

            migrationBuilder.RenameTable(
                name: "Routes",
                schema: "masterdata",
                newName: "Routes");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "masterdata",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Owners",
                schema: "masterdata",
                newName: "Owners");

            migrationBuilder.RenameTable(
                name: "Organisations",
                schema: "masterdata",
                newName: "Organisations");

            migrationBuilder.RenameTable(
                name: "DriverVehicles",
                schema: "masterdata",
                newName: "DriverVehicles");

            migrationBuilder.RenameTable(
                name: "Drivers",
                schema: "masterdata",
                newName: "Drivers");

            migrationBuilder.RenameTable(
                name: "Customers",
                schema: "masterdata",
                newName: "Customers");

            migrationBuilder.RenameTable(
                name: "AxleConfigurations",
                schema: "masterdata",
                newName: "AxleConfigurations");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "masterdata",
                newName: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Affiliations",
                schema: "masterdata",
                newName: "Affiliations");
        }
    }
}
