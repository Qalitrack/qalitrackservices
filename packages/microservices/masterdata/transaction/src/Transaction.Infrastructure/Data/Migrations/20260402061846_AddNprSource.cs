using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNprSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "npr_source",
                table: "tickets",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "npr_source",
                table: "tickets");
        }
    }
}
