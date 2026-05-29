// src/api/MasterData/AxleConfigs.js
import { apiClient } from "../helpers/apiClients";

const BASE_PATH = "/MasterData/AxleConfigurations";

// Fetch all axle configurations
export const getAxleConfigs = async (pageNumber = 1, pageSize = 100, searchTerm = "") => {
  try {
    
    const response = await apiClient.get(BASE_PATH, {
      params: { 
        pageNumber, 
        pageSize, 
        searchTerm: searchTerm || undefined 
      }
    });
    
    
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Create new axle configuration
export const createAxleConfig = async (data) => {
  const response = await apiClient.post(BASE_PATH, data);
  return response.data;
};

// Get axle configuration by ID
export const getAxleConfigById = async (id) => {
  const response = await apiClient.get(`${BASE_PATH}/${id}`);
  return response.data;
};

// Update axle configuration
export const updateAxleConfig = async (id, data) => {
  const response = await apiClient.put(`${BASE_PATH}/${id}`, data);
  return response.data;
};

// Delete axle configuration
export const deleteAxleConfig = async (id) => {
  const response = await apiClient.delete(`${BASE_PATH}/${id}`);
  return response.data;
};

// Toggle status
export const toggleAxleConfigStatus = async (id, isActive) => {
  const response = await apiClient.patch(`${BASE_PATH}/${id}/status`, isActive);
  return response.data;
};