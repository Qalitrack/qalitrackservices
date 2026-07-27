import React, { lazy } from 'react';
import { Navigate } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoutes.jsx";
import useAuth from '../api/helpers/auth.js';
import FeatureLicenseGate from '../components/FeatureLicenseGate.jsx';
import { LicenseFeatures } from '../utils/LicenseFeatures.js';

// ── Layout ──
const MainLayout = lazy(() => import("../layouts/MainLayout.jsx"));

// ── Auth ──
const Login = lazy(() => import("../pages/auth/Login.jsx"));

// ── Pages ──
const Dashboard          = lazy(() => import("../pages/weighing/Dashboard.jsx"));
const FactoryWeighing    = lazy(() => import("../components/weighing/WeighingDashboard.jsx"));
const Automation         = lazy(() => import("../pages/weighing/Automation.jsx"));
const Calibrations       = lazy(() => import("../pages/weighing/Calibrations.jsx"));
const Analytics          = lazy(() => import("../pages/weighing/Analytics.jsx"));
const Reports            = lazy(() => import("../pages/weighing/Reports.jsx"));
const System             = lazy(() => import("../pages/weighing/System.jsx"));
const AdminDashboard     = lazy(() => import("../pages/Userservice/AdminDashboard.jsx"));
const Security           = lazy(() => import("../pages/Userservice/Security.jsx"));
const ShiftsHub           = lazy(() => import("../pages/Userservice/ShiftsHub.jsx"));
const Microservice       = lazy(() => import("../pages/Userservice/Backup/Microservice.jsx"));
const FleetHub           = lazy(() => import("../pages/weighing/FleetHub.jsx"));
const CommerceHub        = lazy(() => import("../pages/weighing/CommerceHub.jsx"));
const Routes             = lazy(() => import("../pages/Routes.jsx"));
const WeighbridgesPortal = lazy(() => import("../pages/weighing/WeighingBridge.jsx"));
const Transaction        = lazy(() => import("../pages/Transaction.jsx"));
const UserManagement     = lazy(() => import("../pages/UserManagement.jsx"));
const Profile            = lazy(() => import("../pages/Profile.jsx"));

// ── Kiosk (public) ──
const SelfServiceWeighing = lazy(() => import("../pages/SelfServiceWeighing.jsx"));

// ═══════════════════════════════════════════════════════════════════════════
// Root redirect
// ═══════════════════════════════════════════════════════════════════════════
const RootRedirect = () => {
    const { isAuthenticated, getCurrentUser } = useAuth();
    if (!isAuthenticated()) return <Navigate to="/login" replace />;

    const user = getCurrentUser();
    const primaryRole = user?.userRoles?.[0];

    // Admin gets the admin dashboard; everyone else lands on factory weighing
    if (primaryRole === "Admin") return <Navigate to="/admin/dashboard" replace />;
    return <Navigate to="/operator/weighing/factory" replace />;
};

// ═══════════════════════════════════════════════════════════════════════════
// Role-specific index redirects
// ═══════════════════════════════════════════════════════════════════════════
const AdminIndexRedirect = () => <Navigate to="/admin/dashboard" replace />;
const OperatorIndexRedirect = () => <Navigate to="/operator/weighing/factory" replace />;

// 404
const NotFound = () => (
    <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
            <h1 className="text-4xl font-bold text-gray-700 mb-4">404</h1>
            <p className="text-gray-500 mb-4">Page Not Found</p>
            <button onClick={() => window.history.back()} className="px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600">
                Go Back
            </button>
        </div>
    </div>
);

