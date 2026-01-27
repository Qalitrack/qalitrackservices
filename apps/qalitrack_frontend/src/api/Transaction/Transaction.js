// src/api/Transaction/Transactions.js
// ✅ FIXED: Corrected POST endpoint that was causing 405 Method Not Allowed
import { transactionsClient } from "../helpers/apiClients";

/* -------------------------------------------------------------------------- */
/*                                 BASE PATH                                   */
/* -------------------------------------------------------------------------- */
// ✅ CORRECTED: Based on successful POST showing 201 response at /Transaction/Transaction
// transactionsClient has baseURL = "/api/Transaction"
// Full path is: /api/Transaction/Transaction (not triple nested)
// So BASE should be: "/Transaction"
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
 * GET /api/Transaction/Transaction
 * List/filter transactions with pagination
 */
export const getTransactions = async (params = {}) => {
  console.log("🔍 GET Fetching transactions with params:", params);
  return handleRequest(transactionsClient.get(BASE, { params }));
};

/**
 * POST /api/Transaction/Transaction
 * Create a new transaction (first weight)
 * ✅ CRITICAL FIX: Don't wrap in "request" object - send payload directly
 */
export const createTransaction = async (payload) => {
  console.log("📤 POST createTransaction - Raw payload:", payload);
  
  // ✅ CRITICAL FIX: Try WITHOUT wrapping first
  // If backend expects wrapped format, we'll need to adjust based on error
  console.log("📦 Sending payload directly (not wrapped)");
  console.log("🎯 Full URL will be: /api/Transaction" + BASE);
  
  // ✅ Send directly without wrapping
  return handleRequest(transactionsClient.post(BASE, payload));
};

/**
 * GET /api/Transaction/Transaction/{ticketId}
 * Get a specific transaction by ID
 */
export const getTransactionById = async (ticketId) => {
  console.log("🔍 GET /Transaction/" + ticketId);
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}`));
};

/**
 * PUT /api/Transaction/Transaction/{ticketId}
 * Update transaction
 */
export const updateTransaction = async (ticketId, payload) => {
  console.log("📝 PUT /Transaction/" + ticketId, payload);
  
  // Try without wrapping first
  return handleRequest(transactionsClient.put(`${BASE}/${ticketId}`, payload));
};

/**
 * DELETE /api/Transaction/Transaction/{ticketId}
 * Delete/deactivate a transaction
 */
export const deleteTransaction = async (ticketId) => {
  console.log("🗑️ DELETE /Transaction/" + ticketId);
  return handleRequest(transactionsClient.delete(`${BASE}/${ticketId}`));
};

export const deactivateTransactionApi = async (ticketId) => {
  return deleteTransaction(ticketId);
};

/* -------------------------------------------------------------------------- */
/*                      RECEIPT & VEHICLE QUERIES                             */
/* -------------------------------------------------------------------------- */

/**
 * GET /api/Transaction/Transaction/receipt/{receiptNo}
 */
export const getTransactionByReceipt = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  console.log("🔍 GET /Transaction/receipt/" + encoded);
  return handleRequest(transactionsClient.get(`${BASE}/receipt/${encoded}`));
};

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle/{noPlate}
 */
export const getIncompleteByPlate = async (noPlate) => {
  const encoded = encodeURIComponent(noPlate);
  console.log("🔍 GET /Transaction/incomplete/vehicle/" + encoded);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle/${encoded}`));
};

export const getIncompleteByVehicleNo = async (noPlate) => {
  return getIncompleteByPlate(noPlate);
};

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle-id/{vehicleId}
 */
