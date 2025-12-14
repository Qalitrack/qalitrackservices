// transporterApi.js
import { apiClient } from "../helpers/apiClients";

// ✅ Fetch paginated transporters
export const getTransporters = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Transporters", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data;
};

// ✅ Create new transporter
export const createTransporter = async (data) => {
  const response = await apiClient.post("/MasterData/Transporters", data);
  return response.data;
};

// ✅ Get transporter by ID
export const getTransporterById = async (id) => {
  const response = await apiClient.get(`/MasterData/Transporters/${id}`);
  return response.data;
};

// ✅ Update transporter
export const updateTransporter = async (id, data) => {
  const response = await apiClient.put(`/MasterData/Transporters/${id}`, data);
  return response.data;
};

// ✅ Delete transporter
export const deleteTransporter = async (id) => {
  const response = await apiClient.delete(`/MasterData/Transporters/${id}`);
  return response.data;
};