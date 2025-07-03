using AnalyticsService.Core.Interfaces;
using MathNet.Numerics.Statistics;

namespace AnalyticsService.Core.Services;

public class MetricsCalculator : IMetricsCalculator
{
    public Task<double> CalculateSum(IEnumerable<double> values)
    {
        return Task.FromResult(values.Sum());
    }

    public Task<double> CalculateAverage(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        return Task.FromResult(valuesList.Any() ? valuesList.Average() : 0);
    }

    public Task<double> CalculateMedian(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        if (!valuesList.Any()) return Task.FromResult(0.0);
        
        return Task.FromResult(Statistics.Median(valuesList));
    }

    public Task<double> CalculateStandardDeviation(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        if (valuesList.Count < 2) return Task.FromResult(0.0);
        
        return Task.FromResult(Statistics.StandardDeviation(valuesList));
    }

    public Task<double> CalculatePercentile(IEnumerable<double> values, double percentile)
    {
        var valuesList = values.ToList();
        if (!valuesList.Any()) return Task.FromResult(0.0);
        
        return Task.FromResult(Statistics.Percentile(valuesList, (int)percentile));
    }

    public Task<double> CalculatePercentageChange(double currentValue, double previousValue)
    {
        if (previousValue == 0) return Task.FromResult(0.0);
        
        var change = ((currentValue - previousValue) / Math.Abs(previousValue)) * 100;
        return Task.FromResult(change);
    }

    public Task<string> DetermineTrend(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        if (valuesList.Count < 2) return Task.FromResult("Stable");

        var firstHalf = valuesList.Take(valuesList.Count / 2).Average();
        var secondHalf = valuesList.Skip(valuesList.Count / 2).Average();
        
        var changeThreshold = 0.05; // 5% change threshold
        var percentageChange = Math.Abs(secondHalf - firstHalf) / firstHalf;
        
        if (percentageChange < changeThreshold)
            return Task.FromResult("Stable");
        
        return Task.FromResult(secondHalf > firstHalf ? "Up" : "Down");
    }

    public Task<Dictionary<string, double>> CalculatePercentiles(IEnumerable<double> values, double[] percentiles)
    {
        var valuesList = values.ToList();
        var result = new Dictionary<string, double>();
        
        if (!valuesList.Any())
        {
            foreach (var p in percentiles)
                result[$"P{p}"] = 0.0;
            return Task.FromResult(result);
        }

        foreach (var percentile in percentiles)
        {
            result[$"P{percentile}"] = Statistics.Percentile(valuesList, (int)percentile);
        }

        return Task.FromResult(result);
    }
}