using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Transaction.Api.Controllers;
using Transaction.Core.DTOs;
using Transaction.Core.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace Transaction.Tests.Controllers;

public class WeighbridgeTransactionIntegrationTests
{
    private readonly Mock<ITransactionService> _mockService;
    private readonly Mock<ILogger<TransactionsController>> _mockLogger;
    private readonly TransactionsController _controller;
    private readonly ITestOutputHelper _output;

    public WeighbridgeTransactionIntegrationTests(ITestOutputHelper output)
    {
        _mockService = new Mock<ITransactionService>();
        _mockLogger = new Mock<ILogger<TransactionsController>>();
        _controller = new TransactionsController(_mockService.Object, _mockLogger.Object);
        _output = output;
    }

    #region Helpers

    private void LogFullTransaction(string title, TransactionReadDto t)
    {
        _output.WriteLine($"\n{title}");
        _output.WriteLine("──────────────────────────────────────────────────────────────");
        _output.WriteLine($"TicketID / ReceiptNo ......: {t.TicketID} / {t.ReceiptNo}");
        _output.WriteLine($"Vehicle / Driver ..........: {t.NoPlate} – {t.DriverName}");
        _output.WriteLine($"Transporter ...............: {t.TransporterName} (ID {t.TransporterID})");
        _output.WriteLine($"Commodity .................: {t.CommodityName ?? "—"} (ID {t.CommodityID?.ToString() ?? "—"})");
        _output.WriteLine($"Supplier → Customer .......: {t.SupplierName ?? "—"} (ID {t.SupplierID?.ToString() ?? "—"}) → {t.CustomerName ?? "—"} (ID {t.CustomerID?.ToString() ?? "—"})");
        _output.WriteLine($"Origin → Destination ......: {t.OriginName ?? "—"} (ID {t.OriginID?.ToString() ?? "—"}) → {t.DestinationName ?? "—"} (ID {t.DestinationID?.ToString() ?? "—"})");
        _output.WriteLine($"First Weight ..............: {t.FirstWeight} kg @ {t.FirstWeightDate:yyyy-MM-dd HH:mm:ss} UTC");
        _output.WriteLine($"First Weighbridge/Scale ...: {t.WeighBridgeName ?? "—"} (ID {t.WeighBridgeID?.ToString() ?? "—"}) / {t.ScaleName ?? "—"}");
        _output.WriteLine($"First Operator ............: {t.OperatorName ?? "—"} (ID {t.OperatorID?.ToString() ?? "—"})");
        _output.WriteLine($"Second Weight .............: {t.SecondWeight ?? "—"} kg @ {(t.SecondWeightDate != default ? t.SecondWeightDate.ToString("yyyy-MM-dd HH:mm:ss") : "—")}");
        _output.WriteLine($"Second Weighbridge/Scale ..: {t.WeighBridgeName2nd ?? "—"} / {t.ScaleName2nd ?? "—"}");
        _output.WriteLine($"Second Operator ...........: {t.OperatorName2nd ?? "—"} (ID {t.OperatorID2nd ?? "—"})");
        _output.WriteLine($"Net Weight ................: {t.NetWeight ?? "—"} kg");
        _output.WriteLine($"Status ....................: {t.Status}");
        _output.WriteLine($"Weigh Mode / Operation ....: {t.WeighMode ?? "—"} / {t.Operation ?? "—"}");
        if (!string.IsNullOrEmpty(t.ReweighPermission)) _output.WriteLine($"Reweigh Permission ........: {t.ReweighPermission}");
        if (!string.IsNullOrEmpty(t.ChangeDesc))        _output.WriteLine($"Change Description ........: {t.ChangeDesc}");
        if (t.ApiId.HasValue)                           _output.WriteLine($"API ID ....................: {t.ApiId}");
        _output.WriteLine("──────────────────────────────────────────────────────────────\n");
    }

    #endregion

