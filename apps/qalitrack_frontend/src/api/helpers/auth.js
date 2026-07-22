// src/api/helpers/auth.js
import { apiClient } from './apiClients.js';
import { useEffect, useState } from 'react';

// Session configuration
const SESSION_DURATION = 480 * 60 * 1000; // 480 minutes in milliseconds
const ACTIVITY_TIMEOUT = 15 * 60 * 1000; // 15 minutes of inactivity
const SESSION_CHECK_INTERVAL = 60 * 1000; // Check every minute
// updateSessionActivity() does a synchronous localStorage read+write.
// mousedown/keydown/scroll/touchstart/click fire the activity handler on
// every keystroke — throttle the actual storage write so rapid typing
// doesn't hit localStorage dozens of times per second. The inactivity
// timeout is still reset on every event (that part is just a timer, no I/O).
const ACTIVITY_WRITE_THROTTLE = 5000;
const activityEvents = ['mousedown', 'keydown', 'scroll', 'touchstart', 'click'];

// ─────────────────────────────────────────────────────────────────────────
// Module-level singleton for session/activity monitoring.
//
// useAuth() is called independently by several components that are all
// mounted at once (Sidebar, Topbar, ProtectedRoute, ...). Session monitoring
// is an app-wide concern, not tied to any single component's lifecycle, so
// its state lives here at module scope: exactly one setInterval and one set
// of window listeners exists for the life of the authenticated session, no
// matter how many components call the hook. Previously each instance kept
// its own refs, so N mounted instances meant N window listeners firing (and
// N synchronous localStorage writes) per keystroke/click/scroll anywhere in
// the app — that's what was freezing form inputs.
// ─────────────────────────────────────────────────────────────────────────
let monitoringActive = false;
let sessionCheckInterval = null;
let activityTimeout = null;
let activityHandler = null;
let lastActivityWrite = 0;
let onSessionInvalid = () => {};

const getSession = () => {
    const sessionData = localStorage.getItem('authSession');
    if (!sessionData) return null;

    try {
        const session = JSON.parse(sessionData);
        // Refresh lastActivity on load so the inactivity timer
        // doesn't expire immediately after the app is reopened.
        const now = Date.now();
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

    const now = Date.now();
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

    const now = Date.now();

    if (now > session.expiresAt) return false;
    if (now - session.lastActivity > ACTIVITY_TIMEOUT) return false;

    return true;
};

const stopSessionMonitoring = () => {
    if (sessionCheckInterval) {
        clearInterval(sessionCheckInterval);
        sessionCheckInterval = null;
    }
};

const clearActivityTimeout = () => {
    if (activityTimeout) {
        clearTimeout(activityTimeout);
        activityTimeout = null;
    }
};

const teardownActivityTracking = () => {
    if (activityHandler) {
        activityEvents.forEach(event => {
            window.removeEventListener(event, activityHandler);
        });
        activityHandler = null;
    }
};

const startSessionMonitoring = () => {
    stopSessionMonitoring();
    sessionCheckInterval = setInterval(() => {
        if (!isSessionValid()) onSessionInvalid();
    }, SESSION_CHECK_INTERVAL);
};

const setupActivityTracking = () => {
    teardownActivityTracking();

    activityHandler = () => {
        clearActivityTimeout();

        const now = Date.now();
        if (now - lastActivityWrite >= ACTIVITY_WRITE_THROTTLE) {
            lastActivityWrite = now;
            updateSessionActivity();
        }

        activityTimeout = setTimeout(() => onSessionInvalid(), ACTIVITY_TIMEOUT);
    };

    activityEvents.forEach(event => {
        window.addEventListener(event, activityHandler);
    });

    activityHandler();
};

// Idempotent: safe to call from every mounted instance's effect and from
// createSession() after login — only the first caller (while a valid
// session exists) actually starts the interval/listeners.
const ensureMonitoringStarted = () => {
    if (monitoringActive || !isSessionValid()) return;
    monitoringActive = true;
    startSessionMonitoring();
    setupActivityTracking();
};

const stopMonitoring = () => {
    monitoringActive = false;
    stopSessionMonitoring();
    clearActivityTimeout();
    teardownActivityTracking();
};

const useAuth = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [requiresPasswordChange, setRequiresPasswordChange] = useState(false);
    const [userId, setUserId] = useState('');

    // Keep the global monitoring callback pointed at a live instance's
    // logout(), and make sure monitoring is running if a session already
    // exists (e.g. app reopened with a valid session in localStorage).
    useEffect(() => {
        onSessionInvalid = () => logout();
        ensureMonitoringStarted();
        // No cleanup here: monitoring is an app-wide concern that should
        // keep running for the life of the authenticated session, not stop
        // just because one of several components sharing it unmounts.
        // It's torn down explicitly by logout() -> stopMonitoring().
    }, []);

    // Session Management Functions
    const createSession = (token, userData) => {
        const now = Date.now();
        const session = {
            token,
            userData,
            createdAt: now,
            expiresAt: now + SESSION_DURATION,
            lastActivity: now
        };

        localStorage.setItem('authSession', JSON.stringify(session));
        onSessionInvalid = () => logout();
        ensureMonitoringStarted();
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
        stopMonitoring();
        setRequiresPasswordChange(false);
        setUserId('');
        setError('');
        window.location.hash = '#/login';
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

        const now = Date.now();
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
