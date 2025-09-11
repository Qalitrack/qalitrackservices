import { Outlet, useLocation } from "react-router-dom";
import Sidebar from "../pages/Userservice/Sidebar.jsx";
import Topbar from "../components/Topbar";
import AdminDashboard from "../pages/Userservice/AdminDashboard.jsx";
import { useState } from "react";

export default function UserServiceLayout() {
    const location = useLocation();
    const isRootPath =
        location.pathname === "/admin" || location.pathname === "/admin/";
    const [sidebarOpen, setSidebarOpen] = useState(false);

    return (
        <div className="flex h-screen bg-gray-50 overflow-hidden">
            {/* Sidebar */}
            <div
                className={`fixed inset-y-0 left-0 transform bg-white border-r border-gray-200 w-64 z-30 transition-transform duration-300 ease-in-out
                ${sidebarOpen ? "translate-x-0" : "-translate-x-full"} md:translate-x-0 md:static md:flex-shrink-0`}
            >
                <Sidebar />
            </div>

            {/* Overlay for mobile */}
            {sidebarOpen && (
                <div
                    className="fixed inset-0 bg-black bg-opacity-30 z-20 md:hidden"
                    onClick={() => setSidebarOpen(false)}
                ></div>
            )}

            {/* Main content area */}
            <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
                <Topbar onToggleSidebar={() => setSidebarOpen(!sidebarOpen)} />

                <main className="flex-1 p-4 overflow-y-auto">
                    {isRootPath ? <AdminDashboard /> : <Outlet />}
                </main>
            </div>
        </div>
    );
}
