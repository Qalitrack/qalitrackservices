// src/store/selfServiceSlice.js
import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { apiClient } from "../api/helpers/apiClients";
import { createTransaction } from "../api/Transaction/Transaction";
import { getHardwareConfig } from "../hooks/useHardwareConfig";

// ─────────────────────────────────────────────────────────────────────────────
// SELF-SERVICE KIOSK THUNKS
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Fetch vehicle by plate number (ANPR detection)
 */
export const fetchVehicleByPlate = createAsyncThunk(
    "selfService/fetchVehicleByRegNumber",
    async (regNumber, { rejectWithValue }) => {
        try {
            
            const response = await apiClient.get("/MasterData/Vehicles", {
                params: { pageNumber: 1, pageSize: 1, regNumber },
            });

            const list = response.data?.data ?? response.data;
            const vehicle = Array.isArray(list) ? list[0] : list;

            if (!vehicle) {
                throw new Error("Vehicle not found");
            }
            
            return vehicle;
        } catch (error) {
            const message = error.response?.data?.message || error.message || "Vehicle not found";
            return rejectWithValue(message);
        }
    }
);

/**
 * Authenticate driver via NFC card
 */
export const authenticateDriver = createAsyncThunk(
    "selfService/authenticateDriver",
    async (cardId, { rejectWithValue }) => {
        try {
            
            const response = await apiClient.post("/MasterData/Drivers/authenticate", {
                cardId: cardId
            });
            
            const driver = response.data?.data || response.data;
            
            if (!driver) {
                throw new Error("Driver not found");
            }
            
            return driver;
        } catch (error) {
            const message = error.response?.data?.message || error.message || "Driver not recognized";
            return rejectWithValue(message);
        }
    }
);

/**
 * Create self-service transaction (first weight)
 */
export const createSelfServiceTransaction = createAsyncThunk(
    "selfService/createTransaction",
    async (payload, { rejectWithValue }) => {
        try {
            
            const response = await createTransaction(payload);
            
            // Extract transaction from response
            let transaction = null;
            
            if (response?.data?.transaction) {
                transaction = response.data.transaction;
            } else if (response?.transaction) {
                transaction = response.transaction;
            } else if (response?.data) {
                transaction = response.data;
            } else {
                transaction = response;
            }
            
            return transaction;
        } catch (error) {
            const message = error.response?.data?.message || error.message || "Failed to create transaction";
            return rejectWithValue(message);
        }
    }
);

/**
 * Print thermal ticket
 */
export const printThermalTicket = createAsyncThunk(
    "selfService/printTicket",
    async ({ content, printerName = "ThermalPrinter01", copies = 1 }, { rejectWithValue }) => {
        try {
            
            const response = await apiClient.post("/Printer/thermal/print", {
                content,
                printerName,
                copies
            });
            
            return response.data;
        } catch (error) {
            // Don't fail the transaction if print fails
            return { success: false, error: error.message };
        }
    }
);

/**
 * Get weighbridge info
 */
export const fetchKioskWeighbridge = createAsyncThunk(
    "selfService/fetchWeighbridge",
    async (weighbridgeId = "KIOSK_WEIGHBRIDGE_01", { rejectWithValue }) => {
        try {
            const response = await apiClient.get(`/MasterData/Weighbridges/${weighbridgeId}`);
            return response.data?.data || response.data;
        } catch (error) {
            // Return default if not found
            return {
                id: "KIOSK_WEIGHBRIDGE_01",
                name: "KTDA Weighbridge",
                location: "Factory Unit A"
            };
        }
    }
);

/**
 * Listen to ANPR stream (for vehicle detection)
 */
export const connectANPRStream = createAsyncThunk(
    "selfService/connectANPR",
    async (_, { dispatch }) => {
        try {
            const eventSource = new EventSource(getHardwareConfig().anprStreamUrl);
            
            eventSource.onmessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    if (data.plateNumber) {
                        dispatch(setDetectedPlate(data.plateNumber));
                        dispatch(setANPRImage(data.imageUrl || data.snapshot));
                    }
                } catch (err) {
                }
            };
            
            eventSource.onerror = (error) => {
                eventSource.close();
            };
            
            return { connected: true };
        } catch (error) {
            return { connected: false, error: error.message };
        }
    }
);

