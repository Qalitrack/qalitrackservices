// src/store/offlineQueueSlice.js
import { createSlice } from '@reduxjs/toolkit';

const offlineQueueSlice = createSlice({
  name: 'offlineQueue',
  initialState: {
    items: [] // { id, type: 'CREATE'|'COMPLETE'|'DELETE', payload }
  },
  reducers: {
    enqueue: (state, action) => {
      state.items.push({ ...action.payload, id: crypto.randomUUID() });
    },
    dequeueById: (state, action) => {
      state.items = state.items.filter(i => i.id !== action.payload);
    },
    clearQueue: (state) => {
      state.items = [];
    }
  }
});

export const { enqueue, dequeueById, clearQueue } = offlineQueueSlice.actions;
export default offlineQueueSlice.reducer;
