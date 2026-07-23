// src/store/weighingSlice.js
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { REHYDRATE } from "redux-persist";
import { apiClient } from "../api/helpers/apiClients";

// ✅ CORRECTED: Import from the right path
import {
    getTransactions,
    getTransactionById,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    addWeighing,
    completeTransaction,
    getTransactionByReceipt,
    getIncompleteByVehicleNo,
    getIncompleteByVehicleId,
    getTransactionsByStatus,
    checkReceipt,
    requestReweigh,
    getReweighRecords,
    approveReweigh,
    rejectReweigh,
} from "../api/Transaction/Transaction";
// ─────────────────────────────────────────────────────────────────────────────
// SIMULATED WEIGHT
// ─────────────────────────────────────────────────────────────────────────────
export const fetchSimulatedWeight = createAsyncThunk(
    "weighing/fetchSimulatedWeight",
    async (_, { rejectWithValue }) => {
        try {
            const weight = Math.floor(Math.random() * 50000) + 10000;
            const position = Math.random() < 0.7 ? "Fully On" : "Partially On";
            return { weight, position };
        } catch (error) {
            return rejectWithValue("Simulation failed");
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// MASTER DATA THUNKS
// ─────────────────────────────────────────────────────────────────────────────

// VEHICLES
export const fetchVehicles = createAsyncThunk(
    "weighing/fetchVehicles",
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Vehicles", {
                params: { pageNumber: 1, pageSize: 50 },
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message || "Network error");
        }
    }
);

