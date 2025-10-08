import React, { useState, useEffect } from "react";
import { Users as UsersIcon, Settings, Activity, UserCheck, UserX } from "lucide-react";
import CountUp from "react-countup";
import { fetchUsers } from "../../helpers/UserService/Users/users.js";
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import Users from "./Users.jsx";
import Chart from "react-apexcharts";

export default function AdminDashboard() {
  const [stats, setStats] = useState([
    { label: "Total Users", value: 0, icon: <UsersIcon size={24} className="text-amber-500" /> },
    { label: "Offline Users", value: 0, icon: <UserX size={24} className="text-gray-400" /> },
    { label: "Online Users", value: 0, icon: <UserCheck size={24} className="text-amber-500" /> },
    { label: "Active Roles", value: 0, icon: <Settings size={24} className="text-amber-500" /> },
  ]);

  const [loading, setLoading] = useState(true);
  const [rolesData, setRolesData] = useState([]);
  const [userGrowthData, setUserGrowthData] = useState({ dates: [], counts: [] });
  const [refreshInterval, setRefreshInterval] = useState(null);

  const fetchData = async (signal) => {
    try {
      setLoading(true);
      const [usersData, rolesData] = await Promise.all([
        fetchUsers(1, 1000, signal), // Fetch more users to get complete history
        fetchRoles(signal),
      ]);

      // Calculate offline and online users
      const allUsers = usersData.items || [];
      if (allUsers.length > 0) {
     
      }
      const offlineUsers = allUsers.filter(u => !u.isActive)?.length || 0;
      const onlineUsers = allUsers.filter(u => u.isActive)?.length || 0;
      
      // Update stats
      setStats([
        { ...stats[0], value: usersData.totalCount },
        { ...stats[1], value: offlineUsers },
        { ...stats[2], value: onlineUsers },
        { ...stats[3], value: rolesData.length },
      ]);

      // Compute real user counts per role using role names
      const roleNameCounts = {};
      rolesData.forEach(role => {
        roleNameCounts[role.name] = 0;
      });
      
      allUsers.forEach((user, index) => {
        if (user.roles && Array.isArray(user.roles)) {
          user.roles.forEach(roleName => {
            if (roleNameCounts.hasOwnProperty(roleName)) {
              roleNameCounts[roleName]++;
            }
          });
        }
      });


      // Set roles data for chart using real counts
      const chartRolesData = rolesData.map(role => ({ 
        name: role.name, 
        value: roleNameCounts[role.name] || 0 
      }));
      setRolesData(chartRolesData);

      // Process user growth data
      const sortedUsers = [...allUsers].sort((a, b) => 
        new Date(a.createdAt) - new Date(b.createdAt)
      );

      if (sortedUsers.length > 0) {
        // Group users by date
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

        // Sort dates chronologically
        const sortedDates = Object.keys(growthMap).sort((a, b) => new Date(a) - new Date(b));

        // Convert to cumulative user counts
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

    // Auto-refresh every 30 seconds
    const interval = setInterval(() => {
      fetchData(abortController.signal);
    }, 30000);

    setRefreshInterval(interval);

    return () => {
      abortController.abort();
      if (interval) clearInterval(interval);
    };
  }, []);

  // Manual refresh function
  const handleRefresh = () => {
    const abortController = new AbortController();
    fetchData(abortController.signal);
  };

  const chartCommonOptions = {
    chart: { 
      toolbar: { show: false }, 
      foreColor: "#000", 
      zoom: { enabled: false } 
    },
    dataLabels: { enabled: false },
    grid: { borderColor: "#00000033", strokeDashArray: 4 },
    tooltip: { theme: "dark" },
    animations: {
      enabled: true,
      easing: "easeout",
      speed: 1000,
      animateGradually: { enabled: true, delay: 150 },
      dynamicAnimation: { enabled: true, speed: 800 },
    },
    responsive: [
      { 
        breakpoint: 1024, 
        options: { 
          chart: { height: 220 } 
        } 
      },
      { 
        breakpoint: 640, 
        options: { 
          chart: { height: 200 } 
        } 
      },
    ],
  };

  const userChartOptions = {
    ...chartCommonOptions,
    xaxis: { 
      categories: ["Total Users", "Offline Users", "Online Users", "Active Roles"], 
      labels: { style: { colors: "#000" } } 
    },
    yaxis: { labels: { style: { colors: "#000" } } },
    colors: ["#f59e0b"],
    fill: {
      type: "gradient",
      gradient: { 
        shade: "light", 
        type: "vertical", 
        shadeIntensity: 0.5, 
        gradientToColors: ["#fbbf24"], 
        opacityFrom: 0.9, 
        opacityTo: 0.9 
      },
    },
  };

  const userGrowthChartOptions = {
    ...chartCommonOptions,
    chart: {
      ...chartCommonOptions.chart,
      type: 'line',
    },
    xaxis: { 
      categories: userGrowthData.dates,
      labels: { 
        style: { colors: "#000" },
        rotate: -45,
        rotateAlways: false,
        hideOverlappingLabels: true,
        trim: true,
        maxHeight: 120
      },
      tickPlacement: 'on'
    },
    yaxis: { 
      labels: { 
        style: { colors: "#000" },
        formatter: (value) => Math.round(value)
      },
      title: {
        text: 'Cumulative Users',
        style: { color: "#000", fontSize: '12px', fontWeight: 600 }
      }
    },
    colors: ["#dc2626"],
    stroke: {
      curve: 'smooth',
      width: 5,
    },
    fill: {
      type: "gradient",
      gradient: {
        shade: "light",
        type: "vertical",
        shadeIntensity: 0.25,
        gradientToColors: ["#f87171"],
        opacityFrom: 0.7,
        opacityTo: 0.3,
        stops: [0, 100]
      }
    },
    tooltip: {
      theme: "dark",
      x: {
        show: true
      },
      y: {
        formatter: (value) => `${Math.round(value)} total users`,
        title: {
          formatter: () => 'Cumulative: '
        }
      }
    }
  };

  const rolesChartOptions = {
    ...chartCommonOptions,
    xaxis: { 
      categories: rolesData.map(r => r.name), 
      labels: { style: { colors: "#000" } } 
    },
    yaxis: { labels: { style: { colors: "#000" } } },
    colors: ["#f59e0b"],
    fill: { 
      type: "gradient", 
      gradient: { 
        shade: "light", 
        type: "vertical", 
        gradientToColors: ["#fbbf24"], 
        opacityFrom: 0.9, 
        opacityTo: 0.6 
      } 
    },
  };

  const systemHealthOptions = {
    chart: { 
      type: "radialBar", 
      sparkline: { enabled: true }, 
      animations: { enabled: true, easing: "easeout", speed: 1200 } 
    },
    plotOptions: {
      radialBar: { 
        hollow: { size: "50%" }, 
        dataLabels: { 
          show: true, 
          name: { show: false }, 
          value: { fontSize: "20px", color: "#000" } 
        } 
      },
    },
    colors: ["#f59e0b"],
  };

  return (
    <div className="p-4 max-w-7xl mx-auto">
      <div className="mb-8 flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Welcome to the Admin Dashboard</h1>
          <p className="text-gray-500 mt-1">Overview of your system stats and activity</p>
        </div>
        <button
          onClick={handleRefresh}
          disabled={loading}
          className="flex items-center gap-2 px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600 transition-colors shadow disabled:opacity-50 disabled:cursor-not-allowed"
          title="Refresh Dashboard Data"
        >
          <svg 
            xmlns="http://www.w3.org/2000/svg" 
            className={`h-5 w-5 ${loading ? 'animate-spin' : ''}`}
            viewBox="0 0 20 20" 
            fill="currentColor"
          >
            <path 
              fillRule="evenodd" 
              d="M4 2a1 1 0 011 1v2.101a7.002 7.002 0 0111.601 2.566 1 1 0 11-1.885.666A5.002 5.002 0 005.999 7H9a1 1 0 010 2H4a1 1 0 01-1-1V3a1 1 0 011-1zm.008 9.057a1 1 0 011.276.61A5.002 5.002 0 0014.001 13H11a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0v-2.101a7.002 7.002 0 01-11.601-2.566 1 1 0 01.61-1.276z" 
              clipRule="evenodd" 
            />
          </svg>
          <span className="hidden sm:inline">Refresh</span>
        </button>
      </div>

      {/* User Stats Cards with Animated Counters */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {stats.map((stat, idx) => (
          <div 
            key={idx} 
            className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow duration-300 flex flex-col justify-between"
          >
            <div className="flex items-center justify-between mb-4">
              <div>
                <p className="text-sm text-gray-500">{stat.label}</p>
                <p className="text-2xl font-semibold mt-1">
                  {loading ? "..." : <CountUp end={stat.value} duration={1.5} separator="," />}
                </p>
              </div>
              <div className="bg-gray-50 p-3 rounded-full">{stat.icon}</div>
            </div>
          </div>
        ))}
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">User Activity ({stats[0].value} users, {stats[1].value} offline, {stats[2].value} online)</h3>
          <Chart 
            options={userChartOptions} 
            series={[{ name: "Users", data: stats.map(s => s.value) }]} 
            type="bar" 
            height={250} 
          />
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">User Growth Trend ({stats[0].value} total users)</h3>
          {userGrowthData.dates.length > 0 ? (
            <Chart 
              options={userGrowthChartOptions} 
              series={[{ name: "Cumulative Users", data: userGrowthData.counts }]} 
              type="line" 
              height={250} 
            />
          ) : (
            <div className="flex items-center justify-center h-[250px] text-gray-500">
              {loading ? "Loading user growth data..." : "No user data available"}
            </div>
          )}
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">Roles Overview ({stats[3].value} active roles)</h3>
          <Chart 
            options={rolesChartOptions} 
            series={[{ name: "Users per Role", data: rolesData.map(r => r.value) }]} 
            type="bar" 
            height={250} 
          />
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300 flex items-center justify-center">
          <div className="text-center">
            <h3 className="font-bold mb-4">System Health</h3>
            <Chart 
              options={systemHealthOptions} 
              series={[98]} 
              type="radialBar" 
              height={200} 
            />
          </div>
        </div>
      </div>

      {/* Users List */}
      <div className="mt-8 w-full max-w-12xl">
        <Users />
      </div>
    </div>
  );
}