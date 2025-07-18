using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QaliTrackGateway.Configuration;
using Xunit;

namespace QaliTrackGateway.Tests.UnitTests;

public class ConfigurationTests
{
    [Fact]
    public void AuditSettings_ShouldLoadFromConfiguration()
    {
        // Arrange
        var configurationData = new Dictionary<string, string>
        {
            ["Audit:EnableAuditLogging"] = "true",
            ["Audit:TransactionServiceBaseUrl"] = "http://test-transaction-service",
            ["Audit:AuditEndpoint"] = "/api/audit/test",
            ["Audit:BatchAuditEndpoint"] = "/api/audit/test/batch",
            ["Audit:EnableBatchProcessing"] = "true",
            ["Audit:BatchSize"] = "100",
            ["Audit:BatchIntervalSeconds"] = "60",
            ["Audit:HttpTimeoutSeconds"] = "45",
            ["Audit:RetryAttempts"] = "5",
            ["Audit:RetryDelayMs"] = "2000",
            ["Audit:EnableHealthCheckAudit"] = "true",
            ["Audit:EnableAuthorizationAudit"] = "true",
            ["Audit:EnableGatewayRequestAudit"] = "true",
            ["Audit:MinimumLogLevel"] = "Debug",
            ["Audit:EnableCorrelationId"] = "true",
            ["Audit:EnableEventFiltering"] = "true",
            ["Audit:ExcludePaths"] = "/health,/metrics,/swagger",
            ["Audit:IncludeHttpMethods"] = "GET,POST"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<AuditSettings>(configuration.GetSection(AuditSettings.SectionName));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var auditSettings = serviceProvider.GetRequiredService<IOptions<AuditSettings>>().Value;

        // Assert
        Assert.True(auditSettings.EnableAuditLogging);
        Assert.Equal("http://test-transaction-service", auditSettings.TransactionServiceBaseUrl);
        Assert.Equal("/api/audit/test", auditSettings.AuditEndpoint);
        Assert.Equal("/api/audit/test/batch", auditSettings.BatchAuditEndpoint);
        Assert.True(auditSettings.EnableBatchProcessing);
        Assert.Equal(100, auditSettings.BatchSize);
        Assert.Equal(60, auditSettings.BatchIntervalSeconds);
        Assert.Equal(45, auditSettings.HttpTimeoutSeconds);
        Assert.Equal(5, auditSettings.RetryAttempts);
        Assert.Equal(2000, auditSettings.RetryDelayMs);
        Assert.True(auditSettings.EnableHealthCheckAudit);
        Assert.True(auditSettings.EnableAuthorizationAudit);
        Assert.True(auditSettings.EnableGatewayRequestAudit);
        Assert.Equal("Debug", auditSettings.MinimumLogLevel);
        Assert.True(auditSettings.EnableCorrelationId);
        Assert.True(auditSettings.EnableEventFiltering);
        Assert.Equal("/health,/metrics,/swagger", auditSettings.ExcludePaths);
        Assert.Equal("GET,POST", auditSettings.IncludeHttpMethods);
    }

    [Fact]
    public void AuditSettings_ExcludedPaths_ShouldParseCorrectly()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            ExcludePaths = "/health,/swagger,/metrics"
        };

        // Act
        var excludedPaths = auditSettings.ExcludedPaths;

