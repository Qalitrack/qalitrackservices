import { apiClient } from "../helpers/apiClients";

// ✅ Fetch suppliers
export const getSuppliers = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Suppliers", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data.items || response.data;
};

// ✅ Create new supplier
export const createSupplier = async (data) => {
  const response = await apiClient.post("/MasterData/Suppliers", data);
  return response.data;
};

// ✅ Get supplier by ID
export const getSupplierById = async (id) => {
  const response = await apiClient.get(`/MasterData/Suppliers/${id}`);
  return response.data;
};

// ✅ Update supplier
export const updateSupplier = async (id, data) => {
  const response = await apiClient.put(`/MasterData/Suppliers/${id}`, data);
  return response.data;
};

// ✅ Delete supplier
export const deleteSupplier = async (id) => {
  const response = await apiClient.delete(`/MasterData/Suppliers/${id}`);
  return response.data;
};
