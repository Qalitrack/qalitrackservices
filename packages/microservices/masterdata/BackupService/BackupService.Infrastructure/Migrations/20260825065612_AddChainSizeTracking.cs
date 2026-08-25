using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackupService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChainSizeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FullBackupSizeBytes",
                schema: "backup",
                table: "backup_chains",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<List<long>>(
                name: "IncrementalSizesBytes",
                schema: "backup",
                table: "backup_chains",
                type: "bigint[]",
                nullable: false,
                defaultValueSql: "'{}'::bigint[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullBackupSizeBytes",
                schema: "backup",
                table: "backup_chains");

            migrationBuilder.DropColumn(
                name: "IncrementalSizesBytes",
                schema: "backup",
                table: "backup_chains");
        }
    }
}
