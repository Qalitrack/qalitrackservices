import apiClient from "./apiClient";

export const fetchTransactionMeta = async () => {
  const [vehicles, customers, products, operations] = await Promise.all([
    apiClient.get("/api/Vehicles"),
    apiClient.get("/api/Customers"),
    apiClient.get("/api/Products"),
    apiClient.get("/api/Operations"),
  ]);

  return {
    vehicles: vehicles.data,
    customers: customers.data,
    products: products.data,
    operations: operations.data,
  };
};

export const createTransaction = async (payload) => {
  return apiClient.post("/api/Transaction", {
    request: payload,
  });
};
