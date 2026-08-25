import { apiClient } from '../../apiClients.js';

export const fetchEmailSettings = async (signal) => {
    const response = await apiClient.get('/EmailSettings', { signal });
    return response.data?.data || response.data;
};

// `settings.smtpPassword` should be omitted/blank to keep the currently saved password.
export const updateEmailSettings = async (settings) => {
    const response = await apiClient.put('/EmailSettings', settings);
    return response.data?.data || response.data;
};

export const sendTestEmail = async (toEmail) => {
    const response = await apiClient.post('/EmailSettings/test', { toEmail });
    return response.data?.data || response.data;
};
