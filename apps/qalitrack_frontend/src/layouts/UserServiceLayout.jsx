import { Outlet } from "react-router-dom";
import Sidebar from "../pages/Userservice/Sidebar.jsx";
import Topbar from "../components/Topbar";

export default function UserServiceLayout() {
    return (
        <div className="flex h-screen bg-gray-50">
            <Sidebar />
            <div className="flex flex-col flex-1">
                <Topbar />
                <main className="flex-1 p-4 overflow-y-auto">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
