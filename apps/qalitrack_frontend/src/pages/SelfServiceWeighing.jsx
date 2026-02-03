import React, { useState, useEffect, useCallback } from "react";
import { message } from "antd";
// Re-wrap with ThemeProvider — the child screens (VehicleDetectionScreen, etc.)
// call useTheme() internally so this context must exist.  ThemeProvider itself
// has zero auth dependency; it only manages dark/light state.
import { ThemeProvider, useTheme } from "../components/Context/ThemeContext.jsx";
import VehicleDetectionScreen from "../components/SelfService/VehicleDetectionScreen";
import DriverAuthScreen from "../components/SelfService/DriverAuthScreen";
import WeighingScreen from "../components/SelfService/WeighingScreen";
import TicketPrintScreen from "../components/SelfService/TicketPrintScreen";

/* ─────────────────────────────────────────────────────────────────────────
   WHY no Redux / useSelector / useDispatch?
   The Redux store Provider in this app sits inside (or alongside) the
   authenticated app shell.  Any slice action can trigger a token-refresh
   that redirects unauthenticated hits to /login.
   All kiosk state lives in plain React useState — fully isolated.
   ───────────────────────────────────────────────────────────────────── */

const STAGES = {
  VEHICLE_DETECTION: "vehicle_detection",
  DRIVER_AUTH:       "driver_auth",
  WEIGHING:          "weighing",
  TICKET_PRINT:      "ticket_print",
  COMPLETE:          "complete",
};

/* ── Error boundary — catches crashes in child screens so you get a
      visible fallback instead of a blank white page ── */
class KioskErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }
  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }
  componentDidCatch(error, info) {
    console.error("🛑 Kiosk error boundary caught:", error, info);
  }
  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center" style={{ background: "#fafafa" }}>
          <div className="text-center max-w-md px-6 py-12 rounded-2xl shadow-lg" style={{ background: "#fff", border: "1px solid #e5e7eb" }}>
            <div className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4" style={{ background: "#fef2f2" }}>
              <span className="text-3xl">⚠️</span>
            </div>
            <h2 className="text-xl font-bold mb-2" style={{ color: "#111827" }}>Kiosk Error</h2>
            <p className="text-sm mb-1" style={{ color: "#6b7280" }}>Something went wrong. Check the browser console for details.</p>
            <p className="text-xs mb-4 font-mono break-all" style={{ color: "#ef4444" }}>{this.state.error?.message}</p>
            <button
              onClick={() => window.location.reload()}
              className="px-5 py-2 rounded-lg text-sm font-semibold text-white"
              style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)" }}
            >
              Reload Kiosk
            </button>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}

