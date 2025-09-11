// Runtime configuration utility
export const getApiBaseUrl = () => {
  // Try runtime config first, fallback to build-time env var, then localhost
  return window.ENV?.API_BASE_URL || 
         import.meta.env.VITE_API_BASE_URL || 
         'http://localhost:5000/api';
};

export const API_BASE_URL = getApiBaseUrl();