import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import useAuth from '../api/helpers/auth';

const ProtectedRoute = ({ allowedRoles = [] }) => {
    const location = useLocation();
    const { isAuthenticated, getCurrentUser } = useAuth();

    // ✅ CRITICAL: Allow kiosk routes to bypass authentication
    // This MUST be checked BEFORE any authentication checks
    if (location.pathname.startsWith('/kiosk')) {
        console.log('✅ Kiosk route detected - bypassing authentication');
        return <Outlet />;
    }

    const user = getCurrentUser();
    const isAuth = isAuthenticated();

    // If not authenticated, redirect to login
    if (!isAuth) {
        console.log('❌ Not authenticated - redirecting to login');
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    const userRoles = user?.userRoles || [];

    // Check if user has any of the allowed roles
    const hasAccess = allowedRoles.length === 0 || allowedRoles.some((role) => userRoles.includes(role));

    if (!hasAccess) {
        console.log('❌ Access denied - insufficient permissions');
        // Redirect based on user's primary role to their default landing page
        const primaryRole = userRoles[0];

        if (primaryRole === 'Admin') {
            return <Navigate to="/admin/dashboard" replace />;
        } else if (primaryRole === 'Operator') {
            return <Navigate to="/operator/weighing/factory" replace />;
        } else {
            // If no valid role, logout and redirect to login
            return <Navigate to="/login" replace />;
        }
    }

    // User has access, render the protected content
    return <Outlet />;
};

export default ProtectedRoute;