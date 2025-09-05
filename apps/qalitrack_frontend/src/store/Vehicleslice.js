// src/store/vehicleSlice.js
import { createSlice } from "@reduxjs/toolkit";

const vehicleSlice = createSlice({
  name: "vehicles",
  initialState: [],
  reducers: {
    addVehicle: (state, action) => {
      state.push(action.payload);
    },
    updateVehicle: (state, action) => {
      const { index, vehicle } = action.payload;
      state[index] = vehicle;
    },
    deleteVehicle: (state, action) => {
      return state.filter((_, i) => i !== action.payload);
    },
  },
});

export const { addVehicle, updateVehicle, deleteVehicle } = vehicleSlice.actions;
export default vehicleSlice.reducer;
