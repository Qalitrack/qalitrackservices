using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Core.DTOs;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Core.Mappings;
using Transaction.Core.Services;
using Transaction.Infrastructure.Data;
using Transaction.Infrastructure.Repositories;
using Xunit;

namespace Transaction.Tests.Integration;

/// <summary>
/// Integration tests demonstrating real-world weighbridge workflows
/// </summary>
public class WeighbridgeWorkflowTests : IDisposable
{
    private readonly TransactionDbContext _context;
    private readonly TransactionRepository _repository;
    private readonly TransactionService _service;
    private readonly ServiceProvider _serviceProvider;

    public WeighbridgeWorkflowTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<TransactionDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                  .EnableSensitiveDataLogging()
                  .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

        // Setup AutoMapper with the TransactionProfile
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            // Add the TransactionProfile which contains all the mappings
            cfg.AddProfile<TransactionProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        // Setup TimeService for East African Time
        services.AddSingleton<ITimeService, TimeService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TransactionDbContext>();
        _repository = new TransactionRepository(_context);
        var timeService = _serviceProvider.GetRequiredService<ITimeService>();
        _service = new TransactionService(_repository, mapper, timeService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
    }

    /// <summary>
    /// Test demonstrating the typical weighbridge workflow:
    /// 1. Truck A arrives and gets first weighing (full load)
    /// 2. Truck A leaves to offload
    /// 3. Other trucks (B, C) are weighed in the meantime
    /// 4. Truck A returns for second weighing (empty)
    /// 5. Transaction completes and net weight is calculated
    /// </summary>
    [Fact]
    public async Task RealWorld_TruckReturnsLaterForSecondWeighing_ShouldCompleteTransaction()
    {
        // ============================================================
        // STEP 1: Truck A arrives with full load - First Weighing
        // ============================================================
        var truckA_ReceiptNo = "WB-2025-001";
        var truckA_NoPlate = "ABC-123";

        var createTruckA = new CreateTransactionDto
        {
            ReceiptNo = truckA_ReceiptNo,
            NoPlate = truckA_NoPlate,
            DriverName = "John Doe",
            TransporterId = 1,
            TransporterName = "Fast Transport Ltd",
            CommodityId = 100,
            CommodityName = "Wheat",
            ExpectedWeighings = 2
        };

        var truckA_Transaction = await _service.CreateAsync(createTruckA);
        truckA_Transaction.Should().NotBeNull();
        var truckA_TransactionId = truckA_Transaction!.Id;

        // Add first weighing (truck is full - heavier)
        var truckA_FirstWeighing = new AddWeighingDto
        {
            TransactionId = truckA_TransactionId,
            Weight = 45000, // 45 tons (full truck)
            WeighBridgeId = 1,
            WeighBridgeName = "Main Weighbridge",
            ScaleName = "Scale-01",
            OperatorId = 101,
            OperatorName = "Operator Alice",
            Notes = "First weighing - truck with full load"
        };

        var truckA_AfterFirst = await _service.AddWeighingAsync(truckA_FirstWeighing);
        truckA_AfterFirst.Should().NotBeNull();
        truckA_AfterFirst!.CompletedWeighings.Should().Be(1);
        truckA_AfterFirst.Status.Should().Be("InProgress");
        truckA_AfterFirst.IsCompleted.Should().BeFalse();
        truckA_AfterFirst.FirstWeight.Should().Be(45000);

        Console.WriteLine($"✓ Truck A ({truckA_NoPlate}) - First weighing: {truckA_AfterFirst.FirstWeight}kg at {truckA_AfterFirst.FirstWeightTimestamp}");
        Console.WriteLine($"  Status: {truckA_AfterFirst.Status}, Receipt: {truckA_ReceiptNo}");
        Console.WriteLine($"  Truck A leaves to offload...\n");

        // ============================================================
        // STEP 2: Meanwhile, other trucks are weighed
        // ============================================================

        // Truck B - Complete transaction (both weighings done immediately)
        var truckB_ReceiptNo = "WB-2025-002";
        var createTruckB = new CreateTransactionDto
        {
            ReceiptNo = truckB_ReceiptNo,
            NoPlate = "XYZ-789",
            DriverName = "Jane Smith",
            TransporterId = 2,
            TransporterName = "Quick Haul Inc",
            CommodityId = 101,
            CommodityName = "Maize",
            ExpectedWeighings = 2
        };

        var truckB_Transaction = await _service.CreateAsync(createTruckB);
        await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = truckB_Transaction!.Id,
            Weight = 38000,
            WeighBridgeId = 1,
            WeighBridgeName = "Main Weighbridge",
            ScaleName = "Scale-01",
            OperatorId = 101,
            OperatorName = "Operator Alice"
        });

        var truckB_Completed = await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = truckB_Transaction.Id,
            Weight = 12000,
            WeighBridgeId = 1,
            WeighBridgeName = "Main Weighbridge",
            ScaleName = "Scale-01",
            OperatorId = 101,
            OperatorName = "Operator Alice"
        });

        Console.WriteLine($"✓ Truck B (XYZ-789) - Completed both weighings");
        Console.WriteLine($"  First: {truckB_Completed!.FirstWeight}kg, Second: {truckB_Completed.SecondWeight}kg");
        Console.WriteLine($"  Net Weight: {truckB_Completed.NetWeight}kg");
        Console.WriteLine($"  Status: {truckB_Completed.Status}\n");

        // Truck C - First weighing only
        var truckC_ReceiptNo = "WB-2025-003";
        var createTruckC = new CreateTransactionDto
        {
            ReceiptNo = truckC_ReceiptNo,
            NoPlate = "LMN-456",
            DriverName = "Bob Wilson",
            TransporterId = 3,
            TransporterName = "Express Logistics",
            CommodityId = 102,
            CommodityName = "Rice",
            ExpectedWeighings = 2
        };

        var truckC_Transaction = await _service.CreateAsync(createTruckC);
        await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = truckC_Transaction!.Id,
            Weight = 42000,
            WeighBridgeId = 1,
            WeighBridgeName = "Main Weighbridge",
            ScaleName = "Scale-01",
            OperatorId = 102,
            OperatorName = "Operator Bob"
        });

        Console.WriteLine($"✓ Truck C (LMN-456) - First weighing done, waiting for return\n");

        // ============================================================
        // STEP 3: Truck A returns for second weighing (after offloading)
        // ============================================================

        // Operator needs to find Truck A's incomplete transaction
        // Option 1: Search by receipt number
        var foundByReceipt = await _repository.GetByReceiptNoAsync(truckA_ReceiptNo);
        foundByReceipt.Should().NotBeNull();
        foundByReceipt!.IsCompleted.Should().BeFalse();

        // Option 2: Search by vehicle plate
        var incompleteForVehicle = await _repository.GetIncompleteTransactionsByVehicleAsync(truckA_NoPlate);
        incompleteForVehicle.Should().ContainSingle();
        incompleteForVehicle.First().ReceiptNo.Should().Be(truckA_ReceiptNo);

        Console.WriteLine($"✓ Truck A ({truckA_NoPlate}) returns for second weighing");
        Console.WriteLine($"  Found incomplete transaction by receipt: {truckA_ReceiptNo}");
        Console.WriteLine($"  Found incomplete transaction by plate: {truckA_NoPlate}\n");

        // Add second weighing (truck is now empty - lighter)
        var truckA_SecondWeighing = new AddWeighingDto
        {
            TransactionId = truckA_TransactionId,
            Weight = 15000, // 15 tons (empty truck)
            WeighBridgeId = 1,
            WeighBridgeName = "Main Weighbridge",
            ScaleName = "Scale-02",
            OperatorId = 102,
            OperatorName = "Operator Bob",
            Notes = "Second weighing - truck empty after offload"
        };

        var truckA_Final = await _service.AddWeighingAsync(truckA_SecondWeighing);

        // ============================================================
        // STEP 4: Verify transaction completed correctly
        // ============================================================
        truckA_Final.Should().NotBeNull();
        truckA_Final!.CompletedWeighings.Should().Be(2);
        truckA_Final.Status.Should().Be("Completed");
        truckA_Final.IsCompleted.Should().BeTrue();
        truckA_Final.FirstWeight.Should().Be(45000);
        truckA_Final.SecondWeight.Should().Be(15000);
        truckA_Final.NetWeight.Should().Be(30000); // 45000 - 15000 = 30000kg (30 tons of wheat)
        truckA_Final.CompletedDate.Should().NotBeNull();
        truckA_Final.NetWeightCalculatedTimestamp.Should().NotBeNull();

        Console.WriteLine($"✓ Truck A ({truckA_NoPlate}) - Transaction Completed!");
        Console.WriteLine($"  First Weight: {truckA_Final.FirstWeight}kg at {truckA_Final.FirstWeightTimestamp}");
        Console.WriteLine($"  Second Weight: {truckA_Final.SecondWeight}kg at {truckA_Final.SecondWeightTimestamp}");
        Console.WriteLine($"  NET WEIGHT: {truckA_Final.NetWeight}kg ({truckA_Final.CommodityName})");
        Console.WriteLine($"  Completed: {truckA_Final.CompletedDate}\n");

        // ============================================================
        // STEP 5: Verify all transactions in system
        // ============================================================
        var allTransactions = await _repository.GetAllAsync();
        allTransactions.Should().HaveCount(3);

        var completed = allTransactions.Where(t => t.IsCompleted).ToList();
        var incomplete = allTransactions.Where(t => !t.IsCompleted).ToList();

        completed.Should().HaveCount(2); // Truck A and Truck B
        incomplete.Should().HaveCount(1); // Truck C still waiting

        Console.WriteLine($"=== Final Status ===");
        Console.WriteLine($"Total Transactions: {allTransactions.Count()}");
        Console.WriteLine($"Completed: {completed.Count} (Truck A, Truck B)");
        Console.WriteLine($"Incomplete: {incomplete.Count} (Truck C - still waiting for second weighing)");

        // Verify time difference between first and second weighing for Truck A
        var timeBetweenWeighings = truckA_Final.SecondWeightTimestamp!.Value - truckA_Final.FirstWeightTimestamp!.Value;
        timeBetweenWeighings.Should().BePositive();
        Console.WriteLine($"\nTime between Truck A weighings: {timeBetweenWeighings.TotalSeconds:F2} seconds");
    }

    /// <summary>
    /// Test verifying that transactions remain accessible and modifiable between weighings
    /// </summary>
    [Fact]
    public async Task MultipleVehicles_InterleavedWeighings_ShouldMaintainCorrectState()
    {
        // Create three transactions
        var transactions = new[]
        {
            await _service.CreateAsync(new CreateTransactionDto
            {
                ReceiptNo = "R001",
                NoPlate = "AAA-111",
                DriverName = "Driver 1",
                TransporterId = 1,
                TransporterName = "Transport 1",
                ExpectedWeighings = 2
            }),
            await _service.CreateAsync(new CreateTransactionDto
            {
                ReceiptNo = "R002",
                NoPlate = "BBB-222",
                DriverName = "Driver 2",
                TransporterId = 2,
                TransporterName = "Transport 2",
                ExpectedWeighings = 2
            }),
            await _service.CreateAsync(new CreateTransactionDto
            {
                ReceiptNo = "R003",
                NoPlate = "CCC-333",
                DriverName = "Driver 3",
                TransporterId = 3,
                TransporterName = "Transport 3",
                ExpectedWeighings = 2
            })
        };

        // Interleaved pattern: 1st weighing for all, then 2nd weighing in different order
        // Vehicle 1 - First weighing
        await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[0]!.Id,
            Weight = 40000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 1,
            OperatorName = "Op1"
        });

        // Vehicle 2 - First weighing
        await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[1]!.Id,
            Weight = 35000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 1,
            OperatorName = "Op1"
        });

        // Vehicle 3 - First weighing
        await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[2]!.Id,
            Weight = 38000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 1,
            OperatorName = "Op1"
        });

        // Verify all are in progress
        var incomplete = await _repository.GetTransactionsByStatusAsync(WeighbridgeTransactionStatus.InProgress);
        incomplete.Should().HaveCount(3);

        // Second weighings in different order: Vehicle 3, then 1, then 2
        var result3 = await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[2]!.Id,
            Weight = 12000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 2,
            OperatorName = "Op2"
        });

        var result1 = await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[0]!.Id,
            Weight = 10000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 2,
            OperatorName = "Op2"
        });

        var result2 = await _service.AddWeighingAsync(new AddWeighingDto
        {
            TransactionId = transactions[1]!.Id,
            Weight = 11000,
            WeighBridgeId = 1,
            WeighBridgeName = "WB1",
            ScaleName = "S1",
            OperatorId = 2,
            OperatorName = "Op2"
        });

        // Verify all completed with correct weights
        result1!.IsCompleted.Should().BeTrue();
        result1.NetWeight.Should().Be(30000); // 40000 - 10000

        result2!.IsCompleted.Should().BeTrue();
        result2.NetWeight.Should().Be(24000); // 35000 - 11000

        result3!.IsCompleted.Should().BeTrue();
        result3.NetWeight.Should().Be(26000); // 38000 - 12000

        var completed = await _repository.GetTransactionsByStatusAsync(WeighbridgeTransactionStatus.Completed);
        completed.Should().HaveCount(3);
    }
}
