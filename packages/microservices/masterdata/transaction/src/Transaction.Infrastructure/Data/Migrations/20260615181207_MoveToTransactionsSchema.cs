using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveToTransactionsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "transactions");

            migrationBuilder.RenameTable(
                name: "tickets",
                newName: "tickets",
                newSchema: "transactions");

            migrationBuilder.RenameTable(
                name: "ReweighRecords",
                newName: "ReweighRecords",
                newSchema: "transactions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SecondWeightDate",
                schema: "transactions",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "tickets",
                schema: "transactions",
                newName: "tickets");

            migrationBuilder.RenameTable(
                name: "ReweighRecords",
                schema: "transactions",
                newName: "ReweighRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SecondWeightDate",
                table: "tickets",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
