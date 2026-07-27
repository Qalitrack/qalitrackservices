// src/api/MasterData/Vehicles.js
import { apiClient } from "../helpers/apiClients";

// 🔹 Centralized request wrapper
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    
    // ✅ Return the full response.data (which contains items, pageNumber, etc.)
    return response.data;
  } catch (error) {
    // ✅ Log everything for debugging
    
    // ✅ Extract detailed error message
    let message = "Request failed";
    
    if (error.response?.data) {
      const errData = error.response.data;
      
      // Check for validation errors (check both lowercase 'errors' and any object with array values)
      if (errData.errors || typeof errData === 'object') {
        // ASP.NET Core validation errors - can be under 'errors' key or directly in response
        const errorObj = errData.errors || errData;
        
        if (typeof errorObj === 'object' && errorObj !== null) {
          const errorMessages = [];
          
          for (const [field, messages] of Object.entries(errorObj)) {
            if (Array.isArray(messages)) {
              // Field validation errors
              errorMessages.push(`${field}: ${messages.join(", ")}`);
            }
          }
          
          if (errorMessages.length > 0) {
            message = errorMessages.join("; ");
          }
        }
      }
      
      // Fallback to other error formats if no validation errors found
      if (message === "Request failed") {
        if (errData.title && errData.detail) {
          // Problem details format
          message = `${errData.title}: ${errData.detail}`;
        } else if (errData.message) {
          message = errData.message;
        } else if (typeof errData === "string") {
          message = errData;
        } else if (errData.title) {
          message = errData.title;
        } else {
          message = `${error.response.status}: ${error.response.statusText}`;
        }
      }
    } else if (error.message) {
      message = error.message;
    }
    
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
export const createVehicle = (vehicleData) => {
  return handleRequest(apiClient.post("/MasterData/Vehicles", vehicleData));
};

// ✅ Fetch single vehicle by ID
export const getVehicleById = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}`));

// ✅ Update vehicle (includes rfiDcode field)
export const updateVehicle = (id, vehicleData) => {
  return handleRequest(apiClient.put(`/MasterData/Vehicles/${id}`, vehicleData));
};

// ✅ Delete vehicle
export const deleteVehicle = (id) =>
  handleRequest(apiClient.delete(`/MasterData/Vehicles/${id}`));

// ✅ Get vehicle by RFID code
export const getVehicleByRfid = (rfidCode) => {
  return handleRequest(apiClient.get(`/MasterData/Vehicles/rfid/${rfidCode}`));
};

// ✅ Get all drivers assigned to a vehicle
export const getVehicleDrivers = (id) =>
  handleRequest(apiClient.get(`/MasterData/Vehicles/${id}/drivers`));

// ✅ Update vehicle status (Active/Inactive)
export const updateVehicleStatus = (id, status) => {
  return handleRequest(apiClient.post(`/MasterData/Vehicles/${id}/status`, { status }));
};

// ✅ Patch a single relationship field (transporterId or supplierId) on a
// vehicle. There's no dedicated backend endpoint for this (unlike owners,
// which has /vehicles/assign) — the update endpoint requires the full
// vehicle payload, so this fetches the current record and resubmits it with
// just that one field changed. Pass `null` to clear the relationship.
export const patchVehicleRelation = async (vehicleId, field, value) => {
  const response = await getVehicleById(vehicleId);
  const vehicle = response?.data || response;

  const payload = {
    registrationNumber: vehicle.registrationNumber,
    type: vehicle.type,
    status: vehicle.status || "Active",
    ownerId: vehicle.ownerId,
    axleConfigurationId: vehicle.axleConfigurationId,
    supplierId: vehicle.supplierId || undefined,
    transporterId: vehicle.transporterId || undefined,
    make: vehicle.make || undefined,
    model: vehicle.model || undefined,
    color: vehicle.color || undefined,
    chassisNumber: vehicle.chassisNumber || undefined,
    engineNumber: vehicle.engineNumber || undefined,
    vehicleClass: vehicle.vehicleClass || undefined,
    bodyType: vehicle.bodyType || undefined,
    insurancePolicyNumber: vehicle.insurancePolicyNumber || undefined,
    roadWorthinessNumber: vehicle.roadWorthinessNumber || undefined,
    yearOfManufacture: vehicle.yearOfManufacture || undefined,
    grossWeight: vehicle.grossWeight || undefined,
    tareWeight: vehicle.tareWeight || undefined,
    netWeightCapacity: vehicle.netWeightCapacity || undefined,
    seatingCapacity: vehicle.seatingCapacity || undefined,
    fuelTankCapacity: vehicle.fuelTankCapacity || undefined,
    insuranceExpiryDate: vehicle.insuranceExpiryDate || undefined,
    roadWorthinessExpiryDate: vehicle.roadWorthinessExpiryDate || undefined,
    driverIds: vehicle.driverIds?.length ? vehicle.driverIds : undefined,
    [field]: value ?? null,
  };

  return updateVehicle(vehicleId, payload);
};