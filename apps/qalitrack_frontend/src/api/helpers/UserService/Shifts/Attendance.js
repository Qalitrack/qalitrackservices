import {apiClient} from "../../apiClients.js";

/**
 * Fetches attendance records across all shifts, most recent first.
 * Note: GET /ShiftAttendance returns a plain page of items with no total
 * count/page metadata, so pagination here is Prev/Next-only (no "of N pages").
 * @param {Object} options - Pagination options
 * @param {number} options.pageNumber - Page number (default: 1)
 * @param {number} options.pageSize - Number of items per page (default: 20)
 * @param {AbortSignal} signal - Optional AbortSignal to cancel the request
 * @returns {Promise<Array>}
 */
export const getAllAttendance = async ({ pageNumber = 1, pageSize = 20 } = {}, signal) => {
    try {
        const response = await apiClient.get('/ShiftAttendance', {
            params: { pageNumber, pageSize },
            signal,
        });
        return Array.isArray(response.data) ? response.data : [];
    } catch (err) {
        if (err.name === 'CanceledError') {
            throw err;
        }
        if (err.response && err.response.status === 404) {
            return [];
        }
        throw err;
    }
};

/**
 * Fetches attendance records for a specific shift instance with pagination
 * @param {string} instanceId - The ID of the shift instance
 * @param {Object} options - Pagination options
 * @param {number} options.pageNumber - Page number (default: 1)
 * @param {number} options.pageSize - Number of items per page (default: 10)
 * @param {AbortSignal} signal - Optional AbortSignal to cancel the request
 * @returns {Promise<{items: Array, page: number, pageSize: number, totalCount: number, totalPages: number, hasPreviousPage: boolean, hasNextPage: boolean}>}
 */
export const getAttendanceByInstanceId = async (instanceId, { pageNumber = 1, pageSize = 10 } = {}, signal) => {
    try {
        const response = await apiClient.get(`/ShiftAttendance/instance/${instanceId}/attendance`, {
            params: { pageNumber, pageSize },
            signal,
        });

        return {
            items: response.data.items || [],
            page: response.data.page,
            pageSize: response.data.pageSize,
            totalCount: response.data.totalCount,
            totalPages: response.data.totalPages,
            hasPreviousPage: response.data.hasPreviousPage,
            hasNextPage: response.data.hasNextPage
        };
    } catch (err) {
        if (err.name === 'CanceledError') {
            throw err;
        }
        
        // If it's a 404, return empty results instead of throwing
        if (err.response && err.response.status === 404) {
            return {
                items: [],
                page: 1,
                pageSize: pageSize,
                totalCount: 0,
                totalPages: 0,
                hasPreviousPage: false,
                hasNextPage: false
            };
        }
        
        throw err;
    }
};

