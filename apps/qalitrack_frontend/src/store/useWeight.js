// useWeightStore.js
import { create } from "zustand";

export const useWeightStore = create((set) => ({
  liveWeight: 0,         // latest real-time stream weight
  setLiveWeight: (w) => set({ liveWeight: w }),
}));
