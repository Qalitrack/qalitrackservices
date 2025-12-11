// src/api/Transaction/Transactions.js
import { apiClient } from "../helpers/apiClients";

/* -------------------------------------------------------------------------- */
/*                                 BASE PATH                                   */
/* -------------------------------------------------------------------------- */
const BASE = "/Transaction/Transaction"; // This will be proxied to https://qalitrack.cseco.co.ke/Transaction

// --------------------------------------------------------------------------
// LIST / FILTER TRANSACTIONS (PAGINATED)
// --------------------------------------------------------------------------
export const getTransactions = async ({
  pageNumber = 1,
  pageSize = 50,
  receiptNo,
  noPlate,
  driverName,
  vehicleId,
  commodityId,
  supplierId,
  customerId,
  transporterId,
  originId,
  destinationId,
  weighBridgeId,
  operatorId,
  status,
  isCompleted,
  startDate,
  endDate,
  weighMode,
  sortBy,
  sortDescending,
  pathSuffix = '' // New parameter for URL suffix
} = {}) => {
  const url = `${BASE}${pathSuffix}`; // Append suffix to BASE
  const response = await apiClient.get(url, {
    params: {
      pageNumber,
      pageSize,
      receiptNo,
      noPlate,
      driverName,
      vehicleId,
      commodityId,
      supplierId,
      customerId,
      transporterId,
      originId,
      destinationId,
      weighBridgeId,
      operatorId,
      status,
      isCompleted,
      startDate,
      endDate,
      weighMode,
      sortBy,
      sortDescending,
    },
  });
  console.log(response);
  return response.data.items || response.data;
};

// --------------------------------------------------------------------------
// CREATE TRANSACTION
// --------------------------------------------------------------------------
export const createTransaction = async (payload) => {
  const response = await apiClient.post(BASE, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// GET SINGLE TRANSACTION BY ID
// --------------------------------------------------------------------------
export const getTransactionById = async (id) => {
  const response = await apiClient.get(`${BASE}/${id}`);
  return response.data;
};

// --------------------------------------------------------------------------
// UPDATE TRANSACTION
// --------------------------------------------------------------------------
export const updateTransaction = async (id, payload) => {
  const response = await apiClient.put(`${BASE}/${id}`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// DELETE TRANSACTION (HARD DELETE → used as deactivate)
// --------------------------------------------------------------------------
export const deleteTransaction = async (id) => {
  const response = await apiClient.delete(`${BASE}/${id}`);
  return response.data;
};

// --------------------------------------------------------------------------
// DEACTIVATE TRANSACTION (uses DELETE per Swagger)
// --------------------------------------------------------------------------
export const deactivateTransactionApi = async (transactionId) => {
  const response = await apiClient.delete(`${BASE}/${transactionId}`);
  return response.data;
};

// --------------------------------------------------------------------------
// GET BY RECEIPT NUMBER
// --------------------------------------------------------------------------
export const getTransactionByReceipt = async (receiptNo) => {
  const response = await apiClient.get(`${BASE}/receipt/${receiptNo}`);
  return response.data;
};

// --------------------------------------------------------------------------
// WEIGHING RECORDS
// --------------------------------------------------------------------------
export const getWeighingRecords = async (transactionId) => {
  const response = await apiClient.get(`${BASE}/${transactionId}/weighing-records`);
  return response.data;
};

// --------------------------------------------------------------------------
// AUDIT LOGS
// --------------------------------------------------------------------------
export const getAuditLogs = async (transactionId) => {
  const response = await apiClient.get(`${BASE}/${transactionId}/audit-logs`);
  return response.data;
};

// --------------------------------------------------------------------------
// INCOMPLETE BY PLATE
// --------------------------------------------------------------------------
export const getIncompleteByPlate = async (noPlate) => {
  const response = await apiClient.get(`${BASE}/incomplete/vehicle/${noPlate}`);
  return response.data;
};

// --------------------------------------------------------------------------
// INCOMPLETE BY VEHICLE ID
// --------------------------------------------------------------------------
export const getIncompleteByVehicleId = async (vehicleId) => {
  const response = await apiClient.get(`${BASE}/incomplete/vehicle-id/${vehicleId}`);
  return response.data;
};

// --------------------------------------------------------------------------
// BY STATUS
// --------------------------------------------------------------------------
export const getTransactionsByStatus = async (status, limit = 100) => {
  const response = await apiClient.get(`${BASE}/status/${status}`, {
    params: { limit },
  });
  return response.data;
};

// --------------------------------------------------------------------------
// ADD WEIGHING (FIRST OR SECOND WEIGHT)
// --------------------------------------------------------------------------
export const addWeighing = async (payload) => {
  const response = await apiClient.post(`${BASE}/add-weighing`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// COMPLETE TRANSACTION (FINALIZE)
// --------------------------------------------------------------------------
export const completeTransaction = async (payload) => {
  const response = await apiClient.post(`${BASE}/complete`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// REQUEST REWEIGH
// --------------------------------------------------------------------------
export const requestReweigh = async (payload) => {
  const response = await apiClient.post(`${BASE}/request-reweigh`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// START REWEIGH
// --------------------------------------------------------------------------
export const startReweigh = async (transactionId, payload) => {
  const response = await apiClient.post(`${BASE}/${transactionId}/start-reweigh`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// ADD REWEIGH WEIGHT
// --------------------------------------------------------------------------
export const addReweighWeight = async (payload) => {
  const response = await apiClient.post(`${BASE}/add-reweigh-weight`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// COMPLETE REWEIGH
// --------------------------------------------------------------------------
export const completeReweigh = async (payload) => {
  const response = await apiClient.post(`${BASE}/complete-reweigh`, payload);
  return response.data;
};

// --------------------------------------------------------------------------
// REWEIGH RECORDS
// --------------------------------------------------------------------------
export const getReweighRecords = async (transactionId) => {
  const response = await apiClient.get(`${BASE}/${transactionId}/reweigh-records`);
  return response.data;
};

// --------------------------------------------------------------------------
// CHECK RECEIPT UNIQUENESS
// --------------------------------------------------------------------------
export const checkReceiptExists = async (receiptNo) => {
  const response = await apiClient.get(`${BASE}/check-receipt/${receiptNo}`);
  return response.data;
};