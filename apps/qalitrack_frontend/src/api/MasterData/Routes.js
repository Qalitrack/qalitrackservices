import { apiClient } from "../helpers/apiClients";

// ✅ Fetch routes
export const getRoutes = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Routes", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data.items || response.data;
};

// ✅ CRUD operations
export const createRoute = async (data) => apiClient.post("/MasterData/Routes", data).then(r => r.data);
export const getRouteById = async (id) => apiClient.get(`/MasterData/Routes/${id}`).then(r => r.data);
export const updateRoute = async (id, data) => apiClient.put(`/MasterData/Routes/${id}`, data).then(r => r.data);
export const deleteRoute = async (id) => apiClient.delete(`/MasterData/Routes/${id}`).then(r => r.data);
