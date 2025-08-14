// src/store/weighingSlice.js
import { createSlice, nanoid } from '@reduxjs/toolkit';

const weighingSlice = createSlice({
  name: 'weighing',
  initialState: {
    transactions: [],
  },
  reducers: {
    startWeighing: {
      reducer(state, action) {
        state.transactions.push(action.payload);
      },
      prepare({ type, driver, plate, orderId, batch, w1 }) {
        return {
          payload: {
            id: nanoid(),
            type,
            driver,
            plate,
            orderId,
            batch,
            w1,
            w2: null,
            ttat: null,
            status: 'active', // active | deactivated
            date: new Date().toISOString(),
          },
        };
      },
    },
    completeWeighing(state, action) {
      const { id, w2 } = action.payload;
      const tx = state.transactions.find(t => t.id === id);
      if (tx) {
        tx.w2 = w2;
        // Example TTAT: seconds between now & creation
        const start = new Date(tx.date).getTime();
        const end = Date.now();
        tx.ttat = Math.round((end - start) / 1000);
      }
    },
    deactivateTransaction(state, action) {
      const id = action.payload;
      const tx = state.transactions.find(t => t.id === id);
      if (tx && !tx.w2) {
        tx.status = 'deactivated';
      }
    },
  },
});

export const { startWeighing, completeWeighing, deactivateTransaction } = weighingSlice.actions;
export default weighingSlice.reducer;
