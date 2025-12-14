// AxleConfigs.js
import { apiClient } from "../helpers/apiClients";

// Fetch all axle configurations
export const getAxleConfigs = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/AxleConfigurations", {
    params: { pageNumber, pageSize, searchTerm },
  });
  console.log(response.data);
  return response.data;
};

// Create new axle configuration
export const createAxleConfig = async (data) => {
  const response = await apiClient.post("/MasterData/AxleConfigurations", data);
  return response.data;
};

// Get axle configuration by ID
export const getAxleConfigById = async (id) => {
  const response = await apiClient.get(`/MasterData/AxleConfigurations/${id}`);
  return response.data;
};

// Update axle configuration
export const updateAxleConfig = async (id, data) => {
  const response = await apiClient.put(`/MasterData/AxleConfigurations/${id}`, data);
  return response.data;
};

// Delete axle configuration
export const deleteAxleConfig = async (id) => {
  const response = await apiClient.delete(`/MasterData/AxleConfigurations/${id}`);
  return response.data;
};

// Toggle status
export const toggleAxleConfigStatus = async (id, isActive) => {
  const response = await apiClient.patch(`/MasterData/AxleConfigurations/${id}/status`, isActive);
  return response.data;
};