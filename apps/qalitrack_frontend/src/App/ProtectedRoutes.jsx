import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import useAuth from '../helpers/auth'; // Adjust path as needed

const ProtectedRoute = ({ allowedRoles = [] }) => {
    const { isAuthenticated, getCurrentUser } = useAuth();
    const location = useLocation();
    const user = getCurrentUser();
    const isAuth = isAuthenticated();

    // If not authenticated, redirect to login
    if (!isAuth) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    const userRoles = user?.userRoles || [];

    // Check if user has any of the allowed roles
    const hasAccess = allowedRoles.length === 0 || allowedRoles.some((role) => userRoles.includes(role));

    if (!hasAccess) {
        // Redirect based on user's primary role
        const primaryRole = userRoles[0];

        if (primaryRole === 'Admin') {
            return <Navigate to="/admin" replace />;
        } else if (primaryRole === 'Operator') {
            return <Navigate to="/operator" replace />;
        } else {
            // If no valid role, logout and redirect to login
            return <Navigate to="/login" replace />;
        }
    }

    // User has access, render the protected content
    return <Outlet />;
};

export default ProtectedRoute;