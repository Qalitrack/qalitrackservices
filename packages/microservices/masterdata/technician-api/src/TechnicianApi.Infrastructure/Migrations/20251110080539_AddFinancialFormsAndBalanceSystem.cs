using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnicianApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialFormsAndBalanceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ManagerId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LocationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    LocationLatitude = table.Column<double>(type: "REAL", nullable: true),
                    LocationLongitude = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technicians",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technicians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "PettyCashAdvanceForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Sum = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PreparedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DisbursedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    StorageUrl = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "AdvanceReturnForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_AdvanceReturnForms_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentBalanceSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TotalAdvancesGiven = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalReturns = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalExpensesClaimed = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalMaterialsRequisitioned = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalRefunds = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalMoneyOut = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalMoneyAccountedFor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NetBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    BalanceStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReconciled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReconciledBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ReconciliationNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsManagerApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ManagerApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ManagerApprovedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IsCfoApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    CfoApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CfoApprovedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LastCalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecommendedAction = table.Column<int>(type: "INTEGER", nullable: false),
                    RecommendedAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RecommendationMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_AssignmentBalanceSummaries_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentTechnicians",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AssignedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentTechnicians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentTechnicians_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentTechnicians_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SupportingDocuments = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Justification = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ManagerReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ManagerReviewedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ManagerComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CfoComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DisbursedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DisbursedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_Claims_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailySummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAssignments = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    PendingTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    DelayedTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    CancelledTasks = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    AlertLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWorkingMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstCheckIn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCheckOut = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TotalRequisitions = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRequisitionAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AutoGeneratedNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailySummaries_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerDiemReturnForms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProjectName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FaresOrCarExpense = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Mileage = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Meals = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Medical = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Incidentals = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ApprovalComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_PerDiemReturnForms_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalAssignments = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAssignments = table.Column<int>(type: "INTEGER", nullable: false),
                    DelayedAssignments = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    OnTimeRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    AlertLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageCompletionMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWorkingHours = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportsSubmitted = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportsApproved = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportsRejected = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportApprovalRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    TotalRequisitions = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedRequisitions = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRequisitionAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AmberThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    RedThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceMetrics_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ManagerReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ManagerReviewedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ManagerComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CfoComments = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_Refunds_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Requisitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ItemsList = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Justification = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TmReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TmReviewedBy = table.Column<string>(type: "TEXT", nullable: true),
                    TmComments = table.Column<string>(type: "TEXT", nullable: true),
                    CfoReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CfoReviewedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CfoComments = table.Column<string>(type: "TEXT", nullable: true),
                    VoucherNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_Requisitions_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AssignmentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TechnicianId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ServiceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LocationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    WorkPerformed = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    MaterialsUsed = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Observations = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CustomerFeedback = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    SignatureData = table.Column<string>(type: "TEXT", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_ServiceReports_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdvanceReturnLineItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AdvanceReturnFormId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Particular = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AmountInKsh = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceReturnLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvanceReturnLineItems_AdvanceReturnForms_AdvanceReturnFormId",
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
                name: "IX_AssignmentTechnicians_AssignmentId_TechnicianId",
                table: "AssignmentTechnicians",
                columns: new[] { "AssignmentId", "TechnicianId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentTechnicians_TechnicianId",
                table: "AssignmentTechnicians",
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
                name: "IX_Claims_Status",
                table: "Claims",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_TechnicianId",
                table: "Claims",
                column: "TechnicianId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_Name",
                table: "Technicians",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_Status",
                table: "Technicians",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvanceReturnLineItems");

            migrationBuilder.DropTable(
                name: "AssignmentBalanceSummaries");

            migrationBuilder.DropTable(
                name: "AssignmentTechnicians");

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
                name: "Technicians");
        }
    }
}
