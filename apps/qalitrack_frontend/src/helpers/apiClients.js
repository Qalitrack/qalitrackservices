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
                if (error.response?.status === 401) {
                }

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