/* ── Inner component (needs theme context) ── */
function KioskInner() {
  const { isDark } = useTheme();

  // fires once on mount — check browser console to confirm kiosk is rendering
  useEffect(() => { console.log("🚀 KioskInner mounted successfully"); }, []);

  const [currentStage, setCurrentStage]       = useState(STAGES.VEHICLE_DETECTION);
  const [vehicleData, setVehicleData]         = useState(null);
  const [driverData,  setDriverData]          = useState(null);
  const [transactionData, setTransactionData] = useState(null);
  const [loading, setLoading]                 = useState(false);
  const [error, setError]                     = useState(null);

  /* stage log */
  useEffect(() => { console.log(`📍 Kiosk stage: ${currentStage}`); }, [currentStage]);

  /* ── reset helper ── */
  const handleReset = useCallback(() => {
    console.log("🔄 Resetting kiosk…");
    setCurrentStage(STAGES.VEHICLE_DETECTION);
    setVehicleData(null);
    setDriverData(null);
    setTransactionData(null);
    setError(null);
  }, []);

  /* auto-reset 10 s after COMPLETE */
  useEffect(() => {
    if (currentStage !== STAGES.COMPLETE) return;
    console.log("✅ Auto-reset in 10 s…");
    const t = setTimeout(handleReset, 10000);
    return () => clearTimeout(t);
  }, [currentStage, handleReset]);

  /* ── Stage 1 ── */
  const handleVehicleDetected = async (detectionData) => {
    try {
      setError(null);
      setLoading(true);

      const mock = {
        id:               "VEHICLE_" + Date.now(),
        plateNumber:      detectionData.plateNumber,
        noPlate:          detectionData.plateNumber,
        rfidTag:          detectionData.rfidTag,
        vehicleType:      "Truck",
        transporterId:    "TRANS_001",
        transporterName:  "Fresh Leaf Carriers Ltd",
        commodityId:      "COMM_001",
        commodityName:    "Purple Tea Leaves",
        supplierId:       "SUPP_001",
        supplierName:     "KTDA Factory 2",
        customerId:       "CUST_001",
        customerName:     "KTDA Tea Processing",
      };

      setVehicleData(mock);
      setLoading(false);
      message.success(`Vehicle ${detectionData.plateNumber} detected!`);
      setCurrentStage(STAGES.DRIVER_AUTH);
    } catch (err) {
      console.error("❌ Vehicle detection failed:", err);
      setLoading(false);
      setError("Vehicle not found in system. Please contact the office.");
      message.error("Vehicle not found in system.");
    }
  };

  /* ── Stage 2 ── */
  const handleDriverAuthenticated = async (nfcResponse) => {
    try {
      setError(null);
      setLoading(true);

      const driver = nfcResponse.driverData || {
        id:          "DRIVER_" + Date.now(),
        driverId:    nfcResponse.driverId || "DRIVER_" + Date.now(),
        fullName:    nfcResponse.driverName || "John Kamau Mwangi",
        name:        nfcResponse.driverName?.split(" ").slice(0, 2).join(" ") || "John Kamau",
        license:     "DL-" + Math.floor(Math.random() * 1000000),
        idNo:        "28000000" + Math.floor(Math.random() * 1000),
        nfcCardId:   nfcResponse.cardId,
        company:     "Fresh Leaf Carriers Ltd",
        phone:       "+254 712 345 678",
        email:       "john.kamau@flc.co.ke",
      };

      setDriverData(driver);
      setLoading(false);
      message.success(`Welcome, ${driver.fullName}!`);
      setCurrentStage(STAGES.WEIGHING);
    } catch (err) {
      console.error("❌ Driver auth failed:", err);
      setLoading(false);
      setError("Driver card not recognized. Please contact the office.");
      message.error("Driver card not recognized.");
    }
  };

  /* ── Stage 3 ── */
  const handleWeighingComplete = async (weighData) => {
    try {
      setError(null);
      setLoading(true);

      const txn = {
        id:                "TXN_" + Date.now(),
        transactionId:     "TXN_" + Date.now(),
        ticketID:          "TXN_" + Date.now(),
        receiptNo:         "TXN-" + Date.now(),
        expectedWeighings: 2,
        noPlate:           weighData.noPlate,
        driverName:        weighData.driverName,
        vehicleId:         vehicleData?.id,
        driverId:          driverData?.driverId || driverData?.id,
        commodityId:       weighData.commodityID,
        commodityName:     weighData.commodityName,
        transporterId:     weighData.transporterID,
        transporterName:   weighData.transporterName,
        supplierId:        weighData.supplierID,
        supplierName:      weighData.supplierName,
        customerId:        weighData.customerID,
        customerName:      weighData.customerName,
        originName:        weighData.originName || "",
        destinationName:   weighData.destinationName || "",
        operation:         weighData.operation,
        weighMode:         weighData.weighMode,
        firstWeight:       weighData.weight || weighData.firstWeight,
        secondWeight:      null,
        scaleName:         weighData.scaleName,
        weighBridgeId:     weighData.weighBridgeId,
        weighBridgeName:   weighData.weighBridgeName,
        operatorId:        "KIOSK_AUTO",
        operatorName:      "Self-Service Kiosk",
        isCompleted:       false,
        notes:             weighData.notes || "",
        createdAt:         new Date().toISOString(),
        weighTime:         new Date().toISOString(),
        status:            "Incomplete",
      };

      setTransactionData(txn);
      setLoading(false);
      message.success("Transaction created successfully!");
      setCurrentStage(STAGES.TICKET_PRINT);
    } catch (err) {
      console.error("❌ Transaction creation failed:", err);
      setLoading(false);
      setError("Failed to create transaction. Please try again.");
      message.error("Failed to create transaction.");
    }
  };

  /* ── Stage 4 ── */
  const handlePrintComplete = () => {
    console.log("🖨️  Ticket printing complete");
    setCurrentStage(STAGES.COMPLETE);
  };

  /* ── Render ── */
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
            onBack={() => setCurrentStage(STAGES.VEHICLE_DETECTION)}
            error={error}
          />
        );
      case STAGES.WEIGHING:
        return (
          <WeighingScreen
            vehicleData={vehicleData}
            driverData={driverData}
            onWeighingComplete={handleWeighingComplete}
            onBack={() => setCurrentStage(STAGES.DRIVER_AUTH)}
            error={error}
          />
        );
      case STAGES.TICKET_PRINT:
        return (
          <TicketPrintScreen
            ticketData={transactionData}
            vehicleData={vehicleData}
            driverData={driverData}
            onComplete={handlePrintComplete}
          />
        );
      case STAGES.COMPLETE:
        return (
          <div
            className="min-h-screen flex items-center justify-center"
            style={{ background: isDark
              ? "linear-gradient(135deg, #111827 0%, #1f2937 50%, #111827 100%)"
              : "linear-gradient(135deg, #fafafa 0%, #ffffff 50%, #f9fafb 100%)"
            }}
          >
            <div className="text-center max-w-xl px-6">
              {/* ✓ circle */}
              <div
                className="w-40 h-40 mx-auto mb-8 rounded-full flex items-center justify-center shadow-xl"
                style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)" }}
              >
                <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              </div>

              <h1 className="text-6xl font-bold mb-4" style={{ color: isDark ? "#fff" : "#111827" }}>
                Thank You!
              </h1>
              <p className="text-2xl mb-3" style={{ color: isDark ? "#9ca3af" : "#6b7280" }}>
                Transaction completed successfully
              </p>
              <p className="text-lg" style={{ color: isDark ? "#6b7280" : "#9ca3af" }}>
                Resetting in 10 seconds…
              </p>

              <button
                onClick={handleReset}
                className="mt-8 px-8 py-3.5 rounded-xl font-semibold text-lg text-white transition-all hover:shadow-lg"
                style={{ background: "linear-gradient(135deg, #d97706, #f59e0b)", boxShadow: "0 3px 10px rgba(217,119,6,0.35)" }}
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
      {renderStage()}

      {/* Loading overlay */}
      {loading && (
        <div className="fixed inset-0 flex items-center justify-center" style={{ background:"rgba(0,0,0,0.45)", zIndex:9999 }}>
          <div className="bg-white rounded-2xl p-10 text-center shadow-2xl border border-gray-100">
            <div className="w-16 h-16 border-4 border-amber-500 border-t-transparent rounded-full animate-spin mx-auto mb-5" />
            <p className="text-xl font-semibold" style={{ color:"#111827" }}>Processing…</p>
          </div>
        </div>
      )}
    </div>
  );
}

/* ── Public export — ThemeProvider + ErrorBoundary, no Redux, no auth ── */
export default function SelfServiceWeighing() {
  return (
    <KioskErrorBoundary>
      <ThemeProvider>
        <KioskInner />
      </ThemeProvider>
    </KioskErrorBoundary>
  );
}