export const getIncompleteByVehicleId = async (vehicleId) => {
  console.log("🔍 GET /Transaction/incomplete/vehicle-id/" + vehicleId);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`));
};

/**
 * GET /api/Transaction/Transaction/check-receipt/{receiptNo}
 */
export const checkReceiptExists = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  console.log("🔍 GET /Transaction/check-receipt/" + encoded);
  return handleRequest(transactionsClient.get(`${BASE}/check-receipt/${encoded}`));
};

export const checkReceipt = async (receiptNo) => {
  return checkReceiptExists(receiptNo);
};

/* -------------------------------------------------------------------------- */
/*                           STATUS QUERIES                                   */
/* -------------------------------------------------------------------------- */

/**
 * GET /api/Transaction/Transaction/status/{status}
 */
export const getTransactionsByStatus = async (status, limit = 100) => {
  console.log("🔍 GET /Transaction/status/" + status, { limit });
  return handleRequest(
    transactionsClient.get(`${BASE}/status/${status}`, {
      params: { limit },
    })
  );
};

/* -------------------------------------------------------------------------- */
/*                         WEIGHING OPERATIONS                                */
/* -------------------------------------------------------------------------- */

/**
 * POST /api/Transaction/Transaction/add-second-weight
 * ✅ FIXED: Send payload directly without wrapping
 */
export const addSecondWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-second-weight:", payload);
  
  // ✅ Send directly
  return handleRequest(transactionsClient.post(`${BASE}/add-second-weight`, payload));
};

/**
 * Alias for backward compatibility
 */
export const addWeighing = async (payload) => {
  // ✅ If payload is already in correct format, use it directly
  if (payload.ticketID && payload.secondWeight) {
    console.log("📤 addWeighing -> using payload as-is");
    return addSecondWeight(payload);
  }
  
  // ✅ Otherwise, map old format to new format
  const mappedPayload = {
    ticketID: payload.transactionId || payload.ticketID || payload.id,
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
 * POST /api/Transaction/Transaction/complete
 */
export const completeTransaction = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId || payload.id
  };
  
  console.log("📤 POST /Transaction/complete:", mappedPayload);
  
  // Send directly
  return handleRequest(transactionsClient.post(`${BASE}/complete`, mappedPayload));
};

/* -------------------------------------------------------------------------- */
/*                         REWEIGH OPERATIONS                                 */
/* -------------------------------------------------------------------------- */

/**
 * POST /api/Transaction/Transaction/request-reweigh
 */
export const requestReweigh = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId,
    reason: payload.reason || ""
  };
  
  console.log("📤 POST /Transaction/request-reweigh:", mappedPayload);
  
  // Send directly
  return handleRequest(transactionsClient.post(`${BASE}/request-reweigh`, mappedPayload));
};

/**
 * GET /api/Transaction/Transaction/{ticketId}/reweigh-records
 */
export const getReweighRecords = async (ticketId) => {
  console.log("🔍 GET /Transaction/" + ticketId + "/reweigh-records");
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}/reweigh-records`));
};

/* -------------------------------------------------------------------------- */
/*                    ADDITIONAL ENDPOINTS                                    */
/* -------------------------------------------------------------------------- */

export const getWeighingRecords = async (transactionId) => {
  console.log("🔍 GET /Transaction/" + transactionId + "/weighing-records");
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/weighing-records`));
};

export const getAuditLogs = async (transactionId) => {
  console.log("🔍 GET /Transaction/" + transactionId + "/audit-logs");
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/audit-logs`));
};

export const startReweigh = async (transactionId, payload) => {
  console.log("📤 POST /Transaction/" + transactionId + "/start-reweigh", payload);
  
  // Send directly
  return handleRequest(transactionsClient.post(`${BASE}/${transactionId}/start-reweigh`, payload));
};

export const addReweighWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-reweigh-weight", payload);
  
  // Send directly
  return handleRequest(transactionsClient.post(`${BASE}/add-reweigh-weight`, payload));
};

export const completeReweigh = async (payload) => {
  console.log("📤 POST /Transaction/complete-reweigh", payload);
  
  // Send directly
  return handleRequest(transactionsClient.post(`${BASE}/complete-reweigh`, payload));
};