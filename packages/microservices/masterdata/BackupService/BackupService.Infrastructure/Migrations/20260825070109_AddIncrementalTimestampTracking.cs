using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackupService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncrementalTimestampTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<DateTime>>(
                name: "IncrementalTimestamps",
                schema: "backup",
                table: "backup_chains",
                type: "timestamp with time zone[]",
                nullable: false,
                defaultValueSql: "'{}'::timestamp with time zone[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncrementalTimestamps",
                schema: "backup",
                table: "backup_chains");
        }
    }
}