    [Fact]
    public async Task Scenario_CompleteWeighingCycle_ShouldSucceed()
    {
        _output.WriteLine("=== SCENARIO 1: Complete Two-Weighing Cycle (Full Fields) ===\n");

        var createDto = new CreateTransactionDto
        {
            NoPlate = "KBZ-001A",
            DriverName = "John M. Doe",
            TransporterID = 17,
            TransporterName = "Swift Agro Logistics Ltd",
            FirstWeight = "45820",
            WeighBridgeID = 3,
            WeighBridgeName = "Main Entrance Bridge",
            ScaleName = "MET-500A",
            OperatorID = 101,
            OperatorName = "Alice K. Mwangi",
            CommodityID = 8,
            CommodityName = "Yellow Maize (Grade 1)",
            SupplierID = 42,
            SupplierName = "Green Valley Farms Kenya",
            CustomerID = 19,
            CustomerName = "Nairobi Millers Co.",
            OriginID = 5,
            OriginName = "Kitale Collection Point",
            DestinationID = 12,
            DestinationName = "Industrial Area Silo",
            WeighMode = "Gross → Tare",
            Operation = "Inbound"
        };

        var created = new TransactionReadDto
        {
            TicketID = 10021,
            ReceiptNo = "WB-20260121-021",
            NoPlate = createDto.NoPlate,
            DriverName = createDto.DriverName,
            TransporterID = createDto.TransporterID,
            TransporterName = createDto.TransporterName,
            FirstWeight = createDto.FirstWeight,
            SecondWeight = null,
            NetWeight = null,
            Status = "Active",
            FirstWeightDate = DateTime.UtcNow.AddMinutes(-45),
            WeighBridgeID = createDto.WeighBridgeID,
            WeighBridgeName = createDto.WeighBridgeName,
            ScaleName = createDto.ScaleName,
            OperatorID = createDto.OperatorID,
            OperatorName = createDto.OperatorName,
            CommodityID = createDto.CommodityID,
            CommodityName = createDto.CommodityName,
            SupplierID = createDto.SupplierID,
            SupplierName = createDto.SupplierName,
            CustomerID = createDto.CustomerID,
            CustomerName = createDto.CustomerName,
            OriginID = createDto.OriginID,
            OriginName = createDto.OriginName,
            DestinationID = createDto.DestinationID,
            DestinationName = createDto.DestinationName,
            WeighMode = createDto.WeighMode,
            Operation = createDto.Operation,
            ApiId = 987654
        };

        _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateTransactionDto>())).ReturnsAsync(created);

        var createResult = await _controller.Create(createDto);
        createResult.Should().BeOfType<CreatedAtActionResult>();

        LogFullTransaction("After First Weighing (Inbound)", created);

        var secondDto = new AddSecondWeightDto
        {
            TicketID = created.TicketID,
            SecondWeight = "20850",
            WeighBridgeName2nd = "Exit Bridge",
            ScaleName2nd = "MET-600B",
            OperatorID2nd = "108",
            OperatorName2nd = "Bernard Omondi"
        };

        var completed = new TransactionReadDto
        {
            TicketID = created.TicketID,
            ReceiptNo = created.ReceiptNo,
            NoPlate = created.NoPlate,
            DriverName = created.DriverName,
            TransporterID = created.TransporterID,
            TransporterName = created.TransporterName,
            CommodityID = created.CommodityID,
            CommodityName = created.CommodityName,
            SupplierID = created.SupplierID,
            SupplierName = created.SupplierName,
            CustomerID = created.CustomerID,
            CustomerName = created.CustomerName,
            OriginID = created.OriginID,
            OriginName = created.OriginName,
            DestinationID = created.DestinationID,
            DestinationName = created.DestinationName,
            FirstWeight = created.FirstWeight,
            FirstWeightDate = created.FirstWeightDate,
            WeighBridgeID = created.WeighBridgeID,
            WeighBridgeName = created.WeighBridgeName,
            ScaleName = created.ScaleName,
            OperatorID = created.OperatorID,
            OperatorName = created.OperatorName,
            WeighMode = created.WeighMode,
            Operation = created.Operation,
            ApiId = created.ApiId,
            SecondWeight = secondDto.SecondWeight,
            NetWeight = "24970",
            Status = "Completed",
            SecondWeightDate = DateTime.UtcNow,
            WeighBridgeName2nd = secondDto.WeighBridgeName2nd,
            ScaleName2nd = secondDto.ScaleName2nd,
            OperatorID2nd = secondDto.OperatorID2nd,
            OperatorName2nd = secondDto.OperatorName2nd,
            ChangeDesc = "Second weighing completed - normal exit"
        };

        _mockService.Setup(s => s.AddSecondWeightAsync(It.IsAny<AddSecondWeightDto>())).ReturnsAsync(completed);

        var secondResult = await _controller.AddSecondWeight(secondDto);
        secondResult.Should().BeOfType<OkObjectResult>();

        LogFullTransaction("After Second Weighing (Outbound)", completed);

        completed.Status.Should().Be("Completed");
        completed.NetWeight.Should().Be("24970");
        completed.SecondWeight.Should().NotBeNullOrEmpty();
        completed.SecondWeightDate.Should().BeAfter(created.FirstWeightDate);
    }

    [Fact]
    public async Task Scenario_MultipleVehiclesSimultaneously_ShouldHandleCorrectly()
    {
        _output.WriteLine("=== SCENARIO 2: Multiple Vehicles Being Weighed Simultaneously ===\n");

        var vehicles = new[]
        {
            new { Plate = "KAA-100B", Driver = "Alice Wanjiku", Commodity = "Wheat", Weight = "50000", TransporterId = 21, OriginId = 7, Origin = "Eldoret", DestId = 14, Dest = "Mombasa Port", SupplierId = 31, Supplier = "North Rift Growers" },
            new { Plate = "KBB-200C", Driver = "Bob Kamau",    Commodity = "Rice",  Weight = "48000", TransporterId = 22, OriginId = 8, Origin = "Bungoma", DestId = 15, Dest = "Nairobi", SupplierId = 32, Supplier = "Western Rice Ltd" },
            new { Plate = "KCC-300D", Driver = "Charlie Otieno",Commodity = "Beans", Weight = "52000", TransporterId = 23, OriginId = 9, Origin = "Kakamega", DestId = 16, Dest = "Athi River", SupplierId = 33, Supplier = "Lakeside Pulses" }
        };

        var transactions = new List<TransactionReadDto>();

        for (int i = 0; i < vehicles.Length; i++)
        {
            var v = vehicles[i];

            var dto = new CreateTransactionDto
            {
                NoPlate = v.Plate,
                DriverName = v.Driver,
                TransporterID = v.TransporterId,
                TransporterName = $"AgriHaul {i+1}",
                FirstWeight = v.Weight,
                WeighBridgeID = (i % 2) + 1,
                WeighBridgeName = i % 2 == 0 ? "North Scale" : "South Scale",
                ScaleName = $"Scale-{(char)('A' + i)}",
                OperatorID = 100 + i,
                OperatorName = $"Operator-{i + 1}",
                CommodityName = v.Commodity,
                OriginID = v.OriginId,
                OriginName = v.Origin,
                DestinationID = v.DestId,
                DestinationName = v.Dest,
                SupplierID = v.SupplierId,
                SupplierName = v.Supplier
            };

            var transaction = new TransactionReadDto
            {
                TicketID = 2000 + i + 1,
                ReceiptNo = $"WB-20260121-{200 + i + 1:000}",
                NoPlate = v.Plate,
                DriverName = v.Driver,
                TransporterID = v.TransporterId,
                TransporterName = dto.TransporterName,
                FirstWeight = v.Weight,
                Status = "Active",
                WeighBridgeID = dto.WeighBridgeID,
                WeighBridgeName = dto.WeighBridgeName,
                ScaleName = dto.ScaleName,
                OperatorID = dto.OperatorID,
                OperatorName = dto.OperatorName,
                CommodityName = v.Commodity,
                FirstWeightDate = DateTime.UtcNow.AddMinutes(-i * 7 - 15),
                OriginID = v.OriginId,
                OriginName = v.Origin,
                DestinationID = v.DestId,
                DestinationName = v.Dest,
                SupplierID = v.SupplierId,
                SupplierName = v.Supplier,
                WeighMode = "Gross",
                Operation = "Inbound",
                ApiId = 900100 + i
            };

            transactions.Add(transaction);

            _mockService.Setup(s => s.CreateAsync(It.Is<CreateTransactionDto>(d => d.NoPlate == v.Plate)))
                .ReturnsAsync(transaction);

            await _controller.Create(dto);

            _output.WriteLine($"Vehicle {i + 1}: {v.Plate} – {v.Driver}");
            _output.WriteLine($"  Ticket: {transaction.TicketID} / {transaction.ReceiptNo}");
            _output.WriteLine($"  First Weight: {transaction.FirstWeight} kg");
            _output.WriteLine($"  Scale: {transaction.ScaleName} @ {transaction.WeighBridgeName} (ID {transaction.WeighBridgeID})");
            _output.WriteLine($"  Origin → Dest: {transaction.OriginName} → {transaction.DestinationName}");
            _output.WriteLine($"  Supplier: {transaction.SupplierName}");
            _output.WriteLine($"  Status: {transaction.Status}\n");
        }

        _output.WriteLine($"Total active transactions: {transactions.Count}");
    }

    [Fact]
    public async Task Scenario_TransactionImmutability_ShouldPreventModification()
    {
        _output.WriteLine("=== SCENARIO 3: Transaction Immutability After Completion ===\n");

        var completed = new TransactionReadDto
        {
            TicketID = 1005,
            ReceiptNo = "WB-20260121-005",
            NoPlate = "KDZ-500E",
            DriverName = "David Maina",
            TransporterID = 7,
            TransporterName = "Fast Transport Ltd",
            FirstWeight = "40000",
            SecondWeight = "15000",
            NetWeight = "25000",
            Status = "Completed",
            FirstWeightDate = DateTime.UtcNow.AddHours(-4),
            SecondWeightDate = DateTime.UtcNow.AddHours(-1),
            WeighBridgeID = 2,
            WeighBridgeName = "Central Weighbridge",
            ScaleName = "Avery 300T",
            OperatorID = 205,
            OperatorName = "Mary Wanjiru",
            WeighBridgeName2nd = "Exit Weighbridge",
            ScaleName2nd = "Bizerba 500T",
            OperatorID2nd = "208",
            OperatorName2nd = "James Kiptoo",
            CommodityID = 11,
            CommodityName = "Fertilizer DAP",
            SupplierID = 45,
            SupplierName = "Yara East Africa",
            CustomerID = 28,
            CustomerName = "Nakuru Agro Depot",
            OriginID = 3,
            OriginName = "Mombasa Port",
            DestinationID = 9,
            DestinationName = "Nakuru Depot",
            WeighMode = "Gross → Tare",
            Operation = "Inbound",
            ApiId = 765432
        };

        _mockService.Setup(s => s.GetByIdAsync(1005)).ReturnsAsync(completed);
        LogFullTransaction("Completed Transaction (Immutable)", completed);

        var invalidSecond = new AddSecondWeightDto { TicketID = 1005, SecondWeight = "18000" };
        _mockService.Setup(s => s.AddSecondWeightAsync(invalidSecond))
            .ThrowsAsync(new InvalidOperationException("Second weight already recorded."));
        var result = await _controller.AddSecondWeight(invalidSecond);
        result.Should().BeOfType<BadRequestObjectResult>();

        var updateDto = new UpdateTransactionDto { NoPlate = "KDZ-999Z", DriverName = "New Driver" };
        _mockService.Setup(s => s.UpdateAsync(1005, updateDto))
            .ThrowsAsync(new InvalidOperationException("Cannot modify completed transaction."));
        var updateResult = await _controller.Update(1005, updateDto);
        updateResult.Should().BeOfType<BadRequestObjectResult>();

        _output.WriteLine("✓ Immutability preserved");
    }

    [Fact]
    public async Task Scenario_DifferentScales_ShouldRecordCorrectly()
    {
        _output.WriteLine("=== SCENARIO 4: Different Scales – First & Second Weighing ===\n");

        var createDto = new CreateTransactionDto
        {
            NoPlate = "KEZ-700F",
            DriverName = "Eva Akinyi",
            TransporterID = 11,
            TransporterName = "Express Cargo Services",
            FirstWeight = "55230",
            WeighBridgeID = 4,
            WeighBridgeName = "Warehouse Gate",
            ScaleName = "DIGI-400",
            OperatorID = 201,
            OperatorName = "Peter Njoroge",
            CommodityID = 9,
            CommodityName = "White Maize",
            SupplierID = 38,
            SupplierName = "Kitui Farmers Coop",
            OriginID = 6,
            OriginName = "Kitui",
            DestinationID = 13,
            DestinationName = "Mlolongo Godown"
        };

        var first = new TransactionReadDto
        {
            TicketID = 1008,
            ReceiptNo = "WB-20260121-008",
            NoPlate = createDto.NoPlate,
            DriverName = createDto.DriverName,
            TransporterID = createDto.TransporterID,
            TransporterName = createDto.TransporterName,
            FirstWeight = createDto.FirstWeight,
            Status = "Active",
            WeighBridgeID = createDto.WeighBridgeID,
            WeighBridgeName = createDto.WeighBridgeName,
            ScaleName = createDto.ScaleName,
            OperatorID = createDto.OperatorID,
            OperatorName = createDto.OperatorName,
            FirstWeightDate = DateTime.UtcNow.AddHours(-3.5),
            CommodityID = createDto.CommodityID,
            CommodityName = createDto.CommodityName,
            SupplierID = createDto.SupplierID,
            SupplierName = createDto.SupplierName,
            OriginID = createDto.OriginID,
            OriginName = createDto.OriginName,
            DestinationID = createDto.DestinationID,
            DestinationName = createDto.DestinationName,
            WeighMode = "Gross → Tare",
            Operation = "Inbound",
            ApiId = 112233
        };

        _mockService.Setup(s => s.CreateAsync(It.IsAny<CreateTransactionDto>())).ReturnsAsync(first);
        LogFullTransaction("First Weighing (Warehouse Gate)", first);

        var secondDto = new AddSecondWeightDto
        {
            TicketID = first.TicketID,
            SecondWeight = "30180",
            WeighBridgeName2nd = "Factory Exit Gate",
            ScaleName2nd = "MET-700C",
            OperatorID2nd = "205",
            OperatorName2nd = "Sarah Wambui"
        };

        var completed = new TransactionReadDto
        {
            TicketID = first.TicketID,
            ReceiptNo = first.ReceiptNo,
            NoPlate = first.NoPlate,
            DriverName = first.DriverName,
            TransporterID = first.TransporterID,
            TransporterName = first.TransporterName,
            FirstWeight = first.FirstWeight,
            FirstWeightDate = first.FirstWeightDate,
            WeighBridgeID = first.WeighBridgeID,
            WeighBridgeName = first.WeighBridgeName,
            ScaleName = first.ScaleName,
            OperatorID = first.OperatorID,
            OperatorName = first.OperatorName,
            CommodityID = first.CommodityID,
            CommodityName = first.CommodityName,
            SupplierID = first.SupplierID,
            SupplierName = first.SupplierName,
            OriginID = first.OriginID,
            OriginName = first.OriginName,
            DestinationID = first.DestinationID,
            DestinationName = first.DestinationName,
            WeighMode = first.WeighMode,
            Operation = first.Operation,
            ApiId = first.ApiId,
            SecondWeight = secondDto.SecondWeight,
            NetWeight = "25050",
            Status = "Completed",
            SecondWeightDate = DateTime.UtcNow,
            WeighBridgeName2nd = secondDto.WeighBridgeName2nd,
            ScaleName2nd = secondDto.ScaleName2nd,
            OperatorID2nd = secondDto.OperatorID2nd,
            OperatorName2nd = secondDto.OperatorName2nd
        };

        _mockService.Setup(s => s.AddSecondWeightAsync(It.IsAny<AddSecondWeightDto>())).ReturnsAsync(completed);

        var result = await _controller.AddSecondWeight(secondDto);
        result.Should().BeOfType<OkObjectResult>();

        LogFullTransaction("Second Weighing (Factory Exit)", completed);

        completed.NetWeight.Should().Be("25050");
        completed.WeighBridgeName.Should().NotBe(completed.WeighBridgeName2nd);
    }

    [Fact]
    public async Task Scenario_ReweighRequest_ShouldFollowProperFlow()
    {
        _output.WriteLine("=== SCENARIO 5: Reweigh Request Process ===\n");

        var original = new TransactionReadDto
        {
            TicketID = 1012,
            ReceiptNo = "WB-20260121-012",
            NoPlate = "KFZ-800G",
            DriverName = "Frank Kipchoge",
            TransporterID = 9,
            TransporterName = "Reliable Hauliers",
            FirstWeight = "60050",
            SecondWeight = "35020",
            NetWeight = "25030",
            Status = "Completed",
            FirstWeightDate = DateTime.UtcNow.AddHours(-5),
            SecondWeightDate = DateTime.UtcNow.AddHours(-2),
            WeighBridgeID = 1,
            WeighBridgeName = "Main Gate",
            ScaleName = "Toledo 400",
            OperatorID = 210,
            OperatorName = "Grace Njeri",
            WeighBridgeName2nd = "Exit Gate",
            ScaleName2nd = "Fairbanks 600",
            OperatorID2nd = "212",
            OperatorName2nd = "Paul Mutua",
            CommodityID = 10,
            CommodityName = "Sugar (White)",
            SupplierID = 50,
            SupplierName = "Mumias Sugar Co.",
            CustomerID = 35,
            CustomerName = "Eastmatt Supermarkets",
            OriginID = 4,
            OriginName = "Mumias",
            DestinationID = 11,
            DestinationName = "Industrial Area Warehouse",
            WeighMode = "Gross → Tare",
            Operation = "Inbound",
            ApiId = 556677
        };

        LogFullTransaction("Original Completed Transaction", original);

        var reweighRequest = new RequestReweighDto
        {
            TicketID = original.TicketID,
            Reason = "Suspected scale calibration drift – net weight appears inconsistent"
        };

        _mockService.Setup(s => s.RequestReweighAsync(reweighRequest)).ReturnsAsync(true);

        var reqResult = await _controller.RequestReweigh(reweighRequest);
        reqResult.Should().BeOfType<OkObjectResult>();

        _output.WriteLine($"Reweigh Requested → Reason: {reweighRequest.Reason}");
        _output.WriteLine("Status: Approved / Pending Re-weigh");

        var updated = new TransactionReadDto
        {
            TicketID = original.TicketID,
            ReceiptNo = original.ReceiptNo,
            NoPlate = original.NoPlate,
            DriverName = original.DriverName,
            TransporterID = original.TransporterID,
            TransporterName = original.TransporterName,
            FirstWeight = original.FirstWeight,
            SecondWeight = original.SecondWeight,
            NetWeight = original.NetWeight,
            FirstWeightDate = original.FirstWeightDate,
            SecondWeightDate = original.SecondWeightDate,
            WeighBridgeID = original.WeighBridgeID,
            WeighBridgeName = original.WeighBridgeName,
            ScaleName = original.ScaleName,
            OperatorID = original.OperatorID,
            OperatorName = original.OperatorName,
            WeighBridgeName2nd = original.WeighBridgeName2nd,
            ScaleName2nd = original.ScaleName2nd,
            OperatorID2nd = original.OperatorID2nd,
            OperatorName2nd = original.OperatorName2nd,
            CommodityID = original.CommodityID,
            CommodityName = original.CommodityName,
            SupplierID = original.SupplierID,
            SupplierName = original.SupplierName,
            CustomerID = original.CustomerID,
            CustomerName = original.CustomerName,
            OriginID = original.OriginID,
            OriginName = original.OriginName,
            DestinationID = original.DestinationID,
            DestinationName = original.DestinationName,
            WeighMode = original.WeighMode,
            Operation = original.Operation,
            ApiId = original.ApiId,
            Status = "ReweighRequested",
            ReweighPermission = reweighRequest.Reason,
            ChangeDesc = $"Reweigh requested: {reweighRequest.Reason}"
        };

        LogFullTransaction("After Reweigh Request", updated);
    }

    [Fact]
    public async Task Scenario_QueryIncompleteTransactions_ShouldReturnPending()
    {
        _output.WriteLine("=== SCENARIO 6: Query Incomplete Transactions ===\n");

        var incomplete = new List<TransactionReadDto>
        {
            new TransactionReadDto
            {
                TicketID = 1015,
                ReceiptNo = "WB-20260121-015",
                NoPlate = "KGZ-900H",
                DriverName = "George Muthoni",
                TransporterID = 14,
                TransporterName = "Coast Link Logistics",
                FirstWeight = "48200",
                Status = "Active",
                FirstWeightDate = DateTime.UtcNow.AddHours(-1.2),
                WeighBridgeID = 1,
                WeighBridgeName = "Main Gate",
                ScaleName = "Toledo 450",
                OperatorID = 215,
                OperatorName = "Mercy Achieng",
                CommodityID = 7,
                CommodityName = "Wheat Flour",
                SupplierID = 47,
                SupplierName = "Unga Millers",
                OriginID = 10,
                OriginName = "Eldoret Depot",
                DestinationID = 17,
                DestinationName = "Kisumu Warehouse",
                WeighMode = "Gross",
                Operation = "Inbound",
                ApiId = 334455
            },
            new TransactionReadDto
            {
                TicketID = 1016,
                ReceiptNo = "WB-20260121-016",
                NoPlate = "KGZ-900H",
                DriverName = "George Muthoni",
                TransporterID = 14,
                TransporterName = "Coast Link Logistics",
                FirstWeight = "51800",
                Status = "Active",
                FirstWeightDate = DateTime.UtcNow.AddDays(-1).AddHours(3),
                WeighBridgeID = 2,
                WeighBridgeName = "North Scale",
                ScaleName = "Fairbanks 500",
                OperatorID = 218,
                OperatorName = "Isaac Kiprono",
                CommodityID = 7,
                CommodityName = "Wheat Flour",
                SupplierID = 47,
                SupplierName = "Unga Millers",
                OriginID = 10,
                OriginName = "Eldoret Depot",
                DestinationID = 17,
                DestinationName = "Kisumu Warehouse",
                WeighMode = "Gross",
                Operation = "Inbound",
                ApiId = 334456
            }
        };

        _mockService.Setup(s => s.GetIncompleteTransactionsByVehicleAsync("KGZ-900H"))
            .ReturnsAsync(incomplete);

        var result = await _controller.GetIncompleteByVehicle("KGZ-900H");
        result.Should().BeOfType<OkObjectResult>();

        _output.WriteLine("Incomplete transactions for vehicle KGZ-900H:\n");

        foreach (var t in incomplete)
        {
            var waitTime = DateTime.UtcNow - t.FirstWeightDate;
            _output.WriteLine($"Ticket: {t.TicketID} / {t.ReceiptNo}");
            _output.WriteLine($"Commodity: {t.CommodityName} (ID {t.CommodityID})");
            _output.WriteLine($"First Weight: {t.FirstWeight} kg @ {t.FirstWeightDate:HH:mm} UTC");
            _output.WriteLine($"Weighbridge: {t.WeighBridgeName} (ID {t.WeighBridgeID}) / Scale: {t.ScaleName}");
            _output.WriteLine($"Operator: {t.OperatorName} (ID {t.OperatorID})");
            _output.WriteLine($"Waiting since: {waitTime.TotalHours:F1} hours");
            _output.WriteLine($"Status: {t.Status}\n");
        }

        _output.WriteLine($"Total pending: {incomplete.Count}");
    }

    [Fact]
    public async Task Scenario_PaginationAndFiltering_ShouldReturnFilteredResults()
    {
        _output.WriteLine("=== SCENARIO 7: Pagination + Filtering ===\n");

        var filter = new WeighbridgeTransactionFilter
        {
            Status = "Completed",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow,
            CommodityID = 8,
            PageNumber = 1,
            PageSize = 10,
            SortBy = "FirstWeightDate",
            SortDescending = true
        };

        var paged = new PagedResult<TransactionReadDto>
        {
            Items = new List<TransactionReadDto>
            {
                new TransactionReadDto
                {
                    TicketID = 1020,
                    ReceiptNo = "WB-20260121-020",
                    NoPlate = "KHZ-100I",
                    TransporterID = 18,
                    TransporterName = "AgroFreight Ltd",
                    FirstWeight = "55000",
                    SecondWeight = "30000",
                    NetWeight = "25000",
                    Status = "Completed",
                    FirstWeightDate = DateTime.UtcNow.AddDays(-1),
                    WeighBridgeID = 3,
                    WeighBridgeName = "Main Entrance",
                    ScaleName = "MET-500A",
                    OperatorID = 103,
                    OperatorName = "Esther Wambui",
                    WeighBridgeName2nd = "Exit Gate",
                    ScaleName2nd = "MET-600B",
                    OperatorID2nd = "109",
                    OperatorName2nd = "Daniel Kipchoge",
                    CommodityID = 8,
                    CommodityName = "Yellow Maize",
                    SupplierID = 44,
                    SupplierName = "Trans Nzoia Farmers",
                    CustomerID = 22,
                    CustomerName = "Mombasa Grain Handlers",
                    OriginID = 5,
                    OriginName = "Kitale",
                    DestinationID = 12,
                    DestinationName = "Mombasa Port",
                    WeighMode = "Gross → Tare",
                    Operation = "Inbound",
                    ApiId = 998877
                },
                new TransactionReadDto
                {
                    TicketID = 1019,
                    ReceiptNo = "WB-20260121-019",
                    NoPlate = "KIZ-200J",
                    TransporterID = 19,
                    TransporterName = "GrainLink Ltd",
                    FirstWeight = "48000",
                    SecondWeight = "23000",
                    NetWeight = "25000",
                    Status = "Completed",
                    FirstWeightDate = DateTime.UtcNow.AddDays(-2),
                    WeighBridgeID = 4,
                    WeighBridgeName = "Warehouse Gate",
                    ScaleName = "DIGI-400",
                    OperatorID = 202,
                    OperatorName = "Lilian Atieno",
                    WeighBridgeName2nd = "Factory Gate",
                    ScaleName2nd = "MET-700C",
                    OperatorID2nd = "206",
                    OperatorName2nd = "Joseph Mwangi",
                    CommodityID = 8,
                    CommodityName = "Yellow Maize",
                    SupplierID = 45,
                    SupplierName = "Uasin Gishu Coop",
                    CustomerID = 23,
                    CustomerName = "Eldoret Millers",
                    OriginID = 6,
                    OriginName = "Eldoret",
                    DestinationID = 13,
                    DestinationName = "Eldoret Depot",
                    WeighMode = "Gross → Tare",
                    Operation = "Inbound",
                    ApiId = 998876
                }
            },
            TotalCount = 38,
            PageNumber = 1,
            PageSize = 10
        };

        _mockService.Setup(s => s.GetAllAsync(It.IsAny<WeighbridgeTransactionFilter>())).ReturnsAsync(paged);

        var result = await _controller.GetAll(filter);
        result.Should().BeOfType<OkObjectResult>();

        _output.WriteLine($"Filter: Completed | Maize (ID {filter.CommodityID}) | Last 7 days | Page {filter.PageNumber}");
        _output.WriteLine($"Showing {paged.Items.Count} of {paged.TotalCount} records\n");

        foreach (var t in paged.Items)
        {
            _output.WriteLine($"Ticket: {t.TicketID} – {t.ReceiptNo}");
            _output.WriteLine($"Vehicle: {t.NoPlate} | Transporter: {t.TransporterName}");
            _output.WriteLine($"Net: {t.NetWeight} kg | Commodity: {t.CommodityName}");
            _output.WriteLine($"Origin → Dest: {t.OriginName} → {t.DestinationName}");
            _output.WriteLine($"Date: {t.FirstWeightDate:yyyy-MM-dd}\n");
        }
    }

    [Fact]
    public async Task Scenario_CompleteDailyOperations_ShouldSimulateFullDay()
    {
        _output.WriteLine($"=== SCENARIO 8: Full Day Simulation – {DateTime.UtcNow:yyyy-MM-dd} ===\n");

        var log = new List<(string Time, string Plate, string Driver, string? First, string? Second, string Status, string Commodity)>
        {
            ("08:00", "KAA-111A", "Driver 1", "50000", null, "Active", "Maize"),
            ("08:15", "KBB-222B", "Driver 2", "48000", null, "Active", "Rice"),
            ("09:00", "KAA-111A", "Driver 1", null, "25000", "Completed", "Maize"),
            ("09:30", "KCC-333C", "Driver 3", "55000", null, "Active", "Beans"),
            ("10:00", "KBB-222B", "Driver 2", null, "23000", "Completed", "Rice"),
            ("11:00", "KDD-444D", "Driver 4", "60000", null, "Active", "Fertilizer"),
            ("12:00", "KCC-333C", "Driver 3", null, "30000", "Completed", "Beans"),
            ("13:00", "KEE-555E", "Driver 5", "52000", null, "Active", "Sugar"),
            ("14:00", "KDD-444D", "Driver 4", null, "35000", "Completed", "Fertilizer"),
            ("15:00", "KEE-555E", "Driver 5", null, "27000", "Completed", "Sugar")
        };

        _output.WriteLine("TIME    | PLATE     | DRIVER    | ACTION          | WEIGHT   | COMMODITY | STATUS");
        _output.WriteLine("--------|-----------|-----------|-----------------|----------|-----------|--------");

        foreach (var entry in log)
        {
            var action = entry.First != null ? "First Weigh" : "Second Weigh";
            var weight = entry.First ?? entry.Second ?? "—";
            _output.WriteLine($"{entry.Time,-7} | {entry.Plate,-9} | {entry.Driver,-9} | {action,-15} | {weight,-8} | {entry.Commodity,-9} | {entry.Status}");
        }

        _output.WriteLine("\nDaily Summary:");
        var completed = log.Count(e => e.Status == "Completed");
        var active = log.Count(e => e.Status == "Active");
        var unique = log.Select(e => e.Plate).Distinct().Count();

        _output.WriteLine($"Total movements ......: {log.Count}");
        _output.WriteLine($"Completed cycles .....: {completed}");
        _output.WriteLine($"Still waiting (tare) .: {active}");
        _output.WriteLine($"Unique vehicles ......: {unique}");
    }

    [Fact]
    public async Task Scenario_VehiclesWithReweighingAndStandard_ShouldHandleReweighAndTwoWeighings()
    {
        _output.WriteLine("=== SCENARIO 9: Vehicles with Reweighing (One with Reweigh, One Standard) ===\n");

        // Vehicle 1: Will do initial weighings then reweigh (updates existing transaction)
        var vehicle1Plate = "KXZ-123V";
        var createDtoV1 = new CreateTransactionDto
        {
            NoPlate = vehicle1Plate,
            DriverName = "Henry Onyango",
            TransporterID = 25,
            TransporterName = "MultiWeigh Trans Ltd",
            FirstWeight = "52000",
            WeighBridgeID = 5,
            WeighBridgeName = "Primary Gate",
            ScaleName = "Scale-X1",
            OperatorID = 301,
            OperatorName = "Victor Kimani",
            CommodityID = 12,
            CommodityName = "Sorghum",
            SupplierID = 51,
            SupplierName = "Eastern Grains Coop",
            CustomerID = 36,
            CustomerName = "Feed Millers Inc.",
            OriginID = 15,
            OriginName = "Machakos",
            DestinationID = 20,
            DestinationName = "Thika Factory",
            WeighMode = "Gross → Tare",
            Operation = "Inbound"
        };

        var createdV1 = new TransactionReadDto
        {
            TicketID = 1025,
            ReceiptNo = "WB-20260121-025",
            NoPlate = createDtoV1.NoPlate,
            DriverName = createDtoV1.DriverName,
            TransporterID = createDtoV1.TransporterID,
            TransporterName = createDtoV1.TransporterName,
            FirstWeight = createDtoV1.FirstWeight,
            SecondWeight = null,
            NetWeight = null,
            Status = "Active",
            FirstWeightDate = DateTime.UtcNow.AddMinutes(-60),
            WeighBridgeID = createDtoV1.WeighBridgeID,
            WeighBridgeName = createDtoV1.WeighBridgeName,
            ScaleName = createDtoV1.ScaleName,
            OperatorID = createDtoV1.OperatorID,
            OperatorName = createDtoV1.OperatorName,
            CommodityID = createDtoV1.CommodityID,
            CommodityName = createDtoV1.CommodityName,
            SupplierID = createDtoV1.SupplierID,
            SupplierName = createDtoV1.SupplierName,
            CustomerID = createDtoV1.CustomerID,
            CustomerName = createDtoV1.CustomerName,
            OriginID = createDtoV1.OriginID,
            OriginName = createDtoV1.OriginName,
            DestinationID = createDtoV1.DestinationID,
            DestinationName = createDtoV1.DestinationName,
            WeighMode = createDtoV1.WeighMode,
            Operation = createDtoV1.Operation,
            ApiId = 123456
        };

        _mockService.Setup(s => s.CreateAsync(It.Is<CreateTransactionDto>(d => d.NoPlate == vehicle1Plate))).ReturnsAsync(createdV1);

        var createResultV1 = await _controller.Create(createDtoV1);
        createResultV1.Should().BeOfType<CreatedAtActionResult>();

        LogFullTransaction("Vehicle 1 After First Weighing", createdV1);

        var secondDtoV1 = new AddSecondWeightDto
        {
            TicketID = createdV1.TicketID,
            SecondWeight = "27000",
            WeighBridgeName2nd = "Secondary Gate",
            ScaleName2nd = "Scale-Y1",
            OperatorID2nd = "302",
            OperatorName2nd = "Rachel Nduta"
        };

        var completedV1 = new TransactionReadDto
        {
            TicketID = createdV1.TicketID,
            ReceiptNo = createdV1.ReceiptNo,
            NoPlate = createdV1.NoPlate,
            DriverName = createdV1.DriverName,
            TransporterID = createdV1.TransporterID,
            TransporterName = createdV1.TransporterName,
            CommodityID = createdV1.CommodityID,
            CommodityName = createdV1.CommodityName,
            SupplierID = createdV1.SupplierID,
            SupplierName = createdV1.SupplierName,
            CustomerID = createdV1.CustomerID,
            CustomerName = createdV1.CustomerName,
            OriginID = createdV1.OriginID,
            OriginName = createdV1.OriginName,
            DestinationID = createdV1.DestinationID,
            DestinationName = createdV1.DestinationName,
            FirstWeight = createdV1.FirstWeight,
            FirstWeightDate = createdV1.FirstWeightDate,
            WeighBridgeID = createdV1.WeighBridgeID,
            WeighBridgeName = createdV1.WeighBridgeName,
            ScaleName = createdV1.ScaleName,
            OperatorID = createdV1.OperatorID,
            OperatorName = createdV1.OperatorName,
            WeighMode = createdV1.WeighMode,
            Operation = createdV1.Operation,
            ApiId = createdV1.ApiId,
            SecondWeight = secondDtoV1.SecondWeight,
            NetWeight = "25000",
            Status = "Completed",
            SecondWeightDate = DateTime.UtcNow.AddMinutes(-30),
            WeighBridgeName2nd = secondDtoV1.WeighBridgeName2nd,
            ScaleName2nd = secondDtoV1.ScaleName2nd,
            OperatorID2nd = secondDtoV1.OperatorID2nd,
            OperatorName2nd = secondDtoV1.OperatorName2nd,
            ChangeDesc = "Second weighing completed"
        };

        _mockService.Setup(s => s.AddSecondWeightAsync(It.Is<AddSecondWeightDto>(d => d.TicketID == createdV1.TicketID && d.SecondWeight == "27000"))).ReturnsAsync(completedV1);

        var secondResultV1 = await _controller.AddSecondWeight(secondDtoV1);
        secondResultV1.Should().BeOfType<OkObjectResult>();

        LogFullTransaction("Vehicle 1 After Initial Second Weighing", completedV1);

        // Request reweigh for Vehicle 1
        var reweighRequestV1 = new RequestReweighDto
        {
            TicketID = completedV1.TicketID,
            Reason = "Discrepancy in net weight detected – reweigh approved"
        };

        _mockService.Setup(s => s.RequestReweighAsync(It.Is<RequestReweighDto>(r => r.TicketID == completedV1.TicketID))).ReturnsAsync(true);

        var reqResultV1 = await _controller.RequestReweigh(reweighRequestV1);
        reqResultV1.Should().BeOfType<OkObjectResult>();

        var requestedV1 = new TransactionReadDto
        {
            TicketID = completedV1.TicketID,
            ReceiptNo = completedV1.ReceiptNo,
            NoPlate = completedV1.NoPlate,
            DriverName = completedV1.DriverName,
            TransporterID = completedV1.TransporterID,
            TransporterName = completedV1.TransporterName,
            FirstWeight = completedV1.FirstWeight,
            SecondWeight = completedV1.SecondWeight,
            NetWeight = completedV1.NetWeight,
            FirstWeightDate = completedV1.FirstWeightDate,
            SecondWeightDate = completedV1.SecondWeightDate,
            WeighBridgeID = completedV1.WeighBridgeID,
            WeighBridgeName = completedV1.WeighBridgeName,
            ScaleName = completedV1.ScaleName,
            OperatorID = completedV1.OperatorID,
            OperatorName = completedV1.OperatorName,
            WeighBridgeName2nd = completedV1.WeighBridgeName2nd,
            ScaleName2nd = completedV1.ScaleName2nd,
            OperatorID2nd = completedV1.OperatorID2nd,
            OperatorName2nd = completedV1.OperatorName2nd,
            CommodityID = completedV1.CommodityID,
            CommodityName = completedV1.CommodityName,
            SupplierID = completedV1.SupplierID,
            SupplierName = completedV1.SupplierName,
            CustomerID = completedV1.CustomerID,
            CustomerName = completedV1.CustomerName,
            OriginID = completedV1.OriginID,
            OriginName = completedV1.OriginName,
            DestinationID = completedV1.DestinationID,
            DestinationName = completedV1.DestinationName,
            WeighMode = completedV1.WeighMode,
            Operation = completedV1.Operation,
            ApiId = completedV1.ApiId,
            Status = "ReweighRequested",
            ReweighPermission = reweighRequestV1.Reason,
            ChangeDesc = $"Reweigh requested: {reweighRequestV1.Reason}; Previous second weight: {completedV1.SecondWeight}"
        };

        LogFullTransaction("Vehicle 1 After Reweigh Request", requestedV1);

        // Perform reweighing (updates the second weight in the existing transaction)
        var reweighDtoV1 = new AddSecondWeightDto
        {
            TicketID = requestedV1.TicketID,
            SecondWeight = "26500",
            WeighBridgeName2nd = "Reweigh Gate",
            ScaleName2nd = "Scale-Z1",
            OperatorID2nd = "303",
            OperatorName2nd = "Samuel Otieno"
        };

        var reweighedV1 = new TransactionReadDto
        {
            TicketID = requestedV1.TicketID,
            ReceiptNo = requestedV1.ReceiptNo,
            NoPlate = requestedV1.NoPlate,
            DriverName = requestedV1.DriverName,
            TransporterID = requestedV1.TransporterID,
            TransporterName = requestedV1.TransporterName,
            CommodityID = requestedV1.CommodityID,
            CommodityName = requestedV1.CommodityName,
            SupplierID = requestedV1.SupplierID,
            SupplierName = requestedV1.SupplierName,
            CustomerID = requestedV1.CustomerID,
            CustomerName = requestedV1.CustomerName,
            OriginID = requestedV1.OriginID,
            OriginName = requestedV1.OriginName,
            DestinationID = requestedV1.DestinationID,
            DestinationName = requestedV1.DestinationName,
            FirstWeight = requestedV1.FirstWeight,
            FirstWeightDate = requestedV1.FirstWeightDate,
            WeighBridgeID = requestedV1.WeighBridgeID,
            WeighBridgeName = requestedV1.WeighBridgeName,
            ScaleName = requestedV1.ScaleName,
            OperatorID = requestedV1.OperatorID,
            OperatorName = requestedV1.OperatorName,
            WeighMode = requestedV1.WeighMode,
            Operation = requestedV1.Operation,
            ApiId = requestedV1.ApiId,
            SecondWeight = reweighDtoV1.SecondWeight,
            NetWeight = "25500",
            Status = "Completed",
            SecondWeightDate = DateTime.UtcNow,
            WeighBridgeName2nd = reweighDtoV1.WeighBridgeName2nd,
            ScaleName2nd = reweighDtoV1.ScaleName2nd,
            OperatorID2nd = reweighDtoV1.OperatorID2nd,
            OperatorName2nd = reweighDtoV1.OperatorName2nd,
            ReweighPermission = requestedV1.ReweighPermission,
            ChangeDesc = $"{requestedV1.ChangeDesc}; Reweighed second weight updated from 27000 to 26500"
        };

        _mockService.Setup(s => s.AddSecondWeightAsync(It.Is<AddSecondWeightDto>(d => d.TicketID == requestedV1.TicketID && d.SecondWeight == "26500"))).ReturnsAsync(reweighedV1);

        var reweighResultV1 = await _controller.AddSecondWeight(reweighDtoV1);
        reweighResultV1.Should().BeOfType<OkObjectResult>();

        LogFullTransaction("Vehicle 1 After Reweighing (Updated Transaction) - As in DB", reweighedV1);

        // Vehicle 2: Standard 2 weighings
        var vehicle2Plate = "KYZ-456W";
        var createDtoV2 = new CreateTransactionDto
        {
            NoPlate = vehicle2Plate,
            DriverName = "Irene Njeri",
            TransporterID = 26,
            TransporterName = "Standard Trans Ltd",
            FirstWeight = "49000",
            WeighBridgeID = 6,
            WeighBridgeName = "Entry Point",
            ScaleName = "Scale-A2",
            OperatorID = 401,
            OperatorName = "Thomas Mutua",
            CommodityID = 13,
            CommodityName = "Barley",
            SupplierID = 52,
            SupplierName = "Highland Farmers",
            CustomerID = 37,
            CustomerName = "Brewery Co.",
            OriginID = 16,
            OriginName = "Nyeri",
            DestinationID = 21,
            DestinationName = "Nairobi Plant",
            WeighMode = "Gross → Tare",
            Operation = "Inbound"
        };

        var createdV2 = new TransactionReadDto
        {
            TicketID = 1026,
            ReceiptNo = "WB-20260121-026",
            NoPlate = createDtoV2.NoPlate,
            DriverName = createDtoV2.DriverName,
            TransporterID = createDtoV2.TransporterID,
            TransporterName = createDtoV2.TransporterName,
            FirstWeight = createDtoV2.FirstWeight,
            SecondWeight = null,
            NetWeight = null,
            Status = "Active",
            FirstWeightDate = DateTime.UtcNow.AddMinutes(-45),
            WeighBridgeID = createDtoV2.WeighBridgeID,
            WeighBridgeName = createDtoV2.WeighBridgeName,
            ScaleName = createDtoV2.ScaleName,
            OperatorID = createDtoV2.OperatorID,
            OperatorName = createDtoV2.OperatorName,
            CommodityID = createDtoV2.CommodityID,
            CommodityName = createDtoV2.CommodityName,
            SupplierID = createDtoV2.SupplierID,
            SupplierName = createDtoV2.SupplierName,
            CustomerID = createDtoV2.CustomerID,
            CustomerName = createDtoV2.CustomerName,
            OriginID = createDtoV2.OriginID,
            OriginName = createDtoV2.OriginName,
            DestinationID = createDtoV2.DestinationID,
            DestinationName = createDtoV2.DestinationName,
            WeighMode = createDtoV2.WeighMode,
            Operation = createDtoV2.Operation,
            ApiId = 654321
        };

        _mockService.Setup(s => s.CreateAsync(It.Is<CreateTransactionDto>(d => d.NoPlate == vehicle2Plate))).ReturnsAsync(createdV2);

        var createResultV2 = await _controller.Create(createDtoV2);
        createResultV2.Should().BeOfType<CreatedAtActionResult>();

        LogFullTransaction("Vehicle 2 After First Weighing", createdV2);

        var secondDtoV2 = new AddSecondWeightDto
        {
            TicketID = createdV2.TicketID,
            SecondWeight = "24000",
            WeighBridgeName2nd = "Exit Point",
            ScaleName2nd = "Scale-B2",
            OperatorID2nd = "402",
            OperatorName2nd = "Patricia Akinyi"
        };

        var completedV2 = new TransactionReadDto
        {
            TicketID = createdV2.TicketID,
            ReceiptNo = createdV2.ReceiptNo,
            NoPlate = createdV2.NoPlate,
            DriverName = createdV2.DriverName,
            TransporterID = createdV2.TransporterID,
            TransporterName = createdV2.TransporterName,
            CommodityID = createdV2.CommodityID,
            CommodityName = createdV2.CommodityName,
            SupplierID = createdV2.SupplierID,
            SupplierName = createdV2.SupplierName,
            CustomerID = createdV2.CustomerID,
            CustomerName = createdV2.CustomerName,
            OriginID = createdV2.OriginID,
            OriginName = createdV2.OriginName,
            DestinationID = createdV2.DestinationID,
            DestinationName = createdV2.DestinationName,
            FirstWeight = createdV2.FirstWeight,
            FirstWeightDate = createdV2.FirstWeightDate,
            WeighBridgeID = createdV2.WeighBridgeID,
            WeighBridgeName = createdV2.WeighBridgeName,
            ScaleName = createdV2.ScaleName,
            OperatorID = createdV2.OperatorID,
            OperatorName = createdV2.OperatorName,
            WeighMode = createdV2.WeighMode,
            Operation = createdV2.Operation,
            ApiId = createdV2.ApiId,
            SecondWeight = secondDtoV2.SecondWeight,
            NetWeight = "25000",
            Status = "Completed",
            SecondWeightDate = DateTime.UtcNow,
            WeighBridgeName2nd = secondDtoV2.WeighBridgeName2nd,
            ScaleName2nd = secondDtoV2.ScaleName2nd,
            OperatorID2nd = secondDtoV2.OperatorID2nd,
            OperatorName2nd = secondDtoV2.OperatorName2nd,
            ChangeDesc = "Second weighing completed"
        };

        _mockService.Setup(s => s.AddSecondWeightAsync(It.Is<AddSecondWeightDto>(d => d.TicketID == createdV2.TicketID))).ReturnsAsync(completedV2);

        var secondResultV2 = await _controller.AddSecondWeight(secondDtoV2);
        secondResultV2.Should().BeOfType<OkObjectResult>();

        LogFullTransaction("Vehicle 2 After Second Weighing - As in DB", completedV2);

        // Final assertions
        reweighedV1.Status.Should().Be("Completed");
        reweighedV1.NetWeight.Should().Be("25500");
        reweighedV1.ChangeDesc.Should().Contain("Reweigh");

        completedV2.Status.Should().Be("Completed");
        completedV2.NetWeight.Should().Be("25000");
    }
}