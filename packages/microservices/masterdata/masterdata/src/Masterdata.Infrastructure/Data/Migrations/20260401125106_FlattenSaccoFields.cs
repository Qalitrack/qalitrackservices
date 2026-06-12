using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlattenSaccoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactInfo",
                table: "Saccos");

            // AlterColumn jsonb→text requires explicit USING cast; EF doesn't generate it for PostgreSQL
            migrationBuilder.Sql(@"ALTER TABLE ""Saccos"" ALTER COLUMN ""OtherDetails"" TYPE text USING ""OtherDetails""::text;");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Saccos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Saccos");

            migrationBuilder.Sql(@"ALTER TABLE ""Saccos"" ALTER COLUMN ""OtherDetails"" TYPE jsonb USING ""OtherDetails""::jsonb;");

            migrationBuilder.AddColumn<string>(
                name: "ContactInfo",
                table: "Saccos",
                type: "jsonb",
                nullable: true);
        }
    }
}
