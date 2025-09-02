import React, { useState } from 'react';
import { apiClient } from './Client';

// Example usage in your Login component:
// import useAuth from './useAuth';
//
// const { login, loading, error, clearError } = useAuth();
//
// const handleSubmit = async (e) => {
//     e.preventDefault();
//     const result = await login(email, password);
//     if (result.success) {
//         // Redirect or update UI
//         window.location.href = '/dashboard';
//     }
// };

const useAuth = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');

    const login = async (email, password) => {
        setLoading(true);
        setError('');

        try {
            const response = await apiClient.post('/Auth/login', {
                email,
                password
            });

            // Store token and user data based on actual server response structure
            if (response.data) {
                // Store the entire response data or specific fields as needed
                localStorage.setItem('authToken', response.data.token || response.data.accessToken);

                // Store user info if it exists in the response
                if (response.data.user) {
                    localStorage.setItem('user', JSON.stringify(response.data.user));
                }
            }

            return { success: true, data: response.data };

        } catch (err) {
            let errorMessage = 'Login failed';

            if (err.response) {
                // Server responded with error status (axios)
                const { status, data } = err.response;

                if (status === 401 && data && data.message) {
                    // Your server returns: { "message": "Invalid email or password" }
                    errorMessage = data.message;
                } else if (data && typeof data === 'object') {
                    errorMessage = data.message ||
                        data.error ||
                        data.details ||
                        `Server error: ${status}`;
                } else if (typeof data === 'string') {
                    errorMessage = data;
                } else {
                    errorMessage = `Server error: ${status}`;
                }
            } else if (err.request) {
                // Request was made but no response received (network error)
                errorMessage = 'Network error. Please check your connection.';
            } else {
                // Something else happened
                errorMessage = err.message || 'An unexpected error occurred';
            }

            setError(errorMessage);
            return { success: false, error: errorMessage };
        } finally {
            setLoading(false);
        }
    };

    const logout = () => {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        // Clear any other auth-related data
    };

    const isAuthenticated = () => {
        return !!localStorage.getItem('authToken');
    };

    const getCurrentUser = () => {
        const user = localStorage.getItem('user');
        return user ? JSON.parse(user) : null;
    };

    const clearError = () => {
        setError('');
    };

    return {
        login,
        logout,
        isAuthenticated,
        getCurrentUser,
        clearError,
        loading,
        error
    };
};

export default useAuth;