export const fetchVehiclesByName = createAsyncThunk(
    "weighing/fetchVehiclesByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Vehicles", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchVehicleById = createAsyncThunk(
    "weighing/fetchVehicleById",
    async (id, { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Vehicles/${id}`);
            const data = response.data?.data?.vehicle || response.data?.vehicle || response.data;
            return data ? [data] : [];
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchVehiclesByRegNumber = createAsyncThunk(
    "weighing/fetchVehiclesByRegNumber",
    async (regNumber, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Vehicles", {
                params: { pageNumber: 1, pageSize: 50, regNumber },
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// DRIVERS
export const fetchDrivers = createAsyncThunk(
    "weighing/fetchDrivers", 
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Drivers", { 
                params: { pageNumber: 1, pageSize: 50 } 
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchDriversByName = createAsyncThunk(
    "weighing/fetchDriversByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Drivers", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchDriverById = createAsyncThunk(
    "weighing/fetchDriverById",
    async (id, { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Drivers/${id}`);
            const data = response.data?.data?.driver || response.data?.driver || response.data;
            return data ? [data] : [];
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// PRODUCTS/COMMODITIES
export const fetchProducts = createAsyncThunk(
    "weighing/fetchProducts", 
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Products", { 
                params: { pageNumber: 1, pageSize: 50 } 
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchProductsByName = createAsyncThunk(
    "weighing/fetchProductsByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Products", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name }
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// ROUTES
export const fetchRoutes = createAsyncThunk(
    "weighing/fetchRoutes", 
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Routes", { 
                params: { pageNumber: 1, pageSize: 50 } 
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchRoutesByName = createAsyncThunk(
    "weighing/fetchRoutesByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Routes", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// SACCOS
export const fetchSaccosByName = createAsyncThunk(
    "weighing/fetchSaccosByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Saccos", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            const data = response.data;
            return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchSaccoById = createAsyncThunk(
    "weighing/fetchSaccoById",
    async (id, { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Saccos/${id}`);
            const data = response.data?.data?.sacco || response.data?.sacco || response.data;
            return data ? [data] : [];
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// SUPPLIERS
export const fetchSuppliers = createAsyncThunk(
    "weighing/fetchSuppliers", 
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Suppliers", { 
                params: { pageNumber: 1, pageSize: 50 } 
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchSuppliersByName = createAsyncThunk(
    "weighing/fetchSuppliersByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Suppliers", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchSupplierById = createAsyncThunk(
    "weighing/fetchSupplierById",
    async (id, { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Suppliers/${id}`);
            const data = response.data?.supplier || response.data;
            return data ? [data] : [];
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// TRANSPORTERS
export const fetchTransporters = createAsyncThunk(
    "weighing/fetchTransporters", 
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Transporters", { 
                params: { pageNumber: 1, pageSize: 50 } 
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchTransportersByName = createAsyncThunk(
    "weighing/fetchTransportersByName",
    async (name, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Transporters", {
                params: { pageNumber: 1, pageSize: 50, searchTerm: name },
            });
            return response.data?.items || (Array.isArray(response.data) ? response.data : []);
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

export const fetchTransporterById = createAsyncThunk(
    "weighing/fetchTransporterById",
    async (id, { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Transporters/${id}`);
            const data = response.data?.transporter || response.data;
            return data ? [data] : [];
        } catch (error) {
            return rejectWithValue(error.response?.data?.message || error.message);
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// USER THUNKS
// ─────────────────────────────────────────────────────────────────────────────
export const fetchUserById = createAsyncThunk(
    "weighing/fetchUserById",
    async (userId, { rejectWithValue }) => {
        if (!userId) {
            return rejectWithValue("User ID is required");
        }
        try {
            const response = await apiClient.get(`/Users/${userId}`);
            return response.data?.data?.user || response.data?.user || response.data || null;
        } catch (error) {
            const message = error.response?.data?.message || error.message || "Failed to fetch user";
            return rejectWithValue(message);
        }
    }
);

export const fetchCurrentUser = createAsyncThunk(
    "weighing/fetchCurrentUser",
    async (_, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/Users/me");
            return response.data?.data?.user || response.data?.user || response.data || null;
        } catch (error) {
            const message = error.response?.data?.message || error.message || "Failed to fetch current user";
            return rejectWithValue(message);
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// WEIGHBRIDGES
// ─────────────────────────────────────────────────────────────────────────────
export const fetchWeighbridges = createAsyncThunk(
    "weighing/fetchWeighbridges",
    async ({ pageNumber = 1, pageSize = 100 } = {}, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Weighbridges", {
                params: { pageNumber, pageSize },
            });


            // Handle different response structures
            let items = [];
            if (response.data?.items) {
                items = response.data.items;
            } else if (response.data?.data?.items) {
                items = response.data.data.items;
            } else if (Array.isArray(response.data?.data)) {
                items = response.data.data;
            } else if (Array.isArray(response.data)) {
                items = response.data;
            }


            const meta = {
                pageNumber: response.data?.pageNumber || pageNumber,
                pageSize: response.data?.pageSize || pageSize,
                totalItems: response.data?.totalItems || items.length,
                totalPages: response.data?.totalPages || 1,
                hasPreviousPage: response.data?.hasPreviousPage || false,
            };

            return { items, meta };
        } catch (error) {
            const message =
                error.response?.data?.message ||
                error.response?.data?.error ||
                error.message ||
                "Failed to fetch weighbridges";
            return rejectWithValue(message);
        }
    }
);

export const fetchWeighbridgesByName = createAsyncThunk(
    "weighing/fetchWeighbridgesByName",
    async (searchTerm = "", { rejectWithValue }) => {
        try {
            if (!searchTerm?.trim()) {
                return { items: [], meta: {} };
            }

            const response = await apiClient.get("/MasterData/Weighbridges", {
                params: {
                    pageNumber: 1,
                    pageSize: 30,
                    searchTerm: searchTerm.trim(),
                },
            });

            const items = response.data?.items || [];
            return { items, meta: response.data || {} };
        } catch (error) {
            const message =
                error.response?.data?.message ||
                error.response?.data?.error ||
                error.message ||
                "Failed to search weighbridges";
            return rejectWithValue(message);
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// TRANSACTION THUNKS - ✅ UPDATED WITH FIXES
// ─────────────────────────────────────────────────────────────────────────────

/**
 * ✅ FIXED: Fetch transactions with better response handling
 */
export const fetchTransactions = createAsyncThunk(
    "weighing/fetchTransactions",
    async (filters = {}, { rejectWithValue }) => {
        try {
            const response = await getTransactions(filters);
            
            
            // ✅ COMPREHENSIVE: Try ALL possible response structures
            let items = [];
            
            // Structure 1: response.data.data.items
            if (response?.data?.data?.items && Array.isArray(response.data.data.items)) {
                items = response.data.data.items;
            }
            // Structure 2: response.data.items
            else if (response?.data?.items && Array.isArray(response.data.items)) {
                items = response.data.items;
            }
            // Structure 3: response.items
            else if (response?.items && Array.isArray(response.items)) {
                items = response.items;
            }
            // Structure 4: response.data as array
            else if (response?.data && Array.isArray(response.data)) {
                items = response.data;
            }
            // Structure 5: response as array
            else if (Array.isArray(response)) {
                items = response;
            }
            // Structure 6: response.data.data as array (some APIs use this)
            else if (response?.data?.data && Array.isArray(response.data.data)) {
                items = response.data.data;
            }
            // Structure 7: Wrapped in result/results
            else if (response?.result && Array.isArray(response.result)) {
                items = response.result;
            }
            else if (response?.results && Array.isArray(response.results)) {
                items = response.results;
            }
            // Structure 8: Check data.result
            else if (response?.data?.result && Array.isArray(response.data.result)) {
                items = response.data.result;
            }
            else {
                if (response?.data) {
                }
                items = [];
            }


            // Log first transaction for debugging
            if (items.length > 0) {
            }

            // ✅ Real server-side total record count, not just this page's length —
            // the Transactions table pagination relies on this to compute page count.
            const totalCount =
                response?.data?.data?.totalCount ??
                response?.data?.totalCount ??
                response?.totalCount ??
                response?.data?.data?.totalItems ??
                response?.data?.totalItems ??
                response?.totalItems ??
                items.length;

            return { items, totalCount };
        } catch (error) {
            return rejectWithValue(error.message || "Failed to load transactions");
        }
    }
);


/**
 * ✅ FIXED: Create new transaction with better response extraction
 */
/**
 * ✅ CORRECT VERSION - No wrapping in weighingSlice
 * Let Transaction.js handle the wrapping
 */
export const addTransaction = createAsyncThunk(
    "weighing/addTransaction",
    async (payload, { rejectWithValue }) => {
        try {
            
            // ✅ Send payload directly - Transaction.js will wrap it
            const response = await createTransaction(payload);  // ← NO WRAPPING HERE
            
            // ✅ Log full response to debug structure
            
            // ✅ Extract transaction from various possible response structures
            let transaction = null;
            
            if (response?.data?.transaction) {
                transaction = response.data.transaction;
            } else if (response?.transaction) {
                transaction = response.transaction;
            } else if (response?.data) {
                transaction = response.data;
            } else {
                transaction = response;
            }
            
            
            return transaction;
        } catch (err) {
            return rejectWithValue(err.message || "Save failed");
        }
    }
);


/**
 * Update transaction - PUT /Transaction/{ticketId}
 */
export const updateTransactionApi = createAsyncThunk(
    "weighing/updateTransactionApi",
    async ({ ticketId, data }, { rejectWithValue }) => {
        try {
            const response = await updateTransaction(ticketId, data);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Delete transaction - DELETE /Transaction/{ticketId}
 */
export const deactivateTransactionApi = createAsyncThunk(
    "weighing/deactivateTransactionApi",
    async (ticketId, { rejectWithValue }) => {
        try {
            await deleteTransaction(ticketId);
            return { ticketId };
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Add second weight - POST /Transaction/add-second-weight
 */
export const addSecondWeight = createAsyncThunk(
    "weighing/addSecondWeight",
    async (payload, { rejectWithValue }) => {
        try {
            // Map payload to match Swagger AddSecondWeightDto
            const weighingData = {
                ticketID: payload.transactionId || payload.ticketID,
                secondWeight: String(payload.weight || payload.secondWeight),
                weighBridgeName2nd: payload.weighBridgeName || payload.weighBridgeName2nd,
                scaleName2nd: payload.scaleName || payload.scaleName2nd || "Scale-01",
                operatorID2nd: String(payload.operatorId || payload.operatorID2nd || ""),
                operatorName2nd: payload.operatorName || payload.operatorName2nd || "",
                notes: payload.notes || "",
            };
            
            const response = await addWeighing(weighingData);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Complete transaction - POST /Transaction/complete
 */
export const completeTransactionThunk = createAsyncThunk(
    "weighing/completeTransaction",
    async (payload, { rejectWithValue }) => {
        try {
            // Match Swagger CompleteTransactionDto
            const data = {
                ticketID: payload.transactionId || payload.ticketID
            };
            const response = await completeTransaction(data);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Get transaction by receipt - GET /Transaction/receipt/{receiptNo}
 */
export const fetchTransactionByReceipt = createAsyncThunk(
    "weighing/fetchTransactionByReceipt",
    async (receiptNo, { rejectWithValue }) => {
        try {
            const response = await getTransactionByReceipt(receiptNo);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Get incomplete by vehicle plate - GET /Transaction/incomplete/vehicle/{noPlate}
 */
export const fetchIncompleteByPlate = createAsyncThunk(
    "weighing/fetchIncompleteByPlate",
    async (noPlate, { rejectWithValue }) => {
        try {
            const response = await getIncompleteByVehicleNo(noPlate);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Get incomplete by vehicle ID - GET /Transaction/incomplete/vehicle-id/{vehicleId}
 */
export const fetchIncompleteByVehicleIdThunk = createAsyncThunk(
    "weighing/fetchIncompleteByVehicleId",
    async (vehicleId, { rejectWithValue }) => {
        try {
            const response = await getIncompleteByVehicleId(vehicleId);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Get by status - GET /Transaction/status/{status}
 */
export const fetchTransactionsByStatus = createAsyncThunk(
    "weighing/fetchTransactionsByStatus",
    async ({ status, limit = 100 }, { rejectWithValue }) => {
        try {
            const response = await getTransactionsByStatus(status, limit);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Request reweigh - POST /Transaction/request-reweigh
 */
export const requestReweighThunk = createAsyncThunk(
    "weighing/requestReweigh",
    async (payload, { rejectWithValue }) => {
        try {
            const data = {
                ticketID: payload.transactionId || payload.ticketID,
                reason: payload.reason || ""
            };
            const response = await requestReweigh(data);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Get reweigh records - GET /Transaction/{ticketId}/reweigh-records
 */
export const fetchReweighRecords = createAsyncThunk(
    "weighing/fetchReweighRecords",
    async (ticketId, { rejectWithValue }) => {
        try {
            const response = await getReweighRecords(ticketId);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Approve reweigh - POST /Transaction/approve-reweigh
 * Clears second weight, resets transaction status to Active
 */
export const approveReweighThunk = createAsyncThunk(
    "weighing/approveReweigh",
    async (payload, { rejectWithValue }) => {
        try {
            const data = {
                ticketID: payload.ticketID || payload.transactionId,
                approvedBy: payload.approvedBy || "",
                notes: payload.notes || ""
            };
            const response = await approveReweigh(data);
            return { response, ticketID: data.ticketID };
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Reject reweigh - POST /Transaction/reject-reweigh
 * Keeps original weights, restores transaction to Completed
 */
export const rejectReweighThunk = createAsyncThunk(
    "weighing/rejectReweigh",
    async (payload, { rejectWithValue }) => {
        try {
            const data = {
                ticketID: payload.ticketID || payload.transactionId,
                rejectionReason: payload.rejectionReason || payload.reason || "",
                rejectedBy: payload.rejectedBy || "",
                notes: payload.notes || ""
            };
            const response = await rejectReweigh(data);
            return { response, ticketID: data.ticketID };
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

/**
 * Check receipt - GET /Transaction/check-receipt/{receiptNo}
 */
export const checkReceiptThunk = createAsyncThunk(
    "weighing/checkReceipt",
    async (receiptNo, { rejectWithValue }) => {
        try {
            const response = await checkReceipt(receiptNo);
            return response;
        } catch (err) {
            return rejectWithValue(err.message);
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// SLICE
// ─────────────────────────────────────────────────────────────────────────────
const initialState = {
    vehicles: [],
    drivers: [],
    products: [],
    routes: [],
    suppliers: [],
    saccos: [],
    weighbridges: [],
    transporters: [],
    transactions: [],
    total: 0,
    incompleteTransactions: [],
    reweighRecords: [],
    
    users: [],
    currentUser: null,

    currentWeight: null,
    vehiclePosition: null,
    detectedPlate: null,
    capturedWeight: null,
    loading: false,
    error: null,
};

const weighingSlice = createSlice({
    name: "weighing",
    initialState,
    reducers: {
        setDetectedPlate: (state, action) => {
            state.detectedPlate = action.payload;
        },
        setCapturedWeight: (state, action) => {
            state.capturedWeight = action.payload;
        },
        clearError: (state) => {
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        const pending = (state) => {
            state.loading = true;
            state.error = null;
        };

        const rejected = (state, action) => {
            state.loading = false;
            state.error = action.payload || "An error occurred";
        };

        builder
            // Always reset loading/error on rehydration so a persisted
            // loading:true can never lock the UI on next app start
            .addCase(REHYDRATE, (state) => {
                state.loading = false;
                state.error = null;
            })

            // Simulated weight
            .addCase(fetchSimulatedWeight.pending, pending)
            .addCase(fetchSimulatedWeight.fulfilled, (state, action) => {
                state.loading = false;
                state.currentWeight = action.payload.weight;
                state.vehiclePosition = action.payload.position;
            })
            .addCase(fetchSimulatedWeight.rejected, rejected)

            // Vehicles
            .addCase(fetchVehicles.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
            .addCase(fetchVehiclesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
            .addCase(fetchVehiclesByRegNumber.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
            .addCase(fetchVehicleById.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })

            // Drivers
            .addCase(fetchDrivers.fulfilled, (state, action) => {
                state.loading = false;
                state.drivers = action.payload;
            })
            .addCase(fetchDriversByName.fulfilled, (state, action) => {
                state.loading = false;
                state.drivers = action.payload;
            })
            .addCase(fetchDriverById.fulfilled, (state, action) => {
                state.loading = false;
                state.drivers = action.payload;
            })

            // Products
            .addCase(fetchProducts.fulfilled, (state, action) => {
                state.loading = false;
                state.products = action.payload;
            })
            .addCase(fetchProductsByName.fulfilled, (state, action) => {
                state.loading = false;
                state.products = action.payload;
            })

            // Routes
            .addCase(fetchRoutes.fulfilled, (state, action) => {
                state.loading = false;
                state.routes = action.payload;
            })
            .addCase(fetchRoutesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.routes = action.payload;
            })

            // Saccos
            .addCase(fetchSaccosByName.fulfilled, (state, action) => {
                state.loading = false;
                state.saccos = action.payload;
            })
            .addCase(fetchSaccoById.fulfilled, (state, action) => {
                state.loading = false;
                state.saccos = action.payload;
            })

            // Suppliers
            .addCase(fetchSuppliers.fulfilled, (state, action) => {
                state.loading = false;
                state.suppliers = action.payload;
            })
            .addCase(fetchSuppliersByName.fulfilled, (state, action) => {
                state.loading = false;
                state.suppliers = action.payload;
            })
            .addCase(fetchSupplierById.fulfilled, (state, action) => {
                state.loading = false;
                state.suppliers = action.payload;
            })

            // Transporters
            .addCase(fetchTransporters.fulfilled, (state, action) => {
                state.loading = false;
                state.transporters = action.payload;
            })
            .addCase(fetchTransportersByName.fulfilled, (state, action) => {
                state.loading = false;
                state.transporters = action.payload;
            })
            .addCase(fetchTransporterById.fulfilled, (state, action) => {
                state.loading = false;
                state.transporters = action.payload;
            })

            // Weighbridges
            .addCase(fetchWeighbridges.pending, pending)
            .addCase(fetchWeighbridges.fulfilled, (state, action) => {
                state.loading = false;
                state.weighbridges = action.payload.items;
            })
            .addCase(fetchWeighbridges.rejected, rejected)

            .addCase(fetchWeighbridgesByName.pending, pending)
            .addCase(fetchWeighbridgesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.weighbridges = action.payload.items;
            })
            .addCase(fetchWeighbridgesByName.rejected, rejected)

            // Users
            .addCase(fetchUserById.pending, pending)
            .addCase(fetchUserById.fulfilled, (state, action) => {
                state.loading = false;
                if (action.payload) {
                    state.users = [action.payload];
                }
            })
            .addCase(fetchUserById.rejected, rejected)

            .addCase(fetchCurrentUser.pending, pending)
            .addCase(fetchCurrentUser.fulfilled, (state, action) => {
                state.loading = false;
                state.currentUser = action.payload;
            })
            .addCase(fetchCurrentUser.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload;
                state.currentUser = null;
            })

            // ✅ FIXED: TRANSACTIONS
            .addCase(fetchTransactions.pending, pending)
            .addCase(fetchTransactions.fulfilled, (state, action) => {
                state.loading = false;
                state.transactions = action.payload.items;
                state.total = action.payload.totalCount;
            })
            .addCase(fetchTransactions.rejected, rejected)

            .addCase(addTransaction.pending, pending)
            .addCase(addTransaction.fulfilled, (state, action) => {
                state.loading = false;
                
                const newTx = action.payload;
                
                
                if (newTx) {
                    // Get transaction ID (might be ticketID or id)
                    const txId = newTx.ticketID || newTx.id;
                    
                    // Check if transaction already exists
                    const existingIndex = state.transactions.findIndex(
                        t => (t.ticketID && t.ticketID === txId) || 
                             (t.id && t.id === txId)
                    );
                    
                    if (existingIndex === -1) {
                        // Add to beginning (newest first)
                        state.transactions.unshift(newTx);
                    } else {
                        // Update existing transaction
                        state.transactions[existingIndex] = { 
                            ...state.transactions[existingIndex], 
                            ...newTx 
                        };
                    }
                } else {
                }
            })
            .addCase(addTransaction.rejected, rejected)

            .addCase(updateTransactionApi.pending, pending)
            .addCase(updateTransactionApi.fulfilled, (state, action) => {
                state.loading = false;
                const updated = action.payload.data || action.payload;
                const idx = state.transactions.findIndex(t => t.id === updated.id || t.ticketID === updated.ticketID);
                if (idx !== -1) state.transactions[idx] = { ...state.transactions[idx], ...updated };
            })
            .addCase(updateTransactionApi.rejected, rejected)

            .addCase(deactivateTransactionApi.pending, pending)
            .addCase(deactivateTransactionApi.fulfilled, (state, action) => {
                state.loading = false;
                const tx = state.transactions.find(t => t.id === action.payload.ticketId || t.ticketID === action.payload.ticketId);
                if (tx) tx.active = false;
            })
            .addCase(deactivateTransactionApi.rejected, rejected)

            .addCase(addSecondWeight.pending, pending)
            .addCase(addSecondWeight.fulfilled, (state, action) => {
    state.loading = false;
    
    const updated = action.payload.data || action.payload;
    
    // ✅ CRITICAL FIX: Search by ticketID, not id
    const txId = updated.ticketID || updated.id;
    const idx = state.transactions.findIndex(
        t => t.ticketID === txId || t.id === txId
    );
    
    if (idx !== -1) {
        // Update the transaction with new data
        state.transactions[idx] = {
            ...state.transactions[idx],
            ...updated,
            isCompleted: true,
            completed: true,
            status: updated.status ?? state.transactions[idx].status ?? 'Completed',
        };
        
    } else {
    }
})
            .addCase(addSecondWeight.rejected, rejected)

            .addCase(completeTransactionThunk.pending, pending)
            .addCase(completeTransactionThunk.fulfilled, (state, action) => {
                state.loading = false;
                const completedTx = action.payload.data || action.payload;
                const tx = state.transactions.find(t => t.id === completedTx.id || t.id === completedTx.ticketID);
                if (tx) tx.isCompleted = true;
            })
            .addCase(completeTransactionThunk.rejected, rejected)

            .addCase(fetchTransactionByReceipt.pending, pending)
            .addCase(fetchTransactionByReceipt.fulfilled, (state, action) => {
                state.loading = false;
                // Optionally store in a separate field or update transactions
            })
            .addCase(fetchTransactionByReceipt.rejected, rejected)

            .addCase(fetchIncompleteByPlate.pending, pending)
            .addCase(fetchIncompleteByPlate.fulfilled, (state, action) => {
                state.loading = false;
                state.incompleteTransactions = Array.isArray(action.payload) 
                    ? action.payload 
                    : action.payload?.data || [];
            })
            .addCase(fetchIncompleteByPlate.rejected, rejected)

            .addCase(fetchIncompleteByVehicleIdThunk.pending, pending)
            .addCase(fetchIncompleteByVehicleIdThunk.fulfilled, (state, action) => {
                state.loading = false;
                state.incompleteTransactions = Array.isArray(action.payload) 
                    ? action.payload 
                    : action.payload?.data || [];
            })
            .addCase(fetchIncompleteByVehicleIdThunk.rejected, rejected)

            .addCase(fetchTransactionsByStatus.pending, pending)
            .addCase(fetchTransactionsByStatus.fulfilled, (state, action) => {
                state.loading = false;
                state.transactions = Array.isArray(action.payload) 
                    ? action.payload 
                    : action.payload?.data || [];
            })
            .addCase(fetchTransactionsByStatus.rejected, rejected)

            .addCase(requestReweighThunk.pending, pending)
            .addCase(requestReweighThunk.fulfilled, (state, action) => {
                state.loading = false;
                // Mark the transaction as ReweighRequested in local state
                const ticketID = action.meta?.arg?.ticketID || action.meta?.arg?.transactionId;
                if (ticketID) {
                    const idx = state.transactions.findIndex(
                        t => t.ticketID === ticketID || t.id === ticketID
                    );
                    if (idx !== -1) {
                        state.transactions[idx] = {
                            ...state.transactions[idx],
                            status: 'ReweighRequested'
                        };
                    }
                }
            })
            .addCase(requestReweighThunk.rejected, rejected)

            .addCase(fetchReweighRecords.pending, pending)
            .addCase(fetchReweighRecords.fulfilled, (state, action) => {
                state.loading = false;
                state.reweighRecords = Array.isArray(action.payload)
                    ? action.payload
                    : action.payload?.data || [];
            })
            .addCase(fetchReweighRecords.rejected, rejected)

            .addCase(approveReweighThunk.pending, pending)
            .addCase(approveReweighThunk.fulfilled, (state, action) => {
                state.loading = false;
                // Reset transaction to Active (backend clears second weight)
                const { ticketID, response } = action.payload;
                const updated = response?.data || response;
                const idx = state.transactions.findIndex(
                    t => t.ticketID === ticketID || t.id === ticketID
                );
                if (idx !== -1) {
                    state.transactions[idx] = {
                        ...state.transactions[idx],
                        ...(updated && typeof updated === 'object' ? updated : {}),
                        status: 'Active',
                        isCompleted: false,
                        completed: false,
                        secondWeight: null,
                        netWeight: null
                    };
                }
            })
            .addCase(approveReweighThunk.rejected, rejected)

            .addCase(rejectReweighThunk.pending, pending)
            .addCase(rejectReweighThunk.fulfilled, (state, action) => {
                state.loading = false;
                // Restore transaction to Completed (backend keeps original weights)
                const { ticketID, response } = action.payload;
                const updated = response?.data || response;
                const idx = state.transactions.findIndex(
                    t => t.ticketID === ticketID || t.id === ticketID
                );
                if (idx !== -1) {
                    state.transactions[idx] = {
                        ...state.transactions[idx],
                        ...(updated && typeof updated === 'object' ? updated : {}),
                        status: 'Completed',
                        isCompleted: true,
                        completed: true
                    };
                }
            })
            .addCase(rejectReweighThunk.rejected, rejected)

            .addCase(checkReceiptThunk.pending, pending)
            .addCase(checkReceiptThunk.fulfilled, (state, action) => {
                state.loading = false;
            })
            .addCase(checkReceiptThunk.rejected, rejected);
    },
});

export const {
    setDetectedPlate,
    setCapturedWeight,
    clearError,
} = weighingSlice.actions;

export default weighingSlice.reducer;