using DataSyncService.Core.Entities;
using DataSyncService.Core.Enums;
using FluentAssertions;

namespace DataSyncService.Tests;

public class SyncSessionTests
{
    [Fact]
    public void SyncSession_Should_Calculate_Progress_Percentage_Correctly()
    {
        // Arrange
        var syncSession = new SyncSession
        {
            TotalRecords = 100,
            ProcessedRecords = 50
        };

        // Act
        var progressPercentage = syncSession.ProgressPercentage;

        // Assert
        progressPercentage.Should().Be(50);
    }

    [Fact]
    public void SyncSession_Should_Calculate_Duration_When_EndTime_Is_Set()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMinutes(5);
        
        var syncSession = new SyncSession
        {
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var duration = syncSession.Duration;

        // Assert
        duration.Should().NotBeNull();
        duration?.TotalMinutes.Should().Be(5);
    }

    [Fact]
    public void SyncSession_Should_Return_Null_Duration_When_EndTime_Is_Not_Set()
    {
        // Arrange
        var syncSession = new SyncSession
        {
            StartTime = DateTime.UtcNow
        };

        // Act
        var duration = syncSession.Duration;

        // Assert
        duration.Should().BeNull();
    }

    [Fact]
    public void SyncSession_Should_Return_Zero_Progress_When_No_Total_Records()
    {
        // Arrange
        var syncSession = new SyncSession
        {
            TotalRecords = 0,
            ProcessedRecords = 10
        };

        // Act
        var progressPercentage = syncSession.ProgressPercentage;

        // Assert
        progressPercentage.Should().Be(0);
    }

    [Fact]
    public void SyncSession_Should_Initialize_With_Correct_Defaults()
    {
        // Arrange & Act
        var syncSession = new SyncSession();

        // Assert
        syncSession.SyncLogs.Should().NotBeNull();
        syncSession.ChangeRecords.Should().NotBeNull();
        syncSession.SyncConflicts.Should().NotBeNull();
        syncSession.SessionId.Should().BeEmpty();
        syncSession.Status.Should().Be(default(SyncStatus));
    }
}