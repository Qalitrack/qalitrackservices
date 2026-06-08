// src/api/helpers/auth.js
import { apiClient } from './apiClients.js';
import { useState, useEffect, useRef } from 'react';

const useAuth = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [requiresPasswordChange, setRequiresPasswordChange] = useState(false);
    const [userId, setUserId] = useState('');

    const listenerSet = useRef(false);
    const sessionCheckInterval = useRef(null);
    const activityTimeout = useRef(null);

    // Session configuration
    const SESSION_DURATION = 480 * 60 * 1000; // 480 minutes in milliseconds
    const ACTIVITY_TIMEOUT = 15 * 60 * 1000; // 15 minutes of inactivity
    const SESSION_CHECK_INTERVAL = 60 * 1000; // Check every minute

    // Initialize session monitoring
    useEffect(() => {
        if (!listenerSet.current) {
            listenerSet.current = true;
            
            // Start session monitoring if user is authenticated
            if (isAuthenticated()) {
                startSessionMonitoring();
                setupActivityTracking();
            }

            return () => {
                stopSessionMonitoring();
                clearActivityTimeout();
            };
        }
    }, []);

    // Session Management Functions
    const createSession = (token, userData) => {
        const now = new Date().getTime();
        const session = {
            token,
            userData,
            createdAt: now,
            expiresAt: now + SESSION_DURATION,
            lastActivity: now
        };
        
        localStorage.setItem('authSession', JSON.stringify(session));
        startSessionMonitoring();
        setupActivityTracking();
    };

    const getSession = () => {
        const sessionData = localStorage.getItem('authSession');
        if (!sessionData) return null;

        try {
            const session = JSON.parse(sessionData);
            // Refresh lastActivity on load so the inactivity timer
            // doesn't expire immediately after the app is reopened.
            const now = new Date().getTime();
            if (session && now < session.expiresAt) {
                session.lastActivity = now;
                localStorage.setItem('authSession', JSON.stringify(session));
            }
            return session;
        } catch (e) {
            return null;
        }
    };

    const updateSessionActivity = () => {
        const session = getSession();
        if (!session) return;

        const now = new Date().getTime();
        session.lastActivity = now;
        
        // Extend session if user is active
        if (now - session.createdAt < SESSION_DURATION) {
            session.expiresAt = now + SESSION_DURATION;
        }
        
        localStorage.setItem('authSession', JSON.stringify(session));
    };

    const isSessionValid = () => {
        const session = getSession();
        if (!session) return false;

        const now = new Date().getTime();
        
        // Check if session has expired
        if (now > session.expiresAt) {
            return false;
        }

        // Check for inactivity timeout
        if (now - session.lastActivity > ACTIVITY_TIMEOUT) {
            return false;
        }

        return true;
    };

    const startSessionMonitoring = () => {
        // Clear any existing interval
        if (sessionCheckInterval.current) {
            clearInterval(sessionCheckInterval.current);
        }

        // Check session validity periodically
        sessionCheckInterval.current = setInterval(() => {
            if (!isSessionValid()) {
                logout();
            }
        }, SESSION_CHECK_INTERVAL);
    };

    const stopSessionMonitoring = () => {
        if (sessionCheckInterval.current) {
            clearInterval(sessionCheckInterval.current);
            sessionCheckInterval.current = null;
        }
    };

    // Activity tracking
    const setupActivityTracking = () => {
        const resetActivityTimeout = () => {
            clearActivityTimeout();
            updateSessionActivity();
            
            // Set new timeout
            activityTimeout.current = setTimeout(() => {
                logout();
            }, ACTIVITY_TIMEOUT);
        };

        // Track user activity
        const activityEvents = ['mousedown', 'keydown', 'scroll', 'touchstart', 'click'];
        activityEvents.forEach(event => {
            window.addEventListener(event, resetActivityTimeout);
        });

        // Initial activity timeout
        resetActivityTimeout();

        // Store cleanup function
        window.addEventListener('beforeunload', () => {
            activityEvents.forEach(event => {
                window.removeEventListener(event, resetActivityTimeout);
            });
        });
    };

    const clearActivityTimeout = () => {
        if (activityTimeout.current) {
            clearTimeout(activityTimeout.current);
            activityTimeout.current = null;
        }
    };

    const login = async (email, password) => {
        setLoading(true);
        setError('');
        setRequiresPasswordChange(false);

        try {
            const response = await apiClient.post('/Auth/login', {
                email,
                password
            });

            // Server wraps response as: { data: { data: { token, ... }, message }, success, ... }
            const outer = response.data?.data ?? response.data;
            const loginData = outer?.data ?? outer;

            if (loginData?.message === "First login detected") {
                setRequiresPasswordChange(true);
                setUserId(loginData.userId);
                return {
                    success: true,
                    requiresPasswordChange: true,
                    userId: loginData.userId,
                    message: loginData.message
                };
            }

            // Normal login success - create session
            if (loginData) {
                // Support both "token" and "accessToken" field names
                const token = loginData.token || loginData.accessToken || loginData.Token || loginData.AccessToken;
                if (!token) {
                    throw new Error('Token not provided in response');
                }

                const userData = {
                    id: loginData.id || loginData.Id || '',
                    email: loginData.email || loginData.Email || email,
                    firstName: loginData.firstName || loginData.FirstName || '',
                    lastName: loginData.lastName || loginData.LastName || '',
                    userRoles: loginData.userRoles || loginData.UserRoles || []
                };

                createSession(token, userData);
                return { success: true, data: loginData };
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
            setError(errorMessage);
            return { success: false, error: errorMessage };
        }

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
                const userData = {
                    id: responseData.id || userId,
                    email: responseData.email || '',
                    firstName: responseData.firstName || '',
                    lastName: responseData.lastName || '',
                    userRoles: responseData.userRoles || []
                };

                if (responseData.token) {
                    createSession(responseData.token, userData);
                }
            }

            setRequiresPasswordChange(false);
            setUserId('');

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
            const { data } = err.response;
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
            return 'Network error. Please check your connection.';
        } else {
            return err.message || 'An unexpected error occurred';
        }
    };

    const logout = () => {
        localStorage.removeItem('authSession');
        stopSessionMonitoring();
        clearActivityTimeout();
        setRequiresPasswordChange(false);
        setUserId('');
        setError('');
    };

    const isAuthenticated = () => {
        return isSessionValid();
    };

    const getCurrentUser = () => {
        const session = getSession();
        return session ? session.userData : null;
    };

    const getAuthToken = () => {
        const session = getSession();
        return session ? session.token : null;
    };

    const getSessionInfo = () => {
        const session = getSession();
        if (!session) return null;

        const now = new Date().getTime();
        return {
            timeRemaining: Math.max(0, session.expiresAt - now),
            lastActivity: session.lastActivity,
            createdAt: session.createdAt,
            isValid: isSessionValid()
        };
    };

    const clearError = () => {
        setError('');
    };

    const resetPasswordChangeState = () => {
        setRequiresPasswordChange(false);
        setUserId('');
        setError('');
    };

    return {
        login,
        updatePassword,
        logout,
        isAuthenticated,
        getCurrentUser,
        getAuthToken,
        getSessionInfo,
        clearError,
        resetPasswordChangeState,
        loading,
        error,
        requiresPasswordChange,
        userId
    };
};

export default useAuth;