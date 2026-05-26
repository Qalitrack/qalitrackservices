using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReweighed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_reweighed",
                table: "tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_reweighed",
                table: "tickets");
        }
    }
}
