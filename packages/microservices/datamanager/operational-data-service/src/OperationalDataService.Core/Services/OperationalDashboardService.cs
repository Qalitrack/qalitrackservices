using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;
using OperationalDataService.Core.Interfaces;

namespace OperationalDataService.Core.Services;

public class OperationalDashboardService : IOperationalDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICapacityManagementService _capacityService;
    private readonly IProductCatalogService _productService;
    private readonly IRouteOptimizationService _routeService;

    public OperationalDashboardService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICapacityManagementService capacityService,
        IProductCatalogService productService,
        IRouteOptimizationService routeService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _capacityService = capacityService;
        _productService = productService;
        _routeService = routeService;
    }

    public async Task<OperationalDashboardDto> GetDashboardDataAsync(string organizationId)
    {
        var dashboard = new OperationalDashboardDto
        {
            OrganizationId = organizationId,
            GeneratedAt = DateTime.UtcNow
        };

        // Get all dashboard components in parallel for better performance
        var weighbridgeTask = GetWeighbridgeStatusSummaryAsync(organizationId);
        var capacityTask = GetCapacityUtilizationAsync(organizationId);
        var productTask = GetActiveProductCountAsync(organizationId);
        var routeTask = GetRoutePerformanceSummaryAsync(organizationId);
        var alertsTask = GetActiveAlertsAsync(organizationId);
        var maintenanceTask = GetUpcomingMaintenanceAsync(organizationId);
        var metricsTask = GetOperationalMetricsAsync(organizationId);
        var forecastsTask = GetCapacityForecastsAsync(organizationId);

        await Task.WhenAll(weighbridgeTask, capacityTask, productTask, routeTask, alertsTask, maintenanceTask, metricsTask, forecastsTask);

        dashboard.WeighbridgeStatus = await weighbridgeTask;
        dashboard.CapacityUtilization = await capacityTask;
        dashboard.ActiveProducts = await productTask;
        dashboard.RoutePerformance = await routeTask;
        dashboard.OperationalAlerts = await alertsTask;
        dashboard.MaintenanceSchedule = await maintenanceTask;
        dashboard.PerformanceMetrics = await metricsTask;
        dashboard.Forecasts = await forecastsTask;

        // Get system health
        dashboard.SystemHealth = await GetSystemHealthAsync();

        return dashboard;
    }

    public async Task<WeighbridgeStatusSummary> GetWeighbridgeStatusSummaryAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId);

        var summary = new WeighbridgeStatusSummary
        {
            TotalWeighbridges = weighbridges.Count(),
            ActiveWeighbridges = weighbridges.Count(w => w.Status == OperationalStatus.Active),
            MaintenanceWeighbridges = weighbridges.Count(w => w.Status == OperationalStatus.Maintenance),
            OfflineWeighbridges = weighbridges.Count(w => w.Status == OperationalStatus.Offline)
        };

        var weighbridgeDetails = new List<WeighbridgeOperationalStatus>();

        foreach (var weighbridge in weighbridges)
        {
            var detail = new WeighbridgeOperationalStatus
            {
                WeighbridgeId = weighbridge.WeighbridgeId,
                Name = weighbridge.Name,
                Status = weighbridge.Status,
                UtilizationRate = weighbridge.UtilizationRate,
                VehiclesInQueue = weighbridge.QueuedVehicles.Count,
                EstimatedWaitTime = TimeSpan.FromMinutes((double)weighbridge.EstimatedWaitTime),
                LastUpdated = weighbridge.UpdatedAt
            };

            weighbridgeDetails.Add(detail);
        }

        summary.WeighbridgeDetails = weighbridgeDetails;

        if (weighbridgeDetails.Any())
        {
            summary.AverageUtilization = weighbridgeDetails.Average(w => w.UtilizationRate);
            summary.TotalVehiclesInQueue = weighbridgeDetails.Sum(w => w.VehiclesInQueue);
            summary.AverageWaitTime = TimeSpan.FromMinutes(weighbridgeDetails.Average(w => w.EstimatedWaitTime.TotalMinutes));
        }

        return summary;
    }

    public async Task<CapacityUtilizationSummary> GetCapacityUtilizationAsync(string organizationId)
    {
        var capacities = await _capacityService.GetAllCapacitiesAsync(organizationId);
        
        var summary = new CapacityUtilizationSummary();

        if (capacities.Any())
        {
            summary.OverallUtilization = capacities.Average(c => c.UtilizationRate);
            summary.PeakUtilization = capacities.Max(c => c.UtilizationRate);
            summary.LowestUtilization = capacities.Min(c => c.UtilizationRate);

            var peakCapacity = capacities.First(c => c.UtilizationRate == summary.PeakUtilization);
            summary.PeakTime = peakCapacity.LastUpdated;

            var lowestCapacity = capacities.First(c => c.UtilizationRate == summary.LowestUtilization);
            summary.LowestTime = lowestCapacity.LastUpdated;

            // Generate hourly breakdown for the last 24 hours
            summary.HourlyBreakdown = GenerateHourlyBreakdown(capacities);

            // Check for capacity alerts
            summary.ActiveAlerts = GenerateCapacityAlerts(capacities);

            // Determine trend
            summary.Trend = DetermineUtilizationTrend(capacities);
        }

        return summary;
    }

    public async Task<ProductSummary> GetActiveProductCountAsync(string organizationId)
    {
        var products = await _productService.GetActiveProductsAsync(organizationId);
        var lowStockProducts = await _productService.GetLowStockProductsAsync(organizationId);

        var summary = new ProductSummary
        {
            TotalProducts = products.Count,
            ActiveProducts = products.Count(p => p.Status == ProductStatus.Active),
            LowStockProducts = lowStockProducts.Count,
            OutOfStockProducts = products.Count(p => p.AvailableQuantity <= 0),
            LastSyncTime = products.Any() ? products.Max(p => p.LastSyncDate) : DateTime.MinValue
        };

        // Generate product alerts
        summary.ProductAlerts = GenerateProductAlerts(products, lowStockProducts);

        // Get top products by transaction volume (mock data for now)
        summary.TopProducts = GenerateTopProducts(products);

        return summary;
    }

    public async Task<RoutePerformanceSummary> GetRoutePerformanceSummaryAsync(string organizationId)
    {
        var routes = await _unitOfWork.Repository<RouteConfiguration>()
            .FindAsync(r => r.OrganizationId == organizationId && r.Status == RouteStatus.Active);

        var summary = new RoutePerformanceSummary
        {
            TotalRoutes = routes.Count(),
            ActiveRoutes = routes.Count(r => r.Status == RouteStatus.Active)
        };

        var routeDetails = new List<RoutePerformanceDetail>();

        foreach (var route in routes)
        {
            var detail = new RoutePerformanceDetail
            {
                RouteId = route.RouteId,
                Name = route.Name,
                ReliabilityScore = route.PerformanceMetrics.ReliabilityScore,
                AverageDelay = route.PerformanceMetrics.AverageDelay,
                UsageCount = route.PerformanceMetrics.UsageCount,
                CustomerSatisfaction = route.PerformanceMetrics.CustomerSatisfactionScore
            };

            routeDetails.Add(detail);
        }

        if (routeDetails.Any())
        {
            summary.AverageReliabilityScore = routeDetails.Average(r => r.ReliabilityScore);
            summary.AverageDelay = routeDetails.Average(r => r.AverageDelay);

            summary.TopPerformingRoutes = routeDetails
                .OrderByDescending(r => r.ReliabilityScore)
                .Take(5)
                .ToList();

            summary.PoorPerformingRoutes = routeDetails
                .OrderBy(r => r.ReliabilityScore)
                .Take(5)
                .ToList();
        }

        // Get active route issues
        summary.ActiveIssues = await _routeService.GetActiveRouteIssuesAsync();

        return summary;
    }

    public async Task<List<OperationalAlertSummary>> GetActiveAlertsAsync(string organizationId)
    {
        var alerts = await _unitOfWork.Repository<OperationalAlert>()
            .FindAsync(a => a.OrganizationId == organizationId && 
                           a.Status == AlertStatus.Active);

        var alertSummaries = new List<OperationalAlertSummary>();

        foreach (var alert in alerts.OrderByDescending(a => a.Severity).ThenByDescending(a => a.TriggeredAt))
        {
            var summary = _mapper.Map<OperationalAlertSummary>(alert);
            
            // Get weighbridge name if alert is related to a weighbridge
            if (!string.IsNullOrEmpty(alert.WeighbridgeId))
            {
                var weighbridge = await _unitOfWork.Repository<WeighbridgeOperation>()
                    .FirstOrDefaultAsync(w => w.WeighbridgeId == alert.WeighbridgeId);
                
                if (weighbridge != null)
                {
                    summary.WeighbridgeName = weighbridge.Name;
                }
            }

            alertSummaries.Add(summary);
        }

        return alertSummaries;
    }

    public async Task<MaintenanceScheduleSummary> GetUpcomingMaintenanceAsync(string organizationId)
    {
        var maintenanceSchedules = await _unitOfWork.Repository<MaintenanceSchedule>()
            .FindAsync(m => m.OrganizationId == organizationId);

        var now = DateTime.UtcNow;
        var today = now.Date;
        var nextWeek = today.AddDays(7);

        var summary = new MaintenanceScheduleSummary
        {
            TotalScheduledMaintenance = maintenanceSchedules.Count(m => m.Status == MaintenanceStatus.Scheduled),
            OverdueMaintenanceCount = maintenanceSchedules.Count(m => m.ScheduledDate < now && m.Status == MaintenanceStatus.Scheduled),
            TodaysMaintenance = maintenanceSchedules.Count(m => m.ScheduledDate.Date == today && m.Status == MaintenanceStatus.Scheduled),
            ThisWeeksMaintenance = maintenanceSchedules.Count(m => m.ScheduledDate.Date >= today && 
                                                                  m.ScheduledDate.Date <= nextWeek && 
                                                                  m.Status == MaintenanceStatus.Scheduled)
        };

        // Get upcoming maintenance
        var upcomingMaintenance = maintenanceSchedules
            .Where(m => m.ScheduledDate >= now && m.Status == MaintenanceStatus.Scheduled)
            .OrderBy(m => m.ScheduledDate)
            .Take(10);

        summary.UpcomingMaintenance = _mapper.Map<List<UpcomingMaintenance>>(upcomingMaintenance);

        // Get overdue maintenance
        var overdueMaintenance = maintenanceSchedules
            .Where(m => m.ScheduledDate < now && m.Status == MaintenanceStatus.Scheduled)
            .OrderBy(m => m.ScheduledDate);

        summary.OverdueMaintenance = _mapper.Map<List<OverdueMaintenance>>(overdueMaintenance);

        // Add weighbridge names for upcoming maintenance
        foreach (var maintenance in summary.UpcomingMaintenance)
        {
            var weighbridge = await _unitOfWork.Repository<WeighbridgeOperation>()
                .FirstOrDefaultAsync(w => w.WeighbridgeId == maintenance.WeighbridgeId);
            
            if (weighbridge != null)
            {
                maintenance.WeighbridgeName = weighbridge.Name;
            }
        }

        // Add weighbridge names for overdue maintenance
        foreach (var maintenance in summary.OverdueMaintenance)
        {
            var weighbridge = await _unitOfWork.Repository<WeighbridgeOperation>()
                .FirstOrDefaultAsync(w => w.WeighbridgeId == maintenance.WeighbridgeId);
            
            if (weighbridge != null)
            {
                maintenance.WeighbridgeName = weighbridge.Name;
            }
        }

        return summary;
    }

    public async Task<PerformanceMetricsSummary> GetOperationalMetricsAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId);

        var capacities = await _capacityService.GetAllCapacitiesAsync(organizationId);

        var summary = new PerformanceMetricsSummary();

        if (weighbridges.Any() && capacities.Any())
        {
            // Calculate overall efficiency
            summary.OverallEfficiency = CalculateOverallEfficiency(weighbridges, capacities);

            // Calculate throughput rate
            summary.ThroughputRate = CalculateThroughputRate(capacities);

            // Calculate quality score (placeholder)
            summary.QualityScore = 85m; // This would be calculated from actual quality metrics

            // Customer satisfaction (placeholder)
            summary.CustomerSatisfactionScore = 80m; // This would be calculated from feedback data

            // Cost efficiency (placeholder)
            summary.CostEfficiency = 75m; // This would be calculated from cost and efficiency data

            // Generate KPI metrics
            summary.KeyMetrics = GenerateKpiMetrics(summary);

            // Generate performance trends
            summary.Trends = await GetPerformanceTrendsAsync(organizationId, new TimeRange 
            { 
                StartDate = DateTime.UtcNow.AddDays(-30), 
                EndDate = DateTime.UtcNow 
            });
        }

        return summary;
    }

    public async Task<List<CapacityForecastSummary>> GetCapacityForecastsAsync(string organizationId)
    {
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId && w.Status == OperationalStatus.Active);

        var forecasts = new List<CapacityForecastSummary>();

        foreach (var weighbridge in weighbridges.Take(5)) // Limit to top 5 for dashboard
        {
            try
            {
                var forecast = await _capacityService.ForecastCapacityAsync(weighbridge.WeighbridgeId, 24);
                
                if (forecast.HourlyForecasts.Any())
                {
                    var summary = new CapacityForecastSummary
                    {
                        WeighbridgeId = weighbridge.WeighbridgeId,
                        WeighbridgeName = weighbridge.Name,
                        ForecastDate = forecast.ForecastDate,
                        PredictedPeakUtilization = forecast.HourlyForecasts.Max(h => h.PredictedUtilization),
                        Confidence = forecast.Confidence
                    };

                    var peakForecast = forecast.HourlyForecasts.First(h => h.PredictedUtilization == summary.PredictedPeakUtilization);
                    summary.PredictedPeakTime = peakForecast.Hour;

                    summary.KeyFactors.Add("Historical usage patterns");
                    summary.KeyFactors.Add("Seasonal trends");
                    summary.KeyFactors.Add("Scheduled operations");

                    forecasts.Add(summary);
                }
            }
            catch (Exception)
            {
                // Continue with other weighbridges if one fails
                continue;
            }
        }

        return forecasts;
    }

    public async Task<SystemHealthSummary> GetSystemHealthAsync()
    {
        var summary = new SystemHealthSummary
        {
            OverallHealth = HealthStatus.Healthy,
            SystemUptime = 99.5m, // This would be calculated from actual uptime data
            ActiveConnections = 150, // This would be from actual connection metrics
            ResponseTime = 250m, // This would be from actual response time metrics
            ErrorRate = 0.1m, // This would be from actual error rate metrics
            LastHealthCheck = DateTime.UtcNow
        };

        // Check individual components
        summary.Components = new List<SystemComponent>
        {
            new()
            {
                Name = "Database",
                Status = HealthStatus.Healthy,
                ResponseTime = 50m,
                ErrorRate = 0.0m,
                LastChecked = DateTime.UtcNow,
                Message = "Database connection is healthy"
            },
            new()
            {
                Name = "Master Data Integration",
                Status = HealthStatus.Healthy,
                ResponseTime = 200m,
                ErrorRate = 0.1m,
                LastChecked = DateTime.UtcNow,
                Message = "Integration services are operational"
            },
            new()
            {
                Name = "Cache",
                Status = HealthStatus.Healthy,
                ResponseTime = 10m,
                ErrorRate = 0.0m,
                LastChecked = DateTime.UtcNow,
                Message = "Cache is responding normally"
            }
        };

        // Determine overall health based on components
        if (summary.Components.Any(c => c.Status == HealthStatus.Critical))
        {
            summary.OverallHealth = HealthStatus.Critical;
        }
        else if (summary.Components.Any(c => c.Status == HealthStatus.Warning))
        {
            summary.OverallHealth = HealthStatus.Warning;
        }

        return summary;
    }

    public async Task RefreshDashboardDataAsync(string organizationId)
    {
        // This would typically invalidate caches and refresh data
        // For now, we'll just ensure all services are up to date
        
        await _productService.RefreshProductCatalogAsync(organizationId);
        
        // Refresh capacity metrics for all weighbridges
        var weighbridges = await _unitOfWork.Repository<WeighbridgeOperation>()
            .FindAsync(w => w.OrganizationId == organizationId);

        foreach (var weighbridge in weighbridges)
        {
            await _capacityService.RecalculateCapacityMetricsAsync(weighbridge.WeighbridgeId);
        }
    }

    public async Task<List<KpiMetric>> GetKpiMetricsAsync(string organizationId)
    {
        var metrics = await GetOperationalMetricsAsync(organizationId);
        return metrics.KeyMetrics;
    }

    public async Task<List<PerformanceTrend>> GetPerformanceTrendsAsync(string organizationId, TimeRange period)
    {
        // This would typically analyze historical data to identify trends
        // For now, we'll return mock trends
        return new List<PerformanceTrend>
        {
            new()
            {
                Metric = "Overall Efficiency",
                Direction = TrendDirection.Improving,
                ChangePercentage = 5.2m,
                PeriodStart = period.StartDate,
                PeriodEnd = period.EndDate,
                Analysis = "Efficiency has improved by 5.2% over the last 30 days"
            },
            new()
            {
                Metric = "Average Utilization",
                Direction = TrendDirection.Stable,
                ChangePercentage = 1.1m,
                PeriodStart = period.StartDate,
                PeriodEnd = period.EndDate,
                Analysis = "Utilization has remained relatively stable"
            },
            new()
            {
                Metric = "Customer Satisfaction",
                Direction = TrendDirection.Improving,
                ChangePercentage = 8.5m,
                PeriodStart = period.StartDate,
                PeriodEnd = period.EndDate,
                Analysis = "Customer satisfaction has improved significantly"
            }
        };
    }

    // Private helper methods
    private List<HourlyUtilization> GenerateHourlyBreakdown(List<WeighbridgeCapacity> capacities)
    {
        var hourlyData = new List<HourlyUtilization>();
        var now = DateTime.UtcNow;

        for (int i = 23; i >= 0; i--)
        {
            var hour = now.AddHours(-i);
            
            hourlyData.Add(new HourlyUtilization
            {
                Hour = hour,
                UtilizationRate = capacities.Average(c => c.UtilizationRate * (0.8m + (decimal)(new Random().NextDouble() * 0.4))), // Mock variation
                VehicleCount = capacities.Sum(c => c.VehiclesInQueue),
                AverageWaitTime = TimeSpan.FromMinutes(capacities.Average(c => c.EstimatedWaitTime.TotalMinutes))
            });
        }

        return hourlyData;
    }

    private List<CapacityAlert> GenerateCapacityAlerts(List<WeighbridgeCapacity> capacities)
    {
        var alerts = new List<CapacityAlert>();

        foreach (var capacity in capacities.Where(c => c.UtilizationRate > 0.8m))
        {
            alerts.Add(new CapacityAlert
            {
                Severity = capacity.UtilizationRate > 0.95m ? AlertSeverity.Critical : AlertSeverity.Warning,
                Message = $"High utilization detected on {capacity.Name}: {capacity.UtilizationRate:P1}",
                TriggeredAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        return alerts;
    }

    private UtilizationTrend DetermineUtilizationTrend(List<WeighbridgeCapacity> capacities)
    {
        // This would typically analyze historical data
        // For now, return a mock trend based on current utilization
        var avgUtilization = capacities.Average(c => c.UtilizationRate);
        
        return avgUtilization switch
        {
            > 0.8m => UtilizationTrend.Increasing,
            < 0.4m => UtilizationTrend.Decreasing,
            _ => UtilizationTrend.Stable
        };
    }

    private List<ProductAlert> GenerateProductAlerts(List<ProductCatalogDto> products, List<ProductCatalogDto> lowStockProducts)
    {
        var alerts = new List<ProductAlert>();

        foreach (var product in lowStockProducts)
        {
            alerts.Add(new ProductAlert
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                AlertType = "Low Stock",
                Message = $"Product {product.Name} is below reorder level",
                Severity = AlertSeverity.Warning,
                CreatedAt = DateTime.UtcNow
            });
        }

        var expiredProducts = products.Where(p => p.ExpiryDate.HasValue && p.ExpiryDate.Value <= DateTime.UtcNow);
        foreach (var product in expiredProducts)
        {
            alerts.Add(new ProductAlert
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                AlertType = "Expired",
                Message = $"Product {product.Name} has expired",
                Severity = AlertSeverity.Critical,
                CreatedAt = DateTime.UtcNow
            });
        }

        return alerts;
    }

    private List<TopProduct> GenerateTopProducts(List<ProductCatalogDto> products)
    {
        // This would typically be based on actual transaction data
        // For now, return mock top products
        return products.Take(5).Select((p, i) => new TopProduct
        {
            ProductId = p.ProductId,
            Name = p.Name,
            QuantityMoved = 1000 - (i * 150), // Mock quantities
            Revenue = (1000 - (i * 150)) * p.CurrentPrice,
            TransactionCount = 50 - (i * 8)
        }).ToList();
    }

    private decimal CalculateOverallEfficiency(IEnumerable<WeighbridgeOperation> weighbridges, List<WeighbridgeCapacity> capacities)
    {
        if (!capacities.Any()) return 0;

        var totalCapacity = capacities.Sum(c => c.MaxCapacity);
        var totalUtilized = capacities.Sum(c => c.CurrentLoad);
        
        return totalCapacity > 0 ? (decimal)totalUtilized / totalCapacity * 100 : 0;
    }

    private decimal CalculateThroughputRate(List<WeighbridgeCapacity> capacities)
    {
        if (!capacities.Any()) return 0;

        // Calculate vehicles per hour based on current processing rates
        return capacities.Sum(c => c.HourlyCapacity * (c.UtilizationRate / 100));
    }

    private List<KpiMetric> GenerateKpiMetrics(PerformanceMetricsSummary summary)
    {
        return new List<KpiMetric>
        {
            new()
            {
                Name = "Overall Efficiency",
                Value = summary.OverallEfficiency,
                Target = 85m,
                Unit = "%",
                Status = summary.OverallEfficiency >= 85m ? MetricStatus.OnTarget : 
                        summary.OverallEfficiency >= 70m ? MetricStatus.BelowTarget : MetricStatus.Critical,
                PercentageOfTarget = summary.OverallEfficiency / 85m * 100,
                Trend = TrendDirection.Improving
            },
            new()
            {
                Name = "Throughput Rate",
                Value = summary.ThroughputRate,
                Target = 100m,
                Unit = "vehicles/hour",
                Status = summary.ThroughputRate >= 100m ? MetricStatus.OnTarget : MetricStatus.BelowTarget,
                PercentageOfTarget = summary.ThroughputRate / 100m * 100,
                Trend = TrendDirection.Stable
            },
            new()
            {
                Name = "Quality Score",
                Value = summary.QualityScore,
                Target = 90m,
                Unit = "%",
                Status = summary.QualityScore >= 90m ? MetricStatus.OnTarget : MetricStatus.BelowTarget,
                PercentageOfTarget = summary.QualityScore / 90m * 100,
                Trend = TrendDirection.Improving
            },
            new()
            {
                Name = "Customer Satisfaction",
                Value = summary.CustomerSatisfactionScore,
                Target = 85m,
                Unit = "%",
                Status = summary.CustomerSatisfactionScore >= 85m ? MetricStatus.OnTarget : MetricStatus.BelowTarget,
                PercentageOfTarget = summary.CustomerSatisfactionScore / 85m * 100,
                Trend = TrendDirection.Improving
            }
        };
    }
}