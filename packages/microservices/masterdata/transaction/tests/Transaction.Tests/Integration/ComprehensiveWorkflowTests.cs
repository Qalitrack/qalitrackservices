using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Core.DTOs;
using Transaction.Infrastructure.Data;
using Xunit;

namespace Transaction.Tests.Integration;

/// <summary>
/// Comprehensive integration tests for weighbridge transaction workflows
/// Tests basic operations, validations, and reweigh workflows
/// </summary>
public class ComprehensiveWorkflowTests : IClassFixture<TestServerFactory>, IDisposable
{
    private readonly TestServerFactory _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly TransactionDbContext _context;

    public ComprehensiveWorkflowTests(TestServerFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _scope.Dispose();
        _client.Dispose();
    }

    private async Task<T?> UnwrapResponse<T>(HttpResponseMessage response)
    {
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponseDto<T>>();
        return wrapper != null ? wrapper.Data : default;
    }

    #region Basic Workflow Tests

    [Fact]
    public async Task BasicWorkflow_CreateAndCompleteTransaction_ShouldCalculateTurnaroundTime()
    {
        // Create transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "ABC-123",
            DriverName = "John Doe",
            FirstWeight = "45000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        created.Should().NotBeNull();
        created!.Status.Should().Be("Active");
        created.FirstWeight.Should().Be("45000");
        created.SecondWeightDate.Should().BeNull();
        created.TurnaroundTime.Should().BeNull();

        // Add second weight
        var secondWeightDto = new AddSecondWeightDto
        {
            TicketID = created.TicketID,
            SecondWeight = "15000",
            WeighBridgeName2nd = "WB-2",
            OperatorName2nd = "Operator Bob"
        };

        var secondWeightResponse = await _client.PostAsJsonAsync("/Transaction/add-second-weight", secondWeightDto);
        secondWeightResponse.EnsureSuccessStatusCode();
        var completed = await UnwrapResponse<TransactionReadDto>(secondWeightResponse);

        // Verify completion
        completed.Should().NotBeNull();
        completed!.Status.Should().Be("Completed");
        completed.SecondWeight.Should().Be("15000");
        completed.NetWeight.Should().Be("30000.00");
        completed.SecondWeightDate.Should().NotBeNull();
        completed.TurnaroundTime.Should().NotBeNull();
        completed.TurnaroundTime!.Value.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);

        Console.WriteLine($"✓ Transaction completed with turnaround time: {completed.TurnaroundTime}");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Validation_CreateWithNegativeWeight_ShouldFail()
    {
        var createDto = new CreateTransactionDto
        {
            NoPlate = "NEG-123",
            DriverName = "Bad Driver",
            FirstWeight = "-100",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var response = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("negative");
    }

    [Fact]
    public async Task Validation_AddSecondWeightToCompleted_ShouldFail()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "DUP-123",
            DriverName = "Test Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        // Add second weight
        var secondWeightDto = new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "10000"
        };
        await _client.PostAsJsonAsync("/Transaction/add-second-weight", secondWeightDto);

        // Try to add second weight again - should fail
        var duplicateDto = new AddSecondWeightDto
        {
            TicketID = created.TicketID,
            SecondWeight = "12000"
        };

        var response = await _client.PostAsJsonAsync("/Transaction/add-second-weight", duplicateDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("completed transaction");
    }

