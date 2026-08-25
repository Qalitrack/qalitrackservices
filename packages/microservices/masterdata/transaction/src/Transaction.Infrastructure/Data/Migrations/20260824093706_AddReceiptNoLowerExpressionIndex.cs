using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptNoLowerExpressionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // GetByReceiptNoAsync/IsReceiptNoAvailableAsync compare on
            // lower("ReceiptNo"), which the plain btree index on ReceiptNo
            // can't serve — this expression index lets Postgres use an index
            // for that comparison instead of a sequential scan. Not
            // expressible via EF Core's fluent HasIndex API, hence raw SQL.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_tickets_ReceiptNo_lower\" ON transactions.tickets (lower(\"ReceiptNo\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS transactions.\"IX_tickets_ReceiptNo_lower\";");
        }
    }
}
