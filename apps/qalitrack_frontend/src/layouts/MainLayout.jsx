import { useState, useCallback } from "react";
import { Outlet } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import UnifiedSidebar from "../components/Sidebar"; // ← Use the new unified sidebar
import Topbar from "../components/Topbar";

export default function MainLayout() {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggleSidebar = useCallback(() => {
    setIsCollapsed((prev) => !prev);
  }, []);

  const sidebarWidth = isCollapsed ? "w-16" : "w-64";

  return (
    <div className="flex h-screen w-screen bg-gray-100 overflow-hidden">
      <aside className={`h-full transition-all duration-300 ease-in-out bg-black text-white ${sidebarWidth} shrink-0`}>
        <UnifiedSidebar isCollapsed={isCollapsed} onToggle={toggleSidebar} />
      </aside>

      <div className="flex flex-col flex-1 min-w-0 h-full overflow-hidden">
        <header className="h-14 bg-white shadow-sm z-20 shrink-0">
          <Topbar isCollapsed={isCollapsed} onToggleSidebar={toggleSidebar} />
        </header>

        <main className="flex-1 bg-gray-50 overflow-hidden relative">
          <div className="absolute inset-0 p-2">
            <Outlet />
          </div>
        </main>
      </div>
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            fontSize: "1rem",
            padding: "14px 18px",
            minWidth: "320px",
            maxWidth: "480px",
          },
          duration: 4000,
        }}
      />
    </div>
  );
}