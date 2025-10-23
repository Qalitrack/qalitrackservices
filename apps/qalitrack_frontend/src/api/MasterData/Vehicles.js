import apiClient from "../helpers/apiClients";

// 🔹 Unified error handler
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    const message =
      error.response?.data?.message ||
      (typeof error.response?.data === "string"
        ? error.response.data
        : error.message);
    throw new Error(message);
  }
};

// ✅ Get all (paginated) vehicles
export const getVehicles = (pageNumber = 1, pageSize = 50, searchTerm = "") =>
  handleRequest(
    apiClient.get("/MasterData/Vehicles", {
      params: { pageNumber, pageSize, searchTerm },
    })
  );

// ✅ Create new vehicle
export const createVehicle = (vehicleData) =>
  handleRequest(apiClient.post("/MasterData/Vehicles", vehicleData));

// ✅ Get vehicle by ID
export const getVehicleById = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}`));

// ✅ Update existing vehicle
export const updateVehicle = (id, vehicleData) =>
  handleRequest(apiClient.put(`/MasterData/Vehicles/${id}`, vehicleData));

// ✅ Delete vehicle
export const deleteVehicle = (id) =>
  handleRequest(apiClient.delete(`/MasterData/Vehicles/${id}`));

// ✅ Update vehicle status
export const updateVehicleStatus = (id, status) =>
  handleRequest(apiClient.post(`/MasterData/Vehicles/${id}/status`, { status }));

// ✅ Get assigned drivers for a vehicle
export const getVehicleDrivers = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}/drivers`));
