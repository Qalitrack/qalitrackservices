// ✅ FINAL FIX - Transaction.js
// Uses correct TRIPLE path: /Transaction/Transaction/Transaction

import { transactionsClient } from "../helpers/apiClients";

const BASE = "/Transaction";
const CREATE_PATH = "/Transaction/Transaction/"; // ✅ Full correct path

const handleRequest = async (promise) => {
  try {
    const response = await promise;
    console.log("✅ API Response:", {
      status: response.status,
      hasData: !!response.data,
      dataType: typeof response.data
    });
    return response.data;
  } catch (error) {
    console.error("❌ Transaction API Error:", error);
    console.error("❌ URL:", error.config?.url);
    console.error("❌ Status:", error.response?.status);
    console.error("❌ Response:", error.response?.data);
    
    let errorMessage = "Unknown error occurred";
    
    if (error.response?.data) {
      const data = error.response.data;
      if (typeof data === 'string') {
        errorMessage = data;
      } else if (data.message) {
        errorMessage = data.message;
      } else if (data.title) {
        errorMessage = data.title;
      } else if (data.errors) {
        errorMessage = Object.values(data.errors).flat().join("; ");
      }
    } else if (error.message) {
      errorMessage = error.message;
    }
    
    throw new Error(errorMessage);
  }
};

/**
 * GET /api/Transaction/Transaction
 */
export const getTransactions = async (params = {}) => {
  console.log("🔍 GET Transactions, params:", params);
  return handleRequest(transactionsClient.get(BASE, { params }));
};

/**
 * POST /api/Transaction/Transaction/Transaction
 * ✅ FIXED: Uses correct triple path and wraps payload in "request" object
 */
export const createTransaction = async (payload) => {
  console.log("");
  console.log("🚀 ========== CREATE TRANSACTION ==========");
  console.log("📥 Original payload:", payload);
  
  // ✅ Wrap payload in "request" object to match API expectation
  const wrappedPayload = {
    request: payload
  };
  
  console.log("📦 Wrapped payload:", wrappedPayload);
  console.log("🎯 Posting to:", CREATE_PATH);
  console.log("");
  
  try {
    // ✅ Use the FULL triple path directly - no fallback needed
    const response = await transactionsClient.post(CREATE_PATH, wrappedPayload);
    
    console.log("✅ Transaction created successfully!");
    console.log("📥 Response:", response.data);
    console.log("");
    
    return response.data;
  } catch (error) {
    console.error("❌ Create transaction failed!");
    console.error("❌ URL attempted:", transactionsClient.client.defaults.baseURL + CREATE_PATH);
    console.error("❌ Payload sent:", JSON.stringify(wrappedPayload, null, 2));
    console.error("❌ Error:", error.message);
    console.error("");
    throw error;
  }
};

/**
 * GET /api/Transaction/Transaction/{ticketId}
 */
export const getTransactionById = async (ticketId) => {
  console.log("🔍 GET Transaction by ID:", ticketId);
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}`));
};

/**
 * PUT /api/Transaction/Transaction/{ticketId}
 */
export const updateTransaction = async (ticketId, payload) => {
  console.log("📝 PUT Transaction:", ticketId);
  return handleRequest(transactionsClient.put(`${BASE}/${ticketId}`, payload));
};

/**
 * DELETE /api/Transaction/Transaction/{ticketId}
 */
export const deleteTransaction = async (ticketId) => {
  console.log("🗑️ DELETE Transaction:", ticketId);
  return handleRequest(transactionsClient.delete(`${BASE}/${ticketId}`));
};

export const deactivateTransactionApi = deleteTransaction;

/**
 * GET /api/Transaction/Transaction/receipt/{receiptNo}
 */
export const getTransactionByReceipt = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  console.log("🔍 GET Transaction by receipt:", receiptNo);
  return handleRequest(transactionsClient.get(`${BASE}/receipt/${encoded}`));
};

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle/{noPlate}
 */
export const getIncompleteByPlate = async (noPlate) => {
  const encoded = encodeURIComponent(noPlate);
  console.log("🔍 GET Incomplete by plate:", noPlate);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle/${encoded}`));
};

export const getIncompleteByVehicleNo = getIncompleteByPlate;

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle-id/{vehicleId}
 */
