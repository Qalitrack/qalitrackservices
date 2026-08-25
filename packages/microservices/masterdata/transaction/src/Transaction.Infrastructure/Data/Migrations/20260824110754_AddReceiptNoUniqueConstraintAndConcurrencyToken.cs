using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptNoUniqueConstraintAndConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: the EF-generated migration also included an AddColumn for
            // "xmin" (from UseXminAsConcurrencyToken()) — removed by hand.
            // xmin is a Postgres system column that already exists on every
            // table; EF's migration differ doesn't know that and generates an
            // AddColumn as if it were an ordinary shadow property, which
            // fails at runtime ("column namexmin conflicts with a system
            // column name"). No actual schema change is needed for the
            // concurrency token — only the ReceiptNo index change below is real.
            migrationBuilder.DropIndex(
                name: "IX_tickets_ReceiptNo",
                schema: "transactions",
                table: "tickets");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ReceiptNo",
                schema: "transactions",
                table: "tickets",
                column: "ReceiptNo",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tickets_ReceiptNo",
                schema: "transactions",
                table: "tickets");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ReceiptNo",
                schema: "transactions",
                table: "tickets",
                column: "ReceiptNo");
        }
    }
}
