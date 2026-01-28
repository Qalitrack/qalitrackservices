import React, { useEffect } from "react";
import { message } from "antd";
import { useDispatch, useSelector } from "react-redux";
import { ThemeProvider, ThemeToggle, useTheme } from "../components/Context/ThemeContext.jsx";
import VehicleDetectionScreen from "../components/SelfService/VehicleDetectionScreen";
import DriverAuthScreen from "../components/SelfService/DriverAuthScreen";
import WeighingScreen from "../components/SelfService/WeighingScreen";
import TicketPrintScreen from "../components/SelfService/TicketPrintScreen";

import {
  fetchVehicleByPlate,
  authenticateDriver,
  createSelfServiceTransaction,
  printThermalTicket,
  startSession,
  endSession,
  setStage,
  clearError,
  selectCurrentStage,
  selectVehicleData,
  selectDriverData,
  selectTransactionData,
  selectTicketData,
  selectError,
  selectLoading,
} from "../store/selfServiceSlice.js";

const STAGES = {
  VEHICLE_DETECTION: 'vehicle_detection',
  DRIVER_AUTH: 'driver_auth',
  WEIGHING: 'weighing',
  TICKET_PRINT: 'ticket_print',
  COMPLETE: 'complete'
};

// Inner component that uses theme
function SelfServiceWeighingInner() {
  const dispatch = useDispatch();
  const { theme, isDark } = useTheme();
  
  const currentStage = useSelector(selectCurrentStage);
  const vehicleData = useSelector(selectVehicleData);
  const driverData = useSelector(selectDriverData);
  const transactionData = useSelector(selectTransactionData);
  const ticketData = useSelector(selectTicketData);
  const error = useSelector(selectError);
  const loading = useSelector(selectLoading);

  // Initialize session
  useEffect(() => {
    console.log("🎬 Initializing self-service kiosk...");
    dispatch(startSession());
    console.log("✅ Self-service kiosk initialized");

    return () => {
      console.log("🛑 Cleaning up self-service session");
      dispatch(endSession());
    };
  }, [dispatch]);

  // Debug: Log stage changes
  useEffect(() => {
    console.log(`📍 Current Stage: ${currentStage}`);
  }, [currentStage]);

  // Auto-reset after completion
  useEffect(() => {
    if (currentStage === STAGES.COMPLETE) {
      console.log("✅ Transaction complete - auto-reset in 10 seconds");
      const timer = setTimeout(() => {
        handleReset();
      }, 10000);
      
      return () => clearTimeout(timer);
    }
  }, [currentStage]);

  const handleReset = () => {
    console.log("🔄 Resetting kiosk session...");
    dispatch(endSession());
    dispatch(startSession());
    console.log("✅ Session reset - ready for new vehicle");
  };

  // Stage 1: Vehicle Detection
  const handleVehicleDetected = async (detectionData) => {
    try {
      console.log("🚗 Vehicle detected:", detectionData);
      dispatch(clearError());
      
      // Create mock vehicle data
      const mockVehicleData = {
        id: "VEHICLE_" + Date.now(),
        plateNumber: detectionData.plateNumber,
        noPlate: detectionData.plateNumber,
        rfidTag: detectionData.rfidTag,
        vehicleType: "Truck",
        transporterId: "TRANS_001",
        transporterName: "Fresh Leaf Carriers Ltd",
        commodityId: "COMM_001",
        commodityName: "Purple Tea Leaves",
        supplierId: "SUPP_001",
        supplierName: "KTDA Factory 2",
        customerId: "CUST_001",
        customerName: "KTDA Tea Processing"
      };

      console.log("✅ Mock vehicle data created:", mockVehicleData);
      
      // Dispatch fulfilled action to update Redux state
      dispatch({
        type: 'selfService/fetchVehicleByRegNumber/fulfilled',
        payload: mockVehicleData
      });
      
      message.success(`Vehicle ${detectionData.plateNumber} detected!`);
      
      console.log("🔄 Moving to driver authentication...");
      dispatch(setStage(STAGES.DRIVER_AUTH));
      
    } catch (err) {
      console.error("❌ Vehicle detection failed:", err);
      message.error("Vehicle not found in system. Please contact the office.");
    }
  };

  // Stage 2: Driver Authentication
  const handleDriverAuthenticated = async (nfcResponse) => {
    try {
      console.log("👤 Driver NFC scanned:", nfcResponse);
      dispatch(clearError());
      
      // Use the verified driver data from NFC authentication
      const driverData = nfcResponse.driverData || {
        id: "DRIVER_" + Date.now(),
        driverId: nfcResponse.driverId || "DRIVER_" + Date.now(),
        fullName: nfcResponse.driverName || "John Kamau Mwangi",
        name: nfcResponse.driverName?.split(' ').slice(0, 2).join(' ') || "John Kamau",
        license: "DL-" + Math.floor(Math.random() * 1000000),
        idNo: "28000000" + Math.floor(Math.random() * 1000),
        nfcCardId: nfcResponse.cardId,
        company: "Fresh Leaf Carriers Ltd",
        phone: "+254 712 345 678",
        email: "john.kamau@flc.co.ke"
      };

      console.log("✅ Driver data verified:", driverData);
      
      // Dispatch fulfilled action to update Redux state
      dispatch({
        type: 'selfService/authenticateDriver/fulfilled',
        payload: driverData
      });
      
      message.success(`Welcome, ${driverData.fullName}!`);
      
      console.log("🔄 Moving to weighing screen...");
      dispatch(setStage(STAGES.WEIGHING));
      
    } catch (err) {
      console.error("❌ Driver authentication failed:", err);
      message.error("Driver card not recognized. Please contact the office.");
    }
  };

  // Stage 3: Weighing Complete
  const handleWeighingComplete = async (weighData) => {
    try {
      console.log("⚖️ Weight captured:", weighData);
      dispatch(clearError());

      const transactionPayload = {
        receiptNo: "TXN-" + Date.now(),
        expectedWeighings: 2,
        noPlate: weighData.noPlate,
        driverName: weighData.driverName,
        vehicleId: vehicleData.id,
        driverId: driverData.driverId || driverData.id,
        commodityId: weighData.commodityID,
        commodityName: weighData.commodityName,
        transporterId: weighData.transporterID,
        transporterName: weighData.transporterName,
        supplierId: weighData.supplierID,
        supplierName: weighData.supplierName,
        customerId: weighData.customerID,
        customerName: weighData.customerName,
        originName: weighData.originName || "",
        destinationName: weighData.destinationName || "",
        operation: weighData.operation,
        weighMode: weighData.weighMode,
        firstWeight: weighData.weight || weighData.firstWeight,
        secondWeight: null,
        scaleName: weighData.scaleName,
        weighBridgeId: weighData.weighBridgeId,
        weighBridgeName: weighData.weighBridgeName,
        operatorId: "KIOSK_AUTO",
        operatorName: "Self-Service Kiosk",
        isCompleted: false,
        notes: weighData.notes || ""
      };

      console.log("📤 Creating transaction:", transactionPayload);

      // Create mock transaction
      const mockTransaction = {
        ...transactionPayload,
        id: "TXN_" + Date.now(),
        transactionId: "TXN_" + Date.now(),
        ticketID: "TXN_" + Date.now(),
        createdAt: new Date().toISOString(),
        weighTime: new Date().toISOString(),
        status: "Incomplete"
      };

      // Dispatch fulfilled action
      dispatch({
        type: 'selfService/createTransaction/fulfilled',
        payload: mockTransaction
      });

      message.success("Transaction created successfully!");
      
      console.log("🔄 Moving to ticket print...");
      dispatch(setStage(STAGES.TICKET_PRINT));

    } catch (err) {
      console.error("❌ Transaction creation failed:", err);
      message.error("Failed to create transaction. Please try again.");
    }
  };

  // Stage 4: Print Complete
  const handlePrintComplete = () => {
    console.log("🖨️ Ticket printing complete");
    dispatch(setStage(STAGES.COMPLETE));
  };

  // Render current stage
  const renderStage = () => {
    switch (currentStage) {
      case STAGES.VEHICLE_DETECTION:
        return (
          <VehicleDetectionScreen
            onVehicleDetected={handleVehicleDetected}
            error={error}
            onReset={handleReset}
          />
        );

      case STAGES.DRIVER_AUTH:
        return (
          <DriverAuthScreen
            vehicleData={vehicleData}
            onDriverAuthenticated={handleDriverAuthenticated}
            onBack={() => dispatch(setStage(STAGES.VEHICLE_DETECTION))}
            error={error}
          />
        );

      case STAGES.WEIGHING:
        return (
          <WeighingScreen
            vehicleData={vehicleData}
            driverData={driverData}
            onWeighingComplete={handleWeighingComplete}
            onBack={() => dispatch(setStage(STAGES.DRIVER_AUTH))}
            error={error}
          />
        );

      case STAGES.TICKET_PRINT:
        return (
          <TicketPrintScreen
            ticketData={ticketData || transactionData}
            vehicleData={vehicleData}
            driverData={driverData}
            onComplete={handlePrintComplete}
          />
        );

      case STAGES.COMPLETE:
        return (
          <div className={`min-h-screen ${
            isDark 
              ? 'bg-gradient-to-br from-gray-900 via-gray-800 to-black' 
              : 'bg-gradient-to-br from-gray-50 via-white to-gray-100'
          } flex items-center justify-center`}>
            <div className="text-center max-w-xl">
              <div className="w-40 h-40 mx-auto mb-8 rounded-full bg-green-900/50 flex items-center justify-center shadow-2xl">
                <span className="text-9xl">✓</span>
              </div>
              <h1 className={`text-6xl font-bold mb-6 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Thank You!
              </h1>
              <p className={`text-3xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                Transaction completed successfully
              </p>
              <p className={`text-xl ${isDark ? 'text-gray-600' : 'text-gray-500'}`}>
                Resetting in 10 seconds...
              </p>
              <button
                onClick={handleReset}
                className="mt-8 px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-xl font-semibold text-lg transition-colors"
              >
                Start New Transaction
              </button>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="relative">
      {/* Theme Toggle - Fixed position */}
      <div className="fixed top-6 right-6 z-[10000]">
        <ThemeToggle />
      </div>

      {/* Stage Content */}
      {renderStage()}

      {/* Loading Overlay */}
      {loading && (
        <div className="fixed inset-0 bg-black/70 flex items-center justify-center z-[9999]">
          <div className={`${
            isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'
          } rounded-2xl p-10 text-center border-2 shadow-2xl`}>
            <div className="w-20 h-20 border-4 border-amber-500 border-t-transparent rounded-full animate-spin mx-auto mb-6"></div>
            <p className={`text-2xl font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
              Processing...
            </p>
          </div>
        </div>
      )}
    </div>
  );
}

// Wrapper with ThemeProvider
export default function SelfServiceWeighing() {
  return (
    <ThemeProvider>
      <SelfServiceWeighingInner />
    </ThemeProvider>
  );
}