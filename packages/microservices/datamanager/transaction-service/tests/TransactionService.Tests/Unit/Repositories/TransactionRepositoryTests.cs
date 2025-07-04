using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Infrastructure.Data;
using TransactionService.Infrastructure.Repositories;
using Xunit;

namespace TransactionService.Tests.Unit.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly TransactionDbContext _context;
    private readonly TransactionRepository _repository;

    public TransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TransactionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TransactionDbContext(options);
        _repository = new TransactionRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidTransaction_AddsToDatabase()
    {
        // Arrange
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN20241201001",
            TransactionType = TransactionType.Incoming,
            VehicleId = "VEH001",
            DriverId = "DRV001",
            SupplierId = "SUP001",
            ProductId = "PRD001",
            RouteId = "RTE001",
            WeighbridgeId = "WB001",
            OrganizationId = "ORG001"
        };

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var savedTransaction = await _context.Transactions.FindAsync(result.Id);
        savedTransaction.Should().NotBeNull();
        savedTransaction!.TransactionNumber.Should().Be("TXN20241201001");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingTransaction_ReturnsTransaction()
    {
        // Arrange
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN20241201002",
            TransactionType = TransactionType.Outgoing,
            VehicleId = "VEH002",
            DriverId = "DRV002",
            SupplierId = "SUP002",
            ProductId = "PRD002",
            RouteId = "RTE002",
            WeighbridgeId = "WB002",
            OrganizationId = "ORG002"
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(transaction.Id);
        result.TransactionNumber.Should().Be("TXN20241201002");
        result.TransactionType.Should().Be(TransactionType.Outgoing);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingTransaction_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedTransaction_ReturnsNull()
    {
        // Arrange
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN20241201003",
            TransactionType = TransactionType.Transfer,
            VehicleId = "VEH003",
            DriverId = "DRV003",
            SupplierId = "SUP003",
            ProductId = "PRD003",
            RouteId = "RTE003",
            WeighbridgeId = "WB003",
            OrganizationId = "ORG003",
            IsDeleted = true
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(transaction.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByVehicleIdAsync_ExistingVehicle_ReturnsTransactions()
    {
        // Arrange
        var vehicleId = "VEH999";
        var transactions = new[]
        {
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201004",
                VehicleId = vehicleId,
                DriverId = "DRV004",
                SupplierId = "SUP004",
                ProductId = "PRD004",
                RouteId = "RTE004",
                WeighbridgeId = "WB004",
                OrganizationId = "ORG004",
                TransactionDate = DateTime.UtcNow.AddDays(-2)
            },
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201005",
                VehicleId = vehicleId,
                DriverId = "DRV005",
                SupplierId = "SUP005",
                ProductId = "PRD005",
                RouteId = "RTE005",
                WeighbridgeId = "WB005",
                OrganizationId = "ORG005",
                TransactionDate = DateTime.UtcNow.AddDays(-1)
            }
        };

        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByVehicleIdAsync(vehicleId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.VehicleId == vehicleId);
        result.Should().BeInDescendingOrder(t => t.TransactionDate);
    }

    [Fact]
    public async Task GetByStatusAsync_ExistingStatus_ReturnsTransactions()
    {
        // Arrange
        var status = TransactionStatus.Completed;
        var transactions = new[]
        {
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201006",
                Status = status,
                VehicleId = "VEH006",
                DriverId = "DRV006",
                SupplierId = "SUP006",
                ProductId = "PRD006",
                RouteId = "RTE006",
                WeighbridgeId = "WB006",
                OrganizationId = "ORG006"
            },
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201007",
                Status = TransactionStatus.Pending,
                VehicleId = "VEH007",
                DriverId = "DRV007",
                SupplierId = "SUP007",
                ProductId = "PRD007",
                RouteId = "RTE007",
                WeighbridgeId = "WB007",
                OrganizationId = "ORG007"
            }
        };

        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(status);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(t => t.Status == status);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ValidDateRange_ReturnsTransactionsInRange()
    {
        // Arrange
        var baseDate = DateTime.Today;
        var transactions = new[]
        {
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201008",
                TransactionDate = baseDate.AddDays(-2),
                VehicleId = "VEH008",
                DriverId = "DRV008",
                SupplierId = "SUP008",
                ProductId = "PRD008",
                RouteId = "RTE008",
                WeighbridgeId = "WB008",
                OrganizationId = "ORG008"
            },
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201009",
                TransactionDate = baseDate.AddDays(-1),
                VehicleId = "VEH009",
                DriverId = "DRV009",
                SupplierId = "SUP009",
                ProductId = "PRD009",
                RouteId = "RTE009",
                WeighbridgeId = "WB009",
                OrganizationId = "ORG009"
            },
            new WeighingTransaction
            {
                TransactionNumber = "TXN20241201010",
                TransactionDate = baseDate.AddDays(-5), // Outside range
                VehicleId = "VEH010",
                DriverId = "DRV010",
                SupplierId = "SUP010",
                ProductId = "PRD010",
                RouteId = "RTE010",
                WeighbridgeId = "WB010",
                OrganizationId = "ORG010"
            }
        };

        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        var startDate = baseDate.AddDays(-3);
        var endDate = baseDate;

        // Act
        var result = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
    }

    [Fact]
    public async Task GenerateTransactionNumberAsync_CallMultipleTimes_ReturnsUniqueNumbers()
    {
        // Act
        var number1 = await _repository.GenerateTransactionNumberAsync();
        var number2 = await _repository.GenerateTransactionNumberAsync();

        // Assert
        number1.Should().NotBeEmpty();
        number2.Should().NotBeEmpty();
        number1.Should().NotBe(number2);
        
        // Should follow the pattern TXNyyyyMMdd###
        number1.Should().MatchRegex(@"^TXN\d{8}\d{3}$");
        number2.Should().MatchRegex(@"^TXN\d{8}\d{3}$");
    }

    [Fact]
    public async Task UpdateAsync_ExistingTransaction_UpdatesTimestamp()
    {
        // Arrange
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN20241201011",
            VehicleId = "VEH011",
            DriverId = "DRV011",
            SupplierId = "SUP011",
            ProductId = "PRD011",
            RouteId = "RTE011",
            WeighbridgeId = "WB011",
            OrganizationId = "ORG011",
            Status = TransactionStatus.Pending
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = transaction.UpdatedAt;
        
        // Wait a moment to ensure timestamp difference
        await Task.Delay(100);

        // Act
        transaction.Status = TransactionStatus.InProgress;
        var result = await _repository.UpdateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TransactionStatus.InProgress);
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTransaction_MarksAsDeleted()
    {
        // Arrange
        var transaction = new WeighingTransaction
        {
            TransactionNumber = "TXN20241201012",
            VehicleId = "VEH012",
            DriverId = "DRV012",
            SupplierId = "SUP012",
            ProductId = "PRD012",
            RouteId = "RTE012",
            WeighbridgeId = "WB012",
            OrganizationId = "ORG012"
        };

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsDeleted.Should().BeTrue();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify it's not returned by GetByIdAsync
        var retrievedTransaction = await _repository.GetByIdAsync(transaction.Id);
        retrievedTransaction.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}