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
  ChevronLeft,
  Factory,
  Truck,
  User,
  Tractor,
  Satellite,
  List,
  Users,
  User2,
  Shield,
  Lock,
  Database
} from "lucide-react";
import useAuth from "../api/helpers/auth";
import logo from "/src/assets/logorange.svg";

export default function UnifiedSidebar({ isCollapsed, onToggle }) {
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState({});
  const { getCurrentUser } = useAuth();
  
  const user = getCurrentUser();
  const userRoles = user?.userRoles || [];
  const isAdmin = userRoles.includes('Admin');
  const isOperator = userRoles.includes('Operator');

  // Determine base path based on primary role
  const basePath = isAdmin ? '/admin' : '/operator';

  useEffect(() => {
    if (location.pathname.includes('/weighing')) {
      setOpenMenus((prev) => ({ ...prev, weighing: true }));
    }
    if (location.pathname.includes('/security')) {
      setOpenMenus((prev) => ({ ...prev, security: true }));
    }
  }, [location.pathname]);

  const toggleMenu = (key) => {
    setOpenMenus((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  // Define all menu items with role restrictions
  const allMenuItems = [
    // Dashboard (everyone sees it)
    {
      key: "dashboard",
      label: "Dashboard",
      icon: <LayoutDashboard size={18} />,
      path: `${basePath}/dashboard`,
      roles: ['Admin', 'Operator']
    },

    // Weighing section (visible to both Admin and Operator)
    {
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      roles: ['Admin', 'Operator'],
      children: [
        { key: "weighing-factory", label: "Factory Weighing", icon: <Factory size={16} />, path: `${basePath}/weighing/factory`, roles: ['Admin', 'Operator'] },
        { key: "transactions", label: "Transactions", icon: <LayoutDashboard size={16} />, path: `${basePath}/transactions`, roles: ['Admin', 'Operator'] },
        { key: "weighing-vehicles", label: "Vehicles", icon: <Truck size={16} />, path: `${basePath}/weighing/vehicle`, roles: ['Admin', 'Operator'] },
        { key: "weighing-drivers", label: "Drivers", icon: <User size={16} />, path: `${basePath}/weighing/drivers`, roles: ['Admin', 'Operator'] },
        { key: "weighing-transporters", label: "Transporters", icon: <Tractor size={16} />, path: `${basePath}/transporters`, roles: ['Admin', 'Operator'] },
        { key: "weighing-owners", label: "Owners", icon: <Users size={16} />, path: `${basePath}/weighing/owners`, roles: ['Admin', 'Operator'] },
        { key: "suppliers", label: "Suppliers", icon: <Satellite size={16} />, path: `${basePath}/suppliers`, roles: ['Admin', 'Operator'] },
        { key: "weighing-products", label: "Products", icon: <BarChart3 size={16} />, path: `${basePath}/weighing/products`, roles: ['Admin', 'Operator'] },
        { key: "weighing-saccos", label: "Saccos", icon: <User2 size={16} />, path: `${basePath}/saccos`, roles: ['Admin', 'Operator'] },
        { key: "weighing-weighbridges", label: "Weighbridges", icon: <Scale size={16} />, path: `${basePath}/weighbridges`, roles: ['Admin', 'Operator'] },
        { key: "weighing-axle-config", label: "Axle Configuration", icon: <List size={16} />, path: `${basePath}/weighing/axle-config`, roles: ['Admin', 'Operator'] },
      ],
    },

    // Automation
    { 
      key: "automation", 
      label: "Automation", 
      icon: <Cog size={18} />, 
      path: `${basePath}/automation`,
      roles: ['Admin', 'Operator']
    },

    // Calibrations
    { 
      key: "calibrations", 
      label: "Calibrations", 
      icon: <Wrench size={18} />, 
      path: `${basePath}/calibrations`,
      roles: ['Admin', 'Operator']
    },

    // Analytics
    { 
      key: "analytics", 
      label: "Analytics", 
      icon: <BarChart3 size={18} />, 
      path: `${basePath}/analytics`,
      roles: ['Admin', 'Operator']
    },

    // Reports
    { 
      key: "reports", 
      label: "Reports", 
      icon: <FileText size={18} />, 
      path: `${basePath}/reports`,
      roles: ['Admin', 'Operator']
    },

    // Shifts (Admin only)
    {
      key: "shifts",
      label: "Shifts",
      icon: <User size={18} />,
      path: `${basePath}/shifts`,
      roles: ['Admin']
    },

    // Shift Assignment (Admin only)
    {
      key: "shift-assignment",
      label: "Shift Assignment",
      icon: <Users size={18} />,
      path: `${basePath}/shift-assignment`,
      roles: ['Admin']
    },

    // Attendance (Admin only)
    {
      key: "attendance",
      label: "Attendance",
      icon: <User2 size={18} />,
      path: `${basePath}/attendance`,
      roles: ['Admin']
    },

    // Security section (Admin only)
    {
      key: "security",
      label: "Security",
      icon: <Shield size={18} />,
      roles: ['Admin'],
      children: [
        { key: "permissions", label: "Permissions", icon: <Shield size={16} />, path: `${basePath}/security/permissions`, roles: ['Admin'] },
        { key: "roles", label: "Roles", icon: <Users size={16} />, path: `${basePath}/security/roles`, roles: ['Admin'] },
        { key: "password-policy", label: "Password Policy", icon: <Lock size={16} />, path: `${basePath}/security/password-policy`, roles: ['Admin'] },
      ],
    },

    // Backup Service (Admin only)
    {
      key: "backup",
      label: "Backup Service",
      icon: <Database size={18} />,
      path: `${basePath}/backup/microservice`,
      roles: ['Admin']
    },

    // System
    { 
      key: "system", 
      label: "System", 
      icon: <Cog size={18} />, 
      path: `${basePath}/system`,
      roles: ['Admin', 'Operator']
    },
  ];

  // Filter menu items based on user roles
  const filterMenuByRole = (items) => {
    return items.filter(item => {
      const hasAccess = item.roles?.some(role => userRoles.includes(role));
      if (!hasAccess) return false;

      if (item.children) {
        item.children = item.children.filter(child => 
          child.roles?.some(role => userRoles.includes(role))
        );
        return item.children.length > 0;
      }
      return true;
    });
  };

  const menuItems = filterMenuByRole(allMenuItems);

  return (
    <aside
      className={`
        fixed top-0 left-0 h-full z-40 flex flex-col transition-all duration-300
        bg-black text-white shadow-lg
        ${isCollapsed ? "w-16" : "w-64"}
      `}
      aria-label="Main sidebar"
    >
      {/* Branding + Collapse */}
      <div className={`flex items-center relative ${isCollapsed ? "justify-center px-0" : "justify-between px-6"} py-6`}>
        <div className="flex items-center gap-3">
          <img
            src={logo}
            alt="Qalitrack"
            className={`transition-all duration-300 ${isCollapsed ? "h-8" : "h-12"}`}
          />
        </div>

        <button
          onClick={onToggle}
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          className={`
            absolute top-1/2 -translate-y-1/2 p-1 rounded-full bg-white/5 hover:bg-white/10
            ${isCollapsed ? "right-1" : "-right-3"}
            border border-white/10
          `}
        >
          {isCollapsed ? <ChevronRight size={16} className="text-amber-400" /> : <ChevronLeft size={16} className="text-amber-400" />}
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto px-1 py-2">
        {menuItems.map((item) =>
          item.children ? (
            <div key={item.key} className="mb-1">
              <button
                onClick={() => toggleMenu(item.key)}
                className={`flex items-center justify-between w-full px-3 py-2 rounded-r-md transition-colors duration-150
                  ${openMenus[item.key] ? "bg-white/5 font-medium" : "hover:bg-white/5"}
                  ${isCollapsed ? "justify-center" : ""}`}
                aria-expanded={!!openMenus[item.key]}
                aria-controls={`${item.key}-sub`}
              >
                <span className="flex items-center gap-3">
                  <span className="text-amber-400">{item.icon}</span>
                  {!isCollapsed && <span className="text-sm">{item.label}</span>}
                </span>
                {!isCollapsed && (
                  <span className="text-white/80">
                    {openMenus[item.key] ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                  </span>
                )}
              </button>

              <div id={`${item.key}-sub`} className={`${openMenus[item.key] && !isCollapsed ? "block" : "hidden"} mt-1`}>
                {item.children.map((child) => (
                  <NavLink
                    key={child.key}
                    to={child.path}
                    className={({ isActive }) =>
                      `flex items-center gap-3 px-3 py-2 rounded-r-md transition-colors duration-150 relative
                       ${isActive ? "bg-white/6" : "hover:bg-white/4"}`
                    }
                  >
                    <ActiveLeftBorder />
                    <span className="text-white/90">{child.icon}</span>
                    {!isCollapsed && <span className="text-sm">{child.label}</span>}
                  </NavLink>
                ))}
              </div>
            </div>
          ) : (
            <div key={item.key} className="relative group mb-1">
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `
                  flex items-center gap-3 px-3 py-2 rounded-r-md transition-colors duration-150 relative
                  ${isActive ? "bg-white/6" : "hover:bg-white/4"}
                  `
                }
              >
                <ActiveLeftBorder />
                <span className="text-amber-400">{item.icon}</span>
                {!isCollapsed && <span className="text-sm">{item.label}</span>}
              </NavLink>

              {isCollapsed && (
                <div
                  className="absolute left-full top-1/2 -translate-y-1/2 ml-3 z-50 opacity-0 group-hover:opacity-100 pointer-events-none
                              bg-white text-black text-xs px-3 py-1 rounded-md shadow-md whitespace-nowrap transition-opacity duration-150"
                  role="tooltip"
                >
                  {item.label}
                </div>
              )}
            </div>
          )
        )}
      </nav>

      {/* Footer / Version */}
      <div className="px-3 py-3 border-t border-white/6 text-xs text-white/70">
        {!isCollapsed ? (
          <div className="flex items-center justify-between">
            <span>v0.1</span>
            <span className="text-amber-400 text-xs">QSL</span>
          </div>
        ) : (
          <div className="flex items-center justify-center">
            <span className="text-amber-400 text-xs">v0.1</span>
          </div>
        )}
      </div>
    </aside>
  );
}

function ActiveLeftBorder() {
  return <span className="absolute left-0 top-0 bottom-0 w-0.5 bg-transparent group-hover:bg-transparent" aria-hidden />;
}