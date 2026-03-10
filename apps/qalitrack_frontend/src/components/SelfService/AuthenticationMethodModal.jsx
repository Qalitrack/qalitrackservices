/**
 * AuthenticationMethodModal.jsx — REAL NFC MODE
 * Theme: White / Black / Amber-600 — light + dark mode
 * Removed: all purple — replaced with amber/black system
 */

import React, { useEffect, useRef, useState, useCallback } from "react";
import { Modal, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import { getDriverByNfc } from "../../api/MasterData/Drivers";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";

const CONTROL_TYPES = new Set(["connected", "heartbeat", "ping", "pong", "keepalive"]);

const parseNfcCode = (raw) => {
  try {
    const p = JSON.parse(raw);
    if (typeof p === "object" && p !== null && CONTROL_TYPES.has(p.type)) return null;
    if (typeof p === "string" && p.trim()) return p.trim().toUpperCase();
    if (p && typeof p === "object") {
      return (p.nfc ?? p.nfcCode ?? p.uid ?? p.code ?? p.cardId ?? null)?.toUpperCase();
    }
    return null;
  } catch {
    return String(raw).trim().toUpperCase() || null;
  }
};

const extractDriver = (raw) => {
  if (!raw) return null;
  for (const c of [raw, raw?.data, raw?.data?.data, raw?.driver, raw?.data?.driver]) {
    if (c && typeof c === "object" && !Array.isArray(c) && (c.id || c.fullName)) return c;
  }
  return null;
};

const normaliseDriver = (d, nfcCode) => ({
  id:            d.id,
  uid:           d.nfCcode ?? nfcCode,
  name:          d.fullName,
  employeeId:    d.employeeId    ?? null,
  phone:         d.phone         ?? null,
  email:         d.email         ?? null,
  licenseNo:     d.licenseNumber ?? null,
  licenseExpiry: d.licenseExpiryDate ?? null,
  status:        d.status        ?? "Active",
  transporterId: d.transporterId ?? null,
  supplierId:    d.supplierId    ?? null,
  assignedVehicleIds: d.assignedVehicleIds ?? [],
  detectedAt:    new Date(),
});

export default function AuthenticationMethodModal({ visible, onClose, onSelectNFC }) {
  const { isDark } = useTheme();

  const hwConfig     = useHardwareConfig();
  const nfcStreamUrl = hwConfig.nfcStreamUrl;

  const [streamStatus, setStreamStatus] = useState("connecting");
  const [lookupStatus, setLookupStatus] = useState("idle");
  const [nfcCode,      setNfcCode]      = useState(null);
  const [driverData,   setDriverData]   = useState(null);
  const [streamError,  setStreamError]  = useState(null);
  const [lookupError,  setLookupError]  = useState(null);
  const [debugLog,     setDebugLog]     = useState([]);
  const [showDebug,    setShowDebug]    = useState(false);

  const esRef       = useRef(null);
  const lookupRef   = useRef(false);
  const lastCodeRef = useRef(null);

  const dbg = useCallback((msg, data) => {
    const line = `[${new Date().toLocaleTimeString()}] ${msg}${data !== undefined ? " → " + JSON.stringify(data) : ""}`;
    console.log("💳", line);
    setDebugLog(p => [line, ...p].slice(0, 30));
  }, []);

  useEffect(() => {
    if (!visible) return;
    setStreamStatus("connecting"); setLookupStatus("idle");
    setNfcCode(null); setDriverData(null);
    setStreamError(null); setLookupError(null);
    lookupRef.current = false; lastCodeRef.current = null;

    try {
      const es = new EventSource(nfcStreamUrl);
      esRef.current = es;
      es.onopen = () => { setStreamStatus("listening"); dbg("NFC stream connected ✓"); };
      es.onmessage = (event) => {
        const code = parseNfcCode(event.data);
        if (!code) return;
        lastCodeRef.current = code;
        lookupRef.current = false;
        setNfcCode(code);
        setStreamStatus("code_detected");
      };
      es.onerror = () => {
        setStreamError(`Cannot connect to NFC reader at ${nfcStreamUrl}`);
        setStreamStatus("error");
        es.close(); esRef.current = null;
      };
    } catch {
      setStreamError("Failed to open NFC stream.");
      setStreamStatus("error");
    }

    return () => { if (esRef.current) { esRef.current.close(); esRef.current = null; } };
  }, [visible, nfcStreamUrl, dbg]);

  useEffect(() => {
    if (!nfcCode || lookupRef.current) return;
    lookupRef.current = true;

    (async () => {
      setLookupStatus("loading");
      setDriverData(null);
      setLookupError(null);

      try {
        const raw = await getDriverByNfc(nfcCode);
        const d   = extractDriver(raw);
        if (!d) { setLookupStatus("not_found"); return; }
        const normalised = normaliseDriver(d, nfcCode);
        setDriverData(normalised);
        setLookupStatus("found");
        setTimeout(() => onSelectNFC(normalised), 2000);
      } catch (err) {
        const notFound = err.message?.includes("404") || /not found|no driver/i.test(err.message ?? "");
        if (notFound) setLookupStatus("not_found");
        else { setLookupStatus("error"); setLookupError(err.message || "Server error."); }
      }
    })();
  }, [nfcCode, dbg, onSelectNFC]);

  const handleRetry = () => {
    const code = lastCodeRef.current;
    if (!code) return;
    lookupRef.current = false;
    setNfcCode(null);
    setTimeout(() => setNfcCode(code), 50);
  };

  const handleReconnect = () => {
    if (esRef.current) esRef.current.close();
    setStreamStatus("connecting"); setLookupStatus("idle");
    setNfcCode(null); setDriverData(null);
    setStreamError(null); setLookupError(null);
  };

  const isListening  = streamStatus === "connecting" || streamStatus === "listening";
  const streamFailed = streamStatus === "error";

  // ── Colours ─────────────────────────────────────────────────────────────────
  const bg   = isDark ? "#111827" : "#ffffff";
  const bdr  = isDark ? "#1f2937" : "#e5e7eb";
  const txt  = isDark ? "#f9fafb" : "#111827";
  const muted= isDark ? "#6b7280" : "#9ca3af";
  const sub  = isDark ? "#1f2937" : "#f9fafb";

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width={640}
      centered
      destroyOnClose
      styles={{ content: { padding: 0, borderRadius: 20, overflow: "hidden", background: bg } }}
    >
      <div style={{ background: bg, fontFamily: "'Inter', system-ui, sans-serif" }}>

        {/* Header */}
        <div style={{ padding: "20px 24px", borderBottom: `1px solid ${bdr}`, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            <div style={{ width: 44, height: 44, borderRadius: 12, background: "linear-gradient(135deg,#d97706,#b45309)", display: "flex", alignItems: "center", justifyContent: "center", boxShadow: "0 4px 12px rgba(180,83,9,0.3)", fontSize: 20 }}>
              💳
            </div>
            <div>
              <h3 style={{ fontSize: 17, fontWeight: 800, color: txt, margin: 0 }}>Driver Authentication</h3>
              <p style={{ fontSize: 12, color: muted, margin: 0 }}>Tap your NFC card on the reader</p>
            </div>
          </div>

          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            {/* Status pill */}
            <div style={{
              display: "flex", alignItems: "center", gap: 6, padding: "5px 12px", borderRadius: 20,
              fontSize: 11, fontWeight: 700,
              background: streamFailed ? "#fef2f2" : streamStatus === "code_detected" ? "#fff7ed" : "#fff7ed",
              border: `1px solid ${streamFailed ? "#fca5a5" : "#fcd34d"}`,
              color: streamFailed ? "#dc2626" : "#92400e",
            }}>
              <span style={{ width: 7, height: 7, borderRadius: "50%", background: streamFailed ? "#ef4444" : "#d97706", display: "inline-block", animation: streamFailed ? "none" : "pulse 1.5s ease-in-out infinite" }} />
              {streamStatus === "connecting"    && "Connecting…"}
              {streamStatus === "listening"     && "Listening"}
              {streamStatus === "code_detected" && nfcCode}
              {streamStatus === "error"         && "Error"}
            </div>
            <button onClick={() => setShowDebug(v => !v)} style={{ padding: "5px 10px", borderRadius: 8, fontSize: 10, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: muted, cursor: "pointer" }}>
              {showDebug ? "Hide Log" : "Debug"}
            </button>
          </div>
        </div>

        {/* Body */}
        <div style={{ padding: 28 }}>

          {/* Stream error */}
          {streamFailed && (
            <div style={{ textAlign: "center", padding: "40px 0" }}>
              <div style={{ width: 72, height: 72, borderRadius: "50%", background: "#fef2f2", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 16px", fontSize: 28 }}>⚠️</div>
              <h4 style={{ fontSize: 18, fontWeight: 800, color: txt, marginBottom: 6 }}>NFC Stream Unavailable</h4>
              <p style={{ fontSize: 13, color: muted, marginBottom: 4 }}>{streamError}</p>
              <p style={{ fontFamily: "monospace", fontSize: 11, color: muted, marginBottom: 20 }}>{nfcStreamUrl}</p>
              <button onClick={handleReconnect} style={{ padding: "10px 20px", borderRadius: 10, fontSize: 13, fontWeight: 700, color: "#fff", background: "#d97706", border: "none", cursor: "pointer" }}>
                🔄 Reconnect
              </button>
            </div>
          )}

          {/* Waiting for tap */}
          {isListening && lookupStatus === "idle" && (
            <div style={{ textAlign: "center", padding: "40px 0" }}>
              <div style={{ position: "relative", width: 120, height: 120, margin: "0 auto 24px", display: "flex", alignItems: "center", justifyContent: "center" }}>
                {[0, 1, 2].map(i => (
                  <span key={i} style={{
                    position: "absolute", inset: 0, borderRadius: "50%",
                    border: "2px solid rgba(217,119,6,0.25)",
                    animation: `ping 2s ease-out ${i * 0.5}s infinite`,
                  }} />
                ))}
                <div style={{ width: 80, height: 80, borderRadius: "50%", border: "3px dashed #d97706", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 40, animation: "pulse 2s ease-in-out infinite" }}>
                  📡
                </div>
              </div>
              <h4 style={{ fontSize: 22, fontWeight: 800, color: txt, marginBottom: 8 }}>Tap Your NFC Card</h4>
              <p style={{ fontSize: 14, color: muted, marginBottom: 20 }}>Hold your card flat against the NFC reader</p>
              <div style={{ maxWidth: 380, margin: "0 auto", padding: "12px 16px", borderRadius: 12, background: isDark ? "#1f2937" : "#fff7ed", border: `1px solid ${isDark ? "#374151" : "#fcd34d"}` }}>
                <p style={{ fontSize: 12, fontWeight: 600, color: isDark ? "#fcd34d" : "#92400e", margin: 0 }}>
                  💡 Make sure your card is flat against the reader
                </p>
              </div>
            </div>
          )}

          {/* Loading driver */}
          {lookupStatus === "loading" && (
            <div style={{ textAlign: "center", padding: "40px 0" }}>
              <div style={{ width: 72, height: 72, borderRadius: "50%", background: "#fff7ed", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 16px" }}>
                <div style={{ width: 36, height: 36, border: "3px solid #fcd34d", borderTopColor: "#d97706", borderRadius: "50%", animation: "spin 0.8s linear infinite" }} />
              </div>
              <h4 style={{ fontSize: 18, fontWeight: 800, color: txt, marginBottom: 6 }}>Authenticating...</h4>
              <p style={{ fontSize: 13, color: muted }}>
                Looking up driver for NFC: <span style={{ fontFamily: "monospace", color: "#d97706" }}>{nfcCode}</span>
              </p>
            </div>
          )}

          {/* Driver authenticated */}
          {lookupStatus === "found" && driverData && (
            <div style={{ padding: "8px 0" }}>
              <div style={{ textAlign: "center", marginBottom: 24 }}>
                <div style={{ width: 84, height: 84, borderRadius: "50%", background: "linear-gradient(135deg,#d97706,#b45309)", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 12px", boxShadow: "0 8px 28px rgba(180,83,9,0.35)", fontSize: 40 }}>
                  ✓
                </div>
                <h4 style={{ fontSize: 20, fontWeight: 900, color: "#d97706", marginBottom: 4 }}>Authenticated!</h4>
                <p style={{ fontSize: 13, color: muted }}>Driver verified · Proceeding to weighing…</p>
              </div>

              <div style={{ borderRadius: 16, border: `1.5px solid #fcd34d`, background: isDark ? "#1f2937" : "#fffbeb", padding: 20 }}>
                <div style={{ display: "flex", alignItems: "center", gap: 14, marginBottom: 16 }}>
                  <div style={{ width: 56, height: 56, borderRadius: "50%", background: "linear-gradient(135deg,#374151,#111827)", display: "flex", alignItems: "center", justifyContent: "center", fontSize: 26, flexShrink: 0 }}>👤</div>
                  <div>
                    <h5 style={{ fontSize: 18, fontWeight: 800, color: txt, margin: 0 }}>{driverData.name}</h5>
                    {driverData.employeeId && <p style={{ fontSize: 12, fontFamily: "monospace", color: muted, margin: 0 }}>ID: {driverData.employeeId}</p>}
                  </div>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10 }}>
                  {[
                    { label: "NFC UID", value: driverData.uid,       mono: true  },
                    { label: "Phone",   value: driverData.phone,     mono: false },
                    { label: "License", value: driverData.licenseNo, mono: true  },
                    { label: "Status",  value: driverData.status,    badge: driverData.status === "Active" },
                  ].filter(f => f.value).map(f => (
                    <div key={f.label} style={{ padding: "10px 12px", borderRadius: 10, background: isDark ? "#111827" : "#fff", border: `1px solid ${bdr}` }}>
                      <p style={{ fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.08em", color: muted, marginBottom: 3 }}>{f.label}</p>
                      {f.badge ? (
                        <span style={{ padding: "2px 8px", borderRadius: 20, fontSize: 11, fontWeight: 700, background: "#fff7ed", color: "#d97706", border: "1px solid #fcd34d" }}>✓ {f.value}</span>
                      ) : (
                        <p style={{ fontSize: 13, fontWeight: 600, fontFamily: f.mono ? "monospace" : "inherit", color: txt, margin: 0 }}>{f.value}</p>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Not found */}
          {lookupStatus === "not_found" && (
            <div style={{ textAlign: "center", padding: "40px 0" }}>
              <div style={{ width: 72, height: 72, borderRadius: "50%", background: "#fff7ed", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 16px", fontSize: 28 }}>❓</div>
              <h4 style={{ fontSize: 18, fontWeight: 800, color: txt, marginBottom: 6 }}>Driver Not Found</h4>
              <p style={{ fontSize: 13, color: muted, marginBottom: 20 }}>
                No driver registered with NFC: <span style={{ fontFamily: "monospace", color: "#d97706" }}>{nfcCode}</span>
              </p>
              <button onClick={handleRetry} style={{ padding: "10px 20px", borderRadius: 10, fontSize: 13, fontWeight: 700, color: "#fff", background: "#d97706", border: "none", cursor: "pointer" }}>
                Try Again
              </button>
            </div>
          )}

          {/* Lookup error */}
          {lookupStatus === "error" && (
            <div style={{ textAlign: "center", padding: "40px 0" }}>
              <div style={{ width: 72, height: 72, borderRadius: "50%", background: "#fef2f2", display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 16px", fontSize: 28 }}>⚠️</div>
              <h4 style={{ fontSize: 18, fontWeight: 800, color: txt, marginBottom: 6 }}>Lookup Failed</h4>
              <p style={{ fontSize: 13, color: muted, marginBottom: 4 }}>Could not fetch driver from server</p>
              <p style={{ fontFamily: "monospace", fontSize: 11, padding: "6px 12px", borderRadius: 8, display: "inline-block", background: "#fef2f2", color: "#dc2626", border: "1px solid #fecaca", marginBottom: 20 }}>{lookupError}</p>
              <div>
                <button onClick={handleRetry} style={{ padding: "10px 20px", borderRadius: 10, fontSize: 13, fontWeight: 700, color: "#fff", background: "#ef4444", border: "none", cursor: "pointer" }}>
                  Retry
                </button>
              </div>
            </div>
          )}

          {/* Debug log */}
          {showDebug && (
            <div style={{ marginTop: 20, borderRadius: 12, border: "1px solid #1f2937", background: "#030712", padding: 16, fontFamily: "monospace", fontSize: 10, color: "#34d399" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
                <span style={{ fontFamily: "system-ui", fontWeight: 700, fontSize: 9, textTransform: "uppercase", letterSpacing: "0.1em", color: "#4b5563" }}>NFC Debug Log</span>
                <button onClick={() => setDebugLog([])} style={{ fontSize: 9, color: "#4b5563", background: "none", border: "none", cursor: "pointer" }}>Clear</button>
              </div>
              {debugLog.length === 0
                ? <p style={{ color: "#374151" }}>No messages yet…</p>
                : <div style={{ maxHeight: 140, overflowY: "auto" }}>{debugLog.map((e, i) => <p key={i} style={{ margin: "2px 0", wordBreak: "break-all" }}>{e}</p>)}</div>
              }
            </div>
          )}
        </div>

        {/* Footer */}
        <div style={{ padding: "12px 24px", borderTop: `1px solid ${bdr}`, background: sub }}>
          <p style={{ fontSize: 11, textAlign: "center", color: muted, margin: 0 }}>
            NFC stream: <span style={{ fontFamily: "monospace" }}>{nfcStreamUrl}</span>
          </p>
        </div>
      </div>

      <style>{`
        @keyframes spin  { to { transform: rotate(360deg); } }
        @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:0.45; } }
        @keyframes ping  { 0% { transform: scale(1); opacity:0.6; } 100% { transform: scale(2.2); opacity:0; } }
      `}</style>
    </Modal>
  );
}