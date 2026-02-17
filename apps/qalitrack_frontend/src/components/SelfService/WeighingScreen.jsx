/**
 * WeighingScreen.jsx — Self-Service Kiosk Weighing
 *
 * Receives: vehicleData = { vehicle: <normalised by VehicleDetectionScreen>, driver: {...} }
 *
 * Field chain (Swagger → normaliseVehicle → here):
 *   registrationNumber → noPlate
 *   ownerId          → ownerId          (direct)
 *   transporterId      → transporterID
 *   transporterName    → transporterName    (direct, with saccoName fallback)
 *   supplierId         → supplierID
 *   supplierName       → supplierName       (direct)
 *   saccoName          → saccoName          (tea-industry transporter alias)
 *   rfiDcode           → rfidTag
 *   netWeightCapacity  → capacity
 *   type               → vehicleType
 */

import React, { useEffect, useState, useRef, useCallback } from "react";
import { message, Select } from "antd";
const { Option } = Select;

const BASE_URL = import.meta.env.VITE_API_URL || "/api";

// ── Auth token ────────────────────────────────────────────────────────────────
function getKioskToken() {
  try {
    const s1 = sessionStorage.getItem("authSession");
    if (s1) { const p = JSON.parse(s1); const t = p?.token ?? p?.accessToken ?? p?.access_token ?? p?.userData?.token; if (t) return t; }
    const s2 = sessionStorage.getItem("user");
    if (s2) { const p = JSON.parse(s2); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
    const l1 = localStorage.getItem("token");      if (l1) return l1;
    const l2 = localStorage.getItem("authToken");  if (l2) return l2;
    const l3 = localStorage.getItem("authSession");
    if (l3) { const p = JSON.parse(l3); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
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
const postTransaction = b => apiFetch(`/Transaction`, { method: "POST", body: JSON.stringify({ request: b }) });

// ── Debounce ──────────────────────────────────────────────────────────────────
function useDebounce(fn, delay) {
  const t = useRef(null);
  return useCallback((...args) => {
    if (t.current) clearTimeout(t.current);
    t.current = setTimeout(() => fn(...args), delay);
  }, [fn, delay]);
}

// ── extractVehicleFields ──────────────────────────────────────────────────────
// vehicleData is { vehicle: <normalised>, driver: {...} } from VehicleDetectionScreen.
// The normalised vehicle uses exact Swagger field names + saccoName from enrichment.
const extractVehicleFields = (vehicleData = {}) => {
  const v = vehicleData?.vehicle ?? vehicleData ?? {};

  const fields = {
    vehicleID:       v.id                  ?? null,
    noPlate:         v.registrationNumber  ?? "",
    rfidTag:         v.rfidTag             ?? "",
    vehicleType:     v.vehicleType         ?? "",
    vehicleMake:     v.vehicleMake         ?? "",
    vehicleModel:    v.vehicleModel        ?? "",
    capacity:        v.capacity            ?? "",
    ownerId:       v.ownerId           ?? "",
    transporterID:   v.transporterId       ?? null,           // Swagger: transporterId
    transporterName: v.transporterName     ?? v.saccoName ?? "", // saccoName fallback
    supplierID:      v.supplierId          ?? null,           // Swagger: supplierId
    supplierName:    v.supplierName        ?? "",
    saccoName:       v.saccoName           ?? "",
    commodityID:     null,
    commodityName:   "",
    customerName:    "",
  };

  console.group("📋 extractVehicleFields");
  console.log("noPlate:         ", fields.noPlate);
  console.log("ownerId:       ", fields.ownerId);
  console.log("transporterName: ", fields.transporterName, "| ID:", fields.transporterID);
  console.log("supplierName:    ", fields.supplierName,    "| ID:", fields.supplierID);
  console.log("saccoName:       ", fields.saccoName);
  console.groupEnd();

  return fields;
};

// ── extractDriverFields ───────────────────────────────────────────────────────
// Driver is nested inside vehicleData.driver (set by VehicleDetectionScreen).
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

// ── Helpers ───────────────────────────────────────────────────────────────────
const isValidGuid = g => g && g !== "00000000-0000-0000-0000-000000000000" && String(g).length > 10;

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function WeighingScreen({ vehicleData = {}, driverData = {}, onWeighingComplete, onBack, error: parentError }) {
  const vf = extractVehicleFields(vehicleData);
  const df = extractDriverFields(vehicleData, driverData);

  const [showInspector, setShowInspector] = useState(false);
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
    ownerId:       vf.ownerId,
    transporterID:   vf.transporterID,
    transporterName: vf.transporterName,
    supplierID:      vf.supplierID,
    supplierName:    vf.supplierName,
    saccoName:       vf.saccoName,
    commodityID:     null,
    commodityName:   "",
    customerName:    "",
    driverID:        df.driverID,
    driverName:      df.driverName,
    driverPhone:     df.driverPhone,
    licenseNo:       df.licenseNo,
    employeeId:      df.employeeId,
    nfcUid:          df.nfcUid,
    operatorID:      df.driverID   || null,
    operatorName:    df.driverName || "Self-Service Kiosk",
    weighMode:       "Gross/Tare",
    weighBridgeID:   null,
    weighBridgeName: "Factory A",
    scaleName:       "Factory A",
    originName:      "",
    destinationName: "",
    operation:       "weighing",
    notes:           "",
  });

  // Re-sync when vehicleData updates (e.g. enrichment completes after rescan)
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
      ownerId:       newVf.ownerId,
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
      operatorName:    newDf.driverName || "Self-Service Kiosk",
    }));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleData, driverData]);

  // ── Weight simulation (replace with real scale SSE) ──────────────────────
  const [weightMode,     setWeightMode]     = useState("captured");
  const [capturedWeight, setCapturedWeight] = useState(0);
  const [manualWeight,   setManualWeight]   = useState("");
  const [isStable,       setIsStable]       = useState(false);
  const [submitting,     setSubmitting]     = useState(false);
  const [apiError,       setApiError]       = useState(null);
  const bufferRef        = useRef(null);
  const lastStableRef    = useRef(null);
  const stabilityCounter = useRef(0);

  // Fetch weighbridges on mount
  useEffect(() => {
    getWeighbridges().then(list => {
      setWeighbridges(list);
      const factoryA = list.find(wb => /factory\s*a/i.test(wb.location ?? wb.name ?? ""));
      if (factoryA) setForm(p => ({
        ...p,
        weighBridgeID:   factoryA.id,
        weighBridgeName: factoryA.location ?? factoryA.name,
        scaleName:       factoryA.location ?? factoryA.name,
      }));
    }).catch(() => {});
  }, []);

  // Simulate scale readings (replace with real WebSocket/SSE from scale hardware)
  useEffect(() => {
    const BASE = 19011; let iter = 0;
    const id = setInterval(() => {
      iter++;
      const w = iter < 6 ? BASE + Math.floor(Math.random() * 40 - 20) : BASE;
      bufferRef.current = w; setCapturedWeight(w);
    }, 1200);
    return () => clearInterval(id);
  }, []);

  // Stability detection
  useEffect(() => {
    const id = setInterval(() => {
      const curr = bufferRef.current;
      if (curr == null) return;
      if (curr === lastStableRef.current) {
        stabilityCounter.current++;
        if (stabilityCounter.current >= 5) setIsStable(true);
      } else {
        lastStableRef.current = curr;
        stabilityCounter.current = 1;
        setIsStable(false);
      }
    }, 800);
    return () => clearInterval(id);
  }, []);

  const debouncedP    = useDebounce(q => { if (q) searchProducts(q).then(setProducts); }, 400);
  const setField      = (k, v) => setForm(p => ({ ...p, [k]: v }));
  const effectiveWeight = weightMode === "manual" ? Number(manualWeight) || 0 : capturedWeight;

  // ── Submit ────────────────────────────────────────────────────────────────
  const handleCapture = async () => {
    setApiError(null);
    if (effectiveWeight <= 0)          { message.error("Weight must be greater than 0"); return; }
    if (!form.noPlate?.trim())         { message.error("Vehicle plate is required");     return; }
    if (!form.weighBridgeID)           { message.error("Weighbridge not resolved — ensure 'Factory A' exists"); return; }
    if (!form.transporterName?.trim()) { message.error("Transporter is required");       return; }

    setSubmitting(true);
    const payload = {
      noPlate:      form.noPlate.toUpperCase().trim(),
      firstWeight:  String(effectiveWeight),
      weighMode:    "Gross/Tare",
      operation:    form.operation,
      operatorName: form.operatorName || "Self-Service Kiosk",
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
    payload.notes = form.notes?.trim() || "Self-service kiosk transaction";

    console.log("📤 Kiosk transaction payload:", payload);
    try {
      const result = await postTransaction(payload);
      message.success(
        `Saved! Receipt: ${result?.data?.receiptNo ?? result?.receiptNo ?? result?.ticketID ?? "Generated"}`,
        4,
      );
      onWeighingComplete?.({ ...payload, ...result, weight: effectiveWeight, weightMode });
    } catch (e) {
      const msg = e.message || "Failed to save transaction";
      setApiError(msg); message.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  // ── Styles ────────────────────────────────────────────────────────────────
  const card    = { background: "#fff", border: "1px solid #e5e7eb", borderRadius: "14px", boxShadow: "0 1px 4px rgba(0,0,0,0.06)", overflow: "hidden" };
  const cardHdr = { padding: "10px 16px", background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf", display: "flex", alignItems: "center", justifyContent: "space-between" };
  const inputStyle = { width: "100%", padding: "7px 10px", borderRadius: "8px", border: "1.5px solid #e5e7eb", fontSize: "13px", color: "#111827", background: "#fff", outline: "none" };
  const onFoc = e => { e.target.style.borderColor = "#d97706"; e.target.style.boxShadow = "0 0 0 3px rgba(217,119,6,0.12)"; };
  const onBlr = e => { e.target.style.borderColor = "#e5e7eb"; e.target.style.boxShadow = "none"; };

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen flex flex-col" style={{ background: "#f8fafc" }}>

      {/* ── HEADER ─────────────────────────────────────────────────────────── */}
      <header className="shrink-0 px-6 py-3 flex items-center justify-between"
        style={{ background: "#fff", borderBottom: "1px solid #e5e7eb", boxShadow: "0 1px 3px rgba(0,0,0,0.06)" }}>
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl flex items-center justify-center"
            style={{ background: "linear-gradient(135deg,#d97706,#f59e0b)" }}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5">
              <path d="M12 2L2 7l10 5 10-5-10-5z" /><path d="M2 17l10 5 10-5" /><path d="M2 12l10 5 10-5" />
            </svg>
          </div>
          <div>
            <h1 className="text-base font-bold" style={{ color: "#111827" }}>Weighing</h1>
            <p className="text-xs" style={{ color: "#6b7280" }}>RFID verified · NFC authenticated · Kiosk mode</p>
          </div>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          {form.noPlate    && <Pill bg="#f0fdf4" border="#bbf7d0" color="#16a34a">✓ {form.noPlate}</Pill>}
          {form.driverName && <Pill bg="#eff6ff" border="#bfdbfe" color="#1d4ed8">✓ {form.driverName}</Pill>}
          <Pill bg="#fffbeb" border="#fcd34d" color="#92400e">🔒 {form.weighBridgeName}</Pill>
          <Pill bg="#dcfce7" border="#bbf7d0" color="#16a34a">● LIVE</Pill>
          <button
            onClick={() => setShowInspector(v => !v)}
            style={{ fontSize: "10px", padding: "2px 8px", borderRadius: "6px", background: showInspector ? "#fef3c7" : "#f3f4f6", border: "1px solid #e5e7eb", cursor: "pointer", color: "#6b7280" }}>
            🔍 {showInspector ? "Hide" : "Inspect"}
          </button>
        </div>
      </header>

      {/* ── DATA INSPECTOR ─────────────────────────────────────────────────── */}
      {showInspector && (
        <div style={{ background: "#0f172a", borderBottom: "2px solid #1e293b", padding: "12px 24px", fontFamily: "monospace", fontSize: "11px", color: "#94a3b8", maxHeight: "220px", overflowY: "auto" }}>
          <p style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: "6px" }}>
            🔍 Raw vehicleData vs resolved form fields
          </p>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
            <div>
              <p style={{ color: "#64748b", fontSize: "10px", marginBottom: "4px" }}>vehicleData.vehicle (from API + enrichment):</p>
              <pre style={{ color: "#86efac", fontSize: "10px", whiteSpace: "pre-wrap", wordBreak: "break-all", margin: 0 }}>
                {JSON.stringify(vehicleData?.vehicle ?? vehicleData, null, 2)}
              </pre>
            </div>
            <div>
              <p style={{ color: "#64748b", fontSize: "10px", marginBottom: "4px" }}>Resolved form values:</p>
              <pre style={{ color: "#93c5fd", fontSize: "10px", margin: 0 }}>
{JSON.stringify({
  noPlate:         form.noPlate,
  ownerId:       form.ownerId,
  transporterName: form.transporterName,
  transporterID:   form.transporterID,
  supplierName:    form.supplierName,
  supplierID:      form.supplierID,
  saccoName:       form.saccoName,
  driverName:      form.driverName,
}, null, 2)}
              </pre>
            </div>
          </div>
        </div>
      )}

      {/* ── BODY ───────────────────────────────────────────────────────────── */}
      <div className="flex-1 overflow-auto px-6 py-5 flex flex-col gap-5">
        <div className="grid grid-cols-12 gap-4">

          {/* ── Vehicle Card ─────────────────────────────────────────────── */}
          <div className="col-span-4" style={card}>
            <div style={cardHdr}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Vehicle</p>
              <Pill bg="#f0fdf4" border="#bbf7d0" color="#16a34a">✓ RFID Verified</Pill>
            </div>
            <div className="p-4 space-y-3">

              {/* Plate number */}
              <div className="rounded-xl py-3 text-center"
                style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", border: "2px solid #fcd34d" }}>
                <p className="text-xs font-semibold uppercase tracking-wider mb-0.5" style={{ color: "#92400e" }}>Registration</p>
                <p className="text-3xl font-black tracking-widest" style={{ color: "#111827" }}>{form.noPlate || "—"}</p>
                {form.rfidTag && <p className="font-mono text-xs mt-1" style={{ color: "#9ca3af" }}>{form.rfidTag}</p>}
              </div>

              {/* Vehicle attributes */}
              <div className="grid grid-cols-2 gap-2">
                {[
                  { label: "Type",     value: form.vehicleType  },
                  { label: "Make",     value: form.vehicleMake  },
                  { label: "Model",    value: form.vehicleModel },
                  { label: "Capacity", value: form.capacity ? `${form.capacity} kg` : null },
                ].filter(f => f.value).map(f => <InfoChip key={f.label} label={f.label} value={f.value} />)}
              </div>

              {/* Relational fields — auto-filled from RFID + enrichment */}
              <ReadOnlyField label="Owner"       value={form.ownerId}       icon="👤" fromRfid />
              <ReadOnlyField label="Transporter" value={form.transporterName} icon="🚛" fromRfid required />
              <ReadOnlyField label="Supplier"    value={form.supplierName}    icon="🏭" fromRfid />
              {form.saccoName && (
                <ReadOnlyField label="SACCO" value={form.saccoName} icon="🤝" fromRfid />
              )}
            </div>
          </div>

          {/* ── Driver Card ──────────────────────────────────────────────── */}
          <div className="col-span-3" style={card}>
            <div style={cardHdr}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Driver / Operator</p>
              <Pill bg="#eff6ff" border="#bfdbfe" color="#1d4ed8">✓ NFC Auth</Pill>
            </div>
            <div className="p-4 space-y-3">
              {form.driverName ? (
                <>
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-full flex items-center justify-center text-2xl flex-shrink-0"
                      style={{ background: "linear-gradient(135deg,#dbeafe,#eff6ff)", border: "2px solid #bfdbfe" }}>
                      👤
                    </div>
                    <div>
                      <p className="font-bold text-sm" style={{ color: "#111827" }}>{form.driverName}</p>
                      {form.employeeId && (
                        <p className="text-xs font-mono" style={{ color: "#6b7280" }}>ID: {form.employeeId}</p>
                      )}
                    </div>
                  </div>
                  <div className="rounded-lg px-3 py-2 flex items-center gap-2"
                    style={{ background: "#fffbeb", border: "1px solid #fcd34d" }}>
                    <span>🔒</span>
                    <div>
                      <p className="text-xs font-bold" style={{ color: "#92400e" }}>Operator (locked)</p>
                      <p className="text-xs font-semibold" style={{ color: "#111827" }}>{form.operatorName}</p>
                    </div>
                  </div>
                  {form.driverPhone && <DRow label="Phone"   value={form.driverPhone} />}
                  {form.licenseNo   && <DRow label="Licence" value={form.licenseNo}   />}
                  {form.nfcUid      && <DRow label="NFC UID" value={form.nfcUid} mono />}
                </>
              ) : (
                <div className="flex flex-col items-center justify-center py-8 text-center">
                  <span className="text-4xl mb-2">👤</span>
                  <p className="text-xs" style={{ color: "#9ca3af" }}>No driver authenticated</p>
                </div>
              )}
            </div>
          </div>

          {/* ── Weight Card ──────────────────────────────────────────────── */}
          <div className="col-span-5 flex flex-col" style={card}>
            <div style={cardHdr}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Weight Reading</p>
              <div className="flex rounded-lg overflow-hidden" style={{ border: "1.5px solid #e5e7eb" }}>
                {["captured", "manual"].map(m => (
                  <button key={m} onClick={() => setWeightMode(m)}
                    className="px-3 py-0.5 text-xs font-semibold transition-all"
                    style={{ background: weightMode === m ? "#d97706" : "#fff", color: weightMode === m ? "#fff" : "#6b7280" }}>
                    {m === "captured" ? "⚡ Captured" : "✏️ Manual"}
                  </button>
                ))}
              </div>
            </div>
            <div className="flex-1 flex flex-col items-center justify-center py-6 gap-2">
              {weightMode === "captured" ? (
                <>
                  <p className="text-6xl font-mono font-bold"
                    style={{ color: isStable ? "#16a34a" : "#d97706" }}>
                    {capturedWeight.toLocaleString()}
                  </p>
                  <p className="text-sm font-semibold" style={{ color: "#9ca3af" }}>KG</p>
                  <span className="px-3 py-0.5 rounded-full text-xs font-bold" style={{
                    background: isStable ? "#dcfce7" : "#fef3c7",
                    color:      isStable ? "#16a34a" : "#d97706",
                    border:     isStable ? "1px solid #bbf7d0" : "1px solid #fcd34d",
                  }}>
                    {isStable ? "● Stable" : "● Stabilising…"}
                  </span>
                  <div className="mt-2 flex gap-2 flex-wrap justify-center">
                    <span className="text-xs px-2 py-1 rounded-lg font-semibold"
                      style={{ background: "#fffbeb", color: "#92400e", border: "1px solid #fcd34d" }}>
                      🔒 {form.weighBridgeName || "Factory A"}
                    </span>
                    <span className="text-xs px-2 py-1 rounded-lg font-semibold"
                      style={{ background: "#f0fdf4", color: "#166534", border: "1px solid #bbf7d0" }}>
                      Mode: Kiosk
                    </span>
                  </div>
                </>
              ) : (
                <>
                  <input
                    type="number"
                    placeholder="0"
                    value={manualWeight}
                    onChange={e => setManualWeight(e.target.value)}
                    style={{ ...inputStyle, width: "180px", fontSize: "32px", textAlign: "center" }}
                    onFocus={onFoc}
                    onBlur={onBlr}
                  />
                  <p className="text-sm font-semibold" style={{ color: "#9ca3af" }}>KG (manual)</p>
                </>
              )}
            </div>
          </div>
        </div>

        {/* ── Transaction Details ──────────────────────────────────────────── */}
        <div style={card}>
          <div style={cardHdr}>
            <div className="flex items-center gap-2">
              <div className="w-1 h-5 rounded-full" style={{ background: "linear-gradient(180deg,#d97706,#f59e0b)" }} />
              <p className="text-sm font-bold" style={{ color: "#111827" }}>Transaction Details</p>
            </div>
            <p className="text-xs" style={{ color: "#9ca3af" }}>🔒 = auto-filled from RFID · remaining fields are optional</p>
          </div>
          <div className="p-5">
            <div className="grid grid-cols-4 gap-4 mb-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Commodity</label>
                <Select
                  showSearch
                  placeholder="Search commodity…"
                  onSearch={debouncedP}
                  onChange={id => {
                    const item = products.find(i => i.id === id);
                    setForm(p => ({ ...p, commodityID: id, commodityName: item?.name ?? "" }));
                  }}
                  value={form.commodityID || undefined}
                  style={{ width: "100%" }}
                  allowClear>
                  {products.map(i => <Option key={i.id} value={i.id}>{i.name}</Option>)}
                </Select>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Customer Name</label>
                <input placeholder="Customer name" value={form.customerName}
                  onChange={e => setField("customerName", e.target.value)}
                  style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Origin</label>
                <input placeholder="Origin" value={form.originName}
                  onChange={e => setField("originName", e.target.value)}
                  style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Destination</label>
                <input placeholder="Destination" value={form.destinationName}
                  onChange={e => setField("destinationName", e.target.value)}
                  style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
              </div>
            </div>

            <div className="grid grid-cols-4 gap-4">
              <div>
                <label className="block text-xs font-semibold text-amber-600 mb-1">
                  Weighbridge <span style={{ color: "#ef4444" }}>*</span>{" "}
                  <span className="font-normal text-amber-400">(default: Factory A)</span>
                </label>
                <Select
                  value={form.weighBridgeName || undefined}
                  onChange={v => {
                    const wb = weighbridges.find(w => (w.location ?? w.name) === v);
                    setForm(p => ({ ...p, weighBridgeID: wb?.id || null, weighBridgeName: v, scaleName: v }));
                  }}
                  style={{ width: "100%" }}
                  placeholder="Select weighbridge…">
                  {weighbridges.map(wb => (
                    <Option key={wb.id} value={wb.location ?? wb.name}>{wb.location ?? wb.name}</Option>
                  ))}
                </Select>
              </div>
              <div>
                <label className="block text-xs font-semibold text-amber-600 mb-1">
                  Weigh Mode <span className="font-normal text-amber-400">(locked)</span>
                </label>
                <div className="rounded-lg px-3 py-2 flex items-center gap-2 h-9"
                  style={{ background: "#fffbeb", border: "1.5px solid #fcd34d" }}>
                  <span className="text-xs">🔒</span>
                  <span className="text-sm font-semibold" style={{ color: "#111827" }}>Kiosk (Gross/Tare)</span>
                </div>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Operation</label>
                <Select value={form.operation} onChange={v => setField("operation", v)} style={{ width: "100%" }}>
                  <Option value="weighing">Weighing</Option>
                  <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                  <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                </Select>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1">Notes</label>
                <input placeholder="Additional notes…" value={form.notes}
                  onChange={e => setField("notes", e.target.value)}
                  style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
              </div>
            </div>
          </div>
        </div>

        {/* ── Error Banner ─────────────────────────────────────────────────── */}
        {(apiError || parentError) && (
          <div className="rounded-xl px-4 py-3 flex items-center gap-3"
            style={{ background: "#fef2f2", border: "1px solid #fecaca" }}>
            <span className="text-lg">⚠️</span>
            <p className="text-sm font-medium" style={{ color: "#dc2626" }}>{apiError || parentError}</p>
          </div>
        )}
      </div>

      {/* ── FOOTER ─────────────────────────────────────────────────────────── */}
      <footer className="shrink-0 px-6 py-4 flex items-center justify-between"
        style={{ background: "#fff", borderTop: "1px solid #e5e7eb", boxShadow: "0 -1px 3px rgba(0,0,0,0.06)" }}>
        <div className="flex items-center gap-4">
          <button onClick={onBack} className="px-4 py-2 rounded-lg text-sm font-semibold"
            style={{ background: "#f3f4f6", color: "#374151", border: "1px solid #e5e7eb" }}>
            ← Back
          </button>
          <div className="text-sm" style={{ color: "#6b7280" }}>
            Weight:{" "}
            <span className="font-bold" style={{ color: "#111827" }}>{effectiveWeight.toLocaleString()} kg</span>
            <span className="ml-2 text-xs" style={{ color: "#9ca3af" }}>({weightMode})</span>
          </div>
        </div>
        <button
          onClick={handleCapture}
          disabled={submitting || (weightMode === "captured" && !isStable) || effectiveWeight <= 0}
          className="px-8 py-2.5 rounded-xl text-sm font-bold text-white transition-all"
          style={{
            background: submitting || (weightMode === "captured" && !isStable) || effectiveWeight <= 0
              ? "#9ca3af"
              : "linear-gradient(135deg,#d97706,#f59e0b)",
            boxShadow: submitting ? "none" : "0 3px 10px rgba(217,119,6,0.35)",
            cursor:    submitting ? "not-allowed" : "pointer",
          }}>
          {submitting ? "Saving…" : "⚡ Capture Weight"}
        </button>
      </footer>
    </div>
  );
}

