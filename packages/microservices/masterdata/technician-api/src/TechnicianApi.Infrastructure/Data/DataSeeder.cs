using TechnicianApi.Core.Entities;

namespace TechnicianApi.Infrastructure.Data;

public class DataSeeder
{
    private readonly TechnicianApiDbContext _context;

    public DataSeeder(TechnicianApiDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (_context.Technicians.Any() || _context.Assignments.Any())
        {
            return; // Data already seeded
        }

        await SeedTechniciansAsync();
        await SeedAssignmentsAsync();
        await SeedCheckInsAsync();
        await SeedPhotosAsync();
        await SeedServiceReportsAsync();
        await SeedRequisitionsAsync();
        await SeedDailySummariesAsync();
        await SeedPerformanceMetricsAsync();
        await SeedFinancialFormsAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedTechniciansAsync()
    {
        var technicians = new List<Technician>
        {
            new Technician
            {
                Id = "tech-001",
                Name = "John Doe",
                Description = "Senior Field Technician - Network & Weighbridge Systems",
                Status = TechnicianStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            },
            new Technician
            {
                Id = "tech-002",
                Name = "Jane Smith",
                Description = "Field Technician - CCTV & Security Systems",
                Status = TechnicianStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.Technicians.AddRangeAsync(technicians);
    }

    private async Task SeedAssignmentsAsync()
    {
        // Get the technicians that were just seeded
        var tech001 = await _context.Technicians.FindAsync("tech-001");
        var tech002 = await _context.Technicians.FindAsync("tech-002");

        var assignment1 = new Assignment
        {
            Id = "assign-001",
            ManagerId = "mgr-001",
            Title = "Install Network Router",
            Description = "Install and configure new network router at client site",
            ServiceType = "Installation",
            Priority = AssignmentPriority.High,
            Status = AssignmentStatus.Completed,
            Deadline = DateTime.UtcNow.AddDays(-1),
            AcceptedAt = DateTime.UtcNow.AddDays(-2),
            StartedAt = DateTime.UtcNow.AddDays(-1).AddHours(-3),
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            LocationName = "ABC Corp HQ",
            LocationAddress = "123 Business Street, Lagos",
            LocationLatitude = 6.5244,
            LocationLongitude = 3.3792,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        if (tech001 != null) assignment1.Technicians.Add(tech001);

        var assignment2 = new Assignment
        {
            Id = "assign-002",
            ManagerId = "mgr-001",
            Title = "Repair Weighbridge Scale",
            Description = "Diagnose and repair malfunctioning weighbridge scale",
            ServiceType = "Repair",
            Priority = AssignmentPriority.Urgent,
            Status = AssignmentStatus.InProgress,
            Deadline = DateTime.UtcNow.AddHours(4),
            AcceptedAt = DateTime.UtcNow.AddHours(-2),
            StartedAt = DateTime.UtcNow.AddHours(-1),
            LocationName = "Port Harcourt Terminal",
            LocationAddress = "Terminal Road, Port Harcourt",
            LocationLatitude = 4.8156,
            LocationLongitude = 7.0498,
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        if (tech001 != null) assignment2.Technicians.Add(tech001);

        var assignment3 = new Assignment
        {
            Id = "assign-003",
            ManagerId = "mgr-001",
            Title = "Preventive Maintenance - Cameras",
            Description = "Perform routine preventive maintenance on CCTV cameras",
            ServiceType = "Maintenance",
            Priority = AssignmentPriority.Normal,
            Status = AssignmentStatus.Pending,
            Deadline = DateTime.UtcNow.AddDays(2),
            LocationName = "Industrial Estate",
            LocationAddress = "Industrial Area, Aba",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        if (tech002 != null) assignment3.Technicians.Add(tech002);

        await _context.Assignments.AddRangeAsync(new[] { assignment1, assignment2, assignment3 });
    }

    private async Task SeedCheckInsAsync()
    {
        var checkIns = new List<CheckIn>
        {
            new CheckIn
            {
                Id = "checkin-001",
                AssignmentId = "assign-001",
                Latitude = 6.5244,
                Longitude = 3.3792,
                CheckInTime = DateTime.UtcNow.AddDays(-1).AddHours(-3),
                Notes = "Arrived on site, met with facility manager",
                Status = CheckInStatus.CheckedOut,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-3)
            },
            new CheckIn
            {
                Id = "checkin-002",
                AssignmentId = "assign-002",
                Latitude = 4.8156,
                Longitude = 7.0498,
                CheckInTime = DateTime.UtcNow.AddHours(-1),
                Notes = "On site, equipment assessment in progress",
                Status = CheckInStatus.OnSite,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        await _context.CheckIns.AddRangeAsync(checkIns);
    }

    private async Task SeedPhotosAsync()
    {
        var photos = new List<Photo>
        {
            new Photo
            {
                Id = "photo-001",
                AssignmentId = "assign-001",
                Type = PhotoType.Before,
                FileName = "before_router_install.jpg",
                FilePath = "/uploads/photos/2024/before_router_install.jpg",
                StorageUrl = "https://storage.example.com/photos/before_router_install.jpg",
                FileSize = 2048000,
                ContentType = "image/jpeg",
                CapturedAt = DateTime.UtcNow.AddDays(-1).AddHours(-3),
                Latitude = 6.5244,
                Longitude = 3.3792,
                Caption = "Old router before replacement",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-3)
            },
            new Photo
            {
                Id = "photo-002",
                AssignmentId = "assign-001",
                Type = PhotoType.After,
                FileName = "after_router_install.jpg",
                FilePath = "/uploads/photos/2024/after_router_install.jpg",
                StorageUrl = "https://storage.example.com/photos/after_router_install.jpg",
                FileSize = 1950000,
                ContentType = "image/jpeg",
                CapturedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1),
                Latitude = 6.5244,
                Longitude = 3.3792,
                Caption = "New router installed and configured",
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1)
            },
            new Photo
            {
                Id = "photo-003",
                AssignmentId = "assign-002",
                Type = PhotoType.Before,
                FileName = "scale_issue.jpg",
                FilePath = "/uploads/photos/2024/scale_issue.jpg",
                StorageUrl = "https://storage.example.com/photos/scale_issue.jpg",
                FileSize = 1800000,
                ContentType = "image/jpeg",
                CapturedAt = DateTime.UtcNow.AddHours(-1),
                Latitude = 4.8156,
                Longitude = 7.0498,
                Caption = "Weighbridge scale malfunction",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        await _context.Photos.AddRangeAsync(photos);
    }

    private async Task SeedServiceReportsAsync()
    {
        var reports = new List<ServiceReport>
        {
            new ServiceReport
            {
                Id = "report-001",
                AssignmentId = "assign-001",
                TechnicianId = "tech-001",
                TechnicianName = "John Doe",
                CustomerName = "ABC Corp",
                LocationName = "ABC Corp HQ",
                LocationAddress = "123 Business Street, Lagos",
                ContactPerson = "Michael Johnson",
                Designation = "Facility Manager",
                MobileNumber = "+234-123-456-7890",
                Email = "michael.johnson@abccorp.com",
                VehicleNo = "LAG-5678",
                NatureOfVisit = NatureOfVisit.PlannedMaintenance,
                MachineDetails = "Cisco XR-500 Router",
                FaultReported = "Intermittent network connectivity issues",
                Findings = "Old router firmware causing stability issues",
                Correction = "Installed new router, updated firmware, reconfigured network settings",
                FinalResult = "Network connectivity fully restored. All systems operational.",
                PartsOffered = "1x Cisco XR-500 Router, 2x Cat6 Ethernet cables, 1x Power adapter",
                CustomerComments = "Excellent service. Network is much faster now.",
                StartDay = DateTime.UtcNow.AddDays(-1).Date,
                EndDay = DateTime.UtcNow.AddDays(-1).Date,
                TotalFieldJobMinutes = 120,
                SignatureData = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgA...",
                SignedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1),
                Status = ServiceReportStatus.Approved,
                SubmittedAt = DateTime.UtcNow.AddDays(-1),
                ApprovedAt = DateTime.UtcNow.AddHours(-6),
                ApprovedBy = "mgr-001",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        await _context.ServiceReports.AddRangeAsync(reports);
    }

    private async Task SeedRequisitionsAsync()
    {
        var requisitions = new List<Requisition>
        {
            new Requisition
            {
                Id = "req-001",
                AssignmentId = "assign-001",
                TechnicianId = "tech-001",
                Type = RequisitionType.MaterialRequisition,
                Description = "Network cables and accessories for router installation",
                Amount = 15000.00m,
                ItemsList = "2x Cat6 cables (5m), 1x Cable tester, 1x RJ45 connectors pack",
                Justification = "Required for professional router installation at ABC Corp",
                Status = RequisitionStatus.Paid,
                TmReviewedAt = DateTime.UtcNow.AddDays(-1).AddHours(-6),
                TmReviewedBy = "mgr-001",
                TmComments = "Approved. Standard materials for this type of installation.",
                CfoReviewedAt = DateTime.UtcNow.AddDays(-1).AddHours(-4),
                CfoReviewedBy = "cfo-001",
                CfoComments = "Payment processed",
                VoucherNumber = "VCH-2024-001",
                ReferenceNumber = "REF-ABC-001",
                PaidAt = DateTime.UtcNow.AddDays(-1).AddHours(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Requisition
            {
                Id = "req-002",
                AssignmentId = "assign-002",
                TechnicianId = "tech-001",
                Type = RequisitionType.CashAdvance,
                Description = "Cash advance for weighbridge scale repair parts",
                Amount = 50000.00m,
                Justification = "Urgent repair requires immediate parts purchase from supplier",
                Status = RequisitionStatus.TmApproved,
                TmReviewedAt = DateTime.UtcNow.AddHours(-1),
                TmReviewedBy = "mgr-001",
                TmComments = "Approved for urgent repair. Forward to CFO for processing.",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        await _context.Requisitions.AddRangeAsync(requisitions);
    }

    private async Task SeedDailySummariesAsync()
    {
        var summaries = new List<DailySummary>
        {
            new DailySummary
            {
                Id = "summary-001",
                TechnicianId = "tech-001",
                Date = DateTime.UtcNow.Date.AddDays(-1),
                TotalAssignments = 3,
                CompletedTasks = 2,
                PendingTasks = 1,
                DelayedTasks = 0,
                CancelledTasks = 0,
                PerformanceScore = 95.5m,
                AlertLevel = PerformanceAlertLevel.None,
                TotalWorkingMinutes = 420,
                FirstCheckIn = DateTime.UtcNow.AddDays(-1).AddHours(-8),
                LastCheckOut = DateTime.UtcNow.AddDays(-1).AddHours(-1),
                TotalRequisitions = 2,
                TotalRequisitionAmount = 65000.00m,
                AutoGeneratedNotes = "Excellent performance. Completed router installation and started weighbridge repair.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new DailySummary
            {
                Id = "summary-002",
                TechnicianId = "tech-002",
                Date = DateTime.UtcNow.Date.AddDays(-1),
                TotalAssignments = 1,
                CompletedTasks = 0,
                PendingTasks = 1,
                DelayedTasks = 0,
                CancelledTasks = 0,
                PerformanceScore = 80.0m,
                AlertLevel = PerformanceAlertLevel.None,
                TotalWorkingMinutes = 0,
                TotalRequisitions = 0,
                TotalRequisitionAmount = 0.00m,
                AutoGeneratedNotes = "Assignment pending acceptance for tomorrow.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        await _context.DailySummaries.AddRangeAsync(summaries);
    }

    private async Task SeedPerformanceMetricsAsync()
    {
        var metrics = new List<PerformanceMetrics>
        {
            new PerformanceMetrics
            {
                Id = "metrics-001",
                TechnicianId = "tech-001",
                PeriodStart = DateTime.UtcNow.Date.AddDays(-30),
                PeriodEnd = DateTime.UtcNow.Date,
                TotalAssignments = 25,
                CompletedAssignments = 23,
                DelayedAssignments = 2,
                CompletionRate = 92.0m,
                OnTimeRate = 88.0m,
                PerformanceScore = 90.5m,
                AlertLevel = PerformanceAlertLevel.None,
                AverageCompletionMinutes = 180,
                TotalWorkingHours = 160,
                ReportsSubmitted = 23,
                ReportsApproved = 22,
                ReportsRejected = 1,
                ReportApprovalRate = 95.65m,
                TotalRequisitions = 18,
                ApprovedRequisitions = 17,
                TotalRequisitionAmount = 450000.00m,
                AmberThreshold = 2,
                RedThreshold = 4,
                CalculatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new PerformanceMetrics
            {
                Id = "metrics-002",
                TechnicianId = "tech-002",
                PeriodStart = DateTime.UtcNow.Date.AddDays(-30),
                PeriodEnd = DateTime.UtcNow.Date,
                TotalAssignments = 20,
                CompletedAssignments = 18,
                DelayedAssignments = 4,
                CompletionRate = 90.0m,
                OnTimeRate = 70.0m,
                PerformanceScore = 75.0m,
                AlertLevel = PerformanceAlertLevel.Red,
                AverageCompletionMinutes = 210,
                TotalWorkingHours = 140,
                ReportsSubmitted = 18,
                ReportsApproved = 17,
                ReportsRejected = 1,
                ReportApprovalRate = 94.44m,
                TotalRequisitions = 12,
                ApprovedRequisitions = 12,
                TotalRequisitionAmount = 280000.00m,
                AmberThreshold = 2,
                RedThreshold = 4,
                CalculatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.PerformanceMetrics.AddRangeAsync(metrics);
    }

    private async Task SeedFinancialFormsAsync()
    {
        // Seed Petty Cash Advance Forms
        var pettyCashForms = new List<PettyCashAdvanceForm>
        {
            new PettyCashAdvanceForm
            {
                Id = "petty-001",
                AssignmentId = "assign-001",
                Sum = 10000.00m,
                Description = "Petty cash advance for router installation supplies",
                PreparedBy = "John Doe",
                Status = PettyCashStatus.Disbursed,
                ApprovedAt = DateTime.UtcNow.AddDays(-2),
                ApprovedBy = "mgr-001",
                ApprovalComments = "Approved for installation project",
                DisbursedAt = DateTime.UtcNow.AddDays(-2).AddHours(2),
                DisbursedBy = "cfo-001",
                VoucherNumber = "PCA-2024-001",
                ReferenceNumber = "REF-PCA-001",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new PettyCashAdvanceForm
            {
                Id = "petty-002",
                AssignmentId = "assign-002",
                Sum = 5000.00m,
                Description = "Petty cash for transport and small parts",
                PreparedBy = "John Doe",
                Status = PettyCashStatus.Disbursed,
                ApprovedAt = DateTime.UtcNow.AddHours(-3),
                ApprovedBy = "mgr-001",
                ApprovalComments = "Approved for urgent weighbridge repair",
                DisbursedAt = DateTime.UtcNow.AddHours(-2),
                DisbursedBy = "cfo-001",
                VoucherNumber = "PCA-2024-002",
                ReferenceNumber = "REF-PCA-002",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            }
        };
        await _context.PettyCashAdvanceForms.AddRangeAsync(pettyCashForms);

        // Seed Advance Return Forms (materials returned by technician)
        var advanceReturnForm = new AdvanceReturnForm
        {
            Id = "advret-001",
            AssignmentId = "assign-001",
            TechnicianId = "tech-001",
            TotalAmount = 3500.00m,
            Status = AdvanceReturnStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddDays(-1),
            ApprovedBy = "mgr-001",
            ApprovalComments = "Materials accounted for correctly",
            CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
        };
        await _context.AdvanceReturnForms.AddAsync(advanceReturnForm);

        var advanceReturnLineItems = new List<AdvanceReturnLineItem>
        {
            new AdvanceReturnLineItem
            {
                Id = "lineitem-001",
                AdvanceReturnFormId = "advret-001",
                Particular = "Cat6 Cable (5m)",
                AmountInKsh = 1500.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            },
            new AdvanceReturnLineItem
            {
                Id = "lineitem-002",
                AdvanceReturnFormId = "advret-001",
                Particular = "RJ45 Connectors Pack",
                AmountInKsh = 800.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            },
            new AdvanceReturnLineItem
            {
                Id = "lineitem-003",
                AdvanceReturnFormId = "advret-001",
                Particular = "Cable Tester",
                AmountInKsh = 1200.00m,
                CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-2)
            }
        };
        await _context.AdvanceReturnLineItems.AddRangeAsync(advanceReturnLineItems);

        // Seed Per Diem Return Forms
        var perDiemForm = new PerDiemReturnForm
        {
            Id = "perdiem-001",
            AssignmentId = "assign-001",
            TechnicianId = "tech-001",
            ProjectName = "Install Network Router - ABC Corp HQ",
            FaresOrCarExpense = 2500.00m,
            Mileage = 1500.00m,
            Meals = 1000.00m,
            Medical = 0.00m,
            Incidentals = 500.00m,
            TotalAmount = 5500.00m,
            Status = PerDiemReturnStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddDays(-1),
            ApprovedBy = "mgr-001",
            ApprovalComments = "Expenses are reasonable for this assignment",
            CreatedAt = DateTime.UtcNow.AddDays(-1).AddHours(-1)
        };
        await _context.PerDiemReturnForms.AddAsync(perDiemForm);

        // Seed Claims (technician requesting money from company)
        var claim = new Claim
        {
            Id = "claim-001",
            AssignmentId = "assign-001",
            TechnicianId = "tech-001",
            TechnicianName = "John Doe",
            Description = "Additional equipment purchased for router installation",
            Amount = 2500.00m,
            SupportingDocuments = "/uploads/receipts/claim-001-receipt.pdf",
            Justification = "Client requested additional network patch cables not in original requisition",
            Status = ClaimStatus.Disbursed,
            ManagerReviewedAt = DateTime.UtcNow.AddHours(-12),
            ManagerReviewedBy = "mgr-001",
            ManagerComments = "Valid additional expense. Approved for CFO processing.",
            CfoReviewedAt = DateTime.UtcNow.AddHours(-6),
            CfoReviewedBy = "cfo-001",
            CfoComments = "Approved for disbursement",
            DisbursedAt = DateTime.UtcNow.AddHours(-2),
            DisbursedBy = "cfo-001",
            VoucherNumber = "CLM-2024-001",
            ReferenceNumber = "REF-CLM-001",
            PaymentMethod = "Bank Transfer",
            CreatedAt = DateTime.UtcNow.AddHours(-18)
        };
        await _context.Claims.AddAsync(claim);

        // Seed Refunds (technician returning money to company)
        var refund = new Refund
        {
            Id = "refund-001",
            AssignmentId = "assign-002",
            TechnicianId = "tech-001",
            TechnicianName = "John Doe",
            Amount = 1000.00m,
            Description = "Unused petty cash from weighbridge repair",
            PaymentMethod = "Cash",
            ReceiptNumber = "RCP-2024-001",
            ReferenceNumber = "REF-RFD-001",
            Status = RefundStatus.CfoReceived,
            ManagerReviewedAt = DateTime.UtcNow.AddHours(-4),
            ManagerReviewedBy = "mgr-001",
            ManagerComments = "Verified refund amount",
            CfoReviewedAt = DateTime.UtcNow.AddHours(-2),
            CfoReviewedBy = "cfo-001",
            CfoComments = "Cash received and recorded",
            ReceivedAt = DateTime.UtcNow.AddHours(-2),
            ReceivedBy = "cfo-001",
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        };
        await _context.Refunds.AddAsync(refund);

        // Seed Assignment Balance Summary (auto-calculated for assign-001)
        var balanceSummary = new AssignmentBalanceSummary
        {
            Id = "balance-001",
            AssignmentId = "assign-001",
            TechnicianId = "tech-001",

            // Money OUT: Petty Cash (10000) + Claim Disbursed (2500) = 12500
            TotalAdvancesGiven = 12500.00m,

            // Money ACCOUNTED: Advance Returns (3500) + Per Diem (5500) + Materials Req (15000) = 24000
            TotalReturns = 3500.00m,
            TotalExpensesClaimed = 5500.00m,
            TotalMaterialsRequisitioned = 15000.00m,
            TotalRefunds = 0.00m,

            // Calculated
            TotalMoneyOut = 12500.00m,
            TotalMoneyAccountedFor = 24000.00m,
            NetBalance = -11500.00m, // Company owes technician 11,500
            BalanceStatus = BalanceStatus.CompanyOwes,

            // Recommendations
            RecommendedAction = RecommendedAction.CreateClaim,
            RecommendedAmount = 11500.00m,
            RecommendationMessage = "Company owes technician KSH 11,500.00. Technician can create a Claim.",

            LastCalculatedAt = DateTime.UtcNow,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        await _context.AssignmentBalanceSummaries.AddAsync(balanceSummary);

        // Seed Balance Summary for assign-002 (technician owes company)
        var balanceSummary2 = new AssignmentBalanceSummary
        {
            Id = "balance-002",
            AssignmentId = "assign-002",
            TechnicianId = "tech-001",

            // Money OUT: Petty Cash (5000) + Cash Advance Req (50000, but pending) = 5000
            TotalAdvancesGiven = 5000.00m,

            // Money ACCOUNTED: Refund (1000)
            TotalReturns = 0.00m,
            TotalExpensesClaimed = 0.00m,
            TotalMaterialsRequisitioned = 0.00m,
            TotalRefunds = 1000.00m,

            // Calculated
            TotalMoneyOut = 5000.00m,
            TotalMoneyAccountedFor = 1000.00m,
            NetBalance = 4000.00m, // Technician owes company 4,000
            BalanceStatus = BalanceStatus.TechnicianOwes,

            // Recommendations
            RecommendedAction = RecommendedAction.CreateRefund,
            RecommendedAmount = 4000.00m,
            RecommendationMessage = "Technician owes KSH 4,000.00. Please create a Refund form.",

            LastCalculatedAt = DateTime.UtcNow,
            IsReconciled = false,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        await _context.AssignmentBalanceSummaries.AddAsync(balanceSummary2);
    }
}
