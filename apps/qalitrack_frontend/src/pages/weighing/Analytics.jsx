import React, { useCallback, useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchTransactions } from "../../store/weighingSlice";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import isBetween from "dayjs/plugin/isBetween";
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend,
  AreaChart, Area, RadarChart, Radar, PolarGrid, PolarAngleAxis, PolarRadiusAxis,
  ScatterChart, Scatter, ComposedChart
} from "recharts";
import {
  TrendingUp, TrendingDown, AlertTriangle, CheckCircle,
  Activity, Clock, Filter, X, BarChart3
} from "lucide-react";

dayjs.extend(relativeTime);
dayjs.extend(isBetween);

// Shared "nothing to chart yet" placeholder — same height as the chart it
// replaces, so panels don't jump size when data shows up.
function ChartEmpty({ message = "No data for this period", height = 200 }) {
  return (
    <div className="flex flex-col items-center justify-center text-center" style={{ height }}>
      <BarChart3 className="w-10 h-10 text-gray-300 mb-2" />
      <p className="text-gray-500 text-sm font-medium">{message}</p>
      <p className="text-gray-400 text-xs mt-1">Try a different time range or clear your filters</p>
    </div>
  );
}

// Amber color palette
const AMBER_COLORS = {
  darkest: "#78350f",
  darker: "#92400e",
  dark: "#b45309",
  medium: "#d97706",
  base: "#f59e0b",
  light: "#fbbf24",
  lighter: "#fcd34d",
  lightest: "#fde68a",
};

const PIE_COLORS = [AMBER_COLORS.medium, AMBER_COLORS.light, "#10b981", "#3b82f6", "#ef4444"];

