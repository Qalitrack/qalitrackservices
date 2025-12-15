// src/features/weighing/weighingSlice.js
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../api/helpers/apiClients";

// Import REAL transaction API helpers (updated to match your new file)
import {
    getTransactions,
    getTransactionById as apiGetTransaction,
    createTransaction,
    updateTransaction as apiUpdateTransaction,
    deleteTransaction, // ← renamed from apiDeleteTransaction
    addWeighing as apiAddWeighing,
    completeTransaction as apiCompleteTransaction,
} from "../api/MasterData/Transaction";

// ─────────────────────────────────────────────────────────────────────────────
// SIMULATED WEIGHT (KEEPING YOUR ORIGINAL — VERY USEFUL FOR TESTING)
// ─────────────────────────────────────────────────────────────────────────────
export const fetchSimulatedWeight = createAsyncThunk(
    "weighing/fetchSimulatedWeight",
    async (_, { rejectWithValue }) => {
        try {
            const weight = Math.floor(Math.random() * 50000) + 10000;
            const position = Math.random() < 0.7 ? "Fully On" : "Partially On";
            console.log("Simulated weight:", weight, "kg, Position:", position);
            return { weight, position };
        } catch (error) {
            return rejectWithValue("Simulation failed");
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// MASTER DATA THUNKS — ALL KEPT EXACTLY AS YOU HAD THEM
// ─────────────────────────────────────────────────────────────────────────────
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

export const fetchDrivers = createAsyncThunk("weighing/fetchDrivers", async (_, { rejectWithValue }) => {
    try {
        const response = await apiClient.get("/MasterData/Drivers", { params: { pageNumber: 1, pageSize: 50 } });
        const data = response.data;
        return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
        return rejectWithValue(error.response?.data?.message || error.message);
    }
});

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

export const fetchProducts = createAsyncThunk("weighing/fetchProducts", async (_, { rejectWithValue }) => {
    try {
        const response = await apiClient.get("/MasterData/Products", { params: { pageNumber: 1, pageSize: 50 } });
        return response.data?.items || (Array.isArray(response.data) ? response.data : []);
    } catch (error) {
        return rejectWithValue(error.response?.data?.message || error.message);
    }
});

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

export const fetchRoutes = createAsyncThunk("weighing/fetchRoutes", async (_, { rejectWithValue }) => {
    try {
        const response = await apiClient.get("/MasterData/Routes", { params: { pageNumber: 1, pageSize: 50 } });
        return response.data?.items || (Array.isArray(response.data) ? response.data : []);
    } catch (error) {
        return rejectWithValue(error.response?.data?.message || error.message);
    }
});

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

export const fetchSuppliers = createAsyncThunk("weighing/fetchSuppliers", async (_, { rejectWithValue }) => {
    try {
        const response = await apiClient.get("/MasterData/Suppliers", { params: { pageNumber: 1, pageSize: 50 } });
        return response.data?.items || (Array.isArray(response.data) ? response.data : []);
    } catch (error) {
        return rejectWithValue(error.response?.data?.message || error.message);
    }
});

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

export const fetchTransporters = createAsyncThunk("weighing/fetchTransporters", async (_, { rejectWithValue }) => {
    try {
        const response = await apiClient.get("/MasterData/Transporters", { params: { pageNumber: 1, pageSize: 50 } });
        return response.data?.items || (Array.isArray(response.data) ? response.data : []);
    } catch (error) {
        return rejectWithValue(error.response?.data?.message || error.message);
    }
});

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
// REAL TRANSACTION THUNKS — USING YOUR CLEAN API HELPERS
// ─────────────────────────────────────────────────────────────────────────────
export const fetchTransactions = createAsyncThunk(
    "weighing/fetchTransactions",
    async (filters = {}, { rejectWithValue }) => {
        try {
            const response = await getTransactions(filters);
            console.log(response);
            const items = response?.data?.items || response?.data || [];
            return Array.isArray(items) ? items : [];
        } catch (error) {
            console.error("Failed to fetch transactions:", error);
            return rejectWithValue(error.message || "Failed to load transactions");
        }
    }
);

export const addTransaction = createAsyncThunk(
    "weighing/addTransaction",
    async (payload, { rejectWithValue }) => {
        try {
            console.log("Creating transaction:", payload);
            const response = await createTransaction(payload);
            return response; // response.data from API
        } catch (err) {
            console.error("Transaction creation failed:", err.response?.data || err);
            return rejectWithValue(err.response?.data || err.message || "Save failed");
        }
    }
);

export const updateTransactionApi = createAsyncThunk(
    "weighing/updateTransactionApi",
    async ({ id, data }, { rejectWithValue }) => {
        try {
            const response = await updateTransaction(id, data);
            return response;
        } catch (err) {
            return rejectWithValue(err.response?.data || err.message);
        }
    }
);

export const deactivateTransactionApi = createAsyncThunk(
    "weighing/deactivateTransactionApi",
    async (transactionId, { rejectWithValue }) => {
        try {
            await deleteTransaction(transactionId); // uses the correct export
            return { transactionId };
        } catch (err) {
            return rejectWithValue(err.response?.data || err.message);
        }
    }
);

export const addWeighing = createAsyncThunk(
    "weighing/addWeighing",
    async (payload, { rejectWithValue }) => {
        try {
            const response = await apiAddWeighing(payload);
            return response;
        } catch (err) {
            return rejectWithValue(err.response?.data || err.message);
        }
    }
);

export const completeTransaction = createAsyncThunk(
    "weighing/completeTransaction",
    async (payload, { rejectWithValue }) => {
        try {
            const response = await apiCompleteTransaction(payload);
            return response;
        } catch (err) {
            return rejectWithValue(err.response?.data || err.message);
        }
    }
);
// ─────────────────────────────────────────────────────────────────────────────
// WEIGHBRIDGES — REAL ENDPOINT: /Weighbridges
// ─────────────────────────────────────────────────────────────────────────────
export const fetchWeighbridges = createAsyncThunk(
    "weighing/fetchWeighbridges",
    async ({ pageNumber = 1, pageSize = 100 } = {}, { rejectWithValue }) => {
        try {
            const response = await apiClient.get("/MasterData/Weighbridges", {
                params: {
                    pageNumber,
                    pageSize,
                },
            });

            const items = response.data?.items || [];
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

            const response = await apiClient.get("/Weighbridges", {
                params: {
                    pageNumber: 1,
                    pageSize: 30, // Good for dropdown search
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
// SLICE — WITH OPTIMISTIC UPDATES
// ─────────────────────────────────────────────────────────────────────────────
const initialState = {
    vehicles: [],
    drivers: [],
    products: [],
    routes: [],
    suppliers: [],
    saccos: [],
    weighbridges: [],        // ← ADD THIS
    transporters: [],
    transactions: [],
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
        completeWeighing: (state, action) => {
            const { transactionId } = action.payload;
            const tx = state.transactions.find(t => t.id === transactionId);
            if (tx) tx.isCompleted = true;
        },
        deactivateTransaction: (state, action) => {
            const { transactionId } = action.payload;
            const tx = state.transactions.find(t => t.id === transactionId);
            if (tx) tx.active = false;
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
            // Simulated weight
            .addCase(fetchSimulatedWeight.pending, pending)
            .addCase(fetchSimulatedWeight.fulfilled, (state, action) => {
                state.loading = false;
                state.currentWeight = action.payload.weight;
                state.vehiclePosition = action.payload.position;
            })
            .addCase(fetchSimulatedWeight.rejected, rejected)

            // Master data fulfilled cases (kept minimal — you can expand if needed)
            .addCase(fetchVehicles.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
            .addCase(fetchVehiclesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })

            // Weighbridges - Load all
            .addCase(fetchWeighbridges.pending, pending)
            .addCase(fetchWeighbridges.fulfilled, (state, action) => {
                state.loading = false;
                state.weighbridges = action.payload.items;
            })
            .addCase(fetchWeighbridges.rejected, rejected)

            // Weighbridges - Search
            .addCase(fetchWeighbridgesByName.pending, pending)
            .addCase(fetchWeighbridgesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.weighbridges = action.payload.items;
            })
            .addCase(fetchWeighbridgesByName.rejected, rejected)
            .addCase(fetchVehiclesByRegNumber.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
            .addCase(fetchVehicleById.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicles = action.payload;
            })
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
            .addCase(fetchProducts.fulfilled, (state, action) => {
                state.loading = false;
                state.products = action.payload;
            })
            .addCase(fetchProductsByName.fulfilled, (state, action) => {
                state.loading = false;
                state.products = action.payload;
            })
            .addCase(fetchRoutes.fulfilled, (state, action) => {
                state.loading = false;
                state.routes = action.payload;
            })
            .addCase(fetchRoutesByName.fulfilled, (state, action) => {
                state.loading = false;
                state.routes = action.payload;
            })
            .addCase(fetchSaccosByName.fulfilled, (state, action) => {
                state.loading = false;
                state.saccos = action.payload;
            })
            .addCase(fetchSaccoById.fulfilled, (state, action) => {
                state.loading = false;
                state.saccos = action.payload;
            })
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

            // Transactions
            .addCase(fetchTransactions.pending, pending)
            .addCase(fetchTransactions.fulfilled, (state, action) => {
                state.loading = false;
                state.transactions = action.payload;
            })
            .addCase(fetchTransactions.rejected, rejected)

            .addCase(addTransaction.pending, pending)
            .addCase(addTransaction.fulfilled, (state, action) => {
                state.loading = false;
                const newTx = action.payload.data || action.payload;
                if (newTx && !state.transactions.find(t => t.id === newTx.id)) {
                    state.transactions.unshift(newTx);
                }
            })
            .addCase(addTransaction.rejected, rejected)

            .addCase(updateTransactionApi.fulfilled, (state, action) => {
                state.loading = false;
                const updated = action.payload.data || action.payload;
                const idx = state.transactions.findIndex(t => t.id === updated.id);
                if (idx !== -1) state.transactions[idx] = { ...state.transactions[idx], ...updated };
            })

            .addCase(deactivateTransactionApi.fulfilled, (state, action) => {
                state.loading = false;
                const tx = state.transactions.find(t => t.id === action.payload.transactionId);
                if (tx) tx.active = false;
            })

            .addCase(addWeighing.fulfilled, (state, action) => {
                state.loading = false;
                // Optionally update transaction weights here if API returns updated tx
                18n
            })

            .addCase(completeTransaction.fulfilled, (state, action) => {
                state.loading = false;
                const completedTx = action.payload.data || action.payload;
                const tx = state.transactions.find(t => t.id === completedTx.id);
                if (tx) tx.isCompleted = true;
            });
    },
});

export const {
    setDetectedPlate,
    setCapturedWeight,
    completeWeighing,
    deactivateTransaction,
} = weighingSlice.actions;

export default weighingSlice.reducer;