using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogClassificationAndChain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "users",
                table: "AuditLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                schema: "users",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                schema: "users",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                schema: "users",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousHash",
                schema: "users",
                table: "AuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                schema: "users",
                table: "AuditLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                schema: "users",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_SequenceNumber",
                schema: "users",
                table: "AuditLogs",
                column: "SequenceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityType",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_SequenceNumber",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Action",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Hash",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "PreviousHash",
                schema: "users",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                schema: "users",
                table: "AuditLogs");
        }
    }
}
