/**
 * AuthenticationMethodModal.jsx — REAL NFC MODE
 *
 * Changes from original:
 *  - NFC stream URL now comes from SystemSettings via useHardwareConfig()
 *    (no hardcoded URL — updates live when System Settings are saved)
 *  - Stream reconnects automatically if the URL changes while modal is open
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
  id:           d.id,
  uid:          d.nfCcode ?? nfcCode,
  name:         d.fullName,
  employeeId:   d.employeeId    ?? null,
  phone:        d.phone         ?? null,
  email:        d.email         ?? null,
  licenseNo:    d.licenseNumber ?? null,
  licenseExpiry: d.licenseExpiryDate ?? null,
  status:       d.status        ?? "Active",
  transporterId:  d.transporterId  ?? null,
  supplierId:     d.supplierId     ?? null,
  assignedVehicleIds: d.assignedVehicleIds ?? [],
  detectedAt:   new Date(),
});

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function AuthenticationMethodModal({ visible, onClose, onSelectNFC }) {
  const { isDark } = useTheme();

  // ── Live NFC URL from SystemSettings ─────────────────────────────────────
  const hwConfig = useHardwareConfig();
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

  // ── Connect / reconnect when modal opens OR nfcStreamUrl changes ──────────
  useEffect(() => {
    if (!visible) return;

    setStreamStatus("connecting");
    setLookupStatus("idle");
    setNfcCode(null);
    setDriverData(null);
    setStreamError(null);
    setLookupError(null);
    lookupRef.current = false;
    lastCodeRef.current = null;

    dbg("Opening NFC stream", nfcStreamUrl);

    try {
      const es = new EventSource(nfcStreamUrl);
      esRef.current = es;

      es.onopen = () => { setStreamStatus("listening"); dbg("NFC stream connected ✓"); };

      es.onmessage = (event) => {
        dbg("SSE raw", event.data);
        const code = parseNfcCode(event.data);
        if (!code) { dbg("Skipped (control msg)"); return; }
        dbg("NFC code detected", code);
        lastCodeRef.current = code;
        lookupRef.current = false;
        setNfcCode(code);
        setStreamStatus("code_detected");
      };

      es.onerror = () => {
        setStreamError(`Cannot connect to NFC reader at ${nfcStreamUrl}`);
        setStreamStatus("error");
        es.close();
        esRef.current = null;
      };
    } catch {
      setStreamError("Failed to open NFC stream.");
      setStreamStatus("error");
    }

    return () => {
      if (esRef.current) { dbg("Closing NFC stream"); esRef.current.close(); esRef.current = null; }
    };
  // Re-run if nfcStreamUrl changes while modal is open
  }, [visible, nfcStreamUrl, dbg]);

  // ── Driver lookup ─────────────────────────────────────────────────────────
  useEffect(() => {
    if (!nfcCode || lookupRef.current) return;
    lookupRef.current = true;

    (async () => {
      setLookupStatus("loading");
      setDriverData(null);
      setLookupError(null);
      dbg("Fetching driver", nfcCode);

      try {
        const raw = await getDriverByNfc(nfcCode);
        dbg("Raw response", raw);
        const d = extractDriver(raw);
        if (!d) { dbg("No driver found"); setLookupStatus("not_found"); return; }

        const normalised = normaliseDriver(d, nfcCode);
        dbg("Driver authenticated", normalised.name);
        setDriverData(normalised);
        setLookupStatus("found");

        setTimeout(() => onSelectNFC(normalised), 2000);

      } catch (err) {
        dbg("Lookup error", err.message);
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
    setStreamStatus("connecting");
    setLookupStatus("idle");
    setNfcCode(null);
    setDriverData(null);
    setStreamError(null);
    setLookupError(null);
  };

  const isListening  = streamStatus === "connecting" || streamStatus === "listening";
  const streamFailed = streamStatus === "error";

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width={700}
      centered
      className={isDark ? "dark-modal" : ""}
      destroyOnClose
    >
      <div className={`${isDark ? "bg-gray-900 text-white" : "bg-white"}`}>

        {/* Header */}
        <div className={`px-6 py-4 border-b ${isDark ? "border-gray-700" : "border-gray-200"}`}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500 to-indigo-600 flex items-center justify-center shadow-lg">
                <span className="text-white text-xl">💳</span>
              </div>
              <div>
                <h3 className={`text-lg font-bold ${isDark ? "text-white" : "text-gray-900"}`}>
                  Driver Authentication
                </h3>
                <p className={`text-xs ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                  Tap your NFC card on the reader
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold border ${
                streamFailed
                  ? "bg-red-50 border-red-200 text-red-600"
                  : streamStatus === "code_detected"
                  ? "bg-purple-50 border-purple-200 text-purple-700"
                  : "bg-green-50 border-green-200 text-green-700"
              }`}>
                <span className={`w-1.5 h-1.5 rounded-full ${
                  streamFailed ? "bg-red-500" : streamStatus === "code_detected" ? "bg-purple-500" : "bg-green-500 animate-pulse"
                }`} />
                {streamStatus === "connecting"    && "Connecting…"}
                {streamStatus === "listening"     && "Listening"}
                {streamStatus === "code_detected" && nfcCode}
                {streamStatus === "error"         && "Error"}
              </div>
              <button
                onClick={() => setShowDebug(v => !v)}
                className={`px-3 py-1.5 rounded-full text-xs font-semibold border ${isDark ? "border-gray-700 text-gray-500" : "border-gray-300 text-gray-400"}`}>
                {showDebug ? "Hide Log" : "Debug"}
              </button>
            </div>
          </div>
        </div>

        {/* Main content */}
        <div className="p-6">

          {/* Stream error */}
          {streamFailed && (
            <div className="text-center py-12">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-red-100 flex items-center justify-center">
                <span className="text-3xl">⚠️</span>
              </div>
              <h4 className={`text-xl font-bold mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>NFC Stream Unavailable</h4>
              <p className={`text-sm mb-1 ${isDark ? "text-gray-400" : "text-gray-500"}`}>{streamError}</p>
              <p className="text-xs font-mono mb-4 text-gray-400">{nfcStreamUrl}</p>
              <Button onClick={handleReconnect} className="bg-purple-500 hover:bg-purple-600 text-white border-0 font-semibold">
                🔄 Reconnect
              </Button>
            </div>
          )}

          {/* Waiting for tap */}
          {isListening && lookupStatus === "idle" && (
            <div className="text-center py-12">
              <div className="relative w-32 h-32 mx-auto mb-6">
                {[0, 1, 2].map(i => (
                  <span key={i} className="absolute inset-0 rounded-full border-2 border-purple-500 animate-ping"
                    style={{ animationDelay: `${i * 0.3}s`, opacity: 0.3 }} />
                ))}
                <div className="absolute inset-0 rounded-full border-4 border-dashed border-purple-500 flex items-center justify-center animate-pulse">
                  <span className="text-6xl">📡</span>
                </div>
              </div>
              <h4 className={`text-2xl font-bold mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Tap Your NFC Card</h4>
              <p className={`text-sm mb-6 ${isDark ? "text-gray-400" : "text-gray-500"}`}>Hold your card against the NFC reader</p>
              <div className={`max-w-md mx-auto p-4 rounded-lg ${isDark ? "bg-gray-800" : "bg-blue-50"}`}>
                <p className={`text-xs font-semibold ${isDark ? "text-blue-300" : "text-blue-800"}`}>
                  💡 Make sure your card is flat against the reader
                </p>
              </div>
            </div>
          )}

          {/* Loading driver */}
          {lookupStatus === "loading" && (
            <div className="text-center py-12">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-purple-100 flex items-center justify-center">
                <div className="w-10 h-10 border-4 border-purple-500 border-t-transparent rounded-full animate-spin" />
              </div>
              <h4 className={`text-xl font-bold mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Authenticating...</h4>
              <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                Looking up driver from NFC: <span className="font-mono">{nfcCode}</span>
              </p>
            </div>
          )}

          {/* Driver authenticated */}
          {lookupStatus === "found" && driverData && (
            <div className="py-6">
              <div className="text-center mb-6">
                <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gradient-to-br from-green-400 to-emerald-600 flex items-center justify-center shadow-2xl">
                  <span className="text-5xl">✓</span>
                </div>
                <h4 className="text-2xl font-black text-green-500 mb-2">Authenticated!</h4>
                <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                  Driver verified · Proceeding to weighing...
                </p>
              </div>

              <div className={`rounded-2xl border-2 p-6 ${isDark ? "bg-gray-800 border-green-700" : "bg-gradient-to-br from-green-50 to-emerald-50 border-green-200"}`}>
                <div className="flex items-center gap-4 mb-4">
                  <div className="w-16 h-16 rounded-full bg-gradient-to-br from-blue-400 to-indigo-600 flex items-center justify-center text-3xl flex-shrink-0">
                    👤
                  </div>
                  <div>
                    <h5 className={`text-xl font-bold ${isDark ? "text-white" : "text-gray-900"}`}>{driverData.name}</h5>
                    {driverData.employeeId && (
                      <p className={`text-sm font-mono ${isDark ? "text-gray-400" : "text-gray-600"}`}>ID: {driverData.employeeId}</p>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-3">
                  {[
                    { label: "NFC UID", value: driverData.uid,       mono: true },
                    { label: "Phone",   value: driverData.phone,     mono: false },
                    { label: "License", value: driverData.licenseNo, mono: true },
                    { label: "Status",  value: driverData.status,    badge: driverData.status === "Active" },
                  ].filter(f => f.value).map(f => (
                    <div key={f.label} className={`p-3 rounded-xl ${isDark ? "bg-gray-900" : "bg-white border border-gray-100"}`}>
                      <p className={`text-xs font-bold uppercase mb-1 ${isDark ? "text-gray-500" : "text-gray-400"}`}>{f.label}</p>
                      {f.badge ? (
                        <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-green-100 text-green-700">✓ {f.value}</span>
                      ) : (
                        <p className={`text-sm font-semibold ${f.mono ? "font-mono" : ""} ${isDark ? "text-white" : "text-gray-900"}`}>
                          {f.value}
                        </p>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Not found */}
          {lookupStatus === "not_found" && (
            <div className="text-center py-12">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-amber-100 flex items-center justify-center">
                <span className="text-3xl">❓</span>
              </div>
              <h4 className={`text-xl font-bold mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Driver Not Found</h4>
              <p className={`text-sm mb-4 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                No driver registered with NFC: <span className="font-mono">{nfcCode}</span>
              </p>
              <Button onClick={handleRetry} className="bg-amber-500 hover:bg-amber-600 text-white border-0 font-semibold">Try Again</Button>
            </div>
          )}

          {/* Lookup error */}
          {lookupStatus === "error" && (
            <div className="text-center py-12">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-red-100 flex items-center justify-center">
                <span className="text-3xl">⚠️</span>
              </div>
              <h4 className={`text-xl font-bold mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Lookup Failed</h4>
              <p className={`text-sm mb-1 ${isDark ? "text-gray-400" : "text-gray-500"}`}>Could not fetch driver from server</p>
              <p className="font-mono text-xs mb-4 px-3 py-2 rounded-lg inline-block bg-red-50 text-red-600">{lookupError}</p>
              <Button onClick={handleRetry} className="bg-red-500 hover:bg-red-600 text-white border-0 font-semibold">Retry</Button>
            </div>
          )}

          {/* Debug log */}
          {showDebug && (
            <div className="mt-6 rounded-xl border border-gray-800 bg-gray-950 p-4 font-mono text-xs text-green-400">
              <div className="flex items-center justify-between mb-2">
                <span className="font-sans font-bold text-xs uppercase tracking-wider text-gray-500">NFC Debug Log</span>
                <button onClick={() => setDebugLog([])} className="text-gray-600 hover:text-gray-400 text-xs">Clear</button>
              </div>
              {debugLog.length === 0 ? (
                <p className="text-gray-600">No messages yet…</p>
              ) : (
                <div className="space-y-1 max-h-40 overflow-y-auto">
                  {debugLog.map((e, i) => <p key={i} className="break-all leading-relaxed">{e}</p>)}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className={`px-6 py-3 border-t ${isDark ? "border-gray-700" : "border-gray-200"}`}>
          <p className={`text-xs text-center ${isDark ? "text-gray-500" : "text-gray-400"}`}>
            NFC stream: <span className="font-mono">{nfcStreamUrl}</span>
          </p>
        </div>
      </div>
    </Modal>
  );
}