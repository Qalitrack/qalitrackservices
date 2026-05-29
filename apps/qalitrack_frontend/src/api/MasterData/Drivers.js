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
    const response = await apiClient.get(`${BASE_PATH}?${queryParams}`);
    return response.data;
  } catch (error) {
    if (error.response) {
    }
    throw error;
  }
};

/**
 * Get single driver by ID
 */
export const getDriverById = async (id) => {
  try {
    const response = await apiClient.get(`${BASE_PATH}/${id}`);
    return response.data;
  } catch (error) {
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
    const response = await apiClient.get(`${BASE_PATH}/nfc/${cleanCode}`);
    return response.data;
  } catch (error) {
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
    const response = await apiClient.post(BASE_PATH, data);
    return response.data;
  } catch (error) {
    if (error.response) {
    }
    throw error;
  }
};

/**
 * Update existing driver
 */
export const updateDriver = async (id, data) => {
  try {
    const response = await apiClient.put(`${BASE_PATH}/${id}`, data);
    return response.data;
  } catch (error) {
    if (error.response) {
    }
    throw error;
  }
};

/**
 * Delete driver
 */
export const deleteDriver = async (id) => {
  try {
    const response = await apiClient.delete(`${BASE_PATH}/${id}`);
    return response.data;
  } catch (error) {
    if (error.response) {
    }
    throw error;
  }
};

/**
 * Assign driver to vehicle
 */
export const assignDriverToVehicle = async (driverId, vehicleId) => {
  try {
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * Unassign driver from vehicle
 */
export const unassignDriverFromVehicle = async (driverId, vehicleId) => {
  try {
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/vehicles/${vehicleId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * Assign driver to supplier
 */
export const assignDriverToSupplier = async (driverId, supplierId) => {
  try {
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * Unassign driver from supplier
 */
export const unassignDriverFromSupplier = async (driverId, supplierId) => {
  try {
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/suppliers/${supplierId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * Assign driver to transporter
 */
export const assignDriverToTransporter = async (driverId, transporterId) => {
  try {
    const response = await apiClient.post(`${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * Unassign driver from transporter
 */
export const unassignDriverFromTransporter = async (driverId, transporterId) => {
  try {
    const response = await apiClient.delete(`${BASE_PATH}/${driverId}/transporters/${transporterId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};