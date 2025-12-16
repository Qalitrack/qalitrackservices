// src/layouts/MainLayout.jsx
import { useState, useCallback } from "react";
import { Outlet } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar";

export default function MainLayout() {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggleSidebar = useCallback(() => {
    setIsCollapsed(prev => !prev);
  }, []);

  return (
    <div className="flex h-screen bg-white overflow-hidden">

      {/* ───────────────────────────────────────────────
          FIXED SIDEBAR (Does NOT push content)
      ─────────────────────────────────────────────── */}
      <aside
        className={`
          fixed top-0 left-0 h-full z-30
          transition-all duration-300 
          bg-black text-white border-r border-neutral-800
          ${isCollapsed ? "w-16" : "w-64"}
        `}
      >
        <Sidebar isCollapsed={isCollapsed} onToggle={toggleSidebar} />
      </aside>

      {/* ───────────────────────────────────────────────
          MAIN WRAPPER (Static & Responsive)
      ─────────────────────────────────────────────── */}
      <div
        className={`
          flex flex-col flex-1 h-full overflow-hidden
          transition-all duration-300
          ${isCollapsed ? "ml-16" : "ml-64"}
        `}
      >
        {/* ─── TOPBAR ─────────────────────────────── */}
        <Topbar isCollapsed={isCollapsed} onToggleSidebar={toggleSidebar} />

        {/* ─── CONTENT (The MAIN scroll area) ─────────────────────────────── */}
        <main className="flex-1 overflow-y-auto p-4 bg-gray-100">
          <Outlet />
        </main>

      </div>
    </div>
  );
}