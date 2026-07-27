/**
 * AuthenticationMethodModal.jsx — REAL NFC MODE
 *
 * Changes from original:
 *  - NFC stream URL now comes from SystemSettings via useHardwareConfig()
 *    (no hardcoded URL — updates live when System Settings are saved)
 *  - Stream reconnects automatically if the URL changes while modal is open
 *  - Styled to match the amber / Qalitrack brand theme
 */

import React, { useEffect, useRef, useState, useCallback } from "react";
import { Modal, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import { getDriverByNfc } from "../../api/MasterData/Drivers";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import logo from "../../assets/qalitrack_logo_full.png";

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
  const [pulsePhase,   setPulsePhase]   = useState(0);

  const esRef       = useRef(null);
  const lookupRef   = useRef(false);
  const lastCodeRef = useRef(null);

  const dbg = useCallback((msg, data) => {
    const line = `[${new Date().toLocaleTimeString()}] ${msg}${data !== undefined ? " → " + JSON.stringify(data) : ""}`;
    setDebugLog(p => [line, ...p].slice(0, 30));
  }, []);

  useEffect(() => {
    const id = setInterval(() => setPulsePhase(p => (p + 1) % 3), 700);
    return () => clearInterval(id);
  }, []);

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
  }, [visible, nfcStreamUrl, dbg]);

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
      width={560}
      centered
      className={isDark ? "dark-modal" : ""}
      destroyOnClose
    >
      <div className={`${isDark ? "bg-gray-900 text-white" : "bg-white"} rounded-xl overflow-hidden`}>

        {/* Header */}
        <div className={`px-6 py-4 border-b ${isDark ? "border-gray-800 bg-gray-900" : "border-gray-100 bg-white"}`}>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <img src={logo} alt="Qalitrack" className="h-12 w-auto" />
              <div className={`w-px h-7 ${isDark ? "bg-gray-700" : "bg-gray-200"}`} />
              <div>
                <h3 className={`text-base font-black ${isDark ? "text-white" : "text-gray-900"}`}>
                  Driver Authentication
                </h3>
                <p className={`text-xs ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                  Tap your NFC card on the reader
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <div className={`flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold border ${
                streamFailed
                  ? "bg-red-50 border-red-200 text-red-600"
                  : streamStatus === "code_detected"
                  ? "bg-amber-50 border-amber-200 text-amber-700"
                  : "bg-green-50 border-green-200 text-green-700"
              }`}>
                <span className={`w-1.5 h-1.5 rounded-full ${
                  streamFailed ? "bg-red-500" : streamStatus === "code_detected" ? "bg-amber-500" : "bg-green-500 animate-pulse"
                }`} />
                {streamStatus === "connecting"    && "Connecting…"}
                {streamStatus === "listening"     && "Listening"}
                {streamStatus === "code_detected" && nfcCode}
                {streamStatus === "error"         && "Error"}
              </div>
              <button
                onClick={() => setShowDebug(v => !v)}
                className={`px-2.5 py-1 rounded-full text-xs font-semibold border ${isDark ? "border-gray-700 text-gray-500" : "border-gray-200 text-gray-400"}`}>
                {showDebug ? "Hide" : "Debug"}
              </button>
            </div>
          </div>
        </div>

        {/* Main content */}
        <div className="p-6">

          {/* Stream error */}
          {streamFailed && (
            <div className="text-center py-10">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-red-100 flex items-center justify-center">
                <svg className="w-10 h-10 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </div>
              <h4 className={`text-xl font-bold mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>NFC Stream Unavailable</h4>
              <p className={`text-sm mb-1 ${isDark ? "text-gray-400" : "text-gray-500"}`}>{streamError}</p>
              <p className="text-xs font-mono mb-5 text-gray-400">{nfcStreamUrl}</p>
              <button onClick={handleReconnect}
                className="px-5 py-2 rounded-xl text-sm font-bold text-white bg-gradient-to-r from-amber-500 to-amber-500 shadow-md hover:shadow-lg transition-all">
                Reconnect
              </button>
            </div>
          )}

          {/* Waiting for tap */}
          {isListening && lookupStatus === "idle" && (
            <div className="text-center py-8">
              <div className="relative w-36 h-36 mx-auto mb-6 flex items-center justify-center">
                {[0, 1, 2].map(i => {
                  const ph = (pulsePhase + i) % 3;
                  return (
                    <span key={i} className="absolute inset-0 rounded-full border-2 transition-all duration-700"
                      style={{ borderColor: "rgba(217,119,6,0.25)", transform: `scale(${1 + (ph / 3) * 0.6})`, opacity: 1 - (ph / 3) * 0.85 }} />
                  );
                })}
                <div className="w-24 h-24 rounded-full bg-gradient-to-br from-amber-400 to-amber-500 flex items-center justify-center shadow-2xl">
                  <svg className="w-12 h-12 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z" />
                  </svg>
                </div>
              </div>
              <h4 className={`text-2xl font-black mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>
                Tap Your NFC Card
              </h4>
              <p className={`text-sm mb-5 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                Hold your card flat against the reader
              </p>
              <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-semibold ${isDark ? "bg-gray-800 text-amber-400" : "bg-amber-50 text-amber-700 border border-amber-200"}`}>
                <span className="w-2 h-2 rounded-full bg-amber-500 animate-pulse" />
                Reader is active and listening
              </div>
            </div>
          )}

          {/* Looking up */}
          {lookupStatus === "loading" && (
            <div className="text-center py-10">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-gradient-to-br from-amber-400 to-amber-500 flex items-center justify-center shadow-xl">
                <div className="w-10 h-10 border-4 border-white/30 border-t-white rounded-full animate-spin" />
              </div>
              <h4 className={`text-xl font-bold mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>Authenticating…</h4>
              <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                Looking up driver · <span className="font-mono">{nfcCode}</span>
              </p>
            </div>
          )}

          {/* Driver authenticated */}
          {lookupStatus === "found" && driverData && (
            <div className="py-4">
              <div className="text-center mb-5">
                <div className="w-20 h-20 mx-auto mb-3 rounded-full bg-gradient-to-br from-green-400 to-emerald-600 flex items-center justify-center shadow-xl">
                  <svg className="w-11 h-11 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <h4 className="text-2xl font-black text-green-500 mb-1">Authenticated!</h4>
                <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                  Proceeding to weighing…
                </p>
              </div>

              <div className={`rounded-2xl border-2 p-5 ${isDark ? "bg-gray-800 border-green-800" : "bg-green-50 border-green-200"}`}>
                <div className="flex items-center gap-4 mb-4">
                  <div className="w-14 h-14 rounded-full bg-gradient-to-br from-amber-400 to-amber-500 flex items-center justify-center flex-shrink-0 shadow-lg">
                    <svg className="w-7 h-7 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
                    </svg>
                  </div>
                  <div>
                    <h5 className={`text-lg font-black ${isDark ? "text-white" : "text-gray-900"}`}>{driverData.name}</h5>
                    {driverData.employeeId && (
                      <p className={`text-xs font-mono ${isDark ? "text-gray-400" : "text-gray-500"}`}>ID: {driverData.employeeId}</p>
                    )}
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  {[
                    { label: "NFC UID", value: driverData.uid,       mono: true },
                    { label: "Phone",   value: driverData.phone,     mono: false },
                    { label: "License", value: driverData.licenseNo, mono: true },
                    { label: "Status",  value: driverData.status,    badge: driverData.status === "Active" },
                  ].filter(f => f.value).map(f => (
                    <div key={f.label} className={`p-3 rounded-xl ${isDark ? "bg-gray-900" : "bg-white border border-green-100"}`}>
                      <p className={`text-xs font-bold uppercase tracking-wider mb-1 ${isDark ? "text-gray-500" : "text-gray-400"}`}>{f.label}</p>
                      {f.badge ? (
                        <span className="px-2 py-0.5 rounded-full text-xs font-bold bg-green-100 text-green-700">✓ {f.value}</span>
                      ) : (
                        <p className={`text-sm font-semibold truncate ${f.mono ? "font-mono" : ""} ${isDark ? "text-white" : "text-gray-900"}`}>
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
            <div className="text-center py-10">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-amber-100 flex items-center justify-center">
                <svg className="w-10 h-10 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <h4 className={`text-xl font-bold mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>Driver Not Found</h4>
              <p className={`text-sm mb-1 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                No driver linked to NFC:
              </p>
              <p className="font-mono text-sm mb-5 px-3 py-1.5 rounded-lg inline-block bg-amber-50 text-amber-700 border border-amber-200">{nfcCode}</p>
              <br />
              <button onClick={handleRetry}
                className="px-5 py-2 rounded-xl text-sm font-bold text-white bg-gradient-to-r from-amber-500 to-amber-500 shadow-md hover:shadow-lg transition-all">
                Try Again
              </button>
            </div>
          )}

          {/* Lookup error */}
          {lookupStatus === "error" && (
            <div className="text-center py-10">
              <div className="w-20 h-20 mx-auto mb-4 rounded-full bg-red-100 flex items-center justify-center">
                <svg className="w-10 h-10 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
                </svg>
              </div>
              <h4 className={`text-xl font-bold mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>Lookup Failed</h4>
              <p className={`text-sm mb-1 ${isDark ? "text-gray-400" : "text-gray-500"}`}>Could not fetch driver from server</p>
              <p className="font-mono text-xs mb-5 px-3 py-2 rounded-lg inline-block bg-red-50 text-red-600 border border-red-200">{lookupError}</p>
              <br />
              <button onClick={handleRetry}
                className="px-5 py-2 rounded-xl text-sm font-bold text-white bg-red-500 hover:bg-red-600 transition-colors">
                Retry
              </button>
            </div>
          )}

          {/* Debug log */}
          {showDebug && (
            <div className="mt-4 rounded-xl border border-gray-800 bg-gray-950 p-4 font-mono text-xs text-green-400">
              <div className="flex items-center justify-between mb-2">
                <span className="font-sans font-bold text-xs uppercase tracking-wider text-gray-500">NFC Debug Log</span>
                <button onClick={() => setDebugLog([])} className="text-gray-600 hover:text-gray-400 text-xs">Clear</button>
              </div>
              {debugLog.length === 0 ? (
                <p className="text-gray-600">No messages yet…</p>
              ) : (
                <div className="space-y-1 max-h-36 overflow-y-auto">
                  {debugLog.map((e, i) => <p key={i} className="break-all leading-relaxed">{e}</p>)}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className={`px-6 py-3 border-t ${isDark ? "border-gray-800" : "border-gray-100"}`}>
          <p className={`text-xs text-center ${isDark ? "text-gray-600" : "text-gray-400"}`}>
            NFC: <span className="font-mono">{nfcStreamUrl}</span>
            {" · "}Powered by <span className="font-black text-amber-500">QALIBRATED SYSTEMS</span>
          </p>
        </div>
      </div>
    </Modal>
  );
}
