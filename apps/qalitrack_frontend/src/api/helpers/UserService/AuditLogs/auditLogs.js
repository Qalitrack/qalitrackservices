import { apiClient } from '../../apiClients.js';

// Returns { items, page, pageSize, totalCount } — see PagedResult<AuditLogDto> on the backend.
export const fetchAuditLogs = async (page = 1, pageSize = 10, signal) => {
    try {
        const response = await apiClient.get('/AuditLogs', { params: { page, pageSize }, signal });
        const data = response.data?.data || response.data;
        return {
            items: data?.items || [],
            page: data?.page || page,
            pageSize: data?.pageSize || pageSize,
            totalCount: data?.totalCount || 0,
        };
    } catch (err) {
        if (err.name !== 'CanceledError') {
        }
        throw err;
    }
};
