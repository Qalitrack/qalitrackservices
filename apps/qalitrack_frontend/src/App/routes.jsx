// App/routes.jsx - Clean routes configuration with RBAC
import React from 'react';
import { lazy } from "react";
import { Navigate } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoutes.jsx";
import useAuth from '../api/helpers/auth.js';

// Layout
const MainLayout = lazy(() => import("../layouts/MainLayout.jsx"));
const UserServiceLayout = lazy(() => import("../layouts/UserServiceLayout.jsx"));
// Auth
const Login = lazy(() => import("../pages/auth/Login.jsx"));

// Admin Components

// Operator/Shared Components
const Dashboard = lazy(() => import("../pages/weighing/Dashboard.jsx"));
const FactoryWeighing = lazy(() => import("../pages/weighing/Weighing.jsx"));
const Automation = lazy(() => import("../pages/weighing/Automation.jsx"));
const Calibrations = lazy(() => import("../pages/weighing/Calibrations.jsx"));
const Analytics = lazy(() => import("../pages/weighing/Analytics.jsx"));
const Reports = lazy(() => import("../pages/weighing/Reports.jsx"));
const System = lazy(() => import("../pages/weighing/System.jsx"));
const Vehicle = lazy(() => import("../components/weighing/Vehicles.jsx"));
const Drivers = lazy(() => import("../components/weighing/Drivers.jsx"));
const AdminDashboard = lazy(() => import("../pages/Userservice/AdminDashboard.jsx"));
const PasswordPolicy = lazy(() => import("../pages/Userservice/PasswordPolicy.jsx"));
const Permissions = lazy(() => import("../pages/Userservice/Permissions.jsx"));
const Roles = lazy(() => import("../pages/Userservice/Roles.jsx"));
const Shifts = lazy(() => import("../pages/Userservice/Shifts.jsx"));
const ShiftAssignment = lazy(() => import("../pages/Userservice/ShiftAssignment.jsx"));
const Attendance = lazy(() => import("../pages/Userservice/Attendance.jsx"));
const Microservice = lazy(() => import("../pages/Userservice/Backup/Microservice.jsx"));
const Transporters = lazy(() => import("../pages/weighing/TransporterFormModal.jsx"));
const Suppliers = lazy(() => import("../pages/Suppliers.jsx"));
const Routes = lazy(() => import("../pages/Routes.jsx"));
const AxleConfigs = lazy(() => import("../pages/weighing/AxleConfigs.jsx"));
const Owners = lazy(() => import("../pages/weighing/Owners.jsx"));
const ProductsPortal = lazy(() => import("../components/weighing/Product.jsx"));
const SaccosPortal = lazy(() => import("../pages/Saccos.jsx"));
const WeighbridgesPortal = lazy(() => import("../pages/weighing/WeighingBridge.jsx"));
// Root redirect component that handles authenticated users
const RootRedirect = () => {
    const { isAuthenticated, getCurrentUser } = useAuth();

    if (!isAuthenticated()) {
        return <Navigate to="/login" replace />;
    }

    const user = getCurrentUser();
    const primaryRole = user?.userRoles?.[0];

    if (primaryRole === 'Admin') {
        return <Navigate to="/admin" replace />;
    } else if (primaryRole === 'Operator') {
        return <Navigate to="/operator" replace />;
    }

    return <Navigate to="/login" replace />;
};

// 404 Component
const NotFound = () => (
    <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
            <h1 className="text-4xl font-bold text-gray-700 mb-4">404</h1>
            <p className="text-gray-500 mb-4">Page Not Found</p>
            <button
                onClick={() => window.history.back()}
                className="px-4 py-2 bg-amber-500 text-white rounded-lg hover:bg-amber-600"
            >
                Go Back
            </button>
        </div>
    </div>
);

// Routes configuration
export const routes = [
    // Public routes
    {
        path: "/login",
        element: <Login />
    },

    // Root redirect
    {
        path: "/login",
        element: <RootRedirect />
    },

    // Admin routes - RBAC protected
    {
        path: "",
        element: <ProtectedRoute allowedRoles={["Operator"]} />,
        children: [
            {
                path: "/Operator",
                element: <MainLayout />,
                children: [
                    {
                        path: "dashboard",
                        element: <Dashboard />
                    },
                    {
                        path: "weighing/factory",
                        element: <FactoryWeighing />
                    },
                    {
                        path: "weighing/vehicle",
                        element: <Vehicle />
                    },
                    {
                        path: "weighing/drivers",
                        element: <Drivers />
                    },
                    {
                        path: "automation",
                        element: <Automation />
                    },
                    {
                        path: "calibrations",
                        element: <Calibrations />
                    },
                    {
                        path: "analytics",
                        element: <Analytics />
                    },
                    {
                        path: "reports",
                        element: <Reports />
                    },
                    {
                        path: "system",
                        element: <System />
                    },
                    {
                        path: "transporters",
                        element: <Transporters />,
                    },
                    {
                        path: "weighing/axle-config",
                        element: <AxleConfigs />,
                    },
                    {
                        path: "weighing/owners",
                        element: <Owners />,
                    },
                    {
                        path: "weighing/products",
                        element: <ProductsPortal />,

                    },
                    {
                        path: "suppliers",
                        element: <Suppliers />,
                    },
                    {
                        path: "saccos",
                        element: <SaccosPortal />,
                    },
                    {
                        path: "weighbridges",
                        element: <WeighbridgesPortal />,
                    },
                    {
                        path: "routes",
                        element: <Routes />
                    }
                ]
            }
        ]
    },

    // Operator routes - RBAC protected (now matching admin path structure)
    {
        path: "",
        element: <ProtectedRoute allowedRoles={["Admin"]} />,
        children: [
            {
                path: "/Admin",
                element: <UserServiceLayout />,
                children: [
                    {
                        path: "dashboard",
                        element: <AdminDashboard />
                    },
                    {
                        path: "security/password-policy",
                        element: <PasswordPolicy />
                    },
                    {
                        path: "security/permissions",
                        element: <Permissions />
                    },
                    {
                        path: "security/roles",
                        element: <Roles />
                    },
                    {
                        path:"shifts",
                        element: <Shifts />
                    },
                    {
                        path:"attendance",
                        element: <Attendance />
                    },
                    {
                        path: "shift-assignment",
                        element: <ShiftAssignment />
                    },
                    {
                        path: "backup/microservice",
                        element: <Microservice />
                    }
                ]
            }
        ]
    },

    // Shared routes - RBAC protected
    {
        path: "",
        element: <ProtectedRoute allowedRoles={["Admin", "Operator"]} />,
        children: [
            {
                path: "/shared",
                element: <MainLayout />,
                children: [
                    {
                        path: "reports",
                        element: <Reports />
                    },
                    {
                        path: "analytics",
                        element: <Analytics />
                    }
                ]
            }
        ]
    },

    // 404 fallback
    {
        path: "*",
        element: <NotFound />
    }
];