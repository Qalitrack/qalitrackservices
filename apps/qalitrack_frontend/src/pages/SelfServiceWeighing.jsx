/**
 * SelfServiceWeighing.jsx — Kiosk Orchestrator
 * Theme: White / Black / Amber-600 — full light + dark mode
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

// ─── Error boundary ────────────────────────────────────────────────────────────
class KioskErrorBoundary extends React.Component {
  state = { hasError: false, error: null };
  static getDerivedStateFromError(e) { return { hasError: true, error: e }; }
  componentDidCatch(e, i) { console.error("🛑 KioskBoundary:", e, i); }

  render() {
    if (!this.state.hasError) return this.props.children;
    return (
      <div style={{ minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center", background: "#fafafa" }}>
        <div style={{ textAlign: "center", maxWidth: 420, padding: "48px 32px", borderRadius: 20, background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 8px 40px rgba(0,0,0,0.08)" }}>
          <div style={{ width: 64, height: 64, borderRadius: "50%", background: "#fff7ed", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 16px", fontSize: 28 }}>⚠️</div>
          <h2 style={{ fontSize: 20, fontWeight: 800, color: "#111827", marginBottom: 8 }}>Kiosk Error</h2>
          <p style={{ fontSize: 13, color: "#6b7280", marginBottom: 4 }}>Something went wrong. Check the browser console.</p>
          <p style={{ fontSize: 11, fontFamily: "monospace", color: "#d97706", marginBottom: 20, wordBreak: "break-all" }}>{this.state.error?.message}</p>
          <button onClick={() => window.location.reload()}
            style={{ padding: "10px 24px", borderRadius: 10, fontSize: 14, fontWeight: 700, color: "#fff", background: "linear-gradient(135deg,#d97706,#b45309)", border: "none", cursor: "pointer" }}>
            Reload Kiosk
          </button>
        </div>
      </div>
    );
  }
}

// ─── Inner kiosk ──────────────────────────────────────────────────────────────
function KioskInner() {
  const { isDark } = useTheme();

  const [stage,               setStage]               = useState(STAGES.VEHICLE_DETECTION);
  const [vehicleData,         setVehicleData]         = useState(null);
  const [driverData,          setDriverData]          = useState(null);
  const [existingTransaction, setExistingTransaction] = useState(null);
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

  const handleVehicleDetected = useCallback((result) => {
    const vehicle    = result?.vehicle    ?? result;
    const driver     = result?.driver     ?? null;
    const pendingTxn = result?.pendingTxn ?? null;

    setVehicleData(vehicle);
    setDriverData(driver);
    setExistingTransaction(pendingTxn);

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

  const handleWeighingComplete = useCallback((weighData) => {
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
      secondWeight:    existingTransaction ? (weighData.weight ?? weighData.secondWeight ?? 0) : null,
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
      isCompleted:     !!existingTransaction,
      status:          existingTransaction ? "Complete" : "Incomplete",
      createdAt:       existingTransaction?.createdAt ?? new Date().toISOString(),
      completedAt:     existingTransaction ? new Date().toISOString() : null,
    };

    setTransactionData(txn);
    setError(null);
    message.success(existingTransaction ? "Transaction completed!" : "First weight captured!", 3);
    setStage(STAGES.TICKET_PRINT);
  }, [vehicleData, driverData, existingTransaction]);

  const handlePrintComplete = useCallback(() => setStage(STAGES.COMPLETE), []);

  // ── Render ─────────────────────────────────────────────────────────────────
  switch (stage) {
    case STAGES.VEHICLE_DETECTION:
      return <VehicleDetectionScreen onVehicleDetected={handleVehicleDetected} error={error} onReset={handleReset} />;

    case STAGES.WEIGHING:
      return (
        <WeighingScreen
          vehicleData={{ vehicle: vehicleData, driver: driverData }}
          driverData={driverData}
          existingTransaction={existingTransaction}
          onWeighingComplete={handleWeighingComplete}
          onBack={handleReset}
          error={error}
        />
      );

    case STAGES.TICKET_PRINT:
      return <TicketPrintScreen ticketData={transactionData} vehicleData={vehicleData} driverData={driverData} onComplete={handlePrintComplete} />;

    case STAGES.COMPLETE:
      return (
        <div style={{
          minHeight: "100vh",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          background: isDark
            ? "linear-gradient(160deg,#0a0a0a 0%,#111827 50%,#0f0f0f 100%)"
            : "linear-gradient(160deg,#fffbeb 0%,#ffffff 60%,#fff7ed 100%)",
        }}>
          <div style={{ textAlign: "center", maxWidth: 560, padding: "0 24px" }}>
            {/* Icon */}
            <div style={{
              width: 140, height: 140, margin: "0 auto 32px",
              borderRadius: "50%",
              background: "linear-gradient(135deg,#d97706,#b45309)",
              display: "flex", alignItems: "center", justifyContent: "center",
              boxShadow: "0 24px 64px rgba(180,83,9,0.35)",
            }}>
              <svg width="72" height="72" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="20 6 9 17 4 12" />
              </svg>
            </div>

            <h1 style={{ fontSize: 56, fontWeight: 900, color: isDark ? "#fff" : "#111827", marginBottom: 12, letterSpacing: "-1px" }}>
              Thank You!
            </h1>
            <p style={{ fontSize: 20, color: isDark ? "#9ca3af" : "#6b7280", marginBottom: 8 }}>
              {transactionData?.isCompleted ? "Transaction completed successfully" : "First weight captured successfully"}
            </p>

            {transactionData?.receiptNo && (
              <p style={{ fontFamily: "monospace", fontSize: 15, color: isDark ? "#6b7280" : "#9ca3af", marginBottom: 20 }}>
                Receipt: <strong style={{ color: "#d97706" }}>{transactionData.receiptNo}</strong>
              </p>
            )}

            {transactionData?.firstWeight && transactionData?.secondWeight ? (
              <div style={{
                display: "flex", alignItems: "center", justifyContent: "center", gap: 32, marginBottom: 20,
                padding: "20px 32px", borderRadius: 16,
                background: isDark ? "rgba(255,255,255,0.04)" : "rgba(217,119,6,0.06)",
                border: `1px solid ${isDark ? "rgba(255,255,255,0.08)" : "rgba(217,119,6,0.15)"}`,
              }}>
                {[
                  { label: "Gross", value: transactionData.firstWeight },
                  { label: "Tare",  value: transactionData.secondWeight },
                  { label: "Net",   value: transactionData.firstWeight - transactionData.secondWeight, accent: true },
                ].map(item => (
                  <div key={item.label} style={{ textAlign: "center" }}>
                    <p style={{ fontSize: 11, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.1em", color: "#9ca3af", marginBottom: 4 }}>{item.label}</p>
                    <p style={{ fontSize: 26, fontWeight: 900, fontFamily: "monospace", color: item.accent ? "#d97706" : (isDark ? "#fff" : "#111827") }}>
                      {item.value} <span style={{ fontSize: 13, fontWeight: 400, color: "#9ca3af" }}>kg</span>
                    </p>
                  </div>
                ))}
              </div>
            ) : null}

            {transactionData?.noPlate && (
              <p style={{ fontSize: 15, color: isDark ? "#6b7280" : "#9ca3af", marginBottom: 24 }}>
                Vehicle: <strong style={{ color: isDark ? "#e5e7eb" : "#111827" }}>{transactionData.noPlate}</strong>
                {transactionData.driverName && (
                  <> &nbsp;·&nbsp; Driver: <strong style={{ color: isDark ? "#e5e7eb" : "#111827" }}>{transactionData.driverName}</strong></>
                )}
              </p>
            )}

            <p style={{ fontSize: 14, color: isDark ? "#4b5563" : "#9ca3af", marginBottom: 32 }}>
              Resetting in 10 seconds…
            </p>

            <button onClick={handleReset} style={{
              padding: "14px 40px", borderRadius: 14, fontSize: 16, fontWeight: 800,
              color: "#fff", border: "none", cursor: "pointer",
              background: "linear-gradient(135deg,#d97706,#b45309)",
              boxShadow: "0 6px 20px rgba(180,83,9,0.4)",
            }}>
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