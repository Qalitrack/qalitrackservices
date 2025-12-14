import { apiClient } from "../helpers/apiClients";

// Base path for owners API - matches the backend route
// The proxy will handle the /api prefix and forward to the correct backend URL
const BASE_PATH = "/MasterData/Owners";

// Log the API client configuration for debugging
console.log('API Client base URL:', apiClient.client.defaults.baseURL);

// Fetch all owners with pagination and search
export const getOwners = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  try {
    console.log(`Fetching owners with params:`, { 
      pageNumber, 
      pageSize, 
      searchTerm,
      baseURL: apiClient.client.defaults.baseURL,
      fullPath: `${apiClient.client.defaults.baseURL}${BASE_PATH}`
    });
    
    // Make the API request with a timeout
    const response = await Promise.race([
      apiClient.get(BASE_PATH, {
        params: { 
          pageNumber, 
          pageSize, 
          searchTerm: searchTerm || undefined 
        },
        validateStatus: (status) => status < 500 // Don't throw for 4xx errors
      }),
      new Promise((_, reject) => 
        setTimeout(() => reject(new Error('Request timeout')), 10000)
      )
    ]);
    
    console.log('API Response:', {
      status: response.status,
      statusText: response.statusText,
      data: response.data,
      headers: response.headers
    });
    
    // Handle different response statuses
    if (response.status >= 400) {
      const error = new Error(response.statusText || 'API request failed');
      error.status = response.status;
      error.data = response.data;
      throw error;
    }
    
    // Handle different response structures
    if (response?.data) {
      return response.data;
    }
    
    // If response is an array, wrap it in the expected format
    if (Array.isArray(response)) {
      return {
        items: response,
        totalItems: response.length,
        pageNumber: 1,
        pageSize: response.length
      };
    }
    
    // If we get here, the response format is unexpected
    console.error('Unexpected API response format:', response);
    throw new Error('Unexpected response format from server');
    
  } catch (error) {
    console.error('Detailed error in getOwners:', {
      name: error.name,
      message: error.message,
      stack: error.stack,
      status: error.status,
      code: error.code,
      response: error.response ? {
        status: error.response.status,
        statusText: error.response.statusText,
        data: error.response.data,
        headers: error.response.headers
      } : undefined,
      config: error.config ? {
        url: error.config.url,
        method: error.config.method,
        baseURL: error.config.baseURL,
        params: error.config.params,
        headers: error.config.headers
      } : undefined
    });
    
    // Create a more descriptive error message
    let errorMessage = 'Failed to fetch owners';
    if (error.message === 'Network Error') {
      errorMessage = 'Network error: Unable to connect to the server. Please check your internet connection.';
    } else if (error.message.includes('timeout')) {
      errorMessage = 'Request timeout: The server took too long to respond.';
    } else if (error.status === 401) {
      errorMessage = 'Session expired. Please log in again.';
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to view owners.';
    } else if (error.status === 404) {
      errorMessage = 'The requested resource was not found.';
    } else if (error.status >= 500) {
      errorMessage = 'Server error: Please try again later.';
    }
    
    const errorObj = new Error(errorMessage);
    errorObj.status = error.status;
    errorObj.code = error.code;
    errorObj.originalError = error;
    throw errorObj;
  }
};

// Create new owner
export const createOwner = async (data) => {
  const response = await apiClient.post(BASE_PATH, data);
  return response.data;
};

// Get owner by ID
export const getOwnerById = async (id) => {
  const response = await apiClient.get(`${BASE_PATH}/${id}`);
  return response.data;
};

// Update owner
export const updateOwner = async (id, data) => {
  const response = await apiClient.put(`${BASE_PATH}/${id}`, data);
  return response.data;
};

// Delete owner
export const deleteOwner = async (id) => {
  const response = await apiClient.delete(`${BASE_PATH}/${id}`);
  return response.data;
};

// Check if owner name is available
export const checkOwnerNameAvailability = async (name, excludeId = null) => {
  const response = await apiClient.get(`${BASE_PATH}/check-name-availability`, {
    params: { name, excludeId },
  });
  return response.data;
};

// Get vehicles for a specific owner
export const getOwnerVehicles = async (ownerId) => {
  const response = await apiClient.get(`${BASE_PATH}/${ownerId}/vehicles`);
  return response.data;
};

// Assign vehicles to owner
export const assignVehiclesToOwner = async (ownerId, vehicleIds) => {
  const response = await apiClient.post(`${BASE_PATH}/${ownerId}/vehicles/assign`, vehicleIds);
  return response.data;
};

// Remove vehicles from owner
export const removeVehiclesFromOwner = async (ownerId, vehicleIds) => {
  const response = await apiClient.post(`${BASE_PATH}/${ownerId}/vehicles/remove`, vehicleIds);
  return response.data;
};