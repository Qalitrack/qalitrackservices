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

const currentUserRole = "admin"; // or "operator"

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
      key: "admin-dashboard",
      label: "Admin Dashboard",
      icon: <LayoutDashboard size={18} />,
      roles: ["admin"],
      path: "/admin",
    },
    {
      key: "operator-dashboard",
      label: "Dashboard",
      icon: <LayoutDashboard size={18} />,
      roles: ["operator"],
      path: "/operator",
    },
    {
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      roles: ["admin", "operator"],
      children: [
        {
          key: "weighing-factory",
          label: "Factory Weighing",
          icon: <Factory size={16} />,
          roles: ["admin", "operator"],
          path: "weighing/factory",
        },
        {
          key: "weighing-vehicles",
          label: "Vehicles",
          icon: <Truck size={16} />,
          roles: ["admin", "operator"],
          path: "weighing/vehicle",
        },
        {
          key: "weighing-drivers",
          label: "Drivers",
          icon: <User size={16} />,
          roles: ["admin"],
          path: "weighing/drivers",
        },
      ],
    },
    {
      key: "automation",
      label: "Automation",
      icon: <Cog size={18} />,
      roles: ["admin"],
      path: "automation",
    },
    {
      key: "calibrations",
      label: "Calibrations",
      icon: <Wrench size={18} />,
      roles: ["admin", "operator"],
      path: "calibrations",
    },
    {
      key: "analytics",
      label: "Analytics",
      icon: <BarChart3 size={18} />,
      roles: ["admin"],
      path: "analytics",
    },
    {
      key: "reports",
      label: "Reports",
      icon: <FileText size={18} />,
      roles: ["admin", "operator"],
      path: "reports",
    },
    {
      key: "system",
      label: "System",
      icon: <Cog size={18} />,
      roles: ["admin"],
      path: "system",
    },
  ];

  // Helper function to build the correct relative or absolute path
  const getPath = (item) => {
    const rootPath = `/${currentUserRole}`;
    
    // Check if the item has an absolute path specified
    if (item.path.startsWith('/')) {
        return item.path;
    }
    
    // For nested routes, make sure the path is relative
    return `${rootPath}/${item.path}`;
  };

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