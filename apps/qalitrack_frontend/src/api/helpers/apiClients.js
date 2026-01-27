import axios from "axios";

/* -------------------------------------------------------------------------- */
/*                           BASE URL CONFIGURATION                           */
/* -------------------------------------------------------------------------- */

// 🔗 Base URL for Master Data/General API (Points to the /api proxy in vite.config.js)
const API_BASE_URL = import.meta.env.VITE_API_URL || "/api";

// 🔗 Transaction Base URL - Points to /api/Transaction
// The nested /Transaction/Transaction path is handled in Transactions.js
const TRANSACTION_BASE_URL = import.meta.env.VITE_TRANSACTION_API_URL || "/api/Transaction";

// ⚠️ DEBUG: Log configuration on load
console.log('🔧 API Client Configuration:', {
  API_BASE_URL,
  TRANSACTION_BASE_URL,
  MODE: import.meta.env.MODE,
  VITE_API_TARGET: import.meta.env.VITE_API_TARGET,
});

/* -------------------------------------------------------------------------- */
/*                         SESSION MANAGEMENT UTILITY                         */
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
  console.log('🧹 Clearing session data');
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
    console.warn('⏰ Session expired, clearing...');
    clearSession();
    return null;
  }

  return session.token;
};

/* -------------------------------------------------------------------------- */
/*                         LOGGING UTILITY                                    */
/* -------------------------------------------------------------------------- */

const logError = (level, message, data) => {
  if (import.meta.env.MODE !== 'production') {
    console[level](message, data);
  } else {
    // In production, you could send to error tracking service
    // Example: Sentry.captureException(data);
  }
};

const logRequest = (method, url, config) => {
  if (import.meta.env.MODE !== 'production') {
    const logData = {
      baseURL: config?.baseURL,
      fullURL: `${config?.baseURL || ''}${url}`,
      params: config?.params,
      data: config?.data ? '(data present)' : undefined,
      headers: config?.headers ? Object.keys(config.headers) : [],
    };
    
    // Hide sensitive data
    if (config?.data?.password) {
      logData.data = { ...config.data, password: '***' };
    } else if (config?.data) {
      logData.data = config.data;
    }
    
    console.log(`🚀 API Request: ${method.toUpperCase()} ${url}`, logData);
  }
};

/* -------------------------------------------------------------------------- */
/*                         INTERCEPTOR LOGIC                                  */
/* -------------------------------------------------------------------------- */

const setupRequestInterceptor = (client) => {
  client.interceptors.request.use(
    (config) => {
      const token = getSessionToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      // ✅ Ensure proper headers
      if (!config.headers['Content-Type']) {
        config.headers['Content-Type'] = 'application/json';
      }
      config.headers['Accept'] = 'application/json';
      
      // Add request timestamp for performance monitoring
      config.metadata = { startTime: Date.now() };
      
      // Log request in development
      logRequest(config.method, config.url, config);
      
      return config;
    },
    (error) => {
      console.error('❌ Request setup error:', error);
      return Promise.reject(error);
    }
  );
};

