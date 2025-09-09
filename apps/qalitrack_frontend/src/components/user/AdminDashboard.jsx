import { Outlet, NavLink } from "react-router-dom";
import {
  LayoutDashboard,
  Users,
  Cog,
  FileText,
  BarChart3,
  LogOut,
} from "lucide-react";
import { useAuth } from "../../helpers/auth.js";

export default function AdminDashboard() {
  const { logout, user } = useAuth();

  const linkClasses = ({ isActive }) =>
    `flex items-center gap-2 px-3 py-2 rounded transition-colors hover:bg-gray-100 ${
      isActive ? "bg-gray-200 font-medium border-l-4 border-green-500" : ""
    }`;

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="px-4 py-4 font-bold text-2xl">Qalitrack Admin</div>

        <nav className="flex-1 px-2 space-y-1">
          <NavLink to="/admin/dashboard" className={linkClasses} end>
            <LayoutDashboard size={18} />
            Overview
          </NavLink>
          <NavLink to="/admin/users" className={linkClasses}>
            <Users size={18} />
            Manage Users
          </NavLink>
          <NavLink to="/admin/reports" className={linkClasses}>
            <FileText size={18} />
            Reports
          </NavLink>
          <NavLink to="/admin/analytics" className={linkClasses}>
            <BarChart3 size={18} />
            Analytics
          </NavLink>
          <NavLink to="/admin/system" className={linkClasses}>
            <Cog size={18} />
            System Settings
          </NavLink>
        </nav>

        <button
          onClick={logout}
          className="flex items-center gap-2 p-3 text-sm text-gray-600 border-t hover:bg-gray-100"
        >
          <LogOut size={18} />
          Logout
        </button>
      </aside>

      {/* Main Area */}
      <div className="flex flex-col flex-1">
        {/* Topbar */}
        <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-4">
          <h1 className="font-medium text-gray-700 text-lg">
            Welcome, <span className="capitalize">{user?.role}</span>
          </h1>
        </header>

        {/* Content */}
        <main className="flex-1 p-4 overflow-y-auto">
          <Outlet />
          {/* Mock Widgets */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
            <div className="p-4 bg-white rounded-lg shadow">📊 Sales Stats</div>
            <div className="p-4 bg-white rounded-lg shadow">👥 User Activity</div>
            <div className="p-4 bg-white rounded-lg shadow">⚙️ System Health</div>
          </div>
        </main>
      </div>
    </div>
  );
}
