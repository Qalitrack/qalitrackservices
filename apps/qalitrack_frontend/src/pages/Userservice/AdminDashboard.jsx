import React from "react";
import { BarChart3, Users, Settings, Activity } from "lucide-react";

export default function AdminDashboard() {
    // Static greeting
    const greeting = "Welcome to the Admin Dashboard";

    // Stats for dashboard
    const stats = [
        { label: "Total Users", value: "124", icon: <Users size={20} className="text-blue-500" /> },
        { label: "Active Roles", value: "8", icon: <Settings size={20} className="text-green-500" /> },
        { label: "System Health", value: "98%", icon: <Activity size={20} className="text-amber-500" /> },
    ];

    return (
        <div className="p-6 max-w-7xl mx-auto">
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
                            <p className="text-2xl font-semibold mt-1">{stat.value}</p>
                        </div>
                        <div className="bg-gray-50 p-3 rounded-full">{stat.icon}</div>
                    </div>
                ))}
            </div>

            {/* Activity Overview */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-100 overflow-hidden">
                <div className="flex items-center justify-between p-5 border-b border-gray-100">
                    <h2 className="font-semibold text-lg">Overview</h2>
                    <div className="flex items-center text-sm text-amber-600 font-medium">
                        <BarChart3 size={16} className="mr-1" />
                        <span>Activity Chart</span>
                    </div>
                </div>

                <div className="p-5">
                    <p className="text-gray-500 mb-8">
                        This administration dashboard provides you with system-wide
                        management capabilities for users, roles, and application settings.
                    </p>

                    <div className="flex flex-col md:flex-row gap-4 items-center justify-center p-8 bg-gray-50 rounded-md">
                        <div className="text-center">
                            <h3 className="font-medium mb-2">Quick Links</h3>
                            <div className="flex gap-2">
                                <button className="px-4 py-2 bg-amber-500 text-white rounded hover:bg-amber-600 transition-colors">
                                    Manage Users
                                </button>
                                <button className="px-4 py-2 bg-gray-100 text-gray-700 rounded hover:bg-gray-200 transition-colors">
                                    System Settings
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}