// ✅ FINAL FIX - Transaction.js
// Uses correct TRIPLE path: /Transaction/Transaction/Transaction

import { transactionsClient } from "../helpers/apiClients";

const BASE = "/Transaction";
const CREATE_PATH = "/Transaction/Transaction/"; // ✅ Full correct path

// Who's making this change, for the audit trail — the transaction service has
// no auth of its own, so the logged-in user's identity has to come from here.
const getChangedByEmail = () => {
  try {
    const session = JSON.parse(localStorage.getItem("authSession"));
    return session?.userData?.email || null;
  } catch {
    return null;
  }
};

const handleRequest = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    
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
  return handleRequest(transactionsClient.get(BASE, { params }));
};

/**
 * POST /api/Transaction/Transaction/Transaction
 * ✅ FIXED: Uses correct triple path and wraps payload in "request" object
 */
export const createTransaction = async (payload) => {
  
  // ✅ Wrap payload in "request" object to match API expectation
  const wrappedPayload = {
    request: payload
  };
  
  
  try {
    // ✅ Use the FULL triple path directly - no fallback needed
    const response = await transactionsClient.post(CREATE_PATH, wrappedPayload);
    
    
    return response.data;
  } catch (error) {
    throw error;
  }
};

/**
 * GET /api/Transaction/Transaction/stats
 * Aggregated dashboard stats (counts, net weight sums, top vehicles/commodities,
 * weekly trend) computed server-side instead of pulling the whole table.
 */
export const getTransactionStats = async (signal) => {
  return handleRequest(transactionsClient.get(`${BASE}/stats`, { signal }));
};

/**
 * GET /api/Transaction/Transaction/{ticketId}
 */
export const getTransactionById = async (ticketId) => {
  return handleRequest(transactionsClient.get(`${BASE}/${ticketId}`));
};

/**
 * PUT /api/Transaction/Transaction/{ticketId}
 */
export const updateTransaction = async (ticketId, payload) => {
  return handleRequest(
    transactionsClient.put(`${BASE}/${ticketId}`, { changedBy: getChangedByEmail(), ...payload })
  );
};

/**
 * DELETE /api/Transaction/Transaction/{ticketId}
 */
export const deleteTransaction = async (ticketId) => {
  return handleRequest(
    transactionsClient.delete(`${BASE}/${ticketId}`, { params: { changedBy: getChangedByEmail() } })
  );
};

export const deactivateTransactionApi = deleteTransaction;

/**
 * GET /api/Transaction/Transaction/receipt/{receiptNo}
 */
export const getTransactionByReceipt = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  return handleRequest(transactionsClient.get(`${BASE}/receipt/${encoded}`));
};

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle/{noPlate}
 */
export const getIncompleteByPlate = async (noPlate) => {
  const encoded = encodeURIComponent(noPlate);
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle/${encoded}`));
};

export const getIncompleteByVehicleNo = getIncompleteByPlate;

/**
 * GET /api/Transaction/Transaction/incomplete/vehicle-id/{vehicleId}
 */
export const getIncompleteByVehicleId = async (vehicleId) => {
  return handleRequest(transactionsClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`));
};

/**
 * GET /api/Transaction/Transaction/check-receipt/{receiptNo}
 */
export const checkReceiptExists = async (receiptNo) => {
  const encoded = encodeURIComponent(receiptNo);
  return handleRequest(transactionsClient.get(`${BASE}/check-receipt/${encoded}`));
};

export const checkReceipt = checkReceiptExists;

/**
 * GET /api/Transaction/Transaction/status/{status}
 */
export const getTransactionsByStatus = async (status, limit = 100) => {
  return handleRequest(
    transactionsClient.get(`${BASE}/status/${status}`, { params: { limit } })
  );
};

/**
 * POST /api/Transaction/Transaction/add-second-weight
 */
export const addSecondWeight = async (payload) => {
  return handleRequest(
    transactionsClient.post(`${BASE}/add-second-weight`, { changedBy: getChangedByEmail(), ...payload })
  );
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
  
  return addSecondWeight(mapped);
};

/**
 * POST /api/Transaction/Transaction/complete
 */
export const completeTransaction = async (payload) => {
  const mapped = {
    ticketID: payload.ticketID || payload.transactionId || payload.id,
    changedBy: getChangedByEmail(),
  };
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
  return handleRequest(transactionsClient.post(`${BASE}/request-reweigh`, mapped));
};

/**
 * GET /api/Transaction/Transaction/{ticketId}/reweigh-records
 */
export const getReweighRecords = async (ticketId) => {
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
  return handleRequest(transactionsClient.post(`${BASE}/reject-reweigh`, mapped));
};

/**
 * GET /api/Transaction/Transaction/{transactionId}/weighing-records
 */
export const getWeighingRecords = async (transactionId) => {
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/weighing-records`));
};

/**
 * GET /api/Transaction/Transaction/{transactionId}/audit-logs
 */
export const getAuditLogs = async (transactionId) => {
  return handleRequest(transactionsClient.get(`${BASE}/${transactionId}/audit-logs`));
};

/**
 * POST /api/Transaction/Transaction/{transactionId}/start-reweigh
 */
export const startReweigh = async (transactionId, payload) => {
  return handleRequest(transactionsClient.post(`${BASE}/${transactionId}/start-reweigh`, payload));
};

/**
 * POST /api/Transaction/Transaction/add-reweigh-weight
 */
export const addReweighWeight = async (payload) => {
  return handleRequest(transactionsClient.post(`${BASE}/add-reweigh-weight`, payload));
};

/**
 * POST /api/Transaction/Transaction/complete-reweigh
 */
export const completeReweigh = async (payload) => {
  return handleRequest(transactionsClient.post(`${BASE}/complete-reweigh`, payload));
};

/**
 * GET /api/Transaction/Settings
 * Transaction settings (e.g. ticket/receipt number prefix)
 */
export const getTransactionSettings = async () => {
  return handleRequest(transactionsClient.get("/Settings"));
};

/**
 * PUT /api/Transaction/Settings
 */
export const updateTransactionSettings = async (payload) => {
  return handleRequest(transactionsClient.put("/Settings", payload));
};