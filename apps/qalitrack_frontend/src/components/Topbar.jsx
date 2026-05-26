import { Link, useLocation, useNavigate } from "react-router-dom";
import { Bell, User, LogOut, Menu } from "lucide-react";
import useAuth from "../api/helpers/auth";
import { useState, useRef, useEffect } from "react";
import { useSidebarSettings } from "../components/Context/Sidebarsettingscontext";

export default function Topbar({ onToggleSidebar }) {
  const location = useLocation();
  const { getCurrentUser, logout } = useAuth();
  const { sidebarSettings } = useSidebarSettings();
  const user = getCurrentUser ? getCurrentUser() : null;
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();

  // Extract role safely
  const getUserRole = () => {
    if (!user || !user.userRoles) return "Guest";
    const role = user.userRoles[0];
    if (typeof role === "string") return role;
    if (typeof role === "object" && role !== null) {
      return role.name || role.role || role.type || "User";
    }
    return "User";
  };

  const userRole = getUserRole();
  const userName = user?.firstName ? `${user.firstName} ${user.lastName ?? ""}` : user?.email || "User";
  const profilePath = userRole === "Admin" ? "/admin/profile" : "/operator/profile";

  // Click outside to close dropdown
  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Breadcrumb
  const pathSegments = location.pathname.split("/").filter(Boolean);
  const formatLabel = (segment) =>
    segment.replace(/-/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());

  const handleLogout = () => {
    logout && logout();
    navigate("/");
  };

  return (
    <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-3 md:px-6 ml-16 md:ml-64">
      {/* Left: mobile toggle + breadcrumb */}
      <div className="flex items-center gap-3">
        {/* Mobile sidebar toggle */}
        <button
          onClick={onToggleSidebar}
          className="md:hidden h-9 w-9 flex items-center justify-center rounded hover:bg-gray-50"
          aria-label="Toggle sidebar"
        >
          <Menu size={20} className="text-gray-700" />
        </button>

        {/* Breadcrumb */}
        <div className="hidden md:flex items-center gap-2 text-sm text-gray-700">
          <Link to="/" className="text-gray-500 hover:underline">
            Home
          </Link>
          {pathSegments.map((segment, idx) => {
            const path = "/" + pathSegments.slice(0, idx + 1).join("/");
            const isLast = idx === pathSegments.length - 1;
            return (
              <span key={path} className="flex items-center gap-2">
                <span className="text-gray-300">/</span>
                {isLast ? (
                  <span className="text-gray-800">{formatLabel(segment)}</span>
                ) : (
                  <Link to={path} className="text-gray-500 hover:underline">
                    {formatLabel(segment)}
                  </Link>
                )}
              </span>
            );
          })}
        </div>
      </div>

      {/* Right: search, bell, profile */}
      <div className="flex items-center gap-3 md:gap-4">
        {/* Company logo from system settings */}
        {sidebarSettings?.companyLogo && (
          <img
            src={sidebarSettings.companyLogo}
            alt={sidebarSettings.companyName || "Company"}
            className="h-8 w-auto max-w-[120px] object-contain rounded"
          />
        )}

        {/* Notifications */}
        <button className="relative h-9 w-9 flex items-center justify-center border rounded hover:bg-gray-50">
          <Bell size={18} className="text-gray-600" />
          <span className="absolute -top-1 -right-1 h-4 w-4 bg-red-500 text-white text-xs flex items-center justify-center rounded-full">
            3
          </span>
        </button>

        {/* Profile dropdown */}
        <div className="relative" ref={dropdownRef}>
          <button
            onClick={() => setDropdownOpen((s) => !s)}
            className="h-9 px-2 md:px-3 border rounded flex items-center gap-2 hover:bg-gray-50"
            aria-haspopup="true"
            aria-expanded={dropdownOpen}
          >
            <User size={18} className="text-gray-600" />
            <span className="hidden sm:inline text-sm text-gray-700">{userName}</span>
          </button>

          {dropdownOpen && (
            <div className="absolute right-0 mt-2 w-52 bg-white rounded-md shadow-lg z-50 border border-gray-200">
              <div className="p-3 border-b border-gray-100">
                <p className="text-sm font-medium text-gray-800">{userName}</p>
                <p className="text-xs text-gray-500">{user?.email}</p>
                <div className="mt-1 flex items-center gap-2">
                  <span className="h-2 w-2 rounded-full bg-green-500 inline-block" />
                  <p className="text-xs font-medium text-amber-500 capitalize">{userRole}</p>
                </div>
              </div>

              <div className="p-2">
                <Link
                  to={profilePath}
                  onClick={() => setDropdownOpen(false)}
                  className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
                >
                  Profile Settings
                </Link>

                <button
                  onClick={handleLogout}
                  className="flex items-center w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100 rounded-md"
                >
                  <LogOut size={14} className="mr-2" /> Logout
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
