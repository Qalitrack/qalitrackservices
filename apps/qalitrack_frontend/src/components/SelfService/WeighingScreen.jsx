/**
 * WeighingScreen.jsx — Unmanned Weighing
 *
 * Changes from original:
 *  1. Accepts `existingTransaction` prop — when present, switches to
 *     second-weight (tare) mode:
 *       • Header shows "2nd Weight" banner with first-weight details
 *       • Submit calls PATCH to update existing transaction instead of POST
 *       • Payload sends secondWeight instead of firstWeight
 *  2. Scale stream URL read from SystemSettings via useHardwareConfig()
 *     (live — updates without page reload when System Settings are saved)
 *  3. Weight now reads from the real scale SSE stream (hwConfig.scaleStreamUrl),
 *     same connection/heartbeat/normalization approach as LiveWeighbridgeStatus.jsx
 *     — this used to simulate a fake weight that converged to a hardcoded
 *     constant regardless of what was actually on the scale.
 *  4. Theme aligned with its sibling kiosk screen (VehicleDetectionScreen.jsx):
 *     useTheme() + Tailwind dark/light classes instead of hardcoded inline hex.
 */

import React, { useEffect, useState, useRef, useCallback } from "react";
import { message, Select } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import logo from "../../assets/qalitrack_logo_full.png";
const { Option } = Select;

const BASE_URL = import.meta.env.VITE_API_URL || "/api";

// ── Auth token ────────────────────────────────────────────────────────────────
function getKioskToken() {
  try {
    const raw = localStorage.getItem("authSession");
    if (raw) { const p = JSON.parse(raw); if (p?.token) return p.token; }
  } catch {}
  return null;
}

// ── API helpers ───────────────────────────────────────────────────────────────
async function apiFetch(path, opts = {}) {
  const token = getKioskToken();
  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(opts.headers || {}),
  };
  const res = await fetch(`${BASE_URL}${path}`, { ...opts, headers });
  if (!res.ok) { const b = await res.text(); throw new Error(b || `HTTP ${res.status}`); }
  return res.json();
}

const searchProducts  = q => apiFetch(`/MasterData/Products?searchTerm=${encodeURIComponent(q)}&pageSize=20`).then(r => r?.items ?? r?.data?.items ?? r?.data?.data?.items ?? []);
const getWeighbridges = () => apiFetch(`/MasterData/Weighbridges?pageSize=50`).then(r => r?.items ?? r?.data?.items ?? r?.data?.data?.items ?? []);

// First weighing — POST new transaction
const postTransaction  = b => apiFetch(`/Transaction/Transaction/Transaction`, { method: "POST", body: JSON.stringify({ request: b }) });

// Second weighing — PATCH existing transaction to add second weight
// Adjust endpoint/payload to match your actual API
const patchTransaction = (id, b) => apiFetch(`/Transaction/Transaction/Transaction/${id}`, { method: "PATCH", body: JSON.stringify(b) });

// ── Debounce ──────────────────────────────────────────────────────────────────
function useDebounce(fn, delay) {
  const t = useRef(null);
  return useCallback((...args) => {
    if (t.current) clearTimeout(t.current);
    t.current = setTimeout(() => fn(...args), delay);
  }, [fn, delay]);
}

/**
 * Normalize raw weight from the stream — same rule as LiveWeighbridgeStatus.jsx.
 * The scale indicator streams values divided by 1000 in some firmware versions
 * (e.g. indicator reads 10 kg → stream sends 0.010000).
 * Detection rule: if the parsed value is > 0 and < 1, multiply by 1000.
 */
function normalizeWeight(raw) {
  const val = typeof raw === "number" ? raw : parseFloat(String(raw).replace(/[^0-9.-]/g, ""));
  if (isNaN(val)) return null;
  if (val > 0 && val < 1) return Math.round(val * 1000 * 100) / 100; // reversed — fix it
  return val; // 0 or ≥ 1 — already correct
}

// ── extractVehicleFields ──────────────────────────────────────────────────────
const extractVehicleFields = (vehicleData = {}) => {
  const v = vehicleData?.vehicle ?? vehicleData ?? {};
  return {
    vehicleID:       v.id                  ?? null,
    noPlate:         v.registrationNumber  ?? "",
    rfidTag:         v.rfidTag             ?? "",
    vehicleType:     v.vehicleType         ?? "",
    vehicleMake:     v.vehicleMake         ?? "",
    vehicleModel:    v.vehicleModel        ?? "",
    capacity:        v.capacity            ?? "",
    ownerId:         v.ownerId             ?? "",
    ownerName:       v.ownerName           ?? "",
    transporterID:   v.transporterId       ?? null,
    transporterName: v.transporterName     ?? v.saccoName ?? "",
    supplierID:      v.supplierId          ?? null,
    supplierName:    v.supplierName        ?? "",
    saccoName:       v.saccoName           ?? "",
    commodityID:     null,
    commodityName:   "",
    customerName:    "",
  };
};

