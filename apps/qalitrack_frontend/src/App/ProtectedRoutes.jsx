import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import useAuth from '../api/helpers/auth';

// Shown in place of the app (not a redirect) when an authenticated account has
// no role assigned yet — e.g. freshly created, before an admin assigns one.
// Fails closed: no role means no access to anything, not a fallback page.
const NoRoleAssigned = () => {
    const { logout } = useAuth();
    return (
        <div className="h-screen w-screen flex items-center justify-center bg-gray-50">
            <div className="max-w-sm w-full bg-white rounded-lg shadow-md border border-gray-200 p-6 text-center">
                <div className="w-12 h-12 mx-auto mb-3 rounded-full bg-red-100 flex items-center justify-center">
                    <span className="text-red-600 text-2xl font-bold">!</span>
                </div>
                <h2 className="text-base font-bold text-gray-900 mb-1">No Role Assigned</h2>
                <p className="text-sm text-gray-600 mb-4">
                    Your account doesn't have a role yet, so there's nothing to show you.
                    Contact your administrator to have one assigned.
                </p>
                <button
                    onClick={logout}
                    className="w-full h-9 text-sm font-semibold bg-amber-500 hover:bg-amber-600 text-white rounded shadow-sm transition-all"
                >
                    Log Out
                </button>
            </div>
        </div>
    );
};

const ProtectedRoute = ({ allowedRoles = [] }) => {
    const location = useLocation();
    const { isAuthenticated, getCurrentUser } = useAuth();

    // ✅ CRITICAL: Allow kiosk routes to bypass authentication
    // This MUST be checked BEFORE any authentication checks
    if (location.pathname.startsWith('/kiosk')) {
        return <Outlet />;
    }

    const user = getCurrentUser();
    const isAuth = isAuthenticated();

    // If not authenticated, redirect to login
    if (!isAuth) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    const userRoles = user?.userRoles || [];

    // Fail closed: no role means no access to anything, not a fallback page.
    // Without this, an unassigned user would fall through every role-specific
    // check (each one legitimately fails on an empty array) straight into
    // whatever routes happen to have no role restriction at all.
    if (userRoles.length === 0) {
        return <NoRoleAssigned />;
    }

    // Operator is a floor/master-data role — weighing plus the master-data
    // screens (vehicles, drivers, transporters, owners, suppliers, products,
    // axle config, saccos), nothing else (no Dashboard, Analytics, Reports,
    // Shifts, User Management, Security, System, Backup). Checked here (not
    // just hidden in the sidebar) so this is real access control, not a UI nicety.
    const operatorAllowedPaths = [
        '/operator/weighing/factory',
        '/operator/transactions',
        '/operator/weighing/vehicle',
        '/operator/weighing/drivers',
        '/operator/transporters',
        '/operator/weighing/owners',
        '/operator/suppliers',
        '/operator/weighing/products',
        '/operator/weighing/axle-config',
        '/operator/saccos',
        '/operator/profile',
    ];
    const isOperatorOnly = userRoles.includes('Operator') && !userRoles.includes('Admin');
    if (isOperatorOnly && !operatorAllowedPaths.some((p) => location.pathname.startsWith(p))) {
        return <Navigate to="/operator/weighing/factory" replace />;
    }

    // Check if user has any of the allowed roles
    const hasAccess = allowedRoles.length === 0 || allowedRoles.some((role) => userRoles.includes(role));

    if (!hasAccess) {
        // Redirect based on user's primary role to their default landing page
        const primaryRole = userRoles[0];

        if (primaryRole === 'Admin') {
            return <Navigate to="/admin/dashboard" replace />;
        } else {
            // Operator and all other authenticated roles land on factory weighing
            return <Navigate to="/operator/weighing/factory" replace />;
        }
    }

    // User has access, render the protected content
    return <Outlet />;
};

export default ProtectedRoute;