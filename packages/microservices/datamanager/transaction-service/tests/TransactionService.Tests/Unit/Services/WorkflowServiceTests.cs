using AutoMapper;
using FluentAssertions;
using Moq;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Core.Services;
using Xunit;

namespace TransactionService.Tests.Unit.Services;

public class WorkflowServiceTests
{
    private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly WorkflowService _workflowService;

    public WorkflowServiceTests()
    {
        _workflowRepositoryMock = new Mock<IWorkflowRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _mapperMock = new Mock<IMapper>();

        _workflowService = new WorkflowService(
            _workflowRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task InitializeWorkflowAsync_ValidTransactionId_CreatesAllWorkflowSteps()
    {
        // Arrange
        var transactionId = "TXN001";
        var expectedSteps = new List<TransactionWorkflow>();

        _workflowRepositoryMock.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TransactionWorkflow>>()))
            .Callback<IEnumerable<TransactionWorkflow>>(steps => expectedSteps.AddRange(steps))
            .ReturnsAsync((IEnumerable<TransactionWorkflow> steps) => steps);

        // Act
        await _workflowService.InitializeWorkflowAsync(transactionId);

        // Assert
        expectedSteps.Should().HaveCount(7); // 8 total steps minus DisputeResolution
        expectedSteps.Should().Contain(s => s.WorkflowStep == WorkflowStep.VehicleArrival && s.Status == StepStatus.InProgress);
        expectedSteps.Should().Contain(s => s.WorkflowStep == WorkflowStep.DocumentCheck && s.Status == StepStatus.NotStarted);
        expectedSteps.Should().Contain(s => s.WorkflowStep == WorkflowStep.Completion && s.Status == StepStatus.NotStarted);
        expectedSteps.Should().NotContain(s => s.WorkflowStep == WorkflowStep.DisputeResolution);
    }

