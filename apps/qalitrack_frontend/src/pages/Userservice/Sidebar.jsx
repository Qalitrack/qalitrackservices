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
        `flex items-center gap-2 px-3 py-2 rounded transition-colors hover:bg-gray-100 ${
            isActive ? "bg-gray-200 font-medium border-l-4 border-amber-500" : ""
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
            label: "Roles & Permissions",
            icon: <Shield size={18} />,
            path: "/admin/roles",
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
        <aside className="w-64 bg-white border-r border-gray-200 flex flex-col h-full">
            <div className="px-4 py-4 font-bold text-2xl">Admin Panel</div>

            <nav className="flex-1 px-2 space-y-1 overflow-y-auto">
                {menuItems.map((item) => (
                    <NavLink
                        key={item.key}
                        to={item.path}
                        className={linkClasses}
                    >
                        {item.icon}
                        {item.label}
                    </NavLink>
                ))}
            </nav>

            <div className="p-3 text-sm text-gray-500 border-t">Version 1.0.0</div>
        </aside>
    );
}