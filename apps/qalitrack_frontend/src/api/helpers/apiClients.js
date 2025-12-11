// src/helpers/apiClients.js
import axios from "axios";

// 🔗 Base URL for Master Data/General API (Uses external URL, Vite proxy handles it via the /api path)
const API_BASE_URL = import.meta.env.VITE_API_URL || "https://qalitrack.cseco.co.ke/api";
// 🔗 Dedicated Transaction Base URL (Uses local proxy path configured in vite.config.js)
const TRANSACTION_BASE_URL =import.meta.env.VITE_TRANSACTION_API_URL || "https://qalitrack.cseco.co.ke/";

// --- Session Management Utility ---

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


// --- Interceptor Logic ---

const setupRequestInterceptor = (client) => {
  client.interceptors.request.use(
    (config) => {
      const token = getSessionToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );
};

const setupResponseInterceptor = (client) => {
  client.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        console.warn("⚠️ Authentication failed — clearing session");
        clearSession();
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }
      }

      let message =
        error.response?.data?.message ||
        (typeof error.response?.data === "string"
          ? error.response.data
          : error.message);

      if (error.response?.data?.errors) {
        message = Object.values(error.response.data.errors)
          .flat()
          .join("; ");
      }

      const err = new Error(message);
      err.originalError = error;
      return Promise.reject(err);
    }
  );
};


// --- API Client Class for Master Data/General (using API_BASE_URL) ---
class ApiClient {
  constructor(baseURL) {
    this.client = axios.create({
      baseURL: baseURL,
      headers: { "Content-Type": "application/json" },
    });

    setupRequestInterceptor(this.client);
    setupResponseInterceptor(this.client);
  }

  setSession(token, expiresIn = 3600) {
    const session = {
      token,
      expiresAt: Date.now() + expiresIn * 1000, // default 1 hour
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


// --- Transaction Client Class (using TRANSACTION_BASE_URL) ---
class TransactionClient extends ApiClient {
    constructor() {
        // This super call uses the local path '/Transaction'
        super(TRANSACTION_BASE_URL);
    }
}


// ✅ Create and export global instances
export const apiClient = new ApiClient(API_BASE_URL);
export const transactionsClient = new TransactionClient();

export { axios }; 
export default apiClient;