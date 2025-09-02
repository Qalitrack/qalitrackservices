import { useState, useEffect } from "react";
import { Outlet, NavLink, useLocation, Link } from "react-router-dom";
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
  Bell,
  Search,
} from "lucide-react";

// Simulate user role (replace with Redux or context later)
const currentUserRole = "admin"; // or "operator"

export default function Dashboard() {
  const location = useLocation();
  const [openWeighing, setOpenWeighing] = useState(false);

  // Auto-open submenu if current route starts with /weighing
  useEffect(() => {
    if (location.pathname.startsWith("/weighing")) {
      setOpenWeighing(true);
    }
  }, [location.pathname]);

  // Common class helper for NavLink
  const linkClasses = ({ isActive }) =>
    `flex items-center gap-2 px-3 py-2 rounded transition-colors hover:bg-gray-100 ${
      isActive ? "bg-gray-200 font-medium border-l-4 border-green-500" : ""
    }`;

  // Breadcrumb generator
  const pathSegments = location.pathname.split("/").filter(Boolean);
  const formatLabel = (segment) =>
    segment.charAt(0).toUpperCase() + segment.slice(1);

  // Menu config with role restrictions
  const menuItems = [
    {
      key: "/",
      label: "Dashboard",
      icon: <LayoutDashboard size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "weighing",
      label: "Weighing",
      icon: <Scale size={18} />,
      roles: ["admin", "operator"],
      children: [
        {
          key: "/weighing/factory",
          label: "Factory Weighing",
          icon: <Factory size={16} />,
          roles: ["admin", "operator"],
        },
        {
          key: "/weighing/vehicle",
          label: "Vehicles",
          icon: <Truck size={16} />,
          roles: ["admin", "operator"],
        },
        {
          key: "/weighing/drivers",
          label: "Drivers",
          icon: <User size={16} />,
          roles: ["admin"],
        },
      ],
    },
    {
      key: "/automation",
      label: "Automation",
      icon: <Cog size={18} />,
      roles: ["admin"],
    },
    {
      key: "/calibrations",
      label: "Calibrations",
      icon: <Wrench size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "/analytics",
      label: "Analytics",
      icon: <BarChart3 size={18} />,
      roles: ["admin"],
    },
    {
      key: "/reports",
      label: "Reports",
      icon: <FileText size={18} />,
      roles: ["admin", "operator"],
    },
    {
      key: "/system",
      label: "System",
      icon: <Cog size={18} />,
      roles: ["admin"],
    },
  ];

  // Filter menu by role
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
    <div className="flex h-screen bg-gray-50">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="px-4 py-4 font-bold text-2xl">Qalitrack</div>

        <nav className="flex-1 px-2 space-y-1">
          {filteredMenu.map((item) =>
            item.children ? (
              <div key={item.key}>
                <button
                  onClick={() => setOpenWeighing(!openWeighing)}
                  className={`flex items-center justify-between w-full px-3 py-2 rounded hover:bg-gray-100 ${
                    openWeighing ? "bg-gray-200 font-medium" : ""
                  }`}
                >
                  <span className="flex items-center gap-2">
                    {item.icon}
                    {item.label}
                  </span>
                  {openWeighing ? (
                    <ChevronDown size={16} />
                  ) : (
                    <ChevronRight size={16} />
                  )}
                </button>

                {openWeighing && (
                  <div className="ml-6 mt-1 space-y-1">
                    {item.children.map((child) => (
                      <NavLink key={child.key} to={child.key} className={linkClasses}>
                        {child.icon}
                        {child.label}
                      </NavLink>
                    ))}
                  </div>
                )}
              </div>
            ) : (
              <NavLink key={item.key} to={item.key} className={linkClasses}>
                {item.icon}
                {item.label}
              </NavLink>
            )
          )}
        </nav>

        <div className="p-3 text-sm text-gray-500 border-t">v0.1</div>
      </aside>

      {/* Main Area */}
      <div className="flex flex-col flex-1">
        {/* Topbar */}
        <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-4">
          {/* Breadcrumb */}
          <div className="font-medium text-gray-700 flex items-center gap-1 text-sm">
            <Link to="/" className="hover:underline text-gray-500">
              Home
            </Link>
            {pathSegments.map((segment, idx) => {
              const path = "/" + pathSegments.slice(0, idx + 1).join("/");
              const isLast = idx === pathSegments.length - 1;
              return (
                <span key={path} className="flex items-center gap-1">
                  <span>/</span>
                  {isLast ? (
                    <span className="text-gray-700">{formatLabel(segment)}</span>
                  ) : (
                    <Link to={path} className="hover:underline text-gray-500">
                      {formatLabel(segment)}
                    </Link>
                  )}
                </span>
              );
            })}
          </div>

          {/* Right Section */}
          <div className="flex items-center gap-4">
            {/* Search */}
            <div className="relative">
              <Search
                className="absolute left-2 top-1/2 -translate-y-1/2 text-gray-400"
                size={16}
              />
              <input
                type="text"
                placeholder="Search"
                className="pl-8 pr-3 py-1.5 h-9 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-green-500 text-sm"
              />
            </div>

            {/* Alerts */}
            <button className="relative h-9 w-9 flex items-center justify-center border rounded hover:bg-gray-50">
              <Bell size={18} className="text-gray-600" />
              <span className="absolute -top-1 -right-1 h-4 w-4 bg-red-500 text-white text-xs flex items-center justify-center rounded-full">
                3
              </span>
            </button>

            {/* User */}
            <div className="h-9 px-3 border rounded flex items-center gap-2 cursor-pointer hover:bg-gray-50">
              <User size={18} className="text-gray-600" />
              <span className="text-sm text-gray-700 capitalize">{currentUserRole}</span>
            </div>
          </div>
        </header>

        {/* Content */}
        <main className="flex-1 p-4 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
