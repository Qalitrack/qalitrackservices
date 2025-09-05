import { configureStore, combineReducers } from '@reduxjs/toolkit';
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';
import weighingReducer from './weighingSlice';
import automationReducer from './automationSlice';
import calibrationReducer from './calibrationSlice';
import vehicleReducer from './Vehicleslice';
// ⬅ added

const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['weighing', 'automation', 'calibration'] 
};

const rootReducer = combineReducers({
  weighing: weighingReducer,
  automation: automationReducer,
  calibration: calibrationReducer,
  vehicles: vehicleReducer,
  // ⬅ added
});

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({ serializableCheck: false })
});

export const persistor = persistStore(store);
