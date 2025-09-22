import {apiClient} from "../../apiClients.js";


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
        console.log(response.data);

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
        
        console.error('Error fetching attendance by instance ID:', err);
        throw err;
    }
};