import React, { useState, useEffect } from "react";
import { Users, Settings, UserCheck, UserX, RefreshCw, TrendingUp, Activity, Sparkles, Shield } from "lucide-react";
import CountUp from "react-countup";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";
import Chart from "react-apexcharts";

export default function AdminDashboard() {
  const [stats, setStats] = useState([
    {
      label: "Total Users",
      value: 0,
      icon: <Users size={22} />,
      accent: "#d97706",
      iconBg: "#fde68a",
      cardBg: "#fffbeb",
      change: "+12%",
      positive: true,
    },
    {
      label: "Offline",
      value: 0,
      icon: <UserX size={22} />,
      accent: "#6b7280",
      iconBg: "#e5e7eb",
      cardBg: "#f9fafb",
      change: "-3%",
      positive: false,
    },
    {
      label: "Online",
      value: 0,
      icon: <UserCheck size={22} />,
      accent: "#059669",
      iconBg: "#a7f3d0",
      cardBg: "#ecfdf5",
      change: "+18%",
      positive: true,
    },
    {
      label: "Roles",
      value: 0,
      icon: <Settings size={22} />,
      accent: "#d97706",
      iconBg: "#fde68a",
      cardBg: "#fffbeb",
      change: "+2",
      positive: true,
    },
  ]);

  const [loading, setLoading] = useState(true);
  const [rolesData, setRolesData] = useState([]);
  const [userGrowthData, setUserGrowthData] = useState({ dates: [], counts: [] });

  const fetchData = async (signal) => {
    try {
      setLoading(true);
      const [usersData, rolesDataRes] = await Promise.all([
        fetchUsers(1, 1000, signal),
        fetchRoles(signal),
      ]);

      const allUsers = usersData.items || [];
      const offlineUsers = allUsers.filter(u => !u.isActive)?.length || 0;
      const onlineUsers = allUsers.filter(u => u.isActive)?.length || 0;

      setStats(prev => [
        { ...prev[0], value: usersData.totalCount },
        { ...prev[1], value: offlineUsers },
        { ...prev[2], value: onlineUsers },
        { ...prev[3], value: rolesDataRes.length },
      ]);

      const roleNameCounts = {};
      rolesDataRes.forEach(role => { roleNameCounts[role.name] = 0; });
      allUsers.forEach((user) => {
        if (user.roles && Array.isArray(user.roles)) {
          user.roles.forEach(roleName => {
            if (roleNameCounts.hasOwnProperty(roleName)) {
              roleNameCounts[roleName]++;
            }
          });
        }
      });

      const chartRolesData = rolesDataRes.map(role => ({
        name: role.name,
        value: roleNameCounts[role.name] || 0,
      }));
      setRolesData(chartRolesData);

      const sortedUsers = [...allUsers].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
      if (sortedUsers.length > 0) {
        const growthMap = {};
        sortedUsers.forEach(user => {
          const date = new Date(user.createdAt).toLocaleDateString('en-US', {
            year: 'numeric', month: 'short', day: 'numeric'
          });
          if (!growthMap[date]) growthMap[date] = 0;
          growthMap[date]++;
        });
        const sortedDates = Object.keys(growthMap).sort((a, b) => new Date(a) - new Date(b));
        let cum = 0;
        const cumulativeCounts = sortedDates.map(date => { cum += growthMap[date]; return cum; });
        setUserGrowthData({ dates: sortedDates, counts: cumulativeCounts });
      }
    } catch (error) {
      if (error.name !== 'AbortError') console.error("Failed to fetch dashboard data:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const abortController = new AbortController();
    fetchData(abortController.signal);
    const interval = setInterval(() => fetchData(abortController.signal), 30000);
    return () => { abortController.abort(); clearInterval(interval); };
  }, []);

  const handleRefresh = () => {
    const abortController = new AbortController();
    fetchData(abortController.signal);
  };

  // ── Chart base styles (light theme) ──
  const chartBase = {
    chart: {
      toolbar: { show: false },
      foreColor: "#6b7280",
      background: "transparent",
      zoom: { enabled: false },
      animations: { enabled: true, easing: "easeinout", speed: 800 },
    },
    dataLabels: { enabled: false },
    grid: {
      borderColor: "#f3f4f6",
      strokeDashArray: 4,
      xaxis: { lines: { show: true } },
      yaxis: { lines: { show: true } },
    },
    tooltip: {
      theme: "light",
      style: { fontSize: "12px" },
      fillColor: "#ffffff",
      borderColor: "#d97706",
    },
  };

  const userChartOptions = {
    ...chartBase,
    xaxis: {
      categories: ["Total", "Offline", "Online", "Roles"],
      labels: { style: { colors: "#6b7280", fontSize: "11px", fontWeight: 600 } },
      axisBorder: { show: true, color: "#e5e7eb" },
      axisTicks: { show: false },
    },
    yaxis: { labels: { style: { colors: "#6b7280", fontSize: "11px", fontWeight: 600 } } },
    colors: ["#d97706"],
    plotOptions: {
      bar: { borderRadius: 8, columnWidth: "55%", distributed: false },
    },
    fill: {
      type: "gradient",
      gradient: {
        shade: "light",
        type: "vertical",
        shadeIntensity: 0.4,
        gradientToColors: ["#fbbf24"],
        opacityFrom: 1,
        opacityTo: 0.7,
      },
    },
  };

  const userGrowthChartOptions = {
    ...chartBase,
    chart: { ...chartBase.chart, type: "area" },
    xaxis: {
      categories: userGrowthData.dates,
      labels: {
        style: { colors: "#6b7280", fontSize: "10px", fontWeight: 600 },
        rotate: -45,
        hideOverlappingLabels: true,
      },
      axisBorder: { show: true, color: "#e5e7eb" },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: {
        style: { colors: "#6b7280", fontSize: "11px", fontWeight: 600 },
        formatter: (v) => Math.round(v),
      },
    },
    colors: ["#d97706"],
    stroke: { curve: "smooth", width: 3 },
    fill: {
      type: "gradient",
      gradient: { shade: "light", shadeIntensity: 1, opacityFrom: 0.25, opacityTo: 0.02, stops: [0, 90, 100] },
    },
  };

  const rolesChartOptions = {
    ...chartBase,
    chart: { ...chartBase.chart, type: "donut" },
    labels: rolesData.map(r => r.name),
    colors: ["#d97706", "#f59e0b", "#fbbf24", "#fcd34d", "#fde68a"],
    legend: {
      position: "bottom",
      fontSize: "11px",
      fontWeight: 600,
      labels: { colors: "#6b7280" },
      markers: { width: 12, height: 12, radius: 6 },
    },
    plotOptions: {
      pie: {
        donut: {
          size: "70%",
          labels: {
            show: true,
            total: { show: true, label: "Total", fontSize: "14px", fontWeight: 700, color: "#d97706" },
            value: { color: "#374151" },
          },
        },
      },
    },
    stroke: { width: 0 },
  };

  // ── Mini stat strip (below main cards) ──
  const miniStats = [
    { label: "Total Users", value: stats[0].value, icon: <Users size={18} />, accent: "#d97706", iconBg: "#fde68a" },
    { label: "Active Now", value: stats[2].value, icon: <div className="w-2.5 h-2.5 rounded-full" style={{ background: "#10b981", boxShadow: "0 0 6px #10b981" }} />, accent: "#059669", iconBg: "#a7f3d0" },
    { label: "Total Roles", value: stats[3].value, icon: <Shield size={18} />, accent: "#d97706", iconBg: "#fde68a" },
    { label: "Offline", value: stats[1].value, icon: <UserX size={18} />, accent: "#6b7280", iconBg: "#e5e7eb" },
  ];

  return (
    <div className="min-h-screen flex flex-col" style={{ background: "#fafafa" }}>

      {/* Header */}
      <div className="shrink-0" style={{ background: "#ffffff", borderBottom: "1px solid #e5e7eb", boxShadow: "0 1px 3px rgba(0,0,0,0.06)" }}>
        <div className="px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl flex items-center justify-center shadow-md" style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)" }}>
              <Sparkles size={20} color="#fff" />
            </div>
            <div>
              <h1 className="text-xl font-bold" style={{ color: "#111827" }}>Admin Dashboard</h1>
              <p className="text-xs" style={{ color: "#6b7280" }}>Analytics overview and insights</p>
            </div>
          </div>
          <button
            onClick={handleRefresh}
            disabled={loading}
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-semibold text-white transition-all disabled:opacity-50 disabled:cursor-not-allowed hover:shadow-md"
            style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)", boxShadow: "0 2px 6px rgba(217,119,6,0.35)" }}
          >
            <RefreshCw size={16} className={loading ? "animate-spin" : ""} />
            Refresh Data
          </button>
        </div>
      </div>

      {/* Scrollable body */}
      <div className="flex-1 overflow-auto px-6 py-6 space-y-6">

        {/* Main Stats Cards */}
        <div className="grid grid-cols-4 gap-5">
          {stats.map((stat, idx) => (
            <div
              key={idx}
              className="relative rounded-2xl overflow-hidden transition-all duration-200 hover:shadow-lg"
              style={{
                background: "#ffffff",
                border: "1px solid #e5e7eb",
                boxShadow: "0 1px 4px rgba(0,0,0,0.06)",
              }}
            >
              {/* subtle top accent strip */}
              <div className="h-1" style={{ background: `linear-gradient(90deg, ${stat.accent}, ${stat.iconBg})` }} />

              <div className="p-5">
                <div className="flex items-start justify-between mb-4">
                  <div
                    className="w-13 h-13 rounded-xl flex items-center justify-center transition-transform duration-200 hover:scale-105"
                    style={{ background: stat.iconBg, color: stat.accent }}
                  >
                    {stat.icon}
                  </div>
                  <span
                    className="px-2.5 py-0.5 rounded-full text-xs font-bold"
                    style={{
                      background: stat.positive ? "#ecfdf5" : "#f3f4f6",
                      color: stat.positive ? "#059669" : "#6b7280",
                      border: `1px solid ${stat.positive ? "#a7f3d0" : "#e5e7eb"}`,
                    }}
                  >
                    {stat.change}
                  </span>
                </div>
                <p className="text-xs font-semibold uppercase tracking-wide mb-0.5" style={{ color: "#9ca3af" }}>
                  {stat.label}
                </p>
                <p className="text-3xl font-bold" style={{ color: "#111827" }}>
                  {loading ? <span style={{ color: "#d1d5db" }}>…</span> : <CountUp end={stat.value} duration={2} separator="," />}
                </p>
              </div>
            </div>
          ))}
        </div>

        {/* Mini Stats Strip */}
        <div className="grid grid-cols-4 gap-4">
          {miniStats.map((ms, i) => (
            <div
              key={i}
              className="rounded-xl p-4 border transition-all duration-200 hover:shadow-md"
              style={{ background: "#ffffff", borderColor: "#e5e7eb", boxShadow: "0 1px 3px rgba(0,0,0,0.06)" }}
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg flex items-center justify-center" style={{ background: ms.iconBg, color: ms.accent }}>
                  {ms.icon}
                </div>
                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide" style={{ color: "#6b7280" }}>{ms.label}</p>
                  <p className="text-xl font-bold" style={{ color: "#111827" }}>
                    {loading ? "…" : <CountUp end={ms.value} duration={1.5} separator="," />}
                  </p>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Charts Row */}
        <div className="grid grid-cols-3 gap-5">
          {/* User Activity */}
          <div
            className="rounded-2xl p-5 transition-all duration-200 hover:shadow-lg"
            style={{ background: "#ffffff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}
          >
            <div className="mb-4">
              <h3 className="text-base font-bold flex items-center gap-2" style={{ color: "#111827" }}>
                <TrendingUp size={18} style={{ color: "#d97706" }} /> User Activity
              </h3>
              <p className="text-xs mt-0.5" style={{ color: "#9ca3af" }}>{stats[0].value} total users</p>
            </div>
            <Chart options={userChartOptions} series={[{ name: "Count", data: stats.map(s => s.value) }]} type="bar" height={220} />
          </div>

          {/* User Growth */}
          <div
            className="rounded-2xl p-5 transition-all duration-200 hover:shadow-lg"
            style={{ background: "#ffffff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}
          >
            <div className="mb-4">
              <h3 className="text-base font-bold flex items-center gap-2" style={{ color: "#111827" }}>
                <TrendingUp size={18} style={{ color: "#d97706" }} /> User Growth
              </h3>
              <p className="text-xs mt-0.5" style={{ color: "#9ca3af" }}>Cumulative over time</p>
            </div>
            {userGrowthData.dates.length > 0 ? (
              <Chart options={userGrowthChartOptions} series={[{ name: "Users", data: userGrowthData.counts }]} type="area" height={220} />
            ) : (
              <div className="flex items-center justify-center" style={{ height: 220 }}>
                <div className="text-center">
                  <div className="w-14 h-14 rounded-full flex items-center justify-center mx-auto mb-2" style={{ background: "#fde68a" }}>
                    <Activity size={22} style={{ color: "#d97706" }} />
                  </div>
                  <p className="text-sm font-medium" style={{ color: "#9ca3af" }}>{loading ? "Loading…" : "No growth data available"}</p>
                </div>
              </div>
            )}
          </div>

          {/* Role Distribution */}
          <div
            className="rounded-2xl p-5 transition-all duration-200 hover:shadow-lg"
            style={{ background: "#ffffff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}
          >
            <div className="mb-4">
              <h3 className="text-base font-bold flex items-center gap-2" style={{ color: "#111827" }}>
                <Settings size={18} style={{ color: "#d97706" }} /> Role Distribution
              </h3>
              <p className="text-xs mt-0.5" style={{ color: "#9ca3af" }}>{stats[3].value} total roles</p>
            </div>
            {rolesData.length > 0 ? (
              <Chart options={rolesChartOptions} series={rolesData.map(r => r.value)} type="donut" height={220} />
            ) : (
              <div className="flex items-center justify-center" style={{ height: 220 }}>
                <div className="text-center">
                  <div className="w-14 h-14 rounded-full flex items-center justify-center mx-auto mb-2" style={{ background: "#fde68a" }}>
                    <Settings size={22} style={{ color: "#d97706" }} />
                  </div>
                  <p className="text-sm font-medium" style={{ color: "#9ca3af" }}>{loading ? "Loading…" : "No roles available"}</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      <style>{`
        .apexcharts-tooltip {
          border-radius: 12px !important;
          box-shadow: 0 4px 16px rgba(217,119,6,0.2) !important;
        }
        .apexcharts-legend-text { font-weight: 600 !important; }
      `}</style>
    </div>
  );
}