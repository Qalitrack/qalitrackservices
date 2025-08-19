// src/store/calibrationSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  records: [] // { id, equipment, date, status }
};

const calibrationSlice = createSlice({
  name: 'calibration',
  initialState,
  reducers: {
    addCalibration: (state, action) => {
      state.records.push(action.payload);
    },
    updateCalibration: (state, action) => {
      const idx = state.records.findIndex(r => r.id === action.payload.id);
      if (idx !== -1) {
        state.records[idx] = { ...state.records[idx], ...action.payload };
      }
    },
    deleteCalibration: (state, action) => {
      state.records = state.records.filter(r => r.id !== action.payload);
    }
  }
});

export const { addCalibration, updateCalibration, deleteCalibration } = calibrationSlice.actions;
export default calibrationSlice.reducer;
