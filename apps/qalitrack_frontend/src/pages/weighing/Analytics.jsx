import React, { useEffect, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchTransactions } from "../../store/weighingSlice";
import dayjs from "dayjs";
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend
} from "recharts";

const COLORS = ["#10b981", "#f59e0b", "#ef4444", "#3b82f6", "#8b5cf6"];

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

  // ✅ NEW: Turnaround Time Trend (Daily Average TAT)
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

  // ✅ NEW: Hourly Performance (Transactions per hour)
  const hourlyPerformance = useMemo(() => {
    const map = {};
    
    // Initialize all 24 hours
    for (let i = 0; i < 24; i++) {
      const hour = i.toString().padStart(2, '0') + ':00';
      map[hour] = { hour, completed: 0, inProgress: 0, total: 0 };
    }
    
    // Count completed transactions by hour
    completedTx.forEach((t) => {
      const hour = dayjs(t.createdAt).format("HH") + ':00';
      if (map[hour]) {
        map[hour].completed += 1;
        map[hour].total += 1;
      }
    });
    
    // Count in-progress transactions by hour
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
    <div className="h-screen overflow-y-auto bg-gradient-to-br from-gray-50 to-gray-100">
      {/* COMPACT HEADER */}
      <div className="bg-white border-b border-gray-200 shadow-sm px-6 py-3 sticky top-0 z-10">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center shadow-md">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <div>
            <h1 className="text-lg font-bold text-gray-900">Analytics Dashboard</h1>
            <p className="text-xs text-gray-500 font-medium">
              Real-time insights from weighbridge transactions
            </p>
          </div>
        </div>
      </div>

      {/* SCROLLABLE CONTENT */}
      <div className="p-4 h-lvh">
        {/* COMPACT KPI CARDS */}
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-3 mb-4">
          {[
            ["Total Tickets", kpis.totalTx, "from-blue-500 to-blue-600"],
            ["Completed", kpis.completed, "from-green-500 to-green-600"],
            ["In Progress", kpis.inProgress, "from-orange-500 to-orange-600"],
            ["Total Net (kg)", kpis.totalNetWeight.toLocaleString(), "from-purple-500 to-purple-600"],
            ["Avg TAT (min)", kpis.avgTurnaround, "from-amber-500 to-amber-600"],
          ].map(([label, value, gradient]) => (
            <div
              key={label}
              className={`bg-gradient-to-br ${gradient} rounded-lg p-3 shadow-md text-white`}
            >
              <div className="text-[10px] font-semibold uppercase tracking-wide opacity-90">{label}</div>
              <div className="text-xl font-bold mt-1">{value}</div>
            </div>
          ))}
        </div>

        {/* COMPACT GRAPHS GRID */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* DAILY THROUGHPUT */}
          <div className="bg-white p-3 rounded-lg border shadow-sm">
            <h3 className="font-semibold text-sm mb-2 text-gray-700">Daily Throughput</h3>
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={dailyTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="day" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip contentStyle={{ fontSize: 12 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Line type="monotone" dataKey="count" stroke="#f59e0b" strokeWidth={2} name="Tickets" dot={{ r: 3 }} />
                <Line type="monotone" dataKey="weight" stroke="#10b981" strokeWidth={2} name="Net Weight (kg)" dot={{ r: 3 }} />
              </LineChart>
            </ResponsiveContainer>
          </div>

          {/* TRANSACTION STATUS PIE */}
          <div className="bg-white p-3 rounded-lg border shadow-sm">
            <h3 className="font-semibold text-sm mb-2 text-gray-700">Transaction Status</h3>
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie
                  data={statusPie}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={50}
                  outerRadius={80}
                  label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                  labelStyle={{ fontSize: 11, fontWeight: 600 }}
                >
                  {statusPie.map((_, i) => (
                    <Cell key={i} fill={COLORS[i]} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
          </div>

          {/* ✅ NEW: TURNAROUND TIME TREND */}
          <div className="bg-white p-3 rounded-lg border shadow-sm">
            <h3 className="font-semibold text-sm mb-2 text-gray-700">Average Turnaround Time (Daily)</h3>
            <ResponsiveContainer width="100%" height={220}>
              <LineChart data={tatTrend}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="day" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} label={{ value: 'Minutes', angle: -90, position: 'insideLeft', style: { fontSize: 11 } }} />
                <Tooltip 
                  contentStyle={{ fontSize: 12 }} 
                  formatter={(value) => [`${value} min`, 'Avg TAT']}
                />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Line 
                  type="monotone" 
                  dataKey="avgTAT" 
                  stroke="#ef4444" 
                  strokeWidth={2} 
                  name="Avg TAT (min)" 
                  dot={{ r: 3, fill: "#ef4444" }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>

          {/* ✅ NEW: HOURLY PERFORMANCE */}
          <div className="bg-white p-3 rounded-lg border shadow-sm">
            <h3 className="font-semibold text-sm mb-2 text-gray-700">Hourly Performance</h3>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={hourlyPerformance}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="hour" tick={{ fontSize: 10 }} interval={2} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip contentStyle={{ fontSize: 12 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Bar dataKey="completed" stackId="a" fill="#10b981" name="Completed" />
                <Bar dataKey="inProgress" stackId="a" fill="#f59e0b" name="In Progress" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* COMMODITIES BAR - FULL WIDTH */}
          <div className="bg-white p-3 rounded-lg border shadow-sm lg:col-span-2">
            <h3 className="font-semibold text-sm mb-2 text-gray-700">Top Commodities by Net Weight</h3>
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={commodityStats}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip 
                  contentStyle={{ fontSize: 12 }} 
                  formatter={(value) => [`${value.toLocaleString()} kg`, 'Net Weight']}
                />
                <Bar dataKey="weight" fill="#3b82f6" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {loading && (
          <div className="text-center text-sm text-gray-500 mt-4 py-2 bg-amber-50 rounded-lg border border-amber-200">
            Loading analytics data…
          </div>
        )}
      </div>
    </div>
  );
}