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
} from "lucide-react";

const currentUserRole = "operator"; // or "admin"

export default function Sidebar() {
    const location = useLocation();
    const [openMenus, setOpenMenus] = useState({});

    useEffect(() => {
        // Determine the root path for the current user's role
        const rootPath = `/${currentUserRole}`;

        // Auto-open submenu based on current route and role
        if (location.pathname.startsWith(`${rootPath}/weighing`)) {
            setOpenMenus((prev) => ({ ...prev, weighing: true }));
        }
    }, [location.pathname]);

    const toggleMenu = (key) => {
        setOpenMenus((prev) => ({ ...prev, [key]: !prev[key] }));
    };

    const linkClasses = ({ isActive }) =>
        `flex items-center gap-2 px-3 py-2 rounded transition-colors hover:bg-gray-100 ${
            isActive ? "bg-gray-200 font-medium border-l-4 border-green-500" : ""
        }`;

    const menuItems = [
        {
            key: "dashboard",
            label: "Dashboard",
            icon: <LayoutDashboard size={18} />,
            path: "/dashboard", // Will be resolved to /{role}/dashboard
        },
        {
            key: "weighing",
            label: "Weighing",
            icon: <Scale size={18} />,
            children: [
                {
                    key: "weighing-factory",
                    label: "Factory Weighing",
                    icon: <Factory size={16} />,
                    path: "weighing/factory",
                },
                {
                    key: "weighing-vehicles",
                    label: "Vehicles",
                    icon: <Truck size={16} />,
                    path: "weighing/vehicle",
                },
                {
                    key: "weighing-drivers",
                    label: "Drivers",
                    icon: <User size={16} />,
                    path: "weighing/drivers",
                },
            ],
        },
        {
            key: "automation",
            label: "Automation",
            icon: <Cog size={18} />,
            path: "automation",
        },
        {
            key: "calibrations",
            label: "Calibrations",
            icon: <Wrench size={18} />,
            path: "calibrations",
        },
        {
            key: "analytics",
            label: "Analytics",
            icon: <BarChart3 size={18} />,
            path: "analytics",
        },
        {
            key: "reports",
            label: "Reports",
            icon: <FileText size={18} />,
            path: "reports",
        },
        {
            key: "system",
            label: "System",
            icon: <Cog size={18} />,
            path: "system",
        },
    ];

    // Helper function to build the correct relative or absolute path
    const getPath = (item) => {
        const rootPath = `/${currentUserRole}`;

        // Check if the item has an absolute path specified
        if (item.path.startsWith('/')) {
            // For absolute paths like "/dashboard", convert to role-based path
            if (item.path === '/dashboard') {
                return rootPath; // Root dashboard for the role
            }
            return item.path;
        }

        // For relative paths, prepend the role-based root path
        return `${rootPath}/${item.path}`;
    };

    return (
        <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
            <div className="px-4 py-4 font-bold text-2xl">Qalitrack</div>

            <nav className="flex-1 px-2 space-y-1">
                {menuItems.map((item) =>
                        item.children ? (
                            <div key={item.key}>
                                <button
                                    onClick={() => toggleMenu(item.key)}
                                    className={`flex items-center justify-between w-full px-3 py-2 rounded hover:bg-gray-100 ${
                                        openMenus[item.key] ? "bg-gray-200 font-medium" : ""
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

            <div className="p-3 text-sm text-gray-500 border-t">v0.1</div>
        </aside>
    );
}