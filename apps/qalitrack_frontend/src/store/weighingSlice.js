import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient, transactionsClient } from "../api/helpers/apiClients";

const handleApiError = (error) => error.message || "API Error";

// ─────────────────────────────────────────────────────────────────────────────
// SIMULATED WEIGHT
// ─────────────────────────────────────────────────────────────────────────────
export const fetchSimulatedWeight = createAsyncThunk(
  "weighing/fetchSimulatedWeight",
  async (_, { rejectWithValue }) => {
    try {
      const weight = Math.floor(Math.random() * 50000);
      const position = Math.random() < 0.7 ? "Fully On" : "Partially On";
      console.log("Simulated weight:", weight, "kg, Position:", position);
      return { weight, position };
    } catch (error) {
      console.error("Error simulating weight:", error.message);
      return rejectWithValue(error.message || "Simulation error");
    }
  }
);

// ─────────────────────────────────────────────────────────────────────────────
// MASTER DATA (Uses apiClient)
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
      return rejectWithValue(error.message || "Network error");
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
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchVehicleById = createAsyncThunk(
  "weighing/fetchVehicleById",
  async (id, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/MasterData/Vehicles/${id}`);
      const data = response.data?.data?.vehicle || response.data?.vehicle || response.data;
      return data ? [data] : rejectWithValue("Invalid vehicle response");
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchDrivers = createAsyncThunk(
  "weighing/fetchDrivers",
  async (_, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Drivers", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchDriverById = createAsyncThunk(
  "weighing/fetchDriverById",
  async (id, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/MasterData/Drivers/${id}`);
      const data = response.data?.data?.driver || response.data?.driver || response.data;
      return data ? [data] : rejectWithValue("Invalid driver response");
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchProducts = createAsyncThunk(
  "weighing/fetchProducts",
  async (_, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Products", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchProductsByName = createAsyncThunk(
  "weighing/fetchProductsByName",
  async (name, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Products", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchRoutes = createAsyncThunk(
  "weighing/fetchRoutes",
  async (_, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Routes", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSaccoById = createAsyncThunk(
  "weighing/fetchSaccoById",
  async (id, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/MasterData/Saccos/${id}`);
      const data = response.data?.data?.sacco || response.data?.sacco || response.data;
      return data ? [data] : rejectWithValue("Invalid sacco response");
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSuppliers = createAsyncThunk(
  "weighing/fetchSuppliers",
  async (_, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Suppliers", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSupplierById = createAsyncThunk(
  "weighing/fetchSupplierById",
  async (id, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/MasterData/Suppliers/${id}`);
      const data = response.data?.supplier || response.data;
      return data ? [data] : rejectWithValue("Invalid supplier response");
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchTransporters = createAsyncThunk(
  "weighing/fetchTransporters",
  async (_, { rejectWithValue }) => {
    try {
      const response = await apiClient.get("/MasterData/Transporters", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
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
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchTransporterById = createAsyncThunk(
  "weighing/fetchTransporterById",
  async (id, { rejectWithValue }) => {
    try {
      const response = await apiClient.get(`/MasterData/Transporters/${id}`);
      const data = response.data?.transporter || response.data;
      return data ? [data] : rejectWithValue("Invalid transporter response");
    } catch (error) {
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// ─────────────────────────────────────────────────────────────────────────────
// TRANSACTION API (Uses transactionsClient)
// ─────────────────────────────────────────────────────────────────────────────

// 1. Fetch transactions list
export const fetchTransactions = createAsyncThunk(
  "weighing/fetchTransactions",
  async (_, { rejectWithValue }) => {
    try {
      const res = await transactionsClient.get("");
      return res.data;
    } catch (error) {
      return rejectWithValue(handleApiError(error));
    }
  }
);

// 2. ADD TRANSACTION (API THUNK - CORRECT VERSION)
export const addTransaction = createAsyncThunk(
  "weighing/addTransaction",
  async (payload, { rejectWithValue }) => {
    try {
      console.log("Saving transaction payload:", payload);
      // This is the call that hits the backend
      const res = await transactionsClient.post("", payload); 
      return res.data;
    } catch (err) {
      console.error("Save failed:", err.response?.data || err.message);
      return rejectWithValue(err.response?.data || err.message);
    }
  }
);

// 3. Update transaction
export const updateTransactionApi = createAsyncThunk(
  "weighing/updateTransactionApi",
  async ({ id, data }, { rejectWithValue }) => {
    try {
      const res = await transactionsClient.put(`/${id}`, data);
      return res.data;
    } catch (error) {
      return rejectWithValue(handleApiError(error));
    }
  }
);

// 4. Deactivate transaction
export const deactivateTransactionApi = createAsyncThunk(
  "weighing/deactivateTransactionApi",
  async (transactionId, { rejectWithValue }) => {
    try {
      const res = await transactionsClient.delete(`/${transactionId}`);
      return res.data;
    } catch (error) {
      return rejectWithValue(handleApiError(error));
    }
  }
);


// 5. Placeholder thunks for other references in extraReducers
export const addWeighing = createAsyncThunk(
  "weighing/addWeighing",
  async (payload, { rejectWithValue }) => {
    try {
      return payload;
    } catch (error) {
      return rejectWithValue(handleApiError(error));
    }
  }
);

export const completeTransaction = createAsyncThunk(
  "weighing/completeTransaction",
  async (id, { rejectWithValue }) => {
    try {
      return id;
    } catch (error) {
      return rejectWithValue(handleApiError(error));
    }
  }
);


// ─────────────────────────────────────────────────────────────────────────────
// SLICE STATE AND REDUCERS 
// ─────────────────────────────────────────────────────────────────────────────
const initialState = {
  vehicles: [],
  drivers: [],
  products: [],
  routes: [],
  suppliers: [],
  saccos: [],
  transporters: [],
  transactions: [], // Initialize as an array
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
    // 💥 REMOVED: local 'addTransaction' reducer to avoid conflict with the async thunk
    completeWeighing: (state, action) => {
      const { transactionId } = action.payload;
      const transaction = state.transactions.find((t) => t.id === transactionId);
      if (transaction) {
        transaction.completed = true;
      } else {
        console.warn(`Transaction with ID ${transactionId} not found`);
      }
    },
    deactivateTransaction: (state, action) => {
      const { transactionId } = action.payload;
      const transaction = state.transactions.find((t) => t.id === transactionId);
      if (transaction) {
        transaction.active = false;
      } else {
        console.warn(`Transaction with ID ${transactionId} not found`);
      }
    },
    setCapturedWeight: (state, action) => {
      state.capturedWeight = action.payload;
    },
  },
  extraReducers: (builder) => {
    const handlePending = (state) => {
      state.loading = true;
      state.error = null;
    };
    const handleRejected = (state, action) => {
      state.loading = false;
      state.error = action.payload || action.error.message;
    };

    builder
      // Simulated Weight
      .addCase(fetchSimulatedWeight.pending, handlePending)
      .addCase(fetchSimulatedWeight.fulfilled, (state, action) => {
        state.currentWeight = action.payload.weight;
        state.vehiclePosition = action.payload.position;
        state.loading = false;
      })
      .addCase(fetchSimulatedWeight.rejected, handleRejected)

      // Vehicles
      .addCase(fetchVehicles.pending, handlePending)
      .addCase(fetchVehicles.fulfilled, (state, action) => {
        state.vehicles = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchVehicles.rejected, handleRejected)
      .addCase(fetchVehiclesByName.pending, handlePending)
      .addCase(fetchVehiclesByName.fulfilled, (state, action) => {
        state.vehicles = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchVehiclesByName.rejected, handleRejected)
      .addCase(fetchVehiclesByRegNumber.pending, handlePending)
      .addCase(fetchVehiclesByRegNumber.fulfilled, (state, action) => {
        state.vehicles = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchVehiclesByRegNumber.rejected, handleRejected)
      .addCase(fetchVehicleById.pending, handlePending)
      .addCase(fetchVehicleById.fulfilled, (state, action) => {
        state.vehicles = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchVehicleById.rejected, handleRejected)

      // Drivers
      .addCase(fetchDrivers.pending, handlePending)
      .addCase(fetchDrivers.fulfilled, (state, action) => {
        state.drivers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchDrivers.rejected, handleRejected)
      .addCase(fetchDriversByName.pending, handlePending)
      .addCase(fetchDriversByName.fulfilled, (state, action) => {
        state.drivers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchDriversByName.rejected, handleRejected)
      .addCase(fetchDriverById.pending, handlePending)
      .addCase(fetchDriverById.fulfilled, (state, action) => {
        state.drivers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchDriverById.rejected, handleRejected)

      // Products
      .addCase(fetchProducts.pending, handlePending)
      .addCase(fetchProducts.fulfilled, (state, action) => {
        state.products = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchProducts.rejected, handleRejected)
      .addCase(fetchProductsByName.pending, handlePending)
      .addCase(fetchProductsByName.fulfilled, (state, action) => {
        state.products = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchProductsByName.rejected, handleRejected)

      // Routes
      .addCase(fetchRoutes.pending, handlePending)
      .addCase(fetchRoutes.fulfilled, (state, action) => {
        state.routes = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchRoutes.rejected, handleRejected)
      .addCase(fetchRoutesByName.pending, handlePending)
      .addCase(fetchRoutesByName.fulfilled, (state, action) => {
        state.routes = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchRoutesByName.rejected, handleRejected)

      // Saccos
      .addCase(fetchSaccosByName.pending, handlePending)
      .addCase(fetchSaccosByName.fulfilled, (state, action) => {
        state.saccos = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchSaccosByName.rejected, handleRejected)
      .addCase(fetchSaccoById.pending, handlePending)
      .addCase(fetchSaccoById.fulfilled, (state, action) => {
        state.saccos = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchSaccoById.rejected, handleRejected)

      // Suppliers
      .addCase(fetchSuppliers.pending, handlePending)
      .addCase(fetchSuppliers.fulfilled, (state, action) => {
        state.suppliers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchSuppliers.rejected, handleRejected)
      .addCase(fetchSuppliersByName.pending, handlePending)
      .addCase(fetchSuppliersByName.fulfilled, (state, action) => {
        state.suppliers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchSuppliersByName.rejected, handleRejected)
      .addCase(fetchSupplierById.pending, handlePending)
      .addCase(fetchSupplierById.fulfilled, (state, action) => {
        state.suppliers = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchSupplierById.rejected, handleRejected)

      // Transporters
      .addCase(fetchTransporters.pending, handlePending)
      .addCase(fetchTransporters.fulfilled, (state, action) => {
        state.transporters = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchTransporters.rejected, handleRejected)
      .addCase(fetchTransportersByName.pending, handlePending)
      .addCase(fetchTransportersByName.fulfilled, (state, action) => {
        state.transporters = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchTransportersByName.rejected, handleRejected)
      .addCase(fetchTransporterById.pending, handlePending)
      .addCase(fetchTransporterById.fulfilled, (state, action) => {
        state.transporters = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchTransporterById.rejected, handleRejected)

      // Transactions
      .addCase(fetchTransactions.pending, handlePending)
      .addCase(fetchTransactions.fulfilled, (state, action) => {
        state.loading = false;
        const responseData = action.payload;

        const transactionsArray = 
          Array.isArray(responseData) ? responseData :
          responseData?.data?.items || 
          responseData?.items || 
          responseData?.data || 
          null;

        state.transactions = Array.isArray(transactionsArray) ? transactionsArray : [];
      })
      .addCase(fetchTransactions.rejected, handleRejected)

      // ADD TRANSACTION (API THUNK)
      .addCase(addTransaction.pending, handlePending)
      .addCase(addTransaction.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(addTransaction.rejected, handleRejected)

      // Other Transaction Actions
      .addCase(addWeighing.pending, handlePending) 
      .addCase(addWeighing.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(addWeighing.rejected, handleRejected)

      .addCase(completeTransaction.pending, handlePending) 
      .addCase(completeTransaction.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(completeTransaction.rejected, handleRejected)

      .addCase(deactivateTransactionApi.pending, handlePending)
      .addCase(deactivateTransactionApi.fulfilled, (state) => {
        state.loading = false;
      })
      .addCase(deactivateTransactionApi.rejected, handleRejected);
  },
});

export const {
  setDetectedPlate,
  // Removed addTransaction export
  completeWeighing,
  deactivateTransaction,
  setCapturedWeight,
} = weighingSlice.actions;

export default weighingSlice.reducer;