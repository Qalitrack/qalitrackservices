// src/api/MasterData/Drivers.js
import { apiClient } from "../helpers/apiClients";

// ✅ Get all drivers
export const getDrivers = async () => {
  try {
    console.log("📡 Fetching drivers from API...");
    const response = await apiClient.get("/MasterData/Drivers?pageNumber=1&pageSize=50");
    console.log("✅ Drivers fetched successfully:", response.data);
    return response.data;
  } catch (error) {
    console.error("❌ Error fetching drivers:", error.message);
    if (error.response) {
      console.error("🔢 Status:", error.response.status);
      console.error("📦 Response data:", error.response.data);
    }
    throw error;
  }
};

// ✅ Create driver
export const createDriver = async (data) => {
  try {
    console.log("📝 Creating driver:", data);
    const response = await apiClient.post("/MasterData/Drivers", data);
    console.log("✅ Driver created successfully:", response.data);
    return response.data;
  } catch (error) {
    console.error("❌ Error creating driver:", error.message);
    throw error;
  }
};

// ✅ Update driver
export const updateDriver = async (id, data) => {
  try {
    console.log(`✏️ Updating driver ${id}:`, data);
    const response = await apiClient.put(`/MasterData/Drivers/${id}`, data);
    console.log("✅ Driver updated successfully:", response.data);
    return response.data;
  } catch (error) {
    console.error("❌ Error updating driver:", error.message);
    throw error;
  }
};

// ✅ Delete driver
export const deleteDriver = async (id) => {
  try {
    console.log(`🗑️ Deleting driver ID: ${id}`);
    const response = await apiClient.delete(`/MasterData/Drivers/${id}`);
    console.log("✅ Driver deleted successfully");
    return response.data;
  } catch (error) {
    console.error("❌ Error deleting driver:", error.message);
    throw error;
  }
};
