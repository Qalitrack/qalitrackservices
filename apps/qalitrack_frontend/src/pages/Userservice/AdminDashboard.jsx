import React, { useState, useEffect } from "react";
import { Users, Settings, Activity, UserCheck, UserX, RefreshCw } from "lucide-react";
import CountUp from "react-countup";
import { fetchUsers } from "../../api/helpers/UserService/Users/users.js";
import { fetchRoles } from "../../api/helpers/UserService/Roles/Roles.js";
import UsersComponent from "./Users.jsx";
import Chart from "react-apexcharts";

export default function AdminDashboard() {
  const [stats, setStats] = useState([
    { label: "Total Users", value: 0, icon: <Users size={20} className="text-amber-500" />, color: "from-amber-400 to-amber-500" },
    { label: "Offline", value: 0, icon: <UserX size={20} className="text-gray-400" />, color: "from-gray-300 to-gray-400" },
    { label: "Online", value: 0, icon: <UserCheck size={20} className="text-green-500" />, color: "from-green-400 to-green-500" },
    { label: "Roles", value: 0, icon: <Settings size={20} className="text-blue-500" />, color: "from-blue-400 to-blue-500" },
  ]);

  const [loading, setLoading] = useState(true);
  const [rolesData, setRolesData] = useState([]);
  const [userGrowthData, setUserGrowthData] = useState({ dates: [], counts: [] });

  const fetchData = async (signal) => {
    try {
      setLoading(true);
      const [usersData, rolesData] = await Promise.all([
        fetchUsers(1, 1000, signal),
        fetchRoles(signal),
      ]);

      const allUsers = usersData.items || [];
      const offlineUsers = allUsers.filter(u => !u.isActive)?.length || 0;
      const onlineUsers = allUsers.filter(u => u.isActive)?.length || 0;
      
      setStats([
        { ...stats[0], value: usersData.totalCount },
        { ...stats[1], value: offlineUsers },
        { ...stats[2], value: onlineUsers },
        { ...stats[3], value: rolesData.length },
      ]);

      const roleNameCounts = {};
      rolesData.forEach(role => {
        roleNameCounts[role.name] = 0;
      });
      
      allUsers.forEach((user) => {
        if (user.roles && Array.isArray(user.roles)) {
          user.roles.forEach(roleName => {
            if (roleNameCounts.hasOwnProperty(roleName)) {
              roleNameCounts[roleName]++;
            }
          });
        }
      });

      const chartRolesData = rolesData.map(role => ({ 
        name: role.name, 
        value: roleNameCounts[role.name] || 0 
      }));
      setRolesData(chartRolesData);

      const sortedUsers = [...allUsers].sort((a, b) => 
        new Date(a.createdAt) - new Date(b.createdAt)
      );

      if (sortedUsers.length > 0) {
        const growthMap = {};
        sortedUsers.forEach(user => {
          const date = new Date(user.createdAt).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
          });
          
          if (!growthMap[date]) {
            growthMap[date] = 0;
          }
          growthMap[date]++;
        });

        const sortedDates = Object.keys(growthMap).sort((a, b) => new Date(a) - new Date(b));

        let cumulativeCount = 0;
        const cumulativeCounts = sortedDates.map(date => {
          cumulativeCount += growthMap[date];
          return cumulativeCount;
        });

        setUserGrowthData({ dates: sortedDates, counts: cumulativeCounts });
      }
    } catch (error) {
      if (error.name !== 'AbortError') {
        console.error("Failed to fetch dashboard data:", error);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const abortController = new AbortController();
    fetchData(abortController.signal);

    const interval = setInterval(() => {
      fetchData(abortController.signal);
    }, 30000);

    return () => {
      abortController.abort();
      if (interval) clearInterval(interval);
    };
  }, []);

  const handleRefresh = () => {
    const abortController = new AbortController();
    fetchData(abortController.signal);
  };

  const chartCommonOptions = {
    chart: { 
      toolbar: { show: false }, 
      foreColor: "#374151",
      zoom: { enabled: false },
      animations: {
        enabled: true,
        easing: "easeinout",
        speed: 800,
      }
    },
    dataLabels: { enabled: false },
    grid: { borderColor: "#e5e7eb", strokeDashArray: 3 },
    tooltip: { theme: "light" },
  };

  const userChartOptions = {
    ...chartCommonOptions,
    xaxis: { 
      categories: ["Total", "Offline", "Online", "Roles"], 
      labels: { style: { colors: "#6b7280", fontSize: "10px" } } 
    },
    yaxis: { labels: { style: { colors: "#6b7280", fontSize: "10px" } } },
    colors: ["#f59e0b"],
    plotOptions: {
      bar: {
        borderRadius: 4,
        columnWidth: '60%',
      }
    },
  };

  const userGrowthChartOptions = {
    ...chartCommonOptions,
    chart: {
      ...chartCommonOptions.chart,
      type: 'area',
    },
    xaxis: { 
      categories: userGrowthData.dates,
      labels: { 
        style: { colors: "#6b7280", fontSize: "9px" },
        rotate: -45,
        hideOverlappingLabels: true,
      },
    },
    yaxis: { 
      labels: { 
        style: { colors: "#6b7280", fontSize: "10px" },
        formatter: (value) => Math.round(value)
      }
    },
    colors: ["#3b82f6"],
    stroke: {
      curve: 'smooth',
      width: 2,
    },
    fill: {
      type: "gradient",
      gradient: {
        shadeIntensity: 1,
        opacityFrom: 0.4,
        opacityTo: 0.1,
      }
    },
  };

  const rolesChartOptions = {
    ...chartCommonOptions,
    chart: {
      ...chartCommonOptions.chart,
      type: 'donut',
    },
    labels: rolesData.map(r => r.name),
    colors: ["#f59e0b", "#3b82f6", "#10b981", "#ef4444", "#8b5cf6"],
    legend: {
      position: 'bottom',
      fontSize: '10px',
    },
    plotOptions: {
      pie: {
        donut: {
          size: '65%',
          labels: {
            show: true,
            total: {
              show: true,
              label: 'Total',
              fontSize: '12px',
              color: '#374151'
            }
          }
        }
      }
    }
  };

  return (
    <div className="h-screen overflow-hidden flex flex-col bg-gradient-to-br from-gray-50 to-gray-100">
      {/* Compact Header */}
      <div className="bg-white border-b shadow-sm px-4 py-2.5 flex items-center justify-between shrink-0">
        <div>
          <h1 className="text-base font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-[10px] text-gray-500">System overview & analytics</p>
        </div>
        <button
          onClick={handleRefresh}
          disabled={loading}
          className="flex items-center gap-1.5 px-3 py-1.5 bg-gradient-to-r from-amber-500 to-amber-600 text-white text-xs rounded-lg hover:shadow-md transition-all disabled:opacity-50"
        >
          <RefreshCw size={14} className={loading ? 'animate-spin' : ''} />
          <span>Refresh</span>
        </button>
      </div>

      <div className="flex-1 overflow-auto px-4 py-3">
        {/* Compact Stats Cards */}
        <div className="grid grid-cols-4 gap-3 mb-3">
          {stats.map((stat, idx) => (
            <div 
              key={idx} 
              className="bg-white rounded-lg shadow-sm hover:shadow-md transition-all duration-300 overflow-hidden"
            >
              <div className={`h-1 bg-gradient-to-r ${stat.color}`} />
              <div className="p-3 flex items-center justify-between">
                <div>
                  <p className="text-[10px] text-gray-500 uppercase tracking-wide">{stat.label}</p>
                  <p className="text-xl font-bold text-gray-900 mt-0.5">
                    {loading ? "..." : <CountUp end={stat.value} duration={1.5} separator="," />}
                  </p>
                </div>
                <div className="bg-gray-50 p-2 rounded-lg">{stat.icon}</div>
              </div>
            </div>
          ))}
        </div>

        {/* Compact Charts Grid */}
        <div className="grid grid-cols-3 gap-3 mb-3">
          <div className="bg-white rounded-lg shadow-sm hover:shadow-md transition-all p-3">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-xs font-bold text-gray-900">User Activity</h3>
              <span className="text-[9px] text-gray-500">{stats[0].value} total</span>
            </div>
            <Chart 
              options={userChartOptions} 
              series={[{ name: "Count", data: stats.map(s => s.value) }]} 
              type="bar" 
              height={180} 
            />
          </div>

          <div className="bg-white rounded-lg shadow-sm hover:shadow-md transition-all p-3">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-xs font-bold text-gray-900">User Growth</h3>
              <span className="text-[9px] text-gray-500">Cumulative</span>
            </div>
            {userGrowthData.dates.length > 0 ? (
              <Chart 
                options={userGrowthChartOptions} 
                series={[{ name: "Users", data: userGrowthData.counts }]} 
                type="area" 
                height={180} 
              />
            ) : (
              <div className="flex items-center justify-center h-[180px] text-xs text-gray-400">
                {loading ? "Loading..." : "No data"}
              </div>
            )}
          </div>

          <div className="bg-white rounded-lg shadow-sm hover:shadow-md transition-all p-3">
            <div className="flex items-center justify-between mb-2">
              <h3 className="text-xs font-bold text-gray-900">Role Distribution</h3>
              <span className="text-[9px] text-gray-500">{stats[3].value} roles</span>
            </div>
            {rolesData.length > 0 ? (
              <Chart 
                options={rolesChartOptions} 
                series={rolesData.map(r => r.value)} 
                type="donut" 
                height={180} 
              />
            ) : (
              <div className="flex items-center justify-center h-[180px] text-xs text-gray-400">
                {loading ? "Loading..." : "No roles"}
              </div>
            )}
          </div>
        </div>

        {/* Compact Users List */}
        <div className="bg-white rounded-lg shadow-sm">
          <div className="border-b px-4 py-2.5 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className="w-1 h-5 bg-gradient-to-b from-amber-500 to-amber-600 rounded-full" />
              <h3 className="text-sm font-bold text-gray-900">User Management</h3>
            </div>
            <span className="text-[10px] text-gray-500">2 users per page</span>
          </div>
          <div className="p-3">
            <UsersComponent compact={true} pageSize={2} />
          </div>
        </div>
      </div>

      <style>{`
        .apexcharts-tooltip {
          font-size: 11px !important;
        }
        .apexcharts-legend-text {
          font-size: 10px !important;
        }
        .apexcharts-xaxis-label,
        .apexcharts-yaxis-label {
          font-size: 9px !important;
        }
      `}</style>
    </div>
  );
}