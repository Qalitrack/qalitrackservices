import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import axios from "axios";

// ----------------------
// Async Thunks
// ----------------------

// Vehicles
export const fetchVehicles = createAsyncThunk(
  "weighing/fetchVehicles",
  async () => {
    const response = await axios.get("/api/vehicles");
    // Adjust path based on actual API response
    return Array.isArray(response.data?.vehicles)
      ? response.data.vehicles
      : [];
  }
);

// Drivers
export const fetchDrivers = createAsyncThunk(
  "weighing/fetchDrivers",
  async () => {
    const response = await axios.get("/api/drivers");
    return Array.isArray(response.data?.drivers)
      ? response.data.drivers
      : [];
  }
);

// Products
export const fetchProducts = createAsyncThunk(
  "weighing/fetchProducts",
  async () => {
    const response = await axios.get("/api/products");
    return Array.isArray(response.data?.products)
      ? response.data.products
      : [];
  }
);

// Routes
export const fetchRoutes = createAsyncThunk(
  "weighing/fetchRoutes",
  async () => {
    const response = await axios.get("/api/routes");
    return Array.isArray(response.data?.routes)
      ? response.data.routes
      : [];
  }
);

// Suppliers
export const fetchSupplierById = createAsyncThunk(
  "weighing/fetchSupplierById",
  async (id) => {
    const response = await axios.get(`/api/suppliers/${id}`);
    return Array.isArray(response.data?.suppliers)
      ? response.data.suppliers
      : [];
  }
);

// Saccos
export const fetchSaccoById = createAsyncThunk(
  "weighing/fetchSaccoById",
  async (id) => {
    const response = await axios.get(`/api/saccos/${id}`);
    return Array.isArray(response.data?.saccos)
      ? response.data.saccos
      : [];
  }
);

// Transporters
export const fetchTransporterById = createAsyncThunk(
  "weighing/fetchTransporterById",
  async (id) => {
    const response = await axios.get(`/api/transporters/${id}`);
    return Array.isArray(response.data?.transporters)
      ? response.data.transporters
      : [];
  }
);

// ----------------------
// Slice
// ----------------------
const initialState = {
  vehicles: [],
  drivers: [],
  products: [],
  routes: [],
  suppliers: [],
  saccos: [],
  transporters: [],
  transactions: [],
  loading: false,
  error: null,
};

const weighingSlice = createSlice({
  name: "weighing",
  initialState,
  reducers: {
    addTransaction: (state, action) => {
      state.transactions.push(action.payload);
    },
    completeWeighing: (state, action) => {
      const { transactionId } = action.payload;
      const transaction = state.transactions.find(t => t.id === transactionId);
      if (transaction) transaction.completed = true;
    },
    deactivateTransaction: (state, action) => {
      const { transactionId } = action.payload;
      const transaction = state.transactions.find(t => t.id === transactionId);
      if (transaction) transaction.active = false;
      // Optional: remove instead
      // state.transactions = state.transactions.filter(t => t.id !== transactionId);
    },
  },
  extraReducers: (builder) => {
    const handlePending = (state) => { state.loading = true; state.error = null; };
    const handleRejected = (state, action) => { state.loading = false; state.error = action.error.message; };

    // Vehicles
    builder
      .addCase(fetchVehicles.pending, handlePending)
      .addCase(fetchVehicles.fulfilled, (state, action) => { state.vehicles = action.payload; state.loading = false; })
      .addCase(fetchVehicles.rejected, handleRejected);

    // Drivers
    builder
      .addCase(fetchDrivers.pending, handlePending)
      .addCase(fetchDrivers.fulfilled, (state, action) => { state.drivers = action.payload; state.loading = false; })
      .addCase(fetchDrivers.rejected, handleRejected);

    // Products
    builder
      .addCase(fetchProducts.pending, handlePending)
      .addCase(fetchProducts.fulfilled, (state, action) => { state.products = action.payload; state.loading = false; })
      .addCase(fetchProducts.rejected, handleRejected);

    // Routes
    builder
      .addCase(fetchRoutes.pending, handlePending)
      .addCase(fetchRoutes.fulfilled, (state, action) => { state.routes = action.payload; state.loading = false; })
      .addCase(fetchRoutes.rejected, handleRejected);

    // Suppliers
    builder.addCase(fetchSupplierById.fulfilled, (state, action) => { state.suppliers = action.payload; });

    // Saccos
    builder.addCase(fetchSaccoById.fulfilled, (state, action) => { state.saccos = action.payload; });

    // Transporters
    builder.addCase(fetchTransporterById.fulfilled, (state, action) => { state.transporters = action.payload; });
  },
});

export const { addTransaction, completeWeighing, deactivateTransaction } = weighingSlice.actions;
export default weighingSlice.reducer;
