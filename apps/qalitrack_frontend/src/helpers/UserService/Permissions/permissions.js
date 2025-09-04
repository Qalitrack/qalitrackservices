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
            console.error('Fetch permissions error:', err);
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
        console.error('Update permission error:', err);
        throw err;
    }
};

/**
 * Deletes a specific permission from the server.
 * @param {string} permissionId - The ID of the permission to delete.
 * @returns {Promise<any>} A promise that resolves when the deletion is successful.
 */
export const deletePermission = async (permissionId) => {
    try {
        if (!permissionId) {
            throw new Error('Permission ID is missing. Cannot delete permission.');
        }
        const response = await apiClient.delete(`/Permissions/${permissionId}`);
        return response.data;
    } catch (err) {
        console.error('Delete permission error:', err);
        throw err;
    }
};

/**
 * Creates a new permission on the server.
 * @param {object} permissionData - The data for the new permission (e.g., { name: 'new.permission', description: '...' }).
 * @returns {Promise<object>} A promise that resolves to the newly created permission object.
 */
export const createPermission = async (permissionData) => {
    try {
        const response = await apiClient.post('/Permissions', permissionData);
        return response.data?.data || response.data;
    } catch (err) {
        console.error('Create permission error:', err);
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
        console.error(`Fetch roles for permission ${permissionId} error:`, err);
        throw err;
    }
};
