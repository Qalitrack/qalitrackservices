import { configureStore, combineReducers } from '@reduxjs/toolkit';
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';
import weighingReducer from './weighingSlice';
import automationReducer from './automationSlice';
import calibrationReducer from './calibrationSlice';
import vehicleReducer from './Vehicleslice';
// ✅ NEW: Self-Service Kiosk reducer
import selfServiceReducer from './selfServiceSlice';

const persistConfig = {
  key: 'root',
  storage,
  // ✅ UPDATED: Don't persist selfService (kiosk sessions should be ephemeral)
  whitelist: ['weighing', 'automation', 'calibration', 'vehicles'],
  // Blacklist selfService to ensure fresh state on reload
  blacklist: ['selfService']
};

const rootReducer = combineReducers({
  weighing: weighingReducer,
  automation: automationReducer,
  calibration: calibrationReducer,
  vehicles: vehicleReducer,
  // ✅ NEW: Self-service kiosk state (not persisted)
  selfService: selfServiceReducer,
});

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({ 
      serializableCheck: {
        // ✅ Ignore EventSource and non-serializable values in selfService
        ignoredActions: [
          'persist/PERSIST',
          'persist/REHYDRATE',
          'selfService/connectANPRStream/fulfilled',
          'selfService/connectRFIDStream/fulfilled',
          'selfService/connectNFCStream/fulfilled',
        ],
        ignoredPaths: [
          'selfService.streams',
          'selfService.sessionStartTime',
        ],
      }
    })
});

export const persistor = persistStore(store);