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
const Vehicle            = lazy(() => import("../components/weighing/Vehicles.jsx"));
const Drivers            = lazy(() => import("../components/weighing/Drivers.jsx"));
const AdminDashboard     = lazy(() => import("../pages/Userservice/AdminDashboard.jsx"));
const PasswordPolicy     = lazy(() => import("../pages/Userservice/PasswordPolicy.jsx"));
const Permissions        = lazy(() => import("../pages/Userservice/Permissions.jsx"));
const Roles              = lazy(() => import("../pages/Userservice/Roles.jsx"));
const Shifts             = lazy(() => import("../pages/Userservice/Shifts.jsx"));
const ShiftAssignment    = lazy(() => import("../pages/Userservice/ShiftAssignment.jsx"));
const Attendance         = lazy(() => import("../pages/Userservice/Attendance.jsx"));
const Microservice       = lazy(() => import("../pages/Userservice/Backup/Microservice.jsx"));
const Transporters       = lazy(() => import("../pages/weighing/TransporterFormModal.jsx"));
const Suppliers          = lazy(() => import("../pages/Suppliers.jsx"));
const Routes             = lazy(() => import("../pages/Routes.jsx"));
const AxleConfigs        = lazy(() => import("../pages/weighing/AxleConfigs.jsx"));
const Owners             = lazy(() => import("../pages/weighing/Owners.jsx"));
const ProductsPortal     = lazy(() => import("../components/weighing/Product.jsx"));
const SaccosPortal       = lazy(() => import("../pages/Saccos.jsx"));
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
                    { path: "weighing/vehicle",       element: <Vehicle /> },
                    { path: "weighing/drivers",       element: <Drivers /> },
                    { path: "automation",             element: <Automation /> },
                    { path: "calibrations",           element: <Calibrations /> },
                    { path: "analytics",              element: <FeatureLicenseGate feature={LicenseFeatures.ANALYTICS}><Analytics /></FeatureLicenseGate> },
                    { path: "reports",                element: <FeatureLicenseGate feature={LicenseFeatures.REPORTS}><Reports /></FeatureLicenseGate> },
                    { path: "system",                 element: <System /> },
                    { path: "transporters",           element: <Transporters /> },
                    { path: "weighing/axle-config",   element: <AxleConfigs /> },
                    { path: "weighing/owners",        element: <Owners /> },
                    { path: "weighing/products",      element: <ProductsPortal /> },
                    { path: "suppliers",              element: <Suppliers /> },
                    { path: "saccos",                 element: <SaccosPortal /> },
                    { path: "weighbridges",           element: <WeighbridgesPortal /> },
                    { path: "routes",                 element: <Routes /> },
                    { path: "profile",                element: <Profile /> },
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
                    { path: "weighing/vehicle",         element: <Vehicle /> },
                    { path: "weighing/drivers",         element: <Drivers /> },
                    { path: "automation",               element: <Automation /> },
                    { path: "calibrations",             element: <Calibrations /> },
                    { path: "analytics",                element: <FeatureLicenseGate feature={LicenseFeatures.ANALYTICS}><Analytics /></FeatureLicenseGate> },
                    { path: "reports",                  element: <FeatureLicenseGate feature={LicenseFeatures.REPORTS}><Reports /></FeatureLicenseGate> },
                    { path: "system",                   element: <System /> },
                    { path: "transporters",             element: <Transporters /> },
                    { path: "weighing/axle-config",     element: <AxleConfigs /> },
                    { path: "weighing/owners",          element: <Owners /> },
                    { path: "weighing/products",        element: <ProductsPortal /> },
                    { path: "suppliers",                element: <Suppliers /> },
                    { path: "saccos",                   element: <SaccosPortal /> },
                    { path: "weighbridges",             element: <WeighbridgesPortal /> },
                    { path: "routes",                   element: <Routes /> },
                    // admin-only
                    { path: "user-management",          element: <FeatureLicenseGate feature={LicenseFeatures.USER_MANAGEMENT}><UserManagement /></FeatureLicenseGate> },
                    { path: "security/password-policy", element: <PasswordPolicy /> },
                    { path: "security/permissions",     element: <Permissions /> },
                    { path: "security/roles",           element: <Roles /> },
                    { path: "shifts",                   element: <FeatureLicenseGate feature={LicenseFeatures.SHIFTS}><Shifts /></FeatureLicenseGate> },
                    { path: "attendance",               element: <Attendance /> },
                    { path: "shift-assignment",         element: <ShiftAssignment /> },
                    { path: "backup/microservice",      element: <FeatureLicenseGate feature={LicenseFeatures.BACKUP}><Microservice /></FeatureLicenseGate> },
                    { path: "profile",                  element: <Profile /> },
                ]
            }
        ]
    },

    // ── 404 (must be last) ──
    { path: "*", element: <NotFound /> }
];