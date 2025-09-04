import { createSlice } from "@reduxjs/toolkit";

const weighingSlice = createSlice({
  name: "weighing",
  initialState: {
    transactions: [],
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
        if (tx.operation === "Inbound Material Receipt" && w2 >= tx.w1) {
          throw new Error("Inbound transaction invalid: W2 must be less than W1.");
        }
        if (tx.operation === "Outbound Product Dispatch" && w2 <= tx.w1) {
          throw new Error("Outbound transaction invalid: W2 must be greater than W1.");
        }

        // Valid transaction → update
        tx.w2 = w2;
        tx.ttat = Math.floor((Date.now() - new Date(tx.date)) / 1000);

        // Auto calculate net
        if (tx.operation === "Inbound Material Receipt") {
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
});

export const { addTransaction, completeWeighing, deactivateTransaction } =
  weighingSlice.actions;

export default weighingSlice.reducer;
