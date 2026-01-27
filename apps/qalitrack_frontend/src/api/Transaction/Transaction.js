// src/api/Transaction/Transactions.js
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
 * GET /api/Transaction/Transaction/Transaction
 * List/filter transactions with pagination
 */
export const getTransactions = async (params = {}) => {
  console.log("🔍 Fetching transactions with params:", params);
  return handleRequest(transactionsClient.get(BASE, { params }));
};

/**
 * POST /api/Transaction/Transaction/Transaction
 * Create a new transaction (first weight)
 * IMPORTANT: Payload must be wrapped in "request" object
 */
export const createTransaction = async (payload) => {
  console.log("📤 POST createTransaction - Raw payload:", payload);
  
  // ✅ Wrap payload in "request" object as shown in curl screenshot
  const wrappedPayload = { request: payload };
  
  console.log("📦 Wrapped payload:", JSON.stringify(wrappedPayload, null, 2));
  console.log("🎯 Full URL will be: /api/Transaction/Transaction");
  
  return handleRequest(transactionsClient.post(BASE, wrappedPayload));
};

/**
 * GET /api/Transaction/Transaction/Transaction/{ticketId}
 * Get a specific transaction by ID
 */
export const getTransactionById = async (ticketId) => {
  console.log("🔍 GET /Transaction/" + ticketId);
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}`));
};

/**
 * PUT /api/Transaction/Transaction/Transaction/{ticketId}
 * Update transaction
 */
export const updateTransaction = async (ticketId, payload) => {
  console.log("📝 PUT /Transaction/" + ticketId, payload);
  
  // Wrap in "request" if not already wrapped
  const wrappedPayload = payload.request ? payload : { request: payload };
  
  return handleRequest(transactionsClient.put(`${BASE}/${ticketId}`, wrappedPayload));
};

/**
 * DELETE /api/Transaction/Transaction/Transaction/{ticketId}
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
 * GET /api/Transaction/Transaction/Transaction/receipt/{receiptNo}
 */
export const getTransactionByReceipt = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  console.log("🔍 GET /Transaction/receipt/" + encoded);
  return handleRequest(transactionsClient.get(`${BASE}/receipt/${encoded}`));
};

/**
 * GET /api/Transaction/Transaction/Transaction/incomplete/vehicle/{noPlate}
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
 * GET /api/Transaction/Transaction/Transaction/incomplete/vehicle-id/{vehicleId}
 */
export const getIncompleteByVehicleId = async (vehicleId) => {
  console.log("🔍 GET /Transaction/incomplete/vehicle-id/" + vehicleId);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`));
};

/**
 * GET /api/Transaction/Transaction/Transaction/check-receipt/{receiptNo}
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
 * GET /api/Transaction/Transaction/Transaction/status/{status}
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
 * POST /api/Transaction/Transaction/Transaction/add-second-weight
 */
export const addSecondWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-second-weight:", payload);
  
  // Wrap in "request" if not already wrapped
  const wrappedPayload = payload.request ? payload : { request: payload };
  
  return handleRequest(transactionsClient.post(`${BASE}/add-second-weight`, wrappedPayload));
};

/**
 * Alias for backward compatibility
 */
/**
 * Alias for backward compatibility - properly maps old format to new
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
 * POST /api/Transaction/Transaction/Transaction/complete
 */
export const completeTransaction = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId || payload.id
  };
  
  console.log("📤 POST /Transaction/complete:", mappedPayload);
  
  // Wrap in "request"
  const wrappedPayload = { request: mappedPayload };
  
  return handleRequest(transactionsClient.post(`${BASE}/complete`, wrappedPayload));
};

/* -------------------------------------------------------------------------- */
/*                         REWEIGH OPERATIONS                                 */
/* -------------------------------------------------------------------------- */

/**
 * POST /api/Transaction/Transaction/Transaction/request-reweigh
 */
export const requestReweigh = async (payload) => {
  const mappedPayload = {
    ticketID: payload.ticketID || payload.transactionId,
    reason: payload.reason || ""
  };
  
  console.log("📤 POST /Transaction/request-reweigh:", mappedPayload);
  
  // Wrap in "request"
  const wrappedPayload = { request: mappedPayload };
  
  return handleRequest(transactionsClient.post(`${BASE}/request-reweigh`, wrappedPayload));
};

/**
 * GET /api/Transaction/Transaction/Transaction/{ticketId}/reweigh-records
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
  
  // Wrap in "request"
  const wrappedPayload = payload.request ? payload : { request: payload };
  
  return handleRequest(transactionsClient.post(`${BASE}/${transactionId}/start-reweigh`, wrappedPayload));
};

export const addReweighWeight = async (payload) => {
  console.log("📤 POST /Transaction/add-reweigh-weight", payload);
  
  // Wrap in "request"
  const wrappedPayload = payload.request ? payload : { request: payload };
  
  return handleRequest(transactionsClient.post(`${BASE}/add-reweigh-weight`, wrappedPayload));
};

export const completeReweigh = async (payload) => {
  console.log("📤 POST /Transaction/complete-reweigh", payload);
  
  // Wrap in "request"
  const wrappedPayload = payload.request ? payload : { request: payload };
  
  return handleRequest(transactionsClient.post(`${BASE}/complete-reweigh`, wrappedPayload));
};