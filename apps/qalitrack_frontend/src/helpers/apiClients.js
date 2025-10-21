import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL;

class ApiClient {
    constructor() {
        this.client = axios.create({
            baseURL: API_BASE_URL,
            headers: {
                'Content-Type': 'application/json',
            },
        });

        // Request interceptor to add auth token from session
        this.client.interceptors.request.use(
            (config) => {
                const token = this.getSessionToken();
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => {
                return Promise.reject(error);
            }
        );

        // Response interceptor for global error handling
        this.client.interceptors.response.use(
            (response) => {
                return response;
            },
            (error) => {
                // Handle 401 Unauthorized - clear session and redirect
                if (error.response?.status === 401) {
                    console.log('Authentication failed - clearing session');
                    this.clearSession();
                    
                    // Redirect to login if not already there
                    if (window.location.pathname !== '/login') {
                        window.location.href = '/login';
                    }
                }

                // Extract error message
                const serverMessage = error.response?.data?.message;
                const serverErrors = error.response?.data?.errors;
                let errorMessage = error.message;

                if (serverMessage) {
                    errorMessage = serverMessage;
                } else if (serverErrors && typeof serverErrors === 'object') {
                    errorMessage = Object.values(serverErrors).flat().join('; ');
                } else if (error.response?.data && typeof error.response.data === 'string') {
                    errorMessage = error.response.data;
                }

                const customError = new Error(errorMessage);
                customError.originalError = error;
                return Promise.reject(customError);
            }
        );
    }

    /**
     * Get authentication token from session storage
     * @returns {string|null} JWT token or null
     */
    getSessionToken() {
        try {
            const sessionData = sessionStorage.getItem('authSession');
            if (!sessionData) return null;

            const session = JSON.parse(sessionData);

            // Check if session has expired
            const now = new Date().getTime();
            if (now > session.expiresAt) {
                console.log('Session expired');
                this.clearSession();
                return null;
            }

            return session.token;
        } catch (error) {
            console.error('Error retrieving session token:', error);
            return null;
        }
    }

    /**
     * Clear session data
     */
    clearSession() {
        sessionStorage.removeItem('authSession');
        sessionStorage.removeItem('temp2FASession');
        
        // Also clear any old localStorage tokens (migration cleanup)
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
    }

    /**
     * Check if user is authenticated
     * @returns {boolean}
     */
    isAuthenticated() {
        const token = this.getSessionToken();
        return !!token;
    }

    get(endpoint, options = {}) {
        return this.client.get(endpoint, options);
    }

    post(endpoint, data, options = {}) {
        return this.client.post(endpoint, data, options);
    }

    put(endpoint, data, options = {}) {
        return this.client.put(endpoint, data, options);
    }

    delete(endpoint, options = {}) {
        return this.client.delete(endpoint, options);
    }

    // Patch method (common in REST APIs)
    patch(endpoint, data, options = {}) {
        return this.client.patch(endpoint, data, options);
    }
}

export const apiClient = new ApiClient();