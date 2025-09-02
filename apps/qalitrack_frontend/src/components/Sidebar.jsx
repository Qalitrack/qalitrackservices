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

// TODO: Replace with Redux/Context later
const currentUserRole = "admin"; // or "operator"

export default function Sidebar() {
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState({});

  // Auto-open submenu based on current route
  useEffect(() => {
    if (location.pathname.startsWith("/weighing")) {
      setOpenMenus((prev) => ({ ...prev, weighing: true }));
    }
  }, [location.pathname]);

  // Toggle submenu
  const toggleMenu = (key) => {
    setOpenMenus((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  // Link style helper
  const linkClasses = ({ isActive }) =>
    `flex items-center gap-2 px-3 py-2 rounded transition-colors hover:bg-gray-100 ${
      isActive ? "bg-gray-200 font-medium border-l-4 border-green-500" : ""
    }`;

  // Menu config with role restrictions
  const menuItems = [
    {
      key: "/",
      label: "Dashboard",
      icon: <LayoutDashboard size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      roles: ["admin", "operator"],
      children: [
        {
          key: "/weighing/factory",
          label: "Factory Weighing",
          icon: <Factory size={16} />,
          roles: ["admin", "operator"],
        },
        {
          key: "/weighing/vehicle",
          label: "Vehicles",
          icon: <Truck size={16} />,
          roles: ["admin", "operator"],
        },
        {
          key: "/weighing/drivers",
          label: "Drivers",
          icon: <User size={16} />,
          roles: ["admin"],
        },
      ],
    },
    {
      key: "/automation",
      label: "Automation",
      icon: <Cog size={18} />,
      roles: ["admin"],
    },
    {
      key: "/calibrations",
      label: "Calibrations",
      icon: <Wrench size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "/analytics",
      label: "Analytics",
      icon: <BarChart3 size={18} />,
      roles: ["admin"],
    },
    {
      key: "/reports",
      label: "Reports",
      icon: <FileText size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "/system",
      label: "System",
      icon: <Cog size={18} />,
      roles: ["admin"],
    },
  ];

  // Filter by role
  const filteredMenu = menuItems.filter((item) => {
    if (!item.roles.includes(currentUserRole)) return false;
    if (item.children) {
      item.children = item.children.filter((child) =>
        child.roles.includes(currentUserRole)
      );
      return item.children.length > 0;
    }
    return true;
  });

  return (
    <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
      <div className="px-4 py-4 font-bold text-2xl">Qalitrack</div>

      <nav className="flex-1 px-2 space-y-1">
        {filteredMenu.map((item) =>
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
                    <NavLink key={child.key} to={child.key} className={linkClasses}>
                      {child.icon}
                      {child.label}
                    </NavLink>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <NavLink key={item.key} to={item.key} className={linkClasses}>
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
