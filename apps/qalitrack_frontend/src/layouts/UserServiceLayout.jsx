import { Outlet, useLocation } from "react-router-dom";
import Sidebar from "../pages/Userservice/Sidebar.jsx";
import Topbar from "../components/Topbar";
import AdminDashboard from "../pages/Userservice/AdminDashboard.jsx"; // Import your dashboard component

export default function UserServiceLayout() {
    const location = useLocation();
    const isRootPath = location.pathname === "/admin" || location.pathname === "/admin/";

    return (
        <div className="flex h-screen bg-gray-50">
            <Sidebar />
            <div className="flex flex-col flex-1">
                <Topbar />
                <main className="flex-1 p-4 overflow-y-auto">
                    {isRootPath ? <AdminDashboard /> : <Outlet />}
                </main>
            </div>
        </div>
    );
}