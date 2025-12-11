// src/helpers/transactionHelper.js
import { transactionsClient } from "./apiClients";

// 🔹 Generic error handler
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    console.error("❌ Transaction API Error:", error.response?.data || error.message);
    throw new Error(
      error.response?.data?.message ||
        (typeof error.response?.data === "string"
          ? error.response.data
          : error.message)
    );
  }
};

// ================== TRANSACTION CRUD ==================

// Create a new transaction
export const createTransaction = (transactionData) =>
  handleRequest(transactionsClient.post("/", transactionData));

// Get all transactions with optional filters/pagination
export const getTransactions = (params = {}) =>
  handleRequest(transactionsClient.get("/", { params }));

// Get a transaction by ID
export const getTransactionById = (id) =>
  handleRequest(transactionsClient.get(`/${id}`));

// Update a transaction
export const updateTransaction = (id, updatedData) =>
  handleRequest(transactionsClient.put(`/${id}`, updatedData));

// Delete a transaction
export const deleteTransaction = (id) =>
  handleRequest(transactionsClient.delete(`/${id}`));

// Get transaction by receipt number
export const getTransactionByReceipt = (receiptNo) =>
  handleRequest(transactionsClient.get(`/receipt/${receiptNo}`));

// ================== WEIGHING RECORDS ==================

// Get weighing records for a transaction
export const getWeighingRecords = (transactionId) =>
  handleRequest(transactionsClient.get(`/${transactionId}/weighing-records`));

// Add a weighing to a transaction
export const addWeighing = (weighingData) =>
  handleRequest(transactionsClient.post("/add-weighing", weighingData));

// Complete weighing for a transaction
export const completeTransaction = (transactionId) =>
  handleRequest(transactionsClient.post("/complete", { transactionId }));

// ================== REWEIGH OPERATIONS ==================

export const requestReweigh = (data) =>
  handleRequest(transactionsClient.post("/request-reweigh", data));

export const startReweigh = (transactionId, data) =>
  handleRequest(transactionsClient.post(`/${transactionId}/start-reweigh`, data));

export const addReweighWeight = (data) =>
  handleRequest(transactionsClient.post("/add-reweigh-weight", data));

export const completeReweigh = (data) =>
  handleRequest(transactionsClient.post("/complete-reweigh", data));

export const getReweighRecords = (transactionId) =>
  handleRequest(transactionsClient.get(`/${transactionId}/reweigh-records`));

// ================== STATUS & CHECKS ==================

export const getTransactionsByStatus = (status, limit = 100) =>
  handleRequest(transactionsClient.get(`/status/${status}`, { params: { limit } }));

export const checkReceipt = (receiptNo) =>
  handleRequest(transactionsClient.get(`/check-receipt/${receiptNo}`));

// ================== INCOMPLETE TRANSACTIONS ==================

export const getIncompleteByVehicleNo = (noPlate) =>
  handleRequest(transactionsClient.get(`/incomplete/vehicle/${noPlate}`));

export const getIncompleteByVehicleId = (vehicleId) =>
  handleRequest(transactionsClient.get(`/incomplete/vehicle-id/${vehicleId}`));

// ================== AUDIT LOGS ==================

export const getTransactionAuditLogs = (transactionId) =>
  handleRequest(transactionsClient.get(`/${transactionId}/audit-logs`));
