import { apiClient } from '../../apiClients.js';

/**
 * Fetches all permissions from the server.
 * @returns {Promise<Array>} A promise that resolves to an array of permission objects.
 */
export const fetchPermissions = async (signal) => {
    try {
        const response = await apiClient.get('/Permissions', { signal });
        const responseData = response.data?.data || response.data;

        if (Array.isArray(responseData)) {
            return responseData;
        } else {
            throw new Error('Invalid response data: Expected an array of permissions.');
        }
    } catch (err) {
        if (err.name !== 'CanceledError') {
        }
        throw err; // Re-throw the error to be handled by the calling component
    }
};

/**
 * Updates a specific permission on the server.
 * @param {object} permission - The permission object to update. Must contain an 'id'.
 * @returns {Promise<object>} A promise that resolves to the updated permission object.
 */
export const updatePermission = async (permission) => {
    try {
        const permissionId = permission.id;
        if (!permissionId) {
            throw new Error('Permission ID is missing. Cannot update permission.');
        }
        const response = await apiClient.put(`/Permissions/${permissionId}`, permission);
        return response.data?.data || response.data;
    } catch (err) {
        throw err;
    }
};

/**
 * Deletes a specific permission from the server.
 * @param {string} permissionId - The ID of the permission to delete.
 * @returns {Promise<any>} A promise that resolves when the deletion is successful.
 * @throws {Error} with a user-friendly message when deletion fails
 */
export const deletePermission = async (permissionId) => {
    try {
        if (!permissionId) {
            throw new Error('Permission ID is missing. Cannot delete permission.');
        }
        const response = await apiClient.delete(`/Permissions/${permissionId}`);
        return response.data?.data || response.data;
    } catch (err) {

        // Handle specific error cases with user-friendly messages
        if (err.response) {
            // Server responded with an error status code
            const status = err.response.status;

            if (status === 404) {
                throw new Error('Permission not found. It may have been already deleted.');
            } else if (status === 403) {
                throw new Error('You do not have permission to delete this permission.');
            } else if (status === 409 || err.response.data?.title?.includes('constraint')) {
                throw new Error('This permission cannot be deleted because it is being used by one or more roles.');
            } else if (status === 400) {
                throw new Error(err.response.data?.message || 'Invalid request when deleting permission.');
            } else {
                throw new Error(`Failed to delete permission: ${err.response.data?.message || 'Unknown server error'}`);
            }
        } else if (err.request) {
            // No response received from server
            throw new Error('No response from server. Please check your network connection and try again.');
        } else {
            // Something happened in setting up the request
            throw new Error(`Error deleting permission: ${err.message}`);
        }
    }
};

/**
 * Fetches all deleted permissions from the server.
 * @returns {Promise<Array>} A promise that resolves to an array of deleted permission objects.
 */
export const fetchDeletedPermissions = async (signal) => {
    try {
        const response = await apiClient.get('/Permissions/deleted', { signal });
        const responseData = response.data?.data;

        if (responseData && Array.isArray(responseData.items)) {
            return responseData.items;
        } else if (Array.isArray(responseData)) {
            return responseData;
        } else {
            throw new Error('Invalid response data: Expected an array of permissions.');
        }
    } catch (err) {
        if (err.name !== 'CanceledError') {
        }
        throw err;
    }
};

/**
 * Restores a specific permission on the server.
 * @param {string} permissionId - The ID of the permission to restore.
 * @returns {Promise<any>} A promise that resolves when the restoration is successful.
 */
export const restorePermission = async (permissionId) => {
    try {
        if (!permissionId) {
            throw new Error('Permission ID is missing. Cannot restore permission.');
        }
        const response = await apiClient.patch(`/Permissions/${permissionId}/restore`);
        return response.data;
    } catch (err) {
        throw err;
    }
};

/**
 * Creates a new permission on the server.
 * @param {object} permission - The permission object to create.
 * @returns {Promise<object>} A promise that resolves to the created permission object.
 */
export const createPermission = async (permission) => {
    try {
        const response = await apiClient.post('/Permissions', permission);
        return response.data?.data || response.data;
    } catch (err) {
        // Check specifically for the unique constraint violation error
        if (err.response?.data?.message?.includes('duplicate key value') ||
            err.response?.status === 409 ||
            err.response?.data?.title?.includes('unique constraint')) {
            throw new Error('A permission with this name already exists. Please use a unique permission name.');
        }
        throw err;
    }
};

/**
 * Fetches all roles associated with a specific permission.
 * @param {string} permissionId - The ID of the permission.
 * @returns {Promise<Array>} A promise that resolves to an array of role objects.
 */
export const fetchRolesForPermission = async (permissionId) => {
    try {
        if (!permissionId) {
            throw new Error('Permission ID is missing.');
        }
        const response = await apiClient.get(`/Permissions/${permissionId}/roles`);
        const responseData = response.data?.data || response.data;

        if (Array.isArray(responseData)) {
            return responseData;
        } else {
            // Handle cases where the API might return a single object or other non-array types
            return [];
        }
    } catch (err) {
        throw err;
    }
};
