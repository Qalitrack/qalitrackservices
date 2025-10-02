using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackupService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "backup");

            migrationBuilder.CreateTable(
                name: "microservices",
                schema: "backup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConnectionString = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    LastBackupAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_microservices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "backup_chains",
                schema: "backup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MicroserviceId = table.Column<int>(type: "integer", nullable: false),
                    MicroserviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FullBackupFile = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Incrementals = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    Lsn = table.Column<string>(type: "text", nullable: true),
                    Timeline = table.Column<int>(type: "integer", nullable: true),
                    LastLsn = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backup_chains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_backup_chains_microservices_MicroserviceId",
                        column: x => x.MicroserviceId,
                        principalSchema: "backup",
                        principalTable: "microservices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_backup_chains_MicroserviceId",
                schema: "backup",
                table: "backup_chains",
                column: "MicroserviceId");

            migrationBuilder.CreateIndex(
                name: "IX_backup_chains_MicroserviceName",
                schema: "backup",
                table: "backup_chains",
                column: "MicroserviceName");

            migrationBuilder.CreateIndex(
                name: "IX_microservices_Name",
                schema: "backup",
                table: "microservices",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "backup_chains",
                schema: "backup");

            migrationBuilder.DropTable(
                name: "microservices",
                schema: "backup");
        }
    }
}
