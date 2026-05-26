/**
 * VehicleDetectionScreen.jsx
 *
 * Changes from original:
 *  1. RFID stream URL now comes from SystemSettings (useHardwareConfig) — live,
 *     no page reload needed when the URL is changed in System Settings → Save.
 *  2. After a vehicle is confirmed, usePendingTransaction checks whether that
 *     plate already has an "Incomplete" transaction:
 *       • None found  → normal first-weighing flow (opens NFC modal)
 *       • Found       → opens NFC modal with pendingTxn attached so the
 *                        parent can route to second-weight capture instead.
 */

import React, { useCallback, useEffect, useRef, useState } from "react";
import { Alert, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import AuthenticationMethodModal from "./AuthenticationMethodModal.jsx";
import { getVehicleByRfid } from "../../api/MasterData/Vehicles";
import { apiClient } from "../../api/helpers/apiClients";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import { usePendingTransaction } from "../../hooks/usePendingTransaction";
import logo from "../../assets/qalitrack_logo_full.png";

const CONTROL_TYPES = new Set(["connected", "heartbeat", "ping", "pong", "keepalive"]);

// ── Parse SSE ─────────────────────────────────────────────────────────────────
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
  let ownerName       = v.ownerName       ?? null;
  let transporterName = v.transporterName ?? null;
  let supplierName    = v.supplierName    ?? null;
  let saccoName       = v.saccoName       ?? null;
  const fetches = [];

  if (!ownerName && v.ownerId)
    fetches.push(apiClient.get(`/Owners/${v.ownerId}`).then(r => { ownerName = pickName(r) ?? ownerName; }).catch(() => {}));
  if (!supplierName && v.supplierId)
    fetches.push(apiClient.get(`/MasterData/Suppliers/${v.supplierId}`).then(r => { supplierName = pickName(r) ?? supplierName; }).catch(() => {}));
  if (!transporterName && v.transporterId)
    fetches.push(apiClient.get(`/MasterData/Transporters/${v.transporterId}`).then(r => { transporterName = pickName(r) ?? transporterName; }).catch(() => {}));
  if (!transporterName && v.saccoId)
    fetches.push(apiClient.get(`/MasterData/Saccos/${v.saccoId}`).then(r => { saccoName = pickName(r) ?? saccoName; transporterName = transporterName ?? saccoName; }).catch(() => {}));

  await Promise.allSettled(fetches);
  return { ownerName, supplierName, transporterName, saccoName };
};

