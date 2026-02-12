import React, { useState, useEffect, useCallback, useMemo } from "react";
import {
  Users,
  UserCheck,
  UserX,
  RefreshCw,
  Activity,
  Sparkles,
  Shield,
} from "lucide-react";
import CountUp from "react-countup";
import Chart from "react-apexcharts";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";

// ========================================
// Constants
// ========================================
const REFRESH_INTERVAL = 30000; // 30 seconds

const STAT_CONFIGS = [
  {
    id: "total",
    label: "Total Users",
    icon: Users,
    accent: "#d97706",
    iconBg: "#fde68a",
    cardBg: "#fffbeb",
  },
  {
    id: "offline",
    label: "Offline",
    icon: UserX,
    accent: "#6b7280",
    iconBg: "#e5e7eb",
    cardBg: "#f9fafb",
  },
  {
    id: "online",
    label: "Online",
    icon: UserCheck,
    accent: "#059669",
    iconBg: "#a7f3d0",
    cardBg: "#ecfdf5",
  },
  {
    id: "roles",
    label: "Roles",
    icon: Shield,
    accent: "#d97706",
    iconBg: "#fde68a",
    cardBg: "#fffbeb",
  },
];

// ========================================
// Utility Functions
// ========================================
const processUserGrowthData = (users) => {
  if (!users || users.length === 0) return { dates: [], counts: [] };

  const sortedUsers = [...users].sort(
    (a, b) => new Date(a.createdAt) - new Date(b.createdAt)
  );

  const growthMap = {};
  sortedUsers.forEach((user) => {
    const date = new Date(user.createdAt).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
    growthMap[date] = (growthMap[date] || 0) + 1;
  });

  const sortedDates = Object.keys(growthMap).sort(
    (a, b) => new Date(a) - new Date(b)
  );

  let cumulative = 0;
  const cumulativeCounts = sortedDates.map((date) => {
    cumulative += growthMap[date];
    return cumulative;
  });

  return { dates: sortedDates, counts: cumulativeCounts };
};

const processRolesData = (roles, users) => {
  if (!roles || !users) return [];

  const roleNameCounts = roles.reduce((acc, role) => {
    acc[role.name] = 0;
    return acc;
  }, {});

  users.forEach((user) => {
    if (user.roles && Array.isArray(user.roles)) {
      user.roles.forEach((roleName) => {
        if (roleNameCounts.hasOwnProperty(roleName)) {
          roleNameCounts[roleName]++;
        }
      });
    }
  });

  return roles.map((role) => ({
    name: role.name,
    value: roleNameCounts[role.name] || 0,
  }));
};

// ========================================
// Chart Configuration
// ========================================
const getChartBase = () => ({
  chart: {
    toolbar: { show: false },
    foreColor: "#6b7280",
    background: "transparent",
    zoom: { enabled: false },
    animations: {
      enabled: true,
      easing: "easeinout",
      speed: 800,
    },
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
});


const getUserGrowthChartOptions = (dates) => ({
  ...getChartBase(),
  chart: {
    ...getChartBase().chart,
    type: "area",
  },
  xaxis: {
    categories: dates || [],
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
      formatter: (v) => (v != null ? Math.round(v) : 0),
    },
  },
  colors: ["#d97706"],
  stroke: { curve: "smooth", width: 3 },
  fill: {
    type: "gradient",
    gradient: {
      shade: "light",
      shadeIntensity: 1,
      opacityFrom: 0.25,
      opacityTo: 0.02,
      stops: [0, 90, 100],
    },
  },
});

const getRolesChartOptions = (rolesData) => ({
  ...getChartBase(),
  chart: {
    ...getChartBase().chart,
    type: "donut",
  },
  labels: (rolesData || []).map((r) => r.name || 'Unknown'),
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
          total: {
            show: true,
            label: "Total",
            fontSize: "14px",
            fontWeight: 700,
            color: "#d97706",
          },
          value: { color: "#374151" },
        },
      },
    },
  },
  stroke: { width: 0 },
});

// ========================================
// Sub-Components
// ========================================
const StatCard = ({ stat, loading }) => {
  const Icon = stat.icon;

  return (
    <div
      className="relative overflow-hidden rounded-xl shadow-sm hover:shadow-md transition-all duration-300 border border-gray-100"
      style={{ backgroundColor: stat.cardBg }}
    >
      {/* Accent strip */}
      <div
        className="h-1 w-full"
        style={{ backgroundColor: stat.accent }}
        aria-hidden="true"
      />

      <div className="p-6">
        <div className="flex items-start justify-between mb-4">
          <div
            className="p-3 rounded-lg"
            style={{ backgroundColor: stat.iconBg }}
            aria-hidden="true"
          >
            <Icon size={24} style={{ color: stat.accent }} />
          </div>
        </div>

        <p className="text-sm font-medium text-gray-600 mb-1">{stat.label}</p>
        <p className="text-3xl font-bold text-gray-900">
          {loading ? (
            <span aria-live="polite">…</span>
          ) : (
            <CountUp
              end={stat.value}
              duration={1.5}
              separator=","
              aria-live="polite"
            />
          )}
        </p>
      </div>
    </div>
  );
};



