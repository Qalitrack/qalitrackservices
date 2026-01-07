using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnicianApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technician",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technician", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ManagerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LocationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LocationLatitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationLongitude = table.Column<double>(type: "double precision", nullable: true),
                    TechnicianIds = table.Column<string>(type: "text", nullable: false),
                    TechnicianId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DailySummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAssignments = table.Column<int>(type: "integer", nullable: false),
                    CompletedTasks = table.Column<int>(type: "integer", nullable: false),
                    PendingTasks = table.Column<int>(type: "integer", nullable: false),
                    DelayedTasks = table.Column<int>(type: "integer", nullable: false),
                    CancelledTasks = table.Column<int>(type: "integer", nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AlertLevel = table.Column<int>(type: "integer", nullable: false),
                    TotalWorkingMinutes = table.Column<int>(type: "integer", nullable: false),
                    FirstCheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalRequisitions = table.Column<int>(type: "integer", nullable: false),
                    TotalRequisitionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AutoGeneratedNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AttachmentId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySummaries_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DailySummaries_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAssignments = table.Column<int>(type: "integer", nullable: false),
                    CompletedAssignments = table.Column<int>(type: "integer", nullable: false),
                    DelayedAssignments = table.Column<int>(type: "integer", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OnTimeRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AlertLevel = table.Column<int>(type: "integer", nullable: false),
                    AverageCompletionMinutes = table.Column<int>(type: "integer", nullable: false),
                    TotalWorkingHours = table.Column<int>(type: "integer", nullable: false),
                    ReportsSubmitted = table.Column<int>(type: "integer", nullable: false),
                    ReportsApproved = table.Column<int>(type: "integer", nullable: false),
                    ReportsRejected = table.Column<int>(type: "integer", nullable: false),
                    ReportApprovalRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalRequisitions = table.Column<int>(type: "integer", nullable: false),
                    ApprovedRequisitions = table.Column<int>(type: "integer", nullable: false),
                    TotalRequisitionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmberThreshold = table.Column<int>(type: "integer", nullable: false),
                    RedThreshold = table.Column<int>(type: "integer", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvanceReturnForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AttachmentId = table.Column<string>(type: "text", nullable: true),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceReturnForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceReturnForms_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvanceReturnForms_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdvanceReturnForms_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentBalanceSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAdvancesGiven = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalReturns = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalExpensesClaimed = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalMaterialsRequisitioned = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalRefunds = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalMoneyOut = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalMoneyAccountedFor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceStatus = table.Column<int>(type: "integer", nullable: false),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReconciledBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReconciliationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsManagerApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ManagerApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsCfoApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CfoApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CfoApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecommendedAction = table.Column<int>(type: "integer", nullable: false),
                    RecommendedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RecommendationMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentBalanceSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentBalanceSummaries_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentBalanceSummaries_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentTechnician",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "text", nullable: false),
                    TechnicianId = table.Column<string>(type: "text", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTechnician", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentTechnician_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentTechnician_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckIns_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SupportingDocuments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ManagerReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ManagerComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CfoComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisbursedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VoucherNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttachmentId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Claims_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Claims_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerDiemReturnForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FaresOrCarExpense = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Mileage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Meals = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Medical = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Incidentals = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttachmentId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerDiemReturnForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerDiemReturnForms_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerDiemReturnForms_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PerDiemReturnForms_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PettyCashAdvanceForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sum = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreparedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisbursedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VoucherNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashAdvanceForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashAdvanceForms_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StorageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ManagerReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ManagerReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ManagerComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CfoComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AttachmentId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Refunds_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Refunds_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requisitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemsList = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TmReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TmReviewedBy = table.Column<string>(type: "text", nullable: true),
                    TmComments = table.Column<string>(type: "text", nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "text", nullable: true),
                    CfoComments = table.Column<string>(type: "text", nullable: true),
                    VoucherNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requisitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requisitions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requisitions_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AssignmentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TechnicianName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LocationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MobileNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VehicleNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NatureOfVisit = table.Column<int>(type: "integer", nullable: false),
                    MachineDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FaultReported = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Findings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Correction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FinalResult = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PartsOffered = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CustomerComments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartDay = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDay = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalFieldJobMinutes = table.Column<int>(type: "integer", nullable: false),
                    SignatureData = table.Column<string>(type: "text", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceReports_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceReports_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvanceReturnLineItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AdvanceReturnFormId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Particular = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AmountInKsh = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceReturnLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceReturnLineItems_AdvanceReturnForms_AdvanceReturnForm~",
                        column: x => x.AdvanceReturnFormId,
                        principalTable: "AdvanceReturnForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceReturnForms_AssignmentId",
                table: "AdvanceReturnForms",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceReturnForms_AttachmentId",
                table: "AdvanceReturnForms",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceReturnForms_Status",
                table: "AdvanceReturnForms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceReturnForms_TechnicianId",
                table: "AdvanceReturnForms",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceReturnLineItems_AdvanceReturnFormId",
                table: "AdvanceReturnLineItems",
                column: "AdvanceReturnFormId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentBalanceSummaries_AssignmentId",
                table: "AssignmentBalanceSummaries",
                column: "AssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentBalanceSummaries_BalanceStatus",
                table: "AssignmentBalanceSummaries",
                column: "BalanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentBalanceSummaries_IsReconciled",
                table: "AssignmentBalanceSummaries",
                column: "IsReconciled");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentBalanceSummaries_RecommendedAction",
                table: "AssignmentBalanceSummaries",
                column: "RecommendedAction");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentBalanceSummaries_TechnicianId",
                table: "AssignmentBalanceSummaries",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Deadline",
                table: "Assignments",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ManagerId",
                table: "Assignments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Status",
                table: "Assignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TechnicianId",
                table: "Assignments",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTechnician_AssignmentId",
                table: "AssignmentTechnician",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTechnician_TechnicianId",
                table: "AssignmentTechnician",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_AssignmentId",
                table: "CheckIns",
                column: "AssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_CheckInTime",
                table: "CheckIns",
                column: "CheckInTime");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AssignmentId",
                table: "Claims",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AttachmentId",
                table: "Claims",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status",
                table: "Claims",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_TechnicianId",
                table: "Claims",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaries_AttachmentId",
                table: "DailySummaries",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaries_Date",
                table: "DailySummaries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaries_TechnicianId",
                table: "DailySummaries",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySummaries_TechnicianId_Date",
                table: "DailySummaries",
                columns: new[] { "TechnicianId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerDiemReturnForms_AssignmentId",
                table: "PerDiemReturnForms",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerDiemReturnForms_AttachmentId",
                table: "PerDiemReturnForms",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PerDiemReturnForms_Status",
                table: "PerDiemReturnForms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PerDiemReturnForms_TechnicianId",
                table: "PerDiemReturnForms",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_PeriodEnd",
                table: "PerformanceMetrics",
                column: "PeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_PeriodStart",
                table: "PerformanceMetrics",
                column: "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_TechnicianId",
                table: "PerformanceMetrics",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAdvanceForms_AssignmentId",
                table: "PettyCashAdvanceForms",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAdvanceForms_PreparedBy",
                table: "PettyCashAdvanceForms",
                column: "PreparedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAdvanceForms_Status",
                table: "PettyCashAdvanceForms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AssignmentId",
                table: "Photos",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CapturedAt",
                table: "Photos",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_Type",
                table: "Photos",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_AssignmentId",
                table: "Refunds",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_AttachmentId",
                table: "Refunds",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_Status",
                table: "Refunds",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_TechnicianId",
                table: "Refunds",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_AssignmentId",
                table: "Requisitions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_Status",
                table: "Requisitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_TechnicianId",
                table: "Requisitions",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_Requisitions_Type",
                table: "Requisitions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReports_AssignmentId",
                table: "ServiceReports",
                column: "AssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReports_NatureOfVisit",
                table: "ServiceReports",
                column: "NatureOfVisit");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReports_Status",
                table: "ServiceReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReports_SubmittedAt",
                table: "ServiceReports",
                column: "SubmittedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReports_TechnicianId",
                table: "ServiceReports",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceReturnLineItems");

            migrationBuilder.DropTable(
                name: "AssignmentBalanceSummaries");

            migrationBuilder.DropTable(
                name: "AssignmentTechnician");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "DailySummaries");

            migrationBuilder.DropTable(
                name: "PerDiemReturnForms");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "PettyCashAdvanceForms");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "Requisitions");

            migrationBuilder.DropTable(
                name: "ServiceReports");

            migrationBuilder.DropTable(
                name: "AdvanceReturnForms");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Technician");
        }
    }
}
