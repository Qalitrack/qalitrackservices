import { backupApiClient } from '../BackupApiclient.js';
// If you want to use the backup client instead, uncomment the line below:
// import { backupApiClient as apiClient } from './BackupApiClient.js';

class MicroserviceAPI {
    constructor() {
        this.endpoint = '/Microservice';
    }

    /**
     * Fetch microservice data from the API
     * @returns {Promise<Object>} The response data from the microservice
     */
    async getMicroserviceData() {
        try {

            const response = await backupApiClient.get(this.endpoint);
            return response.data;

        } catch (error) {

            // You might want to handle specific error cases
            if (error.originalError?.response?.status === 404) {
            } else if (error.originalError?.response?.status === 500) {
            }

            throw error;
        }
    }

    /**
     * Initialize and fetch microservice data immediately
     * Useful for testing or immediate execution
     */
    async init() {
        try {
            const data = await this.getMicroserviceData();
            return data;
        } catch (error) {
            return null;
        }
    }

    /**
     * Update an existing microservice by name
     * @param {string} name - The name of the microservice to update
     * @param {Object} updateData - The data to update the microservice with
     * @returns {Promise<Object>} The updated microservice data
     */
    async updateMicroservice(name, updateData) {
        try {
            if (!name) {
                throw new Error('Microservice name is required for update');
            }
            
            
            const response = await backupApiClient.put(
                `${this.endpoint}/${encodeURIComponent(name)}`,
                updateData
            );
            
            return response.data;
            
        } catch (error) {
            
            if (error.response?.status === 404) {
                throw new Error(`Microservice '${name}' not found`);
            } else if (error.response?.status === 400) {
                throw new Error('Invalid update data provided');
            } else if (error.response?.status === 401) {
                throw new Error('Unauthorized to update microservice');
            }
            
            throw error;
        }
    }

    /**
     * Delete a microservice by name
     * @param {string} name - The name of the microservice to delete
     * @returns {Promise<Object>} The response from the server
     */
    async deleteMicroservice(name) {
        try {
            if (!name) {
                throw new Error('Microservice name is required for deletion');
            }
            const response = await backupApiClient.delete(
                `${this.endpoint}/${encodeURIComponent(name)}`
            );
            return response.data;
            
        } catch (error) {
            
            if (error.response?.status === 404) {
                throw new Error(`Microservice '${name}' not found`);
            } else if (error.response?.status === 401) {
                throw new Error('Unauthorized to delete microservice');
            } else if (error.response?.data) {
                throw new Error(error.response.data.message || 'Failed to delete microservice');
            }
            
            throw error;
        }
    }

    /**
     * Create a new microservice
     * @param {Object} microserviceData - The microservice data to create
     * @param {string} microserviceData.name - The name of the microservice
     * @param {string} microserviceData.connectionString - The connection string
     * @param {number} [microserviceData.status=0] - The status of the microservice (0: Offline, 1: Online, 2: Maintenance)
     * @param {string} [microserviceData.lastBackupAt] - ISO string of the last backup time
     * @returns {Promise<Object>} The created microservice data
     */
    async createMicroservice(microserviceData) {
        try {
            
            const response = await backupApiClient.post(this.endpoint, microserviceData);
            return response.data;
            
        } catch (error) {
            
            // Extract and format error message
            let errorMessage = error.message || 'Failed to create microservice';
            if (error.originalError?.response?.data) {
                const serverError = error.originalError.response.data;
                if (typeof serverError === 'string') {
                    errorMessage = serverError;
                } else if (serverError.errors) {
                    errorMessage = Object.entries(serverError.errors)
                        .map(([field, messages]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
                        .join('; ');
                } else if (serverError.title) {
                    errorMessage = serverError.title;
                }
            }
            
            const errorWithMessage = new Error(errorMessage);
            errorWithMessage.originalError = error;
            throw errorWithMessage;
        }
    }
}

// Create an instance of the MicroserviceAPI
const microserviceAPI = new MicroserviceAPI();

// Export both the class and instance for flexibility
export { MicroserviceAPI, microserviceAPI };

/**
 * Create a new microservice
 * @param {Object} microserviceData - The microservice data to create
 * @returns {Promise<Object>} The created microservice data
 */
export async function createMicroservice(microserviceData) {
    try {
        const data = await microserviceAPI.createMicroservice(microserviceData);
        return data;
    } catch (error) {
        throw error;
    }
}

/**
 * Delete a microservice by name
 * @param {string} name - The name of the microservice to delete
 * @returns {Promise<Object>} The response from the server
 */
export async function deleteMicroservice(name) {
    try {
        const data = await microserviceAPI.deleteMicroservice(name);
        return data;
    } catch (error) {
        throw error;
    }
}

/**
 * Update an existing microservice
 * @param {string} name - The name of the microservice to update
 * @param {Object} updateData - The data to update
 * @returns {Promise<Object>} The updated microservice data
 */
export async function updateMicroservice(name, updateData) {
    try {
        const data = await microserviceAPI.updateMicroservice(name, updateData);
        return data;
    } catch (error) {
        throw error;
    }
}

/**
 * Simple function to fetch and log microservice data
 * @returns {Promise<Object|null>} The microservice data or null if error
 */
export async function fetchMicroserviceData() {
    try {
        const data = await microserviceAPI.getMicroserviceData();
        return data;
    } catch (error) {
        return null;
    }
}