    [Fact]
    public async Task Validation_UpdateCompletedTransaction_ShouldFail()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "UPD-123",
            DriverName = "Test Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "10000"
        });

        // Try to update completed transaction - should fail
        var updateDto = new UpdateTransactionDto
        {
            DriverName = "New Driver"
        };

        var response = await _client.PutAsJsonAsync($"/Transaction/{created.TicketID}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Cannot update a completed transaction");
    }

    [Fact]
    public async Task Validation_DeleteCompletedTransaction_ShouldFail()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "DEL-123",
            DriverName = "Test Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "10000"
        });

        // Try to delete completed transaction - should fail
        var response = await _client.DeleteAsync($"/Transaction/{created.TicketID}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Cannot delete a completed transaction");
    }

    #endregion

    #region Reweigh Workflow Tests

    [Fact]
    public async Task ReweighWorkflow_RequestAndApprove_ShouldResetToActive()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "RWA-123",
            DriverName = "Reweigh Driver",
            FirstWeight = "45000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "15000"
        });

        // Request reweigh
        var requestDto = new RequestReweighDto
        {
            TicketID = created.TicketID,
            Reason = "Weight discrepancy detected"
        };

        var requestResponse = await _client.PostAsJsonAsync("/Transaction/request-reweigh", requestDto);
        requestResponse.EnsureSuccessStatusCode();

        // Verify status changed to ReweighRequested
        var getResponse = await _client.GetAsync($"/Transaction/{created.TicketID}");
        var requested = await UnwrapResponse<TransactionReadDto>(getResponse);
        requested!.Status.Should().Be("ReweighRequested");

        // Approve reweigh
        var approveDto = new ApproveReweighDto
        {
            TicketID = created.TicketID,
            ApprovedBy = "Supervisor Jane",
            Notes = "Approved for re-weighing"
        };

        var approveResponse = await _client.PostAsJsonAsync("/Transaction/approve-reweigh", approveDto);
        approveResponse.EnsureSuccessStatusCode();
        var approved = await UnwrapResponse<TransactionReadDto>(approveResponse);

        // Verify transaction reset to Active
        approved!.Status.Should().Be("Active");
        approved.SecondWeight.Should().BeNull();
        approved.NetWeight.Should().BeNull();
        approved.TurnaroundTime.Should().BeNull();
        approved.SecondWeightDate.Should().BeNull();

        // Verify reweigh records created
        var recordsResponse = await _client.GetAsync($"/Transaction/{created.TicketID}/reweigh-records");
        recordsResponse.EnsureSuccessStatusCode();
        var records = await UnwrapResponse<List<ReweighRecordDto>>(recordsResponse);
        records.Should().HaveCount(2);
        records!.Should().Contain(r => r.Status == "Pending");
        records.Should().Contain(r => r.Status == "Approved");

        Console.WriteLine($"✓ Reweigh approved, transaction reset to Active for re-weighing");

        // Re-weigh the vehicle
        var newSecondWeight = new AddSecondWeightDto
        {
            TicketID = created.TicketID,
            SecondWeight = "16000"
        };

        var reweighResponse = await _client.PostAsJsonAsync("/Transaction/add-second-weight", newSecondWeight);
        reweighResponse.EnsureSuccessStatusCode();
        var reweighed = await UnwrapResponse<TransactionReadDto>(reweighResponse);

        // Verify new completion
        reweighed!.Status.Should().Be("Completed");
        reweighed.SecondWeight.Should().Be("16000");
        reweighed.NetWeight.Should().Be("29000.00");
        reweighed.TurnaroundTime.Should().NotBeNull();

        Console.WriteLine($"✓ Transaction re-weighed successfully with new net weight: {reweighed.NetWeight}kg");
    }

    [Fact]
    public async Task ReweighWorkflow_RequestAndReject_ShouldRestoreToCompleted()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "RWR-123",
            DriverName = "Reject Driver",
            FirstWeight = "42000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        var originalSecondWeight = "12000";
        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = originalSecondWeight
        });

        // Request reweigh
        var requestDto = new RequestReweighDto
        {
            TicketID = created.TicketID,
            Reason = "Driver complaint"
        };

        await _client.PostAsJsonAsync("/Transaction/request-reweigh", requestDto);

        // Reject reweigh
        var rejectDto = new RejectReweighDto
        {
            TicketID = created.TicketID,
            RejectionReason = "No evidence of error",
            RejectedBy = "Supervisor John",
            Notes = "Weights verified, no discrepancy found"
        };

        var rejectResponse = await _client.PostAsJsonAsync("/Transaction/reject-reweigh", rejectDto);
        rejectResponse.EnsureSuccessStatusCode();
        var rejected = await UnwrapResponse<TransactionReadDto>(rejectResponse);

        // Verify transaction restored to Completed with original weights
        rejected!.Status.Should().Be("Completed");
        rejected.SecondWeight.Should().Be(originalSecondWeight);
        rejected.NetWeight.Should().Be("30000.00");

        // Verify reweigh records
        var recordsResponse = await _client.GetAsync($"/Transaction/{created.TicketID}/reweigh-records");
        var records = await UnwrapResponse<List<ReweighRecordDto>>(recordsResponse);
        records.Should().HaveCount(2);
        records!.Should().Contain(r => r.Status == "Rejected");

        Console.WriteLine($"✓ Reweigh rejected, original weights preserved");
    }

    [Fact]
    public async Task ReweighWorkflow_RequestDuplicate_ShouldFail()
    {
        // Create and complete a transaction
        var createDto = new CreateTransactionDto
        {
            NoPlate = "DRP-123",
            DriverName = "Duplicate Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "10000"
        });

        // Request reweigh first time
        var requestDto = new RequestReweighDto
        {
            TicketID = created.TicketID,
            Reason = "First request"
        };
        await _client.PostAsJsonAsync("/Transaction/request-reweigh", requestDto);

        // Try to request again - should fail
        var duplicateDto = new RequestReweighDto
        {
            TicketID = created.TicketID,
            Reason = "Second request"
        };

        var response = await _client.PostAsJsonAsync("/Transaction/request-reweigh", duplicateDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already been requested");
    }

    [Fact]
    public async Task ReweighWorkflow_ApproveNonRequested_ShouldFail()
    {
        // Create and complete a transaction (without requesting reweigh)
        var createDto = new CreateTransactionDto
        {
            NoPlate = "ANR-123",
            DriverName = "Test Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var createRequest = new { Request = createDto };
        var createResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", createRequest);
        var created = await UnwrapResponse<TransactionReadDto>(createResponse);

        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = created!.TicketID,
            SecondWeight = "10000"
        });

        // Try to approve reweigh without requesting first - should fail
        var approveDto = new ApproveReweighDto
        {
            TicketID = created.TicketID,
            ApprovedBy = "Supervisor"
        };

        var response = await _client.PostAsJsonAsync("/Transaction/approve-reweigh", approveDto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("ReweighRequested");
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task Query_GetByStatus_ShouldReturnCorrectTransactions()
    {
        // Create two transactions - one complete, one incomplete
        var incompleteDto = new CreateTransactionDto
        {
            NoPlate = "INC-123",
            DriverName = "Incomplete Driver",
            FirstWeight = "40000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var completeDto = new CreateTransactionDto
        {
            NoPlate = "CMP-123",
            DriverName = "Complete Driver",
            FirstWeight = "45000",
            TransporterID = Guid.NewGuid().ToString(),
            TransporterName = "Test Transport"
        };

        var incRequest = new { Request = incompleteDto };
        var cmpRequest = new { Request = completeDto };

        var incResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", incRequest);
        var incCreated = await UnwrapResponse<TransactionReadDto>(incResponse);

        var cmpResponse = await _client.PostAsJsonAsync("/Transaction/Transaction", cmpRequest);
        var cmpCreated = await UnwrapResponse<TransactionReadDto>(cmpResponse);

        // Complete one transaction
        await _client.PostAsJsonAsync("/Transaction/add-second-weight", new AddSecondWeightDto
        {
            TicketID = cmpCreated!.TicketID,
            SecondWeight = "15000"
        });

        // Query by status
        var activeResponse = await _client.GetAsync("/Transaction/status/Active");
        activeResponse.EnsureSuccessStatusCode();
        var activeTransactions = await UnwrapResponse<List<TransactionReadDto>>(activeResponse);

        activeTransactions.Should().Contain(t => t.TicketID == incCreated!.TicketID);
        activeTransactions.Should().NotContain(t => t.TicketID == cmpCreated.TicketID);

        Console.WriteLine($"✓ Query returned {activeTransactions!.Count} active (incomplete) transactions");
    }

    #endregion
}
