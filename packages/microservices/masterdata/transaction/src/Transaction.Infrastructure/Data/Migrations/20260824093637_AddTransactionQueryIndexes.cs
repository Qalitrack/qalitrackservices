using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tickets_CommodityID",
                schema: "transactions",
                table: "tickets",
                column: "CommodityID");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Status_FirstWeightDate",
                schema: "transactions",
                table: "tickets",
                columns: new[] { "Status", "FirstWeightDate" });

            migrationBuilder.CreateIndex(
                name: "IX_tickets_vehicleID",
                schema: "transactions",
                table: "tickets",
                column: "vehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_CommodityID",
                schema: "transactions",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "IX_tickets_Status_FirstWeightDate",
                schema: "transactions",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "IX_tickets_vehicleID",
                schema: "transactions",
                table: "tickets");
        }
    }
}
