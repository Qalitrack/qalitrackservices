// src/services/apiClient.js
const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:4000';

async function request(path, { method = 'GET', body } = {}) {
  const opts = {
    method,
    headers: { 'Content-Type': 'application/json' }
  };
  if (body !== undefined) opts.body = JSON.stringify(body);

  const res = await fetch(`${BASE_URL}${path}`, opts);
  if (!res.ok) {
    const msg = await res.text().catch(() => res.statusText);
    throw new Error(msg || `HTTP ${res.status}`);
  }
  if (res.status === 204) return null;
  return res.json();
}

export const api = {
  listTransactions: () => request('/weighing', { method: 'GET' }),
  createTransaction: (payload) => request('/weighing', { method: 'POST', body: payload }),
  completeTransaction: (id, payload) => request(`/weighing/${id}/complete`, { method: 'PATCH', body: payload }),
  deleteTransaction: (id) => request(`/weighing/${id}`, { method: 'DELETE' })
};
