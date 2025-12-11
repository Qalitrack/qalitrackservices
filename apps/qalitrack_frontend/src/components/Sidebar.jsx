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
  ChevronLeft, // 👈 New icon for the button
  Factory,
  Truck,
  User,
  Tractor,
  Satellite,
} from "lucide-react";

// 💡 1. Import the logo image file
import logo from "/src/assets/qualitrack.png"; 

const currentUserRole = "operator"; // or "admin"

// 🔄 Component now accepts onToggle prop
export default function Sidebar({ isCollapsed, onToggle }) {
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
    { key: "dashboard", label: "Dashboard", icon: <LayoutDashboard size={18} />, path: "/dashboard" },
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
      {/* Branding and Collapse Button Container */}
      <div 
        className={`py-8 flex items-center relative ${isCollapsed ? "justify-center px-0" : "px-8"}`}
      >
        {/* Logo */}
        <img 
          src={logo} 
          alt="Qualitrack Logo" 
          // Conditional classes for height
          className={`w-auto transition-all duration-300 ${isCollapsed ? "h-10" : "h-20"}`}
        />

        {/* 🚀 Collapse Button */}
        <button
          onClick={onToggle}
          aria-label={isCollapsed ? "Expand Sidebar" : "Collapse Sidebar"}
          className={`absolute top-1/2 -translate-y-1/2 p-1 rounded-full text-gray-500 bg-white shadow-md border 
            hover:bg-gray-100 transition-colors duration-300 
            ${isCollapsed ? "right-1" : "-right-3"} 
            ${isCollapsed ? "border-transparent" : "border-gray-200"}`}
        >
          {/* Change icon based on state */}
          {isCollapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-1 space-y-1 overflow-y-auto">
        {menuItems.map((item) =>
          item.children ? (
            <div key={item.key}>
              <button
                onClick={() => toggleMenu(item.key)}
                className={`flex items-center justify-between w-full px-3 py-2 rounded hover:bg-gray-100 ${
                  openMenus[item.key] ? "bg-gray-200 font-medium" : ""
                } ${isCollapsed ? "justify-center" : ""}`}
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
             // Single Link with Tooltip logic (from previous suggestion)
            <div key={item.key} className="relative group">
                <NavLink to={getPath(item)} className={linkClasses}>
                    {item.icon}
                    {!isCollapsed && item.label}
                </NavLink>

                {/* Tooltip for collapsed state */}
                {isCollapsed && (
                    <span className="absolute left-full ml-4 top-1/2 -translate-y-1/2 z-10 
                                     opacity-0 group-hover:opacity-100 pointer-events-none
                                     bg-gray-800 text-white text-xs p-2 rounded-md whitespace-nowrap 
                                     transition-opacity duration-200">
                        {item.label}
                    </span>
                )}
            </div>
          )
        )}
      </nav>

      <div className="p-3 text-sm text-gray-500 border-t flex justify-center">{!isCollapsed && "v0.1"}</div>
    </aside>
  );
}

// NOTE: You must update the parent component (e.g., your main Layout component) 
// to manage the 'isCollapsed' state and pass a 'onToggle' function.