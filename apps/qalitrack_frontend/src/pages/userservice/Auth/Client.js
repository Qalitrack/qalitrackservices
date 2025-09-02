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

        // Request interceptor to add auth token
        this.client.interceptors.request.use(
            (config) => {
                const token = localStorage.getItem('authToken');
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
                // Handle 401 errors globally (optional)
                if (error.response?.status === 401) {
                    // Optional: Clear auth data and redirect to login
                    // localStorage.removeItem('authToken');
                    // localStorage.removeItem('user');
                    // window.location.href = '/login';
                }
                return Promise.reject(error);
            }
        );
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