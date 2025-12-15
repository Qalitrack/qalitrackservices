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
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ReceiptNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpectedWeighings = table.Column<int>(type: "integer", nullable: false),
                    CompletedWeighings = table.Column<int>(type: "integer", nullable: false),
                    FirstWeight = table.Column<decimal>(type: "numeric", nullable: true),
                    FirstWeightTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SecondWeight = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondWeightTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NetWeight = table.Column<decimal>(type: "numeric", nullable: true),
                    NetWeightCalculatedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoPlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DriverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CommodityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommodityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TransporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransporterName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OriginId = table.Column<Guid>(type: "uuid", nullable: true),
                    OriginName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DestinationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WeighBridgeId = table.Column<Guid>(type: "uuid", nullable: true),
                    WeighBridgeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScaleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WeighBridgeName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScaleName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperatorId2nd = table.Column<Guid>(type: "uuid", nullable: true),
                    OperatorName2nd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WeighMode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Operation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReweighPermissionGranted = table.Column<bool>(type: "boolean", nullable: false),
                    ReweighPermissionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReweighReason = table.Column<string>(type: "text", nullable: true),
                    ReweighRequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReweighRequestedBy = table.Column<string>(type: "text", nullable: true),
                    CurrentReweighAttempt = table.Column<int>(type: "integer", nullable: false),
                    MaxReweighAttempts = table.Column<int>(type: "integer", nullable: false),
                    ChangeDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WeighbridgeTransactionId = table.Column<string>(type: "character varying(36)", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedFields = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: false),
                    NewValues = table.Column<string>(type: "text", nullable: false),
                    ChangeTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Transactions_WeighbridgeTransactionId",
                        column: x => x.WeighbridgeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReweighRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WeighbridgeTransactionId = table.Column<string>(type: "character varying(36)", nullable: false),
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
                        name: "FK_ReweighRecords_Transactions_WeighbridgeTransactionId",
                        column: x => x.WeighbridgeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeighingRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    WeighbridgeTransactionId = table.Column<string>(type: "character varying(36)", nullable: false),
                    WeighingSequence = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    WeighingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeighBridgeId = table.Column<Guid>(type: "uuid", nullable: true),
                    WeighBridgeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScaleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeighingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeighingRecords_Transactions_WeighbridgeTransactionId",
                        column: x => x.WeighbridgeTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangeTimestamp",
                table: "AuditLogs",
                column: "ChangeTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_WeighbridgeTransactionId",
                table: "AuditLogs",
                column: "WeighbridgeTransactionId");

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
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IsCompleted",
                table: "Transactions",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_NoPlate",
                table: "Transactions",
                column: "NoPlate");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiptNo",
                table: "Transactions",
                column: "ReceiptNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Status",
                table: "Transactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeighingRecords_WeighbridgeTransactionId",
                table: "WeighingRecords",
                column: "WeighbridgeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WeighingRecords_WeighingDate",
                table: "WeighingRecords",
                column: "WeighingDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ReweighRecords");

            migrationBuilder.DropTable(
                name: "WeighingRecords");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
