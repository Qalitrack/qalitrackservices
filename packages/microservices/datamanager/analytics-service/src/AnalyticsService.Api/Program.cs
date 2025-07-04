using Microsoft.EntityFrameworkCore;
using AnalyticsService.Core.Interfaces;
using AnalyticsService.Core.Services;
using AnalyticsService.Infrastructure.Data;
using AnalyticsService.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/analytics-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=analytics.db"));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register repositories
builder.Services.AddScoped<IAnalyticsMetricRepository, AnalyticsMetricRepository>();
builder.Services.AddScoped<IAnomalyRepository, AnomalyRepository>();
builder.Services.AddScoped(typeof(IAnalyticsRepository<>), typeof(AnalyticsRepository<>));

// Register core services
builder.Services.AddScoped<IMetricsCalculator, MetricsCalculator>();
builder.Services.AddScoped<IOperationalMetricsCalculator, OperationalMetricsCalculator>();

// Register analytics engine
builder.Services.AddScoped<IAnalyticsEngine, AnalyticsEngine>();

// Register placeholder services (these would be implemented based on requirements)
builder.Services.AddScoped<IFinancialMetricsCalculator, PlaceholderFinancialMetricsCalculator>();
builder.Services.AddScoped<IComplianceMetricsCalculator, PlaceholderComplianceMetricsCalculator>();
builder.Services.AddScoped<ITrendAnalysisService, PlaceholderTrendAnalysisService>();
builder.Services.AddScoped<IAnomalyDetectionService, PlaceholderAnomalyDetectionService>();
builder.Services.AddScoped<IForecastingService, PlaceholderForecastingService>();
builder.Services.AddScoped<IBenchmarkService, PlaceholderBenchmarkService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AnalyticsDbContext>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Analytics Service API", 
        Version = "v1",
        Description = "Real-time analytics and metrics processing for weighbridge operations"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Analytics Service API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add SignalR hub (would be implemented for real-time updates)
// app.MapHub<DashboardHub>("/dashboardHub");

Log.Information("Analytics Service starting up...");

app.Run();

// Placeholder service implementations
public class PlaceholderFinancialMetricsCalculator : IFinancialMetricsCalculator
{
    public Task<MetricResult> CalculateTotalRevenueAsync(string organizationId, TimeRange timeRange)
    {
        return Task.FromResult(new MetricResult
        {
            MetricType = "Revenue",
            Name = "Total Revenue",
            Value = 0,
            Unit = "currency",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { ["status"] = "placeholder" }
        });
    }

    public Task<MetricResult> CalculateRevenueByServiceAsync(string organizationId, TimeRange timeRange) => 
        CalculateTotalRevenueAsync(organizationId, timeRange);

    public Task<MetricResult> CalculateAverageTransactionValueAsync(string organizationId, TimeRange timeRange) => 
        CalculateTotalRevenueAsync(organizationId, timeRange);

    public Task<MetricResult> CalculateRevenueGrowthAsync(string organizationId, TimeRange timeRange) => 
        CalculateTotalRevenueAsync(organizationId, timeRange);

    public Task<MetricResult> CalculateProfitMarginsAsync(string organizationId, TimeRange timeRange) => 
        CalculateTotalRevenueAsync(organizationId, timeRange);

    public Task<Dictionary<string, MetricResult>> CalculateRevenueBreakdownAsync(string organizationId, TimeRange timeRange) => 
        Task.FromResult(new Dictionary<string, MetricResult>());
}

public class PlaceholderComplianceMetricsCalculator : IComplianceMetricsCalculator
{
    public Task<MetricResult> CalculateComplianceRateAsync(string organizationId, TimeRange timeRange)
    {
        return Task.FromResult(new MetricResult
        {
            MetricType = "ComplianceRate",
            Name = "Compliance Rate",
            Value = 95.0,
            Unit = "percentage",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { ["status"] = "placeholder" }
        });
    }

    public Task<MetricResult> CalculateViolationRateAsync(string organizationId, TimeRange timeRange) => 
        CalculateComplianceRateAsync(organizationId, timeRange);

    public Task<MetricResult> CalculateAuditScoreAsync(string organizationId, TimeRange timeRange) => 
        CalculateComplianceRateAsync(organizationId, timeRange);

    public Task<MetricResult> CalculateRegulationAdherenceAsync(string organizationId, TimeRange timeRange, string regulationType) => 
        CalculateComplianceRateAsync(organizationId, timeRange);