// ── Sub-components ────────────────────────────────────────────────────────────
function Pill({ bg, border, color, children }) {
  return (
    <span className="px-2.5 py-0.5 rounded-full text-xs font-bold"
      style={{ background: bg, border: `1px solid ${border}`, color }}>
      {children}
    </span>
  );
}

function InfoChip({ label, value }) {
  return (
    <div className="rounded-lg p-2" style={{ background: "#f9fafb", border: "1px solid #f3f4f6" }}>
      <p className="text-xs font-bold uppercase tracking-wider mb-0.5" style={{ color: "#9ca3af" }}>{label}</p>
      <p className="text-xs font-bold truncate" style={{ color: "#374151" }}>{value}</p>
    </div>
  );
}

function ReadOnlyField({ label, value, icon, fromRfid, required }) {
  const hasValue = Boolean(value && String(value).trim().length > 0);
  return (
    <div>
      <div className="flex items-center gap-1 mb-1">
        <p className="text-xs font-bold uppercase tracking-wider" style={{ color: "#d97706" }}>
          {label}{required && <span style={{ color: "#ef4444" }}>*</span>}
        </p>
        {fromRfid && (
          <span className="text-xs px-1.5 py-0 rounded font-semibold"
            style={{ background: "#f0fdf4", color: "#16a34a", border: "1px solid #bbf7d0" }}>
            RFID
          </span>
        )}
      </div>
      <div className="rounded-lg px-3 py-2 flex items-center gap-2"
        style={{ background: hasValue ? "#fffbeb" : "#f9fafb", border: hasValue ? "1.5px solid #fcd34d" : "1.5px solid #f3f4f6" }}>
        <span className="text-base">{icon}</span>
        <span className="text-sm font-semibold" style={{ color: hasValue ? "#111827" : "#9ca3af" }}>
          {hasValue ? value : "Not provided"}
        </span>
        {hasValue && <span className="ml-auto text-xs" style={{ color: "#d97706" }}>🔒</span>}
      </div>
    </div>
  );
}

function DRow({ label, value, mono }) {
  return (
    <div className="flex items-center justify-between py-1.5 px-2 rounded-lg" style={{ background: "#f9fafb" }}>
      <span className="text-xs" style={{ color: "#9ca3af" }}>{label}</span>
      <span className={`text-xs font-semibold ${mono ? "font-mono" : ""}`} style={{ color: "#374151" }}>
        {value}
      </span>
    </div>
  );
}