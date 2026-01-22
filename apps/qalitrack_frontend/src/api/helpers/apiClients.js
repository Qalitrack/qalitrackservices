// src/api/helpers/apiClients.js
import axios from "axios";

/* -------------------------------------------------------------------------- */
/*                           BASE URL CONFIGURATION                           */
/* -------------------------------------------------------------------------- */
// ✅ CRITICAL: Leave these EMPTY so Vite proxy handles routing
// During development: requests go to http://localhost:5173/Transaction
// Vite proxy forwards to: https://qalitrack.cseco.co.ke/api/Transaction

const API_BASE_URL = "";  // Empty string = use Vite proxy
const TRANSACTION_BASE_URL = "";  // Empty string = use Vite proxy

/* -------------------------------------------------------------------------- */
/*                           SESSION MANAGEMENT                               */
/* -------------------------------------------------------------------------- */

const getSessionData = () => {
  try {
    const sessionData = sessionStorage.getItem("authSession");
    return sessionData ? JSON.parse(sessionData) : null;
  } catch (err) {
    console.error("❌ Error retrieving session token:", err);
    return null;
  }
};

const clearSession = () => {
  sessionStorage.removeItem("authSession");
  sessionStorage.removeItem("temp2FASession");
  localStorage.removeItem("authToken");
  localStorage.removeItem("user");
};

const getSessionToken = () => {
  const session = getSessionData();
  if (!session) return null;

  const now = Date.now();
  if (now > session.expiresAt) {
    clearSession();
    return null;
  }

  return session.token;
};

/* -------------------------------------------------------------------------- */
/*                           INTERCEPTOR SETUP                                */
/* -------------------------------------------------------------------------- */

const setupRequestInterceptor = (client) => {
  client.interceptors.request.use(
    (config) => {
      const token = getSessionToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      // Enhanced logging for debugging
      const fullUrl = `${config.baseURL || ''}${config.url}`;
      console.log(`🔵 API Request: ${config.method?.toUpperCase()} ${fullUrl}`, {
        params: config.params,
        data: config.data,
      });
      
      return config;
    },
    (error) => {
      console.error('❌ Request Interceptor Error:', error);
      return Promise.reject(error);
    }
  );
};

const setupResponseInterceptor = (client) => {
  client.interceptors.response.use(
    (response) => {
      const fullUrl = `${response.config.baseURL || ''}${response.config.url}`;
      console.log(`✅ API Response: ${response.status} ${fullUrl}`, {
        data: response.data,
      });
      return response;
    },
    (error) => {
      // Comprehensive error logging
      const errorDetails = {
        message: error.message,
        code: error.code,
        url: error.config?.url,
        method: error.config?.method,
        baseURL: error.config?.baseURL,
        params: error.config?.params,
        status: error.response?.status,
        statusText: error.response?.statusText,
        responseData: error.response?.data,
      };
      
      console.error('❌ API Error Details:', errorDetails);

      // Handle 401 Unauthorized
      if (error.response?.status === 401) {
        console.warn("⚠️ Authentication failed — clearing session");
        clearSession();
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }
        return Promise.reject(new Error('Session expired. Please log in again.'));
      }

      // Extract meaningful error message
      let message = 'An error occurred while processing your request';
      
      if (error.response) {
        const { data, status, statusText } = error.response;
        
        // Check if we got HTML instead of JSON (common proxy error)
        if (typeof data === 'string' && data.includes('<!DOCTYPE html>')) {
          message = `Server returned HTML instead of JSON. Check proxy configuration for ${error.config?.url}`;
          console.error('⚠️ PROXY ERROR: Received HTML page instead of API response');
        } else if (data?.message) {
          message = data.message;
        } else if (typeof data === 'string') {
          message = data;
        } else if (data?.errors) {
          message = Object.values(data.errors).flat().join("; ");
        } else if (statusText) {
          message = `${status}: ${statusText}`;
        } else {
          message = `Request failed with status code ${status}`;
        }
      } else if (error.request) {
        if (error.code === 'ECONNABORTED') {
          message = 'Request timeout: The server took too long to respond.';
        } else if (error.message === 'Network Error') {
          message = 'Network error: Unable to connect to the server. Please check your internet connection.';
        } else {
          message = 'No response received from the server. Please try again later.';
        }
      } else {
        message = error.message || 'An unknown error occurred';
      }

      // Create enhanced error
      const enhancedError = new Error(message);
      enhancedError.name = error.name || 'ApiError';
      enhancedError.code = error.code;
      enhancedError.status = error.response?.status;
      enhancedError.response = error.response;
      enhancedError.request = error.request;
      enhancedError.config = error.config;
      enhancedError.originalError = error;
      
      return Promise.reject(enhancedError);
    }
  );
};

/* -------------------------------------------------------------------------- */
/*                           API CLIENT CLASS                                 */
/* -------------------------------------------------------------------------- */

class ApiClient {
  constructor(baseURL) {
    this.client = axios.create({
      baseURL: baseURL,
      headers: { 
        "Content-Type": "application/json",
        "Accept": "application/json",
      },
      timeout: 30000, // 30 second timeout
    });

    setupRequestInterceptor(this.client);
    setupResponseInterceptor(this.client);
  }

  setSession(token, expiresIn = 3600) {
    const session = {
      token,
      expiresAt: Date.now() + expiresIn * 1000,
    };
    sessionStorage.setItem("authSession", JSON.stringify(session));
  }
  
  getSessionToken = getSessionToken;
  clearSession = clearSession;
  isAuthenticated = () => !!getSessionToken();

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

  patch(endpoint, data, options = {}) {
    return this.client.patch(endpoint, data, options);
  }
}

/* -------------------------------------------------------------------------- */
/*                       TRANSACTION CLIENT CLASS                             */
/* -------------------------------------------------------------------------- */

class TransactionClient extends ApiClient {
  constructor() {
    super(TRANSACTION_BASE_URL);
  }
}

/* -------------------------------------------------------------------------- */
/*                           EXPORT INSTANCES                                 */
/* -------------------------------------------------------------------------- */

// ✅ Main API client for MasterData, Users, etc.
export const apiClient = new ApiClient(API_BASE_URL);

// ✅ Transaction-specific client (uses same base, both go through Vite proxy)
export const transactionsClient = new TransactionClient();

// Additional exports
export { axios };
export default apiClient;

/* -------------------------------------------------------------------------- */
/*                           HELPER FUNCTIONS                                 */
/* -------------------------------------------------------------------------- */

// Export session helpers for use in other modules
export const sessionHelpers = {
  getSessionToken,
  clearSession,
  getSessionData,
};