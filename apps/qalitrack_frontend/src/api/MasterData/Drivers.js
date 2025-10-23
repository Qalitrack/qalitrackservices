// src/api/MasterData/Drivers.js
import { apiClient } from "../helpers/apiClients";

// ✅ Get all drivers
export const getDrivers = async () => {
  const response = await apiClient.get("/MasterData/Drivers");
  // Ensure we only return an array
  return response.data?.data || response.data || [];
};

// ✅ Get driver by ID
export const getDriverById = async (id) => {
  const response = await apiClient.get(`/MasterData/Drivers/${id}`);
  return response.data?.data || response.data;
};

// ✅ Create a new driver
export const createDriver = async (payload) => {
  const response = await apiClient.post("/MasterData/Drivers", payload);
  return response.data;
};

// ✅ Update an existing driver
export const updateDriver = async (id, payload) => {
  const response = await apiClient.put(`/MasterData/Drivers/${id}`, payload);
  return response.data;
};

// ✅ Delete a driver
export const deleteDriver = async (id) => {
  const response = await apiClient.delete(`/MasterData/Drivers/${id}`);
  return response.data;
};
