import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { getVehicles } from "../api/MasterData/Vehicles";
import { getDrivers } from "../api/MasterData/Drivers";
import { getProducts } from "../api/MasterData/Products";
import { getSupplierById } from "../api/MasterData/Suppliers";
import { getSaccoById } from "../api/MasterData/Saccos";
import { getProductById } from "../api/MasterData/Products";
import { getRoutes } from "../api/MasterData/Routes";
import { getTransporterById} from "../api/MasterData/Transporters"; 
// ✅ Async Thunks (fetch from backend)
export const fetchVehicles = createAsyncThunk("weighing/fetchVehicles", async () => {
  const res = await getVehicles();
  return res;
});

export const fetchDrivers = createAsyncThunk("weighing/fetchDrivers", async () => {
  const res = await getDrivers();
  return res;
});

export const fetchProducts = createAsyncThunk("weighing/fetchProducts", async () => {
  const res = await getProducts(); // <-- updated
  return res;
});
export const fetchSupplierById = createAsyncThunk("weighing/fetchSupplierById", async (id) => {
  const res = await getSupplierById(id);
  return res;
});
export const fetchSaccoById = createAsyncThunk("weighing/fetchSaccoById", async (id) => {
  const res = await getSaccoById(id);
  return res;
});
export const fetchProductById = createAsyncThunk("weighing/fetchProductById", async (id) => {
  const res = await getProductById(id);
  return res;
});
export const fetchRoutes = createAsyncThunk("weighing/fetchRoutes", async () => {
  const res = await getRoutes();
  return res;
});
export const fetchTransporterById = createAsyncThunk("weighing/fetchTransporterById", async (id) => {
  const res = await getTransporterById(id);
  return res;
});

const weighingSlice = createSlice({
  name: "weighing",
  initialState: {
    transactions: [],
    vehicles: [],
    drivers: [],
    products: [], // <-- changed from materials
    suppliers: null,
    saccos: null,
    routes: [],
    transporters: null,
    loading: false,
    error: null,
  },
  reducers: {
    addTransaction: (state, action) => {
      state.transactions.push({
        ...action.payload,
        id: Date.now().toString(),
        date: new Date().toISOString(),
        w2: null,
        netWeight: null,
        ttat: null,
        deactivated: false,
      });
    },
    completeWeighing: (state, action) => {
      const { id, w2 } = action.payload;
      const tx = state.transactions.find((t) => t.id === id);

      if (tx) {
        // Business rules
        if (tx.operation === "Inbound Product Receipt" && w2 >= tx.w1) {
          throw new Error("Inbound transaction invalid: W2 must be less than W1.");
        }
        if (tx.operation === "Outbound Product Dispatch" && w2 <= tx.w1) {
          throw new Error("Outbound transaction invalid: W2 must be greater than W1.");
        }

        // Valid transaction → update
        tx.w2 = w2;
        tx.ttat = Math.floor((Date.now() - new Date(tx.date)) / 1000);

        // Auto calculate net
        if (tx.operation === "Inbound Product Receipt") {
          tx.netWeight = tx.w1 - w2;
        } else if (tx.operation === "Outbound Product Dispatch") {
          tx.netWeight = w2 - tx.w1;
        }
      }
    },
    deactivateTransaction: (state, action) => {
      const tx = state.transactions.find((t) => t.id === action.payload);
      if (tx) {
        tx.deactivated = true;
      }
    },
  },
  extraReducers: (builder) => {
    builder
      // ✅ Fetch Vehicles
      .addCase(fetchVehicles.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchVehicles.fulfilled, (state, action) => {
        state.loading = false;
        state.vehicles = action.payload;
      })
      .addCase(fetchVehicles.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      // ✅ Fetch Drivers
      .addCase(fetchDrivers.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchDrivers.fulfilled, (state, action) => {
        state.loading = false;
        state.drivers = action.payload;
      })
      .addCase(fetchDrivers.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      // ✅ Fetch Products
      .addCase(fetchProducts.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchProducts.fulfilled, (state, action) => {
        state.loading = false;
        state.products = action.payload; // <-- changed from materials
      })
      .addCase(fetchProducts.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      .addCase(fetchSupplierById.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchSupplierById.fulfilled, (state, action) => {
        state.loading = false;
        state.suppliers = action.payload;
      })
      .addCase(fetchSupplierById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      .addCase(fetchSaccoById.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchSaccoById.fulfilled, (state, action) => {
        state.loading = false;
        state.saccos = action.payload;
      })
      .addCase(fetchSaccoById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      .addCase(fetchProductById.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchProductById.fulfilled, (state, action) => {
        state.loading = false;
        state.products = action.payload;
      })
      .addCase(fetchProductById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      .addCase(fetchRoutes.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchRoutes.fulfilled, (state, action) => {
        state.loading = false;
        state.routes = action.payload;
      })
      .addCase(fetchRoutes.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      })

      .addCase(fetchTransporterById.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchTransporterById.fulfilled, (state, action) => {
        state.loading = false;
        state.transporters = action.payload;
      })
      .addCase(fetchTransporterById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message;
      });
  },
});

export const { addTransaction, completeWeighing, deactivateTransaction } =
  weighingSlice.actions;

export default weighingSlice.reducer;
