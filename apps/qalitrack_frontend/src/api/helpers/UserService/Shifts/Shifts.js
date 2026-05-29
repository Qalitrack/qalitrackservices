import { apiClient } from '../../apiClients.js';
import shifts from "../../../../pages/Userservice/Shifts.jsx";

// Helper functions
const formatTime = (dateString) => {
    if (!dateString) return '00:00:00';
    const date = new Date(dateString);
    // Format as HH:mm:ss
    return date.toTimeString().substring(0, 8);
};

const formatDate = (dateString) => {
    if (!dateString) return null;
    return new Date(dateString).toISOString();
};

const prepareShiftPayload = (shiftData) => {
    // Calculate duration in minutes if not provided
    const durationMinutes = shiftData.durationMinutes || 
        (shiftData.startTime && shiftData.endTime 
            ? Math.round((new Date(shiftData.endTime) - new Date(shiftData.startTime)) / (1000 * 60))
            : 60);

    // Format times as HH:mm:ss
    const startTime = formatTime(shiftData.startTime || new Date());
    const endTime = formatTime(shiftData.endTime || new Date(Date.now() + 3600000));
    
    return {
        name: shiftData.name?.trim() || '',
        description: shiftData.description?.trim() || '',
        startTime: startTime,
        endTime: endTime,
        mode: parseInt(shiftData.mode, 10) || 0,
        startDate: formatDate(shiftData.startDate || new Date()),
        endDate: shiftData.endDate ? formatDate(shiftData.endDate) : null,
        type: parseInt(shiftData.type, 10) || 1,
        requiredStaffCount: parseInt(shiftData.requiredStaffCount, 10) || 1,
        recurrenceType: parseInt(shiftData.recurrenceType, 10) || 0,
        recurrenceInterval: parseInt(shiftData.recurrenceInterval, 10) || 1,
        customDays: Array.isArray(shiftData.customDays) ? shiftData.customDays : [],
        exceptionDates: Array.isArray(shiftData.exceptionDates) 
            ? shiftData.exceptionDates.map(date => formatDate(date)).filter(Boolean)
            : [],
        autoRepeatDaily: Boolean(shiftData.autoRepeatDaily),
        durationMinutes: durationMinutes
    };
};

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
 * @param {object} shiftData - The shift data to create.
 * @returns {Promise<any>} A promise that resolves when the shift is created.
 */
export const createShift = async (shiftData) => {
    try {
        const payload = prepareShiftPayload(shiftData);

        // Validate required fields
        if (!payload.name) {
            throw new Error('Shift name is required');
        }

        if (!payload.startTime || !payload.endTime) {
            throw new Error('Start time and end time are required');
        }

        const response = await apiClient.post('/shift', payload);
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