    public Task<Dictionary<string, MetricResult>> CalculateComplianceBreakdownAsync(string organizationId, TimeRange timeRange) => 
        Task.FromResult(new Dictionary<string, MetricResult>());
}

public class PlaceholderTrendAnalysisService : ITrendAnalysisService
{
    public Task<TrendAnalysisResponse> AnalyzeTrendAsync(string organizationId, string metricType, TimeRange timeRange, string? weighbridgeId = null)
    {
        return Task.FromResult(new TrendAnalysisResponse
        {
            MetricType = metricType,
            TrendDirection = "Stable",
            TrendStrength = 0.5,
            Slope = 0.0,
            RSquared = 0.0,
            DataPoints = new List<TrendDataPoint>(),
            Breakpoints = new List<TrendBreakpoint>(),
            Insights = new Dictionary<string, object> { ["status"] = "placeholder" },
            ConfidenceLevel = 0.8,
            AnalysisPeriodStart = timeRange.From,
            AnalysisPeriodEnd = timeRange.To
        });
    }

    public Task<double> CalculateLinearRegressionSlopeAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints) => 
        Task.FromResult(0.0);

    public Task<double> CalculateRSquaredAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints) => 
        Task.FromResult(0.0);

    public Task<List<TrendBreakpoint>> DetectBreakpointsAsync(IEnumerable<(DateTime timestamp, double value)> dataPoints) => 
        Task.FromResult(new List<TrendBreakpoint>());

    public Task<string> DetermineTrendDirectionAsync(double slope, double rSquared) => 
        Task.FromResult("Stable");
}

public class PlaceholderAnomalyDetectionService : IAnomalyDetectionService
{
    public Task<List<Anomaly>> DetectAnomaliesAsync(string organizationId, string metricType, TimeRange timeRange) => 
        Task.FromResult(new List<Anomaly>());

    public Task<List<Anomaly>> DetectRealTimeAnomaliesAsync(string organizationId, double value, string metricType, DateTime timestamp) => 
        Task.FromResult(new List<Anomaly>());

    public Task<bool> IsAnomalousAsync(double value, IEnumerable<double> historicalValues, double threshold = 2.5) => 
        Task.FromResult(false);

    public Task<double> CalculateZScoreAsync(double value, IEnumerable<double> historicalValues) => 
        Task.FromResult(0.0);

    public Task UpdateAnomalyStatusAsync(Guid anomalyId, string status, string? resolution = null, string? resolvedBy = null) => 
        Task.CompletedTask;
}

public class PlaceholderForecastingService : IForecastingService
{
    public Task<ForecastResult> GenerateLinearTrendForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null)
    {
        return Task.FromResult(new ForecastResult
        {
            MetricType = metricType,
            ForecastPeriod = forecastDays,
            Predictions = new List<ForecastPoint>(),
            ConfidenceInterval = new ConfidenceInterval(),
            Accuracy = 0.8,
            Model = "Placeholder",
            ModelParameters = new Dictionary<string, object>(),
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Task<ForecastResult> GenerateSeasonalForecastAsync(string organizationId, string metricType, int forecastDays, string? weighbridgeId = null) => 
        GenerateLinearTrendForecastAsync(organizationId, metricType, forecastDays, weighbridgeId);

    public Task<ForecastResult> GenerateMovingAverageForecastAsync(string organizationId, string metricType, int forecastDays, int windowSize = 7) => 
        GenerateLinearTrendForecastAsync(organizationId, metricType, forecastDays);

    public Task<double> CalculateHistoricalAccuracyAsync(string metricType, int days = 30) => 
        Task.FromResult(0.8);
}

public class PlaceholderBenchmarkService : IBenchmarkService
{
    public Task<List<BenchmarkComparison>> CompareToBenchmarksAsync(string organizationId, List<string> metricTypes) => 
        Task.FromResult(new List<BenchmarkComparison>());

    public Task<BenchmarkComparison> CompareSingleMetricAsync(string organizationId, string metricType, string benchmarkType) => 
        Task.FromResult(new BenchmarkComparison());

    public Task<BenchmarkData> CreateBenchmarkAsync(BenchmarkData benchmark) => 
        Task.FromResult(benchmark);

    public Task<BenchmarkData> UpdateBenchmarkAsync(BenchmarkData benchmark) => 
        Task.FromResult(benchmark);

    public Task<List<BenchmarkData>> GetBenchmarksAsync(string metricType) => 
        Task.FromResult(new List<BenchmarkData>());
}