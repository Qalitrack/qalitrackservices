using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Tests.Helpers;

public static class TestDataHelper
{
    public static CreateTransactionRequest CreateValidTransactionRequest(
        TransactionType transactionType = TransactionType.Incoming)
    {
        return new CreateTransactionRequest
        {
            TransactionType = transactionType,
            VehicleId = "TEST_VEH_001",
            DriverId = "TEST_DRV_001",
            SupplierId = "TEST_SUP_001",
            CustomerId = transactionType == TransactionType.Outgoing ? "TEST_CUST_001" : null,
            ProductId = "TEST_PRD_001",
            RouteId = "TEST_RTE_001",
            WeighbridgeId = "TEST_WB_001",
            OrganizationId = "TEST_ORG_001",
            DeliveryNoteNumber = "TEST_DN_001",
            PermitNumber = "TEST_PM_001",
            Remarks = "Test transaction for unit testing"
        };
    }

    public static WeighingTransaction CreateTestTransaction(
        TransactionStatus status = TransactionStatus.Pending)
    {
        return new WeighingTransaction
        {
            Id = Guid.NewGuid().ToString(),
            TransactionNumber = GenerateTransactionNumber(),
            TransactionType = TransactionType.Incoming,
            Status = status,
            TransactionDate = DateTime.UtcNow,
            VehicleId = "TEST_VEH_001",
            DriverId = "TEST_DRV_001",
            SupplierId = "TEST_SUP_001",
            ProductId = "TEST_PRD_001",
            RouteId = "TEST_RTE_001",
            WeighbridgeId = "TEST_WB_001",
            OrganizationId = "TEST_ORG_001",
            DeliveryNoteNumber = "TEST_DN_001",
            PermitNumber = "TEST_PM_001",
            Remarks = "Test transaction",
            GrossWeight = 1000.50m,
            TareWeight = 200.25m,
            NetWeight = 800.25m,
            EntryWeighingTime = DateTime.UtcNow.AddHours(-2),
            ExitWeighingTime = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public static TransactionWorkflow CreateTestWorkflowStep(
        string transactionId,
        WorkflowStep step = WorkflowStep.VehicleArrival,
        StepStatus status = StepStatus.NotStarted,
        int order = 1)
    {
        return new TransactionWorkflow
        {
            Id = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            WorkflowStep = step,
            Status = status,
            Order = order,
            StartedAt = status >= StepStatus.InProgress ? DateTime.UtcNow.AddMinutes(-30) : null,
            CompletedAt = status == StepStatus.Completed ? DateTime.UtcNow.AddMinutes(-10) : null,
            ProcessedBy = status >= StepStatus.InProgress ? "TEST_USER" : null,
            Notes = $"Test notes for {step}",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
    }

    public static TransactionCharge CreateTestCharge(
        string transactionId,
        ChargeType chargeType = ChargeType.BaseCharge,
        decimal amount = 100.00m)
    {
        return new TransactionCharge
        {
            Id = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            ChargeType = chargeType,
            Description = $"Test {chargeType} charge",
            Amount = amount,
            TaxRate = 0.15m,
            TaxAmount = amount * 0.15m,
            TotalAmount = amount * 1.15m,
            Currency = "USD",
            IsApproved = false,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
    }

    public static TransactionDocument CreateTestDocument(
        string transactionId,
        string documentType = "DeliveryNote")
    {
        return new TransactionDocument
        {
            Id = Guid.NewGuid().ToString(),
            TransactionId = transactionId,
            FileName = "test_document.pdf",
            OriginalFileName = "Original Test Document.pdf",
            ContentType = "application/pdf",
            FileSize = 1024 * 1024, // 1MB
            FilePath = "/uploads/test_document.pdf",
            DocumentType = documentType,
            Description = "Test document for unit testing",
            Version = 1,
            IsActive = true,
            ChecksumMd5 = "d41d8cd98f00b204e9800998ecf8427e",
            ChecksumSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            CreatedBy = "TEST_USER"
        };
    }

    public static CreateChargeRequest CreateValidChargeRequest(
        string transactionId,
        ChargeType chargeType = ChargeType.ProcessingFee,
        decimal amount = 50.00m)
    {
        return new CreateChargeRequest
        {
            TransactionId = transactionId,
            ChargeType = chargeType,
            Description = $"Test {chargeType} charge",
            Amount = amount,
            TaxRate = 0.15m,
            Currency = "USD"
        };
    }

    public static UpdateTransactionRequest CreateValidUpdateRequest()
    {
        return new UpdateTransactionRequest
        {
            Status = TransactionStatus.InProgress,
            GrossWeight = 1200.75m,
            TareWeight = 250.50m,
            NetWeight = 950.25m,
            EntryWeighingTime = DateTime.UtcNow.AddHours(-1),
            ExitWeighingTime = DateTime.UtcNow.AddMinutes(-30),
            DeliveryNoteNumber = "UPDATED_DN_001",
            PermitNumber = "UPDATED_PM_001",
            Remarks = "Updated transaction for testing",
            Metadata = new Dictionary<string, object>
            {
                ["updated"] = true,
                ["updateTime"] = DateTime.UtcNow.ToString("O")
            }
        };
    }

    public static List<WeighingTransaction> CreateTestTransactionList(int count = 5)
    {
        var transactions = new List<WeighingTransaction>();
        
        for (int i = 0; i < count; i++)
        {
            var transaction = CreateTestTransaction();
            transaction.TransactionNumber = $"TXN{DateTime.Today:yyyyMMdd}{i + 1:D3}";
            transaction.TransactionDate = DateTime.Today.AddDays(-i);
            transaction.VehicleId = $"VEH{i + 1:D3}";
            transactions.Add(transaction);
        }
        
        return transactions;
    }

    private static string GenerateTransactionNumber()
    {
        var today = DateTime.Today;
        var random = new Random();
        var sequence = random.Next(1, 999);
        return $"TXN{today:yyyyMMdd}{sequence:D3}";
    }
}