/**
 * Listen to RFID stream (for vehicle tag detection)
 */
export const connectRFIDStream = createAsyncThunk(
    "selfService/connectRFID",
    async (_, { dispatch }) => {
        try {
            const eventSource = new EventSource(getHardwareConfig().rfidStreamUrl);
            
            eventSource.onmessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    if (data.tagId || data.rfidTag) {
                        dispatch(setRFIDTag(data.tagId || data.rfidTag));
                    }
                } catch (err) {
                }
            };
            
            eventSource.onerror = (error) => {
                eventSource.close();
            };
            
            return { connected: true };
        } catch (error) {
            return { connected: false, error: error.message };
        }
    }
);

/**
 * Listen to NFC stream (for driver authentication)
 */
export const connectNFCStream = createAsyncThunk(
    "selfService/connectNFC",
    async (_, { dispatch }) => {
        try {
            const eventSource = new EventSource(getHardwareConfig().nfcStreamUrl);
            
            eventSource.onmessage = (event) => {
                try {
                    const data = JSON.parse(event.data);
                    if (data.cardId || data.nfcId) {
                        dispatch(setNFCCard(data.cardId || data.nfcId));
                    }
                } catch (err) {
                }
            };
            
            eventSource.onerror = (error) => {
                eventSource.close();
            };
            
            return { connected: true };
        } catch (error) {
            return { connected: false, error: error.message };
        }
    }
);

// ─────────────────────────────────────────────────────────────────────────────
// SLICE
// ─────────────────────────────────────────────────────────────────────────────

const initialState = {
    // Session stages
    currentStage: 'vehicle_detection', // vehicle_detection | driver_auth | weighing | ticket_print | complete
    
    // Detection data
    detectedPlate: null,
    anprImage: null,
    rfidTag: null,
    nfcCard: null,
    
    // Session data
    vehicleData: null,
    driverData: null,
    transactionData: null,
    ticketData: null,
    weighbridgeData: null,
    
    // Stream connections
    streams: {
        anpr: false,
        rfid: false,
        nfc: false,
    },
    
    // UI state
    loading: false,
    error: null,
    printStatus: null,
    
    // Session metadata
    sessionStartTime: null,
    sessionId: null,
};