const extractDriverFields = (vehicleData = {}, driverData = {}) => {
  const d = vehicleData?.driver ?? driverData ?? {};
  return {
    driverID:    d.id         ?? null,
    driverName:  d.name       ?? "",
    driverPhone: d.phone      ?? "",
    licenseNo:   d.licenseNo  ?? "",
    employeeId:  d.employeeId ?? "",
    nfcUid:      d.uid        ?? "",
  };
};

const isValidGuid = g => g && g !== "00000000-0000-0000-0000-000000000000" && String(g).length > 10;

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function WeighingScreen({
  vehicleData = {},
  driverData  = {},
  existingTransaction = null,  // ← null: first weight | object: second weight
  onWeighingComplete,
  onBack,
  error: parentError,
}) {
  const { isDark } = useTheme();

  // ── Live scale URL from SystemSettings ────────────────────────────────────
  const hwConfig = useHardwareConfig();

  const isSecondWeigh = Boolean(existingTransaction);
  const vf = extractVehicleFields(vehicleData);
  const df = extractDriverFields(vehicleData, driverData);

  const [products,      setProducts]      = useState([]);
  const [weighbridges,  setWeighbridges]  = useState([]);

  const [form, setForm] = useState({
    vehicleID:       vf.vehicleID,
    noPlate:         vf.noPlate,
    rfidTag:         vf.rfidTag,
    vehicleType:     vf.vehicleType,
    vehicleMake:     vf.vehicleMake,
    vehicleModel:    vf.vehicleModel,
    capacity:        vf.capacity,
    ownerId:         vf.ownerId,
    ownerName:       vf.ownerName,
    transporterID:   vf.transporterID,
    transporterName: vf.transporterName,
    supplierID:      vf.supplierID,
    supplierName:    vf.supplierName,
    saccoName:       vf.saccoName,
    commodityID:     existingTransaction?.commodityID   ?? null,
    commodityName:   existingTransaction?.commodityName ?? "",
    customerName:    existingTransaction?.customerName  ?? "",
    driverID:        df.driverID,
    driverName:      df.driverName,
    driverPhone:     df.driverPhone,
    licenseNo:       df.licenseNo,
    employeeId:      df.employeeId,
    nfcUid:          df.nfcUid,
    operatorID:      df.driverID   || null,
    operatorName:    df.driverName || "Unmanned",
    weighMode:       "Gross/Tare",
    weighBridgeID:   null,
    weighBridgeName: existingTransaction?.weighBridgeName ?? "Factory A",
    scaleName:       existingTransaction?.weighBridgeName ?? "Factory A",
    originName:      existingTransaction?.originName      ?? "",
    destinationName: existingTransaction?.destinationName ?? "",
    operation:       existingTransaction?.operation       ?? "weighing",
    notes:           existingTransaction?.notes           ?? "",
  });

  // Re-sync on vehicleData updates
  useEffect(() => {
    const newVf = extractVehicleFields(vehicleData);
    const newDf = extractDriverFields(vehicleData, driverData);
    setForm(prev => ({
      ...prev,
      vehicleID:       newVf.vehicleID,
      noPlate:         newVf.noPlate,
      rfidTag:         newVf.rfidTag,
      vehicleType:     newVf.vehicleType,
      vehicleMake:     newVf.vehicleMake,
      vehicleModel:    newVf.vehicleModel,
      capacity:        newVf.capacity,
      ownerId:         newVf.ownerId,
      ownerName:       newVf.ownerName,
      transporterID:   newVf.transporterID,
      transporterName: newVf.transporterName,
      supplierID:      newVf.supplierID,
      supplierName:    newVf.supplierName,
      saccoName:       newVf.saccoName,
      driverID:        newDf.driverID,
      driverName:      newDf.driverName,
      driverPhone:     newDf.driverPhone,
      licenseNo:       newDf.licenseNo,
      employeeId:      newDf.employeeId,
      nfcUid:          newDf.nfcUid,
      operatorID:      newDf.driverID   || null,
      operatorName:    newDf.driverName || "Unmanned",
    }));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleData, driverData]);

  // ── Weight — real scale SSE stream ────────────────────────────────────────
  const [weightMode,     setWeightMode]     = useState("captured");
  const [capturedWeight, setCapturedWeight] = useState(0);
  const [manualWeight,   setManualWeight]   = useState("");
  const [isStable,       setIsStable]       = useState(false);
  const [scaleConnected, setScaleConnected] = useState(false);
  const [submitting,     setSubmitting]     = useState(false);
  const [apiError,       setApiError]       = useState(null);
  const bufferRef        = useRef(null);
  const lastStableRef    = useRef(null);
  const stabilityCounter = useRef(0);

  useEffect(() => {
    getWeighbridges().then(list => {
      setWeighbridges(list);
      if (existingTransaction?.weighBridgeID) {
        const wb = list.find(w => w.id === existingTransaction.weighBridgeID);
        if (wb) setForm(p => ({
          ...p,
          weighBridgeID:   wb.id,
          weighBridgeName: wb.location ?? wb.name,
          scaleName:       wb.location ?? wb.name,
        }));
        return;
      }
      const factoryA = list.find(wb => /factory\s*a/i.test(wb.location ?? wb.name ?? ""));
      if (factoryA) setForm(p => ({
        ...p,
        weighBridgeID:   factoryA.id,
        weighBridgeName: factoryA.location ?? factoryA.name,
        scaleName:       factoryA.location ?? factoryA.name,
      }));
    }).catch(() => {});
  }, [existingTransaction]);

  // Real scale stream — same connect/heartbeat/normalize approach as
  // LiveWeighbridgeStatus.jsx, feeding the same buffer + stability pipeline.
  useEffect(() => {
    bufferRef.current = null;
    lastStableRef.current = null;
    stabilityCounter.current = 0;
    setIsStable(false);
    setCapturedWeight(0);
    setScaleConnected(false);

    const source = new EventSource(hwConfig.scaleStreamUrl);

    source.onerror = () => setScaleConnected(false);

    source.addEventListener("heartbeat", (event) => {
      try {
        const status = JSON.parse(event.data);
        setScaleConnected(!!status.connected);
      } catch {
        // malformed heartbeat payload — leave connected state as-is
      }
    });

    source.onmessage = (event) => {
      setScaleConnected(true);
      try {
        const data = JSON.parse(event.data);
        const raw = data?.weight !== undefined ? data.weight : (typeof data === "number" ? data : null);
        if (raw !== null) {
          const w = normalizeWeight(raw);
          if (w !== null && w !== bufferRef.current) {
            bufferRef.current = w;
            setCapturedWeight(w);
          }
        }
      } catch {
        const w = normalizeWeight(event.data);
        if (w !== null && w !== bufferRef.current) {
          bufferRef.current = w;
          setCapturedWeight(w);
        }
      }
    };

    return () => source.close();
  }, [hwConfig.scaleStreamUrl, isSecondWeigh]);

  useEffect(() => {
    const requiredCycles = hwConfig.scaleStabilityThreshold || 5;
    const id = setInterval(() => {
      const curr = bufferRef.current;
      if (curr == null) return;
      if (curr === lastStableRef.current) {
        stabilityCounter.current++;
        if (stabilityCounter.current >= requiredCycles) setIsStable(true);
      } else {
        lastStableRef.current = curr;
        stabilityCounter.current = 1;
        setIsStable(false);
      }
    }, 800);
    return () => clearInterval(id);
  }, [hwConfig.scaleStabilityThreshold]);

  const debouncedP    = useDebounce(q => { if (q) searchProducts(q).then(setProducts); }, 400);
  const setField      = (k, v) => setForm(p => ({ ...p, [k]: v }));
  const effectiveWeight = weightMode === "manual" ? Number(manualWeight) || 0 : capturedWeight;

  // ── Submit ────────────────────────────────────────────────────────────────
  const handleCapture = async () => {
    setApiError(null);
    if (effectiveWeight <= 0)          { message.error("Weight must be greater than 0"); return; }
    if (!form.noPlate?.trim())         { message.error("Vehicle plate is required");     return; }
    if (!form.weighBridgeID)           { message.error("Weighbridge not resolved");      return; }
    if (!form.transporterName?.trim()) { message.error("Transporter is required");       return; }

    setSubmitting(true);

    try {
      let result;

      if (isSecondWeigh) {
        // ── Second weight: PATCH existing transaction ────────────────────────
        const txnId = existingTransaction.id ?? existingTransaction.ticketID;
        const patchPayload = {
          secondWeight: String(effectiveWeight),
          weighMode:    "Kiosk",
          isCompleted:  true,
          status:       "Complete",
          notes:        form.notes?.trim() || "Unmanned — second weight",
        };
        if (isValidGuid(form.driverID)) patchPayload.driverID = form.driverID;
        if (form.driverName?.trim())    patchPayload.driverName = form.driverName.trim();

        result = await patchTransaction(txnId, patchPayload);
        message.success(
          `Transaction completed! Net weight: ${effectiveWeight - (existingTransaction.firstWeight ?? 0)} kg`,
          4,
        );
      } else {
        // ── First weight: POST new transaction ───────────────────────────────
        const payload = {
          noPlate:      form.noPlate.toUpperCase().trim(),
          firstWeight:  String(effectiveWeight),
          weighMode:    "Kiosk",
          operation:    form.operation,
          operatorName: form.operatorName || "Unmanned",
        };

        if (isValidGuid(form.vehicleID))  payload.vehicleID  = form.vehicleID;
        if (isValidGuid(form.driverID))   payload.driverID   = form.driverID;
        if (isValidGuid(form.operatorID)) payload.operatorID = form.operatorID;
        if (form.driverName?.trim())      payload.driverName = form.driverName.trim();

        if (isValidGuid(form.transporterID)) {
          payload.transporterID   = form.transporterID;
          payload.transporterName = form.transporterName;
        } else if (form.transporterName?.trim()) {
          payload.transporterName = form.transporterName.trim();
        }

        if (isValidGuid(form.weighBridgeID)) {
          payload.weighBridgeID   = form.weighBridgeID;
          payload.weighBridgeName = form.weighBridgeName;
          payload.scaleName       = form.scaleName;
        }

        if (isValidGuid(form.supplierID)) {
          payload.supplierID   = form.supplierID;
          payload.supplierName = form.supplierName;
        } else if (form.supplierName?.trim()) {
          payload.supplierName = form.supplierName.trim();
        }

        if (isValidGuid(form.commodityID)) {
          payload.commodityID   = form.commodityID;
          payload.commodityName = form.commodityName;
        }
        if (form.customerName?.trim())    payload.customerName    = form.customerName.trim();
        if (form.originName?.trim())      payload.originName      = form.originName.trim();
        if (form.destinationName?.trim()) payload.destinationName = form.destinationName.trim();
        payload.notes = form.notes?.trim() || "Unmanned transaction";

        result = await postTransaction(payload);
        message.success(
          `Saved! Receipt: ${result?.data?.receiptNo ?? result?.receiptNo ?? result?.ticketID ?? "Generated"}`,
          4,
        );
      }

      onWeighingComplete?.({ ...form, ...result, weight: effectiveWeight, weightMode });
    } catch (e) {
      const msg = e.message || "Failed to save transaction";
      setApiError(msg);
      message.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  // ── Shared classnames (mirrors VehicleDetectionScreen.jsx conventions) ─────
  const cardCls   = isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200";
  const cardHdrCls = isDark
    ? "bg-gray-800 border-gray-700"
    : "bg-gradient-to-b from-amber-50 to-amber-50 border-amber-100";
  const inputCls = `w-full px-3 py-1.5 rounded-lg border text-sm outline-none transition-colors focus:border-amber-500 focus:ring-2 focus:ring-amber-500/20 ${
    isDark ? "bg-gray-800 border-gray-700 text-white placeholder:text-gray-600" : "bg-white border-gray-200 text-gray-900 placeholder:text-gray-400"
  }`;
  const lockedFieldCls = isDark
    ? "bg-gray-800 border-amber-800/60"
    : "bg-amber-50 border-amber-200";

  const scaleStatusLabel = !scaleConnected ? "No Signal" : isStable ? "Stable" : "Live";
  const scaleStatusDot   = !scaleConnected ? "bg-red-500" : isStable ? "bg-green-500" : "bg-amber-500 animate-pulse";

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className={`min-h-screen flex flex-col ${isDark ? "bg-gray-950" : "bg-slate-100"}`}>

      {/* ── HEADER ─────────────────────────────────────────────────────────── */}
      <header className={`shrink-0 px-6 py-3 flex items-center justify-between border-b shadow-sm ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"}`}>
        <div className="flex items-center gap-3">
          <img src={logo} alt="Qalitrack" className="h-12 w-auto" />
          <div className={`w-px h-7 ${isDark ? "bg-gray-700" : "bg-gray-200"}`} />
          <div>
            <h1 className={`text-base font-bold ${isDark ? "text-white" : "text-gray-900"}`}>
              {isSecondWeigh ? "Second (Tare) Weight" : "Self-Service Weighing"}
            </h1>
            <p className={`text-xs ${isDark ? "text-gray-500" : "text-gray-400"}`}>
              {isSecondWeigh
                ? `Completing transaction · First weight: ${existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"} kg`
                : "RFID verified · NFC authenticated · Unmanned mode"
              }
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          {form.noPlate    && <Pill isDark={isDark} tone="green">✓ {form.noPlate}</Pill>}
          {form.driverName && <Pill isDark={isDark} tone="amber">✓ {form.driverName}</Pill>}
          {isSecondWeigh && <Pill isDark={isDark} tone="amber">⚠️ 2nd Weight · 1st: {existingTransaction?.firstWeight ?? "—"} kg</Pill>}
          <Pill isDark={isDark} tone="amber">🔒 {form.weighBridgeName}</Pill>
          <span className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-bold border ${
            !scaleConnected
              ? isDark ? "bg-red-900/30 border-red-800 text-red-400" : "bg-red-50 border-red-200 text-red-600"
              : isDark ? "bg-green-900/30 border-green-800 text-green-400" : "bg-green-50 border-green-200 text-green-700"
          }`}>
            <span className={`w-1.5 h-1.5 rounded-full ${scaleStatusDot}`} /> {scaleStatusLabel}
          </span>
        </div>
      </header>

      {/* ── SECOND WEIGHT BANNER ────────────────────────────────────────────── */}
      {isSecondWeigh && (
        <div className={`shrink-0 px-6 py-3 flex items-center gap-4 border-b-2 ${isDark ? "bg-amber-950/30 border-amber-800" : "bg-gradient-to-r from-amber-50 to-orange-50 border-amber-300"}`}>
          <span className="text-2xl">⚖️</span>
          <div>
            <p className={`text-sm font-black ${isDark ? "text-amber-400" : "text-amber-700"}`}>
              SECOND WEIGHING MODE — Completing Existing Transaction
            </p>
            <p className={`text-xs ${isDark ? "text-amber-300/80" : "text-amber-800"}`}>
              Ticket: <strong>{existingTransaction?.ticketID ?? existingTransaction?.id ?? "—"}</strong>
              {" · "}
              First weight (gross): <strong>{existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"} kg</strong>
              {" · "}
              Captured: <strong>{existingTransaction?.createdAt ? new Date(existingTransaction.createdAt).toLocaleString() : "—"}</strong>
            </p>
          </div>
        </div>
      )}

      {/* ── BODY ───────────────────────────────────────────────────────────── */}
      <div className="flex-1 overflow-auto px-6 py-5 flex flex-col gap-5">
        <div className="grid grid-cols-12 gap-4">

          {/* ── Vehicle Card ─────────────────────────────────────────────── */}
          <div className={`col-span-4 rounded-2xl border overflow-hidden ${cardCls}`}>
            <div className={`px-4 py-2.5 flex items-center justify-between border-b ${cardHdrCls}`}>
              <p className={`text-xs font-bold uppercase tracking-wide ${isDark ? "text-amber-400" : "text-amber-700"}`}>Vehicle</p>
              <Pill isDark={isDark} tone="green">✓ RFID Verified</Pill>
            </div>
            <div className="p-4 space-y-3">
              <div className={`rounded-xl py-3 text-center border-2 ${isDark ? "bg-amber-950/20 border-amber-800/60" : "bg-gradient-to-r from-amber-50 to-orange-50 border-amber-300"}`}>
                <p className={`text-xs font-semibold uppercase tracking-wider mb-0.5 ${isDark ? "text-amber-400" : "text-amber-700"}`}>Registration</p>
                <p className={`text-3xl font-black tracking-widest ${isDark ? "text-white" : "text-gray-900"}`}>{form.noPlate || "—"}</p>
                {form.rfidTag && <p className={`font-mono text-xs mt-1 ${isDark ? "text-gray-500" : "text-gray-400"}`}>{form.rfidTag}</p>}
              </div>

              <div className="grid grid-cols-2 gap-2">
                {[
                  { label: "Type",     value: form.vehicleType  },
                  { label: "Make",     value: form.vehicleMake  },
                  { label: "Model",    value: form.vehicleModel },
                  { label: "Capacity", value: form.capacity ? `${form.capacity} kg` : null },
                ].filter(f => f.value).map(f => <InfoChip key={f.label} label={f.label} value={f.value} isDark={isDark} />)}
              </div>

              <ReadOnlyField label="Owner"       value={form.ownerId}         icon="👤" fromRfid isDark={isDark} />
              <ReadOnlyField label="Transporter" value={form.transporterName} icon="🚛" fromRfid required isDark={isDark} />
              <ReadOnlyField label="Supplier"    value={form.supplierName}    icon="🏭" fromRfid isDark={isDark} />
              {form.saccoName && <ReadOnlyField label="SACCO" value={form.saccoName} icon="🤝" fromRfid isDark={isDark} />}
            </div>
          </div>

          {/* ── Driver Card ──────────────────────────────────────────────── */}
          <div className={`col-span-3 rounded-2xl border overflow-hidden ${cardCls}`}>
            <div className={`px-4 py-2.5 flex items-center justify-between border-b ${cardHdrCls}`}>
              <p className={`text-xs font-bold uppercase tracking-wide ${isDark ? "text-amber-400" : "text-amber-700"}`}>Driver / Operator</p>
              <Pill isDark={isDark} tone="blue">✓ NFC Auth</Pill>
            </div>
            <div className="p-4 space-y-3">
              {form.driverName ? (
                <>
                  <div className="flex items-center gap-3">
                    <div className={`w-12 h-12 rounded-full flex items-center justify-center text-2xl flex-shrink-0 border-2 ${isDark ? "bg-blue-950/40 border-blue-800" : "bg-gradient-to-br from-blue-100 to-blue-50 border-blue-200"}`}>
                      👤
                    </div>
                    <div>
                      <p className={`font-bold text-sm ${isDark ? "text-white" : "text-gray-900"}`}>{form.driverName}</p>
                      {form.employeeId && <p className={`text-xs font-mono ${isDark ? "text-gray-500" : "text-gray-500"}`}>ID: {form.employeeId}</p>}
                    </div>
                  </div>
                  <div className={`rounded-lg px-3 py-2 flex items-center gap-2 border ${isDark ? "bg-amber-950/20 border-amber-800/60" : "bg-amber-50 border-amber-300"}`}>
                    <span>🔒</span>
                    <div>
                      <p className={`text-xs font-bold ${isDark ? "text-amber-400" : "text-amber-700"}`}>Operator (locked)</p>
                      <p className={`text-xs font-semibold ${isDark ? "text-white" : "text-gray-900"}`}>{form.operatorName}</p>
                    </div>
                  </div>
                  {form.driverPhone && <DRow label="Phone"   value={form.driverPhone} isDark={isDark} />}
                  {form.licenseNo   && <DRow label="Licence" value={form.licenseNo}   isDark={isDark} />}
                  {form.nfcUid      && <DRow label="NFC UID" value={form.nfcUid} mono isDark={isDark} />}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-8 text-center">
                  <span className="text-4xl mb-2">👤</span>
                  <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>No driver authenticated</p>
                </div>
              )}

              {/* Show first weight card in second-weigh mode */}
              {isSecondWeigh && (
                <div className={`rounded-xl p-3 mt-2 border ${isDark ? "bg-amber-950/20 border-amber-800/60" : "bg-gradient-to-r from-amber-50 to-orange-50 border-amber-300"}`}>
                  <p className={`text-xs font-bold uppercase mb-1 ${isDark ? "text-amber-400" : "text-amber-700"}`}>First Weight (Gross)</p>
                  <p className={`text-2xl font-black font-mono ${isDark ? "text-white" : "text-gray-900"}`}>
                    {existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"} <span className={`text-sm font-normal ${isDark ? "text-gray-600" : "text-gray-400"}`}>kg</span>
                  </p>
                  <p className={`text-xs mt-1 ${isDark ? "text-amber-300/80" : "text-amber-800"}`}>
                    Captured: {existingTransaction?.createdAt ? new Date(existingTransaction.createdAt).toLocaleString() : "—"}
                  </p>
                </div>
              )}
            </div>
          </div>

          {/* ── Weight Card ──────────────────────────────────────────────── */}
          <div className={`col-span-5 flex flex-col rounded-2xl border overflow-hidden ${cardCls}`}>
            <div className={`px-4 py-2.5 flex items-center justify-between border-b ${cardHdrCls}`}>
              <p className={`text-xs font-bold uppercase tracking-wide ${isDark ? "text-amber-400" : "text-amber-700"}`}>
                {isSecondWeigh ? "Second (Tare) Weight" : "Weight Reading"}
              </p>
              <div className={`flex rounded-lg overflow-hidden border ${isDark ? "border-gray-700" : "border-gray-200"}`}>
                {["captured", "manual"].map(m => (
                  <button key={m} onClick={() => setWeightMode(m)}
                    className={`px-3 py-0.5 text-xs font-semibold transition-all ${
                      weightMode === m
                        ? "bg-amber-500 text-white"
                        : isDark ? "bg-gray-800 text-gray-400" : "bg-white text-gray-500"
                    }`}>
                    {m === "captured" ? "⚡ Captured" : "✏️ Manual"}
                  </button>
                ))}
              </div>
            </div>
            <div className="flex-1 flex flex-col items-center justify-center py-6 gap-2">
              {weightMode === "captured" ? (
                <>
                  {!scaleConnected && (
                    <p className={`text-xs font-semibold mb-1 ${isDark ? "text-red-400" : "text-red-600"}`}>
                      ⚠ No signal from scale — check hardware connection
                    </p>
                  )}
                  <p className={`text-6xl font-mono font-bold ${!scaleConnected ? (isDark ? "text-gray-700" : "text-gray-300") : isStable ? "text-green-600" : "text-amber-500"}`}>
                    {capturedWeight.toLocaleString()}
                  </p>
                  <p className={`text-sm font-semibold ${isDark ? "text-gray-600" : "text-gray-400"}`}>KG</p>
                  <span className={`px-3 py-0.5 rounded-full text-xs font-bold border ${
                    isStable
                      ? isDark ? "bg-green-900/30 border-green-800 text-green-400" : "bg-green-50 border-green-200 text-green-700"
                      : isDark ? "bg-amber-950/30 border-amber-800 text-amber-400" : "bg-amber-50 border-amber-300 text-amber-700"
                  }`}>
                    {isStable ? "● Stable" : "● Stabilising…"}
                  </span>

                  {/* Net weight preview in second-weigh mode */}
                  {isSecondWeigh && capturedWeight > 0 && (existingTransaction?.firstWeight ?? 0) > 0 && (
                    <div className={`mt-3 px-4 py-2 rounded-xl border ${isDark ? "bg-green-900/20 border-green-800" : "bg-green-50 border-green-200"}`}>
                      <p className={`text-xs font-bold uppercase ${isDark ? "text-green-400" : "text-green-800"}`}>Net Weight Preview</p>
                      <p className={`text-xl font-black font-mono ${isDark ? "text-green-400" : "text-green-600"}`}>
                        {Math.abs((existingTransaction?.firstWeight ?? 0) - capturedWeight).toLocaleString()} kg
                      </p>
                    </div>
                  )}

                  <div className="mt-2 flex gap-2 flex-wrap justify-center">
                    <span className={`text-xs px-2 py-1 rounded-lg font-semibold border ${isDark ? "bg-amber-950/20 text-amber-400 border-amber-800/60" : "bg-amber-50 text-amber-700 border-amber-300"}`}>
                      🔒 {form.weighBridgeName || "Factory A"}
                    </span>
                    <span className={`text-xs px-2 py-1 rounded-lg font-semibold border ${isDark ? "bg-green-900/20 text-green-400 border-green-800" : "bg-green-50 text-green-800 border-green-200"}`}>
                      Mode: Unmanned
                    </span>
                  </div>
                </>
              ) : (
                <>
                  <input type="number" placeholder="0" value={manualWeight}
                    onChange={e => setManualWeight(e.target.value)}
                    className={`${inputCls} w-[180px] text-center`}
                    style={{ fontSize: "32px" }} />
                  <p className={`text-sm font-semibold ${isDark ? "text-gray-600" : "text-gray-400"}`}>KG (manual)</p>
                </>
              )}
            </div>
          </div>
        </div>

        {/* ── Transaction Details ──────────────────────────────────────────── */}
        <div className={`rounded-2xl border overflow-hidden ${cardCls}`}>
          <div className={`px-4 py-2.5 flex items-center justify-between border-b ${cardHdrCls}`}>
            <div className="flex items-center gap-2">
              <div className="w-1 h-5 rounded-full bg-gradient-to-b from-amber-600 to-amber-400" />
              <p className={`text-sm font-bold ${isDark ? "text-white" : "text-gray-900"}`}>
                {isSecondWeigh ? "Transaction Details (from first weighing)" : "Transaction Details"}
              </p>
            </div>
            <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>
              {isSecondWeigh
                ? "Pre-filled from original transaction — update if needed"
                : "🔒 = auto-filled from RFID · remaining fields are optional"
              }
            </p>
          </div>
          <div className="p-5">
            <div className="grid grid-cols-4 gap-4 mb-4">
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Commodity</label>
                <Select
                  showSearch placeholder="Search commodity…"
                  onSearch={debouncedP}
                  onChange={id => {
                    const item = products.find(i => i.id === id);
                    setForm(p => ({ ...p, commodityID: id, commodityName: item?.name ?? "" }));
                  }}
                  value={form.commodityID || undefined}
                  style={{ width: "100%" }} allowClear>
                  {products.map(i => <Option key={i.id} value={i.id}>{i.name}</Option>)}
                </Select>
              </div>
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Customer Name</label>
                <input placeholder="Customer name" value={form.customerName}
                  onChange={e => setField("customerName", e.target.value)}
                  className={inputCls} />
              </div>
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Origin</label>
                <input placeholder="Origin" value={form.originName}
                  onChange={e => setField("originName", e.target.value)}
                  className={inputCls} />
              </div>
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Destination</label>
                <input placeholder="Destination" value={form.destinationName}
                  onChange={e => setField("destinationName", e.target.value)}
                  className={inputCls} />
              </div>
            </div>

            <div className="grid grid-cols-4 gap-4">
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-amber-400" : "text-amber-700"}`}>
                  Weighbridge {isSecondWeigh ? "(locked to original)" : <span className="text-red-500">*</span>}
                </label>
                {isSecondWeigh ? (
                  <div className={`rounded-lg px-3 py-2 flex items-center gap-2 h-9 border ${lockedFieldCls}`}>
                    <span className="text-xs">🔒</span>
                    <span className={`text-sm font-semibold ${isDark ? "text-white" : "text-gray-900"}`}>{form.weighBridgeName}</span>
                  </div>
                ) : (
                  <Select value={form.weighBridgeName || undefined}
                    onChange={v => {
                      const wb = weighbridges.find(w => (w.location ?? w.name) === v);
                      setForm(p => ({ ...p, weighBridgeID: wb?.id || null, weighBridgeName: v, scaleName: v }));
                    }}
                    style={{ width: "100%" }} placeholder="Select weighbridge…">
                    {weighbridges.map(wb => <Option key={wb.id} value={wb.location ?? wb.name}>{wb.location ?? wb.name}</Option>)}
                  </Select>
                )}
              </div>
              <div>
                <label className="block text-xs font-semibold mb-1 text-amber-600">Weigh Mode (locked)</label>
                <div className={`rounded-lg px-3 py-2 flex items-center gap-2 h-9 border ${lockedFieldCls}`}>
                  <span className="text-xs">🔒</span>
                  <span className={`text-sm font-semibold ${isDark ? "text-white" : "text-gray-900"}`}>Unmanned</span>
                </div>
              </div>
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Operation</label>
                {isSecondWeigh ? (
                  <div className={`rounded-lg px-3 py-2 flex items-center gap-2 h-9 border ${isDark ? "bg-gray-800 border-gray-700" : "bg-gray-50 border-gray-200"}`}>
                    <span className={`text-sm font-semibold ${isDark ? "text-gray-300" : "text-gray-700"}`}>{form.operation}</span>
                  </div>
                ) : (
                  <Select value={form.operation} onChange={v => setField("operation", v)} style={{ width: "100%" }}>
                    <Option value="weighing">Weighing</Option>
                    <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                    <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                  </Select>
                )}
              </div>
              <div>
                <label className={`block text-xs font-semibold mb-1 ${isDark ? "text-gray-500" : "text-gray-500"}`}>Notes</label>
                <input placeholder="Additional notes…" value={form.notes}
                  onChange={e => setField("notes", e.target.value)}
                  className={inputCls} />
              </div>
            </div>
          </div>
        </div>

        {/* ── Error Banner ─────────────────────────────────────────────────── */}
        {(apiError || parentError) && (
          <div className={`rounded-xl px-4 py-3 flex items-center gap-3 border ${isDark ? "bg-red-950/30 border-red-900" : "bg-red-50 border-red-200"}`}>
            <span className="text-lg">⚠️</span>
            <p className={`text-sm font-medium ${isDark ? "text-red-400" : "text-red-600"}`}>{apiError || parentError}</p>
          </div>
        )}
      </div>

      {/* ── FOOTER ─────────────────────────────────────────────────────────── */}
      <footer className={`shrink-0 px-6 py-4 flex items-center justify-between border-t ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"}`}>
        <div className="flex items-center gap-4">
          <button onClick={onBack}
            className={`px-4 py-2 rounded-lg text-sm font-semibold border ${isDark ? "bg-gray-800 text-gray-300 border-gray-700" : "bg-gray-100 text-gray-700 border-gray-200"}`}>
            ← Back
          </button>
          <div className={`text-sm ${isDark ? "text-gray-500" : "text-gray-500"}`}>
            {isSecondWeigh ? "Tare weight: " : "Weight: "}
            <span className={`font-bold ${isDark ? "text-white" : "text-gray-900"}`}>{effectiveWeight.toLocaleString()} kg</span>
            {isSecondWeigh && (existingTransaction?.firstWeight ?? 0) > 0 && effectiveWeight > 0 && (
              <span className="ml-2 text-xs text-green-600 font-semibold">
                Net: {Math.abs((existingTransaction?.firstWeight ?? 0) - effectiveWeight).toLocaleString()} kg
              </span>
            )}
          </div>
        </div>
        <button
          onClick={handleCapture}
          disabled={submitting || (weightMode === "captured" && !isStable) || effectiveWeight <= 0}
          className={`px-8 py-2.5 rounded-xl text-sm font-bold text-white transition-all ${
            submitting || (weightMode === "captured" && !isStable) || effectiveWeight <= 0
              ? "bg-gray-400 cursor-not-allowed"
              : "bg-gradient-to-r from-amber-600 to-amber-500 hover:from-amber-700 hover:to-amber-600 shadow-lg shadow-amber-600/30 cursor-pointer"
          }`}>
          {submitting
            ? "Saving…"
            : isSecondWeigh
            ? "⚡ Capture Tare Weight"
            : "⚡ Capture Weight"
          }
        </button>
      </footer>
    </div>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────
function Pill({ tone, isDark, children }) {
  const tones = {
    green: isDark ? "bg-green-900/30 border-green-800 text-green-400" : "bg-green-50 border-green-200 text-green-700",
    amber: isDark ? "bg-amber-950/30 border-amber-800 text-amber-400" : "bg-amber-50 border-amber-300 text-amber-700",
    blue:  isDark ? "bg-blue-950/30 border-blue-800 text-blue-400"   : "bg-blue-50 border-blue-200 text-blue-700",
  };
  return (
    <span className={`px-2.5 py-0.5 rounded-full text-xs font-bold border ${tones[tone]}`}>
      {children}
    </span>
  );
}

function InfoChip({ label, value, isDark }) {
  return (
    <div className={`rounded-lg p-2 border ${isDark ? "bg-gray-800 border-gray-700" : "bg-gray-50 border-gray-100"}`}>
      <p className={`text-xs font-bold uppercase tracking-wider mb-0.5 ${isDark ? "text-gray-600" : "text-gray-400"}`}>{label}</p>
      <p className={`text-xs font-bold truncate ${isDark ? "text-gray-200" : "text-gray-700"}`}>{value}</p>
    </div>
  );
}

function ReadOnlyField({ label, value, icon, fromRfid, required, isDark }) {
  const hasValue = Boolean(value && String(value).trim().length > 0);
  return (
    <div>
      <div className="flex items-center gap-1 mb-1">
        <p className={`text-xs font-bold uppercase tracking-wider ${isDark ? "text-amber-400" : "text-amber-700"}`}>
          {label}{required && <span className="text-red-500">*</span>}
        </p>
        {fromRfid && (
          <span className={`text-xs px-1.5 py-0 rounded font-semibold border ${isDark ? "bg-green-900/30 text-green-400 border-green-800" : "bg-green-50 text-green-700 border-green-200"}`}>
            RFID
          </span>
        )}
      </div>
      <div className={`rounded-lg px-3 py-2 flex items-center gap-2 border ${
        hasValue
          ? isDark ? "bg-amber-950/20 border-amber-800/60" : "bg-amber-50 border-amber-300"
          : isDark ? "bg-gray-800 border-gray-700" : "bg-gray-50 border-gray-100"
      }`}>
        <span className="text-base">{icon}</span>
        <span className={`text-sm font-semibold ${hasValue ? (isDark ? "text-white" : "text-gray-900") : (isDark ? "text-gray-600" : "text-gray-400")}`}>
          {hasValue ? value : "Not provided"}
        </span>
        {hasValue && <span className={`ml-auto text-xs ${isDark ? "text-amber-400" : "text-amber-600"}`}>🔒</span>}
      </div>
    </div>
  );
}

function DRow({ label, value, mono, isDark }) {
  return (
    <div className={`flex items-center justify-between py-1.5 px-2 rounded-lg ${isDark ? "bg-gray-800" : "bg-gray-50"}`}>
      <span className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>{label}</span>
      <span className={`text-xs font-semibold ${mono ? "font-mono" : ""} ${isDark ? "text-gray-300" : "text-gray-700"}`}>{value}</span>
    </div>
  );
}