        // Assert
        Assert.Equal(3, excludedPaths.Length);
        Assert.Contains("/health", excludedPaths);
        Assert.Contains("/swagger", excludedPaths);
        Assert.Contains("/metrics", excludedPaths);
    }

    [Fact]
    public void AuditSettings_IncludedHttpMethods_ShouldParseCorrectly()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            IncludeHttpMethods = "GET,POST,PUT"
        };

        // Act
        var includedMethods = auditSettings.IncludedHttpMethods;

        // Assert
        Assert.Equal(3, includedMethods.Length);
        Assert.Contains("GET", includedMethods);
        Assert.Contains("POST", includedMethods);
        Assert.Contains("PUT", includedMethods);
    }

    [Fact]
    public void AuditSettings_EmptyIncludeHttpMethods_ShouldReturnEmpty()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            IncludeHttpMethods = ""
        };

        // Act
        var includedMethods = auditSettings.IncludedHttpMethods;

        // Assert
        Assert.Empty(includedMethods);
    }

    [Fact]
    public void MonitoringSettings_ShouldLoadFromConfiguration()
    {
        // Arrange
        var configurationData = new Dictionary<string, string>
        {
            ["Monitoring:EnableHealthMonitoring"] = "true",
            ["Monitoring:DefaultHealthCheckIntervalSeconds"] = "45",
            ["Monitoring:DefaultTimeoutSeconds"] = "15",
            ["Monitoring:DegradedResponseTimeMs"] = "3000",
            ["Monitoring:MaxConsecutiveFailures"] = "5",
            ["Monitoring:EnableHealthCheckAudit"] = "true",
            ["Monitoring:HealthCheckAuditIntervalSeconds"] = "600",
            ["Monitoring:MaxHealthHistoryRecords"] = "2000",
            ["Monitoring:HealthDataRetentionHours"] = "336",
            ["Monitoring:EnableDependencyTracking"] = "true",
            ["Monitoring:EnablePerformanceMetrics"] = "true",
            ["Monitoring:EnableAlerting"] = "true",
            ["Monitoring:AlertHealthScoreThreshold"] = "75.0",
            ["Monitoring:EnableAutoDiscovery"] = "true",
            ["Monitoring:AutoDiscoveryPatterns"] = "*-service,*-api",
            ["Monitoring:EnableCircuitBreaker"] = "true",
            ["Monitoring:CircuitBreakerFailureThreshold"] = "3",
            ["Monitoring:CircuitBreakerTimeoutSeconds"] = "30",
            ["Monitoring:EnableHealthBasedLoadBalancing"] = "true",
            ["Monitoring:ServiceDefaults:Environment"] = "production",
            ["Monitoring:ServiceDefaults:HealthCheckPath"] = "/status",
            ["Monitoring:ServiceDefaults:DefaultPort"] = "8080",
            ["Monitoring:ServiceDefaults:DefaultProtocol"] = "https",
            ["Monitoring:ServiceDefaults:AutoRegisterDiscoveredServices"] = "true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.Configure<MonitoringSettings>(configuration.GetSection(MonitoringSettings.SectionName));

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var monitoringSettings = serviceProvider.GetRequiredService<IOptions<MonitoringSettings>>().Value;

        // Assert
        Assert.True(monitoringSettings.EnableHealthMonitoring);
        Assert.Equal(45, monitoringSettings.DefaultHealthCheckIntervalSeconds);
        Assert.Equal(15, monitoringSettings.DefaultTimeoutSeconds);
        Assert.Equal(3000, monitoringSettings.DegradedResponseTimeMs);
        Assert.Equal(5, monitoringSettings.MaxConsecutiveFailures);
        Assert.True(monitoringSettings.EnableHealthCheckAudit);
        Assert.Equal(600, monitoringSettings.HealthCheckAuditIntervalSeconds);
        Assert.Equal(2000, monitoringSettings.MaxHealthHistoryRecords);
        Assert.Equal(336, monitoringSettings.HealthDataRetentionHours);
        Assert.True(monitoringSettings.EnableDependencyTracking);
        Assert.True(monitoringSettings.EnablePerformanceMetrics);
        Assert.True(monitoringSettings.EnableAlerting);
        Assert.Equal(75.0, monitoringSettings.AlertHealthScoreThreshold);
        Assert.True(monitoringSettings.EnableAutoDiscovery);
        Assert.Equal("*-service,*-api", monitoringSettings.AutoDiscoveryPatterns);
        Assert.True(monitoringSettings.EnableCircuitBreaker);
        Assert.Equal(3, monitoringSettings.CircuitBreakerFailureThreshold);
        Assert.Equal(30, monitoringSettings.CircuitBreakerTimeoutSeconds);
        Assert.True(monitoringSettings.EnableHealthBasedLoadBalancing);
        
        // Service defaults
        Assert.Equal("production", monitoringSettings.ServiceDefaults.Environment);
        Assert.Equal("/status", monitoringSettings.ServiceDefaults.HealthCheckPath);
        Assert.Equal(8080, monitoringSettings.ServiceDefaults.DefaultPort);
        Assert.Equal("https", monitoringSettings.ServiceDefaults.DefaultProtocol);
        Assert.True(monitoringSettings.ServiceDefaults.AutoRegisterDiscoveredServices);
    }

    [Fact]
    public void AuditSettings_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var auditSettings = new AuditSettings();

        // Assert
        Assert.True(auditSettings.EnableAuditLogging);
        Assert.Equal("http://transaction-service", auditSettings.TransactionServiceBaseUrl);
        Assert.Equal("/api/audit/gateway", auditSettings.AuditEndpoint);
        Assert.Equal("/api/audit/gateway/batch", auditSettings.BatchAuditEndpoint);
        Assert.True(auditSettings.EnableBatchProcessing);
        Assert.Equal(50, auditSettings.BatchSize);
        Assert.Equal(30, auditSettings.BatchIntervalSeconds);
        Assert.Equal(30, auditSettings.HttpTimeoutSeconds);
        Assert.Equal(3, auditSettings.RetryAttempts);
        Assert.Equal(1000, auditSettings.RetryDelayMs);
        Assert.False(auditSettings.EnableHealthCheckAudit);
        Assert.True(auditSettings.EnableAuthorizationAudit);
        Assert.True(auditSettings.EnableGatewayRequestAudit);
        Assert.Equal("Information", auditSettings.MinimumLogLevel);
        Assert.True(auditSettings.EnableCorrelationId);
        Assert.True(auditSettings.EnableEventFiltering);
        Assert.Equal("/health,/swagger,/metrics", auditSettings.ExcludePaths);
        Assert.Equal("", auditSettings.IncludeHttpMethods);
    }

    [Fact]
    public void MonitoringSettings_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var monitoringSettings = new MonitoringSettings();

        // Assert
        Assert.True(monitoringSettings.EnableHealthMonitoring);
        Assert.Equal(30, monitoringSettings.DefaultHealthCheckIntervalSeconds);
        Assert.Equal(10, monitoringSettings.DefaultTimeoutSeconds);
        Assert.Equal(5000, monitoringSettings.DegradedResponseTimeMs);
        Assert.Equal(3, monitoringSettings.MaxConsecutiveFailures);
        Assert.False(monitoringSettings.EnableHealthCheckAudit);
        Assert.Equal(300, monitoringSettings.HealthCheckAuditIntervalSeconds);
        Assert.Equal(1000, monitoringSettings.MaxHealthHistoryRecords);
        Assert.Equal(168, monitoringSettings.HealthDataRetentionHours);
        Assert.True(monitoringSettings.EnableDependencyTracking);
        Assert.True(monitoringSettings.EnablePerformanceMetrics);
        Assert.True(monitoringSettings.EnableAlerting);
        Assert.Equal(80.0, monitoringSettings.AlertHealthScoreThreshold);
        Assert.False(monitoringSettings.EnableAutoDiscovery);
        Assert.Equal("*-service", monitoringSettings.AutoDiscoveryPatterns);
        Assert.True(monitoringSettings.EnableCircuitBreaker);
        Assert.Equal(5, monitoringSettings.CircuitBreakerFailureThreshold);
        Assert.Equal(60, monitoringSettings.CircuitBreakerTimeoutSeconds);
        Assert.True(monitoringSettings.EnableHealthBasedLoadBalancing);
        
        // Service defaults
        Assert.Equal("development", monitoringSettings.ServiceDefaults.Environment);
        Assert.Equal("/health", monitoringSettings.ServiceDefaults.HealthCheckPath);
        Assert.Equal(80, monitoringSettings.ServiceDefaults.DefaultPort);
        Assert.Equal("http", monitoringSettings.ServiceDefaults.DefaultProtocol);
        Assert.Empty(monitoringSettings.ServiceDefaults.DefaultTags);
        Assert.True(monitoringSettings.ServiceDefaults.AutoRegisterDiscoveredServices);
    }
}