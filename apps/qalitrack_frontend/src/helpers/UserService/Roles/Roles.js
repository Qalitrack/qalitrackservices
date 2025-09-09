import { apiClient } from '../../apiClients.js';

/**
 * Fetches all roles from the server.
 * @returns {Promise<Array>} A promise that resolves to an array of role objects.
 */
export const fetchRoles = async (signal) => {
    try {
        const response = await apiClient.get('/Roles', { signal });
        const responseData = response.data?.data || response.data;

        if (Array.isArray(responseData)) {
            return responseData;
        } else {
            throw new Error('Invalid response data: Expected an array of roles.');
        }
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch roles error:', err);
        }
        throw err;
    }
};

/**
 * Creates a new role on the server.
 * @param {object} role - The role object to create.
 * @returns {Promise<object>} A promise that resolves to the created role object.
 */
export const createRole = async (role) => {
    try {
        const response = await apiClient.post('/Roles', role);
        return response.data?.data || response.data;
    } catch (err) {
        console.error('Create role error:', err);
        throw err;
    }
};

/**
 * Updates a specific role on the server.
 * @param {object} role - The role object to update. Must contain an 'id'.
 * @returns {Promise<object>} A promise that resolves to the updated role object.
 */
export const updateRole = async (role) => {
    try {
        const roleId = role.id;
        if (!roleId) {
            throw new Error('Role ID is missing. Cannot update role.');
        }
        const response = await apiClient.put(`/Roles/${roleId}`, role);
        return response.data?.data || response.data;
    } catch (err) {
        console.error('Update role error:', err);
        throw err;
    }
};

/**
 * Deletes a specific role from the server.
 * @param {string} roleId - The ID of the role to delete.
 * @returns {Promise<any>} A promise that resolves when the deletion is successful.
 */
export const deleteRole = async (roleId) => {
    try {
        if (!roleId) {
            throw new Error('Role ID is missing. Cannot delete role.');
        }
        const response = await apiClient.delete(`/Roles/${roleId}`);
        return response.data;
    } catch (err) {
        console.error('Delete role error:', err);
        throw err;
    }
};

/**
 * Fetches all deleted roles from the server.
 * @returns {Promise<Array>} A promise that resolves to an array of role objects.
 */
export const fetchDeletedRoles = async (signal) => {
    try {
        const response = await apiClient.get('/Roles/deleted', { signal });
        const responseData = response.data;

        if (responseData && Array.isArray(responseData.items)) {
            return responseData.items;
        } else if (Array.isArray(responseData)) {
            return responseData;
        } else {
            throw new Error('Invalid response data: Expected an array of roles.');
        }
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch deleted roles error:', err);
        }
        throw err;
    }
};

/**
 * Restores a specific role on the server.
 * @param {string} roleId - The ID of the role to restore.
 * @returns {Promise<any>} A promise that resolves when the restoration is successful.
 */
export const restoreRole = async (roleId) => {
    try {
        if (!roleId) {
            throw new Error('Role ID is missing. Cannot restore role.');
        }
        const response = await apiClient.patch(`/Roles/${roleId}/restore`);
        return response.data;
    } catch (err) {
        console.error('Restore role error:', err);
        throw err;
    }
};

/**
 * Fetches all permissions for a specific role.
 * @param {string} roleId - The ID of the role.
 * @returns {Promise<Array>} A promise that resolves to an array of permission objects.
 */
export const getPermissionsForRole = async (roleId, signal) => {
    try {
        const response = await apiClient.get(`/Roles/${roleId}/permissions`, { signal });
        const responseData = response.data?.data || response.data;
        return Array.isArray(responseData) ? responseData : [];
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error(`Fetch permissions for role ${roleId} error:`, err);
        }
        throw err;
    }
};

/**
 * Assigns a permission to a role.
 * @param {string} roleId - The ID of the role.
 * @param {string} permissionId - The ID of the permission to assign.
 * @returns {Promise<any>} A promise that resolves when the assignment is successful.
 */
export const assignPermissionToRole = async (roleId, permissionId) => {
    try {
        return await apiClient.post(`/Roles/${roleId}/permissions/${permissionId}`);
    } catch (err) {
        console.error(`Assign permission ${permissionId} to role ${roleId} error:`, err);
        throw err;
    }
};

/**
 * Removes a permission from a role.
 * @param {string} roleId - The ID of the role.
 * @param {string} permissionId - The ID of the permission to remove.
 * @returns {Promise<any>} A promise that resolves when the removal is successful.
 */
export const removePermissionFromRole = async (roleId, permissionId) => {
    try {
        return await apiClient.delete(`/Roles/${roleId}/permissions/${permissionId}`);
    } catch (err) {
        console.error(`Remove permission ${permissionId} from role ${roleId} error:`, err);
        throw err;
    }
};
