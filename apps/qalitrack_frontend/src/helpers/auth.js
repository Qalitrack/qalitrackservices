import React, { useState, useEffect, useRef } from 'react';
import { apiClient } from './apiClients.js';
import axios from "axios";

const useAuth = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [sessionId, setSessionId] = useState('');
    const [maskedEmail, setMaskedEmail] = useState('');
    const [requires2FA, setRequires2FA] = useState(false);
    const [requiresPasswordChange, setRequiresPasswordChange] = useState(false);
    const [userId, setUserId] = useState('');

    const listenerSet = useRef(false);

    useEffect(() => {
        if (!listenerSet.current) {
            listenerSet.current = true;
            const handleBeforeUnload = () => {
                logout();
            };
            window.addEventListener('beforeunload', handleBeforeUnload);
            return () => {
                window.removeEventListener('beforeunload', handleBeforeUnload);
            };
        }
    }, []);

    const login = async (email, password) => {
        setLoading(true);
        setError('');
        setRequires2FA(false);
        setRequiresPasswordChange(false);

        try {
            const response = await apiClient.post('/Auth/login', {
                email,
                password
            });

            const responseData = response.data?.data || response.data;

            if (responseData?.message === "First login detected") {
                setRequiresPasswordChange(true);
                setUserId(responseData.userId);
                return {
                    success: true,
                    requiresPasswordChange: true,
                    userId: responseData.userId,
                    message: responseData.message
                };
            }

            // Check if 2FA is required
            if (responseData?.requires2FA) {
                setRequires2FA(true);
                setSessionId(responseData.sessionId);
                setMaskedEmail(responseData.email);
                // Temporarily store session details in sessionStorage for persistence across reloads during 2FA flow
                sessionStorage.setItem('tempSessionId', responseData.sessionId);
                sessionStorage.setItem('tempMaskedEmail', responseData.email);
                return {
                    success: true,
                    requires2FA: true,
                    sessionId: responseData.sessionId,
                    maskedEmail: responseData.email,
                    message: responseData.message
                };
            }

            // Normal login success - store token and user data in localStorage
            if (responseData) {
                if (!responseData.token) {
                    throw new Error('Token not provided in response');
                }
                localStorage.setItem('authToken', responseData.token);

                // Store user info (handle cases where some fields might be missing)
                const userData = {
                    id: responseData.id || '',
                    email: responseData.email || email,
                    firstName: responseData.firstName || '',
                    lastName: responseData.lastName || '',
                    userRoles: responseData.userRoles || []
                };
                localStorage.setItem('user', JSON.stringify(userData));
                return { success: true, data: responseData };
            }

            throw new Error('Invalid response data');

        } catch (err) {
            const errorMessage = extractErrorMessage(err);
            setError(errorMessage);
            return { success: false, error: errorMessage };
        } finally {
            setLoading(false);
        }
    };

    const updatePassword = async (currentPassword, newPassword, confirmPassword) => {
        if (!userId) {
            const errorMessage = 'No active session. Please login again.';
            console.error('No userId:', errorMessage); // Debug log
            setError(errorMessage);
            return { success: false, error: errorMessage };
        }

        console.log('Sending updatePassword request:', { userId, currentPassword, newPassword, confirmPassword }); // Debug log
        setLoading(true);
        setError('');

        try {
            const response = await apiClient.put(`/Auth/update-password/${userId}`, {
                currentPassword,
                newPassword,
                confirmPassword
            }, {
                headers: {
                    'accept': '*/*',
                    'Content-Type': 'application/json',
                    'Authorization': undefined
                }
            });


            const responseData = response.data?.data || response.data;

            if (responseData) {
                if (responseData.token) {
                    localStorage.setItem('authToken', responseData.token);
                }

                const userData = {
                    id: responseData.id || userId,
                    email: responseData.email || '',
                    firstName: responseData.firstName || '',
                    lastName: responseData.lastName || '',
                    userRoles: responseData.userRoles || []
                };
                localStorage.setItem('user', JSON.stringify(userData));
            }

            setRequiresPasswordChange(false);
            setUserId('');

            return { success: true, data: responseData };

        } catch (err) {
            const errorMessage = extractErrorMessage(err);
            console.error('Update password error:', err, 'Message:', errorMessage); // Debug log
            setError(errorMessage);
            return { success: false, error: errorMessage };
        } finally {
            setLoading(false);
        }
    };

    const verify2FA = async (code) => {
        // Check for persisted temp session from reload
        if (!sessionId) {
            const tempSessionId = sessionStorage.getItem('tempSessionId');
            const tempMaskedEmail = sessionStorage.getItem('tempMaskedEmail');
            if (tempSessionId) {
                setSessionId(tempSessionId);
                setMaskedEmail(tempMaskedEmail || '');
            } else {
                const errorMessage = 'No active session. Please login again.';
                setError(errorMessage);
                return { success: false, error: errorMessage };
            }
        }

        setLoading(true);
        setError('');

        try {
            const response = await apiClient.post('/Auth/verify-2fa', {
                sessionId,
                code
            });

            // The response data is nested under response.data.data
            const responseData = response.data?.data || response.data;
            
            // Extract the actual user data from the response
            const userData = responseData?.data || responseData;

            // Store token and user data after successful 2FA in localStorage
            if (userData) {
                if (!userData.token) {
                    throw new Error('Token not provided in response');
                }
                localStorage.setItem('authToken', userData.token);

                // Store user info
                const userInfo = {
                    id: userData.id || '',
                    email: userData.email || maskedEmail,
                    firstName: userData.firstName || '',
                    lastName: userData.lastName || '',
                    userRoles: userData.userRoles || []
                };
                localStorage.setItem('user', JSON.stringify(userInfo));
            }

            // Clear 2FA state and temp storage
            setRequires2FA(false);
            setSessionId('');
            setMaskedEmail('');
            sessionStorage.removeItem('tempSessionId');
            sessionStorage.removeItem('tempMaskedEmail');

            return { success: true, data: responseData };

        } catch (err) {
            const errorMessage = extractErrorMessage(err);
            setError(errorMessage);
            return { success: false, error: errorMessage };
        } finally {
            setLoading(false);
        }
    };

    const extractErrorMessage = (err) => {
        if (err.response) {
            // Server responded with error status
            const { data } = err.response;

            // Handle nested data structure
            const responseData = data?.data || data;

            if (responseData?.message) {
                return responseData.message;
            } else if (responseData?.error) {
                return responseData.error;
            } else if (responseData?.errors && Array.isArray(responseData.errors) && responseData.errors.length > 0) {
                return responseData.errors[0];
            } else if (typeof data === 'string') {
                return data;
            } else {
                return `Server error: ${err.response.status}`;
            }
        } else if (err.request) {
            // Request was made but no response received
            return 'Network error. Please check your connection.';
        } else {
            // Something else happened
            return err.message || 'An unexpected error occurred';
        }
    };

    const logout = () => {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        // Also clear any temp 2FA state
        sessionStorage.removeItem('tempSessionId');
        sessionStorage.removeItem('tempMaskedEmail');
        setRequires2FA(false);
        setRequiresPasswordChange(false);
        setSessionId('');
        setMaskedEmail('');
        setUserId('');
        setError('');
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

    const reset2FAState = () => {
        setRequires2FA(false);
        setSessionId('');
        setMaskedEmail('');
        sessionStorage.removeItem('tempSessionId');
        sessionStorage.removeItem('tempMaskedEmail');
        setError('');
    };

    const resetPasswordChangeState = () => {
        setRequiresPasswordChange(false);
        setUserId('');
        setError('');
    };

    return {
        login,
        verify2FA,
        updatePassword,
        logout,
        isAuthenticated,
        getCurrentUser,
        clearError,
        reset2FAState,
        resetPasswordChangeState,
        loading,
        error,
        requires2FA,
        requiresPasswordChange,
        maskedEmail,
        userId
    };
};

export default useAuth;