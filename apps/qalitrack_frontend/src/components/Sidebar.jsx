/**
 * UnifiedSidebar.jsx — Updated to consume SidebarSettingsContext
 *
 * Changes from original:
 *  - Logo is now pulled from sidebarSettings.companyLogo (falls back to static SVG)
 *  - All colors (bg, text, accent, hover, active, border) come from resolvedTheme
 *  - No API calls — all state is in-memory via Context
 */

import { useState, useEffect, useRef } from "react";
import { createPortal } from "react-dom";
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
  Database,
} from "lucide-react";
import useAuth from "../api/helpers/auth";
import qalitrackLogoFull from "/src/assets/qalitrack_logo_full.png";
import { useSidebarSettings } from "../components/Context/Sidebarsettingscontext";

// A single nav row: icon + label when expanded, icon + hover tooltip when
// collapsed. Shared by top-level items and by a group's children once
// flattened in collapsed mode, so both look and behave identically.
//
// The tooltip is rendered via a portal to document.body, positioned from the
// icon's actual on-screen rect. It used to be a plain `absolute left-full`
// div, but the nav list needs `overflow-x: hidden` to stop a scrollbar from
// the browser's overflow-x/overflow-y pairing rule (setting overflow-y:auto
// forces overflow-x to also become non-visible) — that same rule clips any
// horizontally-overflowing content, tooltip included. A portal escapes that
// clipping entirely instead of fighting it.
function NavIconItem({ to, icon, label, isCollapsed, resolvedTheme, accentStyle }) {
  const anchorRef = useRef(null);
  const [tooltipPos, setTooltipPos] = useState(null);

  const showTooltip = () => {
    if (!isCollapsed || !anchorRef.current) return;
    const rect = anchorRef.current.getBoundingClientRect();
    setTooltipPos({ top: rect.top + rect.height / 2, left: rect.right + 12 });
  };
  const hideTooltip = () => setTooltipPos(null);

  return (
    <div
      ref={anchorRef}
      className="relative mb-1"
      onMouseEnter={showTooltip}
      onMouseLeave={hideTooltip}
    >
      <NavLink
        to={to}
        className={`flex items-center gap-3 py-2 rounded-r-md transition-colors duration-150 relative w-full ${
          isCollapsed ? "justify-center" : "px-3"
        }`}
        style={({ isActive }) => ({
          background: isActive ? resolvedTheme.activeBg : "transparent",
          borderLeft: isActive ? `2px solid ${resolvedTheme.accent}` : "2px solid transparent",
          paddingLeft: isCollapsed ? 0 : "10px",
        })}
        onMouseEnter={(e) => (e.currentTarget.style.background = resolvedTheme.hoverBg)}
        onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
      >
        <span style={accentStyle}>{icon}</span>
        {!isCollapsed && (
          <span className="text-sm" style={{ color: resolvedTheme.text }}>
            {label}
          </span>
        )}
      </NavLink>

      {isCollapsed && tooltipPos &&
        createPortal(
          <div
            style={{ position: "fixed", top: tooltipPos.top, left: tooltipPos.left, transform: "translateY(-50%)" }}
            className="z-[100] pointer-events-none bg-white text-black text-xs px-3 py-1 rounded-md shadow-md whitespace-nowrap"
          >
            {label}
          </div>,
          document.body
        )}
    </div>
  );
}

