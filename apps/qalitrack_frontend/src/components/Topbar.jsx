import { Link, useLocation } from "react-router-dom";
import { Bell, Search, User } from "lucide-react";

// TODO: Replace with Redux/Context later
const currentUserRole = "admin"; // or "operator"

export default function Topbar() {
  const location = useLocation();

  // Breadcrumb
  const pathSegments = location.pathname.split("/").filter(Boolean);
  const formatLabel = (segment) =>
    segment.replace(/-/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());

  return (
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
  );
}
