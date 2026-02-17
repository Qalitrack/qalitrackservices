/**
 * VehicleDetectionScreen.jsx
 *
 * 1. Opens SSE stream → http://172.16.0.134:5000/api/rfid/stream
 * 2. Parses payload:  data: "110520008553A225"  (JSON-encoded string)
 * 3. Calls getVehicleByRfid(code) via existing apiClient
 * 4. Shows vehicle details
 * 5. Opens AuthenticationMethodModal for NFC driver auth
 * 6. Calls onVehicleDetected({ vehicle, driver }) when both are done
 */

import React, { useCallback, useEffect, useRef, useState } from "react";
import { Alert, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import AuthenticationMethodModal from "./AuthenticationMethodModal.jsx";
import { getVehicleByRfid } from "../../api/MasterData/Vehicles";

const RFID_STREAM_URL = "http://172.16.0.134:5000/api/rfid/stream";
const CONTROL_TYPES   = new Set(["connected", "heartbeat", "ping", "pong", "keepalive"]);

// ── Parse SSE event.data ───────────────────────────────────────────────────────
// Stream sends:  data: "110520008553A225"   (a JSON-encoded string)
// JSON.parse → the bare string "110520008553A225" — use it directly
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
    const s = String(raw).trim();
    return s || null;
  }
};

// ── Extract vehicle object from any API response shape ────────────────────────
const extractVehicle = (raw) => {
  if (!raw) return null;
  const v = raw?.data?.data?.vehicle ?? raw?.data?.vehicle ?? raw?.data?.data
         ?? raw?.vehicle ?? raw?.data ?? raw;
  if (!v || typeof v !== "object" || Array.isArray(v)) return null;
  if (Object.keys(v).length === 0) return null;
  return v;
};

