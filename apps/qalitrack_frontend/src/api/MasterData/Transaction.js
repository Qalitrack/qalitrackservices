// src/api/Transaction/Transactions.js
import { apiClient } from "../helpers/apiClients";

/* -------------------------------------------------------------------------- */
/*                                 BASE PATH                                   */
/* -------------------------------------------------------------------------- */
// ✅ CORRECTED: According to Swagger, the base path is just /Transaction
const BASE = "/Transaction";

// 🔹 Generic error handler
const handleRequest = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    console.error("❌ Transaction API Error:", error);
    
    let errorMessage = "Unknown error occurred";
    
    if (error.response?.data) {
      const data = error.response.data;
      if (typeof data === 'string') {
        errorMessage = data;
      } else if (data.message) {
        errorMessage = data.message;
      } else if (data.errors) {
        errorMessage = Object.values(data.errors).flat().join("; ");
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    throw new Error(errorMessage);
  }
};

/* -------------------------------------------------------------------------- */
/*                           TRANSACTION CRUD                                 */
/* -------------------------------------------------------------------------- */

/**
 * GET /Transaction
 * List/filter transactions with pagination
 * Swagger: Supports filters like ReceiptNo, NoPlate, Status, etc.
 */
export const getTransactions = async (params = {}) => {
  console.log("🔍 Fetching transactions with params:", params);
  return handleRequest(apiClient.get(BASE, { params }));
};

/**
 * POST /Transaction
 * Create a new transaction (first weight)
 * Swagger Schema: CreateTransactionDto
 */
export const createTransaction = async (payload) => {
  console.log("📤 POST /Transaction - Creating transaction:", payload);
  return handleRequest(apiClient.post(BASE, payload));
};

/**
 * GET /Transaction/{ticketId}
 * Get a specific transaction by ID
 */
export const getTransactionById = async (ticketId) => {
  console.log("🔍 GET /Transaction/" + ticketId);
  return handleRequest(apiClient.get(`${BASE}/${ticketId}`));
};

/**
 * PUT /Transaction/{ticketId}
 * Update transaction
 * Swagger Schema: UpdateTransactionDto
 */
export const updateTransaction = async (ticketId, payload) => {
  console.log("📝 PUT /Transaction/" + ticketId, payload);
  return handleRequest(apiClient.put(`${BASE}/${ticketId}`, payload));
};

/**
 * DELETE /Transaction/{ticketId}
 * Delete/deactivate a transaction
 */
export const deleteTransaction = async (ticketId) => {
  console.log("🗑️ DELETE /Transaction/" + ticketId);
  return handleRequest(apiClient.delete(`${BASE}/${ticketId}`));
};

export const deactivateTransactionApi = async (ticketId) => {
  return deleteTransaction(ticketId);
};

/* -------------------------------------------------------------------------- */
/*                      RECEIPT & VEHICLE QUERIES                             */
/* -------------------------------------------------------------------------- */

/**
 * GET /Transaction/receipt/{receiptNo}
 * Get transaction by receipt number
 */
export const getTransactionByReceipt = async (receiptNo) => {
  console.log("🔍 GET /Transaction/receipt/" + receiptNo);
  return handleRequest(apiClient.get(`${BASE}/receipt/${receiptNo}`));
};

/**
 * GET /Transaction/incomplete/vehicle/{noPlate}
 * Get incomplete transactions for a vehicle by plate number
 */
export const getIncompleteByPlate = async (noPlate) => {
  console.log("🔍 GET /Transaction/incomplete/vehicle/" + noPlate);
  return handleRequest(apiClient.get(`${BASE}/incomplete/vehicle/${noPlate}`));
};

export const getIncompleteByVehicleNo = async (noPlate) => {
  return getIncompleteByPlate(noPlate);
};

/**
 * GET /Transaction/incomplete/vehicle-id/{vehicleId}
 * Get incomplete transactions by vehicle ID
 */
