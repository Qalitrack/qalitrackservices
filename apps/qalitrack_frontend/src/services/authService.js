import apiClient from '../apiClients';
import { AUTH } from '../endpoints';

export const login = (email, password) => {
  return apiClient.post(AUTH.LOGIN, { email, password });
};

export const verify2FA = (sessionId, code) => {
  return apiClient.post(AUTH.VERIFY_2FA, { sessionId, code });
};

export const updatePassword = (userId, payload) => {
  return apiClient.put(AUTH.UPDATE_PASSWORD(userId), payload, {
    headers: {
      accept: '*/*',
      'Content-Type': 'application/json',
      Authorization: undefined,
    },
  });
};

export const logout = () => {
  return apiClient.post(AUTH.LOGOUT);
};
