import { apiClient } from "../helpers/apiClients";

// ✅ Fetch products
export const getProducts = async (pageNumber = 1, pageSize = 10, searchTerm = "") => {
  const response = await apiClient.get("/MasterData/Products", {
    params: { pageNumber, pageSize, searchTerm },
  });
  return response.data.items || response.data;
};

// ✅ Create product
export const createProduct = async (data) => {
  const response = await apiClient.post("/MasterData/Products", data);
  return response.data;
};

// ✅ Get product by ID
export const getProductById = async (id) => {
  const response = await apiClient.get(`/MasterData/Products/${id}`);
  return response.data;
};

// ✅ Update product
export const updateProduct = async (id, data) => {
  await apiClient.put(`/MasterData/Products/${id}`, data);
  return { id, ...data };
};

// ✅ Delete product
export const deleteProduct = async (id) => {
  await apiClient.delete(`/MasterData/Products/${id}`);
  return id;
};