    [Fact]
    public async Task AdvanceWorkflowAsync_ValidCurrentStep_CompletesCurrentAndStartsNext()
    {
        // Arrange
        var transactionId = "TXN001";
        var currentStep = new TransactionWorkflow
        {
            Id = "WF001",
            TransactionId = transactionId,
            WorkflowStep = WorkflowStep.VehicleArrival,
            Status = StepStatus.InProgress
        };

        var nextStep = new TransactionWorkflow
        {
            Id = "WF002",
            TransactionId = transactionId,
            WorkflowStep = WorkflowStep.DocumentCheck,
            Status = StepStatus.NotStarted
        };

        _workflowRepositoryMock.Setup(x => x.GetCurrentStepAsync(transactionId))
            .ReturnsAsync(currentStep);
        _workflowRepositoryMock.Setup(x => x.GetByTransactionAndStepAsync(transactionId, WorkflowStep.DocumentCheck))
            .ReturnsAsync(nextStep);
        _workflowRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()))
            .ReturnsAsync((TransactionWorkflow w) => w);

        // Act
        var result = await _workflowService.AdvanceWorkflowAsync(transactionId, "TestUser", "Test notes");

        // Assert
        result.Should().BeTrue();
        currentStep.Status.Should().Be(StepStatus.Completed);
        currentStep.ProcessedBy.Should().Be("TestUser");
        currentStep.Notes.Should().Be("Test notes");
        currentStep.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        nextStep.Status.Should().Be(StepStatus.InProgress);
        nextStep.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _workflowRepositoryMock.Verify(x => x.UpdateAsync(currentStep), Times.Once);
        _workflowRepositoryMock.Verify(x => x.UpdateAsync(nextStep), Times.Once);
    }

    [Fact]
    public async Task AdvanceWorkflowAsync_LastStep_CompletesTransactionAndWorkflow()
    {
        // Arrange
        var transactionId = "TXN001";
        var currentStep = new TransactionWorkflow
        {
            Id = "WF007",
            TransactionId = transactionId,
            WorkflowStep = WorkflowStep.Completion,
            Status = StepStatus.InProgress
        };

        var transaction = new WeighingTransaction
        {
            Id = transactionId,
            Status = TransactionStatus.InProgress
        };

        _workflowRepositoryMock.Setup(x => x.GetCurrentStepAsync(transactionId))
            .ReturnsAsync(currentStep);
        _workflowRepositoryMock.Setup(x => x.GetByTransactionAndStepAsync(transactionId, It.IsAny<WorkflowStep>()))
            .ReturnsAsync((TransactionWorkflow?)null); // No next step
        _transactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId))
            .ReturnsAsync(transaction);
        _workflowRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()))
            .ReturnsAsync((TransactionWorkflow w) => w);
        _transactionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<WeighingTransaction>()))
            .ReturnsAsync((WeighingTransaction t) => t);

        // Act
        var result = await _workflowService.AdvanceWorkflowAsync(transactionId);

        // Assert
        result.Should().BeTrue();
        currentStep.Status.Should().Be(StepStatus.Completed);
        transaction.Status.Should().Be(TransactionStatus.Completed);

        _workflowRepositoryMock.Verify(x => x.UpdateAsync(currentStep), Times.Once);
        _transactionRepositoryMock.Verify(x => x.UpdateAsync(transaction), Times.Once);
    }

    [Fact]
    public async Task AdvanceWorkflowAsync_NoCurrentStep_ReturnsFalse()
    {
        // Arrange
        var transactionId = "TXN001";
        _workflowRepositoryMock.Setup(x => x.GetCurrentStepAsync(transactionId))
            .ReturnsAsync((TransactionWorkflow?)null);

        // Act
        var result = await _workflowService.AdvanceWorkflowAsync(transactionId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetWorkflowAsync_ValidTransaction_ResetsAllStepsAndStartsFirst()
    {
        // Arrange
        var transactionId = "TXN001";
        var reason = "Reset for testing";
        var steps = new List<TransactionWorkflow>
        {
            new() { Id = "WF001", WorkflowStep = WorkflowStep.VehicleArrival, Order = 1, Status = StepStatus.Completed },
            new() { Id = "WF002", WorkflowStep = WorkflowStep.DocumentCheck, Order = 2, Status = StepStatus.InProgress },
            new() { Id = "WF003", WorkflowStep = WorkflowStep.EntryWeighing, Order = 3, Status = StepStatus.NotStarted }
        };

        _workflowRepositoryMock.Setup(x => x.GetByTransactionIdAsync(transactionId))
            .ReturnsAsync(steps);
        _workflowRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()))
            .ReturnsAsync((TransactionWorkflow w) => w);

        // Act
        var result = await _workflowService.ResetWorkflowAsync(transactionId, reason);

        // Assert
        result.Should().BeTrue();
        
        steps[0].Status.Should().Be(StepStatus.InProgress);
        steps[0].StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        steps[1].Status.Should().Be(StepStatus.NotStarted);
        steps[1].StartedAt.Should().BeNull();
        steps[1].CompletedAt.Should().BeNull();
        steps[1].Notes.Should().Be(reason);
        
        steps[2].Status.Should().Be(StepStatus.NotStarted);
        steps[2].Notes.Should().Be(reason);

        _workflowRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()), Times.Exactly(steps.Count * 2)); // Once for reset, once for first step
    }

    [Fact]
    public async Task SkipStepAsync_ValidStep_MarksStepAsSkipped()
    {
        // Arrange
        var transactionId = "TXN001";
        var step = WorkflowStep.DocumentCheck;
        var reason = "Document not required";
        var workflowStep = new TransactionWorkflow
        {
            Id = "WF002",
            TransactionId = transactionId,
            WorkflowStep = step,
            Status = StepStatus.NotStarted
        };

        _workflowRepositoryMock.Setup(x => x.GetByTransactionAndStepAsync(transactionId, step))
            .ReturnsAsync(workflowStep);
        _workflowRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()))
            .ReturnsAsync((TransactionWorkflow w) => w);

        // Act
        var result = await _workflowService.SkipStepAsync(transactionId, step, reason);

        // Assert
        result.Should().BeTrue();
        workflowStep.Status.Should().Be(StepStatus.Skipped);
        workflowStep.Notes.Should().Be(reason);
        workflowStep.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _workflowRepositoryMock.Verify(x => x.UpdateAsync(workflowStep), Times.Once);
    }

    [Fact]
    public async Task UpdateStepAsync_ValidStep_UpdatesStepStatus()
    {
        // Arrange
        var transactionId = "TXN001";
        var step = WorkflowStep.EntryWeighing;
        var status = StepStatus.RequiresAttention;
        var processedBy = "TestUser";
        var notes = "Weight seems incorrect";
        
        var workflowStep = new TransactionWorkflow
        {
            Id = "WF003",
            TransactionId = transactionId,
            WorkflowStep = step,
            Status = StepStatus.InProgress
        };

        _workflowRepositoryMock.Setup(x => x.GetByTransactionAndStepAsync(transactionId, step))
            .ReturnsAsync(workflowStep);
        _workflowRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TransactionWorkflow>()))
            .ReturnsAsync((TransactionWorkflow w) => w);

        // Act
        var result = await _workflowService.UpdateStepAsync(transactionId, step, status, processedBy, notes);

        // Assert
        result.Should().BeTrue();
        workflowStep.Status.Should().Be(status);
        workflowStep.ProcessedBy.Should().Be(processedBy);
        workflowStep.Notes.Should().Be(notes);

        _workflowRepositoryMock.Verify(x => x.UpdateAsync(workflowStep), Times.Once);
    }
}