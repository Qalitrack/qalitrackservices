import apiClient from "../../helpers/apiClients";

// 🔹 Generic error handler
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    console.error("❌ API Error:", error.response?.data || error.message);
    throw new Error(
      error.response?.data?.message ||
        (typeof error.response?.data === "string"
          ? error.response.data
          : error.message)
    );
  }
};

// ✅ Create new weighing transaction
export const createWeighingTransaction = (transactionData) =>
  handleRequest(apiClient.post("/Weighing/Transactions", transactionData));

// ✅ Get all weighing transactions
export const getAllWeighingTransactions = (pageNumber = 1, pageSize = 50) =>
  handleRequest(
    apiClient.get("/Weighing/Transactions", {
      params: { pageNumber, pageSize },
    })
  );

// ✅ Get a transaction by ID
export const getWeighingTransactionById = (id) =>
  handleRequest(apiClient.get(`/Weighing/Transactions/${id}`));

// ✅ Update a transaction (e.g., after W2 or net weight is recorded)
export const updateWeighingTransaction = (id, updatedData) =>
  handleRequest(apiClient.put(`/Weighing/Transactions/${id}`, updatedData));

// ✅ Delete a transaction
export const deleteWeighingTransaction = (id) =>
  handleRequest(apiClient.delete(`/Weighing/Transactions/${id}`));

// ✅ Complete weighing (record W2)
export const completeWeighingTransaction = (id, w2) =>
  handleRequest(apiClient.post(`/Weighing/Transactions/${id}/complete`, { w2 }));
