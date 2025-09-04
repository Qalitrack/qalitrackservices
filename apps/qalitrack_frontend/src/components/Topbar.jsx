import { Link, useLocation } from "react-router-dom";
import { Bell, Search, User, LogOut } from "lucide-react";
import useAuth from "../helpers/auth";
import { useState, useRef, useEffect } from "react";

export default function Topbar() {
    const location = useLocation();
    const { getCurrentUser, logout } = useAuth();
    const user = getCurrentUser();
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const dropdownRef = useRef(null);

    // Better role extraction with fallback
    const getUserRole = () => {
        if (!user || !user.userRoles) return "Guest";

        // Handle both array of strings and array of objects
        const role = user.userRoles[0];
        if (typeof role === "string") {
            return role;
        } else if (typeof role === "object" && role !== null) {
            return role.name || role.role || role.type || "User";
        }

        return "User";
    };

    const userRole = getUserRole();
    const userName = user?.firstName ? `${user.firstName} ${user.lastName}` : user?.email || "User";

    // Close dropdown when clicking outside
    useEffect(() => {
        function handleClickOutside(event) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setDropdownOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    // Breadcrumb
    const pathSegments = location.pathname.split("/").filter(Boolean);
    const formatLabel = (segment) =>
        segment.replace(/-/g, " ").replace(/\b\w/g, (c) => c.toUpperCase());

    const handleLogout = () => {
        logout();
        window.location.href = "/login";
    };

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
                        className="pl-8 pr-3 py-1.5 h-9 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-amber-500 text-sm"
                    />
                </div>

                {/* Alerts */}
                <button className="relative h-9 w-9 flex items-center justify-center border rounded hover:bg-gray-50">
                    <Bell size={18} className="text-gray-600" />
                    <span className="absolute -top-1 -right-1 h-4 w-4 bg-red-500 text-white text-xs flex items-center justify-center rounded-full">
                        3
                    </span>
                </button>

                {/* User Profile Dropdown */}
                <div className="relative" ref={dropdownRef}>
                    <button
                        onClick={() => setDropdownOpen(!dropdownOpen)}
                        className="h-9 px-3 border rounded flex items-center gap-2 hover:bg-gray-50"
                    >
                        <User size={18} className="text-gray-600" />
                        <span className="text-sm text-gray-700">{userName}</span>
                    </button>

                    {dropdownOpen && (
                        <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-10 border border-gray-200">
                            <div className="p-3 border-b border-gray-100">
                                <p className="text-sm font-medium text-gray-800">{userName}</p>
                                <p className="text-xs text-gray-500">{user?.email}</p>
                                <div className="mt-1 flex items-center">
                                    <span className="h-2 w-2 rounded-full bg-green-500 mr-1.5"></span>
                                    <p className="text-xs font-medium text-amber-600 capitalize">{userRole}</p>
                                </div>
                            </div>
                            <div className="p-2">
                                <Link
                                    to="/profile"
                                    className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md"
                                    onClick={() => setDropdownOpen(false)}
                                >
                                    Profile Settings
                                </Link>
                                <button
                                    onClick={handleLogout}
                                    className="flex items-center w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-100 rounded-md"
                                >
                                    <LogOut size={14} className="mr-2" />
                                    Logout
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
}