/**
 * SelfServiceWeighing.jsx — Kiosk Orchestrator
 *
 * Changes from original:
 *  1. handleVehicleDetected now reads pendingTxn from VehicleDetectionScreen
 *     and passes it down to WeighingScreen via secondWeighingMode prop.
 *  2. WeighingScreen receives existingTransaction when pendingTxn is present
 *     so it can submit a PATCH / second-weight POST instead of a new POST.
 *
 * Flow:
 *   VEHICLE_DETECTION  → RFID + pending check + NFC auth
 *        ↓  { vehicle, driver, pendingTxn }
 *   WEIGHING           → if pendingTxn: second-weight mode; else: first weight
 *        ↓  onWeighingComplete(weighData)
 *   TICKET_PRINT       → receipt
 *        ↓  onComplete()
 *   COMPLETE           → thank-you, auto-reset 10s
 */

import React, { useState, useEffect, useCallback } from "react";
import { message } from "antd";
import { ThemeProvider, useTheme } from "../components/Context/ThemeContext.jsx";
import VehicleDetectionScreen from "../components/SelfService/VehicleDetectionScreen";
import WeighingScreen         from "../components/SelfService/WeighingScreen";
import TicketPrintScreen      from "../components/SelfService/TicketPrintScreen";

const STAGES = {
  VEHICLE_DETECTION: "vehicle_detection",
  WEIGHING:          "weighing",
  TICKET_PRINT:      "ticket_print",
  COMPLETE:          "complete",
};

// ─── Error boundary ───────────────────────────────────────────────────────────
class KioskErrorBoundary extends React.Component {
  state = { hasError: false, error: null };
  static getDerivedStateFromError(e) { return { hasError: true, error: e }; }
  componentDidCatch(e, i)            { console.error("🛑 KioskBoundary:", e, i); }

  render() {
    if (!this.state.hasError) return this.props.children;
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ background: "#fafafa" }}>
        <div className="text-center max-w-md px-6 py-12 rounded-2xl shadow-lg"
          style={{ background: "#fff", border: "1px solid #e5e7eb" }}>
          <div className="w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4"
            style={{ background: "#fef2f2" }}>
            <span className="text-3xl">⚠️</span>
          </div>
          <h2 className="text-xl font-bold mb-2" style={{ color: "#111827" }}>Kiosk Error</h2>
          <p className="text-sm mb-1" style={{ color: "#6b7280" }}>Something went wrong. Check the browser console.</p>
          <p className="text-xs mb-4 font-mono break-all" style={{ color: "#ef4444" }}>{this.state.error?.message}</p>
          <button onClick={() => window.location.reload()}
            className="px-5 py-2 rounded-lg text-sm font-semibold text-white"
            style={{ background: "linear-gradient(135deg,#d97706,#f59e0b)" }}>
            Reload Kiosk
          </button>
        </div>
      </div>
    );
  }
}

