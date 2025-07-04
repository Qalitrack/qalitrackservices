using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComplianceService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LicenseMonitoring",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DriverId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DriverName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LicenseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsMonitored = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextCheckDue = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiryWarningDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseMonitoring", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryStandards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StandardType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RegulatoryBody = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "KENYA"),
                    Region = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StandardDetails = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, defaultValue: "1.0"),
                    DocumentReference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryStandards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RouteRestrictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RouteId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RouteName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RestrictionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProductType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxHeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxWidth = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxLength = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RestrictedFromTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    RestrictedToTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    IsWeekdaysOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsWeekendsOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ViolationPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PenaltyCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "KES"),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "MEDIUM"),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryReference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AdditionalDetails = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteRestrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeightLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    VehicleClass = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaxGrossWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAxleWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxFrontAxleWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxRearAxleWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAxleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WeighbridgeId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RouteId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    OverweightPenaltyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PenaltyCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "KES"),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegulatoryReference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightLimits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RuleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "MEDIUM"),
                    MinValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PenaltyCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "KES"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RegulatoryStandardId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRules_RegulatoryStandards_RegulatoryStandardId",
                        column: x => x.RegulatoryStandardId,
                        principalTable: "RegulatoryStandards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComplianceRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VehicleId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DriverId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CheckedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LimitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CheckDetails = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CheckedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceChecks_ComplianceRules_ComplianceRuleId",
                        column: x => x.ComplianceRuleId,
                        principalTable: "ComplianceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceViolations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComplianceRuleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ViolationType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VehicleId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DriverId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WeighbridgeId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "ACTIVE"),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "MEDIUM"),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LimitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExcessValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    PenaltyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PenaltyCurrency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "KES"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LicenseMonitoringId = table.Column<int>(type: "INTEGER", nullable: true),
                    RouteRestrictionsId = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightLimitsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceViolations_ComplianceRules_ComplianceRuleId",
                        column: x => x.ComplianceRuleId,
                        principalTable: "ComplianceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplianceViolations_LicenseMonitoring_LicenseMonitoringId",
                        column: x => x.LicenseMonitoringId,
                        principalTable: "LicenseMonitoring",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceViolations_RouteRestrictions_RouteRestrictionsId",
                        column: x => x.RouteRestrictionsId,
                        principalTable: "RouteRestrictions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceViolations_WeightLimits_WeightLimitsId",
                        column: x => x.WeightLimitsId,
                        principalTable: "WeightLimits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: false),
                    NewValues = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ComplianceViolationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceAudits_ComplianceViolations_ComplianceViolationId",
                        column: x => x.ComplianceViolationId,
                        principalTable: "ComplianceViolations",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ComplianceRules",
                columns: new[] { "Id", "Category", "ConfigurationJson", "CreatedAt", "CreatedBy", "Description", "IsActive", "MaxValue", "MinValue", "Name", "PenaltyAmount", "PenaltyCurrency", "RegulatoryStandardId", "RuleType", "Severity", "Unit", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "GROSS_WEIGHT", "{\"maxWeight\": 56000, \"unit\": \"KG\", \"penaltyRate\": 5.0}", new DateTime(2025, 7, 2, 22, 30, 22, 85, DateTimeKind.Utc).AddTicks(3689), "SYSTEM", "Check if vehicle gross weight exceeds legal limits", true, 56000m, null, "Gross Weight Compliance", 5.0m, "KES", null, "WEIGHT", "HIGH", "KG", new DateTime(2025, 7, 2, 22, 30, 22, 85, DateTimeKind.Utc).AddTicks(8020), "SYSTEM" },
                    { 2, "EXPIRY", "{\"warningDays\": 30, \"criticalDays\": 7}", new DateTime(2025, 7, 2, 22, 30, 22, 86, DateTimeKind.Utc).AddTicks(131), "SYSTEM", "Monitor driver license expiry dates", true, null, 0m, "License Expiry Check", null, "KES", null, "LICENSE", "CRITICAL", "DAYS", new DateTime(2025, 7, 2, 22, 30, 22, 86, DateTimeKind.Utc).AddTicks(132), "SYSTEM" }
                });

            migrationBuilder.InsertData(
                table: "WeightLimits",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "EffectiveDate", "ExpiryDate", "IsActive", "MaxAxleCount", "MaxAxleWeight", "MaxFrontAxleWeight", "MaxGrossWeight", "MaxRearAxleWeight", "Name", "OrganizationId", "OverweightPenaltyRate", "PenaltyCurrency", "RegulatoryReference", "RouteId", "UpdatedAt", "UpdatedBy", "VehicleClass", "VehicleType", "WeighbridgeId" },
                values: new object[] { 1, new DateTime(2025, 7, 2, 22, 30, 22, 82, DateTimeKind.Utc).AddTicks(8332), "SYSTEM", "Standard weight limits for trucks in Kenya", new DateTime(2025, 7, 2, 22, 30, 22, 82, DateTimeKind.Utc).AddTicks(1187), null, true, 5, 18000m, 7000m, 56000m, 18000m, "Standard Truck Weight Limits", "", 5.0m, "KES", "Traffic Act Cap 403", "", new DateTime(2025, 7, 2, 22, 30, 22, 82, DateTimeKind.Utc).AddTicks(8820), "SYSTEM", "STANDARD", "TRUCK", "" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_Action",
                table: "ComplianceAudits",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_ComplianceViolationId",
                table: "ComplianceAudits",
                column: "ComplianceViolationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_EntityId",
                table: "ComplianceAudits",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_EntityType",
                table: "ComplianceAudits",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_Timestamp",
                table: "ComplianceAudits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAudits_UserId",
                table: "ComplianceAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_CheckedAt",
                table: "ComplianceChecks",
                column: "CheckedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_ComplianceRuleId",
                table: "ComplianceChecks",
                column: "ComplianceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_OrganizationId",
                table: "ComplianceChecks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_Result",
                table: "ComplianceChecks",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_Status",
                table: "ComplianceChecks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_Category",
                table: "ComplianceRules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_IsActive",
                table: "ComplianceRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_RegulatoryStandardId",
                table: "ComplianceRules",
                column: "RegulatoryStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_RuleType",
                table: "ComplianceRules",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_ComplianceRuleId",
                table: "ComplianceViolations",
                column: "ComplianceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_DetectedAt",
                table: "ComplianceViolations",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_DriverId",
                table: "ComplianceViolations",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_LicenseMonitoringId",
                table: "ComplianceViolations",
                column: "LicenseMonitoringId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_OrganizationId",
                table: "ComplianceViolations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_RouteRestrictionsId",
                table: "ComplianceViolations",
                column: "RouteRestrictionsId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_Status",
                table: "ComplianceViolations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_TransactionId",
                table: "ComplianceViolations",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_VehicleId",
                table: "ComplianceViolations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_ViolationType",
                table: "ComplianceViolations",
                column: "ViolationType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_WeightLimitsId",
                table: "ComplianceViolations",
                column: "WeightLimitsId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseMonitoring_DriverId",
                table: "LicenseMonitoring",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseMonitoring_ExpiryDate",
                table: "LicenseMonitoring",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseMonitoring_LicenseNumber",
                table: "LicenseMonitoring",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseMonitoring_OrganizationId",
                table: "LicenseMonitoring",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseMonitoring_Status",
                table: "LicenseMonitoring",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryStandards_Country",
                table: "RegulatoryStandards",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryStandards_IsActive",
                table: "RegulatoryStandards",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryStandards_StandardType",
                table: "RegulatoryStandards",
                column: "StandardType");

            migrationBuilder.CreateIndex(
                name: "IX_RouteRestrictions_IsActive",
                table: "RouteRestrictions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RouteRestrictions_OrganizationId",
                table: "RouteRestrictions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteRestrictions_RestrictionType",
                table: "RouteRestrictions",
                column: "RestrictionType");

            migrationBuilder.CreateIndex(
                name: "IX_RouteRestrictions_RouteId",
                table: "RouteRestrictions",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightLimits_IsActive",
                table: "WeightLimits",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WeightLimits_OrganizationId",
                table: "WeightLimits",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightLimits_VehicleClass",
                table: "WeightLimits",
                column: "VehicleClass");

            migrationBuilder.CreateIndex(
                name: "IX_WeightLimits_VehicleType",
                table: "WeightLimits",
                column: "VehicleType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplianceAudits");

            migrationBuilder.DropTable(
                name: "ComplianceChecks");

            migrationBuilder.DropTable(
                name: "ComplianceViolations");

            migrationBuilder.DropTable(
                name: "ComplianceRules");

            migrationBuilder.DropTable(
                name: "LicenseMonitoring");

            migrationBuilder.DropTable(
                name: "RouteRestrictions");

            migrationBuilder.DropTable(
                name: "WeightLimits");

            migrationBuilder.DropTable(
                name: "RegulatoryStandards");
        }
    }
}
