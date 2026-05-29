import { useMemo, useState } from "react";
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  AreaChart, Area, ComposedChart,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend,
  ResponsiveContainer
} from "recharts";
import {
  TrendingUp, TrendingDown
} from "lucide-react";
import dayjs from "dayjs";
import isBetween from "dayjs/plugin/isBetween"; // ← FIX: Import the plugin

// ← FIX: Extend dayjs with the plugin
dayjs.extend(isBetween);

export default function ReportAnalytics({ transactions = [] }) {
  const [timeRange, setTimeRange] = useState("30days");
  const [viewMode, setViewMode] = useState("trends");

  // Safety check
  if (!Array.isArray(transactions)) {
    return (
      <div className="p-8 text-center">
        <p className="text-red-600 font-semibold">Error: Invalid transactions data</p>
        <p className="text-sm text-gray-600 mt-2">Expected an array, received: {typeof transactions}</p>
      </div>
    );
  }

  if (transactions.length === 0) {
    return (
      <div className="p-8 text-center bg-white border border-amber-200 rounded-lg">
        <p className="text-gray-600 font-semibold">No transaction data available</p>
        <p className="text-sm text-gray-500 mt-2">Load some transactions to see analytics</p>
      </div>
    );
  }

  // ==================== TIME SERIES DATA ====================
  const timeSeriesData = useMemo(() => {
    try {
      const days = timeRange === "7days" ? 7 : timeRange === "30days" ? 30 : 90;
      const now = dayjs();
      const data = [];

      for (let i = days - 1; i >= 0; i--) {
        const date = now.subtract(i, "day");
        const dayTransactions = transactions.filter(t => {
          try {
            return dayjs(t.createdAt).isSame(date, "day");
          } catch (e) {
            return false;
          }
        });

        const completed = dayTransactions.filter(t => 
          t.secondWeight && parseFloat(t.secondWeight) > 0
        );

        data.push({
          date: date.format("MMM DD"),
          transactions: dayTransactions.length,
          completed: completed.length,
          inProgress: dayTransactions.length - completed.length,
          totalWeight: dayTransactions.reduce((sum, t) => {
            const weight = parseFloat(t.netWeight);
            return sum + (isNaN(weight) ? 0 : weight);
          }, 0),
        });
      }

      return data;
    } catch (error) {
      return [];
    }
  }, [transactions, timeRange]);

  // ==================== PERIOD COMPARISON ====================
  const periodComparison = useMemo(() => {
    try {
      const days = timeRange === "7days" ? 7 : timeRange === "30days" ? 30 : 90;
      const now = dayjs();

      const currentPeriod = transactions.filter(t => {
        try {
          return dayjs(t.createdAt).isAfter(now.subtract(days, "day"));
        } catch (e) {
          return false;
        }
      });

      const previousPeriod = transactions.filter(t => {
        try {
          
          return dayjs(t.createdAt).isBetween(
            now.subtract(days * 2, "day"),
            now.subtract(days, "day")
          );
        } catch (e) {
          return false;
        }
      });

      const calcMetrics = (data) => {
        const weight = data.reduce((sum, t) => {
          const w = parseFloat(t.netWeight);
          return sum + (isNaN(w) ? 0 : w);
        }, 0);
        
        return {
          count: data.length,
          weight: weight,
          avgWeight: data.length > 0 ? weight / data.length : 0,
          completed: data.filter(t => t.secondWeight && parseFloat(t.secondWeight) > 0).length,
        };
      };

      const current = calcMetrics(currentPeriod);
      const previous = calcMetrics(previousPeriod);

      return {
        current,
        previous,
        growth: {
          count: previous.count > 0 ? ((current.count - previous.count) / previous.count) * 100 : 0,
          weight: previous.weight > 0 ? ((current.weight - previous.weight) / previous.weight) * 100 : 0,
          avgWeight: previous.avgWeight > 0 ? ((current.avgWeight - previous.avgWeight) / previous.avgWeight) * 100 : 0,
          completed: previous.completed > 0 ? ((current.completed - previous.completed) / previous.completed) * 100 : 0,
        },
      };
    } catch (error) {
      return {
        current: { count: 0, weight: 0, avgWeight: 0, completed: 0 },
        previous: { count: 0, weight: 0, avgWeight: 0, completed: 0 },
        growth: { count: 0, weight: 0, avgWeight: 0, completed: 0 },
      };
    }
  }, [transactions, timeRange]);

  // ==================== ENTITY PERFORMANCE ====================
  const entityPerformance = useMemo(() => {
    try {
      const drivers = {};
      const customers = {};
      const commodities = {};

      transactions.forEach(t => {
        try {
          // Drivers
          const driver = t.driverName || "Unknown";
          if (!drivers[driver]) drivers[driver] = { trips: 0, weight: 0 };
          drivers[driver].trips += 1;
          const driverWeight = parseFloat(t.netWeight);
          drivers[driver].weight += isNaN(driverWeight) ? 0 : driverWeight;

          // Customers
          const customer = t.destinationName || "Unknown";
          if (!customers[customer]) customers[customer] = { trips: 0, weight: 0 };
          customers[customer].trips += 1;
          const customerWeight = parseFloat(t.netWeight);
          customers[customer].weight += isNaN(customerWeight) ? 0 : customerWeight;

          // Commodities
          const commodity = t.commodityName || "Unknown";
          if (!commodities[commodity]) commodities[commodity] = { trips: 0, weight: 0 };
          commodities[commodity].trips += 1;
          const commodityWeight = parseFloat(t.netWeight);
          commodities[commodity].weight += isNaN(commodityWeight) ? 0 : commodityWeight;
        } catch (e) {
          // Skip invalid transaction
        }
      });

      const processEntity = (obj) => {
        return Object.entries(obj)
          .map(([key, val]) => ({
            name: key,
            trips: val.trips,
            weight: Math.round(val.weight),
            avgWeight: val.trips > 0 ? Math.round(val.weight / val.trips) : 0,
          }))
          .sort((a, b) => b.weight - a.weight)
          .slice(0, 5);
      };

      return {
        topDrivers: processEntity(drivers),
        topCustomers: processEntity(customers),
        topCommodities: processEntity(commodities),
      };
    } catch (error) {
      return {
        topDrivers: [],
        topCustomers: [],
        topCommodities: [],
      };
    }
  }, [transactions]);

  // ==================== HOURLY DISTRIBUTION ====================
  const hourlyDistribution = useMemo(() => {
    try {
      const hours = Array.from({ length: 24 }, (_, i) => ({
        hour: `${i.toString().padStart(2, "0")}:00`,
        transactions: 0,
      }));

      transactions.forEach(t => {
        try {
          const hour = dayjs(t.createdAt).hour();
          if (hour >= 0 && hour < 24) {
            hours[hour].transactions += 1;
          }
        } catch (e) {
          // Skip invalid date
        }
      });

      return hours;
    } catch (error) {
      return [];
    }
  }, [transactions]);

  // ==================== COMMODITY MIX ====================
  const commodityMix = useMemo(() => {
    try {
      const mix = {};
      
      transactions.forEach(t => {
        try {
          const commodity = t.commodityName || "Unknown";
          if (!mix[commodity]) mix[commodity] = { count: 0, weight: 0 };
          mix[commodity].count += 1;
          const weight = parseFloat(t.netWeight);
          mix[commodity].weight += isNaN(weight) ? 0 : weight;
        } catch (e) {
          // Skip invalid transaction
        }
      });

      return Object.entries(mix)
        .map(([name, data]) => ({
          name,
          value: data.weight,
          count: data.count,
        }))
        .sort((a, b) => b.value - a.value)
        .slice(0, 8);
    } catch (error) {
      return [];
    }
  }, [transactions]);

  const COLORS = ["#f59e0b", "#10b981", "#3b82f6", "#ef4444", "#8b5cf6", "#ec4899", "#14b8a6", "#f97316"];

  // ==================== METRIC CARD ====================
  const MetricCard = ({ title, current, previous, format = "number" }) => {
    const growth = previous > 0 ? ((current - previous) / previous) * 100 : 0;
    const isPositive = growth >= 0;

    const formatValue = (val) => {
      if (isNaN(val)) return "0";
      if (format === "weight") return `${Math.round(val).toLocaleString()} kg`;
      if (format === "percent") return `${val.toFixed(1)}%`;
      return Math.round(val).toLocaleString();
    };

    return (
      <div className="bg-white border border-amber-200 rounded-lg p-4 shadow-sm">
        <div className="flex items-center justify-between mb-2">
          <span className="text-xs font-semibold text-gray-600 uppercase tracking-wide">{title}</span>
          <div className={`flex items-center gap-1 text-xs font-bold ${isPositive ? "text-green-600" : "text-red-600"}`}>
            {isPositive ? <TrendingUp size={14} /> : <TrendingDown size={14} />}
            {Math.abs(growth).toFixed(1)}%
          </div>
        </div>
        <div className="text-2xl font-bold text-gray-900 mb-1">
          {formatValue(current)}
        </div>
        <div className="text-xs text-gray-500">
          Previous: {formatValue(previous)}
        </div>
      </div>
    );
  };

  return (
    <div className="space-y-4">
      {/* HEADER & CONTROLS */}
      <div className="bg-white border border-amber-200 rounded-lg p-4 sticky top-0 z-10 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-lg font-bold text-gray-900">Report Analytics Dashboard</h2>
            <p className="text-xs text-gray-600 mt-1">
              Analyzing {transactions.length.toLocaleString()} transactions
            </p>
          </div>

          <div className="flex flex-wrap gap-2">
            {/* Time Range */}
            <select
              value={timeRange}
              onChange={(e) => setTimeRange(e.target.value)}
              className="border border-amber-300 rounded-lg px-3 py-1.5 text-xs font-medium focus:ring-2 focus:ring-amber-200"
            >
              <option value="7days">Last 7 Days</option>
              <option value="30days">Last 30 Days</option>
              <option value="90days">Last 90 Days</option>
            </select>

            {/* View Mode */}
            <select
              value={viewMode}
              onChange={(e) => setViewMode(e.target.value)}
              className="border border-amber-300 rounded-lg px-3 py-1.5 text-xs font-medium focus:ring-2 focus:ring-amber-200"
            >
              <option value="trends">Trends</option>
              <option value="comparison">Comparison</option>
              <option value="distribution">Distribution</option>
              <option value="performance">Performance</option>
            </select>
          </div>
        </div>
      </div>

      {/* PERIOD COMPARISON METRICS */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <MetricCard
          title="Total Transactions"
          current={periodComparison.current.count}
          previous={periodComparison.previous.count}
        />
        <MetricCard
          title="Total Weight"
          current={periodComparison.current.weight}
          previous={periodComparison.previous.weight}
          format="weight"
        />
        <MetricCard
          title="Average Weight"
          current={periodComparison.current.avgWeight}
          previous={periodComparison.previous.avgWeight}
          format="weight"
        />
        <MetricCard
          title="Completed"
          current={periodComparison.current.completed}
          previous={periodComparison.previous.completed}
        />
      </div>

      {/* MAIN VISUALIZATIONS */}
      {viewMode === "trends" && timeSeriesData.length > 0 && (
        <>
          {/* TRANSACTION TRENDS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <h3 className="text-sm font-bold text-gray-900 mb-3">Transaction Trends</h3>
            <ResponsiveContainer width="100%" height={300}>
              <ComposedChart data={timeSeriesData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                <YAxis yAxisId="left" tick={{ fontSize: 11 }} />
                <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 11 }} />
                <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8, border: "1px solid #fbbf24" }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Area
                  yAxisId="left"
                  type="monotone"
                  dataKey="transactions"
                  fill="#fbbf24"
                  stroke="#f59e0b"
                  fillOpacity={0.3}
                  name="Total"
                />
                <Bar yAxisId="left" dataKey="completed" fill="#10b981" name="Completed" />
                <Bar yAxisId="left" dataKey="inProgress" fill="#ef4444" name="In Progress" />
                <Line
                  yAxisId="right"
                  type="monotone"
                  dataKey="totalWeight"
                  stroke="#3b82f6"
                  strokeWidth={2}
                  name="Weight (kg)"
                  dot={{ r: 3 }}
                />
              </ComposedChart>
            </ResponsiveContainer>
          </div>

          {/* WEIGHT TRENDS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <h3 className="text-sm font-bold text-gray-900 mb-3">Weight Analysis</h3>
            <ResponsiveContainer width="100%" height={250}>
              <AreaChart data={timeSeriesData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Area
                  type="monotone"
                  dataKey="totalWeight"
                  stroke="#f59e0b"
                  fill="#fbbf24"
                  fillOpacity={0.6}
                  name="Total Weight (kg)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </>
      )}

      {viewMode === "distribution" && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* HOURLY DISTRIBUTION */}
          {hourlyDistribution.length > 0 && (
            <div className="bg-white border border-amber-200 rounded-lg p-4">
              <h3 className="text-sm font-bold text-gray-900 mb-3">Hourly Distribution</h3>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={hourlyDistribution}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                  <XAxis dataKey="hour" tick={{ fontSize: 10 }} interval={2} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8 }} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                  <Bar dataKey="transactions" fill="#f59e0b" name="Transactions" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}

          {/* COMMODITY MIX */}
          {commodityMix.length > 0 && (
            <div className="bg-white border border-amber-200 rounded-lg p-4">
              <h3 className="text-sm font-bold text-gray-900 mb-3">Commodity Distribution</h3>
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={commodityMix}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {commodityMix.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{ fontSize: 11, borderRadius: 8 }}
                    formatter={(value) => `${Math.round(value).toLocaleString()} kg`}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          )}
        </div>
      )}

      {viewMode === "performance" && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* TOP DRIVERS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <h3 className="text-sm font-bold text-gray-900 mb-3">Top Drivers</h3>
            <div className="space-y-2">
              {entityPerformance.topDrivers.length > 0 ? (
                entityPerformance.topDrivers.map((driver, idx) => (
                  <div key={driver.name} className="flex items-center justify-between p-2 bg-amber-50 rounded border border-amber-200">
                    <div className="flex items-center gap-2">
                      <span className="flex items-center justify-center w-6 h-6 bg-amber-500 text-white rounded-full text-xs font-bold">
                        {idx + 1}
                      </span>
                      <div>
                        <div className="text-xs font-semibold text-gray-900">{driver.name}</div>
                        <div className="text-[10px] text-gray-600">{driver.trips} trips</div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-xs font-bold text-amber-600">{driver.weight.toLocaleString()}</div>
                      <div className="text-[10px] text-gray-600">kg</div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-xs text-gray-500 text-center py-4">No driver data available</p>
              )}
            </div>
          </div>

          {/* TOP CUSTOMERS */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <h3 className="text-sm font-bold text-gray-900 mb-3">Top Customers</h3>
            <div className="space-y-2">
              {entityPerformance.topCustomers.length > 0 ? (
                entityPerformance.topCustomers.map((customer, idx) => (
                  <div key={customer.name} className="flex items-center justify-between p-2 bg-green-50 rounded border border-green-200">
                    <div className="flex items-center gap-2">
                      <span className="flex items-center justify-center w-6 h-6 bg-green-500 text-white rounded-full text-xs font-bold">
                        {idx + 1}
                      </span>
                      <div>
                        <div className="text-xs font-semibold text-gray-900">{customer.name}</div>
                        <div className="text-[10px] text-gray-600">{customer.trips} trips</div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-xs font-bold text-green-600">{customer.weight.toLocaleString()}</div>
                      <div className="text-[10px] text-gray-600">kg</div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-xs text-gray-500 text-center py-4">No customer data available</p>
              )}
            </div>
          </div>

          {/* TOP COMMODITIES */}
          <div className="bg-white border border-amber-200 rounded-lg p-4">
            <h3 className="text-sm font-bold text-gray-900 mb-3">Top Commodities</h3>
            <div className="space-y-2">
              {entityPerformance.topCommodities.length > 0 ? (
                entityPerformance.topCommodities.map((commodity, idx) => (
                  <div key={commodity.name} className="flex items-center justify-between p-2 bg-blue-50 rounded border border-blue-200">
                    <div className="flex items-center gap-2">
                      <span className="flex items-center justify-center w-6 h-6 bg-blue-500 text-white rounded-full text-xs font-bold">
                        {idx + 1}
                      </span>
                      <div>
                        <div className="text-xs font-semibold text-gray-900">{commodity.name}</div>
                        <div className="text-[10px] text-gray-600">{commodity.trips} trips</div>
                      </div>
                    </div>
                    <div className="text-right">
                      <div className="text-xs font-bold text-blue-600">{commodity.weight.toLocaleString()}</div>
                      <div className="text-[10px] text-gray-600">kg</div>
                    </div>
                  </div>
                ))
              ) : (
                <p className="text-xs text-gray-500 text-center py-4">No commodity data available</p>
              )}
            </div>
          </div>
        </div>
      )}

      {viewMode === "comparison" && (
        <div className="bg-white border border-amber-200 rounded-lg p-4">
          <h3 className="text-sm font-bold text-gray-900 mb-3">Period Comparison</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart
              data={[
                {
                  name: "Transactions",
                  current: periodComparison.current.count,
                  previous: periodComparison.previous.count,
                },
                {
                  name: "Weight (kg)",
                  current: Math.round(periodComparison.current.weight),
                  previous: Math.round(periodComparison.previous.weight),
                },
                {
                  name: "Avg Weight",
                  current: Math.round(periodComparison.current.avgWeight),
                  previous: Math.round(periodComparison.previous.avgWeight),
                },
                {
                  name: "Completed",
                  current: periodComparison.current.completed,
                  previous: periodComparison.previous.completed,
                },
              ]}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
              <XAxis dataKey="name" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip contentStyle={{ fontSize: 11, borderRadius: 8 }} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar dataKey="current" fill="#f59e0b" name="Current Period" />
              <Bar dataKey="previous" fill="#94a3b8" name="Previous Period" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}