const setupResponseInterceptor = (client) => {
  client.interceptors.response.use(
    (response) => {
      // Log response time in development
      if (import.meta.env.MODE !== 'production' && response.config.metadata) {
        const duration = Date.now() - response.config.metadata.startTime;
        console.log(`✅ API Response (${duration}ms): ${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`);
      }
      return response;
    },
    (error) => {
      // Calculate request duration if available
      const duration = error.config?.metadata?.startTime 
        ? Date.now() - error.config.metadata.startTime 
        : 'unknown';
      
      // Log the full error for debugging
      logError('error', `❌ API Error (${duration}ms):`, {
        message: error.message,
        code: error.code,
        status: error.response?.status,
        config: {
          url: error.config?.url,
          baseURL: error.config?.baseURL,
          fullURL: error.config?.baseURL ? `${error.config.baseURL}${error.config.url}` : error.config?.url,
          method: error.config?.method?.toUpperCase(),
          params: error.config?.params,
        },
        response: error.response ? {
          status: error.response.status,
          statusText: error.response.statusText,
          data: error.response.data,
        } : 'No response received',
      });

      // ⚠️ CRITICAL: Better timeout error messaging
      if (error.code === 'ECONNABORTED' && error.message.includes('timeout')) {
        console.error('⏱️ Request Timeout Details:', {
          url: error.config?.url,
          timeout: error.config?.timeout,
          hint: 'The server is not responding. Check if the backend is running and accessible.',
        });
      }

      // ⚠️ CRITICAL: Better 404 error messaging
      if (error.response?.status === 404) {
        console.error('🔍 404 Not Found Details:', {
          requestedURL: error.config?.url,
          baseURL: error.config?.baseURL,
          fullURL: `${error.config?.baseURL || ''}${error.config?.url || ''}`,
          hint: 'The endpoint does not exist. Check backend routes and proxy configuration.',
        });
      }

      // Handle 401 Unauthorized
      if (error.response?.status === 401) {
        console.warn("⚠️ 401 Unauthorized — clearing session");
        clearSession();
        if (window.location.pathname !== "/login") {
          console.log('🔄 Redirecting to login...');
          window.location.href = "/login";
        }
        return Promise.reject(new Error('Session expired. Please log in again.'));
      }

      // Extract error message from response
      let message = 'An error occurred while processing your request';
      
      if (error.response) {
        // The request was made and the server responded with a status code
        // that falls out of the range of 2xx
        const { data, status, statusText } = error.response;
        
        if (data?.message) {
          message = data.message;
        } else if (data?.error) {
          message = data.error;
        } else if (typeof data === 'string') {
          message = data;
        } else if (data?.errors) {
          // Handle validation errors
          if (Array.isArray(data.errors)) {
            message = data.errors.join('; ');
          } else if (typeof data.errors === 'object') {
            message = Object.values(data.errors)
              .flat()
              .join("; ");
          }
        } else if (status === 404) {
          message = `Endpoint not found: ${error.config?.url}. Please check if the backend server is running.`;
        } else if (statusText) {
          message = `${status}: ${statusText}`;
        } else {
          message = `Request failed with status code ${status}`;
        }
      } else if (error.request) {
        // The request was made but no response was received
        if (error.code === 'ECONNABORTED') {
          message = 'Request timeout: The server took too long to respond. Please check if the backend is running.';
        } else if (error.message === 'Network Error') {
          message = 'Network error: Unable to connect to the server. Please check if the backend is running and accessible.';
        } else {
          message = 'No response received from the server. Please verify the backend is running.';
        }
      } else {
        // Something happened in setting up the request that triggered an Error
        message = error.message || 'An unknown error occurred';
      }

      // Create a new error with the enhanced message
      const enhancedError = new Error(message);
      
      // Preserve all the original error information
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
/*                         API CLIENT CLASS                                   */
/* -------------------------------------------------------------------------- */

class ApiClient {
  constructor(baseURL, timeout = 30000) {
    console.log(`🏗️ Creating API Client:`, { baseURL, timeout });
    
    this.client = axios.create({
      baseURL: baseURL,
      timeout: timeout, // 30 seconds default
      headers: { 
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
    });

    this.cancelTokens = new Map();

    setupRequestInterceptor(this.client);
    setupResponseInterceptor(this.client);
  }

  /**
   * Set session token and expiration
   */
  setSession(token, expiresIn = 3600) {
    const session = {
      token,
      expiresAt: Date.now() + expiresIn * 1000, // default 1 hour
    };
    console.log('💾 Setting session:', { expiresAt: new Date(session.expiresAt) });
    sessionStorage.setItem("authSession", JSON.stringify(session));
  }
  
  getSessionToken = getSessionToken;
  clearSession = clearSession;
  isAuthenticated = () => {
    const authenticated = !!getSessionToken();
    console.log('🔐 Authentication check:', authenticated);
    return authenticated;
  };

  /**
   * Create a cancellable request
   * @param {string} key - Unique key for this request
   */
  createCancelToken(key) {
    // Cancel previous request with same key
    if (this.cancelTokens.has(key)) {
      console.log(`🚫 Cancelling previous request: ${key}`);
      this.cancelTokens.get(key).cancel('Request superseded');
    }
    
    const source = axios.CancelToken.source();
    this.cancelTokens.set(key, source);
    return source.token;
  }

  /**
   * Cancel a specific request
   */
  cancelRequest(key) {
    if (this.cancelTokens.has(key)) {
      console.log(`🚫 Cancelling request: ${key}`);
      this.cancelTokens.get(key).cancel('Request cancelled by user');
      this.cancelTokens.delete(key);
    }
  }

  /**
   * GET request
   */
  get(endpoint, options = {}) {
    const { cancelKey, ...axiosOptions } = options;
    
    if (cancelKey) {
      axiosOptions.cancelToken = this.createCancelToken(cancelKey);
    }
    
    return this.client.get(endpoint, axiosOptions);
  }

  /**
   * POST request
   */
  post(endpoint, data, options = {}) {
    const { cancelKey, ...axiosOptions } = options;
    
    if (cancelKey) {
      axiosOptions.cancelToken = this.createCancelToken(cancelKey);
    }
    
    return this.client.post(endpoint, data, axiosOptions);
  }

  /**
   * PUT request
   */
  put(endpoint, data, options = {}) {
    const { cancelKey, ...axiosOptions } = options;
    
    if (cancelKey) {
      axiosOptions.cancelToken = this.createCancelToken(cancelKey);
    }
    
    return this.client.put(endpoint, data, axiosOptions);
  }

  /**
   * DELETE request
   */
  delete(endpoint, options = {}) {
    const { cancelKey, ...axiosOptions } = options;
    
    if (cancelKey) {
      axiosOptions.cancelToken = this.createCancelToken(cancelKey);
    }
    
    return this.client.delete(endpoint, axiosOptions);
  }

  /**
   * PATCH request
   */
  patch(endpoint, data, options = {}) {
    const { cancelKey, ...axiosOptions } = options;
    
    if (cancelKey) {
      axiosOptions.cancelToken = this.createCancelToken(cancelKey);
    }
    
    return this.client.patch(endpoint, data, axiosOptions);
  }
}

/* -------------------------------------------------------------------------- */
/*                         TRANSACTION CLIENT CLASS                           */
/* -------------------------------------------------------------------------- */

class TransactionClient extends ApiClient {
  constructor() {
    // Uses /api/Transaction as base
    // The nested /Transaction path is handled in Transactions.js
    super(TRANSACTION_BASE_URL);
  }
}

/* -------------------------------------------------------------------------- */
/*                         EXPORT GLOBAL INSTANCES                            */
/* -------------------------------------------------------------------------- */

// ✅ Create and export global instances
export const apiClient = new ApiClient(API_BASE_URL);
export const transactionsClient = new TransactionClient();

console.log('✅ API Clients initialized successfully');

export { axios };
export default apiClient;