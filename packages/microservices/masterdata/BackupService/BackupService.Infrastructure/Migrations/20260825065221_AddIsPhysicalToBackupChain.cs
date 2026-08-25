using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackupService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPhysicalToBackupChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPhysical",
                schema: "backup",
                table: "backup_chains",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPhysical",
                schema: "backup",
                table: "backup_chains");
        }
    }
}
