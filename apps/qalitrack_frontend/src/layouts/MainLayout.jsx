import { useState, useCallback } from "react";
import { Outlet } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import Topbar from "../components/Topbar"; 

export default function MainLayout() {
  const [isCollapsed, setIsCollapsed] = useState(false);

  // Memoize the toggle function using useCallback. 
  // This is crucial for performance as it prevents the Sidebar component (a child) 
  // from re-rendering unless the state actually changes.
  const toggleSidebar = useCallback(() => {
    setIsCollapsed(prev => !prev);
  }, []);

  // Determine the left margin based on the sidebar's width.
  // This ensures the main content area always starts immediately after the sidebar.
  const contentShiftClass = isCollapsed ? "md:ml-16" : "md:ml-64"; 
  // The 'md:' prefix is important for responsiveness, allowing the layout to adapt 
  // on smaller screens where the sidebar might be hidden or overlaid.

  return (
    <div className="flex h-screen bg-gray-50">
      
      {/* 1. Sidebar (Fixed Component) */}
      {/* It controls the 'isCollapsed' state via the 'onToggle' prop */}
      <Sidebar 
        isCollapsed={isCollapsed} 
        onToggle={toggleSidebar} 
      />

      {/* 2. Main Content Wrapper (Dynamic Position) */}
      {/* The margin dynamically changes with the sidebar, and 'transition-all' smooths the movement. */}
      <div 
        className={`flex flex-col flex-1 transition-all duration-300 ${contentShiftClass}`}
      >
        
        {/* 3. Topbar (Header) */}
        {/* Renders at the top of the content area. */}
        <Topbar isCollapsed={isCollapsed} /> 

        {/* 4. Page Content */}
        <main className="flex-1 p-4 overflow-y-auto">
          {/* Outlet renders the component associated with the current route */}
          <Outlet />
        </main>
      </div>
    </div>
  );
}