// ═══════════════════════════════════════════════════════════════════════════
// Routes
// ═══════════════════════════════════════════════════════════════════════════
export const routes = [
    // ───────────────────────────────────────────────────────────────────────
    // /kiosk — PUBLIC. No layout. No ProtectedRoute.
    // ───────────────────────────────────────────────────────────────────────
    {
        path: "/kiosk",
        element: <SelfServiceWeighing />
    },

    // ── Public ──
    { path: "/login", element: <Login /> },
    { path: "/",      element: <RootRedirect /> },

    // ── Operator (protected) — any authenticated user ──
    {
        path: "",
        element: <ProtectedRoute allowedRoles={[]} />,
        children: [
            {
                path: "/operator",
                element: <MainLayout />,
                children: [
                    // Redirect /operator to /operator/weighing/factory
                    { index: true, element: <OperatorIndexRedirect /> },
                    
                    { path: "dashboard",              element: <AdminDashboard /> },
                    { path: "weighing/factory",       element: <FactoryWeighing /> },
                    { path: "transactions",           element: <Transaction /> },
                    { path: "fleet",                  element: <FleetHub /> },
                    { path: "fleet/:tab",             element: <FleetHub /> },
                    { path: "commerce",               element: <CommerceHub /> },
                    { path: "commerce/:tab",          element: <CommerceHub /> },
                    { path: "automation",             element: <Automation /> },
                    { path: "calibrations",           element: <Calibrations /> },
                    { path: "weighbridges",           element: <WeighbridgesPortal /> },
                    { path: "routes",                 element: <Routes /> },
                    { path: "profile",                element: <Profile /> },

                    // Reporting — everyone except Operator (not master data)
                    {
                        element: <ProtectedRoute allowedRoles={["Admin", "Manager", "Supervisor"]} />,
                        children: [
                            { path: "analytics", element: <FeatureLicenseGate feature={LicenseFeatures.ANALYTICS}><Analytics /></FeatureLicenseGate> },
                            { path: "reports",   element: <FeatureLicenseGate feature={LicenseFeatures.REPORTS}><Reports /></FeatureLicenseGate> },
                            { path: "shifts",            element: <FeatureLicenseGate feature={LicenseFeatures.SHIFTS}><ShiftsHub /></FeatureLicenseGate> },
                            { path: "shifts/:tab",       element: <FeatureLicenseGate feature={LicenseFeatures.SHIFTS}><ShiftsHub /></FeatureLicenseGate> },
                        ],
                    },

                    // Staff administration — Supervisor and Admin only
                    {
                        element: <ProtectedRoute allowedRoles={["Admin", "Supervisor"]} />,
                        children: [
                            { path: "user-management", element: <FeatureLicenseGate feature={LicenseFeatures.USER_MANAGEMENT}><UserManagement /></FeatureLicenseGate> },
                        ],
                    },

                    // System settings — the dangerous lever, Admin only
                    {
                        element: <ProtectedRoute allowedRoles={["Admin"]} />,
                        children: [
                            { path: "system", element: <System /> },
                        ],
                    },
                ]
            }
        ]
    },

    // ── Admin (protected) ──
    {
        path: "",
        element: <ProtectedRoute allowedRoles={["Admin"]} />,
        children: [
            {
                path: "/admin",
                element: <MainLayout />,
                children: [
                    // Redirect /admin to /admin/dashboard
                    { index: true, element: <AdminIndexRedirect /> },
                    
                    { path: "dashboard",                element: <AdminDashboard /> },
                    { path: "weighing/factory",         element: <FactoryWeighing /> },
                    { path: "transactions",             element: <Transaction /> },
                    { path: "fleet",                     element: <FleetHub /> },
                    { path: "fleet/:tab",                element: <FleetHub /> },
                    { path: "commerce",                 element: <CommerceHub /> },
                    { path: "commerce/:tab",             element: <CommerceHub /> },
                    { path: "automation",               element: <Automation /> },
                    { path: "calibrations",             element: <Calibrations /> },
                    { path: "analytics",                element: <FeatureLicenseGate feature={LicenseFeatures.ANALYTICS}><Analytics /></FeatureLicenseGate> },
                    { path: "reports",                  element: <FeatureLicenseGate feature={LicenseFeatures.REPORTS}><Reports /></FeatureLicenseGate> },
                    { path: "system",                   element: <System /> },
                    { path: "weighbridges",             element: <WeighbridgesPortal /> },
                    { path: "routes",                   element: <Routes /> },
                    // admin-only
                    { path: "user-management",          element: <FeatureLicenseGate feature={LicenseFeatures.USER_MANAGEMENT}><UserManagement /></FeatureLicenseGate> },
                    { path: "security",                 element: <Security /> },
                    { path: "security/:tab",             element: <Security /> },
                    { path: "shifts",                   element: <FeatureLicenseGate feature={LicenseFeatures.SHIFTS}><ShiftsHub /></FeatureLicenseGate> },
                    { path: "shifts/:tab",               element: <FeatureLicenseGate feature={LicenseFeatures.SHIFTS}><ShiftsHub /></FeatureLicenseGate> },
                    { path: "backup/microservice",      element: <FeatureLicenseGate feature={LicenseFeatures.BACKUP}><Microservice /></FeatureLicenseGate> },
                    { path: "profile",                  element: <Profile /> },
                ]
            }
        ]
    },

    // ── 404 (must be last) ──
    { path: "*", element: <NotFound /> }
];