const selfServiceSlice = createSlice({
    name: "selfService",
    initialState,
    reducers: {
        // Stage management
        setStage: (state, action) => {
            state.currentStage = action.payload;
        },
        
        // Detection setters
        setDetectedPlate: (state, action) => {
            state.detectedPlate = action.payload;
        },
        
        setANPRImage: (state, action) => {
            state.anprImage = action.payload;
        },
        
        setRFIDTag: (state, action) => {
            state.rfidTag = action.payload;
        },
        
        setNFCCard: (state, action) => {
            state.nfcCard = action.payload;
        },
        
        // Session management
        startSession: (state) => {
            state.sessionId = `SESSION-${Date.now()}`;
            state.sessionStartTime = new Date().toISOString();
            state.currentStage = 'vehicle_detection';
        },
        
        endSession: (state) => {
            // Reset to initial state
            Object.assign(state, initialState);
        },
        
        // Error handling
        setError: (state, action) => {
            state.error = action.payload;
            state.loading = false;
        },
        
        clearError: (state) => {
            state.error = null;
        },
        
        // Print status
        setPrintStatus: (state, action) => {
            state.printStatus = action.payload;
        },
    },
    extraReducers: (builder) => {
        // ═══════════════════════════════════════════════════════════════════
        // ALL addCase MUST COME BEFORE addMatcher
        // ═══════════════════════════════════════════════════════════════════
        
        // Vehicle lookup
        builder
            .addCase(fetchVehicleByPlate.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(fetchVehicleByPlate.fulfilled, (state, action) => {
                state.loading = false;
                state.vehicleData = action.payload;
                state.currentStage = 'driver_auth';
            })
            .addCase(fetchVehicleByPlate.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload;
            });
        
        // Driver authentication
        builder
            .addCase(authenticateDriver.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(authenticateDriver.fulfilled, (state, action) => {
                state.loading = false;
                state.driverData = action.payload;
                state.currentStage = 'weighing';
            })
            .addCase(authenticateDriver.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload;
            });
        
        // Transaction creation
        builder
            .addCase(createSelfServiceTransaction.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(createSelfServiceTransaction.fulfilled, (state, action) => {
                state.loading = false;
                state.transactionData = action.payload;
                state.ticketData = {
                    ...action.payload,
                    arrivalTime: state.sessionStartTime,
                    weighTime: new Date().toISOString(),
                };
                state.currentStage = 'ticket_print';
            })
            .addCase(createSelfServiceTransaction.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload;
            });
        
        // Ticket printing
        builder
            .addCase(printThermalTicket.pending, (state) => {
                state.printStatus = 'printing';
            })
            .addCase(printThermalTicket.fulfilled, (state, action) => {
                state.printStatus = action.payload.success !== false ? 'success' : 'failed';
                if (state.printStatus === 'success') {
                    state.currentStage = 'complete';
                }
            })
            .addCase(printThermalTicket.rejected, (state, action) => {
                state.printStatus = 'failed';
                // Still proceed to complete even if print fails
                state.currentStage = 'complete';
            });
        
        // Weighbridge info
        builder
            .addCase(fetchKioskWeighbridge.fulfilled, (state, action) => {
                state.weighbridgeData = action.payload;
            });
        
        // Stream connections
        builder
            .addCase(connectANPRStream.fulfilled, (state, action) => {
                state.streams.anpr = action.payload.connected;
            })
            .addCase(connectRFIDStream.fulfilled, (state, action) => {
                state.streams.rfid = action.payload.connected;
            })
            .addCase(connectNFCStream.fulfilled, (state, action) => {
                state.streams.nfc = action.payload.connected;
            });
        
        // ═══════════════════════════════════════════════════════════════════
        // ALL addMatcher MUST COME AFTER ALL addCase
        // ═══════════════════════════════════════════════════════════════════
        
        // Handle manual dispatch of vehicle fulfilled action (for simulation mode)
        builder.addMatcher(
            (action) => action.type === 'selfService/fetchVehicleByRegNumber/fulfilled',
            (state, action) => {
                state.loading = false;
                state.vehicleData = action.payload;
                state.currentStage = 'driver_auth';
            }
        );
        
        // Handle manual dispatch of driver fulfilled action (for simulation mode)
        builder.addMatcher(
            (action) => action.type === 'selfService/authenticateDriver/fulfilled',
            (state, action) => {
                state.loading = false;
                state.driverData = action.payload;
                state.currentStage = 'weighing';
            }
        );
    },
});

// Export actions
export const {
    setStage,
    setDetectedPlate,
    setANPRImage,
    setRFIDTag,
    setNFCCard,
    startSession,
    endSession,
    setError,
    clearError,
    setPrintStatus,
} = selfServiceSlice.actions;

// Export selectors
export const selectCurrentStage = (state) => state.selfService.currentStage;
export const selectVehicleData = (state) => state.selfService.vehicleData;
export const selectDriverData = (state) => state.selfService.driverData;
export const selectTransactionData = (state) => state.selfService.transactionData;
export const selectTicketData = (state) => state.selfService.ticketData;
export const selectDetectedPlate = (state) => state.selfService.detectedPlate;
export const selectRFIDTag = (state) => state.selfService.rfidTag;
export const selectNFCCard = (state) => state.selfService.nfcCard;
export const selectStreams = (state) => state.selfService.streams;
export const selectError = (state) => state.selfService.error;
export const selectLoading = (state) => state.selfService.loading;
export const selectPrintStatus = (state) => state.selfService.printStatus;

export default selfServiceSlice.reducer;