import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  alerts: [],
  devices: [
    { deviceId: 'gate1', name: 'Main Entry Gate', status: 'offline' },
    { deviceId: 'weigh1', name: 'Weighbridge #1', status: 'offline' },
    { deviceId: 'signal1', name: 'Traffic Signal', status: 'offline' }
  ]
};

const automationSlice = createSlice({
  name: 'automation',
  initialState,
  reducers: {
    addAlert: (state, action) => {
      state.alerts.push({
        id: Date.now(),
        message: action.payload.message,
        type: action.payload.type || 'info',
        timestamp: action.payload.timestamp || new Date().toISOString()
      });
    },
    clearAlerts: (state) => {
      state.alerts = [];
    },
    removeExpiredAlerts: (state) => {
      const now = Date.now();
      state.alerts = state.alerts.filter(
        alert => now - new Date(alert.timestamp).getTime() < 60 * 1000
      );
    },
    setDeviceState: (state, action) => {
      const { deviceId, status, name } = action.payload;
      if (!Array.isArray(state.devices)) {
        state.devices = [];
      }
      const existing = state.devices.find(d => d.deviceId === deviceId);
      if (existing) {
        existing.status = status;
      } else {
        state.devices.push({ deviceId, status, name });
      }
    }
  }
});

export const { addAlert, clearAlerts, removeExpiredAlerts, setDeviceState } = automationSlice.actions;
export default automationSlice.reducer;
