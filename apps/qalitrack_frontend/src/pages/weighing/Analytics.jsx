import React, { useEffect, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchTransactions } from "../../store/weighingSlice";
import dayjs from "dayjs";
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend
} from "recharts";

// Amber color palette with different shades
const AMBER_COLORS = {
  darkest: "#78350f",   // amber-950
  darker: "#92400e",    // amber-900
  dark: "#b45309",      // amber-800
  medium: "#d97706",    // amber-700
  base: "#f59e0b",      // amber-500
  light: "#fbbf24",     // amber-400
  lighter: "#fcd34d",   // amber-300
  lightest: "#fde68a",  // amber-200
};

const PIE_COLORS = [AMBER_COLORS.medium, AMBER_COLORS.light];

export default function Analytics() {
  const dispatch = useDispatch();
  const { transactions, loading } = useSelector((state) => state.weighing);

  useEffect(() => {
    dispatch(fetchTransactions({ pageSize: 10000 }));
  }, [dispatch]);

  const completedTx = useMemo(
    () =>
      (transactions || []).filter(
        (t) => t.secondWeight && parseFloat(t.secondWeight) > 0
      ),
    [transactions]
  );

  const inProgressTx = useMemo(
    () =>
      (transactions || []).filter(
        (t) => !t.secondWeight || parseFloat(t.secondWeight) === 0
      ),
    [transactions]
  );

  const kpis = useMemo(() => {
    const totalNet = completedTx.reduce(
      (sum, t) => sum + (parseFloat(t.netWeight) || 0),
      0
    );

    const avgTAT =
      completedTx.reduce((sum, t) => {
        if (!t.firstWeightTime || !t.secondWeightTime) return sum;
        return (
          sum +
          dayjs(t.secondWeightTime).diff(dayjs(t.firstWeightTime), "minute")
        );
      }, 0) / (completedTx.length || 1);

    return {
      totalTx: transactions.length,
      completed: completedTx.length,
      inProgress: inProgressTx.length,
      totalNetWeight: totalNet,
      avgTurnaround: Math.round(avgTAT),
    };
  }, [transactions, completedTx, inProgressTx]);

  const dailyTrend = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      const day = dayjs(t.createdAt).format("DD MMM");
      if (!map[day]) map[day] = { day, count: 0, weight: 0 };
      map[day].count += 1;
      map[day].weight += parseFloat(t.netWeight) || 0;
    });
    return Object.values(map);
  }, [completedTx]);

  const tatTrend = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      if (!t.firstWeightTime || !t.secondWeightTime) return;
      
      const day = dayjs(t.createdAt).format("DD MMM");
      const tat = dayjs(t.secondWeightTime).diff(dayjs(t.firstWeightTime), "minute");
      
      if (!map[day]) map[day] = { day, totalTAT: 0, count: 0 };
      map[day].totalTAT += tat;
      map[day].count += 1;
    });
    
    return Object.values(map).map(item => ({
      day: item.day,
      avgTAT: Math.round(item.totalTAT / item.count)
    }));
  }, [completedTx]);

  const hourlyPerformance = useMemo(() => {
    const map = {};
    
    for (let i = 0; i < 24; i++) {
      const hour = i.toString().padStart(2, '0') + ':00';
      map[hour] = { hour, completed: 0, inProgress: 0, total: 0 };
    }
    
    completedTx.forEach((t) => {
      const hour = dayjs(t.createdAt).format("HH") + ':00';
      if (map[hour]) {
        map[hour].completed += 1;
        map[hour].total += 1;
      }
    });
    
    inProgressTx.forEach((t) => {
      const hour = dayjs(t.createdAt).format("HH") + ':00';
      if (map[hour]) {
        map[hour].inProgress += 1;
        map[hour].total += 1;
      }
    });
    
    return Object.values(map);
  }, [completedTx, inProgressTx]);

  const commodityStats = useMemo(() => {
    const map = {};
    completedTx.forEach((t) => {
      const key = t.commodityName || "Unknown";
      if (!map[key]) map[key] = { name: key, weight: 0 };
      map[key].weight += parseFloat(t.netWeight) || 0;
    });
    return Object.values(map).slice(0, 8);
  }, [completedTx]);

  const statusPie = [
    { name: "Completed", value: completedTx.length },
    { name: "In Progress", value: inProgressTx.length },
  ];

  return (
    <div className="h-screen bg-gradient-to-br from-gray-50 to-gray-100 overflow-y-scroll">

      {/* HEADER */}
      <div className="bg-white border-b border-gray-200 shadow-sm px-4 sm:px-6 py-3 sm:py-4 sticky top-0 z-10">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-lg bg-gradient-to-br from-amber-500 to-amber-600 flex items-center justify-center shadow-md">
            <svg className="w-4 h-4 sm:w-5 sm:h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <div>
            <h1 className="text-base sm:text-lg font-bold text-gray-900">Analytics Dashboard</h1>
            <p className="text-xs text-gray-500 font-medium hidden sm:block">
              Real-time insights from weighbridge transactions
            </p>
          </div>
        </div>
      </div>

      {/* CONTENT */}
      <div className="p-3 sm:p-4 md:p-6 pb-20">
        {/* KPI CARDS */}
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-2 sm:gap-3 md:gap-4 mb-4 sm:mb-6">
          {[
            ["Total Tickets", kpis.totalTx, "from-amber-300 to-amber-100"],
            ["Completed", kpis.completed, "from-amber-300 to-amber-100"],
            ["In Progress", kpis.inProgress, "from-amber-300 to-amber-100"],
            ["Total Net (kg)", kpis.totalNetWeight.toLocaleString(), "from-amber-300 to-amber-100"],
            ["Avg TAT (min)", kpis.avgTurnaround, "from-amber-300 to-amber-100"],
          ].map(([label, value, gradient]) => (
            <div
              key={label}
              className={`bg-gradient-to-br ${gradient} rounded-lg p-3 sm:p-4 shadow-md text-amber-900 transform transition-transform hover:scale-105`}
            >
              <div className="text-[10px] sm:text-xs font-semibold uppercase tracking-wide opacity-90">{label}</div>
              <div className="text-lg sm:text-xl md:text-2xl font-bold mt-1">{value}</div>
            </div>
          ))}
        </div>

        {/* GRAPHS GRID */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-3 sm:gap-4 md:gap-6">
          {/* DAILY THROUGHPUT */}
          <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
            <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Daily Throughput</h3>
            <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
              <LineChart data={dailyTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis 
                  dataKey="day" 
                  tick={{ fontSize: 10 }} 
                  className="sm:text-xs"
                  interval="preserveStartEnd"
                />
                <YAxis tick={{ fontSize: 10 }} className="sm:text-xs" />
                <Tooltip 
                  contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} 
                  className="sm:text-xs"
                />
                <Legend wrapperStyle={{ fontSize: 10 }} className="sm:text-xs" />
                <Line 
                  type="monotone" 
                  dataKey="count" 
                  stroke={AMBER_COLORS.dark} 
                  strokeWidth={2} 
                  name="Tickets" 
                  dot={{ r: 3, fill: AMBER_COLORS.dark }} 
                />
                <Line 
                  type="monotone" 
                  dataKey="weight" 
                  stroke={AMBER_COLORS.base} 
                  strokeWidth={2} 
                  name="Net Weight (kg)" 
                  dot={{ r: 3, fill: AMBER_COLORS.base }} 
                />
              </LineChart>
            </ResponsiveContainer>
          </div>

          {/* TRANSACTION STATUS PIE */}
          <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
            <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Transaction Status</h3>
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
                  labelStyle={{ fontSize: 10, fontWeight: 600, fill: '#000' }}
                  className="sm:text-xs"
                >
                  {statusPie.map((_, i) => (
                    <Cell key={i} fill={PIE_COLORS[i]} />
                  ))}
                </Pie>
                <Tooltip 
                  contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} 
                  className="sm:text-xs"
                />
              </PieChart>
            </ResponsiveContainer>
          </div>

          {/* TURNAROUND TIME TREND */}
          <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
            <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Average Turnaround Time (Daily)</h3>
            <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
              <LineChart data={tatTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis 
                  dataKey="day" 
                  tick={{ fontSize: 10 }} 
                  className="sm:text-xs"
                  interval="preserveStartEnd"
                />
                <YAxis 
                  tick={{ fontSize: 10 }} 
                  className="sm:text-xs"
                  label={{ 
                    value: 'Minutes', 
                    angle: -90, 
                    position: 'insideLeft', 
                    style: { fontSize: 10, fill: '#000' } 
                  }} 
                />
                <Tooltip 
                  contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} 
                  formatter={(value) => [`${value} min`, 'Avg TAT']}
                  className="sm:text-xs"
                />
                <Legend wrapperStyle={{ fontSize: 10 }} className="sm:text-xs" />
                <Line 
                  type="monotone" 
                  dataKey="avgTAT" 
                  stroke={AMBER_COLORS.darker} 
                  strokeWidth={2} 
                  name="Avg TAT (min)" 
                  dot={{ r: 3, fill: AMBER_COLORS.darker }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>

          {/* HOURLY PERFORMANCE */}
          <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow">
            <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Hourly Performance</h3>
            <ResponsiveContainer width="100%" height={200} className="sm:h-[220px] md:h-[240px]">
              <BarChart data={hourlyPerformance}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis 
                  dataKey="hour" 
                  tick={{ fontSize: 9 }} 
                  interval={3}
                  className="sm:text-[10px]"
                />
                <YAxis tick={{ fontSize: 10 }} className="sm:text-xs" />
                <Tooltip 
                  contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} 
                  className="sm:text-xs"
                />
                <Legend wrapperStyle={{ fontSize: 10 }} className="sm:text-xs" />
                <Bar 
                  dataKey="completed" 
                  stackId="a" 
                  fill={AMBER_COLORS.dark} 
                  name="Completed" 
                />
                <Bar 
                  dataKey="inProgress" 
                  stackId="a" 
                  fill={AMBER_COLORS.light} 
                  name="In Progress" 
                />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* COMMODITIES BAR - FULL WIDTH */}
          <div className="bg-white p-3 sm:p-4 rounded-lg border border-gray-200 shadow-sm hover:shadow-md transition-shadow lg:col-span-2">
            <h3 className="font-semibold text-sm sm:text-base mb-3 text-gray-900">Top Commodities by Net Weight</h3>
            <ResponsiveContainer width="100%" height={220} className="sm:h-[240px] md:h-[260px]">
              <BarChart data={commodityStats}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis 
                  dataKey="name" 
                  tick={{ fontSize: 10 }} 
                  angle={-45}
                  textAnchor="end"
                  height={80}
                  className="sm:text-xs"
                />
                <YAxis tick={{ fontSize: 10 }} className="sm:text-xs" />
                <Tooltip 
                  contentStyle={{ fontSize: 11, backgroundColor: '#fff', border: '1px solid #e5e7eb' }} 
                  formatter={(value) => [`${value.toLocaleString()} kg`, 'Net Weight']}
                  className="sm:text-xs"
                />
                <Bar 
                  dataKey="weight" 
                  fill={AMBER_COLORS.medium} 
                  radius={[6, 6, 0, 0]} 
                />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {loading && (
          <div className="text-center text-xs sm:text-sm text-gray-700 mt-4 sm:mt-6 py-3 bg-amber-50 rounded-lg border border-amber-300">
            Loading analytics data…
          </div>
        )}
      </div>
    </div>
  );
}