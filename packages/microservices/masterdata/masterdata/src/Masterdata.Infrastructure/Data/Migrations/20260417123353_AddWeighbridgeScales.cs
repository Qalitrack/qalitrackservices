using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdata.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeighbridgeScales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddColumn with nullable:false generates no default, failing on tables with existing rows.
            // Use raw SQL to add with a temporary default (empty array), then drop the default.
            migrationBuilder.Sql(@"ALTER TABLE ""Weighbridges"" ADD COLUMN IF NOT EXISTS ""Scales"" jsonb NOT NULL DEFAULT '[]'::jsonb;");
            migrationBuilder.Sql(@"ALTER TABLE ""Weighbridges"" ALTER COLUMN ""Scales"" DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scales",
                table: "Weighbridges");
        }
    }
}
