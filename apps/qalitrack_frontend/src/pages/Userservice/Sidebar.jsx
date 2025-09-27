import { NavLink, useNavigate } from "react-router-dom";
import {
    LayoutDashboard,
    BarChart3,
    FileText,
    Users,
    Lock,
    User,
    Shield,
    LogOut
} from "lucide-react";
import useAuth from '../../helpers/auth';
import { useState } from 'react';

export default function UserServiceSidebar() {
    const { logout } = useAuth();
    const navigate = useNavigate();
    const [isOpen, setIsOpen] = useState(false);



    const linkClasses = ({ isActive }) =>
        `flex items-center gap-3 px-4 py-2.5 rounded-md transition-colors duration-200 ${
            isActive
                ? "bg-amber-100 text-amber-700 font-semibold border-l-4 border-amber-500"
                : "text-gray-600 hover:bg-gray-100"
        }`;

    const menuItems = [
        {
            key: "admin-dashboard",
            label: "Dashboard",
            icon: <LayoutDashboard size={18} />,
            path: "/admin/dashboard",
        },
        {
            key: "Shifts",
            label: "Shifts",
            icon: <User size={18} />,
            path: "/admin/shifts",
        },
        {
            key: "shift-assignment",
            label: "Shift Assignment",
            icon: <User size={18} />,
            path: "/admin/shift-assignment",
        },
        {
            key: "persmissions",
            label: "Permissions",
            icon: <Shield size={18} />,
            path: "/admin/security/permissions",
        },
        {
            key: "roles",
            label: "Roles",
            icon: <Users size={18} />,
            path: "/admin/security/roles",
        },
        {
            key: "password-policy",
            label: "Password Policy",
            icon: <Lock size={18} />,
            path: "/admin/security/password-policy",
        },
        {
            key: "BackupService",
            label: "BackupService",
            icon: <Shield size={18} />,
            path: "/admin/backup/microservice",
        },
    ];

    return (
        <>
            <button
                className="md:hidden fixed top-4 left-4 z-50 px-4 bg-amber-500 text-white rounded-md"
                onClick={() => setIsOpen(!isOpen)}
            >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={isOpen ? "M6 18L18 6M6 6l12 12" : "M4 6h16M4 12h16M4 18h16"} />
                </svg>
            </button>

            <aside className={`fixed inset-y-0 left-0 bg-white border-r border-gray-200 flex flex-col h-full transition-all duration-300
                ${isOpen ? 'w-64' : 'w-20'} md:w-64 ${isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'}`}>
                <div className="px-4 py-4 font-bold text-xl md:text-2xl text-center">
                    <span className="hidden md:inline">Admin Panel</span>
                    <span className="md:hidden">AP</span>
                </div>

                <nav className="flex-1 space-y-2 overflow-y-auto px-4">
                    {menuItems.map((item) => (
                        <NavLink
                            key={item.key}
                            to={item.path}
                            className={linkClasses}
                            title={item.label}
                            onClick={() => setIsOpen(false)}
                        >
                            <span className="p-1 rounded-md bg-amber-500 text-white">
                                {item.icon}
                            </span>
                            <span className={`${isOpen ? 'inline' : 'hidden'} md:inline`}>{item.label}</span>
                        </NavLink>
                    ))}
                </nav>
            </aside>
        </>
    );
}