// Same row styling and portal-tooltip approach as NavIconItem, but a plain
// button (not a route) since it toggles collapse state instead of navigating.
function ToggleNavItem({ isCollapsed, onToggle, resolvedTheme, accentStyle }) {
  const anchorRef = useRef(null);
  const [tooltipPos, setTooltipPos] = useState(null);

  const showTooltip = () => {
    if (!isCollapsed || !anchorRef.current) return;
    const rect = anchorRef.current.getBoundingClientRect();
    setTooltipPos({ top: rect.top + rect.height / 2, left: rect.right + 12 });
  };
  const hideTooltip = () => setTooltipPos(null);

  return (
    <div ref={anchorRef} className="relative mb-1" onMouseEnter={showTooltip} onMouseLeave={hideTooltip}>
      <button
        onClick={onToggle}
        aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        className={`flex items-center gap-3 w-full px-3 py-2 rounded-r-md transition-colors duration-150 ${
          isCollapsed ? "justify-center" : ""
        }`}
        onMouseEnter={(e) => (e.currentTarget.style.background = resolvedTheme.hoverBg)}
        onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
      >
        <span style={accentStyle}>
          {isCollapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
        </span>
        {!isCollapsed && (
          <span className="text-sm" style={{ color: resolvedTheme.text }}>
            Collapse
          </span>
        )}
      </button>

      {isCollapsed && tooltipPos &&
        createPortal(
          <div
            style={{ position: "fixed", top: tooltipPos.top, left: tooltipPos.left, transform: "translateY(-50%)" }}
            className="z-[100] pointer-events-none bg-white text-black text-xs px-3 py-1 rounded-md shadow-md whitespace-nowrap"
          >
            Expand sidebar
          </div>,
          document.body
        )}
    </div>
  );
}

// Sits in the branding row in place of the logo when collapsed — a small
// square button (matching the earlier squared-icon design), always expands
// since this only ever renders in the collapsed state.
function CollapsedToggleButton({ onToggle, resolvedTheme, chevronStyle }) {
  const anchorRef = useRef(null);
  const [tooltipPos, setTooltipPos] = useState(null);

  const showTooltip = () => {
    if (!anchorRef.current) return;
    const rect = anchorRef.current.getBoundingClientRect();
    setTooltipPos({ top: rect.top + rect.height / 2, left: rect.right + 12 });
  };
  const hideTooltip = () => setTooltipPos(null);

  return (
    <div ref={anchorRef} onMouseEnter={showTooltip} onMouseLeave={hideTooltip}>
      <button
        onClick={onToggle}
        aria-label="Expand sidebar"
        style={{ borderColor: resolvedTheme.border, background: "rgba(255,255,255,0.04)" }}
        className="p-1.5 rounded-md hover:bg-white/10 border"
      >
        <ChevronRight size={18} style={chevronStyle} />
      </button>

      {tooltipPos &&
        createPortal(
          <div
            style={{ position: "fixed", top: tooltipPos.top, left: tooltipPos.left, transform: "translateY(-50%)" }}
            className="z-[100] pointer-events-none bg-white text-black text-xs px-3 py-1 rounded-md shadow-md whitespace-nowrap"
          >
            Expand sidebar
          </div>,
          document.body
        )}
    </div>
  );
}

export default function UnifiedSidebar({ isCollapsed, onToggle }) {
  const location = useLocation();
  const [openMenus, setOpenMenus] = useState({});
  const { getCurrentUser } = useAuth();

  // Scrollbar should only be visible while actively scrolling, not sitting on
  // screen permanently — flip a class on scroll and drop it again once the
  // user stops for a moment.
  const [isNavScrolling, setIsNavScrolling] = useState(false);
  const navScrollTimeoutRef = useRef(null);
  const handleNavScroll = () => {
    setIsNavScrolling(true);
    if (navScrollTimeoutRef.current) clearTimeout(navScrollTimeoutRef.current);
    navScrollTimeoutRef.current = setTimeout(() => setIsNavScrolling(false), 800);
  };
  useEffect(() => () => navScrollTimeoutRef.current && clearTimeout(navScrollTimeoutRef.current), []);

  // ── Live sidebar settings from Context ─────────────────────────────────────
  const { sidebarSettings, resolvedTheme } = useSidebarSettings();

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
        { key: "weighing-axle-config", label: "Axle Configuration",icon: <List size={16} />,           path: `${basePath}/weighing/axle-config`,roles: null },
        { key: "weighing-saccos",      label: "Saccos",            icon: <User2 size={16} />,           path: `${basePath}/saccos`,              roles: null },
      ],
    },
    // User Management — Supervisor and Admin only (Manager does not get staff admin)
    { key: "user-management",  label: "User Management",  icon: <Users size={18} />,    path: `${basePath}/user-management`,   roles: ["Admin", "Supervisor"] },
    // Analytics, Reports — everyone except Operator (reporting, not master data)
    { key: "analytics",        label: "Analytics",         icon: <BarChart3 size={18} />,path: `${basePath}/analytics`,        roles: null, excludeRoles: ["Operator"] },
    { key: "reports",          label: "Reports",           icon: <FileText size={18} />, path: `${basePath}/reports`,          roles: null, excludeRoles: ["Operator"] },
    // Shifts / Shift Assignment — Manager, Supervisor, Admin (staff scheduling)
    { key: "shifts",           label: "Shifts",            icon: <User size={18} />,     path: `${basePath}/shifts`,           roles: ["Admin", "Manager", "Supervisor"] },
    { key: "shift-assignment", label: "Shift Assignment",  icon: <Users size={18} />,    path: `${basePath}/shift-assignment`, roles: ["Admin", "Manager", "Supervisor"] },
    // Security — Permissions/Roles/Password Policy/Audit Logs live as tabs
    // inside a single page now instead of separate nav items.
    { key: "security", label: "Security", icon: <Shield size={18} />, path: `${basePath}/security`, roles: ["Admin"] },
    // System / Backup — the genuinely dangerous levers, Admin only
    { key: "system",      label: "System",         icon: <Cog size={18} />,      path: `${basePath}/system`,              roles: ["Admin"] },
    { key: "backup",      label: "Backup",         icon: <Database size={18} />, path: `${basePath}/backup/microservice`, roles: ["Admin"] },
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
      className={`fixed top-0 left-0 h-full z-40 flex flex-col shadow-lg transition-all duration-300 overflow-x-hidden ${
        isCollapsed ? "w-16" : "w-64"
      }`}
      aria-label="Main sidebar"
    >
      {/* ── Branding — fixed height to line up exactly with the topbar/app-bar ──
          Collapsed: the logo doesn't fit meaningfully next to a toggle in a
          64px-wide row, so the toggle takes its place here instead; expanded:
          logo as usual, toggle lives in the nav list below. */}
      <div className={`h-14 flex items-center shrink-0 ${isCollapsed ? "justify-center" : "px-3"}`}>
        {isCollapsed ? (
          <CollapsedToggleButton onToggle={onToggle} resolvedTheme={resolvedTheme} chevronStyle={chevronStyle} />
        ) : (
          <img
            src={qalitrackLogoFull}
            alt="QaliTrack"
            className="h-8 w-auto max-w-[150px] object-contain transition-all duration-300"
          />
        )}
      </div>
      <div style={{ borderBottom: `1px solid ${resolvedTheme.border}` }} />

      {/* ── Navigation ──────────────────────────────────────────────────────── */}
      <nav
        onScroll={handleNavScroll}
        className={`sidebar-nav-scroll ${isNavScrolling ? "is-scrolling" : ""} flex-1 overflow-y-auto overflow-x-hidden py-2 ${
          isCollapsed ? "" : "px-1"
        }`}
      >
        {/* No horizontal padding here when collapsed: the branding row's
            toggle button is centered across the full 64px width, so the
            nav's px-1 would otherwise nudge these icons a few pixels off
            from that column (this is what the reference screenshots show —
            icons and the collapse chevron share one vertical column). */}
        {!isCollapsed && (
          <>
            <ToggleNavItem isCollapsed={isCollapsed} onToggle={onToggle} resolvedTheme={resolvedTheme} accentStyle={accentStyle} />
            <div style={{ borderBottom: `1px solid ${resolvedTheme.border}`, opacity: 0.6 }} className="mb-2" />
          </>
        )}

        {menuItems.map((item) => {
          if (item.children) {
            if (isCollapsed) {
              // A group icon alone can't show its children and grouping/labels
              // don't mean anything once collapsed, so a click on it used to do
              // nothing. Flatten instead: each sub-item becomes its own icon
              // with a tooltip, exactly like a top-level item.
              return item.children.map((child) => (
                <NavIconItem key={child.key} to={child.path} icon={child.icon} label={child.label} isCollapsed={isCollapsed} resolvedTheme={resolvedTheme} accentStyle={accentStyle} />
              ));
            }
            return (
              <div key={item.key} className="mb-1">
                <button
                  onClick={() => toggleMenu(item.key)}
                  style={openMenus[item.key] ? { background: resolvedTheme.hoverBg } : {}}
                  className="flex items-center justify-between w-full px-3 py-2 rounded-r-md transition-colors duration-150"
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
                    <span className="text-sm" style={{ color: resolvedTheme.text }}>
                      {item.label}
                    </span>
                  </span>
                  <span style={{ color: resolvedTheme.text, opacity: 0.6 }}>
                    {openMenus[item.key] ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                  </span>
                </button>

                <div className={`${openMenus[item.key] ? "block" : "hidden"} mt-1`}>
                  {item.children.map((child) => (
                    <NavLink
                      key={child.key}
                      to={child.path}
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
                      <span style={{ color: resolvedTheme.text, opacity: 0.8 }}>{child.icon}</span>
                      <span className="text-sm" style={{ color: resolvedTheme.text, opacity: 0.85 }}>
                        {child.label}
                      </span>
                    </NavLink>
                  ))}
                </div>
              </div>
            );
          }
          return <NavIconItem key={item.key} to={item.path} icon={item.icon} label={item.label} isCollapsed={isCollapsed} resolvedTheme={resolvedTheme} accentStyle={accentStyle} />;
        })}
      </nav>

      {/* ── Footer ──────────────────────────────────────────────────────────── */}
      {!isCollapsed && (
        <div
          className="px-3 py-3 text-xs"
          style={{ borderTop: `1px solid ${resolvedTheme.border}`, opacity: 0.75 }}
        >
          <div className="text-center space-y-0.5">
            <p style={{ color: resolvedTheme.text }} className="text-[10px]">
              Powered by{" "}
              <span style={accentStyle} className="font-semibold">
                Qalibrated Systems
              </span>
            </p>
            <p style={{ color: resolvedTheme.text }} className="text-[10px] opacity-60">
              &copy; {new Date().getFullYear()} All rights reserved.
            </p>
          </div>
        </div>
      )}

      <style>{`
        .sidebar-nav-scroll {
          scrollbar-width: thin;
          scrollbar-color: transparent transparent;
        }
        .sidebar-nav-scroll.is-scrolling {
          scrollbar-color: rgba(245, 158, 11, 0.35) transparent;
        }
        .sidebar-nav-scroll::-webkit-scrollbar {
          width: 6px;
        }
        .sidebar-nav-scroll::-webkit-scrollbar-track {
          background: transparent;
        }
        .sidebar-nav-scroll::-webkit-scrollbar-thumb {
          background-color: transparent;
          border-radius: 999px;
          transition: background-color 0.3s ease;
        }
        .sidebar-nav-scroll.is-scrolling::-webkit-scrollbar-thumb {
          background-color: rgba(245, 158, 11, 0.35);
        }
        .sidebar-nav-scroll::-webkit-scrollbar-thumb:hover {
          background-color: rgba(245, 158, 11, 0.55);
        }
      `}</style>
    </aside>
  );
}