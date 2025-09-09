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
 * Fetches a single user by ID.
 * @param {string} userId - The ID of the user to fetch.
 * @returns {Promise<object>} A promise that resolves to the user data object.
 */
export const fetchUserById = async (userId) => {
    try {
        const response = await apiClient.get(`/Users/${userId}`);
        return response.data;
    } catch (err) {
        console.error(`Fetch user with ID ${userId} error:`, err);
        throw err;
    }
};

/**
 * Updates a user.
 * @param {string} userId - The ID of the user to update.
 * @param {object} userData - The user data to update.
 * @returns {Promise<any>} A promise that resolves when the user is updated.
 */
export const updateUser = async (userId, userData) => {
    try {
        return await apiClient.put(`/Users/${userId}`, userData);
    } catch (err) {
        console.error(`Update user ${userId} error:`, err);
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

/**
 * Fetches roles for a specific user.
 * @param {string} userId - The ID of the user.
 * @returns {Promise<any>} A promise that resolves with the user's roles.
 */
export const fetchUserRoles = async (userId) => {
    try {
        const response = await apiClient.get(`/Users/${userId}/roles`);
        return response.data.roles;
    } catch (err) {
        console.error(`Fetch roles for user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Resets a user's password.
 * @param {string} userId - The ID of the user to reset the password for.
 * @returns {Promise<any>} A promise that resolves when the password is reset.
 */
export const resetPassword = async (userId) => {
    try {
        return await apiClient.post(`/Users/${userId}/resetpassword`);
    } catch (err) {
        console.error(`Reset password for user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Assigns a role to a user.
 * @param {string} userId - The ID of the user.
 * @param {string} roleId - The ID of the role to assign.
 * @returns {Promise<any>} A promise that resolves when the role is assigned to the user.
 */
export const assignRoleToUser = async (userId, roleId) => {
    try {
        if (!userId) {
            throw new Error('User ID is missing. Cannot assign role.');
        }
        if (!roleId) {
            throw new Error('Role ID is missing. Cannot assign role.');
        }
        return await apiClient.post(`/UserRole/${userId}/roles/${roleId}`);
    } catch (err) {
        console.error(`Assign role ${roleId} to user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Removes a role from a user.
 * @param {string} userId - The ID of the user.
 * @param {string} roleId - The ID of the role to remove.
 * @returns {Promise<any>} A promise that resolves when the role is removed from the user.
 */
export const removeRoleFromUser = async (userId, roleId) => {
    try {
        if (!userId) {
            throw new Error('User ID is missing. Cannot remove role.');
        }
        if (!roleId) {
            throw new Error('Role ID is missing. Cannot remove role.');
        }
        return await apiClient.delete(`/UserRole/${userId}/roles/${roleId}`);
    } catch (err) {
        console.error(`Remove role ${roleId} from user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Fetches shifts for a specific user.
 * @param {string} userId - The ID of the user.
 * @returns {Promise<any>} A promise that resolves with the user's shifts.
 */
export const fetchUserShifts = async (userId) => {
    try {
        if (!userId) {
            throw new Error('User ID is missing. Cannot fetch shifts.');
        }

        // Fix: Use template literal syntax with ${} instead of {}
        const response = await apiClient.get(`/Reports/users/${userId}`);
        // Transform the server response to match what the component expects
        const userData = response.data;

        // Map assignedShifts to the format expected by the component
        const transformedShifts = userData.assignedShifts.map(shift => ({
            id: shift.shiftId,
            name: shift.shiftName,
            mode: shift.shiftMode,
            startTime: shift.assignedAt, // Using assignedAt as startTime
            endTime: null, // Server doesn't provide endTime
            duration: null, // Server doesn't provide duration
            isActive: shift.isActive,
            role: null // Server doesn't provide role info in this response
        }));

        return transformedShifts;
    } catch (err) {
        console.error(`Fetch shifts for user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Creates a new user.
 * @param {object} userData - The user data to create.
 * @returns {Promise<any>} A promise that resolves when the user is created.
 */
export const createUser = async (userData) => {
    try {
        const response = await apiClient.post('/Users', userData);
        return response.data;
    } catch (err) {
        console.error('Create user error:', err);
        throw err;
    }
};
