import React, { useState, useEffect } from "react";
import { Users as UsersIcon, Settings, Activity, Clock } from "lucide-react";
import CountUp from "react-countup";
import { fetchUsers } from "../../helpers/UserService/Users/users.js";
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import { fetchShifts } from "../../helpers/UserService/Shifts/Shifts.js";
import Users from "./Users.jsx";
import Chart from "react-apexcharts";

export default function AdminDashboard() {
  const [stats, setStats] = useState([
    { label: "Total Users", value: 0, icon: <UsersIcon size={24} className="text-amber-500" /> },
    { label: "Inactive Users", value: 0, icon: <UsersIcon size={24} className="text-gray-400" /> },
    { label: "Active Roles", value: 0, icon: <Settings size={24} className="text-amber-500" /> },
  ]);

  const [shiftStats, setShiftStats] = useState([
    { label: "Active Shifts", value: 0, icon: <Clock size={24} className="text-amber-500" /> },
    { label: "Inactive Shifts", value: 0, icon: <Clock size={24} className="text-gray-400" /> },
    { label: "System Health", value: 98, icon: <Activity size={24} className="text-amber-500" /> },
  ]);

  const [loading, setLoading] = useState(true);
  const [monthlyUsers, setMonthlyUsers] = useState([]);
  const [monthlyShifts, setMonthlyShifts] = useState([]);
  const [rolesData, setRolesData] = useState([]);

  useEffect(() => {
    const abortController = new AbortController();
    const signal = abortController.signal;

    const fetchData = async () => {
      try {
        setLoading(true);
        const [usersData, rolesData, shiftsData] = await Promise.all([
          fetchUsers(1, 100, signal),
          fetchRoles(signal),
          fetchShifts(1, 100, signal),
        ]);

        const inactiveUsers = usersData.items?.filter(u => u.isDeleted)?.length || 0;
        const allShifts = shiftsData?.items || [];
        const activeShifts = allShifts.filter(s => s.isActive).length;
        const inactiveShifts = allShifts.filter(s => !s.isActive).length;

        setStats([
          { ...stats[0], value: usersData.totalCount },
          { ...stats[1], value: inactiveUsers },
          { ...stats[2], value: rolesData.length },
        ]);

        setShiftStats([
          { ...shiftStats[0], value: activeShifts },
          { ...shiftStats[1], value: inactiveShifts },
          shiftStats[2],
        ]);

        setRolesData(rolesData.map(role => ({ name: role.name, value: Math.floor(Math.random() * 10 + 5) })));

        const last7Days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        setMonthlyUsers(last7Days.map(() => Math.floor(Math.random() * 50 + 50)));
        setMonthlyShifts(last7Days.map(() => Math.floor(Math.random() * 10 + 5)));
      } catch (error) {
        console.error("Failed to fetch dashboard data:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
    return () => abortController.abort();
  }, []);

  const chartCommonOptions = {
    chart: { toolbar: { show: false }, foreColor: "#000", zoom: { enabled: false } },
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
      { breakpoint: 1024, options: { chart: { height: 220 }, plotOptions: { bar: { columnWidth: "60%" } } } },
      { breakpoint: 640, options: { chart: { height: 200 }, plotOptions: { bar: { columnWidth: "70%" } } } },
    ],
  };

  const userChartOptions = {
    ...chartCommonOptions,
    xaxis: { categories: ["Total Users", "Inactive Users", "Active Roles"], labels: { style: { colors: "#000" } } },
    yaxis: { labels: { style: { colors: "#000" } } },
    colors: ["#f59e0b"],
    fill: {
      type: "gradient",
      gradient: { shade: "light", type: "vertical", shadeIntensity: 0.5, gradientToColors: ["#fbbf24"], opacityFrom: 0.9, opacityTo: 0.9 },
    },
  };

  const shiftChartOptions = {
    ...chartCommonOptions,
    xaxis: { categories: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"], labels: { style: { colors: "#000" } } },
    yaxis: { labels: { style: { colors: "#000" } } },
    colors: ["#f59e0b"],
    fill: {
      type: "gradient",
      gradient: { shade: "light", type: "vertical", gradientToColors: ["#fbbf24"], opacityFrom: 0.8, opacityTo: 0.3 },
    },
    stroke: { curve: "smooth", width: 2 },
  };

  const rolesChartOptions = {
    ...chartCommonOptions,
    xaxis: { categories: rolesData.map(r => r.name), labels: { style: { colors: "#000" } } },
    yaxis: { labels: { style: { colors: "#000" } } },
    colors: ["#f59e0b"],
    fill: { type: "gradient", gradient: { shade: "light", type: "vertical", gradientToColors: ["#fbbf24"], opacityFrom: 0.9, opacityTo: 0.6 } },
  };

  const systemHealthOptions = {
    chart: { type: "radialBar", sparkline: { enabled: true }, animations: { enabled: true, easing: "easeout", speed: 1200 } },
    plotOptions: {
      radialBar: { hollow: { size: "50%" }, dataLabels: { show: true, name: { show: false }, value: { fontSize: "20px", color: "#000" } } },
    },
    colors: ["#f59e0b"],
  };

  return (
    <div className="p-4 max-w-7xl mx-auto">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-800">Welcome to the Admin Dashboard</h1>
        <p className="text-gray-500 mt-1">Overview of your system stats and activity</p>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">User Activity</h3>
          <Chart options={userChartOptions} series={[{ name: "Users", data: stats.map(s => s.value) }]} type="bar" height={250} />
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">Shift Activity</h3>
          <Chart options={shiftChartOptions} series={[{ name: "Shifts", data: monthlyShifts }]} type="area" height={250} />
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300">
          <h3 className="font-bold mb-4">Roles Distribution</h3>
          <Chart options={rolesChartOptions} series={[{ name: "Roles", data: rolesData.map(r => r.value) }]} type="bar" height={250} />
        </div>

        <div className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition-shadow duration-300 flex items-center justify-center">
          <div className="text-center">
            <h3 className="font-bold mb-4">System Health</h3>
            <Chart options={systemHealthOptions} series={[98]} type="radialBar" height={200} />
          </div>
        </div>
      </div>

      {/* Shift & System Cards with Animated Counters */}
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 mb-8">
        {shiftStats.map((stat, idx) => (
          <div key={idx} className="bg-white p-6 rounded-lg shadow hover:shadow-lg transition-shadow duration-300 flex flex-col justify-between">
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

      {/* Users List */}
      <div className="mt-8 w-full max-w-12xl">
        <Users />
      </div>
    </div>
  );
}
