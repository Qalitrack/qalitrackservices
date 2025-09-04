import { apiClient } from '../../apiClients.js';

export const fetchPasswordPolicy = async (signal) => {
    try {
        const response = await apiClient.get('/PasswordPolicy', { signal });
        const responseData = response.data?.data || response.data;

        if (responseData && typeof responseData === 'object') {
            return responseData;
        } else {
            throw new Error('Invalid response data');
        }
    } catch (err) {
        if (err.name !== 'CanceledError') {
            console.error('Fetch policy error:', err);
        }
        throw err;
    }
};

export const updatePasswordPolicy = async (policy) => {
    try {
        const policyId = policy.id || policy.policyId;
        if (!policyId) {
            throw new Error('Policy ID is missing. Cannot update policy.');
        }
        const response = await apiClient.put(`/PasswordPolicy/${policyId}`, policy);
        return response.data?.data || response.data;
    } catch (err) {
        console.error('Update policy error:', err);
        throw err;
    }
};
