/**
 * UnifiedSidebar.jsx — Updated to consume SidebarSettingsContext
 *
 * Changes from original:
 *  - Logo is now pulled from sidebarSettings.companyLogo (falls back to static SVG)
 *  - All colors (bg, text, accent, hover, active, border) come from resolvedTheme
 *  - No API calls — all state is in-memory via Context
 */

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
  Database,
} from "lucide-react";
import useAuth from "../api/helpers/auth";
import staticLogo from "/src/assets/logorange.svg";
import { useSidebarSettings } from "../components/Context/Sidebarsettingscontext";

export default function UnifiedSidebar({ isCollapsed, onToggle }) {
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState({});
  const { getCurrentUser } = useAuth();

  // ── Live sidebar settings from Context ─────────────────────────────────────
  const { sidebarSettings, resolvedTheme } = useSidebarSettings();
  const logoSrc = sidebarSettings.companyLogo || staticLogo;

  const user = getCurrentUser();
  const userRoles = user?.userRoles || [];
  const isAdmin = userRoles.includes("Admin");

  const basePath = isAdmin ? "/admin" : "/operator";

  useEffect(() => {
    if (location.pathname.includes("/weighing")) {
      setOpenMenus((prev) => ({ ...prev, weighing: true }));
    }
    if (location.pathname.includes("/security")) {
      setOpenMenus((prev) => ({ ...prev, security: true }));
    }
  }, [location.pathname]);

  const toggleMenu = (key) => {
    setOpenMenus((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  // ── Menu items (unchanged from original) ──────────────────────────────────
  const allMenuItems = [
    // Dashboard — all roles except Operator
    { key: "dashboard", label: "Dashboard", icon: <LayoutDashboard size={18} />, path: `${basePath}/dashboard`, roles: null, excludeRoles: ["Operator"] },
    {
      // Weighing group — visible to all authenticated users
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      roles: null,
      children: [
        { key: "weighing-factory",     label: "Factory Weighing",  icon: <Factory size={16} />,        path: `${basePath}/weighing/factory`,    roles: null },
        { key: "transactions",         label: "Transactions",      icon: <LayoutDashboard size={16} />, path: `${basePath}/transactions`,        roles: null },
        { key: "weighing-vehicles",    label: "Vehicles",          icon: <Truck size={16} />,           path: `${basePath}/weighing/vehicle`,    roles: null },
        { key: "weighing-drivers",     label: "Drivers",           icon: <User size={16} />,            path: `${basePath}/weighing/drivers`,    roles: null },
        { key: "weighing-transporters",label: "Transporters",      icon: <Tractor size={16} />,         path: `${basePath}/transporters`,        roles: null },
        { key: "weighing-owners",      label: "Owners",            icon: <Users size={16} />,           path: `${basePath}/weighing/owners`,     roles: null },
        { key: "suppliers",            label: "Suppliers",         icon: <Satellite size={16} />,       path: `${basePath}/suppliers`,           roles: null },
        { key: "weighing-products",    label: "Products",          icon: <BarChart3 size={16} />,       path: `${basePath}/weighing/products`,   roles: null },
        { key: "weighing-weighbridges",label: "Weighbridges",      icon: <Scale size={16} />,           path: `${basePath}/weighbridges`,        roles: ["Admin"] },
        { key: "weighing-axle-config", label: "Axle Configuration",icon: <List size={16} />,           path: `${basePath}/weighing/axle-config`,roles: ["Admin"] },
        { key: "weighing-saccos",      label: "Saccos",            icon: <User2 size={16} />,           path: `${basePath}/saccos`,              roles: null },
      ],
    },
    { key: "user-management",  label: "User Management",  icon: <Users size={18} />,    path: `${basePath}/user-management`,   roles: ["Admin"] },
    // Analytics, Reports — visible to all authenticated users; System — all except Operator
    { key: "analytics",        label: "Analytics",         icon: <BarChart3 size={18} />,path: `${basePath}/analytics`,        roles: null },
    { key: "reports",          label: "Reports",           icon: <FileText size={18} />, path: `${basePath}/reports`,          roles: null },
    { key: "shifts",           label: "Shifts",            icon: <User size={18} />,     path: `${basePath}/shifts`,           roles: ["Admin"] },
    { key: "shift-assignment", label: "Shift Assignment",  icon: <Users size={18} />,    path: `${basePath}/shift-assignment`, roles: ["Admin"] },
    {
      key: "security",
      label: "Security",
      icon: <Shield size={18} />,
      roles: ["Admin"],
      children: [
        { key: "permissions",     label: "Permissions",    icon: <Shield size={16} />, path: `${basePath}/security/permissions`,    roles: ["Admin"] },
        { key: "roles",           label: "Roles",          icon: <Users size={16} />,  path: `${basePath}/security/roles`,          roles: ["Admin"] },
        { key: "password-policy", label: "Password Policy",icon: <Lock size={16} />,   path: `${basePath}/security/password-policy`,roles: ["Admin"] },
      ],
    },
    { key: "system",      label: "System",         icon: <Cog size={18} />,  path: `${basePath}/system`,     roles: null, excludeRoles: ["Operator"] },
    // { key: "automation",  label: "Automation",     icon: <Cog size={18} />,  path: `${basePath}/automation`, roles: ["Admin"] },
  ];

  const filterMenuByRole = (items) => {
    return items.filter((item) => {
      // excludeRoles: hide from specific roles even if they'd otherwise have access
      if (item.excludeRoles?.some((role) => userRoles.includes(role))) return false;
      // null/undefined roles = visible to all authenticated users
      const hasAccess = !item.roles || item.roles.some((role) => userRoles.includes(role));
      if (!hasAccess) return false;
      if (item.children) {
        item.children = item.children.filter((child) => {
          if (child.excludeRoles?.some((role) => userRoles.includes(role))) return false;
          return !child.roles || child.roles.some((role) => userRoles.includes(role));
        });
        return item.children.length > 0;
      }
      return true;
    });
  };

  const menuItems = filterMenuByRole(allMenuItems);

  // ── Inline style helpers using resolvedTheme ───────────────────────────────
  const sidebarStyle = {
    backgroundColor: resolvedTheme.bg,
    color: resolvedTheme.text,
    transition: "background-color 0.3s ease, color 0.3s ease",
  };

  const accentStyle  = { color: resolvedTheme.accent };
  const borderStyle  = { borderColor: resolvedTheme.border };
  const chevronStyle = { color: resolvedTheme.accent };

  return (
    <aside
      style={sidebarStyle}
      className={`fixed top-0 left-0 h-full z-40 flex flex-col shadow-lg transition-all duration-300 ${
        isCollapsed ? "w-16" : "w-64"
      }`}
      aria-label="Main sidebar"
    >
      {/* ── Branding + Collapse ─────────────────────────────────────────────── */}
      <div
        className={`flex items-center relative py-6 ${
          isCollapsed ? "justify-center px-0" : "justify-between px-6"
        }`}
      >
        <div className="flex items-center gap-3">
          <img
            src={logoSrc}
            alt="Logo"
            className={`transition-all duration-300 object-contain ${
              isCollapsed ? "h-8 w-8" : "h-12"
            }`}
            style={{
              // If it's a base64 upload, give it a white bg pill so it reads on dark sidebar
              borderRadius: sidebarSettings.companyLogo ? "6px" : undefined,
              background: sidebarSettings.companyLogo ? "rgba(255,255,255,0.9)" : undefined,
              padding: sidebarSettings.companyLogo ? "2px" : undefined,
            }}
          />
          {!isCollapsed && sidebarSettings.companyLogo && (
            <span
              className="text-[11px] font-semibold leading-tight max-w-[120px] truncate"
              style={{ color: resolvedTheme.text, opacity: 0.85 }}
              title={sidebarSettings.companyName}
            >
              {sidebarSettings.companyName}
            </span>
          )}
        </div>

        <button
          onClick={onToggle}
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          style={{ borderColor: resolvedTheme.border, background: "rgba(255,255,255,0.04)" }}
          className={`absolute top-1/2 -translate-y-1/2 p-1 rounded-full hover:bg-white/10 border ${
            isCollapsed ? "right-1" : "-right-3"
          }`}
        >
          {isCollapsed ? (
            <ChevronRight size={16} style={chevronStyle} />
          ) : (
            <ChevronLeft size={16} style={chevronStyle} />
          )}
        </button>
      </div>

      {/* ── Navigation ──────────────────────────────────────────────────────── */}
      <nav className="flex-1 overflow-y-auto px-1 py-2">
        {menuItems.map((item) =>
          item.children ? (
            <div key={item.key} className="mb-1">
              <button
                onClick={() => toggleMenu(item.key)}
                style={openMenus[item.key] ? { background: resolvedTheme.hoverBg } : {}}
                className={`flex items-center justify-between w-full px-3 py-2 rounded-r-md transition-colors duration-150 ${
                  isCollapsed ? "justify-center" : ""
                }`}
                onMouseEnter={(e) => (e.currentTarget.style.background = resolvedTheme.hoverBg)}
                onMouseLeave={(e) =>
                  (e.currentTarget.style.background = openMenus[item.key]
                    ? resolvedTheme.hoverBg
                    : "transparent")
                }
                aria-expanded={!!openMenus[item.key]}
              >
                <span className="flex items-center gap-3">
                  <span style={accentStyle}>{item.icon}</span>
                  {!isCollapsed && (
                    <span className="text-sm" style={{ color: resolvedTheme.text }}>
                      {item.label}
                    </span>
                  )}
                </span>
                {!isCollapsed && (
                  <span style={{ color: resolvedTheme.text, opacity: 0.6 }}>
                    {openMenus[item.key] ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                  </span>
                )}
              </button>

              <div className={`${openMenus[item.key] && !isCollapsed ? "block" : "hidden"} mt-1`}>
                {item.children.map((child) => (
                  <NavLink
                    key={child.key}
                    to={child.path}
                    className="flex items-center gap-3 px-3 py-2 rounded-r-md transition-colors duration-150 relative group"
                    style={({ isActive }) => ({
                      background: isActive ? resolvedTheme.activeBg : "transparent",
                      borderLeft: isActive
                        ? `2px solid ${resolvedTheme.accent}`
                        : "2px solid transparent",
                      paddingLeft: "10px",
                    })}
                    onMouseEnter={(e) => (e.currentTarget.style.background = resolvedTheme.hoverBg)}
                    onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
                  >
                    <span style={{ color: resolvedTheme.text, opacity: 0.8 }}>{child.icon}</span>
                    {!isCollapsed && (
                      <span className="text-sm" style={{ color: resolvedTheme.text, opacity: 0.85 }}>
                        {child.label}
                      </span>
                    )}
                  </NavLink>
                ))}
              </div>
            </div>
          ) : (
            <div key={item.key} className="relative group mb-1">
              <NavLink
                to={item.path}
                className="flex items-center gap-3 px-3 py-2 rounded-r-md transition-colors duration-150 relative"
                style={({ isActive }) => ({
                  background: isActive ? resolvedTheme.activeBg : "transparent",
                  borderLeft: isActive
                    ? `2px solid ${resolvedTheme.accent}`
                    : "2px solid transparent",
                  paddingLeft: "10px",
                })}
                onMouseEnter={(e) => (e.currentTarget.style.background = resolvedTheme.hoverBg)}
                onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
              >
                <span style={accentStyle}>{item.icon}</span>
                {!isCollapsed && (
                  <span className="text-sm" style={{ color: resolvedTheme.text }}>
                    {item.label}
                  </span>
                )}
              </NavLink>

              {/* Collapsed tooltip */}
              {isCollapsed && (
                <div
                  className="absolute left-full top-1/2 -translate-y-1/2 ml-3 z-50 opacity-0 group-hover:opacity-100 pointer-events-none
                              bg-white text-black text-xs px-3 py-1 rounded-md shadow-md whitespace-nowrap transition-opacity duration-150"
                >
                  {item.label}
                </div>
              )}
            </div>
          )
        )}
      </nav>

      {/* ── Footer ──────────────────────────────────────────────────────────── */}
      <div
        className="px-3 py-3 text-xs"
        style={{ borderTop: `1px solid ${resolvedTheme.border}`, color: resolvedTheme.text, opacity: 0.6 }}
      >
        {!isCollapsed ? (
          <div className="flex items-center justify-between">
            <span>v0.1</span>
            <span style={accentStyle} className="font-semibold">QSL</span>
          </div>
        ) : (
          <div className="flex justify-center">
            <span style={accentStyle}>v0.1</span>
          </div>
        )}
      </div>
    </aside>
  );
}