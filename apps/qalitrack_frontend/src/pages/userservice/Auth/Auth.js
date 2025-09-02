import React, { useState } from 'react';
import { apiClient } from './Client.js'

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

            console.log('Login successful:', response);

            // Store token in localStorage or wherever you manage auth state
            if (response.data && response.data.token) {
                localStorage.setItem('authToken', response.data.token);
                localStorage.setItem('user', JSON.stringify(response.data.user));
            }

            return { success: true, data: response.data };

        } catch (err) {
            const errorMessage = err.response?.data?.message || err.message || 'Login failed';
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