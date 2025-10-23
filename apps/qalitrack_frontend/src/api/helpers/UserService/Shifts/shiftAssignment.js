import { apiClient } from '../../apiClients.js';

/**
 * Fetches a paginated list of shifts from the server.
 * @param {number} page - The page number to fetch.
 * @param {number} pageSize - The number of shifts per page.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the paginated shift data object.
 */
export const fetchShifts = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/Reports/shifts', {
            params: {
                page,
                pageSize,
            },
            signal,
        });
        return response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch shifts error:', err);
        }
        throw err;
    }
};

/**
 * Fetches a paginated list of deleted shifts from the server.
 * @param {number} page - The page number to fetch.
 * @param {number} pageSize - The number of shifts per page.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the paginated deleted shift data object.
 */
export const fetchDeletedShifts = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/shift/deleted', {
            params: {
                page,
                pageSize,
            },
            signal,
        });
        return response.data.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch deleted shifts error:', err);
        }
        throw err;
    }
};

/**
 * Updates a shift by ID.
 * @param {string} shiftId - The ID of the shift to update.
 * @param {object} shiftData - The shift data to update.
 * @returns {Promise<any>} A promise that resolves when the shift is updated.
 */
export const updateShift = async (shiftId, shiftData) => {
    try {
        const payload = {
            name: shiftData.name,
            description: shiftData.description,
            startTime: shiftData.startTime,
            durationMinutes: shiftData.durationMinutes,
            mode: shiftData.mode,
            autoRepeatDaily: shiftData.autoRepeatDaily,
        };


        return await apiClient.put(`/Shift/${shiftId}`, payload);
    } catch (err) {
        console.error(`Update shift ${shiftId} error:`, err);
        throw err;
    }
};

/**
 * Deletes a shift by ID.
 * @param {string} shiftId - The ID of the shift to delete.
 * @returns {Promise<any>} A promise that resolves when the shift is deleted.
 */
export const deleteShift = async (shiftId) => {
    try {
        return await apiClient.delete(`/Shift/${shiftId}`);
    } catch (err) {
        console.error(`Delete shift ${shiftId} error:`, err);
        throw err;
    }
};

/**
 * Creates a new shift.
 * @param {object} shiftData - The shift data to create.
 * @returns {Promise<any>} A promise that resolves when the shift is created.
 */
export const createShift = async (shiftData) => {
    try {
        // Ensure payload matches required format
        const payload = {
            name: shiftData.name,
            description: shiftData.description,
            startTime: shiftData.startTime,
            durationMinutes: shiftData.durationMinutes,
            mode: shiftData.mode,
            autoRepeatDaily: shiftData.autoRepeatDaily,
        };
        return await apiClient.post('/Shift', payload);
    } catch (err) {
        console.error('Create shift error:', err);
        throw err;
    }
};
// javascript
/**
 * Fetches users assigned to a specific shift.
 * @param {string} shiftId - The ID of the shift.
 * @param {AbortSignal} [signal] - Optional AbortSignal to cancel the request.
 * @returns {Promise<Array>} Resolves to an array of user objects.
 */
export const fetchShiftUsers = async (shiftId, signal) => {
    try {
        if (!shiftId) {
            throw new Error('Shift ID is required to fetch users.');
        }

        const response = await apiClient.get(`/UserShift/shift/${shiftId}/users`, {
            signal,
        });

        // Unwrap nested data if API returns { data: { items: [...] } } else return response.data
        return response.data?.data?.items ?? response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error(`Fetch users for shift ${shiftId} error:`, err);
        }
        throw err;
    }
};
/**
 * Assigns a shift to a role.
 * @param {string} roleId - The ID of the role.
 * @param {string} shiftId - The ID of the shift.
 * @returns {Promise<any>} A promise that resolves when the shift is assigned to the role.
 */
export const assignShiftToRole = async (roleId, shiftId) => {
    try {
        if (!roleId || !shiftId) {
            throw new Error('Role ID and Shift ID are required.');
        }
        return await apiClient.post(`/UserShift/role/${roleId}/shift/${shiftId}`);
    } catch (err) {
        console.error(`Assign shift ${shiftId} to role ${roleId} error:`, err);
        throw err;
    }
};

/**
 * Removes a shift assignment from a role.
 * @param {string} roleId - The ID of the role.
 * @param {string} shiftId - The ID of the shift.
 * @returns {Promise<any>} A promise that resolves when the shift assignment is removed.
 */
export const removeShiftFromRole = async (roleId, shiftId) => {
    try {
        if (!roleId || !shiftId) {
            throw new Error('Role ID and Shift ID are required.');
        }
        return await apiClient.delete(`/UserShift/role/${roleId}/shift/${shiftId}`);
    } catch (err) {
        console.error(`Remove shift ${shiftId} from role ${roleId} error:`, err);
        throw err;
    }
};
/**
 * Assigns a shift to a user.
 * @param {string} userId - The ID of the user.
 * @param {string} shiftId - The ID of the shift.
 * @returns {Promise<any>} A promise that resolves when the shift is assigned to the user.
 */
export const assignShiftToUser = async (userId, shiftId) => {
    try {
        if (!userId || !shiftId) {
            throw new Error('User ID and Shift ID are required.');
        }
        return await apiClient.post(`/UserShift/${userId}/shift/${shiftId}`);
    } catch (err) {
        console.error(`Assign shift ${shiftId} to user ${userId} error:`, err);
        throw err;
    }
};

/**
 * Removes a shift assignment from a user.
 * @param {string} userId - The ID of the user.
 * @param {string} shiftId - The ID of the shift.
 * @returns {Promise<any>} A promise that resolves when the shift assignment is removed.
 */
export const removeShiftFromUser = async (userId, shiftId) => {
    try {
        if (!userId || !shiftId) {
            throw new Error('User ID and Shift ID are required.');
        }
        return await apiClient.delete(`/UserShift/${userId}/shift/${shiftId}`);
    } catch (err) {
        console.error(`Remove shift ${shiftId} from user ${userId} error:`, err);
        throw err;
    }
};


/**
 * Fetches a paginated list of deleted shifts from the server.
 * @param {number} page - The page number to fetch.
 * @param {number} pageSize - The number of shifts per page.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the paginated deleted shift data object.
 */
export const fetchDeletedUserShifts = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/UserShift/deleted/paged', {
            params: {
                page,
                pageSize,
            },
            signal,
        });
        console.log('Deleted User Shifts response:', response.data);
        return response.data.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch deleted shifts error:', err);
        }
        throw err;
    }
};

/**
 * Fetches a single shift by ID from the server.
 * @param {string} shiftId - The ID of the shift to fetch.
 * @param {AbortSignal} signal - An optional AbortSignal to cancel the request.
 * @returns {Promise<object>} A promise that resolves to the shift data object.
 */
export const fetchShiftById = async (shiftId, signal) => {
    try {
        const response = await apiClient.get(`/Shift/${shiftId}`, { signal });
        return response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error(`Fetch shift ${shiftId} error:`, err);
        }
        throw err;
    }
};