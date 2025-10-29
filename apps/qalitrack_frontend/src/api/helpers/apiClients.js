// src/helpers/apiClients.js
import axios from "axios";

// 🔗 Base URL (from .env file or default fallback)
const API_BASE_URL = import.meta.env.VITE_API_URL || "https://qalitrack.cseco.co.ke/api";

class ApiClient {
  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: { "Content-Type": "application/json" },
    });

    // ✅ Add token to every request
    this.client.interceptors.request.use(
      (config) => {
        const token = this.getSessionToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // ✅ Handle errors globally
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          console.warn("⚠️ Authentication failed — clearing session");
          this.clearSession();
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
  }

  // ✅ Store token + expiry in sessionStorage
  setSession(token, expiresIn = 3600) {
    const session = {
      token,
      expiresAt: Date.now() + expiresIn * 1000, // default 1 hour
    };
    sessionStorage.setItem("authSession", JSON.stringify(session));
  }

  // ✅ Retrieve valid token
  getSessionToken() {
    try {
      const sessionData = sessionStorage.getItem("authSession");
      if (!sessionData) return null;

      const session = JSON.parse(sessionData);
      const now = Date.now();
      if (now > session.expiresAt) {
        this.clearSession();
        return null;
      }

      return session.token;
    } catch (err) {
      console.error("❌ Error retrieving session token:", err);
      return null;
    }
  }

  // ✅ Remove session (used on logout or 401)
  clearSession() {
    sessionStorage.removeItem("authSession");
    sessionStorage.removeItem("temp2FASession");
    localStorage.removeItem("authToken");
    localStorage.removeItem("user");
  }

  // ✅ Check if user is authenticated
  isAuthenticated() {
    return !!this.getSessionToken();
  }

  // ✅ REST methods
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

// ✅ Create and export global instance
export const apiClient = new ApiClient();
export default apiClient;
