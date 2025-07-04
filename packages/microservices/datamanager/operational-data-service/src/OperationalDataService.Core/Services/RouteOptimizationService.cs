using AutoMapper;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Entities;
using OperationalDataService.Core.Interfaces;
using RoutePerformanceMetricsDto = OperationalDataService.Core.DTOs.RoutePerformanceMetricsDto;

namespace OperationalDataService.Core.Services;

public class RouteOptimizationService : IRouteOptimizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMasterDataIntegrationService _masterDataService;

    public RouteOptimizationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMasterDataIntegrationService masterDataService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _masterDataService = masterDataService;
    }

    public async Task<OptimizedRoute> OptimizeRouteAsync(RouteOptimizationRequest request)
    {
        // Get available routes from database
        var availableRoutes = await GetAvailableRoutesAsync(request.Origin, request.Destination);
        
        if (!availableRoutes.Any())
        {
            // If no routes found locally, try to get from master data
            var masterRoutes = await _masterDataService.GetRoutesFromMasterDataAsync(request.OrganizationId ?? "");
            availableRoutes = masterRoutes.Where(r => 
                r.Origin.Equals(request.Origin, StringComparison.OrdinalIgnoreCase) &&
                r.Destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!availableRoutes.Any())
        {
            throw new InvalidOperationException($"No routes found from {request.Origin} to {request.Destination}");
        }

        // Apply optimization criteria
        var optimizedRoutes = ApplyOptimizationCriteria(availableRoutes, request.Criteria);

        // Consider real-time factors
        var realTimeFactors = await GetRealTimeFactorsAsync(optimizedRoutes);
        var finalRoute = SelectOptimalRoute(optimizedRoutes, realTimeFactors, request.Criteria);

        // Update route with real-time data
        await UpdateRouteWithRealTimeData(finalRoute);

        return finalRoute;
    }

    public async Task<List<RouteRecommendation>> GetRouteRecommendationsAsync(string origin, string destination, string? organizationId = null)
    {
        var routes = await GetAvailableRoutesAsync(origin, destination);
        var recommendations = new List<RouteRecommendation>();

        foreach (var route in routes.Take(5)) // Top 5 recommendations
        {
            var recommendation = new RouteRecommendation
            {
                RouteId = route.RouteId,
                Name = $"Route via {string.Join(", ", route.Waypoints.Take(2).Select(w => w.Name))}",
                Description = GenerateRouteDescription(route),
                Score = CalculateRouteScore(route),
                Reason = DetermineRecommendationReason(route),
                Advantages = GenerateRouteAdvantages(route),
                Disadvantages = GenerateRouteDisadvantages(route),
                Route = route,
                CreatedAt = DateTime.UtcNow
            };

            recommendations.Add(recommendation);
        }

        return recommendations.OrderByDescending(r => r.Score).ToList();
    }

    public async Task<RoutePerformanceMetricsDto> AnalyzeRoutePerformanceAsync(string routeId, TimeRange period)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            throw new InvalidOperationException($"Route {routeId} not found");
        }

        // Calculate performance metrics
        var metrics = new RoutePerformanceMetricsDto
        {
            RouteId = routeId,
            Period = period,
            AverageSpeed = route.PerformanceMetrics.AverageSpeed,
            AverageDelay = route.PerformanceMetrics.AverageDelay,
            ReliabilityScore = route.PerformanceMetrics.ReliabilityScore,
            TotalTrips = route.PerformanceMetrics.UsageCount,
            CustomerSatisfactionScore = route.PerformanceMetrics.CustomerSatisfactionScore,
            LastCalculated = DateTime.UtcNow
        };

        // Add performance trends
        metrics.Trends = await CalculatePerformanceTrends(routeId, period);

        // Add issues if any
        metrics.Issues = await GetRouteIssuesInPeriod(routeId, period);

        return metrics;
    }

    public async Task UpdateTrafficPatternsAsync(string routeId, TrafficData trafficData)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            throw new InvalidOperationException($"Route {routeId} not found");
        }

        // Update traffic condition
        route.TrafficCondition = trafficData.Condition;
        route.LastUpdated = DateTime.UtcNow;

        // Update performance metrics based on traffic data
        if (trafficData.AverageSpeed > 0)
        {
            route.PerformanceMetrics.AverageSpeed = trafficData.AverageSpeed;
        }

        await _unitOfWork.Repository<RouteConfiguration>().UpdateAsync(route);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<OptimizedRoute>> GetAlternativeRoutesAsync(RouteOptimizationRequest request, int maxAlternatives = 3)
    {
        var allRoutes = await GetAvailableRoutesAsync(request.Origin, request.Destination);
        var optimizedRoutes = ApplyOptimizationCriteria(allRoutes, request.Criteria);

        return optimizedRoutes.Take(maxAlternatives).ToList();
    }

    public async Task<OptimizedRoute?> GetRouteByIdAsync(string routeId)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId && !r.IsDeleted);

        return route != null ? _mapper.Map<OptimizedRoute>(route) : null;
    }

    public async Task<List<RouteIssue>> GetActiveRouteIssuesAsync(string? routeId = null)
    {
        var routes = await _unitOfWork.Repository<RouteConfiguration>()
            .FindAsync(r => (routeId == null || r.RouteId == routeId) && !r.IsDeleted);

        var issues = new List<RouteIssue>();
        foreach (var route in routes)
        {
            foreach (var restriction in route.Restrictions.Where(r => r.IsActive))
            {
                issues.Add(new RouteIssue
                {
                    Type = restriction.Type,
                    Description = restriction.Description,
                    Severity = IssueSeverity.Medium, // Default severity
                    ReportedAt = DateTime.UtcNow,
                    Location = route.Name,
                    IsActive = true
                });
            }
        }

        return issues;
    }

    public async Task<TrafficData> GetCurrentTrafficDataAsync(string routeId)
    {
        // Try to get from external service first
        var externalTrafficData = await _masterDataService.GetTrafficDataFromExternalSourceAsync(routeId);
        if (externalTrafficData != null)
        {
            return externalTrafficData;
        }

        // Fall back to stored data
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            throw new InvalidOperationException($"Route {routeId} not found");
        }

        return new TrafficData
        {
            RouteId = routeId,
            Condition = route.TrafficCondition,
            AverageSpeed = route.PerformanceMetrics.AverageSpeed,
            Congestion = CalculateCongestionLevel(route.TrafficCondition),
            UpdatedAt = route.LastUpdated,
            Source = "Local Database"
        };
    }

    public async Task<List<OptimizedRoute>> GetFavoriteRoutesAsync(string origin, string destination, string organizationId)
    {
        var routes = await _unitOfWork.Repository<RouteConfiguration>()
            .FindAsync(r => r.Origin == origin && 
                           r.Destination == destination && 
                           r.OrganizationId == organizationId && 
                           r.Priority >= RoutePriority.High &&
                           !r.IsDeleted);

        return _mapper.Map<List<OptimizedRoute>>(routes);
    }

    public async Task<OptimizedRoute> SaveRouteAsync(OptimizedRoute route)
    {
        var routeConfig = _mapper.Map<RouteConfiguration>(route);
        routeConfig.Id = Guid.NewGuid().ToString();
        routeConfig.CreatedAt = DateTime.UtcNow;
        routeConfig.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<RouteConfiguration>().AddAsync(routeConfig);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<OptimizedRoute>(routeConfig);
    }

    public async Task<bool> DeleteRouteAsync(string routeId)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            return false;
        }

        route.IsDeleted = true;
        route.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<RouteConfiguration>().UpdateAsync(route);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<OptimizedRoute>> GetRouteHistoryAsync(string origin, string destination, DateTime fromDate, DateTime toDate)
    {
        var routes = await _unitOfWork.Repository<RouteConfiguration>()
            .FindAsync(r => r.Origin == origin && 
                           r.Destination == destination && 
                           r.CreatedAt >= fromDate && 
                           r.CreatedAt <= toDate &&
                           !r.IsDeleted);

        return _mapper.Map<List<OptimizedRoute>>(routes);
    }

    public async Task<RouteQuality> CalculateRouteQualityAsync(string routeId)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            throw new InvalidOperationException($"Route {routeId} not found");
        }

        var quality = new RouteQuality
        {
            RoadQuality = CalculateRoadQuality(route),
            SafetyScore = CalculateSafetyScore(route),
            TrafficScore = CalculateTrafficScore(route),
            WeatherScore = CalculateWeatherScore(route),
            InfrastructureScore = CalculateInfrastructureScore(route)
        };

        quality.OverallScore = (quality.RoadQuality + quality.SafetyScore + quality.TrafficScore + 
                               quality.WeatherScore + quality.InfrastructureScore) / 5;

        return quality;
    }

    public async Task ReportRouteIssueAsync(string routeId, RouteIssue issue)
    {
        var route = await _unitOfWork.Repository<RouteConfiguration>()
            .FirstOrDefaultAsync(r => r.RouteId == routeId);

        if (route == null)
        {
            throw new InvalidOperationException($"Route {routeId} not found");
        }

        // Add the issue as a restriction
        var restriction = new RouteRestriction
        {
            Type = issue.Type,
            Description = issue.Description,
            StartDate = issue.ReportedAt,
            IsActive = true
        };

        route.Restrictions.Add(restriction);
        route.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<RouteConfiguration>().UpdateAsync(route);
        await _unitOfWork.SaveChangesAsync();
    }

    // Private helper methods
    private async Task<List<OptimizedRoute>> GetAvailableRoutesAsync(string origin, string destination)
    {
        var routes = await _unitOfWork.Repository<RouteConfiguration>()
            .FindAsync(r => r.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase) && 
                           r.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase) && 
                           r.Status == RouteStatus.Active &&
                           !r.IsDeleted);

        return _mapper.Map<List<OptimizedRoute>>(routes);
    }

    private List<OptimizedRoute> ApplyOptimizationCriteria(List<OptimizedRoute> routes, OptimizationCriteria criteria)
    {
        var optimizedRoutes = routes.AsQueryable();

        if (criteria.OptimizeForTime)
        {
            optimizedRoutes = optimizedRoutes.OrderBy(r => r.EstimatedDuration);
        }
        else if (criteria.OptimizeForDistance)
        {
            optimizedRoutes = optimizedRoutes.OrderBy(r => r.Distance);
        }
        else if (criteria.OptimizeForCost)
        {
            optimizedRoutes = optimizedRoutes.OrderBy(r => r.EstimatedCost);
        }

        // Apply avoidance criteria
        if (criteria.AvoidTolls)
        {
            optimizedRoutes = optimizedRoutes.Where(r => r.TollCost == 0);
        }

        return optimizedRoutes.ToList();
    }

    private async Task<Dictionary<string, object>> GetRealTimeFactorsAsync(List<OptimizedRoute> routes)
    {
        var factors = new Dictionary<string, object>();
        
        foreach (var route in routes)
        {
            var trafficData = await GetCurrentTrafficDataAsync(route.RouteId);
            factors[route.RouteId] = trafficData;
        }

        return factors;
    }

    private OptimizedRoute SelectOptimalRoute(List<OptimizedRoute> routes, Dictionary<string, object> realTimeFactors, OptimizationCriteria criteria)
    {
        var scoredRoutes = new List<(OptimizedRoute Route, decimal Score)>();

        foreach (var route in routes)
        {
            decimal score = 0;

            if (criteria.OptimizeForTime)
            {
                score += 100 / (decimal)route.EstimatedDuration.TotalMinutes;
            }

            if (criteria.OptimizeForDistance)
            {
                score += 100 / route.Distance;
            }

            if (criteria.OptimizeForCost)
            {
                score += 100 / (route.EstimatedCost + 1); // +1 to avoid division by zero
            }

            // Apply traffic condition factor
            if (realTimeFactors.ContainsKey(route.RouteId) && realTimeFactors[route.RouteId] is TrafficData trafficData)
            {
                score *= GetTrafficMultiplier(trafficData.Condition);
            }

            scoredRoutes.Add((route, score));
        }

        return scoredRoutes.OrderByDescending(sr => sr.Score).First().Route;
    }

    private async Task UpdateRouteWithRealTimeData(OptimizedRoute route)
    {
        var trafficData = await GetCurrentTrafficDataAsync(route.RouteId);
        route.TrafficConditions = trafficData.Condition;

        // Adjust estimated duration based on traffic
        var trafficMultiplier = GetTrafficDelayMultiplier(trafficData.Condition);
        route.EstimatedDuration = TimeSpan.FromMinutes(route.EstimatedDuration.TotalMinutes * (double)trafficMultiplier);
    }

    private string GenerateRouteDescription(OptimizedRoute route)
    {
        return $"Route from {route.Origin} to {route.Destination} covering {route.Distance:F1} km in approximately {route.EstimatedDuration.TotalMinutes:F0} minutes";
    }

    private decimal CalculateRouteScore(OptimizedRoute route)
    {
        // Simple scoring algorithm - can be made more sophisticated
        decimal score = 50; // Base score

        // Distance factor (shorter is better)
        if (route.Distance < 50) score += 20;
        else if (route.Distance > 200) score -= 20;

        // Duration factor (faster is better)
        if (route.EstimatedDuration.TotalHours < 2) score += 15;
        else if (route.EstimatedDuration.TotalHours > 5) score -= 15;

        // Traffic factor
        score += GetTrafficScore(route.TrafficConditions);

        // Cost factor (cheaper is better)
        if (route.EstimatedCost < 100) score += 10;
        else if (route.EstimatedCost > 500) score -= 10;

        return Math.Max(0, Math.Min(100, score)); // Ensure score is between 0 and 100
    }

    private RecommendationReason DetermineRecommendationReason(OptimizedRoute route)
    {
        // Simple logic to determine recommendation reason
        if (route.EstimatedDuration == route.EstimatedDuration) return RecommendationReason.Fastest; // Placeholder logic
        if (route.Distance <= 100) return RecommendationReason.Shortest;
        if (route.EstimatedCost <= 200) return RecommendationReason.Cheapest;
        if (route.TrafficConditions == RouteTrafficCondition.Light) return RecommendationReason.BestTraffic;
        
        return RecommendationReason.MostReliable;
    }

    private List<string> GenerateRouteAdvantages(OptimizedRoute route)
    {
        var advantages = new List<string>();

        if (route.Distance < 100)
            advantages.Add("Short distance");

        if (route.EstimatedDuration.TotalHours < 2)
            advantages.Add("Quick travel time");

        if (route.EstimatedCost < 200)
            advantages.Add("Low cost");

        if (route.TrafficConditions == RouteTrafficCondition.Light)
            advantages.Add("Light traffic conditions");

        if (route.TollCost == 0)
            advantages.Add("No toll charges");

        return advantages;
    }

    private List<string> GenerateRouteDisadvantages(OptimizedRoute route)
    {
        var disadvantages = new List<string>();

        if (route.Distance > 300)
            disadvantages.Add("Long distance");

        if (route.EstimatedDuration.TotalHours > 4)
            disadvantages.Add("Long travel time");

        if (route.EstimatedCost > 500)
            disadvantages.Add("High cost");

        if (route.TrafficConditions == RouteTrafficCondition.Heavy || route.TrafficConditions == RouteTrafficCondition.Congested)
            disadvantages.Add("Heavy traffic expected");

        if (route.TollCost > 50)
            disadvantages.Add("Significant toll charges");

        if (route.Restrictions.Any())
            disadvantages.Add("Route restrictions apply");

        return disadvantages;
    }

    private async Task<List<PerformanceTrend>> CalculatePerformanceTrends(string routeId, TimeRange period)
    {
        // This would typically involve historical data analysis
        // For now, return empty list - can be implemented with actual historical data
        return new List<PerformanceTrend>();
    }

    private async Task<List<RouteIssue>> GetRouteIssuesInPeriod(string routeId, TimeRange period)
    {
        // This would typically query a separate issues/incidents table
        // For now, return empty list - can be implemented with actual incident tracking
        return new List<RouteIssue>();
    }

    private decimal GetTrafficMultiplier(RouteTrafficCondition condition)
    {
        return condition switch
        {
            RouteTrafficCondition.Light => 1.2m,
            RouteTrafficCondition.Normal => 1.0m,
            RouteTrafficCondition.Heavy => 0.7m,
            RouteTrafficCondition.Congested => 0.5m,
            RouteTrafficCondition.Blocked => 0.1m,
            _ => 1.0m
        };
    }

    private decimal GetTrafficDelayMultiplier(RouteTrafficCondition condition)
    {
        return condition switch
        {
            RouteTrafficCondition.Light => 0.9m,
            RouteTrafficCondition.Normal => 1.0m,
            RouteTrafficCondition.Heavy => 1.3m,
            RouteTrafficCondition.Congested => 1.6m,
            RouteTrafficCondition.Blocked => 2.0m,
            _ => 1.0m
        };
    }

    private decimal GetTrafficScore(RouteTrafficCondition condition)
    {
        return condition switch
        {
            RouteTrafficCondition.Light => 20,
            RouteTrafficCondition.Normal => 10,
            RouteTrafficCondition.Heavy => -5,
            RouteTrafficCondition.Congested => -15,
            RouteTrafficCondition.Blocked => -30,
            _ => 0
        };
    }

    private decimal CalculateCongestionLevel(RouteTrafficCondition condition)
    {
        return condition switch
        {
            RouteTrafficCondition.Light => 0.2m,
            RouteTrafficCondition.Normal => 0.5m,
            RouteTrafficCondition.Heavy => 0.7m,
            RouteTrafficCondition.Congested => 0.9m,
            RouteTrafficCondition.Blocked => 1.0m,
            _ => 0.5m
        };
    }

    private decimal CalculateRoadQuality(RouteConfiguration route) => 75m; // Placeholder
    private decimal CalculateSafetyScore(RouteConfiguration route) => 80m; // Placeholder
    private decimal CalculateTrafficScore(RouteConfiguration route) => GetTrafficScore(route.TrafficCondition) + 50m;
    private decimal CalculateWeatherScore(RouteConfiguration route) => 70m; // Placeholder
    private decimal CalculateInfrastructureScore(RouteConfiguration route) => 85m; // Placeholder
}