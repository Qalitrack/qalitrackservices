import { apiClient } from '../../apiClients.js';

/**
 * Fetches a paginated list of users from the server.
 * @param {number} page - The page number to fetch.
 * @param {number} pageSize - The number of users per page.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the paginated user data object.
 */
export const fetchUsers = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/Users/paged', {
            params: {
                page,
                pageSize,
            },
            signal,
        });
        return response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch users error:', err);
        }
        throw err;
    }
};

/**
 * Fetches a paginated list of deleted users from the server.
 * @param {number} page - The page number to fetch.
 * @param {number} pageSize - The number of users per page.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the paginated user data object.
 */
export const fetchDeletedUsers = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/Users/deleted/paged', {
            params: {
                page,
                pageSize,
            },
            signal,
        });
        return response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch deleted users error:', err);
        }
        throw err;
    }
};

/**
 * Soft deletes a user.
 * @param {string} userId - The ID of the user to delete.
 * @returns {Promise<any>} A promise that resolves when the user is deleted.
 */
export const deleteUser = async (userId) => {
    try {
        // Using apiClient.delete as it's more conventional for deletion.
        // The backend should handle this as a soft delete.
        return await apiClient.delete(`/Users/${userId}`);
    } catch (err) {
        console.error(`Delete user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Restores a soft-deleted user.
 * @param {string} userId - The ID of the user to restore.
 * @returns {Promise<any>} A promise that resolves when the user is restored.
 */
export const restoreUser = async (userId) => {
    try {
        return await apiClient.patch(`/Users/${userId}/restore`);
    } catch (err) {
        console.error(`Restore user ${userId} error:`, err);
        throw err;
    }
};