const ChartCard = ({ title, subtitle, children, loading }) => (
  <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100 hover:shadow-md transition-shadow">
    <div className="flex items-center gap-2 mb-4">
      <Activity size={20} className="text-amber-600" aria-hidden="true" />
      <div className="flex-1">
        <h3 className="text-base font-bold text-gray-900">{title}</h3>
        {subtitle && <p className="text-xs text-gray-500 mt-0.5">{subtitle}</p>}
      </div>
    </div>
    {loading ? (
      <div
        className="flex items-center justify-center h-[220px]"
        role="status"
        aria-live="polite"
      >
        <p className="text-gray-400">Loading…</p>
      </div>
    ) : (
      children
    )}
  </div>
);

const EmptyState = ({ message }) => (
  <div
    className="flex items-center justify-center h-[220px] text-gray-400"
    role="status"
  >
    <p>{message}</p>
  </div>
);

// ========================================
// Main Component
// ========================================
export default function AdminDashboard() {
  const [stats, setStats] = useState(
    STAT_CONFIGS.map((config) => ({ ...config, value: 0 }))
  );
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [rolesData, setRolesData] = useState([]);
  const [userGrowthData, setUserGrowthData] = useState({
    dates: [],
    counts: [],
  });

  const fetchData = useCallback(async (signal) => {
    try {
      setLoading(true);
      setError(null);

      const [usersData, rolesDataRes] = await Promise.all([
        fetchUsers(1, 1000, signal),
        fetchRoles(signal),
      ]);

      const allUsers = usersData?.items || [];
      const offlineUsers = allUsers.filter((u) => !u.isActive)?.length || 0;
      const onlineUsers = allUsers.filter((u) => u.isActive)?.length || 0;

      setStats((prev) =>
        prev.map((stat, idx) => {
          const values = [
            usersData?.totalCount || 0,
            offlineUsers,
            onlineUsers,
            rolesDataRes?.length || 0,
          ];
          return { ...stat, value: values[idx] };
        })
      );

      const processedRoles = processRolesData(rolesDataRes, allUsers);
      setRolesData(processedRoles);

      const growthData = processUserGrowthData(allUsers);
      setUserGrowthData(growthData);
    } catch (err) {
      if (err.name !== "AbortError" && err.name !== "CanceledError") {
        console.error("Failed to fetch dashboard data:", err);
        setError("Failed to load dashboard data. Please try again.");
      }
      // Don't show error for canceled/aborted requests
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const abortController = new AbortController();
    fetchData(abortController.signal);

    const interval = setInterval(
      () => fetchData(abortController.signal),
      REFRESH_INTERVAL
    );

    return () => {
      abortController.abort();
      clearInterval(interval);
    };
  }, [fetchData]);

  const handleRefresh = useCallback(() => {
    const abortController = new AbortController();
    fetchData(abortController.signal);
  }, [fetchData]);

  // Memoized chart options
  const userGrowthChartOptions = useMemo(
    () => getUserGrowthChartOptions(userGrowthData.dates),
    [userGrowthData.dates.length] // Only update when length changes
  );
  const rolesChartOptions = useMemo(
    () => getRolesChartOptions(rolesData),
    [rolesData.length] // Only update when length changes
  );


  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-amber-100 rounded-lg" aria-hidden="true">
              <Sparkles size={24} className="text-amber-600" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                Admin Dashboard
              </h1>
              <p className="text-sm text-gray-500">
                Analytics overview and insights
              </p>
            </div>
          </div>
          <button
            onClick={handleRefresh}
            disabled={loading}
            className="flex items-center gap-2 px-4 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-sm"
            aria-label="Refresh dashboard data"
          >
            <RefreshCw size={16} className={loading ? "animate-spin" : ""} />
            <span className="font-medium text-sm">Refresh Data</span>
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-6 py-8 space-y-8">
        {/* Error Message */}
        {error && (
          <div
            className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg"
            role="alert"
          >
            <p className="font-medium">{error}</p>
          </div>
        )}

        {/* Main Stats Cards */}
        <section aria-labelledby="main-stats-heading">
          <h2 id="main-stats-heading" className="sr-only">
            Main Statistics
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {stats.map((stat) => (
              <StatCard key={stat.id} stat={stat} loading={loading} />
            ))}
          </div>
        </section>


        {/* Charts Row */}
        <section aria-labelledby="charts-heading">
          <h2 id="charts-heading" className="sr-only">
            Analytics Charts
          </h2>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* User Growth */}
            <ChartCard
              title="User Growth"
              subtitle="Cumulative over time"
              loading={loading}
            >
              {!loading && userGrowthData.dates.length > 0 && userGrowthData.counts.length > 0 ? (
                <Chart
                  options={userGrowthChartOptions}
                  series={[{ name: "Users", data: userGrowthData.counts }]}
                  type="area"
                  height={220}
                />
              ) : !loading ? (
                <EmptyState message="No growth data available" />
              ) : null}
            </ChartCard>

            {/* Role Distribution */}
            <ChartCard
              title="Role Distribution"
              subtitle={`${stats[3].value} total roles`}
              loading={loading}
            >
              {!loading && rolesData.length > 0 && rolesData.some(r => r.value > 0) ? (
                <Chart
                  options={rolesChartOptions}
                  series={rolesData.map((r) => r.value)}
                  type="donut"
                  height={220}
                />
              ) : !loading ? (
                <EmptyState message="No roles available" />
              ) : null}
            </ChartCard>
          </div>
        </section>
      </main>
    </div>
  );
}