const normaliseVehicle = (v, rfidCode, enriched = {}) => ({
  id:                 v.id,
  rfidTag:            v.rfiDcode           ?? rfidCode,
  registrationNumber: v.registrationNumber ?? "—",
  vehicleType:        v.type               ?? null,
  vehicleMake:        v.make               ?? null,
  vehicleModel:       v.model              ?? null,
  capacity:           v.netWeightCapacity  ?? v.grossWeight ?? null,
  status:             v.status             ?? null,
  isActive:           !v.isDeleted,
  detectedAt:         new Date(),
  ownerId:            v.ownerId            ?? null,
  ownerName:          v.ownerName          ?? enriched.ownerName       ?? null,
  transporterId:      v.transporterId      ?? null,
  transporterName:    v.transporterName    ?? enriched.transporterName ?? enriched.saccoName ?? null,
  supplierId:         v.supplierId         ?? null,
  supplierName:       v.supplierName       ?? enriched.supplierName    ?? null,
  saccoId:            v.saccoId            ?? null,
  saccoName:          v.saccoName          ?? enriched.saccoName       ?? null,
  driverIds:          v.driverIds          ?? [],
  driverNames:        v.driverNames        ?? [],
});

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function VehicleDetectionScreen({ onVehicleDetected, error: externalError, onReset }) {
  const { isDark } = useTheme();

  // ── Live hardware config from SystemSettings localStorage ─────────────────
  const hwConfig = useHardwareConfig();
  const rfidStreamUrl = hwConfig.rfidStreamUrl;

  // ── Pending transaction check ─────────────────────────────────────────────
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

  // ── SSE stream — reconnects whenever rfidStreamUrl changes ───────────────
  const openStream = useCallback(() => {
    if (esRef.current) { esRef.current.close(); esRef.current = null; }
    setStreamStatus("connecting"); setStreamError(null);
    dbg("Opening RFID stream", rfidStreamUrl);

    try {
      const es = new EventSource(rfidStreamUrl);
      esRef.current = es;
      es.onopen = () => { setStreamStatus("listening"); dbg("Stream connected ✓"); };
      es.onmessage = (event) => {
        dbg("SSE raw", event.data);
        const code = parseRfidCode(event.data);
        if (!code) { dbg("Skipped (control msg)"); return; }
        dbg("RFID code", code);
        lastCode.current = code;
        lookupRef.current = false;
        setRfidCode(code);
        setStreamStatus("tag_received");
      };
      es.onerror = () => {
        setStreamError("Cannot connect to RFID reader at " + rfidStreamUrl);
        setStreamStatus("error");
        es.close();
        esRef.current = null;
      };
    } catch {
      setStreamError("Failed to open SSE connection.");
      setStreamStatus("error");
    }
  }, [rfidStreamUrl, dbg]);

  // Restart stream if URL changes (SystemSettings save)
  useEffect(() => {
    openStream();
    return () => { if (esRef.current) esRef.current.close(); };
  }, [openStream]);

  // ── Vehicle lookup + enrichment ───────────────────────────────────────────
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
        if (!v) { dbg("No vehicle found"); setVehicleStatus("not_found"); return; }

        // Step 1: show partial vehicle immediately
        const partial = normaliseVehicle(v, rfidCode);
        setVehicleData(partial);
        setVehicleStatus("found");

        // Step 2: enrich missing relational names
        const needsEnrichment = !v.ownerName || !v.supplierName || !v.transporterName;
        if (needsEnrichment) {
          dbg("Enriching names…");
          setEnriching(true);
          try {
            const enriched = await enrichVehicleNames(v);
            const full = normaliseVehicle(v, rfidCode, enriched);
            setVehicleData(full);
            dbg("Enrichment done");
          } finally {
            setEnriching(false);
          }
        }

        // Step 3: check for pending transaction on this plate
        const plate = v.registrationNumber ?? partial.registrationNumber;
        dbg("Checking for pending transaction", plate);
        await checkPending(plate);

        // Step 4: open auth modal (pendingTxn state is now set)
        setTimeout(() => setShowAuthModal(true), 1200);

      } catch (err) {
        dbg("Lookup error", err.message);
        const notFound = err.message?.includes("404") || /not found|no vehicle/i.test(err.message ?? "");
        if (notFound) setVehicleStatus("not_found");
        else { setVehicleStatus("error"); setLookupError(err.message || "Server error."); }
      }
    })();
  }, [rfidCode, dbg, checkPending, clearPending]);

  const handleRescan = () => {
    lastCode.current = null;
    lookupRef.current = false;
    setRfidCode(null);
    setVehicleData(null);
    setVehicleStatus("idle");
    setStreamStatus("listening");
    setLookupError(null);
    setShowAuthModal(false);
    clearPending();
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
    // Pass pendingTxn to parent so it can route to second-weight screen
    onVehicleDetected({ vehicle: vehicleData, driver, pendingTxn: pendingTxn ?? null });
  };

  const isListening  = streamStatus === "connecting" || streamStatus === "listening";
  const streamFailed = streamStatus === "error";

  // ── Pending transaction banner (shown on vehicle card while checking/found)
  const PendingBanner = () => {
    if (vehicleStatus !== "found") return null;
    if (checkingPending) {
      return (
        <div className="flex items-center gap-2 px-3 py-2 rounded-xl text-xs font-semibold"
          style={{ background: "#fffbeb", border: "1px solid #fcd34d", color: "#92400e" }}>
          <span className="w-3 h-3 border-2 border-amber-400 border-t-transparent rounded-full animate-spin flex-shrink-0" />
          Checking for pending transaction…
        </div>
      );
    }
    if (pendingTxn) {
      const fw = pendingTxn.firstWeight ?? pendingTxn.grossWeight ?? "—";
      const at = pendingTxn.createdAt
        ? new Date(pendingTxn.createdAt).toLocaleString()
        : "Unknown time";
      return (
        <div className="px-3 py-2.5 rounded-xl text-xs"
          style={{ background: "#fef2f2", border: "1.5px solid #fca5a5" }}>
          <div className="flex items-center gap-2 mb-1">
            <span className="text-base">⚠️</span>
            <p className="font-bold text-sm" style={{ color: "#dc2626" }}>
              Pending Transaction Detected
            </p>
          </div>
          <p style={{ color: "#7f1d1d" }}>
            This vehicle has an incomplete weighing from <strong>{at}</strong>.
            First weight: <strong>{fw} kg</strong>.
          </p>
          <p className="mt-1 font-semibold" style={{ color: "#dc2626" }}>
            This visit will be recorded as the <u>second (tare) weight</u>.
          </p>
        </div>
      );
    }
    return null;
  };

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className={`min-h-screen flex flex-col ${isDark ? "bg-gray-950" : "bg-slate-100"}`}>

      {/* Header */}
      <header className={`${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"} border-b px-8 py-5 shadow-sm`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <img src={logo} alt="Qalitrack" className="h-14 w-auto" />
            <div className={`w-px h-8 ${isDark ? "bg-gray-700" : "bg-gray-200"}`} />
            <div>
              <h1 className={`text-xl font-black ${isDark ? "text-white" : "text-gray-900"}`}>Self-Service Weighing</h1>
              <p className={`text-xs ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                RFID: <span className="font-mono">{rfidStreamUrl}</span>
              </p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold border ${
              streamFailed
                ? "bg-red-50 border-red-200 text-red-600"
                : streamStatus === "tag_received"
                ? "bg-amber-50 border-amber-200 text-amber-700"
                : "bg-green-50 border-green-200 text-green-700"
            }`}>
              <span className={`w-1.5 h-1.5 rounded-full ${streamFailed ? "bg-red-500" : streamStatus === "tag_received" ? "bg-amber-500" : "bg-green-500 animate-pulse"}`} />
              {streamStatus === "connecting"    && "Connecting…"}
              {streamStatus === "listening"     && "Listening"}
              {streamStatus === "tag_received"  && rfidCode}
              {streamStatus === "error"         && "Error"}
            </div>
            <button onClick={() => setShowDebug(v => !v)}
              className={`px-3 py-1.5 rounded-full text-xs font-semibold border ${isDark ? "border-gray-700 text-gray-500" : "border-gray-300 text-gray-400"}`}>
              {showDebug ? "Hide Log" : "Debug"}
            </button>
          </div>
        </div>
      </header>

      {/* Main */}
      <main className="flex-1 flex items-center justify-center p-6 lg:p-10">
        <div className="w-full max-w-4xl space-y-5">

          {externalError && (
            <Alert type="error" message="Error" description={externalError} showIcon
              action={<Button size="small" onClick={onReset}>Reset</Button>} />
          )}

          {/* RFID Status Card */}
          <div className={`rounded-3xl border shadow-xl overflow-hidden ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"}`}>
            <div className={`h-1.5 w-full transition-colors duration-500 ${
              streamFailed || vehicleStatus === "not_found" || vehicleStatus === "error" ? "bg-red-500"
              : vehicleStatus === "found"   ? "bg-gradient-to-r from-green-400 to-emerald-500"
              : vehicleStatus === "loading" ? "bg-gradient-to-r from-amber-400 to-orange-500"
              : "bg-gradient-to-r from-amber-400 to-orange-500"
            }`} />

            <div className="p-8 flex flex-col sm:flex-row items-center gap-8">

              {/* Animated icon */}
              <div className="relative flex-shrink-0 w-44 h-44 flex items-center justify-center">
                {isListening && [0,1,2].map(i => {
                  const ph = (pulsePhase + i) % 3;
                  return <span key={i} className="absolute inset-0 rounded-full border-2 transition-all duration-700"
                    style={{ borderColor: "rgba(217,119,6,0.25)", transform: `scale(${1 + (ph / 3) * 0.65})`, opacity: 1 - (ph / 3) * 0.9 }} />;
                })}
                <div className={`w-32 h-32 rounded-full flex items-center justify-center shadow-2xl transition-all duration-500 ${
                  streamFailed || vehicleStatus === "not_found" || vehicleStatus === "error" ? "bg-red-600"
                  : vehicleStatus === "found"   ? "bg-gradient-to-br from-green-400 to-emerald-600"
                  : vehicleStatus === "loading" ? "bg-gradient-to-br from-amber-400 to-orange-500"
                  : "bg-gradient-to-br from-amber-500 to-orange-600"
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
                    <svg className="w-14 h-14 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M8.288 15.038a5.25 5.25 0 017.424 0M5.106 11.856c3.807-3.808 9.98-3.808 13.788 0M1.924 8.674c5.565-5.565 14.587-5.565 20.152 0M12.53 18.22l-.53.53-.53-.53a.75.75 0 011.06 0z" />
                    </svg>
                  )}
                </div>
              </div>

              {/* Status text */}
              <div className="flex-1 min-w-0">
                {streamStatus === "connecting" && (
                  <Blurb label="Initialising" color="amber" isDark={isDark} title="Connecting to RFID Reader…" sub={rfidStreamUrl} />
                )}
                {streamStatus === "listening" && vehicleStatus === "idle" && (
                  <Blurb label="Ready" color="amber" isDark={isDark} title="Awaiting Vehicle RFID Tag" sub="Drive into the reader zone. The tag will be detected automatically.">
                    <div className="flex flex-wrap gap-2 mt-4">
                      {["Drive into zone","Auto-detected","Details populated"].map((s, i) => (
                        <span key={i} className="flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold bg-slate-100 text-gray-500">
                          <span className="w-4 h-4 rounded-full flex items-center justify-center bg-amber-500 text-white text-xs font-black">{i + 1}</span>{s}
                        </span>
                      ))}
                    </div>
                  </Blurb>
                )}
                {vehicleStatus === "loading" && (
                  <Blurb label="Tag Detected" color="amber" isDark={isDark} title="Looking Up Vehicle…">
                    <div className="inline-flex items-center gap-2 font-mono text-sm px-4 py-2 rounded-xl mt-3 bg-amber-50 text-amber-700 border border-amber-200">
                      <span className="w-2 h-2 bg-amber-500 rounded-full animate-pulse" />{rfidCode}
                    </div>
                  </Blurb>
                )}
                {vehicleStatus === "found" && vehicleData && (
                  <Blurb label={pendingTxn ? "⚠️ Second Weighing" : "Vehicle Verified ✓"} color={pendingTxn ? "amber" : "green"} isDark={isDark} title={vehicleData.registrationNumber}>
                    <p className={`font-mono text-xs mt-1 mb-3 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
                      RFID: {vehicleData.rfidTag}
                    </p>

                    {/* Pending transaction banner */}
                    <div className="mb-3"><PendingBanner /></div>

                    {/* Relational fields */}
                    <div className="grid grid-cols-2 gap-2 mb-4">
                      {[
                        { label: "Owner",       value: vehicleData.ownerName       },
                        { label: "Supplier",    value: vehicleData.supplierName    },
                        { label: "Transporter", value: vehicleData.transporterName },
                        { label: "SACCO",       value: vehicleData.saccoName       },
                      ].map(f => (
                        <div key={f.label} className={`px-3 py-2 rounded-xl text-xs border flex items-center gap-2 ${
                          f.value
                            ? isDark ? "bg-gray-800 border-gray-700" : "bg-slate-50 border-slate-200"
                            : "bg-amber-50 border-amber-200"
                        }`}>
                          <span className={`text-xs font-bold w-20 flex-shrink-0 ${f.value ? "text-gray-400" : "text-amber-500"}`}>{f.label}</span>
                          {enriching && !f.value ? (
                            <span className="flex items-center gap-1.5 text-amber-600">
                              <span className="w-3 h-3 border-2 border-amber-400 border-t-transparent rounded-full animate-spin" />
                              <span className="text-xs italic">fetching…</span>
                            </span>
                          ) : (
                            <span className={`font-semibold truncate ${f.value ? isDark ? "text-white" : "text-gray-800" : "text-amber-400 italic text-xs"}`}>
                              {f.value || "not linked in DB"}
                            </span>
                          )}
                        </div>
                      ))}
                    </div>

                    <div className="flex gap-3 flex-wrap">
                      <Button type="primary" onClick={() => setShowAuthModal(true)}
                        className={`${pendingTxn ? "bg-amber-600 hover:bg-amber-700" : "bg-green-600 hover:bg-green-700"} border-0 h-10 px-6 font-bold rounded-xl`}>
                        {pendingTxn ? "Authenticate for 2nd Weight →" : "Authenticate Driver →"}
                      </Button>
                      <Button onClick={handleRescan}
                        className={`h-10 px-5 rounded-xl border font-semibold ${isDark ? "border-gray-700 text-gray-300 bg-gray-800" : "border-gray-200 text-gray-600"}`}>
                        🔄 Rescan
                      </Button>
                    </div>
                  </Blurb>
                )}
                {vehicleStatus === "not_found" && (
                  <Blurb label="Not Registered" color="red" isDark={isDark} title="Vehicle Not Found">
                    <p className="text-sm mt-1 mb-4 text-gray-500">No vehicle linked to RFID: <span className="font-mono">{rfidCode}</span></p>
                    <Button onClick={handleRescan} className="h-10 px-5 rounded-xl border font-semibold border-gray-200 text-gray-600">🔄 Scan Again</Button>
                  </Blurb>
                )}
                {vehicleStatus === "error" && (
                  <Blurb label="Lookup Failed" color="red" isDark={isDark} title="Could Not Fetch Vehicle">
                    <p className="font-mono text-xs mt-2 mb-4 px-3 py-2 rounded-lg inline-block bg-red-50 text-red-600 border border-red-200">{lookupError}</p>
                    <div className="flex gap-3">
                      <Button onClick={handleRetryLookup} className="h-10 px-5 rounded-xl bg-amber-500 hover:bg-amber-600 text-white border-0 font-semibold">Retry</Button>
                      <Button onClick={handleRescan} className="h-10 px-5 rounded-xl border font-semibold border-gray-200 text-gray-600">🔄 Rescan</Button>
                    </div>
                  </Blurb>
                )}
                {streamFailed && (
                  <Blurb label="Connection Failed" color="red" isDark={isDark} title="RFID Stream Unavailable">
                    <p className="text-sm mt-1 mb-4 text-gray-500">{streamError}</p>
                    <Button onClick={openStream} className="h-10 px-5 rounded-xl bg-amber-500 hover:bg-amber-600 text-white border-0 font-semibold">🔄 Reconnect</Button>
                  </Blurb>
                )}
              </div>
            </div>
          </div>

          {/* Vehicle Detail Card */}
          {vehicleStatus === "found" && vehicleData && (
            <div className={`rounded-3xl border shadow-xl overflow-hidden ${isDark ? "bg-gray-900 border-green-800" : "bg-white border-green-200"}`}>
              <div className={`px-8 py-5 flex items-center justify-between ${
                pendingTxn
                  ? "bg-gradient-to-r from-amber-500 via-orange-500 to-red-500"
                  : "bg-gradient-to-r from-green-500 via-emerald-500 to-teal-500"
              }`}>
                <div>
                  <p className="text-green-100 text-xs font-semibold uppercase tracking-widest">
                    {pendingTxn ? "Second (Tare) Weighing" : "Registered Vehicle"}
                  </p>
                  <p className="text-white text-3xl font-black tracking-widest">{vehicleData.registrationNumber}</p>
                  {pendingTxn && (
                    <p className="text-orange-100 text-xs mt-1">
                      Completing transaction from {pendingTxn.createdAt ? new Date(pendingTxn.createdAt).toLocaleString() : "earlier"}
                      {" · "}First weight: <strong>{pendingTxn.firstWeight ?? pendingTxn.grossWeight ?? "—"} kg</strong>
                    </p>
                  )}
                </div>
                <div className="text-right">
                  <p className="text-green-100 text-xs">RFID Tag</p>
                  <p className="font-mono text-white text-sm">{vehicleData.rfidTag}</p>
                  <p className="text-green-200 text-xs mt-1">{vehicleData.detectedAt?.toLocaleTimeString()}</p>
                </div>
              </div>

              <div className={`p-6 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3 ${isDark ? "bg-gray-900" : "bg-white"}`}>
                {[
                  { label: "Type",        value: vehicleData.vehicleType     },
                  { label: "Make",        value: vehicleData.vehicleMake     },
                  { label: "Model",       value: vehicleData.vehicleModel    },
                  { label: "Capacity",    value: vehicleData.capacity ? `${vehicleData.capacity} kg` : null },
                  { label: "Owner",       value: vehicleData.ownerName       },
                  { label: "Supplier",    value: vehicleData.supplierName    },
                  { label: "Transporter", value: vehicleData.transporterName },
                  { label: "SACCO",       value: vehicleData.saccoName       },
                  { label: "Status",      value: vehicleData.status, badge: vehicleData.isActive ? "green" : "red" },
                ].filter(f => f.value).map(f => (
                  <div key={f.label} className={`p-3 rounded-2xl ${isDark ? "bg-gray-800" : "bg-slate-50 border border-slate-100"}`}>
                    <p className={`text-xs font-bold uppercase tracking-wider mb-1 ${isDark ? "text-gray-500" : "text-gray-400"}`}>{f.label}</p>
                    {f.badge
                      ? <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-bold ${f.badge === "green" ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>{f.value}</span>
                      : <p className={`font-bold text-sm ${isDark ? "text-white" : "text-gray-900"}`}>{f.value}</p>}
                  </div>
                ))}
                {enriching && (
                  <div className={`p-3 rounded-2xl col-span-2 flex items-center gap-2 ${isDark ? "bg-gray-800" : "bg-amber-50 border border-amber-100"}`}>
                    <span className="w-4 h-4 border-2 border-amber-400 border-t-transparent rounded-full animate-spin flex-shrink-0" />
                    <p className={`text-xs font-semibold ${isDark ? "text-amber-400" : "text-amber-600"}`}>
                      Fetching owner / supplier / transporter names…
                    </p>
                  </div>
                )}
              </div>

              <div className={`px-6 py-4 flex items-center justify-between border-t ${isDark ? "bg-gray-900 border-gray-800" : "bg-slate-50 border-slate-100"}`}>
                <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>
                  {pendingTxn
                    ? "Tap your NFC card to authenticate and capture the second (tare) weight"
                    : "Tap your NFC card to authenticate and proceed to weighing"
                  }
                </p>
                <Button type="primary" size="large" onClick={() => setShowAuthModal(true)}
                  className={`${pendingTxn ? "bg-amber-600 hover:bg-amber-700" : "bg-green-600 hover:bg-green-700"} border-0 h-11 px-8 font-black rounded-2xl`}>
                  {pendingTxn ? "Authenticate for 2nd Weight →" : "Authenticate Driver →"}
                </Button>
              </div>
            </div>
          )}

          {/* Debug log */}
          {showDebug && (
            <div className="rounded-2xl border border-gray-800 bg-gray-950 p-5 font-mono text-xs text-green-400">
              <div className="flex items-center justify-between mb-3">
                <span className="font-sans font-bold text-xs uppercase tracking-wider text-gray-500">Debug Log</span>
                <button onClick={() => setDebugLog([])} className="text-gray-600 hover:text-gray-400 text-xs">Clear</button>
              </div>
              {debugLog.length === 0
                ? <p className="text-gray-600">No messages yet…</p>
                : <div className="space-y-1 max-h-52 overflow-y-auto">{debugLog.map((e, i) => <p key={i} className="break-all leading-relaxed">{e}</p>)}</div>}
            </div>
          )}
        </div>
      </main>

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

function Blurb({ label, color, title, sub, isDark, children }) {
  const cls = { amber: isDark ? "text-amber-400" : "text-amber-600", green: "text-green-500", red: "text-red-500" };
  return (
    <div>
      <p className={`text-xs font-bold uppercase tracking-widest mb-1 ${cls[color]}`}>{label}</p>
      <h2 className={`text-2xl font-black mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>{title}</h2>
      {sub && <p className={`text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>{sub}</p>}
      {children}
    </div>
  );
}