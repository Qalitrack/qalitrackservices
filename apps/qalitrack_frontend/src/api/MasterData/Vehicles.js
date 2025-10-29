// src/api/MasterData/Vehicles.js
import { apiClient } from "../helpers/apiClients"; // ✅ must be named import (matches your setup)

// 🔹 Centralized request wrapper
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    // Normalize response to always return array or object data cleanly
    return response.data?.items || response.data;
  } catch (error) {
    const message =
      error.response?.data?.message ||
      (typeof error.response?.data === "string"
        ? error.response.data
        : error.message);
    console.error("🚨 Vehicle API Error:", message);
    throw new Error(message);
  }
};

// ✅ Fetch all vehicles (paginated)
export const getVehicles = (pageNumber = 1, pageSize = 50, searchTerm = "") =>
  handleRequest(
    apiClient.get("/MasterData/Vehicles", {
      params: { pageNumber, pageSize, searchTerm },
    })
  );

// ✅ Create a new vehicle
export const createVehicle = (vehicleData) =>
  handleRequest(apiClient.post("/MasterData/Vehicles", vehicleData));

// ✅ Fetch single vehicle by ID
export const getVehicleById = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}`));

// ✅ Update vehicle
export const updateVehicle = (id, vehicleData) =>
  handleRequest(apiClient.put(`/MasterData/Vehicles/${id}`, vehicleData));

// ✅ Delete vehicle
export const deleteVehicle = (id) =>
  handleRequest(apiClient.delete(`/MasterData/Vehicles/${id}`));

// ✅ Update vehicle status (Active/Inactive)
export const updateVehicleStatus = (id, status) =>
  handleRequest(apiClient.post(`/MasterData/Vehicles/${id}/status`, { status }));

// ✅ Get all drivers assigned to a vehicle
export const getVehicleDrivers = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}/drivers`));
