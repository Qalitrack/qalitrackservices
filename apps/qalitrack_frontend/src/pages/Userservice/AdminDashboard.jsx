import React, { useState, useEffect } from "react";
import { BarChart3, Users as UsersIcon, Settings, Activity, Clock } from "lucide-react";
import { fetchUsers } from "../../helpers/UserService/Users/users.js";
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import { fetchShifts } from "../../helpers/UserService/Shifts/Shifts.js";
import Users from "./Users.jsx";
import Chart from "react-apexcharts";

export default function AdminDashboard() {
    const [stats, setStats] = useState([
        { label: "Total Users", value: "...", icon: <UsersIcon size={20} className="text-amber-500" />, progress: 70 },
        { label: "Inactive Users", value: "...", icon: <UsersIcon size={20} className="text-gray-500" />, progress: 30 },
        { label: "Active Roles", value: "...", icon: <Settings size={20} className="text-amber-500" />, progress: 80 },
        { label: "Active Shifts", value: "...", icon: <Clock size={20} className="text-amber-500" />, progress: 60 },
        { label: "Inactive Shifts", value: "...", icon: <Clock size={20} className="text-gray-400" />, progress: 40 },
        { label: "System Health", value: "98%", icon: <Activity size={20} className="text-amber-500" />, progress: 98 },
    ]);
    const [loading, setLoading] = useState(true);

    // Chart data
    const [monthlyUsers, setMonthlyUsers] = useState([]);
    const [monthlyShifts, setMonthlyShifts] = useState([]);

    useEffect(() => {
        const abortController = new AbortController();
        const signal = abortController.signal;

        const fetchData = async () => {
            try {
                setLoading(true);
                const [usersData, rolesData, shiftsResponse] = await Promise.all([
                    fetchUsers(1, 100, signal),
                    fetchRoles(signal),
                    fetchShifts(1, 100, signal)
                ]);

                const inactiveUsers = usersData.items?.filter(user => user.isDeleted)?.length || 0;
                const allShifts = shiftsResponse?.items || [];
                const activeShifts = allShifts.filter(shift => shift.isActive).length;
                const inactiveShifts = allShifts.filter(shift => !shift.isActive).length;

                // Update stats cards
                setStats(prevStats => [
                    { ...prevStats[0], value: usersData.totalCount.toString() },
                    { ...prevStats[1], value: inactiveUsers.toString() },
                    { ...prevStats[2], value: rolesData.length.toString() },
                    { ...prevStats[3], value: activeShifts.toString() },
                    { ...prevStats[4], value: inactiveShifts.toString() },
                    prevStats[5]
                ]);

                // Chart data: last 7 days
                const last7Days = Array.from({ length: 7 }, (_, i) => i + 1);
                const usersCountByDay = last7Days.map(day => Math.floor(Math.random() * 50 + 50));
                const shiftsCountByDay = last7Days.map(day => Math.floor(Math.random() * 10 + 5));

                setMonthlyUsers(usersCountByDay);
                setMonthlyShifts(shiftsCountByDay);

            } catch (error) {
                if (error.name !== "CanceledError") {
                    console.error("Failed to fetch dashboard data:", error);
                    setStats(prevStats => prevStats.map(stat => ({ ...stat, value: "N/A" })));
                }
            } finally {
                setLoading(false);
            }
        };

        fetchData();
        return () => abortController.abort();
    }, []);

    // Chart options
    const userChartOptions = {
        chart: { type: "area", height: 250, toolbar: { show: false }, foreColor: "#000000" },
        dataLabels: { enabled: false },
        stroke: { curve: "smooth" },
        colors: ["#f59e0b"], // amber-500
        grid: { borderColor: "#00000033", strokeDashArray: 4 },
        xaxis: {
            categories: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
            axisBorder: { show: true, color: "#000000" },
            axisTicks: { show: true, color: "#000000" },
            labels: { style: { colors: "#000000" } }
        },
        yaxis: { labels: { style: { colors: "#000000" } } },
        tooltip: { theme: "dark", x: { show: true }, y: { formatter: val => `${val} users` } }
    };

    const shiftChartOptions = {
        chart: { type: "bar", height: 250, toolbar: { show: false }, foreColor: "#000000" },
        plotOptions: { bar: { horizontal: false, columnWidth: "25%", borderRadius: 5 } },
        colors: ["#f59e0b"], // amber-500
        grid: { borderColor: "#00000033", strokeDashArray: 4 },
        xaxis: {
            categories: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
            axisBorder: { show: true, color: "#000000" },
            axisTicks: { show: true, color: "#000000" },
            labels: { style: { colors: "#000000" } }
        },
        yaxis: { labels: { style: { colors: "#000000" } } },
        tooltip: { theme: "dark", x: { show: true }, y: { formatter: val => `${val} shifts` } }
    };

    const greeting = "Welcome to the Admin Dashboard";

    return (
        <div className="p-4 max-w-7xl mx-auto">
            {/* Welcome Section */}
            <div className="mb-8">
                <h1 className="text-2xl font-bold text-gray-800">{greeting}</h1>
                <p className="text-gray-500 mt-1">Here's what's happening in your admin panel today</p>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                {stats.map((stat, index) => (
                    <div key={index} className="bg-white p-6 rounded-lg shadow-sm border border-gray-100 flex flex-col justify-between">
                        <div className="flex justify-between items-center mb-4">
                            <div>
                                <p className="text-sm text-gray-500">{stat.label}</p>
                                <p className="text-2xl font-semibold mt-1">{loading ? '...' : stat.value}</p>
                            </div>
                            <div className="bg-gray-50 p-3 rounded-full">{stat.icon}</div>
                        </div>
                        {/* Progress Bar */}
                        <div className="w-full h-1 bg-gray-200 rounded-full">
                            <div
                                className="h-1 rounded-full"
                                style={{ width: `${stat.progress}%`, backgroundColor: "#f59e0b" }}
                            ></div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Charts Section */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                <div className="bg-white p-4 rounded-lg shadow">
                    <h3 className="font-bold mb-4">User Activity (Last 7 Days)</h3>
                    <Chart
                        options={userChartOptions}
                        series={[{ name: "Users", data: monthlyUsers }]}
                        type="area"
                        height={250}
                    />
                </div>

                <div className="bg-white p-4 rounded-lg shadow">
                    <h3 className="font-bold mb-4">Shifts Activity (Last 7 Days)</h3>
                    <Chart
                        options={shiftChartOptions}
                        series={[{ name: "Shifts", data: monthlyShifts }]}
                        type="bar"
                        height={250}
                    />
                </div>
            </div>

            {/* Users List */}
            <div className="mt-8 w-full max-w-12xl">
                <Users />
            </div>
        </div>
    );
}
