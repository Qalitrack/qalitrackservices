/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Infrastructure.Data;
using Transaction.Infrastructure.Repositories;
using Xunit;

namespace Transaction.Tests.Repositories;

public class TransactionRepositoryTests : IDisposable
{
    private readonly TransactionDbContext _context;
    private readonly TransactionRepository _repository;
    private readonly ServiceProvider _serviceProvider;

    public TransactionRepositoryTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TransactionDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                  .EnableSensitiveDataLogging()
                  .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TransactionDbContext>();
        _repository = new TransactionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
    }

    #region Base Repository CRUD Tests

    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldCreateAndReturnEntity()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-001",
            NoPlate = "KAA 123A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.ReceiptNo.Should().Be("TRX-001");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-002",
            NoPlate = "KBB 456B",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };

        // Act
        var created = await _repository.CreateAsync(transaction);

        // Assert - Retrieve from database
        var retrieved = await _context.Transactions.FindAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.ReceiptNo.Should().Be("TRX-002");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-003",
            NoPlate = "KCC 789C",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);

        // Act
        var result = await _repository.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.ReceiptNo.Should().Be("TRX-003");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedEntity_ShouldReturnNull()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-004",
            NoPlate = "KDD 101D",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);
        await _repository.DeleteAsync(created.Id);

        // Act
        var result = await _repository.GetByIdAsync(created.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-005",
            NoPlate = "KEE 202E",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-006",
            NoPlate = "KFF 303F",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedEntities()
    {
        // Arrange
        var transaction1 = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-007",
            NoPlate = "KGG 404G",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-008",
            NoPlate = "KHH 505H",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction1.Id);

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().ReceiptNo.Should().Be("TRX-008");
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateAndReturnEntity()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-009",
            NoPlate = "KII 606I",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);
        var originalUpdatedAt = created.UpdatedAt;

        // Wait a bit to ensure UpdatedAt changes
        await Task.Delay(10);

        // Act
        created.DriverName = "John Doe";
        created.Status = WeighbridgeTransactionStatus.InProgress;
        var result = await _repository.UpdateAsync(created);

        // Assert
        result.Should().NotBeNull();
        result!.DriverName.Should().Be("John Doe");
        result.Status.Should().Be(WeighbridgeTransactionStatus.InProgress);
        result.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-010",
            NoPlate = "KJJ 707J",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);

        // Act
        created.DriverName = "Jane Smith";
        await _repository.UpdateAsync(created);

        // Assert - Retrieve from database
        var retrieved = await _context.Transactions.FindAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.DriverName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldSoftDeleteEntity()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-011",
            NoPlate = "KKK 808K",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);

        // Act
        var result = await _repository.DeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete - Need to IgnoreQueryFilters to see soft-deleted entities
        var entity = await _context.Transactions.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == created.Id);
        entity.Should().NotBeNull();
        entity!.IsDeleted.Should().BeTrue();
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync("non-existent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithAlreadyDeletedEntity_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-012",
            NoPlate = "KLL 909L",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);
        await _repository.DeleteAsync(created.Id);

        // Act
        var result = await _repository.DeleteAsync(created.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-013",
            NoPlate = "KMM 101M",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);

        // Act
        var result = await _repository.ExistsAsync(created.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync("non-existent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithDeletedEntity_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-014",
            NoPlate = "KNN 202N",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        };
        var created = await _repository.CreateAsync(transaction);
        await _repository.DeleteAsync(created.Id);

        // Act
        var result = await _repository.ExistsAsync(created.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TransactionRepository Specific Tests

    [Fact]
    public async Task IsReceiptNoAvailableAsync_WithAvailableReceiptNo_ShouldReturnTrue()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-100",
            NoPlate = "KAA 100A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.IsReceiptNoAvailableAsync("TRX-999");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsReceiptNoAvailableAsync_WithExistingReceiptNo_ShouldReturnFalse()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-101",
            NoPlate = "KAA 101A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.IsReceiptNoAvailableAsync("TRX-101");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReceiptNoAvailableAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-102",
            NoPlate = "KAA 102A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.IsReceiptNoAvailableAsync("trx-102");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsReceiptNoAvailableAsync_WithDeletedReceiptNo_ShouldReturnTrue()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-103",
            NoPlate = "KAA 103A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction.Id);

        // Act
        var result = await _repository.IsReceiptNoAvailableAsync("TRX-103");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetByReceiptNoAsync_WithExistingReceiptNo_ShouldReturnTransaction()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-104",
            NoPlate = "KAA 104A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.GetByReceiptNoAsync("TRX-104");

        // Assert
        result.Should().NotBeNull();
        result!.ReceiptNo.Should().Be("TRX-104");
    }

    [Fact]
    public async Task GetByReceiptNoAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-105",
            NoPlate = "KAA 105A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.GetByReceiptNoAsync("trx-105");

        // Assert
        result.Should().NotBeNull();
        result!.ReceiptNo.Should().Be("TRX-105");
    }

    [Fact]
    public async Task GetByReceiptNoAsync_WithNonExistentReceiptNo_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByReceiptNoAsync("NON-EXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByReceiptNoAsync_WithDeletedTransaction_ShouldReturnNull()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-106",
            NoPlate = "KAA 106A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction.Id);

        // Act
        var result = await _repository.GetByReceiptNoAsync("TRX-106");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithNoFilters_ShouldReturnAllTransactions()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-200",
            NoPlate = "KAA 200A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-201",
            NoPlate = "KAA 201A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedAsync_WithReceiptNoFilter_ShouldReturnMatchingTransactions()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-300",
            NoPlate = "KAA 300A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "INV-301",
            NoPlate = "KAA 301A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            ReceiptNo = "TRX",
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().ReceiptNo.Should().Contain("TRX");
    }

    [Fact]
    public async Task GetPagedAsync_WithNoPlateFilter_ShouldReturnMatchingTransactions()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-400",
            NoPlate = "KAA 400A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-401",
            NoPlate = "KBB 401B",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            NoPlate = "KAA",
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().NoPlate.Should().Contain("KAA");
    }

    [Fact]
    public async Task GetPagedAsync_WithStatusFilter_ShouldReturnMatchingTransactions()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-500",
            NoPlate = "KAA 500A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-501",
            NoPlate = "KAA 501A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-502",
            NoPlate = "KAA 502A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Completed
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            Status = WeighbridgeTransactionStatus.InProgress,
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be(WeighbridgeTransactionStatus.InProgress);
    }

    [Fact]
    public async Task GetPagedAsync_WithVehicleIdFilter_ShouldReturnMatchingTransactions()
    {
        // Arrange
        var vehicleId1 = Guid.Parse("00000000-0000-0000-0000-000000000100");
        var vehicleId2 = Guid.Parse("00000000-0000-0000-0000-000000000200");
        
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-600",
            NoPlate = "KAA 600A",
            VehicleId = vehicleId1,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-601",
            NoPlate = "KAA 601A",
            VehicleId = vehicleId2,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            VehicleId = vehicleId1,
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().VehicleId.Should().Be(vehicleId1);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await _repository.CreateAsync(new WeighbridgeTransaction
            {
                ReceiptNo = $"TRX-{700 + i}",
                NoPlate = $"KAA {700 + i}A",
                ExpectedWeighings = 2,
                Status = WeighbridgeTransactionStatus.Pending
            });
        }

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 2,
            PageSize = 10,
            SortBy = "ReceiptNo",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_WithSortDescending_ShouldReturnInDescendingOrder()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-800",
            NoPlate = "KAA 800A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await Task.Delay(10);
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-801",
            NoPlate = "KAA 801A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDescending = true
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.First().ReceiptNo.Should().Be("TRX-801");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldExcludeDeletedTransactions()
    {
        // Arrange
        var transaction1 = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-900",
            NoPlate = "KAA 900A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-901",
            NoPlate = "KAA 901A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction1.Id);

        var filter = new WeighbridgeTransactionFilter
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var result = await _repository.GetPagedAsync(filter);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().ReceiptNo.Should().Be("TRX-901");
    }

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleAsync_ShouldReturnIncompleteTransactions()
    {
        // Arrange
        var noPlate = "KAA 1000A";
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1000",
            NoPlate = noPlate,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress,
            IsCompleted = false
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1001",
            NoPlate = noPlate,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Completed,
            IsCompleted = true
        });

        // Act
        var result = await _repository.GetIncompleteTransactionsByVehicleAsync(noPlate);

        // Assert
        result.Should().HaveCount(1);
        result.First().ReceiptNo.Should().Be("TRX-1000");
        result.First().IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleAsync_ShouldExcludeDeletedTransactions()
    {
        // Arrange
        var noPlate = "KAA 1100A";
        var transaction1 = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1100",
            NoPlate = noPlate,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress,
            IsCompleted = false
        });
        await _repository.DeleteAsync(transaction1.Id);

        // Act
        var result = await _repository.GetIncompleteTransactionsByVehicleAsync(noPlate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIncompleteTransactionsByVehicleIdAsync_ShouldReturnIncompleteTransactions()
    {
        // Arrange
        var vehicleId = "00000000-0000-0000-0000-000000001000";
        var vehicleIdGuid = Guid.Parse(vehicleId);
        
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            Id = Guid.NewGuid().ToString(),
            ReceiptNo = "TRX-1200",
            NoPlate = "KAA 1200A",
            VehicleId = vehicleIdGuid,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending,
            IsCompleted = false
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            Id = Guid.NewGuid().ToString(),
            ReceiptNo = "TRX-1201",
            NoPlate = "KAA 1201A",
            VehicleId = vehicleIdGuid,
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Completed,
            IsCompleted = true
        });

        // Act
        var result = await _repository.GetIncompleteTransactionsByVehicleIdAsync(vehicleId);

        // Assert
        result.Should().HaveCount(1);
        result.First().ReceiptNo.Should().Be("TRX-1200");
    }

    [Fact]
    public async Task GetTransactionsByStatusAsync_ShouldReturnTransactionsWithMatchingStatus()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1300",
            NoPlate = "KAA 1300A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1301",
            NoPlate = "KAA 1301A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress
        });
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1302",
            NoPlate = "KAA 1302A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.GetTransactionsByStatusAsync(WeighbridgeTransactionStatus.Pending);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Status == WeighbridgeTransactionStatus.Pending);
    }

    [Fact]
    public async Task GetTransactionsByStatusAsync_WithLimit_ShouldReturnLimitedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _repository.CreateAsync(new WeighbridgeTransaction
            {
                ReceiptNo = $"TRX-{1400 + i}",
                NoPlate = $"KAA {1400 + i}A",
                ExpectedWeighings = 2,
                Status = WeighbridgeTransactionStatus.Pending
            });
        }

        // Act
        var result = await _repository.GetTransactionsByStatusAsync(WeighbridgeTransactionStatus.Pending, limit: 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetTransactionsByStatusAsync_ShouldReturnInDescendingOrderByCreatedAt()
    {
        // Arrange
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1500",
            NoPlate = "KAA 1500A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await Task.Delay(10);
        await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1501",
            NoPlate = "KAA 1501A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });

        // Act
        var result = await _repository.GetTransactionsByStatusAsync(WeighbridgeTransactionStatus.Pending);

        // Assert
        result.First().ReceiptNo.Should().Be("TRX-1501");
    }

    [Fact]
    public async Task GetWithWeighingRecordsAsync_ShouldIncludeWeighingRecords()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1600",
            NoPlate = "KAA 1600A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.InProgress,
            WeighingRecords = new List<WeighingRecord>
            {
                new WeighingRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    WeighingSequence = 1,
                    Weight = 1000,
                    WeighingDate = DateTime.UtcNow
                }
            }
        });

        // Act
        var result = await _repository.GetWithWeighingRecordsAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.WeighingRecords.Should().NotBeNull();
        result.WeighingRecords.Should().HaveCount(1);
        result.WeighingRecords.First().Weight.Should().Be(1000);
    }

    [Fact]
    public async Task GetWithWeighingRecordsAsync_ShouldIncludeReweighRecords()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1700",
            NoPlate = "KAA 1700A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.ReweighRequested,
            ReweighRecords = new List<ReweighRecord>
            {
                new ReweighRecord
                {
                    AttemptNumber = 1,
                    Status = "InProgress",
                    Weight1 = 1500
                }
            }
        });

        // Act
        var result = await _repository.GetWithWeighingRecordsAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ReweighRecords.Should().NotBeNull();
        result.ReweighRecords.Should().HaveCount(1);
        result.ReweighRecords.First().Weight1.Should().Be(1500);
    }

    [Fact]
    public async Task GetWithWeighingRecordsAsync_WithDeletedTransaction_ShouldReturnNull()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1800",
            NoPlate = "KAA 1800A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction.Id);

        // Act
        var result = await _repository.GetWithWeighingRecordsAsync(transaction.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithAuditLogsAsync_ShouldIncludeAuditLogs()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-1900",
            NoPlate = "KAA 1900A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending,
            AuditLogs = new List<TransactionAuditLog>
            {
                new TransactionAuditLog
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = "Created",
                    ChangedBy = "TestUser",
                    ChangeTimestamp = DateTime.UtcNow
                }
            }
        });

        // Act
        var result = await _repository.GetWithAuditLogsAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.AuditLogs.Should().NotBeNull();
        result.AuditLogs.Should().HaveCount(1);
        result.AuditLogs.First().Action.Should().Be("Created");
    }

    [Fact]
    public async Task GetWithAuditLogsAsync_WithDeletedTransaction_ShouldReturnNull()
    {
        // Arrange
        var transaction = await _repository.CreateAsync(new WeighbridgeTransaction
        {
            ReceiptNo = "TRX-2000",
            NoPlate = "KAA 2000A",
            ExpectedWeighings = 2,
            Status = WeighbridgeTransactionStatus.Pending
        });
        await _repository.DeleteAsync(transaction.Id);

        // Act
        var result = await _repository.GetWithAuditLogsAsync(transaction.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithAuditLogsAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetWithAuditLogsAsync("non-existent-id");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
*/