// ── Normalise vehicle fields ───────────────────────────────────────────────────
// Handles flat fields AND nested objects e.g. v.transporter = { id, name }
const normaliseVehicle = (v, rfidCode) => {
  // ── Helpers: nested object OR flat string ────────────────────────────────
  const nestedId   = (obj) => obj?.id   ?? obj?.Id   ?? null;
  const nestedName = (obj) => obj?.name ?? obj?.Name ?? null;

  // ── Transporter ──────────────────────────────────────────────────────────
  const tObj = v.transporter ?? v.Transporter ?? null;
  const transporterID   = v.transporterID   ?? v.transporter_id   ?? nestedId(tObj)   ?? null;
  const transporterName = v.transporterName ?? v.transporter_name ?? nestedName(tObj)
                        ?? v.saccoName      ?? v.sacco_name        ?? v.sacco          ?? null;

  // ── Supplier ─────────────────────────────────────────────────────────────
  const sObj = v.supplier ?? v.Supplier ?? null;
  const supplierID   = v.supplierID   ?? v.supplier_id   ?? nestedId(sObj)   ?? null;
  const supplierName = v.supplierName ?? v.supplier_name ?? nestedName(sObj) ?? null;

  // ── Owner ────────────────────────────────────────────────────────────────
  const oObj = v.owner ?? v.Owner ?? null;
  const ownerName = typeof oObj === "string" ? oObj
                  : nestedName(oObj) ?? v.ownerName ?? v.owner_name ?? null;

  // ── Commodity (pre-fill if vehicle carries it) ───────────────────────────
  const cObj = v.commodity ?? v.Commodity ?? v.product ?? v.Product ?? null;
  const commodityID   = v.commodityID   ?? v.commodity_id   ?? nestedId(cObj)   ?? null;
  const commodityName = v.commodityName ?? v.commodity_name ?? nestedName(cObj) ?? null;

  // ── Log everything for debugging ─────────────────────────────────────────
  console.log("🚗 normaliseVehicle raw keys:", Object.keys(v));
  console.log("🚗 transporter:", transporterID, transporterName);
  console.log("🚗 supplier:",    supplierID,    supplierName);
  console.log("🚗 owner:",       ownerName);
  console.log("🚗 commodity:",   commodityID,   commodityName);

  return {
    id:             v.id           ?? v.vehicleId    ?? v.vehicle_id   ?? null,
    rfidTag:        rfidCode,
    plateNumber:    v.plateNumber  ?? v.plate_number ?? v.plate        ??
                    v.registration ?? v.regNumber    ?? v.numberPlate  ?? "—",
    vehicleType:    v.vehicleType  ?? v.vehicle_type ?? v.type         ?? null,
    vehicleMake:    v.vehicleMake  ?? v.make         ?? v.brand        ?? null,
    vehicleModel:   v.vehicleModel ?? v.model                          ?? null,
    capacity:       v.capacity     ?? v.maxCapacity  ?? v.max_capacity ?? null,
    saccoName:      v.saccoName    ?? v.sacco_name   ?? v.sacco        ?? null,
    saccoId:        v.saccoId      ?? v.sacco_id                       ?? null,
    status:         v.status       ?? v.vehicleStatus                  ?? null,
    isActive:       v.isActive     ?? v.active       ?? true,
    detectedAt:     new Date(),
    // Resolved relational fields
    ownerName,
    transporterID,
    transporterName,
    supplierID,
    supplierName,
    commodityID,
    commodityName,
  };
};

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function VehicleDetectionScreen({ onVehicleDetected, error: externalError, onReset }) {
  const { isDark } = useTheme();

  // stream: connecting | listening | tag_received | error
  const [streamStatus,  setStreamStatus]  = useState("connecting");
  // vehicle: idle | loading | found | not_found | error
  const [vehicleStatus, setVehicleStatus] = useState("idle");

  const [rfidCode,      setRfidCode]      = useState(null);
  const [vehicleData,   setVehicleData]   = useState(null);
  const [streamError,   setStreamError]   = useState(null);
  const [lookupError,   setLookupError]   = useState(null);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [pulsePhase,    setPulsePhase]    = useState(0);
  const [debugLog,      setDebugLog]      = useState([]);
  const [showDebug,     setShowDebug]     = useState(false);

  const esRef       = useRef(null);
  const lookupRef   = useRef(false);
  const lastCodeRef = useRef(null);

  // ── Debug logger ──────────────────────────────────────────────────────────
  const dbg = useCallback((msg, data) => {
    const line = `[${new Date().toLocaleTimeString()}] ${msg}${data !== undefined ? " → " + JSON.stringify(data) : ""}`;
    console.log("🔍", line);
    setDebugLog(p => [line, ...p].slice(0, 40));
  }, []);

  // ── Pulse animation ───────────────────────────────────────────────────────
  useEffect(() => {
    const id = setInterval(() => setPulsePhase(p => (p + 1) % 3), 800);
    return () => clearInterval(id);
  }, []);

  // ── Open SSE stream ───────────────────────────────────────────────────────
  const openStream = useCallback(() => {
    if (esRef.current) { esRef.current.close(); esRef.current = null; }
    setStreamStatus("connecting");
    setStreamError(null);
    dbg("Opening RFID stream");

    try {
      const es = new EventSource(RFID_STREAM_URL);
      esRef.current = es;

      es.onopen = () => { setStreamStatus("listening"); dbg("Stream connected ✓"); };

      es.onmessage = (event) => {
        dbg("SSE raw", event.data);
        const code = parseRfidCode(event.data);
        if (!code) { dbg("Skipped (control msg)"); return; }

        dbg("RFID code", code);
        lastCodeRef.current = code;
        lookupRef.current   = false;   // allow fresh lookup (handles rescan)
        setRfidCode(code);
        setStreamStatus("tag_received");
      };

      es.onerror = () => {
        dbg("Stream error");
        setStreamError("Cannot connect to RFID reader at " + RFID_STREAM_URL);
        setStreamStatus("error");
        es.close(); esRef.current = null;
      };
    } catch (err) {
      setStreamError("Failed to open SSE connection.");
      setStreamStatus("error");
    }
  }, [dbg]);

  useEffect(() => {
    openStream();
    return () => { if (esRef.current) esRef.current.close(); };
  }, [openStream]);

  // ── Vehicle lookup whenever rfidCode changes ──────────────────────────────
  useEffect(() => {
    if (!rfidCode || lookupRef.current) return;
    lookupRef.current = true;

    (async () => {
      setVehicleStatus("loading");
      setVehicleData(null);
      setLookupError(null);
      setShowAuthModal(false);
      dbg("Fetching vehicle", rfidCode);

      try {
        const raw = await getVehicleByRfid(rfidCode);
        dbg("Raw response", raw);
        const v = extractVehicle(raw);
        if (!v) { setVehicleStatus("not_found"); return; }
        const normalised = normaliseVehicle(v, rfidCode);
        dbg("Vehicle", normalised.plateNumber);
        setVehicleData(normalised);
        setVehicleStatus("found");
        // Auto-open NFC modal after 1.2 s so user can see the vehicle card first
        setTimeout(() => setShowAuthModal(true), 1200);
      } catch (err) {
        dbg("Lookup error", err.message);
        const msg = err.message ?? "";
        const notFound = msg.includes("404") || /not found|no vehicle|does not exist/i.test(msg);
        if (notFound) { setVehicleStatus("not_found"); }
        else          { setVehicleStatus("error"); setLookupError(msg || "Server error."); }
      }
    })();
  }, [rfidCode, dbg]);

  // ── Rescan — keep stream open, reset vehicle state ────────────────────────
  const handleRescan = () => {
    lastCodeRef.current = null;
    lookupRef.current   = false;
    setRfidCode(null);
    setVehicleData(null);
    setVehicleStatus("idle");
    setStreamStatus("listening");
    setLookupError(null);
    setShowAuthModal(false);
  };

  // ── Retry lookup without rescan ───────────────────────────────────────────
  const handleRetryLookup = () => {
    const code = lastCodeRef.current;
    if (!code) return;
    lookupRef.current = false;
    setRfidCode(null);
    setTimeout(() => setRfidCode(code), 50);
  };

  // ── NFC auth complete → pass { vehicle, driver } up to orchestrator ───────
  const handleAuthComplete = (driverData) => {
    setShowAuthModal(false);
    onVehicleDetected({ vehicle: vehicleData, driver: driverData });
  };

  // ─── Derived ──────────────────────────────────────────────────────────────
  const isListening  = streamStatus === "connecting" || streamStatus === "listening";
  const streamFailed = streamStatus === "error";

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className={`min-h-screen flex flex-col ${isDark ? "bg-gray-950" : "bg-slate-100"}`}>

      {/* Header */}
      <header className={`${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"} border-b px-8 py-5 shadow-sm`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className="w-14 h-14 bg-gradient-to-br from-amber-400 to-orange-600 rounded-2xl flex items-center justify-center shadow-lg">
              <span className="text-white text-xl font-black">KW</span>
            </div>
            <div>
              <h1 className={`text-2xl font-black ${isDark ? "text-white" : "text-gray-900"}`}>Self-Service Weighing</h1>
              <p className={`text-xs ${isDark ? "text-gray-500" : "text-gray-400"}`}>Automated tea collection — RFID verification</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            {/* Stream status */}
            <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold border ${
              streamFailed
                ? isDark ? "bg-red-900/30 border-red-700 text-red-400" : "bg-red-50 border-red-200 text-red-600"
                : streamStatus === "tag_received"
                ? isDark ? "bg-amber-900/30 border-amber-700 text-amber-400" : "bg-amber-50 border-amber-200 text-amber-700"
                : isDark ? "bg-green-900/30 border-green-700 text-green-400" : "bg-green-50 border-green-200 text-green-700"
            }`}>
              <span className={`w-1.5 h-1.5 rounded-full ${
                streamFailed ? "bg-red-500" : streamStatus === "tag_received" ? "bg-amber-500" : "bg-green-500 animate-pulse"
              }`} />
              {streamStatus === "connecting"   && "Connecting…"}
              {streamStatus === "listening"    && "Listening"}
              {streamStatus === "tag_received" && rfidCode}
              {streamStatus === "error"        && "Error"}
            </div>
            <button onClick={() => setShowDebug(v => !v)}
              className={`px-3 py-1.5 rounded-full text-xs font-semibold border ${
                isDark ? "border-gray-700 text-gray-500 hover:text-gray-300" : "border-gray-300 text-gray-400 hover:text-gray-600"
              }`}>
              {showDebug ? "Hide Debug" : "Debug"}
            </button>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="flex-1 flex items-start justify-center p-6 lg:p-10">
        <div className="w-full max-w-4xl space-y-5">

          {externalError && (
            <Alert type="error" message="Error" description={externalError} showIcon
              action={<Button size="small" onClick={onReset}>Reset</Button>} />
          )}

          {/* RFID Status Card */}
          <div className={`rounded-3xl border shadow-xl overflow-hidden ${
            isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"
          }`}>
            {/* Accent bar */}
            <div className={`h-1.5 w-full transition-colors duration-500 ${
              streamFailed || vehicleStatus === "not_found" || vehicleStatus === "error"
                ? "bg-red-500"
                : vehicleStatus === "found"
                ? "bg-gradient-to-r from-green-400 to-emerald-500"
                : vehicleStatus === "loading"
                ? "bg-gradient-to-r from-amber-400 to-orange-500"
                : "bg-gradient-to-r from-purple-500 to-indigo-500"
            }`} />

            <div className="p-8 flex flex-col sm:flex-row items-center gap-8">

              {/* Animated icon */}
              <div className="relative flex-shrink-0 w-44 h-44 flex items-center justify-center">
                {isListening && [0,1,2].map(i => {
                  const phase = (pulsePhase + i) % 3;
                  return (
                    <span key={i} className="absolute inset-0 rounded-full border-2 transition-all duration-700"
                      style={{
                        borderColor: isDark ? "rgba(139,92,246,0.3)" : "rgba(109,40,217,0.2)",
                        transform:   `scale(${1 + (phase / 3) * 0.65})`,
                        opacity:     1 - (phase / 3) * 0.9,
                      }} />
                  );
                })}

                <div className={`w-32 h-32 rounded-full flex items-center justify-center shadow-2xl transition-all duration-500 ${
                  streamFailed || vehicleStatus === "not_found" || vehicleStatus === "error"
                    ? "bg-red-600"
                    : vehicleStatus === "found"
                    ? "bg-gradient-to-br from-green-400 to-emerald-600"
                    : vehicleStatus === "loading"
                    ? "bg-gradient-to-br from-amber-400 to-orange-500"
                    : isDark ? "bg-gradient-to-br from-purple-700 to-indigo-700"
                             : "bg-gradient-to-br from-purple-600 to-indigo-600"
                }`}>
                  {vehicleStatus === "loading" ? (
                    <div className="w-10 h-10 border-4 border-white/30 border-t-white rounded-full animate-spin" />
                  ) : vehicleStatus === "found" ? (
                    <svg className="w-14 h-14 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  ) : streamFailed || vehicleStatus === "not_found" || vehicleStatus === "error" ? (
                    <svg className="w-14 h-14 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  ) : (
                    <svg className="w-14 h-14 text-white opacity-90" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round"
                        d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z" />
                    </svg>
                  )}
                </div>
              </div>

              {/* Status text */}
              <div className="flex-1 min-w-0">
                {streamStatus === "connecting" && (
                  <Blurb label="Initialising" color="purple" isDark={isDark}
                    title="Connecting to RFID Reader…" sub={RFID_STREAM_URL} />
                )}

                {streamStatus === "listening" && vehicleStatus === "idle" && (
                  <Blurb label="Ready" color="purple" isDark={isDark}
                    title="Awaiting Vehicle RFID Tag"
                    sub="Drive into the reader zone. The tag will be detected automatically.">
                    <div className="flex flex-wrap gap-2 mt-4">
                      {["Drive into zone", "Auto-detected", "Details populated"].map((s, i) => (
                        <span key={i} className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold ${
                          isDark ? "bg-gray-800 text-gray-400" : "bg-slate-100 text-gray-500"
                        }`}>
                          <span className={`w-4 h-4 rounded-full flex items-center justify-center text-white text-xs font-black ${
                            isDark ? "bg-purple-600" : "bg-purple-500"
                          }`}>{i + 1}</span>
                          {s}
                        </span>
                      ))}
                    </div>
                  </Blurb>
                )}

                {vehicleStatus === "loading" && (
                  <Blurb label="Tag Detected" color="amber" isDark={isDark} title="Looking Up Vehicle…">
                    <div className={`inline-flex items-center gap-2 font-mono text-sm px-4 py-2 rounded-xl mt-3 ${
                      isDark ? "bg-gray-800 text-amber-300" : "bg-amber-50 text-amber-700 border border-amber-200"
                    }`}>
                      <span className="w-2 h-2 bg-amber-500 rounded-full animate-pulse" />
                      {rfidCode}
                    </div>
                    <p className={`text-xs mt-2 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                      Querying /MasterData/Vehicles/rfid/…
                    </p>
                  </Blurb>
                )}

                {vehicleStatus === "found" && vehicleData && (
                  <Blurb label="Vehicle Verified ✓" color="green" isDark={isDark} title={vehicleData.plateNumber}>
                    <p className={`font-mono text-xs mt-1 mb-4 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                      RFID: {vehicleData.rfidTag}
                    </p>
                    <div className="flex gap-3 flex-wrap">
                      <Button type="primary" onClick={() => setShowAuthModal(true)}
                        className="bg-green-600 hover:bg-green-700 border-0 h-10 px-6 font-bold rounded-xl">
                        Authenticate Driver →
                      </Button>
                      <Button onClick={handleRescan}
                        className={`h-10 px-5 rounded-xl border font-semibold ${
                          isDark ? "border-gray-700 text-gray-300 bg-gray-800" : "border-gray-200 text-gray-600"
                        }`}>
                        🔄 Rescan
                      </Button>
                    </div>
                  </Blurb>
                )}

                {vehicleStatus === "not_found" && (
                  <Blurb label="Not Registered" color="red" isDark={isDark} title="Vehicle Not Found">
                    <p className={`text-sm mt-1 mb-2 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                      No vehicle linked to RFID tag:
                    </p>
                    <span className={`font-mono text-sm px-3 py-1 rounded-lg inline-block mb-4 ${
                      isDark ? "bg-gray-800 text-red-300" : "bg-red-50 text-red-700 border border-red-200"
                    }`}>{rfidCode}</span>
                    <p className={`text-xs mb-4 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                      Contact the weighbridge office to register this vehicle.
                    </p>
                    <Button onClick={handleRescan}
                      className={`h-10 px-5 rounded-xl border font-semibold ${
                        isDark ? "border-gray-700 text-gray-300 bg-gray-800" : "border-gray-200 text-gray-600"
                      }`}>🔄 Scan Again</Button>
                  </Blurb>
                )}

                {vehicleStatus === "error" && (
                  <Blurb label="Lookup Failed" color="red" isDark={isDark} title="Could Not Fetch Vehicle">
                    <p className={`font-mono text-xs mt-2 mb-4 px-3 py-2 rounded-lg inline-block ${
                      isDark ? "bg-gray-800 text-red-300" : "bg-red-50 text-red-600 border border-red-200"
                    }`}>{lookupError}</p>
                    <div className="flex gap-3">
                      <Button onClick={handleRetryLookup}
                        className="h-10 px-5 rounded-xl bg-amber-500 hover:bg-amber-600 text-white border-0 font-semibold">
                        Retry
                      </Button>
                      <Button onClick={handleRescan}
                        className={`h-10 px-5 rounded-xl border font-semibold ${
                          isDark ? "border-gray-700 text-gray-300 bg-gray-800" : "border-gray-200 text-gray-600"
                        }`}>🔄 Rescan</Button>
                    </div>
                  </Blurb>
                )}

                {streamFailed && (
                  <Blurb label="Connection Failed" color="red" isDark={isDark} title="RFID Stream Unavailable">
                    <p className={`text-sm mt-1 mb-4 ${isDark ? "text-gray-400" : "text-gray-500"}`}>{streamError}</p>
                    <Button onClick={openStream}
                      className="h-10 px-5 rounded-xl bg-amber-500 hover:bg-amber-600 text-white border-0 font-semibold">
                      🔄 Reconnect
                    </Button>
                  </Blurb>
                )}
              </div>
            </div>
          </div>

          {/* Vehicle Detail Card — shown after successful lookup */}
          {vehicleStatus === "found" && vehicleData && (
            <div className={`rounded-3xl border shadow-xl overflow-hidden ${
              isDark ? "bg-gray-900 border-green-800" : "bg-white border-green-200"
            }`}>
              <div className="bg-gradient-to-r from-green-500 via-emerald-500 to-teal-500 px-8 py-5 flex items-center justify-between">
                <div>
                  <p className="text-green-100 text-xs font-semibold uppercase tracking-widest">Registered Vehicle</p>
                  <p className="text-white text-3xl font-black tracking-widest">{vehicleData.plateNumber}</p>
                </div>
                <div className="text-right">
                  <p className="text-green-100 text-xs">RFID Tag</p>
                  <p className="font-mono text-white text-sm">{vehicleData.rfidTag}</p>
                  <p className="text-green-200 text-xs mt-1">{vehicleData.detectedAt?.toLocaleTimeString()}</p>
                </div>
              </div>

              <div className={`p-6 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3 ${isDark ? "bg-gray-900" : "bg-white"}`}>
                {[
                  { label: "Type",     value: vehicleData.vehicleType },
                  { label: "Make",     value: vehicleData.vehicleMake },
                  { label: "Model",    value: vehicleData.vehicleModel },
                  { label: "Capacity", value: vehicleData.capacity ? `${vehicleData.capacity} kg` : null },
                  { label: "SACCO",    value: vehicleData.saccoName },
                  { label: "Owner",    value: vehicleData.ownerName },
                  { label: "Status",   value: vehicleData.status,
                    badge: vehicleData.isActive ? "green" : "red" },
                ].filter(f => f.value).map(f => (
                  <div key={f.label} className={`p-3 rounded-2xl ${isDark ? "bg-gray-800" : "bg-slate-50 border border-slate-100"}`}>
                    <p className={`text-xs font-bold uppercase tracking-wider mb-1 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                      {f.label}
                    </p>
                    {f.badge
                      ? <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-bold ${f.badge === "green" ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>{f.value}</span>
                      : <p className={`font-bold text-sm ${isDark ? "text-white" : "text-gray-900"}`}>{f.value}</p>
                    }
                  </div>
                ))}
              </div>

              <div className={`px-6 py-4 flex items-center justify-between border-t ${
                isDark ? "bg-gray-900 border-gray-800" : "bg-slate-50 border-slate-100"
              }`}>
                <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>
                  Tap your NFC card to authenticate and proceed
                </p>
                <Button type="primary" size="large" onClick={() => setShowAuthModal(true)}
                  className="bg-green-600 hover:bg-green-700 border-0 h-11 px-8 font-black rounded-2xl">
                  Authenticate Driver →
                </Button>
              </div>
            </div>
          )}

          {/* Debug panel */}
          {showDebug && (
            <div className="rounded-2xl border border-gray-800 bg-gray-950 p-5 font-mono text-xs text-green-400">
              <div className="flex items-center justify-between mb-3">
                <span className="font-sans font-bold text-xs uppercase tracking-wider text-gray-500">SSE Debug</span>
                <button onClick={() => setDebugLog([])} className="text-gray-600 hover:text-gray-400 text-xs">Clear</button>
              </div>
              {debugLog.length === 0
                ? <p className="text-gray-600">No messages yet…</p>
                : <div className="space-y-1 max-h-52 overflow-y-auto">
                    {debugLog.map((e, i) => <p key={i} className="break-all leading-relaxed">{e}</p>)}
                  </div>
              }
            </div>
          )}
        </div>
      </main>

      {/* NFC Auth Modal */}
      <AuthenticationMethodModal
        visible={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        onSelectNFC={handleAuthComplete}
      />

      <footer className={`${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"} border-t px-8 py-4 text-center`}>
        <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>
          Powered by <span className="font-black text-amber-500">QALIBRATED SYSTEMS</span>
        </p>
      </footer>
    </div>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────
function Blurb({ label, color, title, sub, isDark, children }) {
  const cls = { purple: isDark ? "text-purple-400" : "text-purple-600", amber: isDark ? "text-amber-400" : "text-amber-600", green: "text-green-500", red: "text-red-500" };
  return (
    <div>
      <p className={`text-xs font-bold uppercase tracking-widest mb-1 ${cls[color]}`}>{label}</p>
      <h2 className={`text-2xl font-black mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>{title}</h2>
      {sub && <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>{sub}</p>}
      {children}
    </div>
  );
}