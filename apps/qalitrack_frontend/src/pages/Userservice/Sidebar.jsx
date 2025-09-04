import { NavLink } from "react-router-dom";
import {
    LayoutDashboard,
    Scale,
    BarChart3,
    FileText,
    Settings,
    Users,
    Lock,
    Factory,
    Truck,
    User,
    Shield,
    Wrench,
} from "lucide-react";

export default function UserServiceSidebar() {
    const linkClasses = ({ isActive }) =>
        `flex items-center gap-3 px-3 py-2.5 rounded-md transition-colors duration-200 ${
            isActive
                ? "bg-amber-100 text-amber-700 font-semibold border-l-4 border-amber-500"
                : "text-gray-600 hover:bg-gray-100"
        }`;

    const menuItems = [
        {
            key: "admin-dashboard",
            label: "Dashboard",
            icon: <LayoutDashboard size={18} />,
            path: "/admin/dashboard",
        },
        {
            key: "users-list",
            label: "All Users",
            icon: <User size={18} />,
            path: "/admin/users",
        },
        {
            key: "roles",
            label: "Permissions",
            icon: <Shield size={18} />,
            path: "/admin/security/permissions",
        },
        {
            key: "password-policy",
            label: "Password Policy",
            icon: <Lock size={18} />,
            path: "/admin/security/password-policy",
        },
        {
            key: "authentication",
            label: "Authentication",
            icon: <Shield size={18} />,
            path: "/admin/security/authentication",
        },
    ];

    return (
        <aside className="w-20 md:w-64 bg-white border-r border-gray-200 flex flex-col h-full transition-all duration-300">
            <div className="px-4 py-4 font-bold text-xl md:text-2xl text-center">
                <span className="hidden md:inline">Admin Panel</span>
                <span className="md:hidden">AP</span>
            </div>

            <nav className="flex-1 px-2 space-y-2 overflow-y-auto">
                {menuItems.map((item) => (
                    <NavLink
                        key={item.key}
                        to={item.path}
                        className={linkClasses}
                        title={item.label} // Add title for hover tooltip on small screens
                    >
                        {item.icon}
                        <span className="hidden md:inline">{item.label}</span>
                    </NavLink>
                ))}
            </nav>

            <div className="p-3 text-xs text-gray-500 border-t text-center hidden md:block">
                Version 1.0.0
            </div>
        </aside>
    );
}