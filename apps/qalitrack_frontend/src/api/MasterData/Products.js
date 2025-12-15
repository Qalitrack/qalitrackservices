import { apiClient } from "../helpers/apiClients";

/**
 * Normalize API responses safely
 */
const extractData = (response) => {
  if (!response?.data) return null;
  return response.data.items ?? response.data;
};

/**
 * Handle API errors consistently
 */
const handleError = (error) => {
  const message =
    error?.response?.data?.message ||
    error?.message ||
    "Something went wrong";
  throw new Error(message);
};

// ─────────────────────────────────────────────────────────────
// PRODUCTS API
// ─────────────────────────────────────────────────────────────

// ✅ Fetch products (paginated + searchable)
export const getProducts = async (
  pageNumber = 1,
  pageSize = 10,
  searchTerm = "",
  signal
) => {
  try {
    const response = await apiClient.get("/MasterData/Products", {
      params: { pageNumber, pageSize, searchTerm },
      signal,
    });

    return {
      items: extractData(response),
      totalCount: response.data?.totalCount,
      pageNumber,
      pageSize,
    };
  } catch (error) {
    handleError(error);
  }
};

// ✅ Create product
export const createProduct = async (data) => {
  try {
    const response = await apiClient.post("/MasterData/Products", data);
    return response.data;
  } catch (error) {
    handleError(error);
  }
};

// ✅ Get product by ID
export const getProductById = async (id) => {
  try {
    const response = await apiClient.get(`/MasterData/Products/${id}`);
    return response.data;
  } catch (error) {
    handleError(error);
  }
};

// ✅ Update product
export const updateProduct = async (id, data) => {
  try {
    const response = await apiClient.put(
      `/MasterData/Products/${id}`,
      data
    );
    return response.data ?? { id, ...data };
  } catch (error) {
    handleError(error);
  }
};

// ✅ Delete product
export const deleteProduct = async (id) => {
  try {
    await apiClient.delete(`/MasterData/Products/${id}`);
    return id;
  } catch (error) {
    handleError(error);
  }
};