export const getIncompleteByVehicleId = async (vehicleId) => {
  console.log("🔍 GET /Transaction/incomplete/vehicle-id/" + vehicleId);
  return handleRequest(apiClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`));
};

/**
 * GET /Transaction/check-receipt/{receiptNo}
 * Check if receipt exists
 */
export const checkReceiptExists = async (receiptNo) => {
  console.log("🔍 GET /Transaction/check-receipt/" + receiptNo);
  return handleRequest(apiClient.get(`${BASE}/check-receipt/${receiptNo}`));
};

export const checkReceipt = async (receiptNo) => {
  return checkReceiptExists(receiptNo);
};

/* -------------------------------------------------------------------------- */
/*                           STATUS QUERIES                                   */
/* -------------------------------------------------------------------------- */

/**
 * GET /Transaction/status/{status}
 * Get transactions by status with optional limit
 */
export const getTransactionsByStatus = async (status, limit = 100) => {
  console.log("🔍 GET /Transaction/status/" + status, { limit });
  return handleRequest(
    apiClient.get(`${BASE}/status/${status}`, {
      params: { limit },
    })
  );
};

/* -------------------------------------------------------------------------- */
/*                         WEIGHING OPERATIONS                                */
/* -------------------------------------------------------------------------- */

/**
 * POST /Transaction/add-second-weight
 * Add second weight to complete weighing
 * Swagger Schema: AddSecondWeightDto
 * Required fields:
 * - ticketID: number
 * - secondWeight: string
 * - weighBridgeName2nd: string
 * - scaleName2nd: string
 * - operatorID2nd: string
 * - operatorName2nd: string
 * - notes: string
 */
export const addSecondWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-second-weight:", payload);
  return handleRequest(apiClient.post(`${BASE}/add-second-weight`, payload));
};

/**
 * Alias for backward compatibility
 * Maps various payload formats to AddSecondWeightDto schema
 */
export const addWeighing = async (payload) => {
  const mappedPayload = {
    ticketID: payload.transactionId || payload.ticketID,
    secondWeight: String(payload.weight || payload.secondWeight || "0"),
    weighBridgeName2nd: payload.weighBridgeName || payload.weighBridgeName2nd || "",
    scaleName2nd: payload.scaleName || payload.scaleName2nd || "Scale-01",
    operatorID2nd: String(payload.operatorId || payload.operatorID2nd || ""),
    operatorName2nd: payload.operatorName || payload.operatorName2nd || "",
    notes: payload.notes || "",
  };
  
  console.log("📤 addWeighing -> mapped payload:", mappedPayload);
  return addSecondWeight(mappedPayload);
};

/**
 * POST /Transaction/complete
 * Mark transaction as complete
 * Swagger Schema: CompleteTransactionDto
 * Required: ticketID
 */
export const completeTransaction = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId || payload.id
  };
  
  console.log("📤 POST /Transaction/complete:", mappedPayload);
  return handleRequest(apiClient.post(`${BASE}/complete`, mappedPayload));
};

/* -------------------------------------------------------------------------- */
/*                         REWEIGH OPERATIONS                                 */
/* -------------------------------------------------------------------------- */

/**
 * POST /Transaction/request-reweigh
 * Request a reweigh for a transaction
 * Swagger Schema: RequestReweighDto
 * Required: ticketID, reason
 */
export const requestReweigh = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId,
    reason: payload.reason || ""
  };
  
  console.log("📤 POST /Transaction/request-reweigh:", mappedPayload);
  return handleRequest(apiClient.post(`${BASE}/request-reweigh`, mappedPayload));
};

/**
 * GET /Transaction/{ticketId}/reweigh-records
 * Get all reweigh records for a transaction
 */
export const getReweighRecords = async (ticketId) => {
  console.log("🔍 GET /Transaction/" + ticketId + "/reweigh-records");
  return handleRequest(apiClient.get(`${BASE}/${ticketId}/reweigh-records`));
};

/* -------------------------------------------------------------------------- */
/*                    ADDITIONAL ENDPOINTS (if they exist)                   */
/* -------------------------------------------------------------------------- */

export const getWeighingRecords = async (transactionId) => {
  console.log("🔍 GET /Transaction/" + transactionId + "/weighing-records");
  return handleRequest(apiClient.get(`${BASE}/${transactionId}/weighing-records`));
};

export const getAuditLogs = async (transactionId) => {
  console.log("🔍 GET /Transaction/" + transactionId + "/audit-logs");
  return handleRequest(apiClient.get(`${BASE}/${transactionId}/audit-logs`));
};

export const startReweigh = async (transactionId, payload) => {
  console.log("📤 POST /Transaction/" + transactionId + "/start-reweigh", payload);
  return handleRequest(apiClient.post(`${BASE}/${transactionId}/start-reweigh`, payload));
};

export const addReweighWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-reweigh-weight", payload);
  return handleRequest(apiClient.post(`${BASE}/add-reweigh-weight`, payload));
};

export const completeReweigh = async (payload) => {
  console.log("📤 POST /Transaction/complete-reweigh", payload);
  return handleRequest(apiClient.post(`${BASE}/complete-reweigh`, payload));
};