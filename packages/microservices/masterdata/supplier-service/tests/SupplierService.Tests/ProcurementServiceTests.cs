using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Core.Mappings;
using SupplierService.Core.Services;
using SupplierService.Infrastructure.Data;
using SupplierService.Infrastructure.Repositories;
using Xunit;

namespace SupplierService.Tests;

public class ProcurementServiceTests : IDisposable
{
    private readonly SupplierDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProcurementRepository _procurementRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly ProcurementService _procurementService;

    public ProcurementServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SupplierProfile>());
        _mapper = config.CreateMapper();

        // Setup repositories and service
        _procurementRepository = new ProcurementRepository(_context);
        _supplierRepository = new SupplierRepository(_context);
        _procurementService = new ProcurementService(_procurementRepository, _supplierRepository, _mapper);
    }

    [Fact]
    public async Task CreateProcurement_ShouldCreateProcurementSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var request = new CreateProcurementRequest
        {
            SupplierId = supplier.Id,
            Title = "Test Procurement",
            Description = "Test procurement description",
            ProcurementType = ProcurementType.Goods,
            RequiredDate = DateTime.UtcNow.AddDays(30),
            EstimatedValue = 5000m,
            Currency = "USD",
            RequestedBy = "Test User",
            Priority = ProcurementPriority.Medium,
            Items = new List<CreateProcurementItemRequest>
            {
                new CreateProcurementItemRequest
                {
                    ProductCode = "PROD-001",
                    ProductName = "Test Product",
                    Quantity = 10,
                    Unit = "piece",
                    UnitPrice = 100m
                }
            }
        };

        // Act
        var result = await _procurementService.CreateProcurementAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.SupplierId, result.SupplierId);
        Assert.Equal(ProcurementStatus.Draft, result.Status);
        Assert.NotEmpty(result.ProcurementNumber);
        Assert.Equal(1000m, result.EstimatedValue); // 10 * 100
    }

    [Fact]
    public async Task CreateProcurement_WithInvalidSupplier_ShouldThrowException()
    {
        // Arrange
        var request = new CreateProcurementRequest
        {
            SupplierId = "invalid-supplier-id",
            Title = "Test Procurement",
            RequestedBy = "Test User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _procurementService.CreateProcurementAsync(request));
    }

    [Fact]
    public async Task GetProcurementsBySupplier_ShouldReturnCorrectProcurements()
    {
        // Arrange
        var supplier1 = await CreateTestSupplier("Supplier 1");
        var supplier2 = await CreateTestSupplier("Supplier 2");
        
        await CreateTestProcurement(supplier1.Id, "Procurement 1");
        await CreateTestProcurement(supplier1.Id, "Procurement 2");
        await CreateTestProcurement(supplier2.Id, "Procurement 3");

        // Act
        var result = await _procurementService.GetProcurementsBySupplierAsync(supplier1.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(supplier1.Id, p.SupplierId));
    }

    [Fact]
    public async Task ApproveProcurement_ShouldUpdateStatusSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var procurement = await CreateTestProcurement(supplier.Id, "Test Procurement");
        
        // First submit the procurement
        await _procurementService.UpdateProcurementAsync(procurement.Id, new UpdateProcurementRequest
        {
            Title = procurement.Title,
            Description = procurement.Description,
            ProcurementType = procurement.ProcurementType,
            Status = ProcurementStatus.Submitted,
            Priority = procurement.Priority
        });

        // Act
        var result = await _procurementService.ApproveProcurementAsync(procurement.Id, "Test Approver");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProcurementStatus.Approved, result.Status);
        Assert.Equal("Test Approver", result.ApprovedBy);
        Assert.NotNull(result.ApprovalDate);
    }

    [Fact]
    public async Task RejectProcurement_ShouldUpdateStatusSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var procurement = await CreateTestProcurement(supplier.Id, "Test Procurement");
        
        // First submit the procurement
        await _procurementService.UpdateProcurementAsync(procurement.Id, new UpdateProcurementRequest
        {
            Title = procurement.Title,
            Description = procurement.Description,
            ProcurementType = procurement.ProcurementType,
            Status = ProcurementStatus.Submitted,
            Priority = procurement.Priority
        });

        // Act
        var result = await _procurementService.RejectProcurementAsync(procurement.Id, "Test Rejector", "Budget constraints");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProcurementStatus.Rejected, result.Status);
        Assert.Equal("Test Rejector", result.ApprovedBy);
        Assert.Contains("Budget constraints", result.Notes);
    }

    [Fact]
    public async Task MarkAsOrdered_ShouldUpdateStatusSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var procurement = await CreateTestProcurement(supplier.Id, "Test Procurement");
        
        // First approve the procurement
        await _procurementService.UpdateProcurementAsync(procurement.Id, new UpdateProcurementRequest
        {
            Title = procurement.Title,
            Description = procurement.Description,
            ProcurementType = procurement.ProcurementType,
            Status = ProcurementStatus.Approved,
            Priority = procurement.Priority
        });

        // Act
        var result = await _procurementService.MarkAsOrderedAsync(procurement.Id, "PO-2024-001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProcurementStatus.Ordered, result.Status);
        Assert.Equal("PO-2024-001", result.PurchaseOrderNumber);
        Assert.NotNull(result.OrderDate);
    }

    [Fact]
    public async Task MarkAsDelivered_ShouldUpdateStatusSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var procurement = await CreateTestProcurement(supplier.Id, "Test Procurement");
        
        // First mark as ordered
        await _procurementService.UpdateProcurementAsync(procurement.Id, new UpdateProcurementRequest
        {
            Title = procurement.Title,
            Description = procurement.Description,
            ProcurementType = procurement.ProcurementType,
            Status = ProcurementStatus.Ordered,
            Priority = procurement.Priority
        });

        var deliveryDate = DateTime.UtcNow;

        // Act
        var result = await _procurementService.MarkAsDeliveredAsync(procurement.Id, deliveryDate, 4500m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProcurementStatus.Delivered, result.Status);
        Assert.Equal(deliveryDate, result.ActualDeliveryDate);
        Assert.Equal(4500m, result.ActualValue);
    }

    [Fact]
    public async Task GetPendingApprovals_ShouldReturnCorrectProcurements()
    {
        // Arrange
        var supplier = await CreateTestSupplier();
        var procurement1 = await CreateTestProcurement(supplier.Id, "Pending 1");
        var procurement2 = await CreateTestProcurement(supplier.Id, "Pending 2");
        var procurement3 = await CreateTestProcurement(supplier.Id, "Approved");

        // Set statuses
        await _procurementService.UpdateProcurementAsync(procurement1.Id, new UpdateProcurementRequest
        {
            Title = procurement1.Title,
            Description = procurement1.Description,
            ProcurementType = procurement1.ProcurementType,
            Status = ProcurementStatus.Submitted,
            Priority = procurement1.Priority
        });

        await _procurementService.UpdateProcurementAsync(procurement2.Id, new UpdateProcurementRequest
        {
            Title = procurement2.Title,
            Description = procurement2.Description,
            ProcurementType = procurement2.ProcurementType,
            Status = ProcurementStatus.UnderReview,
            Priority = procurement2.Priority
        });

        await _procurementService.UpdateProcurementAsync(procurement3.Id, new UpdateProcurementRequest
        {
            Title = procurement3.Title,
            Description = procurement3.Description,
            ProcurementType = procurement3.ProcurementType,
            Status = ProcurementStatus.Approved,
            Priority = procurement3.Priority
        });

        // Act
        var result = await _procurementService.GetPendingApprovalsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Id == procurement1.Id);
        Assert.Contains(result, p => p.Id == procurement2.Id);
        Assert.DoesNotContain(result, p => p.Id == procurement3.Id);
    }

    private async Task<Supplier> CreateTestSupplier(string name = "Test Supplier")
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            ContactEmail = $"{name.Replace(" ", "").ToLower()}@test.com",
            Address = "Test Address",
            SupplierType = SupplierType.Manufacturer,
            Status = SupplierStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Suppliers.AddAsync(supplier);
        await _context.SaveChangesAsync();
        return supplier;
    }

    private async Task<ProcurementDto> CreateTestProcurement(string supplierId, string title)
    {
        var request = new CreateProcurementRequest
        {
            SupplierId = supplierId,
            Title = title,
            Description = $"Description for {title}",
            ProcurementType = ProcurementType.Goods,
            RequiredDate = DateTime.UtcNow.AddDays(30),
            EstimatedValue = 5000m,
            Currency = "USD",
            RequestedBy = "Test User",
            Priority = ProcurementPriority.Medium
        };

        return await _procurementService.CreateProcurementAsync(request);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}