// ─── Inner kiosk ─────────────────────────────────────────────────────────────
function KioskInner() {
  const { isDark } = useTheme();

  const [stage,               setStage]               = useState(STAGES.VEHICLE_DETECTION);
  const [vehicleData,         setVehicleData]         = useState(null);
  const [driverData,          setDriverData]          = useState(null);
  const [existingTransaction, setExistingTransaction] = useState(null); // ← pending txn
  const [transactionData,     setTransactionData]     = useState(null);
  const [error,               setError]               = useState(null);

  useEffect(() => { console.log("📍 Stage →", stage); }, [stage]);

  const handleReset = useCallback(() => {
    setStage(STAGES.VEHICLE_DETECTION);
    setVehicleData(null);
    setDriverData(null);
    setExistingTransaction(null);
    setTransactionData(null);
    setError(null);
  }, []);

  useEffect(() => {
    if (stage !== STAGES.COMPLETE) return;
    const t = setTimeout(handleReset, 10_000);
    return () => clearTimeout(t);
  }, [stage, handleReset]);

  // ── STAGE 1 callback ──────────────────────────────────────────────────────
  // VehicleDetectionScreen now includes pendingTxn in its result
  const handleVehicleDetected = useCallback((result) => {
    console.log("✅ handleVehicleDetected:", result);

    const vehicle    = result?.vehicle    ?? result;
    const driver     = result?.driver     ?? null;
    const pendingTxn = result?.pendingTxn ?? null;

    setVehicleData(vehicle);
    setDriverData(driver);
    setExistingTransaction(pendingTxn); // null = first weight, object = second weight

    const plate      = vehicle?.plateNumber ?? vehicle?.registrationNumber ?? vehicle?.noPlate ?? "Unknown";
    const driverName = driver?.name ?? driver?.fullName ?? "Driver";

    if (pendingTxn) {
      const fw = pendingTxn.firstWeight ?? pendingTxn.grossWeight ?? "—";
      message.warning(`⚠️ ${plate} — completing existing transaction (first weight: ${fw} kg)`, 4);
    } else {
      message.success(`✅ ${plate} verified — Welcome, ${driverName}!`, 3);
    }

    setError(null);
    setStage(STAGES.WEIGHING);
  }, []);

  // ── STAGE 2 callback ──────────────────────────────────────────────────────
  const handleWeighingComplete = useCallback((weighData) => {
    console.log("⚖️ handleWeighingComplete:", weighData);

    const txn = {
      id:              weighData?.ticketID ?? weighData?.id ?? ("TXN_" + Date.now()),
      ticketID:        weighData?.ticketID ?? weighData?.id ?? ("TXN_" + Date.now()),
      receiptNo:       weighData?.receiptNo ?? weighData?.data?.receiptNo ?? ("RCPT-" + Date.now()),

      noPlate:         weighData.noPlate         || vehicleData?.registrationNumber || vehicleData?.plateNumber || "",
      vehicleID:       weighData.vehicleID        || vehicleData?.id     || null,

      driverName:      weighData.driverName       || driverData?.name    || "",
      driverID:        weighData.driverID         || driverData?.id      || null,

      firstWeight:     existingTransaction
                         ? (existingTransaction.firstWeight ?? existingTransaction.grossWeight ?? 0)
                         : (weighData.weight ?? weighData.firstWeight ?? 0),
      secondWeight:    existingTransaction
                         ? (weighData.weight ?? weighData.secondWeight ?? 0)
                         : null,

      commodityName:   weighData.commodityName    || "",
      transporterName: weighData.transporterName  || "",
      supplierName:    weighData.supplierName     || "",
      customerName:    weighData.customerName     || "",
      originName:      weighData.originName       || "",
      destinationName: weighData.destinationName  || "",
      weighBridgeName: weighData.weighBridgeName  || weighData.scaleName || "",
      operation:       weighData.operation        || "weighing",
      weighMode:       weighData.weighMode        || "entry",
      notes:           weighData.notes            || "",

      isCompleted: !!existingTransaction,
      status:      existingTransaction ? "Complete" : "Incomplete",
      createdAt:   existingTransaction?.createdAt ?? new Date().toISOString(),
      completedAt: existingTransaction ? new Date().toISOString() : null,
    };

    setTransactionData(txn);
    setError(null);
    message.success(existingTransaction ? "Transaction completed!" : "First weight captured!", 3);
    setStage(STAGES.TICKET_PRINT);
  }, [vehicleData, driverData, existingTransaction]);

  // ── STAGE 3 callback ──────────────────────────────────────────────────────
  const handlePrintComplete = useCallback(() => setStage(STAGES.COMPLETE), []);

  // ── Render ────────────────────────────────────────────────────────────────
  switch (stage) {

    case STAGES.VEHICLE_DETECTION:
      return (
        <VehicleDetectionScreen
          onVehicleDetected={handleVehicleDetected}
          error={error}
          onReset={handleReset}
        />
      );

    case STAGES.WEIGHING:
      return (
        <WeighingScreen
          vehicleData={{ vehicle: vehicleData, driver: driverData }}
          driverData={driverData}
          existingTransaction={existingTransaction}   // ← NEW: null or pending txn
          onWeighingComplete={handleWeighingComplete}
          onBack={handleReset}
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
        <div className="min-h-screen flex items-center justify-center"
          style={{
            background: isDark
              ? "linear-gradient(135deg,#111827 0%,#1f2937 50%,#111827 100%)"
              : "linear-gradient(135deg,#fffbeb 0%,#fff 60%,#fff7ed 100%)",
          }}>
          <div className="text-center max-w-xl px-6">
            <div className="w-40 h-40 mx-auto mb-8 rounded-full flex items-center justify-center shadow-2xl"
              style={{ background: "linear-gradient(135deg,#d97706,#f59e0b)", boxShadow: "0 20px 60px rgba(217,119,6,0.35)" }}>
              <svg width="80" height="80" viewBox="0 0 24 24" fill="none"
                stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </div>
            <h1 className="text-6xl font-black mb-4" style={{ color: isDark ? "#fff" : "#111827" }}>
              Thank You!
            </h1>
            <p className="text-2xl mb-2" style={{ color: isDark ? "#9ca3af" : "#6b7280" }}>
              {transactionData?.isCompleted ? "Transaction completed successfully" : "First weight captured successfully"}
            </p>
            {transactionData?.receiptNo && (
              <p className="font-mono text-lg mb-3" style={{ color: isDark ? "#6b7280" : "#9ca3af" }}>
                Receipt: <strong>{transactionData.receiptNo}</strong>
              </p>
            )}
            {transactionData?.firstWeight && transactionData?.secondWeight ? (
              <div className="mb-4 flex items-center justify-center gap-6">
                <div className="text-center">
                  <p className="text-xs uppercase tracking-wider" style={{ color: "#9ca3af" }}>Gross</p>
                  <p className="text-2xl font-black" style={{ color: isDark ? "#fff" : "#111827" }}>{transactionData.firstWeight} kg</p>
                </div>
                <div className="text-center">
                  <p className="text-xs uppercase tracking-wider" style={{ color: "#9ca3af" }}>Tare</p>
                  <p className="text-2xl font-black" style={{ color: isDark ? "#fff" : "#111827" }}>{transactionData.secondWeight} kg</p>
                </div>
                <div className="text-center">
                  <p className="text-xs uppercase tracking-wider" style={{ color: "#9ca3af" }}>Net</p>
                  <p className="text-2xl font-black text-green-500">{transactionData.firstWeight - transactionData.secondWeight} kg</p>
                </div>
              </div>
            ) : null}
            {transactionData?.noPlate && (
              <p className="text-base mb-6" style={{ color: isDark ? "#6b7280" : "#9ca3af" }}>
                Vehicle: <strong>{transactionData.noPlate}</strong>
                {transactionData.driverName && <> &nbsp;·&nbsp; Driver: <strong>{transactionData.driverName}</strong></>}
              </p>
            )}
            <p className="text-base mb-8" style={{ color: isDark ? "#4b5563" : "#9ca3af" }}>
              Resetting in 10 seconds…
            </p>
            <button onClick={handleReset}
              className="px-10 py-4 rounded-2xl font-bold text-xl text-white transition-all"
              style={{ background: "linear-gradient(135deg,#d97706,#f59e0b)", boxShadow: "0 4px 15px rgba(217,119,6,0.4)" }}>
              Start New Transaction
            </button>
          </div>
        </div>
      );

    default:
      return null;
  }
}

export default function SelfServiceWeighing() {
  return (
    <KioskErrorBoundary>
      <ThemeProvider>
        <KioskInner />
      </ThemeProvider>
    </KioskErrorBoundary>
  );
}