export default function Analytics() {
  const dispatch = useDispatch();
  const { transactions, loading } = useSelector((state) => state.weighing);
  const [lastUpdated, setLastUpdated] = useState(dayjs());
  
  // NEW: Advanced filters
  const [timeRange, setTimeRange] = useState("all");
  const [showFilters, setShowFilters] = useState(false);
  const [selectedCommodity, setSelectedCommodity] = useState("all");
  const [selectedDriver, setSelectedDriver] = useState("all");
  const [alertsOnly, setAlertsOnly] = useState(false);

  // This used to unconditionally pull { pageSize: 10000 } — the entire
  // transaction table — every 30 seconds regardless of which time range was
  // selected, so the payload only grows as more tickets are recorded. Instead,
  // fetch a window sized to what the view actually needs: the selected range
  // plus the corresponding "previous period" used below for growth comparisons.
  const getFetchRange = useCallback((range) => {
    const now = dayjs();
    switch (range) {
      case "week":
        return { start: now.subtract(14, "day").startOf("day"), end: now.endOf("day") };
      case "month":
        return { start: now.subtract(60, "day").startOf("day"), end: now.endOf("day") };
      case "twoMonths":
        return { start: now.subtract(120, "day").startOf("day"), end: now.endOf("day") };
      case "all":
        return { start: null, end: null };
      case "today":
      case "yesterday":
      default:
        return { start: now.subtract(2, "day").startOf("day"), end: now.endOf("day") };
    }
  }, []);

  const loadTransactions = useCallback(() => {
    const { start, end } = getFetchRange(timeRange);
    const params = { pageSize: 10000 };
    if (start && end) {
      params.startDate = start.toISOString();
      params.endDate = end.toISOString();
    }
    dispatch(fetchTransactions(params));
    setLastUpdated(dayjs());
  }, [dispatch, timeRange, getFetchRange]);

  // Auto-refresh every 30 seconds
  useEffect(() => {
    loadTransactions();
    const interval = setInterval(loadTransactions, 30000);
    return () => clearInterval(interval);
  }, [loadTransactions]);

  // NEW: Filtered transactions based on time range and filters
  const filteredTransactions = useMemo(() => {
    let data = [...(transactions || [])];
    const now = dayjs();

    // Time range filter
    switch (timeRange) {
      case "today":
        data = data.filter(t => dayjs(t.createdAt).isSame(now, "day"));
        break;
      case "yesterday":
        data = data.filter(t => dayjs(t.createdAt).isSame(now.subtract(1, "day"), "day"));
        break;
      case "week":
        data = data.filter(t => dayjs(t.createdAt).isAfter(now.subtract(7, "day")));
        break;
      case "month":
        data = data.filter(t => dayjs(t.createdAt).isAfter(now.subtract(30, "day")));
        break;
      case "twoMonths":
        data = data.filter(t => dayjs(t.createdAt).isAfter(now.subtract(60, "day")));
        break;
      case "all":
      default:
        break;
    }

    // Commodity filter
    if (selectedCommodity !== "all") {
      data = data.filter(t => t.commodityName === selectedCommodity);
    }

    // Driver filter
    if (selectedDriver !== "all") {
      data = data.filter(t => t.driverName === selectedDriver);
    }

    return data;
  }, [transactions, timeRange, selectedCommodity, selectedDriver]);

  const completedTx = useMemo(
    () => filteredTransactions.filter(t => t.secondWeight && parseFloat(t.secondWeight) > 0),
    [filteredTransactions]
  );

  const inProgressTx = useMemo(
    () => filteredTransactions.filter(t => !t.secondWeight || parseFloat(t.secondWeight) === 0),
    [filteredTransactions]
  );

  // NEW: Get unique commodities and drivers for filters
  const commodities = useMemo(() => {
    const unique = [...new Set(transactions.map(t => t.commodityName).filter(Boolean))];
    return unique.sort();
  }, [transactions]);

  const drivers = useMemo(() => {
    const unique = [...new Set(transactions.map(t => t.driverName).filter(Boolean))];
    return unique.sort();
  }, [transactions]);

  // NEW: Advanced KPIs with comparisons
  const advancedKPIs = useMemo(() => {
    const totalNet = completedTx.reduce((sum, t) => sum + (parseFloat(t.netWeight) || 0), 0);
    
    // Calculate average TAT
    const avgTAT = completedTx.reduce((sum, t) => {
      if (t.turnaroundTime) {
        const parts = t.turnaroundTime.split(":");
        if (parts.length >= 2) return sum + parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10);
      }
      if (!t.firstWeightDate || !t.secondWeightDate) return sum;
      return sum + dayjs(t.secondWeightDate).diff(dayjs(t.firstWeightDate), "minute");
    }, 0) / (completedTx.length || 1);

    // NEW: Efficiency score (completed / total * 100)
    const efficiency = filteredTransactions.length > 0 
      ? (completedTx.length / filteredTransactions.length) * 100 
      : 0;

    // NEW: Average weight per transaction
    const avgWeight = completedTx.length > 0 ? totalNet / completedTx.length : 0;

    // NEW: Capacity utilization (assume 50 transactions per day is 100%)
    const targetPerDay = 50;
    const actualToday = filteredTransactions.filter(t => 
      dayjs(t.createdAt).isSame(dayjs(), "day")
    ).length;
    const capacityUtilization = (actualToday / targetPerDay) * 100;

    // NEW: Transactions per hour (last hour)
    const lastHour = filteredTransactions.filter(t =>
      dayjs(t.createdAt).isAfter(dayjs().subtract(1, "hour"))
    ).length;

    // NEW: Previous period comparison (for growth indicators)
    const now = dayjs();
    let previousPeriodData = [];
    
    switch (timeRange) {
      case "today":
        previousPeriodData = transactions.filter(t => 
          dayjs(t.createdAt).isSame(now.subtract(1, "day"), "day")
        );
        break;
      case "week":
        previousPeriodData = transactions.filter(t =>
          dayjs(t.createdAt).isBetween(now.subtract(14, "day"), now.subtract(7, "day"))
        );
        break;
      case "month":
        previousPeriodData = transactions.filter(t =>
          dayjs(t.createdAt).isBetween(now.subtract(60, "day"), now.subtract(30, "day"))
        );
        break;
      case "twoMonths":
        previousPeriodData = transactions.filter(t =>
          dayjs(t.createdAt).isBetween(now.subtract(120, "day"), now.subtract(60, "day"))
        );
        break;
      case "all":
        previousPeriodData = [];
        break;
      default:
        previousPeriodData = transactions.filter(t =>
          dayjs(t.createdAt).isSame(now.subtract(1, "day"), "day")
        );
    }

    const prevCompleted = previousPeriodData.filter(t => 
      t.secondWeight && parseFloat(t.secondWeight) > 0
    );
    const prevTotalNet = prevCompleted.reduce((sum, t) => 
      sum + (parseFloat(t.netWeight) || 0), 0
    );

    // Growth calculations
    const txGrowth = previousPeriodData.length > 0
      ? ((filteredTransactions.length - previousPeriodData.length) / previousPeriodData.length) * 100
      : 0;
    const weightGrowth = prevTotalNet > 0
      ? ((totalNet - prevTotalNet) / prevTotalNet) * 100
      : 0;

    return {
      totalTx: filteredTransactions.length,
      completed: completedTx.length,
      inProgress: inProgressTx.length,
      totalNetWeight: totalNet,
      avgTurnaround: Math.round(avgTAT),
      efficiency: Math.round(efficiency),
      avgWeight: Math.round(avgWeight),
      capacityUtilization: Math.round(capacityUtilization),
      txPerHour: lastHour,
      txGrowth: txGrowth.toFixed(1),
      weightGrowth: weightGrowth.toFixed(1),
    };
  }, [filteredTransactions, completedTx, inProgressTx, transactions, timeRange]);

  // NEW: Alerts system
  const alerts = useMemo(() => {
    const alertList = [];

    // Low efficiency alert
    if (advancedKPIs.efficiency < 70) {
      alertList.push({
        type: "warning",
        icon: AlertTriangle,
        message: `Low completion rate: ${advancedKPIs.efficiency}%`,
        color: "amber",
      });
    }

    // High TAT alert
    if (advancedKPIs.avgTurnaround > 60) {
      alertList.push({
        type: "warning",
        icon: Clock,
        message: `High turnaround time: ${advancedKPIs.avgTurnaround} min`,
        color: "red",
      });
    }

    // Capacity alert
    if (advancedKPIs.capacityUtilization > 90) {
      alertList.push({
        type: "info",
        icon: Activity,
        message: `Near capacity: ${advancedKPIs.capacityUtilization}%`,
        color: "blue",
      });
    }

    // Low activity alert
    if (advancedKPIs.txPerHour < 2 && dayjs().hour() >= 8 && dayjs().hour() <= 17) {
      alertList.push({
        type: "warning",
        icon: TrendingDown,
        message: `Low activity: ${advancedKPIs.txPerHour} tx/hour`,
        color: "amber",
      });
    }

    // Good performance
    if (advancedKPIs.efficiency >= 90 && advancedKPIs.avgTurnaround < 30) {
      alertList.push({
        type: "success",
        icon: CheckCircle,
        message: "Excellent performance!",
        color: "green",
      });
    }

    return alertList;
  }, [advancedKPIs]);

  const dailyTrend = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      const day = dayjs(t.createdAt).format("DD MMM");
      if (!map[day]) map[day] = { day, count: 0, weight: 0 };
      map[day].count += 1;
      map[day].weight += parseFloat(t.netWeight) || 0;
    });
    return Object.values(map).slice(-7); // Last 7 days
  }, [completedTx]);

  const tatTrend = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      const day = dayjs(t.createdAt).format("DD MMM");
      let tat = 0;
      if (t.turnaroundTime) {
        const parts = t.turnaroundTime.split(":");
        if (parts.length >= 2) tat = parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10);
      } else if (t.firstWeightDate && t.secondWeightDate) {
        tat = dayjs(t.secondWeightDate).diff(dayjs(t.firstWeightDate), "minute");
      } else return;
      
      if (!map[day]) map[day] = { day, totalTAT: 0, count: 0 };
      map[day].totalTAT += tat;
      map[day].count += 1;
    });
    
    return Object.values(map).map(item => ({
      day: item.day,
      avgTAT: Math.round(item.totalTAT / item.count)
    })).slice(-7);
  }, [completedTx]);

  const hourlyPerformance = useMemo(() => {
    const map = {};
    
    for (let i = 0; i < 24; i++) {
      const hour = i.toString().padStart(2, '0') + ':00';
      map[hour] = { hour, completed: 0, inProgress: 0, total: 0, avgTAT: 0, tatSum: 0, tatCount: 0 };
    }
    
    completedTx.forEach((t) => {
      const hour = dayjs(t.createdAt).format("HH") + ':00';
      if (map[hour]) {
        map[hour].completed += 1;
        map[hour].total += 1;
        
        // Calculate TAT for this transaction
        let tat = 0;
        if (t.turnaroundTime) {
          const parts = t.turnaroundTime.split(":");
          if (parts.length >= 2) tat = parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10);
        } else if (t.firstWeightDate && t.secondWeightDate) {
          tat = dayjs(t.secondWeightDate).diff(dayjs(t.firstWeightDate), "minute");
        }
        if (tat > 0) {
          map[hour].tatSum += tat;
          map[hour].tatCount += 1;
        }
      }
    });
    
    inProgressTx.forEach((t) => {
      const hour = dayjs(t.createdAt).format("HH") + ':00';
      if (map[hour]) {
        map[hour].inProgress += 1;
        map[hour].total += 1;
      }
    });

    // Calculate average TAT per hour
    Object.values(map).forEach(item => {
      if (item.tatCount > 0) {
        item.avgTAT = Math.round(item.tatSum / item.tatCount);
      }
    });
    
    return Object.values(map);
  }, [completedTx, inProgressTx]);

  // NEW: Performance by driver
  const driverPerformance = useMemo(() => {
    const map = {};
    
    completedTx.forEach(t => {
      const driver = t.driverName || "Unknown";
      if (!map[driver]) {
        map[driver] = { 
          driver, 
          trips: 0, 
          weight: 0, 
          tatSum: 0, 
          tatCount: 0 
        };
      }
      
      map[driver].trips += 1;
      map[driver].weight += parseFloat(t.netWeight) || 0;
      
      let tat = 0;
      if (t.turnaroundTime) {
        const parts = t.turnaroundTime.split(":");
        if (parts.length >= 2) tat = parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10);
      } else if (t.firstWeightDate && t.secondWeightDate) {
        tat = dayjs(t.secondWeightDate).diff(dayjs(t.firstWeightDate), "minute");
      }
      if (tat > 0) {
        map[driver].tatSum += tat;
        map[driver].tatCount += 1;
      }
    });
    
    return Object.values(map)
      .map(d => ({
        driver: d.driver,
        trips: d.trips,
        weight: Math.round(d.weight),
        avgTAT: d.tatCount > 0 ? Math.round(d.tatSum / d.tatCount) : 0,
      }))
      .sort((a, b) => b.weight - a.weight)
      .slice(0, 5);
  }, [completedTx]);

  // NEW: Radar chart data for multi-metric performance
  const radarData = useMemo(() => {
    const maxTx = Math.max(...driverPerformance.map(d => d.trips));
    const maxWeight = Math.max(...driverPerformance.map(d => d.weight));
    const maxTAT = Math.max(...driverPerformance.map(d => d.avgTAT));
    
    return driverPerformance.slice(0, 3).map(driver => ({
      driver: driver.driver,
      trips: maxTx > 0 ? (driver.trips / maxTx) * 100 : 0,
      weight: maxWeight > 0 ? (driver.weight / maxWeight) * 100 : 0,
      speed: maxTAT > 0 ? 100 - ((driver.avgTAT / maxTAT) * 100) : 0, // Inverse: lower TAT = higher score
    }));
  }, [driverPerformance]);

  const commodityStats = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      const key = t.commodityName || "Unknown";
      if (!map[key]) map[key] = { name: key, weight: 0, count: 0 };
      map[key].weight += parseFloat(t.netWeight) || 0;
      map[key].count += 1;
    });
    return Object.values(map).slice(0, 8);
  }, [completedTx]);

  const statusPie = [
    { name: "Completed", value: completedTx.length },
    { name: "In Progress", value: inProgressTx.length },
  ];

  // NEW: Clear all filters
  const clearFilters = () => {
    setTimeRange("all");
    setSelectedCommodity("all");
    setSelectedDriver("all");
    setAlertsOnly(false);
  };

  const activeFiltersCount = [
    timeRange !== "all",
    selectedCommodity !== "all",
    selectedDriver !== "all",
    alertsOnly
  ].filter(Boolean).length;

  return (
    <div className="h-full bg-gradient-to-br from-gray-50 to-gray-100 overflow-hidden flex flex-col">

      {/* HEADER */}
      <div className="shadow-sm px-4 sm:px-6 py-3 sm:py-4 shrink-0" style={{ backgroundColor: "var(--cs-appbar-bg)", borderBottom: "1px solid rgba(255,255,255,0.1)" }}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <div className="w-9 h-9 rounded-lg cs-icon-box flex items-center justify-center shadow-md shrink-0">
              <Activity className="w-5 h-5" style={{ color: "var(--cs-icon-accent)" }} />
            </div>
            <div>
              <div className="text-sm font-bold leading-tight" style={{ color: "var(--cs-appbar-text)" }}>Live Analytics Dashboard</div>
              <div className="text-[11px] font-medium leading-tight" style={{ color: "var(--cs-appbar-text)", opacity: 0.7 }}>
                Real-time insights • Auto-refresh every 30s
              </div>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className="text-right hidden sm:block">
              <div className="text-[10px] font-medium" style={{ color: "var(--cs-appbar-text)", opacity: 0.6 }}>Last updated</div>
              <div className="text-xs font-bold" style={{ color: "var(--cs-appbar-text)" }}>{lastUpdated.fromNow()}</div>
            </div>
            <button
              onClick={loadTransactions}
              disabled={loading}
              className="flex items-center gap-2 px-3 py-1.5 border cs-solid-chip-btn rounded-lg text-xs font-semibold shadow-sm transition-all disabled:opacity-50"
            >
              <Activity className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
              <span className="hidden sm:inline">Refresh</span>
            </button>
          </div>
        </div>
      </div>

      {/* FILTERS BAR */}
      <div className="bg-white border-b border-gray-200 px-4 sm:px-6 py-2 shrink-0">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            {/* Time Range */}
            <select
              value={timeRange}
              onChange={(e) => setTimeRange(e.target.value)}
              className="border border-amber-300 rounded px-2 py-1 text-xs font-medium focus:ring-2 focus:ring-amber-200"
            >
              <option value="all">All Time</option>
              <option value="today">Today</option>
              <option value="yesterday">Yesterday</option>
              <option value="week">Last 7 Days</option>
              <option value="month">Last 30 Days</option>
              <option value="twoMonths">Last 60 Days</option>
            </select>

            {/* Toggle Filters */}
            <button
              onClick={() => setShowFilters(!showFilters)}
              className={`flex items-center gap-1 px-2 py-1 rounded text-xs font-medium transition-all ${
                showFilters 
                  ? "bg-amber-500 text-white border border-amber-500" 
                  : "border border-gray-300 hover:border-amber-400"
              }`}
            >
              <Filter size={12} />
              Filters
              {activeFiltersCount > 0 && (
                <span className="bg-white text-amber-900 rounded-full w-4 h-4 flex items-center justify-center text-[10px] font-bold">
                  {activeFiltersCount}
                </span>
              )}
            </button>

            {activeFiltersCount > 0 && (
              <button
                onClick={clearFilters}
                className="flex items-center gap-1 px-2 py-1 border border-red-300 bg-red-50 text-red-700 rounded text-xs font-medium hover:bg-red-100"
              >
                <X size={12} />
                Clear
              </button>
            )}
          </div>
        </div>

        {/* Collapsible Filters */}
        {showFilters && (
          <div className="mt-2 pt-2 border-t flex gap-2 flex-wrap">
            <select
              value={selectedCommodity}
              onChange={(e) => setSelectedCommodity(e.target.value)}
              className="border border-gray-300 rounded px-2 py-1 text-xs"
            >
              <option value="all">All Commodities</option>
              {commodities.map(c => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>

            <select
              value={selectedDriver}
              onChange={(e) => setSelectedDriver(e.target.value)}
              className="border border-gray-300 rounded px-2 py-1 text-xs"
            >
              <option value="all">All Drivers</option>
              {drivers.map(d => (
                <option key={d} value={d}>{d}</option>
              ))}
            </select>
          </div>
        )}
      </div>

      {/* SCROLLABLE CONTENT */}
      <div className="flex-1 overflow-y-auto">
        <div className="p-3 sm:p-4 md:p-6 pb-20">
          {/* ALERTS */}
          {alerts.length > 0 && (
            <div className="mb-4 space-y-2">
              {alerts.map((alert, idx) => {
                const Icon = alert.icon;
                const colorClasses = {
                  amber: "bg-amber-50 border-amber-300 text-amber-900",
                  red: "bg-red-50 border-red-300 text-red-900",
                  blue: "bg-blue-50 border-blue-300 text-blue-900",
                  green: "bg-green-50 border-green-300 text-green-900",
                };
                
                return (
                  <div
                    key={idx}
                    className={`flex items-center gap-2 px-3 py-2 rounded-lg border ${colorClasses[alert.color]} text-xs font-semibold`}
                  >
                    <Icon size={14} />
                    {alert.message}
                  </div>
                );
              })}
            </div>
          )}

          {/* KPI CARDS */}
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-2 sm:gap-3 md:gap-4 mb-4 sm:mb-6">
            {[
              {
                label: "Total Tickets",
                value: advancedKPIs.totalTx,
                growth: advancedKPIs.txGrowth,
              },
              {
                label: "Completed",
                value: advancedKPIs.completed,
              },
              {
                label: "Efficiency",
                value: `${advancedKPIs.efficiency}%`,
              },
              {
                label: "Capacity",
                value: `${advancedKPIs.capacityUtilization}%`,
              },
              {
                label: "Avg TAT (min)",
                value: advancedKPIs.avgTurnaround,
              },
              {
                label: "Total Net (kg)",
                value: advancedKPIs.totalNetWeight.toLocaleString(),
                growth: advancedKPIs.weightGrowth,
              },
              {
                label: "Avg Weight",
                value: advancedKPIs.avgWeight.toLocaleString(),
              },
              {
                label: "Tx/Hour",
                value: advancedKPIs.txPerHour,
              },
            ].map(({ label, value, growth }) => (
              // Same recipe as ReportAnalytics' MetricCard: white card, amber
              // border, growth badge next to the label.
              <div
                key={label}
                className="bg-white border border-amber-200 rounded-lg p-3 sm:p-4 shadow-sm"
              >
                <div className="flex items-center justify-between gap-1">
                  <span className="text-[10px] sm:text-xs font-semibold uppercase tracking-wide text-gray-600">
                    {label}
                  </span>
                  {growth && (
                    <div className={`text-[10px] font-bold flex items-center gap-1 shrink-0 ${
                      parseFloat(growth) >= 0 ? "text-green-600" : "text-red-600"
                    }`}>
                      {parseFloat(growth) >= 0 ? <TrendingUp size={10} /> : <TrendingDown size={10} />}
                      {Math.abs(parseFloat(growth))}%
                    </div>
                  )}
                </div>
                <div className="text-lg sm:text-xl md:text-2xl font-bold mt-1 text-gray-900">{value}</div>
              </div>
            ))}
          </div>

          {/* GRAPHS GRID */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-3 sm:gap-4 md:gap-6">
            {/* DAILY THROUGHPUT */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Daily Throughput</h3>
              {dailyTrend.length === 0 ? <ChartEmpty /> : (
              <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
                <ComposedChart data={dailyTrend}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="day" tick={{ fontSize: 10 }} className="sm:text-xs" interval="preserveStartEnd" />
                  <YAxis tick={{ fontSize: 10 }} className="sm:text-xs" />
                  <Tooltip contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} />
                  <Legend wrapperStyle={{ fontSize: 10 }} />
                  <Area type="monotone" dataKey="count" fill={AMBER_COLORS.lighter} stroke={AMBER_COLORS.dark} name="Tickets" />
                  <Line type="monotone" dataKey="weight" stroke={AMBER_COLORS.base} strokeWidth={2} name="Weight (kg)" dot={{ r: 3 }} />
                </ComposedChart>
              </ResponsiveContainer>
              )}
            </div>

            {/* DRIVER PERFORMANCE RADAR */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Top Driver Performance</h3>
              {radarData.length === 0 ? <ChartEmpty /> : (
              <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
                <RadarChart data={radarData.length > 0 ? [
                  { metric: "Trips", ...radarData.reduce((acc, d) => ({ ...acc, [d.driver]: d.trips }), {}) },
                  { metric: "Weight", ...radarData.reduce((acc, d) => ({ ...acc, [d.driver]: d.weight }), {}) },
                  { metric: "Speed", ...radarData.reduce((acc, d) => ({ ...acc, [d.driver]: d.speed }), {}) },
                ] : []}>
                  <PolarGrid stroke="#e5e7eb" />
                  <PolarAngleAxis dataKey="metric" tick={{ fontSize: 10 }} />
                  <PolarRadiusAxis angle={90} domain={[0, 100]} tick={{ fontSize: 9 }} />
                  <Tooltip contentStyle={{ fontSize: 10 }} />
                  {radarData.map((driver, idx) => (
                    <Radar
                      key={driver.driver}
                      name={driver.driver}
                      dataKey={driver.driver}
                      stroke={PIE_COLORS[idx]}
                      fill={PIE_COLORS[idx]}
                      fillOpacity={0.3}
                    />
                  ))}
                  <Legend wrapperStyle={{ fontSize: 10 }} />
                </RadarChart>
              </ResponsiveContainer>
              )}
            </div>

            {/* TURNAROUND TIME TREND */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Average Turnaround Time</h3>
              {tatTrend.length === 0 ? <ChartEmpty /> : (
              <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
                <AreaChart data={tatTrend}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="day" tick={{ fontSize: 10 }} interval="preserveStartEnd" />
                  <YAxis tick={{ fontSize: 10 }} label={{ value: 'Minutes', angle: -90, position: 'insideLeft', style: { fontSize: 10 } }} />
                  <Tooltip contentStyle={{ fontSize: 11 }} formatter={(value) => [`${value} min`, 'Avg TAT']} />
                  <Area type="monotone" dataKey="avgTAT" stroke={AMBER_COLORS.darker} fill={AMBER_COLORS.light} fillOpacity={0.6} />
                </AreaChart>
              </ResponsiveContainer>
              )}
            </div>

            {/* TRANSACTION STATUS PIE */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Transaction Status</h3>
              {statusPie.every((d) => d.value === 0) ? <ChartEmpty /> : (
              <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
                <PieChart>
                  <Pie
                    data={statusPie}
                    dataKey="value"
                    nameKey="name"
                    cx="50%"
                    cy="50%"
                    innerRadius={40}
                    outerRadius={70}
                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    labelStyle={{ fontSize: 10, fontWeight: 600 }}
                  >
                    {statusPie.map((_, i) => (
                      <Cell key={i} fill={PIE_COLORS[i]} />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
              )}
            </div>

            {/* HOURLY PERFORMANCE */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Hourly Performance</h3>
              {hourlyPerformance.every((d) => d.total === 0) ? <ChartEmpty /> : (
              <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
                <BarChart data={hourlyPerformance}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="hour" tick={{ fontSize: 9 }} interval={3} />
                  <YAxis tick={{ fontSize: 10 }} />
                  <Tooltip contentStyle={{ fontSize: 11 }} />
                  <Legend wrapperStyle={{ fontSize: 10 }} />
                  <Bar dataKey="completed" stackId="a" fill={AMBER_COLORS.dark} name="Completed" />
                  <Bar dataKey="inProgress" stackId="a" fill={AMBER_COLORS.light} name="In Progress" />
                </BarChart>
              </ResponsiveContainer>
              )}
            </div>

            {/* TOP DRIVERS TABLE */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Top Drivers</h3>
              {driverPerformance.length === 0 ? <ChartEmpty height={140} /> : (
              <div className="space-y-2">
                {driverPerformance.map((driver, idx) => (
                  <div key={driver.driver} className="flex items-center justify-between p-2 bg-amber-50 rounded border border-amber-200">
                    <div className="flex items-center gap-2">
                      <span className="flex items-center justify-center w-6 h-6 bg-amber-500 text-white rounded-full text-xs font-bold">
                        {idx + 1}
                      </span>
                      <div>
                        <div className="text-xs font-semibold text-gray-900">{driver.driver}</div>
                        <div className="text-[10px] text-gray-600">{driver.trips} trips • {driver.avgTAT} min TAT</div>
                      </div>
                    </div>
                    <div className="text-xs font-bold text-amber-600">{driver.weight.toLocaleString()} kg</div>
                  </div>
                ))}
              </div>
              )}
            </div>

            {/* COMMODITIES BAR - FULL WIDTH */}
            <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow lg:col-span-2">
              <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Commodities by Weight & Count</h3>
              {commodityStats.length === 0 ? <ChartEmpty height={220} /> : (
              <ResponsiveContainer width="100%" height={220} className="sm:h-[240px] md:h-[260px]">
                <ComposedChart data={commodityStats}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                  <XAxis dataKey="name" tick={{ fontSize: 10 }} angle={-45} textAnchor="end" height={80} />
                  <YAxis yAxisId="left" tick={{ fontSize: 10 }} />
                  <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 10 }} />
                  <Tooltip contentStyle={{ fontSize: 11 }} />
                  <Legend wrapperStyle={{ fontSize: 10 }} />
                  <Bar yAxisId="left" dataKey="weight" fill={AMBER_COLORS.medium} radius={[6, 6, 0, 0]} name="Weight (kg)" />
                  <Line yAxisId="right" type="monotone" dataKey="count" stroke="#3b82f6" strokeWidth={2} name="Count" dot={{ r: 4 }} />
                </ComposedChart>
              </ResponsiveContainer>
              )}
            </div>
          </div>

          {loading && (
            <div className="text-center text-xs sm:text-sm text-gray-700 mt-4 sm:mt-6 py-3 bg-amber-50 rounded-lg border border-amber-300">
              Loading analytics data…
            </div>
          )}
        </div>
      </div>
    </div>
  );
}