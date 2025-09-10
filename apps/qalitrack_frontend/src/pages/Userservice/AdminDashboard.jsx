import React, { useState, useEffect } from "react";
import { BarChart3, Users as UsersIcon, Settings, Activity, Clock } from "lucide-react";
import { fetchUsers } from "../../helpers/UserService/Users/users.js";
import { fetchRoles } from "../../helpers/UserService/Roles/Roles.js";
import { fetchShifts, fetchDeletedShifts } from "../../helpers/UserService/Shifts/Shifts.js";
import Users from "./Users.jsx";

export default function AdminDashboard() {
    const [stats, setStats] = useState([
        { label: "Total Users", value: "...", icon: <UsersIcon size={20} className="text-blue-500" /> },
        { label: "Inactive Users", value: "...", icon: <UsersIcon size={20} className="text-gray-500" /> },
        { label: "Active Roles", value: "...", icon: <Settings size={20} className="text-green-500" /> },
        { label: "Active Shifts", value: "...", icon: <Clock size={20} className="text-purple-500" /> },
        { label: "Inactive Shifts", value: "...", icon: <Clock size={20} className="text-gray-400" /> },
        { label: "System Health", value: "98%", icon: <Activity size={20} className="text-amber-500" /> },
    ]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const abortController = new AbortController();
        const signal = abortController.signal;

        const fetchData = async () => {
            try {
                setLoading(true);
                const [usersData, rolesData, shiftsResponse] = await Promise.all([
                    fetchUsers(1, 1, signal), // We only need the total count
                    fetchRoles(signal),
                    fetchShifts(1, 100, signal) // Fetch more shifts to count active/inactive
                ]);

                const inactiveUsers = usersData.items?.filter(user => user.isDeleted)?.length || 0;

                // Count active and inactive shifts based on isActive flag
                const allShifts = shiftsResponse?.items || [];
                const activeShifts = allShifts.filter(shift => shift.isActive).length;
                const inactiveShifts = allShifts.filter(shift => !shift.isActive).length;

                setStats(prevStats => [
                    { ...prevStats[0], value: usersData.totalCount.toString() },
                    { ...prevStats[1], value: inactiveUsers.toString() },
                    { ...prevStats[2], value: rolesData.length.toString() },
                    { ...prevStats[3], value: activeShifts.toString() },
                    { ...prevStats[4], value: inactiveShifts.toString() },
                    prevStats[5]
                ]);

            } catch (error) {
                if (error.name !== 'CanceledError') {
                    console.error("Failed to fetch dashboard data:", error);
                    // Optionally set stats to an error state
                    setStats(prevStats => [
                        { ...prevStats[0], value: "N/A" },
                        { ...prevStats[1], value: "N/A" },
                        { ...prevStats[2], value: "N/A" },
                        { ...prevStats[3], value: "N/A" },
                        { ...prevStats[4], value: "N/A" },
                        prevStats[5]
                    ]);
                }
            } finally {
                setLoading(false);
            }
        };

        fetchData();

        return () => {
            abortController.abort();
        };
    }, []);

    // Static greeting
    const greeting = "Welcome to the Admin Dashboard";

    return (
        <div className="p-6 max-w-7xl mx-auto mr-60">
            {/* Welcome Section */}
            <div className="mb-8">
                <h1 className="text-2xl font-bold text-gray-800">{greeting}</h1>
                <p className="text-gray-500 mt-1">
                    Here's what's happening in your admin panel today
                </p>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                {stats.map((stat, index) => (
                    <div
                        key={index}
                        className="bg-white p-6 rounded-lg shadow-sm border border-gray-100 flex justify-between items-center"
                    >
                        <div>
                            <p className="text-sm text-gray-500">{stat.label}</p>
                            <p className="text-2xl font-semibold mt-1">{loading ? '...' : stat.value}</p>
                        </div>
                        <div className="bg-gray-50 p-3 rounded-full">{stat.icon}</div>
                    </div>
                ))}
            </div>

            {/* Users List */}
            <div className="mt-8 w-full max-w-11xl">
                <Users />
            </div>
        </div>
    );
}