import { useState, useEffect } from "react";
import { NavLink, useLocation } from "react-router-dom";
import {
    LayoutDashboard,
    Scale,
    Cog,
    BarChart3,
    FileText,
    Wrench,
    ChevronDown,
    ChevronRight,
    Factory,
    Truck,
    User,
    Shield,
    Settings,
    Users,
    Lock
} from "lucide-react";
import useAuth from "../helpers/auth";

export default function UserServiceSidebar() {
    const location = useLocation();
    const [openMenus, setOpenMenus] = useState({});
    const { getCurrentUser } = useAuth();
    const user = getCurrentUser();
    const currentUserRole = user?.userRoles?.[0]?.toLowerCase() || "admin";

    useEffect(() => {
        // Auto-open submenu based on current route
        if (location.pathname.includes("/admin/users")) {
            setOpenMenus((prev) => ({ ...prev, userManagement: true }));
        }
        if (location.pathname.includes("/admin/security")) {
            setOpenMenus((prev) => ({ ...prev, security: true }));
        }
    }, [location.pathname]);

    const toggleMenu = (key) => {
        setOpenMenus((prev) => ({ ...prev, [key]: !prev[key] }));
    };

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
            key: "userManagement",
            label: "User Management",
            icon: <Users size={18} />,
            children: [
                {
                    key: "users-list",
                    label: "All Users",
                    icon: <User size={16} />,
                    path: "users",
                },
                {
                    key: "roles",
                    label: "Roles & Permissions",
                    icon: <Shield size={16} />,
                    path: "roles",
                },
            ],
        },
        {
            key: "security",
            label: "Security",
            icon: <Lock size={18} />,
            children: [
                {
                    key: "password-policy",
                    label: "Password Policy",
                    icon: <Shield size={16} />,
                    path: "security/password-policy",
                },
                {
                    key: "authentication",
                    label: "Authentication",
                    icon: <Lock size={16} />,
                    path: "security/authentication",
                },
            ],
        },
        {
            key: "weighing",
            label: "Weighing Management",
            icon: <Scale size={18} />,
            children: [
                {
                    key: "factory-settings",
                    label: "Factory Settings",
                    icon: <Factory size={16} />,
                    path: "weighing/factory",
                },
                {
                    key: "vehicles",
                    label: "Vehicles",
                    icon: <Truck size={16} />,
                    path: "weighing/vehicle",
                },
                {
                    key: "drivers",
                    label: "Drivers",
                    icon: <User size={16} />,
                    path: "weighing/drivers",
                },
            ],
        },
        {
            key: "system",
            label: "System Settings",
            icon: <Settings size={18} />,
            path: "system",
        },
    ];

    // Helper function to build the correct path
    const getPath = (item) => {
        const rootPath = "/admin";

        if (item.path.startsWith('/')) {
            return item.path;
        }

        return `${rootPath}/${item.path}`;
    };

    return (
        <aside className="w-64 bg-white border-r border-gray-200 flex flex-col h-full">
            <div className="px-4 py-4 font-bold text-2xl">Admin Panel</div>

            <nav className="flex-1 px-2 space-y-1 overflow-y-auto">
                {menuItems.map((item) =>
                        item.children ? (
                            <div key={item.key}>
                                <button
                                    onClick={() => toggleMenu(item.key)}
                                    className={`flex items-center justify-between w-full px-3 py-2 rounded hover:bg-gray-100 ${
                                        openMenus[item.key] ? "bg-gray-100 font-medium" : ""
                                    }`}
                                >
                <span className="flex items-center gap-2">
                  {item.icon}
                    {item.label}
                </span>
                                    {openMenus[item.key] ? (
                                        <ChevronDown size={16} />
                                    ) : (
                                        <ChevronRight size={16} />
                                    )}
                                </button>

                                {openMenus[item.key] && (
                                    <div className="ml-6 mt-1 space-y-1">
                                        {item.children.map((child) => (
                                            <NavLink
                                                key={child.key}
                                                to={getPath(child)}
                                                className={linkClasses}
                                            >
                                                {child.icon}
                                                {child.label}
                                            </NavLink>
                                        ))}
                                    </div>
                                )}
                            </div>
                        ) : (
                            <NavLink key={item.key} to={getPath(item)} className={linkClasses}>
                                {item.icon}
                                {item.label}
                            </NavLink>
                        )
                )}
            </nav>

            <div className="p-3 text-sm text-gray-500 border-t">Version 1.0.0</div>
        </aside>
    );
}