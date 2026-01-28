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
        customerName: "KTDA Tea Processing",
        deliveryNumber: "SAP-DLV-" + Date.now(),
        referenceNumber: "SAP-REF-" + Date.now(),
        batchNumber: "SAP-BCH-" + Date.now(),
        materialClass: "RAW-TEA-001",
        transportCode: "TCB101/2"
      };

      console.log("✅ Mock vehicle data created:", mockVehicleData);
      
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
  const handleDriverAuthenticated = async (nfcData) => {
    try {
      console.log("👤 Driver NFC scanned:", nfcData);
      dispatch(clearError());
      
      const mockDriverData = {
        id: "DRIVER_" + Date.now(),
        driverId: "DRIVER_" + Date.now(),
        fullName: "John Kamau Mwangi",
        name: "John Kamau",
        license: "DL-" + Math.floor(Math.random() * 1000000),
        idNo: "28000000",
        nfcCardId: nfcData.cardId,
        company: "Fresh Leaf Carriers Ltd",
        phone: "+254 712 345 678",
        email: "john.kamau@flc.co.ke",
        address: "PO BOX 123, Nairobi"
      };

      console.log("✅ Mock driver data created:", mockDriverData);
      
      dispatch({
        type: 'selfService/authenticateDriver/fulfilled',
        payload: mockDriverData
      });
      
      message.success(`Welcome, ${mockDriverData.fullName}!`);
      
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
        noPlate: vehicleData.noPlate,
        driverName: driverData.fullName,
        vehicleId: vehicleData.id,
        driverId: driverData.driverId,
        commodityId: vehicleData.commodityId,
        commodityName: vehicleData.commodityName,
        transporterId: vehicleData.transporterId,
        transporterName: vehicleData.transporterName,
        supplierId: vehicleData.supplierId,
        supplierName: vehicleData.supplierName,
        customerId: vehicleData.customerId,
        customerName: vehicleData.customerName,
        operation: "Inbound Product Receipt",
        weighMode: "Gross/Tare",
        firstWeight: weighData.weight,
        secondWeight: null,
        scaleName: weighData.scaleName,
        weighBridgeId: weighData.weighBridgeId,
        weighBridgeName: weighData.weighBridgeName,
        operatorId: "KIOSK_AUTO",
        operatorName: "Self-Service Kiosk",
        isCompleted: false,
        deliveryNumber: vehicleData.deliveryNumber,
        referenceNumber: vehicleData.referenceNumber,
        batchNumber: vehicleData.batchNumber,
        materialClass: vehicleData.materialClass,
        transportCode: vehicleData.transportCode
      };

      console.log("📤 Creating transaction:", transactionPayload);

      const mockTransaction = {
        ...transactionPayload,
        id: "TXN_" + Date.now(),
        transactionId: "TXN_" + Date.now(),
        createdAt: new Date().toISOString(),
        status: "Incomplete"
      };

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
            onPrintComplete={handlePrintComplete}
            onBack={() => dispatch(setStage(STAGES.WEIGHING))}
          />
        );

      case STAGES.COMPLETE:
        return (
          <div className={`h-screen bg-gradient-to-br ${theme.gradientBg} flex items-center justify-center`}>
            <div className="text-center">
              <div className="w-32 h-32 mx-auto mb-8 rounded-full bg-green-900 flex items-center justify-center">
                <span className="text-8xl">✓</span>
              </div>
              <h1 className={`text-5xl font-bold ${theme.textPrimary} mb-4`}>
                Thank You!
              </h1>
              <p className={`text-2xl ${theme.textSecondary} mb-8`}>
                Transaction completed successfully
              </p>
              <p className={`text-lg ${theme.textMuted}`}>
                Resetting in {10} seconds...
              </p>
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
      <div className="fixed top-4 right-4 z-[10000]">
        <ThemeToggle />
      </div>

      {/* Stage Content */}
      {renderStage()}

      {/* Loading Overlay */}
      {loading && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[9999]">
          <div className={`${theme.bgCard} rounded-2xl p-8 text-center`}>
            <div className="w-16 h-16 border-4 border-amber-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
            <p className={`text-xl ${theme.textPrimary}`}>Processing...</p>
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