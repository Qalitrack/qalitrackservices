import { Link, useNavigate } from "react-router-dom";
import { User, LogOut } from "lucide-react";
import useAuth from "../api/helpers/auth";
import { useState, useRef, useEffect } from "react";

export default function Topbar() {
  const { getCurrentUser, logout } = useAuth();
  const user = getCurrentUser ? getCurrentUser() : null;
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef(null);
  const navigate = useNavigate();

  const getUserRole = () => {
    if (!user?.userRoles) return "User";
    const role = user.userRoles[0];
    if (typeof role === "string") return role;
    if (typeof role === "object" && role !== null) return role.name || role.role || "User";
    return "User";
  };

  const userRole = getUserRole();
  const userName = user?.firstName
    ? `${user.firstName} ${user.lastName ?? ""}`.trim()
    : user?.email || "User";
  const profilePath = userRole === "Admin" ? "/admin/profile" : "/operator/profile";

  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setDropdownOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLogout = () => {
    logout && logout();
    navigate("/");
  };

  return (
    <header className="h-14 bg-white border-b border-gray-200 flex items-center justify-end px-3 md:px-6">
      {/* User dropdown */}
      <div className="relative" ref={dropdownRef}>
        <button
          onClick={() => setDropdownOpen((s) => !s)}
          className="h-9 px-3 border rounded flex items-center gap-2 hover:bg-gray-50"
          aria-haspopup="true"
          aria-expanded={dropdownOpen}
        >
          <User size={18} className="text-gray-600" />
          <span className="text-sm text-gray-700 font-medium">{userName}</span>
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
    </header>
  );
}
