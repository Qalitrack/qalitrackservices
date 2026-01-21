using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Transaction.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    TicketID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiptNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstWeight = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SecondWeight = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NetWeight = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    vehicleID = table.Column<int>(type: "integer", nullable: true),
                    NoPlate = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DriverName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CommodityID = table.Column<int>(type: "integer", nullable: true),
                    CommodityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupplierID = table.Column<int>(type: "integer", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomerID = table.Column<int>(type: "integer", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransporterID = table.Column<int>(type: "integer", nullable: false),
                    TransporterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OriginID = table.Column<int>(type: "integer", nullable: true),
                    OriginName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DestinationID = table.Column<int>(type: "integer", nullable: true),
                    DestinationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeighBridgeID = table.Column<int>(type: "integer", nullable: true),
                    WeighBridgeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScaleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperatorID = table.Column<int>(type: "integer", nullable: true),
                    OperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeighBridgeName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ScaleName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OperatorID2nd = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OperatorName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeighMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstWeightDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SecondWeightDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Active"),
                    ReweighPermission = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ChangeDesc = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    api_id = table.Column<int>(type: "integer", nullable: true),
                    Id = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.TicketID);
                });

            migrationBuilder.CreateTable(
                name: "ReweighRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WeighbridgeTransactionId = table.Column<int>(type: "integer", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Weight1 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Operator1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Weight1Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Weight2 = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Operator2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Weight2Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NetWeight = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReweighRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReweighRecords_tickets_WeighbridgeTransactionId",
                        column: x => x.WeighbridgeTransactionId,
                        principalTable: "tickets",
                        principalColumn: "TicketID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReweighRecords_AttemptNumber",
                table: "ReweighRecords",
                column: "AttemptNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ReweighRecords_StartedAt",
                table: "ReweighRecords",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReweighRecords_Status",
                table: "ReweighRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReweighRecords_WeighbridgeTransactionId",
                table: "ReweighRecords",
                column: "WeighbridgeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_FirstWeightDate",
                table: "tickets",
                column: "FirstWeightDate");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_NoPlate",
                table: "tickets",
                column: "NoPlate");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_ReceiptNo",
                table: "tickets",
                column: "ReceiptNo");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_Status",
                table: "tickets",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReweighRecords");

            migrationBuilder.DropTable(
                name: "tickets");
        }
    }
}
