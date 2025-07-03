using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataSyncService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfigId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direction = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultConflictResolution = table.Column<string>(type: "TEXT", nullable: false),
                    SyncIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    FilterCondition = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FieldMapping = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TransformationRules = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LastSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRetries = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationEmails = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EnableNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ConnectionString = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TimeZone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastSyncTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "TEXT", nullable: true),
                    IsMain = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthTokenHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    AuthTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSites", x => x.Id);
                    table.UniqueConstraint("AK_SyncSites_SiteId", x => x.SiteId);
                });

            migrationBuilder.CreateTable(
                name: "SiteHealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HealthCheckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CheckTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CheckType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsHealthy = table.Column<bool>(type: "INTEGER", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteHealthChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteHealthChecks_SyncSites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "SyncSites",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TotalRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessfulRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    FailedRecords = table.Column<int>(type: "INTEGER", nullable: false),
                    ConflictCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    SyncSiteId = table.Column<int>(type: "INTEGER", nullable: true),
                    SyncSiteId1 = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncSessions", x => x.Id);
                    table.UniqueConstraint("AK_SyncSessions_SessionId", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_SyncSessions_SyncSites_SyncSiteId",
                        column: x => x.SyncSiteId,
                        principalTable: "SyncSites",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SyncSessions_SyncSites_SyncSiteId1",
                        column: x => x.SyncSiteId1,
                        principalTable: "SyncSites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChangeRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChangeId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RecordId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Operation = table.Column<string>(type: "TEXT", nullable: false),
                    OldDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    NewDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    DeltaJson = table.Column<string>(type: "TEXT", nullable: true),
                    ChangeTimestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastRetryTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    ChecksumHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SequenceNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRecords_SyncSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SyncSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncConflicts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConflictId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RecordId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ConflictType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceDataJson = table.Column<string>(type: "TEXT", nullable: false),
                    TargetDataJson = table.Column<string>(type: "TEXT", nullable: false),
                    ResolvedDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    ResolutionStrategy = table.Column<string>(type: "TEXT", nullable: false),
                    ResolutionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SourceSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetSiteId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    ConflictDetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConflicts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncConflicts_SyncSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SyncSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Level = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Exception = table.Column<string>(type: "TEXT", nullable: true),
                    StackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    LogTimestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "TEXT", nullable: true),
                    CorrelationId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncLogs_SyncSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "SyncSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRecordConflicts",
                columns: table => new
                {
                    RelatedChangesId = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedConflictsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRecordConflicts", x => new { x.RelatedChangesId, x.RelatedConflictsId });
                    table.ForeignKey(
                        name: "FK_ChangeRecordConflicts_ChangeRecords_RelatedChangesId",
                        column: x => x.RelatedChangesId,
                        principalTable: "ChangeRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChangeRecordConflicts_SyncConflicts_RelatedConflictsId",
                        column: x => x.RelatedConflictsId,
                        principalTable: "SyncConflicts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecordConflicts_RelatedConflictsId",
                table: "ChangeRecordConflicts",
                column: "RelatedConflictsId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecords_ChangeId",
                table: "ChangeRecords",
                column: "ChangeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecords_ChangeTimestamp",
                table: "ChangeRecords",
                column: "ChangeTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecords_SequenceNumber",
                table: "ChangeRecords",
                column: "SequenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecords_SessionId",
                table: "ChangeRecords",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRecords_TableName_RecordId",
                table: "ChangeRecords",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteHealthChecks_CheckTime",
                table: "SiteHealthChecks",
                column: "CheckTime");

            migrationBuilder.CreateIndex(
                name: "IX_SiteHealthChecks_HealthCheckId",
                table: "SiteHealthChecks",
                column: "HealthCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteHealthChecks_SiteId",
                table: "SiteHealthChecks",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConfigurations_ConfigId",
                table: "SyncConfigurations",
                column: "ConfigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncConfigurations_SourceSiteId_TargetSiteId_TableName",
                table: "SyncConfigurations",
                columns: new[] { "SourceSiteId", "TargetSiteId", "TableName" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_ConflictId",
                table: "SyncConflicts",
                column: "ConflictId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_SessionId",
                table: "SyncConflicts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_Status",
                table: "SyncConflicts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SyncConflicts_TableName_RecordId",
                table: "SyncConflicts",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_Level",
                table: "SyncLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_LogId",
                table: "SyncLogs",
                column: "LogId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_LogTimestamp",
                table: "SyncLogs",
                column: "LogTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_SessionId",
                table: "SyncLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_SessionId",
                table: "SyncSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_SyncSiteId",
                table: "SyncSessions",
                column: "SyncSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_SyncSiteId1",
                table: "SyncSessions",
                column: "SyncSiteId1");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSites_SiteId",
                table: "SyncSites",
                column: "SiteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeRecordConflicts");

            migrationBuilder.DropTable(
                name: "SiteHealthChecks");

            migrationBuilder.DropTable(
                name: "SyncConfigurations");

            migrationBuilder.DropTable(
                name: "SyncLogs");

            migrationBuilder.DropTable(
                name: "ChangeRecords");

            migrationBuilder.DropTable(
                name: "SyncConflicts");

            migrationBuilder.DropTable(
                name: "SyncSessions");

            migrationBuilder.DropTable(
                name: "SyncSites");
        }
    }
}