export const getIncompleteByVehicleId = async (vehicleId) => {
  console.log("🔍 GET Incomplete by vehicle ID:", vehicleId);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`));
};

/**
 * GET /api/Transaction/Transaction/check-receipt/{receiptNo}
 */
export const checkReceiptExists = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  console.log("🔍 Check receipt exists:", receiptNo);
  return handleRequest(transactionsClient.get(`${BASE}/check-receipt/${encoded}`));
};

export const checkReceipt = checkReceiptExists;

/**
 * GET /api/Transaction/Transaction/status/{status}
 */
export const getTransactionsByStatus = async (status, limit = 100) => {
  console.log("🔍 GET Transactions by status:", status, "limit:", limit);
  return handleRequest(
    transactionsClient.get(`${BASE}/status/${status}`, { params: { limit } })
  );
};

/**
 * POST /api/Transaction/Transaction/add-second-weight
 */
export const addSecondWeight = async (payload) => {
  console.log("📤 POST Add second weight");
  console.log("📦 Payload:", payload);
  return handleRequest(transactionsClient.post(`${BASE}/add-second-weight`, payload));
};

/**
 * Helper function that maps various payload formats to AddSecondWeightDto
 */
export const addWeighing = async (payload) => {
  const mapped = {
    ticketID: payload.transactionId || payload.ticketID || payload.id,
    secondWeight: String(payload.weight || payload.secondWeight || "0"),
    weighBridgeName2nd: payload.weighBridgeName || payload.weighBridgeName2nd || "",
    scaleName2nd: payload.scaleName || payload.scaleName2nd || "Scale-01",
    operatorID2nd: String(payload.operatorId || payload.operatorID2nd || ""),
    operatorName2nd: payload.operatorName || payload.operatorName2nd || "",
    notes: payload.notes || "",
  };
  
  console.log("📤 Mapped weighing payload:", mapped);
  return addSecondWeight(mapped);
};

/**
 * POST /api/Transaction/Transaction/complete
 */
export const completeTransaction = async (payload) => {
  const mapped = {
    ticketID: payload.ticketID || payload.transactionId || payload.id
  };
  console.log("📤 POST Complete transaction:", mapped);
  return handleRequest(transactionsClient.post(`${BASE}/complete`, mapped));
};

/**
 * POST /api/Transaction/Transaction/request-reweigh
 */
export const requestReweigh = async (payload) => {
  const mapped = {
    ticketID: payload.ticketID || payload.transactionId,
    reason: payload.reason || ""
  };
  console.log("📤 POST Request reweigh:", mapped);
  return handleRequest(transactionsClient.post(`${BASE}/request-reweigh`, mapped));
};

/**
 * GET /api/Transaction/Transaction/{ticketId}/reweigh-records
 */
export const getReweighRecords = async (ticketId) => {
  console.log("🔍 GET Reweigh records:", ticketId);
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}/reweigh-records`));
};

/**
 * POST /api/Transaction/Transaction/approve-reweigh
 * Clears second weight data and resets transaction to Active for re-weighing
 */
export const approveReweigh = async (payload) => {
  const mapped = {
    ticketID: payload.ticketID || payload.transactionId,
    approvedBy: payload.approvedBy || "",
    notes: payload.notes || ""
  };
  console.log("📤 POST Approve reweigh:", mapped);
  return handleRequest(transactionsClient.post(`${BASE}/approve-reweigh`, mapped));
};

/**
 * POST /api/Transaction/Transaction/reject-reweigh
 * Keeps original weights and restores transaction to Completed
 */
export const rejectReweigh = async (payload) => {
  const mapped = {
    ticketID: payload.ticketID || payload.transactionId,
    rejectionReason: payload.rejectionReason || payload.reason || "",
    rejectedBy: payload.rejectedBy || "",
    notes: payload.notes || ""
  };
  console.log("📤 POST Reject reweigh:", mapped);
  return handleRequest(transactionsClient.post(`${BASE}/reject-reweigh`, mapped));
};

/**
 * GET /api/Transaction/Transaction/{transactionId}/weighing-records
 */
export const getWeighingRecords = async (transactionId) => {
  console.log("🔍 GET Weighing records:", transactionId);
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/weighing-records`));
};

/**
 * GET /api/Transaction/Transaction/{transactionId}/audit-logs
 */
export const getAuditLogs = async (transactionId) => {
  console.log("🔍 GET Audit logs:", transactionId);
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/audit-logs`));
};

/**
 * POST /api/Transaction/Transaction/{transactionId}/start-reweigh
 */
export const startReweigh = async (transactionId, payload) => {
  console.log("📤 POST Start reweigh:", transactionId);
  return handleRequest(transactionsClient.post(`${BASE}/${transactionId}/start-reweigh`, payload));
};

/**
 * POST /api/Transaction/Transaction/add-reweigh-weight
 */
export const addReweighWeight = async (payload) => {
  console.log("📤 POST Add reweigh weight");
  return handleRequest(transactionsClient.post(`${BASE}/add-reweigh-weight`, payload));
};

/**
 * POST /api/Transaction/Transaction/complete-reweigh
 */
export const completeReweigh = async (payload) => {
  console.log("📤 POST Complete reweigh");
  return handleRequest(transactionsClient.post(`${BASE}/complete-reweigh`, payload));
};