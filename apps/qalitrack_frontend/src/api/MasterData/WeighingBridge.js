import { apiClient } from "../helpers/apiClients";

// ✅ Fetch weighbridges with pagination & optional search
export const getWeighbridges = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Weighbridges", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data.items || response.data;
};

// ✅ Create new weighbridge
export const createWeighbridge = async (data) => {
  const response = await apiClient.post("/MasterData/Weighbridges", data);
  return response.data;
};

// ✅ Get weighbridge by ID
export const getWeighbridgeById = async (id) => {
  const response = await apiClient.get(`/MasterData/Weighbridges/${id}`);
  return response.data;
};

// ✅ Update weighbridge
export const updateWeighbridge = async (id, data) => {
  const response = await apiClient.put(`/MasterData/Weighbridges/${id}`, data);
  return response.data;
};

// ✅ Delete weighbridge
export const deleteWeighbridge = async (id) => {
  const response = await apiClient.delete(`/MasterData/Weighbridges/${id}`);
  return response.data;
};

// ✅ Check if location is available
export const checkWeighbridgeLocation = async (location, excludeId = null) => {
  const response = await apiClient.get("/MasterData/Weighbridges/check-location-availability", {
    params: { location, excludeId },
  });
  return response.data;
};
