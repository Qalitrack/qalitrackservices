/**
 * VehicleDetectionScreen.jsx
 * Theme: White / Black / Amber-600 — full light + dark mode
 * Removed: all purple, all green accents replaced with amber/black
 */

import React, { useCallback, useEffect, useRef, useState } from "react";
import { Alert, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import AuthenticationMethodModal from "./AuthenticationMethodModal.jsx";
import { getVehicleByRfid } from "../../api/MasterData/Vehicles";
import { apiClient } from "../../api/helpers/apiClients";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import { usePendingTransaction } from "../../hooks/usePendingTransaction";

const CONTROL_TYPES = new Set(["connected", "heartbeat", "ping", "pong", "keepalive"]);

const parseRfidCode = (raw) => {
  try {
    const p = JSON.parse(raw);
    if (typeof p === "string" && p.trim()) return p.trim();
    if (p && typeof p === "object") {
      if (CONTROL_TYPES.has(p.type)) return null;
      return p.rfid ?? p.rfidCode ?? p.tag ?? p.tagId ?? p.code ?? p.uid ?? null;
    }
    return null;
  } catch {
    return String(raw).trim() || null;
  }
};

const extractVehicle = (raw) => {
  if (!raw) return null;
  for (const c of [raw, raw?.data, raw?.data?.data, raw?.vehicle, raw?.data?.vehicle]) {
    if (c && typeof c === "object" && !Array.isArray(c) && (c.id || c.registrationNumber)) return c;
  }
  return null;
};

const pickName = (res) => {
  const obj = res?.data?.data ?? res?.data ?? res ?? {};
  return obj.name ?? obj.fullName ?? obj.ownerName ?? obj.saccoName ?? obj.companyName ?? obj.businessName ?? null;
};

const enrichVehicleNames = async (v) => {
  let ownerName = v.ownerName ?? null, transporterName = v.transporterName ?? null,
      supplierName = v.supplierName ?? null, saccoName = v.saccoName ?? null;
  const fetches = [];
  if (!ownerName && v.ownerId)       fetches.push(apiClient.get(`/Owners/${v.ownerId}`).then(r => { ownerName = pickName(r) ?? ownerName; }).catch(() => {}));
  if (!supplierName && v.supplierId) fetches.push(apiClient.get(`/MasterData/Suppliers/${v.supplierId}`).then(r => { supplierName = pickName(r) ?? supplierName; }).catch(() => {}));
  if (!transporterName && v.transporterId) fetches.push(apiClient.get(`/MasterData/Transporters/${v.transporterId}`).then(r => { transporterName = pickName(r) ?? transporterName; }).catch(() => {}));
  if (!transporterName && v.saccoId) fetches.push(apiClient.get(`/MasterData/Saccos/${v.saccoId}`).then(r => { saccoName = pickName(r) ?? saccoName; transporterName = transporterName ?? saccoName; }).catch(() => {}));
  await Promise.allSettled(fetches);
  return { ownerName, supplierName, transporterName, saccoName };
};

const normaliseVehicle = (v, rfidCode, enriched = {}) => ({
  id: v.id,
  rfidTag: v.rfiDcode ?? rfidCode,
  registrationNumber: v.registrationNumber ?? "—",
  vehicleType:    v.type              ?? null,
  vehicleMake:    v.make              ?? null,
  vehicleModel:   v.model             ?? null,
  capacity:       v.netWeightCapacity ?? v.grossWeight ?? null,
  status:         v.status            ?? null,
  isActive:       !v.isDeleted,
  detectedAt:     new Date(),
  ownerId:        v.ownerId           ?? null,
  ownerName:      v.ownerName         ?? enriched.ownerName       ?? null,
  transporterId:  v.transporterId     ?? null,
  transporterName:v.transporterName   ?? enriched.transporterName ?? enriched.saccoName ?? null,
  supplierId:     v.supplierId        ?? null,
  supplierName:   v.supplierName      ?? enriched.supplierName    ?? null,
  saccoId:        v.saccoId           ?? null,
  saccoName:      v.saccoName         ?? enriched.saccoName       ?? null,
  driverIds:      v.driverIds         ?? [],
  driverNames:    v.driverNames       ?? [],
});

// ─── TOKEN ────────────────────────────────────────────────────────────────────
function getKioskToken() {
  try {
    const s1 = sessionStorage.getItem("authSession");
    if (s1) { const p = JSON.parse(s1); const t = p?.token ?? p?.accessToken; if (t) return t; }
  } catch {}
  return null;
}

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function VehicleDetectionScreen({ onVehicleDetected, error: externalError, onReset }) {
  const { isDark, toggleTheme } = useTheme();

  const hwConfig      = useHardwareConfig();
  const rfidStreamUrl = hwConfig.rfidStreamUrl;

  const { checking: checkingPending, pendingTxn, checkPending, clearPending } = usePendingTransaction();

  const [streamStatus,  setStreamStatus]  = useState("connecting");
  const [vehicleStatus, setVehicleStatus] = useState("idle");
  const [rfidCode,      setRfidCode]      = useState(null);
  const [vehicleData,   setVehicleData]   = useState(null);
  const [streamError,   setStreamError]   = useState(null);
  const [lookupError,   setLookupError]   = useState(null);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [pulsePhase,    setPulsePhase]    = useState(0);
  const [debugLog,      setDebugLog]      = useState([]);
  const [showDebug,     setShowDebug]     = useState(false);
  const [enriching,     setEnriching]     = useState(false);

  const esRef     = useRef(null);
  const lookupRef = useRef(false);
  const lastCode  = useRef(null);

  const dbg = useCallback((msg, data) => {
    const line = `[${new Date().toLocaleTimeString()}] ${msg}${data !== undefined ? " → " + JSON.stringify(data) : ""}`;
    console.log("🔍", line);
    setDebugLog(p => [line, ...p].slice(0, 40));
  }, []);

  useEffect(() => {
    const id = setInterval(() => setPulsePhase(p => (p + 1) % 3), 800);
    return () => clearInterval(id);
  }, []);

  const openStream = useCallback(() => {
    if (esRef.current) { esRef.current.close(); esRef.current = null; }
    setStreamStatus("connecting"); setStreamError(null);
    dbg("Opening RFID stream", rfidStreamUrl);
    try {
      const es = new EventSource(rfidStreamUrl);
      esRef.current = es;
      es.onopen    = () => { setStreamStatus("listening"); dbg("Stream connected ✓"); };
      es.onmessage = (event) => {
        dbg("SSE raw", event.data);
        const code = parseRfidCode(event.data);
        if (!code) { dbg("Skipped (control msg)"); return; }
        lastCode.current = code;
        lookupRef.current = false;
        setRfidCode(code);
        setStreamStatus("tag_received");
      };
      es.onerror = () => {
        setStreamError("Cannot connect to RFID reader at " + rfidStreamUrl);
        setStreamStatus("error");
        es.close(); esRef.current = null;
      };
    } catch {
      setStreamError("Failed to open SSE connection.");
      setStreamStatus("error");
    }
  }, [rfidStreamUrl, dbg]);

  useEffect(() => {
    openStream();
    return () => { if (esRef.current) esRef.current.close(); };
  }, [openStream]);

  useEffect(() => {
    if (!rfidCode || lookupRef.current) return;
    lookupRef.current = true;

    (async () => {
      setVehicleStatus("loading");
      setVehicleData(null);
      setLookupError(null);
      setShowAuthModal(false);
      clearPending();
      dbg("Fetching vehicle", rfidCode);

      try {
        const raw = await getVehicleByRfid(rfidCode);
        const v   = extractVehicle(raw);
        if (!v) { setVehicleStatus("not_found"); return; }

        const partial = normaliseVehicle(v, rfidCode);
        setVehicleData(partial);
        setVehicleStatus("found");

        const needsEnrichment = !v.ownerName || !v.supplierName || !v.transporterName;
        if (needsEnrichment) {
          setEnriching(true);
          try {
            const enriched = await enrichVehicleNames(v);
            setVehicleData(normaliseVehicle(v, rfidCode, enriched));
          } finally {
            setEnriching(false);
          }
        }

        const plate = v.registrationNumber ?? partial.registrationNumber;
        await checkPending(plate);
        setTimeout(() => setShowAuthModal(true), 1200);

      } catch (err) {
        const notFound = err.message?.includes("404") || /not found|no vehicle/i.test(err.message ?? "");
        if (notFound) setVehicleStatus("not_found");
        else { setVehicleStatus("error"); setLookupError(err.message || "Server error."); }
      }
    })();
  }, [rfidCode, dbg, checkPending, clearPending]);

  const handleRescan = () => {
    lastCode.current = null; lookupRef.current = false;
    setRfidCode(null); setVehicleData(null); setVehicleStatus("idle");
    setStreamStatus("listening"); setLookupError(null);
    setShowAuthModal(false); clearPending();
  };

  const handleRetryLookup = () => {
    const code = lastCode.current;
    if (!code) return;
    lookupRef.current = false;
    setRfidCode(null);
    setTimeout(() => setRfidCode(code), 50);
  };

  const handleAuthComplete = (rawD) => {
    setShowAuthModal(false);
    const d = rawD?.data?.driver ?? rawD?.driver ?? rawD?.data ?? rawD ?? {};
    const driver = {
      id:         d.id         ?? d.driverId    ?? null,
      name:       d.name       ?? d.fullName    ?? d.driverName ?? "",
      phone:      d.phone      ?? d.phoneNumber ?? "",
      licenseNo:  d.licenseNo  ?? d.license     ?? "",
      employeeId: d.employeeId ?? d.employee_id ?? "",
      uid:        d.uid        ?? d.nfcUid      ?? "",
    };
    onVehicleDetected({ vehicle: vehicleData, driver, pendingTxn: pendingTxn ?? null });
  };

  const isListening  = streamStatus === "connecting" || streamStatus === "listening";
  const streamFailed = streamStatus === "error";

  // ── Colours ────────────────────────────────────────────────────────────────
  const bg    = isDark ? "#0a0a0a"   : "#f8f7f4";
  const card  = isDark ? "#111827"   : "#ffffff";
  const bdr   = isDark ? "#1f2937"   : "#e5e7eb";
  const txt   = isDark ? "#f9fafb"   : "#111827";
  const muted = isDark ? "#6b7280"   : "#9ca3af";
  const sub   = isDark ? "#374151"   : "#f3f4f6";

  const statusColor = streamFailed           ? "#ef4444"
                    : streamStatus === "tag_received" ? "#d97706"
                    : "#d97706";

  const vehicleStatusColor = vehicleStatus === "found"    ? "#d97706"
                           : vehicleStatus === "loading"  ? "#d97706"
                           : vehicleStatus === "error" || vehicleStatus === "not_found" ? "#ef4444"
                           : "#d97706";

  // Pending banner
  const PendingBanner = () => {
    if (vehicleStatus !== "found") return null;
    if (checkingPending) {
      return (
        <div style={{ display: "flex", alignItems: "center", gap: 8, padding: "10px 14px", borderRadius: 10, background: "#fffbeb", border: "1px solid #fcd34d", fontSize: 12, fontWeight: 600, color: "#92400e" }}>
          <span style={{ width: 12, height: 12, borderRadius: "50%", border: "2px solid #d97706", borderTopColor: "transparent", display: "inline-block", animation: "spin 0.7s linear infinite", flexShrink: 0 }} />
          Checking for pending transaction…
        </div>
      );
    }
    if (pendingTxn) {
      const fw = pendingTxn.firstWeight ?? pendingTxn.grossWeight ?? "—";
      const at = pendingTxn.createdAt ? new Date(pendingTxn.createdAt).toLocaleString() : "Unknown time";
      return (
        <div style={{ padding: "12px 14px", borderRadius: 10, background: "#fff7ed", border: "1.5px solid #d97706" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 6, marginBottom: 6 }}>
            <span>⚠️</span>
            <p style={{ fontWeight: 800, fontSize: 13, color: "#92400e", margin: 0 }}>Pending Transaction Detected</p>
          </div>
          <p style={{ fontSize: 12, color: "#78350f", margin: "0 0 4px" }}>
            Incomplete weighing from <strong>{at}</strong>. First weight: <strong>{fw} kg</strong>.
          </p>
          <p style={{ fontSize: 12, fontWeight: 700, color: "#d97706", margin: 0 }}>
            This visit → second (tare) weight.
          </p>
        </div>
      );
    }
    return null;
  };

  return (
    <div style={{ minHeight: "100vh", display: "flex", flexDirection: "column", background: bg, fontFamily: "'Inter', system-ui, sans-serif" }}>

      {/* Header */}
      <header style={{ background: card, borderBottom: `1px solid ${bdr}`, padding: "16px 32px", display: "flex", alignItems: "center", justifyContent: "space-between", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 14 }}>
          <div style={{ width: 48, height: 48, borderRadius: 14, background: "linear-gradient(135deg,#d97706,#b45309)", display: "flex", alignItems: "center", justifyContent: "center", boxShadow: "0 4px 12px rgba(180,83,9,0.3)" }}>
            <span style={{ color: "#fff", fontSize: 18, fontWeight: 900 }}>KW</span>
          </div>
          <div>
            <h1 style={{ fontSize: 20, fontWeight: 900, color: txt, margin: 0 }}>Self-Service Weighing</h1>
            <p style={{ fontSize: 11, color: muted, margin: 0, fontFamily: "monospace" }}>RFID: {rfidStreamUrl}</p>
          </div>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          {/* Stream status pill */}
          <div style={{
            display: "flex", alignItems: "center", gap: 6, padding: "6px 12px", borderRadius: 20,
            fontSize: 11, fontWeight: 700,
            background: streamFailed ? "#fef2f2" : "#fff7ed",
            border: `1px solid ${streamFailed ? "#fca5a5" : "#fcd34d"}`,
            color: streamFailed ? "#dc2626" : "#92400e",
          }}>
            <span style={{ width: 7, height: 7, borderRadius: "50%", background: streamFailed ? "#ef4444" : "#d97706", display: "inline-block", animation: streamFailed ? "none" : "pulse 1.5s ease-in-out infinite" }} />
            {streamStatus === "connecting"   && "Connecting…"}
            {streamStatus === "listening"    && "Listening"}
            {streamStatus === "tag_received" && rfidCode}
            {streamStatus === "error"        && "Error"}
          </div>

          {/* Debug toggle */}
          <button onClick={() => setShowDebug(v => !v)} style={{ padding: "6px 12px", borderRadius: 8, fontSize: 11, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: muted, cursor: "pointer" }}>
            {showDebug ? "Hide Log" : "Debug"}
          </button>

          {/* Theme toggle */}
          <button onClick={toggleTheme} style={{ padding: "6px 12px", borderRadius: 8, fontSize: 13, background: sub, border: `1px solid ${bdr}`, cursor: "pointer" }}>
            {isDark ? "☀️" : "🌙"}
          </button>
        </div>
      </header>

      {/* Main */}
      <main style={{ flex: 1, padding: "32px", display: "flex", justifyContent: "center" }}>
        <div style={{ width: "100%", maxWidth: 860 }}>

          {externalError && (
            <Alert type="error" message="Error" description={externalError} showIcon
              action={<Button size="small" onClick={onReset}>Reset</Button>}
              style={{ marginBottom: 20, borderRadius: 10 }} />
          )}

          {/* Main RFID detection card */}
          <div style={{ background: card, border: `1px solid ${bdr}`, borderRadius: 20, overflow: "hidden", boxShadow: "0 4px 24px rgba(0,0,0,0.08)", marginBottom: 20 }}>

            {/* Colour bar */}
            <div style={{ height: 4, background: vehicleStatus === "found" ? "linear-gradient(90deg,#d97706,#f59e0b)" : vehicleStatus === "loading" ? "linear-gradient(90deg,#d97706,#fbbf24)" : vehicleStatus === "error" || vehicleStatus === "not_found" ? "#ef4444" : streamFailed ? "#ef4444" : "linear-gradient(90deg,#d97706,#b45309)" }} />

            <div style={{ padding: 32, display: "flex", alignItems: "center", gap: 32 }}>

              {/* Animated icon */}
              <div style={{ position: "relative", width: 144, height: 144, flexShrink: 0, display: "flex", alignItems: "center", justifyContent: "center" }}>
                {isListening && [0, 1, 2].map(i => {
                  const ph = (pulsePhase + i) % 3;
                  return (
                    <span key={i} style={{
                      position: "absolute", inset: 0, borderRadius: "50%",
                      border: "2px solid rgba(217,119,6,0.2)",
                      transform: `scale(${1 + (ph / 3) * 0.65})`,
                      opacity: 1 - (ph / 3) * 0.9,
                      transition: "all 0.7s ease",
                    }} />
                  );
                })}
                <div style={{
                  width: 112, height: 112, borderRadius: "50%",
                  display: "flex", alignItems: "center", justifyContent: "center",
                  boxShadow: "0 8px 32px rgba(180,83,9,0.3)",
                  background: vehicleStatus === "error" || vehicleStatus === "not_found" || streamFailed
                    ? "#ef4444"
                    : "linear-gradient(135deg,#d97706,#b45309)",
                }}>
                  {vehicleStatus === "loading" ? (
                    <div style={{ width: 36, height: 36, border: "3px solid rgba(255,255,255,0.3)", borderTopColor: "#fff", borderRadius: "50%", animation: "spin 0.8s linear infinite" }} />
                  ) : vehicleStatus === "found" ? (
                    <svg width="52" height="52" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5"><path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" /></svg>
                  ) : vehicleStatus === "not_found" || vehicleStatus === "error" || streamFailed ? (
                    <svg width="52" height="52" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5"><path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
                  ) : (
                    <svg width="52" height="52" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="1.5"><path strokeLinecap="round" strokeLinejoin="round" d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z" /></svg>
                  )}
                </div>
              </div>

              {/* Status text */}
              <div style={{ flex: 1, minWidth: 0 }}>
                {streamStatus === "connecting" && (
                  <StatusBlurb label="Initialising" title="Connecting to RFID Reader…" sub={rfidStreamUrl} txt={txt} muted={muted} />
                )}

                {streamStatus === "listening" && vehicleStatus === "idle" && (
                  <StatusBlurb label="Ready" title="Awaiting Vehicle RFID Tag" sub="Drive into the reader zone. The tag will be detected automatically." txt={txt} muted={muted}>
                    <div style={{ display: "flex", flexWrap: "wrap", gap: 8, marginTop: 16 }}>
                      {["Drive into zone", "Auto-detected", "Details populated"].map((s, i) => (
                        <span key={i} style={{ display: "flex", alignItems: "center", gap: 6, padding: "6px 12px", borderRadius: 20, fontSize: 11, fontWeight: 600, background: sub, color: muted, border: `1px solid ${bdr}` }}>
                          <span style={{ width: 18, height: 18, borderRadius: "50%", background: "#d97706", color: "#fff", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 10, fontWeight: 900, flexShrink: 0 }}>{i + 1}</span>
                          {s}
                        </span>
                      ))}
                    </div>
                  </StatusBlurb>
                )}

                {vehicleStatus === "loading" && (
                  <StatusBlurb label="Tag Detected" title="Looking Up Vehicle…" txt={txt} muted={muted}>
                    <div style={{ display: "inline-flex", alignItems: "center", gap: 8, fontFamily: "monospace", fontSize: 13, padding: "8px 16px", borderRadius: 10, marginTop: 12, background: "#fff7ed", color: "#92400e", border: "1px solid #fcd34d" }}>
                      <span style={{ width: 8, height: 8, background: "#d97706", borderRadius: "50%", animation: "pulse 1.5s ease-in-out infinite" }} />
                      {rfidCode}
                    </div>
                  </StatusBlurb>
                )}

                {vehicleStatus === "found" && vehicleData && (
                  <StatusBlurb
                    label={pendingTxn ? "⚠️ Second Weighing Required" : "Vehicle Verified"}
                    title={vehicleData.registrationNumber}
                    txt={txt} muted={muted}
                  >
                    <p style={{ fontFamily: "monospace", fontSize: 11, color: muted, marginTop: 2, marginBottom: 12 }}>RFID: {vehicleData.rfidTag}</p>

                    <div style={{ marginBottom: 12 }}><PendingBanner /></div>

                    {/* Relational fields grid */}
                    <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 8, marginBottom: 16 }}>
                      {[
                        { label: "Owner",       value: vehicleData.ownerName       },
                        { label: "Supplier",    value: vehicleData.supplierName    },
                        { label: "Transporter", value: vehicleData.transporterName },
                        { label: "SACCO",       value: vehicleData.saccoName       },
                      ].map(f => (
                        <div key={f.label} style={{ padding: "8px 12px", borderRadius: 10, fontSize: 11, border: `1px solid ${f.value ? bdr : "#fcd34d"}`, background: f.value ? sub : "#fffbeb", display: "flex", alignItems: "center", gap: 8 }}>
                          <span style={{ fontSize: 11, fontWeight: 700, width: 76, flexShrink: 0, color: f.value ? muted : "#d97706" }}>{f.label}</span>
                          {enriching && !f.value ? (
                            <span style={{ display: "flex", alignItems: "center", gap: 6, color: "#d97706", fontSize: 11, fontStyle: "italic" }}>
                              <span style={{ width: 10, height: 10, border: "2px solid #d97706", borderTopColor: "transparent", borderRadius: "50%", display: "inline-block", animation: "spin 0.7s linear infinite" }} />
                              fetching…
                            </span>
                          ) : (
                            <span style={{ fontWeight: 600, color: f.value ? txt : "#d97706", fontStyle: f.value ? "normal" : "italic", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                              {f.value || "not linked"}
                            </span>
                          )}
                        </div>
                      ))}
                    </div>

                    <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                      <button onClick={() => setShowAuthModal(true)} style={{
                        padding: "10px 20px", borderRadius: 10, fontSize: 13, fontWeight: 800,
                        color: "#fff", border: "none", cursor: "pointer",
                        background: "linear-gradient(135deg,#d97706,#b45309)",
                        boxShadow: "0 4px 12px rgba(180,83,9,0.3)",
                      }}>
                        {pendingTxn ? "Authenticate for 2nd Weight →" : "Authenticate Driver →"}
                      </button>
                      <button onClick={handleRescan} style={{ padding: "10px 16px", borderRadius: 10, fontSize: 13, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: txt, cursor: "pointer" }}>
                        🔄 Rescan
                      </button>
                    </div>
                  </StatusBlurb>
                )}

                {vehicleStatus === "not_found" && (
                  <StatusBlurb label="Not Registered" title="Vehicle Not Found" txt={txt} muted={muted}>
                    <p style={{ fontSize: 13, color: muted, margin: "8px 0 16px" }}>No vehicle linked to RFID: <span style={{ fontFamily: "monospace" }}>{rfidCode}</span></p>
                    <button onClick={handleRescan} style={{ padding: "10px 16px", borderRadius: 10, fontSize: 13, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: txt, cursor: "pointer" }}>🔄 Scan Again</button>
                  </StatusBlurb>
                )}

                {vehicleStatus === "error" && (
                  <StatusBlurb label="Lookup Failed" title="Could Not Fetch Vehicle" txt={txt} muted={muted}>
                    <p style={{ fontFamily: "monospace", fontSize: 11, color: "#ef4444", background: "#fef2f2", border: "1px solid #fecaca", borderRadius: 8, padding: "6px 12px", display: "inline-block", margin: "8px 0 16px" }}>{lookupError}</p>
                    <div style={{ display: "flex", gap: 8 }}>
                      <button onClick={handleRetryLookup} style={{ padding: "10px 16px", borderRadius: 10, fontSize: 13, fontWeight: 700, color: "#fff", background: "#d97706", border: "none", cursor: "pointer" }}>Retry</button>
                      <button onClick={handleRescan} style={{ padding: "10px 16px", borderRadius: 10, fontSize: 13, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: txt, cursor: "pointer" }}>🔄 Rescan</button>
                    </div>
                  </StatusBlurb>
                )}

                {streamFailed && (
                  <StatusBlurb label="Connection Failed" title="RFID Stream Unavailable" txt={txt} muted={muted}>
                    <p style={{ fontSize: 13, color: muted, margin: "8px 0 16px" }}>{streamError}</p>
                    <button onClick={openStream} style={{ padding: "10px 16px", borderRadius: 10, fontSize: 13, fontWeight: 700, color: "#fff", background: "#d97706", border: "none", cursor: "pointer" }}>🔄 Reconnect</button>
                  </StatusBlurb>
                )}
              </div>
            </div>
          </div>

          {/* Expanded vehicle detail card */}
          {vehicleStatus === "found" && vehicleData && (
            <div style={{ background: card, border: `1px solid ${bdr}`, borderRadius: 20, overflow: "hidden", boxShadow: "0 4px 24px rgba(0,0,0,0.06)" }}>
              {/* Card header band */}
              <div style={{ padding: "20px 28px", display: "flex", alignItems: "center", justifyContent: "space-between", background: pendingTxn ? "linear-gradient(135deg,#d97706,#b45309)" : "linear-gradient(135deg,#111827,#374151)" }}>
                <div>
                  <p style={{ fontSize: 11, fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.1em", color: "rgba(255,255,255,0.6)", margin: "0 0 4px" }}>
                    {pendingTxn ? "Second (Tare) Weighing" : "Registered Vehicle"}
                  </p>
                  <p style={{ fontSize: 28, fontWeight: 900, letterSpacing: "0.12em", color: "#fff", margin: 0 }}>{vehicleData.registrationNumber}</p>
                  {pendingTxn && (
                    <p style={{ fontSize: 11, color: "rgba(255,255,255,0.7)", marginTop: 4 }}>
                      Completing transaction from {pendingTxn.createdAt ? new Date(pendingTxn.createdAt).toLocaleString() : "earlier"}
                      {" · "}First weight: <strong>{pendingTxn.firstWeight ?? pendingTxn.grossWeight ?? "—"} kg</strong>
                    </p>
                  )}
                </div>
                <div style={{ textAlign: "right" }}>
                  <p style={{ fontSize: 11, color: "rgba(255,255,255,0.6)", margin: "0 0 2px" }}>RFID Tag</p>
                  <p style={{ fontFamily: "monospace", fontSize: 13, color: "#fff", margin: "0 0 4px" }}>{vehicleData.rfidTag}</p>
                  <p style={{ fontSize: 11, color: "rgba(255,255,255,0.5)", margin: 0 }}>{vehicleData.detectedAt?.toLocaleTimeString()}</p>
                </div>
              </div>

              {/* Vehicle attributes grid */}
              <div style={{ padding: 24, display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 12 }}>
                {[
                  { label: "Type",        value: vehicleData.vehicleType     },
                  { label: "Make",        value: vehicleData.vehicleMake     },
                  { label: "Model",       value: vehicleData.vehicleModel    },
                  { label: "Capacity",    value: vehicleData.capacity ? `${vehicleData.capacity} kg` : null },
                  { label: "Owner",       value: vehicleData.ownerName       },
                  { label: "Supplier",    value: vehicleData.supplierName    },
                  { label: "Transporter", value: vehicleData.transporterName },
                  { label: "SACCO",       value: vehicleData.saccoName       },
                  { label: "Status",      value: vehicleData.status, badge: vehicleData.isActive ? "amber" : "red" },
                ].filter(f => f.value).map(f => (
                  <div key={f.label} style={{ padding: "12px 14px", borderRadius: 12, background: sub, border: `1px solid ${bdr}` }}>
                    <p style={{ fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.08em", color: muted, marginBottom: 4 }}>{f.label}</p>
                    {f.badge ? (
                      <span style={{ padding: "2px 8px", borderRadius: 20, fontSize: 11, fontWeight: 700, background: f.badge === "amber" ? "#fff7ed" : "#fef2f2", color: f.badge === "amber" ? "#d97706" : "#dc2626", border: `1px solid ${f.badge === "amber" ? "#fcd34d" : "#fca5a5"}` }}>
                        {f.value}
                      </span>
                    ) : (
                      <p style={{ fontSize: 13, fontWeight: 700, color: txt, margin: 0 }}>{f.value}</p>
                    )}
                  </div>
                ))}
                {enriching && (
                  <div style={{ gridColumn: "span 2", padding: "12px 14px", borderRadius: 12, background: "#fffbeb", border: "1px solid #fcd34d", display: "flex", alignItems: "center", gap: 8 }}>
                    <span style={{ width: 14, height: 14, border: "2px solid #d97706", borderTopColor: "transparent", borderRadius: "50%", display: "inline-block", animation: "spin 0.7s linear infinite" }} />
                    <span style={{ fontSize: 12, fontWeight: 600, color: "#92400e" }}>Fetching relational names…</span>
                  </div>
                )}
              </div>

              <div style={{ padding: "16px 24px", borderTop: `1px solid ${bdr}`, display: "flex", alignItems: "center", justifyContent: "space-between", background: sub }}>
                <p style={{ fontSize: 12, color: muted, margin: 0 }}>
                  {pendingTxn ? "Tap your NFC card to authenticate and capture the second (tare) weight" : "Tap your NFC card to authenticate and proceed to weighing"}
                </p>
                <button onClick={() => setShowAuthModal(true)} style={{
                  padding: "12px 28px", borderRadius: 12, fontSize: 14, fontWeight: 800,
                  color: "#fff", border: "none", cursor: "pointer",
                  background: "linear-gradient(135deg,#d97706,#b45309)",
                  boxShadow: "0 4px 14px rgba(180,83,9,0.35)",
                }}>
                  {pendingTxn ? "Authenticate for 2nd Weight →" : "Authenticate Driver →"}
                </button>
              </div>
            </div>
          )}

          {/* Debug log */}
          {showDebug && (
            <div style={{ marginTop: 20, borderRadius: 14, border: "1px solid #1f2937", background: "#030712", padding: 20, fontFamily: "monospace", fontSize: 11, color: "#34d399" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 10 }}>
                <span style={{ fontFamily: "system-ui", fontWeight: 700, fontSize: 10, textTransform: "uppercase", letterSpacing: "0.1em", color: "#4b5563" }}>Debug Log</span>
                <button onClick={() => setDebugLog([])} style={{ fontSize: 10, color: "#4b5563", background: "none", border: "none", cursor: "pointer" }}>Clear</button>
              </div>
              {debugLog.length === 0
                ? <p style={{ color: "#374151" }}>No messages yet…</p>
                : <div style={{ maxHeight: 200, overflowY: "auto" }}>{debugLog.map((e, i) => <p key={i} style={{ margin: "2px 0", wordBreak: "break-all", lineHeight: 1.5 }}>{e}</p>)}</div>
              }
            </div>
          )}
        </div>
      </main>

      <AuthenticationMethodModal
        visible={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        onSelectNFC={handleAuthComplete}
      />

      <footer style={{ background: card, borderTop: `1px solid ${bdr}`, padding: "14px 32px", textAlign: "center" }}>
        <p style={{ fontSize: 11, color: muted, margin: 0 }}>
          Powered by <span style={{ fontWeight: 900, color: "#d97706" }}>QALIBRATED SYSTEMS</span>
        </p>
      </footer>

      <style>{`
        @keyframes spin { to { transform: rotate(360deg); } }
        @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:0.4; } }
      `}</style>
    </div>
  );
}

function StatusBlurb({ label, title, sub, txt, muted, children }) {
  return (
    <div>
      <p style={{ fontSize: 10, fontWeight: 800, textTransform: "uppercase", letterSpacing: "0.12em", color: "#d97706", marginBottom: 4 }}>{label}</p>
      <h2 style={{ fontSize: 26, fontWeight: 900, color: txt, margin: "0 0 4px", letterSpacing: "-0.5px" }}>{title}</h2>
      {sub && <p style={{ fontSize: 13, color: muted, margin: 0 }}>{sub}</p>}
      {children}
    </div>
  );
}