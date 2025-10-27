import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../api/helpers/apiClients"; // Ensure this matches your project structure

// Weighing Panel Simulation
export const fetchSimulatedWeight = createAsyncThunk(
  "weighing/fetchSimulatedWeight",
  async (_, { rejectWithValue }) => {
    try {
      const weight = Math.floor(Math.random() * 50000);
      const position = Math.random() < 0.7 ? "Fully On" : "Partially On";
      console.log("📡 Simulated weight:", weight, "kg, Position:", position);
      return { weight, position };
    } catch (error) {
      console.error("❌ Error simulating weight:", error.message);
      return rejectWithValue(error.message || "Simulation error");
    }
  }
);

// Vehicles
export const fetchVehicles = createAsyncThunk(
  "weighing/fetchVehicles",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching vehicles from API...");
      const response = await apiClient.get("/MasterData/Vehicles", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Vehicles fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching vehicles:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchVehiclesByName = createAsyncThunk(
  "weighing/fetchVehiclesByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching vehicles by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Vehicles", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Vehicles fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching vehicles by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchVehicleById = createAsyncThunk(
  "weighing/fetchVehicleById",
  async (id, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching vehicle ID: ${id}...`);
      const response = await apiClient.get(`/MasterData/Vehicles/${id}`);
      console.log("✅ Vehicle fetched successfully:", response.data);
      const data = response.data?.data?.vehicle || response.data?.vehicle || response.data;
      return data ? [data] : rejectWithValue("Invalid vehicle response");
    } catch (error) {
      console.error("❌ Error fetching vehicle:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchVehiclesByRegNumber = createAsyncThunk(
  "weighing/fetchVehiclesByRegNumber",
  async (regNumber, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching vehicles by registration number: ${regNumber}...`);
      const response = await apiClient.get("/MasterData/Vehicles", {
        params: { pageNumber: 1, pageSize: 50, regNumber },
      });
      console.log("✅ Vehicles fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching vehicles by reg number:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Drivers
export const fetchDrivers = createAsyncThunk(
  "weighing/fetchDrivers",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching drivers from API...");
      const response = await apiClient.get("/MasterData/Drivers", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Drivers fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching drivers:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchDriversByName = createAsyncThunk(
  "weighing/fetchDriversByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching drivers by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Drivers", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Drivers fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching drivers by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchDriverById = createAsyncThunk(
  "weighing/fetchDriverById",
  async (id, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching driver ID: ${id}...`);
      const response = await apiClient.get(`/MasterData/Drivers/${id}`);
      console.log("✅ Driver fetched successfully:", response.data);
      const data = response.data?.data?.driver || response.data?.driver || response.data;
      return data ? [data] : rejectWithValue("Invalid driver response");
    } catch (error) {
      console.error("❌ Error fetching driver:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Products
export const fetchProducts = createAsyncThunk(
  "weighing/fetchProducts",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching products from API...");
      const response = await apiClient.get("/MasterData/Products", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Products fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching products:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchProductsByName = createAsyncThunk(
  "weighing/fetchProductsByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching products by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Products", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Products fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching products by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Routes
export const fetchRoutes = createAsyncThunk(
  "weighing/fetchRoutes",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching routes from API...");
      const response = await apiClient.get("/MasterData/Routes", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Routes fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching routes:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchRoutesByName = createAsyncThunk(
  "weighing/fetchRoutesByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching routes by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Routes", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Routes fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching routes by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Saccos
export const fetchSaccosByName = createAsyncThunk(
  "weighing/fetchSaccosByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching saccos by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Saccos", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Saccos fetched successfully:", response.data);
      const data = response.data;
      return data?.data?.items || data?.items || (Array.isArray(data.data) ? data.data : []);
    } catch (error) {
      console.error("❌ Error fetching saccos by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSaccoById = createAsyncThunk(
  "weighing/fetchSaccoById",
  async (id, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching sacco ID: ${id}...`);
      const response = await apiClient.get(`/MasterData/Saccos/${id}`);
      console.log("✅ Sacco fetched successfully:", response.data);
      const data = response.data?.data?.sacco || response.data?.sacco || response.data;
      return data ? [data] : rejectWithValue("Invalid sacco response");
    } catch (error) {
      console.error("❌ Error fetching sacco:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Suppliers
export const fetchSuppliers = createAsyncThunk(
  "weighing/fetchSuppliers",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching suppliers from API...");
      const response = await apiClient.get("/MasterData/Suppliers", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Suppliers fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching suppliers:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSuppliersByName = createAsyncThunk(
  "weighing/fetchSuppliersByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching suppliers by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Suppliers", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Suppliers fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching suppliers by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchSupplierById = createAsyncThunk(
  "weighing/fetchSupplierById",
  async (id, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching supplier ID: ${id}...`);
      const response = await apiClient.get(`/MasterData/Suppliers/${id}`);
      console.log("✅ Supplier fetched successfully:", response.data);
      const data = response.data?.supplier || response.data;
      return data ? [data] : rejectWithValue("Invalid supplier response");
    } catch (error) {
      console.error("❌ Error fetching supplier:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Transporters
export const fetchTransporters = createAsyncThunk(
  "weighing/fetchTransporters",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching transporters from API...");
      const response = await apiClient.get("/MasterData/Transporters", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Transporters fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching transporters:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchTransportersByName = createAsyncThunk(
  "weighing/fetchTransportersByName",
  async (name, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching transporters by name: ${name}...`);
      const response = await apiClient.get("/MasterData/Transporters", {
        params: { pageNumber: 1, pageSize: 50, searchTerm: name },
      });
      console.log("✅ Transporters fetched successfully:", response.data);
      const data = response.data;
      return data?.items || (Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("❌ Error fetching transporters by name:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

export const fetchTransporterById = createAsyncThunk(
  "weighing/fetchTransporterById",
  async (id, { rejectWithValue }) => {
    try {
      console.log(`📡 Fetching transporter ID: ${id}...`);
      const response = await apiClient.get(`/MasterData/Transporters/${id}`);
      console.log("✅ Transporter fetched successfully:", response.data);
      const data = response.data?.transporter || response.data;
      return data ? [data] : rejectWithValue("Invalid transporter response");
    } catch (error) {
      console.error("❌ Error fetching transporter:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Transactions (Placeholder)
export const fetchTransactions = createAsyncThunk(
  "weighing/fetchTransactions",
  async (_, { rejectWithValue }) => {
    try {
      console.log("📡 Fetching transactions from API...");
      const response = await apiClient.get("/MasterData/Transactions", {
        params: { pageNumber: 1, pageSize: 50 },
      });
      console.log("✅ Transactions fetched successfully:", response.data);
      return response.data?.items || [];
    } catch (error) {
      console.error("❌ Error fetching transactions:", error.message, error.response?.data);
      if (error.response) {
        console.error("🔢 Status:", error.response.status);
        console.error("📦 Response data:", error.response.data);
      }
      return rejectWithValue(error.message || "Network error");
    }
  }
);

// Slice
const initialState = {
  vehicles: [],
  drivers: [],
  products: [],
  routes: [],
  suppliers: [],
  saccos: [],
  transporters: [],
  transactions: [],
  currentWeight: null, // For real-time weight
  vehiclePosition: null, // For positioning status
  detectedPlate: null, // For camera-detected number plate
  capturedWeight: null, // For captured weight to prefill form
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
    addTransaction: (state, action) => {
      state.transactions.push(action.payload);
    },
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
      // Weighing Panel Simulation
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
        state.transactions = action.payload || [];
        state.loading = false;
      })
      .addCase(fetchTransactions.rejected, handleRejected);
  },
});

export const { setDetectedPlate, addTransaction, completeWeighing, deactivateTransaction, setCapturedWeight } =
  weighingSlice.actions;
export default weighingSlice.reducer;