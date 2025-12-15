// src/api/MasterData/Transporters.js
import { apiClient } from "../helpers/apiClients";

const BASE_URL = "/MasterData/Transporters";

/**
 * ✅ Fetch paginated transporters
 */
export const getTransporters = async ({
  pageNumber = 1,
  pageSize = 10,
  search = "",
} = {}) => {
  const { data } = await apiClient.get(BASE_URL, {
    params: { pageNumber, pageSize, searchTerm: search },
  });

  return {
    items: data?.items ?? data?.data?.items ?? [],
    totalItems:
      data?.totalItems ??
      data?.data?.totalItems ??
      data?.items?.length ??
      0,
  };
};

/**
 * ✅ Get transporter by ID
 */
export const getTransporterById = async (id) => {
  const { data } = await apiClient.get(`${BASE_URL}/${id}`);
  return data;
};

/**
 * ✅ Create transporter
 */
export const createTransporter = async (payload) => {
  const { data } = await apiClient.post(BASE_URL, payload);
  return data;
};

/**
 * ✅ Update transporter
 */
export const updateTransporter = async (id, payload) => {
  const { data } = await apiClient.put(`${BASE_URL}/${id}`, payload);
  return data;
};

/**
 * ✅ Delete transporter
 */
export const deleteTransporter = async (id) => {
  await apiClient.delete(`${BASE_URL}/${id}`);
  return id;
};
