/**
 * api/MasterData/Drivers.js
 *
 * Driver API helpers including NFC card lookup
 * Mirrors Vehicle RFID pattern: getDriverByNfc() for kiosk authentication
 */

import { apiClient } from "../helpers/apiClients";

const BASE_PATH = "/MasterData/Drivers";

/**
 * Get paginated list of drivers
 * @param {Object} params - { pageNumber, pageSize, searchTerm }
 */
export const getDrivers = async (params = {}) => {
  const { pageNumber = 1, pageSize = 50, searchTerm = "" } = params;
  
  const queryParams = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
    ...(searchTerm ? { searchTerm } : {}),
  });

  try {
    console.log(`📡 Fetching drivers from API: ${BASE_PATH}?${queryParams}`);
    const response = await apiClient.get(`${BASE_PATH}?${queryParams}`);
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

/**
 * Get single driver by ID
 */
export const getDriverById = async (id) => {
  try {
    console.log(`📡 GET ${BASE_PATH}/${id}`);
    const response = await apiClient.get(`${BASE_PATH}/${id}`);
    console.log("✅ Driver fetched:", response.data);
    return response.data;
  } catch (error) {
    console.error(`❌ Error fetching driver ${id}:`, error.message);
    throw error;
  }
};

/**
 * Get driver by NFC card UID
 * Used by kiosk for driver authentication via NFC tap
 * Mirrors getVehicleByRfid() pattern
 *
 * @param {string} nfcCode - NFC card UID (e.g. "3CD2FF9D")
 * @returns {Promise} Driver data with all fields
 */
export const getDriverByNfc = async (nfcCode) => {
  if (!nfcCode || typeof nfcCode !== "string") {
    throw new Error("NFC code is required and must be a string");
  }

  const cleanCode = nfcCode.trim().toUpperCase();
  
  try {
    console.log(`📡 GET ${BASE_PATH}/nfc/${cleanCode}`);
    const response = await apiClient.get(`${BASE_PATH}/nfc/${cleanCode}`);
    console.log("✅ Driver found by NFC:", response.data);
    return response.data;
  } catch (error) {
    console.error(`❌ Driver NFC lookup failed for ${cleanCode}:`, error.message);
    if (error.response?.status === 404) {
      throw new Error(`No driver registered with NFC card: ${cleanCode}`);
    }
    throw error;
  }
};

/**
 * Create new driver
 */
export const createDriver = async (data) => {
  try {
    console.log("📝 Creating driver:", data);
    const response = await apiClient.post(BASE_PATH, data);
    console.log("✅ Driver created successfully:", response.data);
    return response.data;
  } catch (error) {
    console.error("❌ Error creating driver:", error.message);
    if (error.response) {
      console.error("🔢 Status:", error.response.status);
      console.error("📦 Response data:", error.response.data);
    }
    throw error;
  }
};

/**
 * Update existing driver
 */
export const updateDriver = async (id, data) => {
  try {
    console.log(`✏️ Updating driver ${id}:`, data);
    const response = await apiClient.put(`${BASE_PATH}/${id}`, data);
    console.log("✅ Driver updated successfully:", response.data);
    return response.data;
  } catch (error) {
    console.error(`❌ Error updating driver ${id}:`, error.message);
    if (error.response) {
      console.error("🔢 Status:", error.response.status);
      console.error("📦 Response data:", error.response.data);
    }
    throw error;
  }
};

/**
 * Delete driver
 */
export const deleteDriver = async (id) => {
  try {
    console.log(`🗑️ Deleting driver ID: ${id}`);
    const response = await apiClient.delete(`${BASE_PATH}/${id}`);
    console.log("✅ Driver deleted successfully");
    return response.data;
  } catch (error) {
    console.error(`❌ Error deleting driver ${id}:`, error.message);
    if (error.response) {
      console.error("🔢 Status:", error.response.status);
      console.error("📦 Response data:", error.response.data);
    }
    throw error;
  }
};

/**
 * Assign driver to vehicle
 */
export const assignDriverToVehicle = async (driverId, vehicleId) => {
  try {
    console.log(`📡 POST ${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    console.log("✅ Driver assigned to vehicle");
    return response.data;
  } catch (error) {
    console.error("❌ Error assigning driver to vehicle:", error.message);
    throw error;
  }
};

/**
 * Unassign driver from vehicle
 */
export const unassignDriverFromVehicle = async (driverId, vehicleId) => {
  try {
    console.log(`📡 DELETE ${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    console.log("✅ Driver unassigned from vehicle");
    return response.data;
  } catch (error) {
    console.error("❌ Error unassigning driver from vehicle:", error.message);
    throw error;
  }
};

/**
 * Assign driver to supplier
 */
export const assignDriverToSupplier = async (driverId, supplierId) => {
  try {
    console.log(`📡 POST ${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    console.log("✅ Driver assigned to supplier");
    return response.data;
  } catch (error) {
    console.error("❌ Error assigning driver to supplier:", error.message);
    throw error;
  }
};

/**
 * Unassign driver from supplier
 */
export const unassignDriverFromSupplier = async (driverId, supplierId) => {
  try {
    console.log(`📡 DELETE ${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    console.log("✅ Driver unassigned from supplier");
    return response.data;
  } catch (error) {
    console.error("❌ Error unassigning driver from supplier:", error.message);
    throw error;
  }
};

/**
 * Assign driver to transporter
 */
export const assignDriverToTransporter = async (driverId, transporterId) => {
  try {
    console.log(`📡 POST ${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    console.log("✅ Driver assigned to transporter");
    return response.data;
  } catch (error) {
    console.error("❌ Error assigning driver to transporter:", error.message);
    throw error;
  }
};

/**
 * Unassign driver from transporter
 */
export const unassignDriverFromTransporter = async (driverId, transporterId) => {
  try {
    console.log(`📡 DELETE ${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    console.log("✅ Driver unassigned from transporter");
    return response.data;
  } catch (error) {
    console.error("❌ Error unassigning driver from transporter:", error.message);
    throw error;
  }
};