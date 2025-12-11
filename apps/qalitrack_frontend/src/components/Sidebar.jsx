import { useState, useEffect } from "react";
import { NavLink, useLocation } from "react-router-dom";
import {
  // ... (import lucide-react icons)
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
  Tractor,
  Satellite,
  Menu,
  X
} from "lucide-react";

// 💡 1. Import the logo image file
import logo from "/src/assets/qualitrack.png"; // Adjust path if logo is elsewhere

const currentUserRole = "operator"; // or "admin"

export default function Sidebar({ isCollapsed }) {
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState({});

  useEffect(() => {
    const rootPath = `/${currentUserRole}`;
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
    // ... (Your menu items array remains the same)
    {
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      children: [
        { key: "weighing-factory", label: "Factory Weighing", icon: <Factory size={16} />, path: "weighing/factory" },
        { key: "weighing-vehicles", label: "Vehicles", icon: <Truck size={16} />, path: "weighing/vehicle" },
        { key: "weighing-drivers", label: "Drivers", icon: <User size={16} />, path: "weighing/drivers" },
        { key: "weighing-transporters", label: "Transporters", icon: <Tractor size={16} />, path: "transporters" },
        { key: "suppliers", label: "Suppliers", icon: <Satellite size={16} />, path: "suppliers" },
      ],
    },
    { key: "automation", label: "Automation", icon: <Cog size={18} />, path: "automation" },
    { key: "calibrations", label: "Calibrations", icon: <Wrench size={18} />, path: "calibrations" },
    { key: "analytics", label: "Analytics", icon: <BarChart3 size={18} />, path: "analytics" },
    { key: "reports", label: "Reports", icon: <FileText size={18} />, path: "reports" },
    { key: "system", label: "System", icon: <Cog size={18} />, path: "system" },
  ];

  const getPath = (item) => {
    const rootPath = `/${currentUserRole}`;
    if (item.path.startsWith("/")) {
      return item.path === "/dashboard" ? rootPath : item.path;
    }
    return `${rootPath}/${item.path}`;
  };

  return (
    <aside
      className={`h-screen bg-white border-r border-gray-200 flex flex-col transition-all duration-300 ${
        isCollapsed ? "w-16" : "w-64"
      }`}
    >
      {/* Branding - Replaced text with logo */}
      <div className={`px-8 py-8 flex items-center justify-center ${isCollapsed ? "justify-center" : ""}`}>
        {/* Logo when sidebar is expanded */}
        {!isCollapsed && (
          <img src={logo} alt="Qalitrack Logo" className="h-25 w-auto" />
        )}
        
        {/* Logo when sidebar is collapsed (using a smaller, centered version) */}
        {isCollapsed && (
            <img src={logo} alt="Qalitrack Logo" className="h-20 w-auto" />
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-1 space-y-1">
        {/* ... (Menu rendering logic remains the same) ... */}
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
                  {!isCollapsed && item.label}
                </span>
                {!isCollapsed && (openMenus[item.key] ? <ChevronDown size={16} /> : <ChevronRight size={16} />)}
              </button>

              {openMenus[item.key] && !isCollapsed && (
                <div className="ml-6 mt-1 space-y-1">
                  {item.children.map((child) => (
                    <NavLink key={child.key} to={getPath(child)} className={linkClasses}>
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
              {!isCollapsed && item.label}
            </NavLink>
          )
        )}
      </nav>

      <div className="p-3 text-sm text-gray-500 border-t">{!isCollapsed && "v0.1"}</div>
    </aside>
  );
}