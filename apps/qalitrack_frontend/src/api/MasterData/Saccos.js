import { apiClient } from "../helpers/apiClients";

// ✅ Fetch saccos
export const getSaccos = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/Saccos", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data.items || response.data;
};

// ✅ Create sacco
export const createSacco = async (data) => {
  const response = await apiClient.post("/Saccos", data);
  return response.data;
};

// ✅ Get sacco by ID
export const getSaccoById = async (id) => {
  const response = await apiClient.get(`/Saccos/${id}`);
  return response.data;
};

// ✅ Update sacco
export const updateSacco = async (id, data) => {
  const response = await apiClient.put(`/Saccos/${id}`, data);
  return response.data;
};

// ✅ Delete sacco
export const deleteSacco = async (id) => {
  const response = await apiClient.delete(`/Saccos/${id}`);
  return response.data;
};
