// src/components/Sidebar.jsx
import { NavLink } from "react-router-dom";
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
import { useState } from "react";

export default function Sidebar() {
  const [openWeighing, setOpenWeighing] = useState(false);

  return (
    <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
      <div className="px-4 py-4 font-bold text-2xl">Qalitrack</div>

      <nav className="flex-1 px-2 space-y-1">
        {/* Dashboard */}
        <NavLink
          to="/"
          end
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <LayoutDashboard size={18} />
          Dashboard
        </NavLink>

        {/* Weighing with Submenu */}
        <div>
          <button
            onClick={() => setOpenWeighing(!openWeighing)}
            className={`flex items-center justify-between w-full px-3 py-2 rounded hover:bg-gray-100 ${
              openWeighing ? "bg-gray-200 font-medium" : ""
            }`}
          >
            <span className="flex items-center gap-2">
              <Scale size={18} />
              Weighing
            </span>
            {openWeighing ? (
              <ChevronDown size={16} />
            ) : (
              <ChevronRight size={16} />
            )}
          </button>

          {openWeighing && (
            <div className="ml-6 mt-1 space-y-1">
              <NavLink
                to="/weighing/factory"
                className={({ isActive }) =>
                  `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
                    isActive ? "bg-gray-200 font-medium" : ""
                  }`
                }
              >
                <Factory size={16} />
                Factory Weighing
              </NavLink>
              <NavLink
                to="/weighing/vehicle"
                className={({ isActive }) =>
                  `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
                    isActive ? "bg-gray-200 font-medium" : ""
                  }`
                }
              >
                <Truck size={16} />
                Vehicles
              </NavLink>
              <NavLink
                to="/weighing/drivers"
                className={({ isActive }) =>
                  `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
                    isActive ? "bg-gray-200 font-medium" : ""
                  }`
                }
              >
                <User size={16} />
                Drivers
              </NavLink>
            </div>
          )}
        </div>

        {/* Automation */}
        <NavLink
          to="/automation"
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <Cog size={18} />
          Automation
        </NavLink>

        {/* Calibrations */}
        <NavLink
          to="/calibrations"
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <Wrench size={18} />
          Calibrations
        </NavLink>

        {/* Analytics */}
        <NavLink
          to="/analytics"
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <BarChart3 size={18} />
          Analytics
        </NavLink>

        {/* Reports */}
        <NavLink
          to="/reports"
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <FileText size={18} />
          Reports
        </NavLink>

        {/* System */}
        <NavLink
          to="/system"
          className={({ isActive }) =>
            `flex items-center gap-2 px-3 py-2 rounded hover:bg-gray-100 ${
              isActive ? "bg-gray-200 font-medium" : ""
            }`
          }
        >
          <Cog size={18} />
          System
        </NavLink>
      </nav>

      <div className="p-3 text-sm text-gray-500 border-t">v0.1</div>
    </aside>
  );
}
