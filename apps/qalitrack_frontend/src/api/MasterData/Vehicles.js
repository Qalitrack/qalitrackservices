import apiClient from "../helpers/apiClients"; // uses your axios instance

// ✅ Fetch paginated vehicles
export const getVehicles = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Vehicles", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data;
};

// ✅ Create new vehicle
export const createVehicle = async (vehicleData) => {
  const response = await apiClient.post("/MasterData/Vehicles", vehicleData);
  return response.data;
};

// ✅ Get vehicle by ID
export const getVehicleById = async (id) => {
  const response = await apiClient.get(`/MasterData/Vehicles/${id}`);
  return response.data;
};

// ✅ Update existing vehicle
export const updateVehicle = async (id, vehicleData) => {
  const response = await apiClient.put(`/MasterData/Vehicles/${id}`, vehicleData);
  return response.data;
};

// ✅ Delete a vehicle
export const deleteVehicle = async (id) => {
  const response = await apiClient.delete(`/MasterData/Vehicles/${id}`);
  return response.data;
};

// ✅ Update vehicle status
export const updateVehicleStatus = async (id, status) => {
  const response = await apiClient.post(`/MasterData/Vehicles/${id}/status`, { status });
  return response.data;
};

// ✅ Get assigned drivers for a vehicle
export const getVehicleDrivers = async (id) => {
  const response = await apiClient.get(`/MasterData/Vehicles/${id}/drivers`);
  return response.data;
};
