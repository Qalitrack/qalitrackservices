import { Outlet, useLocation } from "react-router-dom";
import Sidebar from "../pages/Userservice/Sidebar.jsx";
import Topbar from "../components/Topbar";
import AdminDashboard from "../pages/Userservice/AdminDashboard.jsx";

export default function UserServiceLayout() {
    const location = useLocation();
    const isRootPath = location.pathname === "/admin" || location.pathname === "/admin/";

    return (
        <div className="flex h-screen bg-gray-50 overflow-hidden">
            {/* Sidebar with fixed width */}
            <div className="flex-shrink-0 w-64 bg-white border-r border-gray-200">
                <Sidebar />
            </div>

            {/* Main content area */}
            <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
                <Topbar />

                <main className="flex-1 p-4 overflow-y-auto">
                    {isRootPath ? <AdminDashboard /> : <Outlet />}
                </main>
            </div>
        </div>
    );
}
