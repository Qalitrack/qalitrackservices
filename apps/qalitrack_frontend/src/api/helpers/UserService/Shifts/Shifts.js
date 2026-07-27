import { apiClient } from '../../apiClients.js';

/**
 * Fetches all shifts from the server with pagination support.
 * @param {number} [page=1] - The page number to fetch (1-based).
 * @param {number} [pageSize=10] - The number of items per page.
 * @param {AbortSignal} [signal] - An optional AbortSignal to cancel the request.
 * @returns {Promise<Object>} A promise that resolves to an object containing items and pagination info.
 */
export const fetchShifts = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/shift', {
            params: { page, pageSize },
            signal
        });

        const responseData = response.data.data || {};
        // Extract items and pagination data from the response
        const items = Array.isArray(responseData.items) ? responseData.items : [];
        const paginationData = {
            page: parseInt(responseData.page) || page,
            pageSize: parseInt(responseData.pageSize) || pageSize,
            totalCount: parseInt(responseData.totalCount) || 0,
            totalPages: parseInt(responseData.totalPages) || 0,
            hasNextPage: Boolean(responseData.hasNextPage),
            hasPreviousPage: Boolean(responseData.hasPreviousPage)
        };

        // Return a flat object to match loadData expectations
        return {
            items,
            ...paginationData
        };
    } catch (err) {
        if (err.name !== 'CanceledError') {
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
            params: { page, pageSize },
            signal,
        });
        return response.data.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
        }
        throw err;
    }
};

/**
 * Gets a shift by ID.
 * @param {string} shiftId - The ID of the shift to retrieve.
 * @returns {Promise<object>} A promise that resolves to the shift data.
 */
export const getShiftById = async (shiftId) => {
    try {
        const response = await apiClient.get(`/shift/${shiftId}`);
        return response.data;
    } catch (err) {
        throw err;
    }
};

/**
 * Updates a shift by ID.
 * @param {string} shiftId - The ID of the shift to update.
 * @param {object} shiftData - The shift data to update (should be pre-formatted).
 * @returns {Promise<any>} A promise that resolves when the shift is updated.
 */
export const updateShift = async (shiftId, shiftData) => {
    try {
        // Use the data as-is since it's already formatted in the component
        const response = await apiClient.put(`/shift/${shiftId}`, shiftData);
        return response.data;
    } catch (err) {
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
        const response = await apiClient.delete(`/shift/${shiftId}`);
        return response.data;
    } catch (err) {
        throw err;
    }
};

/**
 * Creates a new shift.
 * @param {object} shiftData - The shift payload, already shaped to match CreateShiftRequest
 *   (startTime/endTime as "HH:MM:SS", startDate/endDate as "YYYY-MM-DD").
 * @returns {Promise<any>} A promise that resolves when the shift is created.
 */
export const createShift = async (shiftData) => {
    try {
        const response = await apiClient.post('/shift', shiftData);
        return response.data;
    } catch (err) {
        throw err;
    }
};


/**
 * Fetches shift instances for a specific shift ID
 * @param {string} shiftId - The ID of the shift to fetch instances for
 * @param {Object} [params] - Optional query parameters
 * @param {AbortSignal} [signal] - Optional AbortSignal to cancel the request
 * @returns {Promise<Array>} A promise that resolves to an array of shift instances
 */
export const fetchShiftInstances = async (shiftId, params = {}, signal) => {
    try {
        const response = await apiClient.get(`ShiftInstance/by-shift/${shiftId}`, {
            params,
            signal
        });
        return response.data;
    } catch (err) {
        if (err.name !== 'CanceledError') {
        }
        throw err;
    }
};

