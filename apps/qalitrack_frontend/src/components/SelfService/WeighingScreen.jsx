/**
 * WeighingScreen.jsx — Self-Service Kiosk Weighing
 * Theme: White / Black / Amber-600 — light + dark mode
 *
 * Layout: Fully contained — no overflow, no spill.
 * Weight reading section: ALWAYS gray/neutral regardless of mode or theme.
 * Removed: all purple, replaced green accents with amber/black.
 */

import React, { useEffect, useState, useRef, useCallback } from "react";
import { message, Select } from "antd";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import { useTheme } from "../Context/ThemeContext.jsx";

const { Option } = Select;
const BASE_URL = import.meta.env.VITE_API_URL || "/api";

function getKioskToken() {
  try {
    const s1 = sessionStorage.getItem("authSession");
    if (s1) { const p = JSON.parse(s1); const t = p?.token ?? p?.accessToken ?? p?.access_token ?? p?.userData?.token; if (t) return t; }
    const s2 = sessionStorage.getItem("user");
    if (s2) { const p = JSON.parse(s2); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
    const l1 = localStorage.getItem("token");     if (l1) return l1;
    const l2 = localStorage.getItem("authToken"); if (l2) return l2;
    const l3 = localStorage.getItem("authSession");
    if (l3) { const p = JSON.parse(l3); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
  } catch {}
  return null;
}

async function apiFetch(path, opts = {}) {
  const token = getKioskToken();
  const headers = { "Content-Type": "application/json", ...(token ? { Authorization: `Bearer ${token}` } : {}), ...(opts.headers || {}) };
  const res = await fetch(`${BASE_URL}${path}`, { ...opts, headers });
  if (!res.ok) { const b = await res.text(); throw new Error(b || `HTTP ${res.status}`); }
  return res.json();
}

const searchProducts  = q => apiFetch(`/MasterData/Products?searchTerm=${encodeURIComponent(q)}&pageSize=20`).then(r => r?.items ?? r?.data?.items ?? r?.data?.data?.items ?? []);
const getWeighbridges = ()  => apiFetch(`/MasterData/Weighbridges?pageSize=50`).then(r => r?.items ?? r?.data?.items ?? r?.data?.data?.items ?? []);
const postTransaction  = b  => apiFetch(`/Transaction/Transaction/Transaction`, { method: "POST", body: JSON.stringify({ request: b }) });
const patchTransaction = (id, b) => apiFetch(`/Transaction/Transaction/Transaction/${id}`, { method: "PATCH", body: JSON.stringify(b) });

function useDebounce(fn, delay) {
  const t = useRef(null);
  return useCallback((...args) => {
    if (t.current) clearTimeout(t.current);
    t.current = setTimeout(() => fn(...args), delay);
  }, [fn, delay]);
}

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

// ─── COMPONENT ─────────────────────────────────────────────────────────────────
export default function WeighingScreen({
  vehicleData = {},
  driverData  = {},
  existingTransaction = null,
  onWeighingComplete,
  onBack,
  error: parentError,
}) {
  const { isDark, toggleTheme } = useTheme();
  const hwConfig = useHardwareConfig();

  const isSecondWeigh = Boolean(existingTransaction);
  const vf = extractVehicleFields(vehicleData);
  const df = extractDriverFields(vehicleData, driverData);

  // ── Colour system ────────────────────────────────────────────────────────────
  const bg    = isDark ? "#0a0a0a"   : "#f4f4f4";
  const card  = isDark ? "#111827"   : "#ffffff";
  const bdr   = isDark ? "#1f2937"   : "#e5e7eb";
  const txt   = isDark ? "#f9fafb"   : "#111827";
  const muted = isDark ? "#6b7280"   : "#9ca3af";
  const sub   = isDark ? "#1a2234"   : "#f9fafb";
  const field = isDark ? "#1f2937"   : "#fff";

  // Weight area is ALWAYS neutral gray
  const weightBg   = isDark ? "#1c1c1c" : "#f0f0f0";
  const weightBdr  = isDark ? "#2a2a2a" : "#d1d5db";
  const weightCard = isDark ? "#242424" : "#e8e8e8";

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
    ownerId:         vf.ownerId,
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
    operatorName:    df.driverName || "Self-Service Kiosk",
    weighMode:       "Gross/Tare",
    weighBridgeID:   null,
    weighBridgeName: existingTransaction?.weighBridgeName ?? "Factory A",
    scaleName:       existingTransaction?.weighBridgeName ?? "Factory A",
    originName:      existingTransaction?.originName      ?? "",
    destinationName: existingTransaction?.destinationName ?? "",
    operation:       existingTransaction?.operation       ?? "weighing",
    notes:           existingTransaction?.notes           ?? "",
  });

  useEffect(() => {
    const newVf = extractVehicleFields(vehicleData);
    const newDf = extractDriverFields(vehicleData, driverData);
    setForm(prev => ({
      ...prev,
      vehicleID: newVf.vehicleID, noPlate: newVf.noPlate, rfidTag: newVf.rfidTag,
      vehicleType: newVf.vehicleType, vehicleMake: newVf.vehicleMake, vehicleModel: newVf.vehicleModel,
      capacity: newVf.capacity, ownerId: newVf.ownerId,
      transporterID: newVf.transporterID, transporterName: newVf.transporterName,
      supplierID: newVf.supplierID, supplierName: newVf.supplierName, saccoName: newVf.saccoName,
      driverID: newDf.driverID, driverName: newDf.driverName, driverPhone: newDf.driverPhone,
      licenseNo: newDf.licenseNo, employeeId: newDf.employeeId, nfcUid: newDf.nfcUid,
      operatorID: newDf.driverID || null, operatorName: newDf.driverName || "Self-Service Kiosk",
    }));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicleData, driverData]);

  const [weightMode,     setWeightMode]     = useState("captured");
  const [capturedWeight, setCapturedWeight] = useState(0);
  const [manualWeight,   setManualWeight]   = useState("");
  const [isStable,       setIsStable]       = useState(false);
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
        if (wb) setForm(p => ({ ...p, weighBridgeID: wb.id, weighBridgeName: wb.location ?? wb.name, scaleName: wb.location ?? wb.name }));
        return;
      }
      const factoryA = list.find(wb => /factory\s*a/i.test(wb.location ?? wb.name ?? ""));
      if (factoryA) setForm(p => ({ ...p, weighBridgeID: factoryA.id, weighBridgeName: factoryA.location ?? factoryA.name, scaleName: factoryA.location ?? factoryA.name }));
    }).catch(() => {});
  }, [existingTransaction]);

  useEffect(() => {
    const BASE = isSecondWeigh ? 8400 : 19011;
    let iter = 0;
    const id = setInterval(() => {
      iter++;
      const w = iter < 6 ? BASE + Math.floor(Math.random() * 40 - 20) : BASE;
      bufferRef.current = w;
      setCapturedWeight(w);
    }, 1200);
    return () => clearInterval(id);
  }, [isSecondWeigh]);

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
        const txnId = existingTransaction.id ?? existingTransaction.ticketID;
        const patchPayload = { secondWeight: String(effectiveWeight), weighMode: "Kiosk", isCompleted: true, status: "Complete", notes: form.notes?.trim() || "Self-service kiosk — second weight" };
        if (isValidGuid(form.driverID)) patchPayload.driverID = form.driverID;
        if (form.driverName?.trim())    patchPayload.driverName = form.driverName.trim();
        result = await patchTransaction(txnId, patchPayload);
        message.success(`Transaction completed! Net weight: ${effectiveWeight - (existingTransaction.firstWeight ?? 0)} kg`, 4);
      } else {
        const payload = { noPlate: form.noPlate.toUpperCase().trim(), firstWeight: String(effectiveWeight), weighMode: "Kiosk", operation: form.operation, operatorName: form.operatorName || "Self-Service Kiosk" };
        if (isValidGuid(form.vehicleID))  payload.vehicleID  = form.vehicleID;
        if (isValidGuid(form.driverID))   payload.driverID   = form.driverID;
        if (isValidGuid(form.operatorID)) payload.operatorID = form.operatorID;
        if (form.driverName?.trim())      payload.driverName = form.driverName.trim();
        if (isValidGuid(form.transporterID)) { payload.transporterID = form.transporterID; payload.transporterName = form.transporterName; }
        else if (form.transporterName?.trim()) payload.transporterName = form.transporterName.trim();
        if (isValidGuid(form.weighBridgeID)) { payload.weighBridgeID = form.weighBridgeID; payload.weighBridgeName = form.weighBridgeName; payload.scaleName = form.scaleName; }
        if (isValidGuid(form.supplierID)) { payload.supplierID = form.supplierID; payload.supplierName = form.supplierName; }
        else if (form.supplierName?.trim()) payload.supplierName = form.supplierName.trim();
        if (isValidGuid(form.commodityID)) { payload.commodityID = form.commodityID; payload.commodityName = form.commodityName; }
        if (form.customerName?.trim())    payload.customerName    = form.customerName.trim();
        if (form.originName?.trim())      payload.originName      = form.originName.trim();
        if (form.destinationName?.trim()) payload.destinationName = form.destinationName.trim();
        payload.notes = form.notes?.trim() || "Self-service kiosk transaction";
        result = await postTransaction(payload);
        message.success(`Saved! Receipt: ${result?.data?.receiptNo ?? result?.receiptNo ?? result?.ticketID ?? "Generated"}`, 4);
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

  // ── Input styles ─────────────────────────────────────────────────────────────
  const inputStyle = {
    width: "100%", padding: "8px 12px", borderRadius: 8,
    border: `1.5px solid ${bdr}`, fontSize: 13, color: txt,
    background: field, outline: "none",
  };
  const onFoc = e => { e.target.style.borderColor = "#d97706"; e.target.style.boxShadow = "0 0 0 3px rgba(217,119,6,0.12)"; };
  const onBlr = e => { e.target.style.borderColor = bdr; e.target.style.boxShadow = "none"; };

  // ── Section card ─────────────────────────────────────────────────────────────
  const SectionCard = ({ title, badge, children, style = {} }) => (
    <div style={{ background: card, border: `1px solid ${bdr}`, borderRadius: 14, overflow: "hidden", ...style }}>
      <div style={{ padding: "10px 16px", borderBottom: `1px solid ${bdr}`, background: sub, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <p style={{ fontSize: 11, fontWeight: 800, textTransform: "uppercase", letterSpacing: "0.08em", color: "#d97706", margin: 0 }}>{title}</p>
        {badge}
      </div>
      <div style={{ padding: 16 }}>{children}</div>
    </div>
  );

  const Badge = ({ label, icon }) => (
    <span style={{ display: "flex", alignItems: "center", gap: 4, padding: "3px 8px", borderRadius: 20, fontSize: 10, fontWeight: 700, background: isDark ? "#1f2937" : "#fff7ed", border: `1px solid ${isDark ? "#374151" : "#fcd34d"}`, color: isDark ? "#fcd34d" : "#92400e" }}>
      {icon && <span>{icon}</span>}{label}
    </span>
  );

  const DataRow = ({ label, value, mono }) => (
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "6px 8px", borderRadius: 6, background: sub, marginBottom: 4 }}>
      <span style={{ fontSize: 11, color: muted }}>{label}</span>
      <span style={{ fontSize: 11, fontWeight: 600, fontFamily: mono ? "monospace" : "inherit", color: txt }}>{value}</span>
    </div>
  );

  const LockedField = ({ label, value, icon, required }) => {
    const has = Boolean(value?.trim?.());
    return (
      <div style={{ marginBottom: 10 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 4, marginBottom: 4 }}>
          <span style={{ fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: "#d97706" }}>{label}{required && <span style={{ color: "#ef4444" }}>*</span>}</span>
          <span style={{ fontSize: 9, fontWeight: 700, padding: "1px 5px", borderRadius: 4, background: "#fff7ed", color: "#d97706", border: "1px solid #fcd34d" }}>RFID</span>
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 8, padding: "8px 12px", borderRadius: 8, background: has ? (isDark ? "#1a2010" : "#fffbeb") : sub, border: `1.5px solid ${has ? "#fcd34d" : bdr}` }}>
          <span>{icon}</span>
          <span style={{ fontSize: 12, fontWeight: 600, color: has ? txt : muted, fontStyle: has ? "normal" : "italic", flex: 1, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
            {has ? value : "Not provided"}
          </span>
          {has && <span style={{ fontSize: 10, color: "#d97706" }}>🔒</span>}
        </div>
      </div>
    );
  };

  const canSubmit = !submitting && (weightMode === "manual" || isStable) && effectiveWeight > 0;

  // ─── RENDER ──────────────────────────────────────────────────────────────────
  return (
    <div style={{ height: "100vh", display: "flex", flexDirection: "column", background: bg, fontFamily: "'Inter', system-ui, sans-serif", overflow: "hidden" }}>

      {/* ── HEADER ─────────────────────────────────────────────────────────────── */}
      <header style={{ flexShrink: 0, padding: "12px 24px", background: card, borderBottom: `1px solid ${bdr}`, display: "flex", alignItems: "center", justifyContent: "space-between", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
          <div style={{ width: 38, height: 38, borderRadius: 10, background: isSecondWeigh ? "linear-gradient(135deg,#dc2626,#b91c1c)" : "linear-gradient(135deg,#d97706,#b45309)", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5">
              <path d="M12 2L2 7l10 5 10-5-10-5z" /><path d="M2 17l10 5 10-5" /><path d="M2 12l10 5 10-5" />
            </svg>
          </div>
          <div>
            <h1 style={{ fontSize: 15, fontWeight: 800, color: txt, margin: 0 }}>
              {isSecondWeigh ? "Second (Tare) Weight" : "Weighing"}
            </h1>
            <p style={{ fontSize: 11, color: muted, margin: 0 }}>
              {isSecondWeigh
                ? `Completing transaction · First weight: ${existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"} kg`
                : "RFID verified · NFC authenticated · Kiosk mode"}
            </p>
          </div>
        </div>

        <div style={{ display: "flex", alignItems: "center", gap: 8, flexWrap: "wrap" }}>
          {form.noPlate    && <Badge label={form.noPlate}    icon="🚗" />}
          {form.driverName && <Badge label={form.driverName} icon="👤" />}
          {isSecondWeigh   && <Badge label={`2nd Weight · 1st: ${existingTransaction?.firstWeight ?? "—"} kg`} icon="⚠️" />}
          <Badge label={form.weighBridgeName} icon="🔒" />
          <span style={{ display: "flex", alignItems: "center", gap: 4, padding: "3px 8px", borderRadius: 20, fontSize: 10, fontWeight: 700, background: "#1f2937", border: "1px solid #374151", color: "#4ade80" }}>
            <span style={{ width: 6, height: 6, borderRadius: "50%", background: "#4ade80", animation: "pulse 1.5s ease-in-out infinite" }} /> LIVE
          </span>
          <button onClick={() => setShowInspector(v => !v)} style={{ padding: "4px 8px", borderRadius: 6, fontSize: 10, background: sub, border: `1px solid ${bdr}`, color: muted, cursor: "pointer" }}>
            {showInspector ? "Hide" : "🔍 Inspect"}
          </button>
          <button onClick={toggleTheme} style={{ padding: "6px 10px", borderRadius: 8, fontSize: 13, background: sub, border: `1px solid ${bdr}`, cursor: "pointer" }}>
            {isDark ? "☀️" : "🌙"}
          </button>
        </div>
      </header>

      {/* ── SECOND WEIGHT BANNER ───────────────────────────────────────────────── */}
      {isSecondWeigh && (
        <div style={{ flexShrink: 0, padding: "10px 24px", display: "flex", alignItems: "center", gap: 12, background: "#fff7ed", borderBottom: "2px solid #d97706" }}>
          <span style={{ fontSize: 20 }}>⚖️</span>
          <div>
            <p style={{ fontSize: 13, fontWeight: 900, color: "#92400e", margin: 0 }}>SECOND WEIGHING MODE — Completing Existing Transaction</p>
            <p style={{ fontSize: 11, color: "#78350f", margin: 0 }}>
              Ticket: <strong>{existingTransaction?.ticketID ?? existingTransaction?.id ?? "—"}</strong>
              {" · "}First weight: <strong>{existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"} kg</strong>
              {" · "}Captured: <strong>{existingTransaction?.createdAt ? new Date(existingTransaction.createdAt).toLocaleString() : "—"}</strong>
            </p>
          </div>
        </div>
      )}

      {/* ── INSPECTOR ──────────────────────────────────────────────────────────── */}
      {showInspector && (
        <div style={{ flexShrink: 0, background: "#030712", borderBottom: "2px solid #1e293b", padding: "12px 24px", fontFamily: "monospace", fontSize: 10, color: "#94a3b8", maxHeight: 180, overflowY: "auto" }}>
          <p style={{ color: "#f59e0b", fontWeight: "bold", marginBottom: 6, margin: "0 0 6px" }}>🔍 Data Inspector</p>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12 }}>
            <div><p style={{ color: "#64748b", fontSize: 9, marginBottom: 3 }}>vehicleData.vehicle:</p><pre style={{ color: "#86efac", fontSize: 9, margin: 0, whiteSpace: "pre-wrap", wordBreak: "break-all" }}>{JSON.stringify(vehicleData?.vehicle ?? vehicleData, null, 2)}</pre></div>
            <div><p style={{ color: "#64748b", fontSize: 9, marginBottom: 3 }}>Resolved form:</p><pre style={{ color: "#93c5fd", fontSize: 9, margin: 0 }}>{JSON.stringify({ noPlate: form.noPlate, ownerId: form.ownerId, transporterName: form.transporterName, supplierName: form.supplierName, driverName: form.driverName }, null, 2)}</pre></div>
            <div><p style={{ color: "#64748b", fontSize: 9, marginBottom: 3 }}>existingTransaction:</p><pre style={{ color: "#fda4af", fontSize: 9, margin: 0, whiteSpace: "pre-wrap", wordBreak: "break-all" }}>{JSON.stringify(existingTransaction, null, 2)}</pre></div>
          </div>
        </div>
      )}

      {/* ── BODY: 3-column grid, scrollable ────────────────────────────────────── */}
      <div style={{ flex: 1, overflow: "auto", padding: "16px 24px" }}>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1.4fr", gap: 16, marginBottom: 16 }}>

          {/* ── Column 1: Vehicle ─────────────────────────────────────────────── */}
          <SectionCard title="Vehicle" badge={<Badge label="RFID Verified" />}>
            {/* Plate */}
            <div style={{ textAlign: "center", padding: "12px 0 16px", borderRadius: 10, marginBottom: 14, background: isDark ? "#0f1117" : "#f8f8f8", border: `1.5px solid ${isSecondWeigh ? "#d97706" : bdr}` }}>
              <p style={{ fontSize: 10, fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.1em", color: isSecondWeigh ? "#92400e" : muted, marginBottom: 4 }}>Registration</p>
              <p style={{ fontSize: 28, fontWeight: 900, letterSpacing: "0.12em", color: txt, margin: 0, fontFamily: "monospace" }}>{form.noPlate || "—"}</p>
              {form.rfidTag && <p style={{ fontFamily: "monospace", fontSize: 10, color: muted, marginTop: 4 }}>{form.rfidTag}</p>}
            </div>

            {/* Vehicle attributes */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 6, marginBottom: 14 }}>
              {[
                { label: "Type",     value: form.vehicleType  },
                { label: "Make",     value: form.vehicleMake  },
                { label: "Model",    value: form.vehicleModel },
                { label: "Capacity", value: form.capacity ? `${form.capacity} kg` : null },
              ].filter(f => f.value).map(f => (
                <div key={f.label} style={{ padding: "7px 10px", borderRadius: 8, background: sub, border: `1px solid ${bdr}` }}>
                  <p style={{ fontSize: 9, fontWeight: 700, textTransform: "uppercase", color: muted, marginBottom: 2 }}>{f.label}</p>
                  <p style={{ fontSize: 11, fontWeight: 700, color: txt, margin: 0 }}>{f.value}</p>
                </div>
              ))}
            </div>

            <LockedField label="Owner"       value={form.ownerId}         icon="👤" />
            <LockedField label="Transporter" value={form.transporterName} icon="🚛" required />
            <LockedField label="Supplier"    value={form.supplierName}    icon="🏭" />
            {form.saccoName && <LockedField label="SACCO" value={form.saccoName} icon="🤝" />}
          </SectionCard>

          {/* ── Column 2: Driver ──────────────────────────────────────────────── */}
          <SectionCard title="Driver / Operator" badge={<Badge label="NFC Auth" />}>
            {form.driverName ? (
              <>
                <div style={{ display: "flex", alignItems: "center", gap: 12, marginBottom: 14 }}>
                  <div style={{ width: 48, height: 48, borderRadius: "50%", background: isDark ? "#1f2937" : "#f3f4f6", border: `2px solid ${bdr}`, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 22, flexShrink: 0 }}>👤</div>
                  <div>
                    <p style={{ fontSize: 14, fontWeight: 800, color: txt, margin: 0 }}>{form.driverName}</p>
                    {form.employeeId && <p style={{ fontSize: 11, fontFamily: "monospace", color: muted, margin: 0 }}>ID: {form.employeeId}</p>}
                  </div>
                </div>

                {/* Operator locked chip */}
                <div style={{ display: "flex", alignItems: "center", gap: 8, padding: "8px 12px", borderRadius: 8, background: isDark ? "#1a1a0f" : "#fffbeb", border: "1px solid #fcd34d", marginBottom: 12 }}>
                  <span>🔒</span>
                  <div>
                    <p style={{ fontSize: 10, fontWeight: 700, color: "#d97706", margin: 0 }}>Operator (locked)</p>
                    <p style={{ fontSize: 12, fontWeight: 600, color: txt, margin: 0 }}>{form.operatorName}</p>
                  </div>
                </div>

                {form.driverPhone && <DataRow label="Phone"   value={form.driverPhone} />}
                {form.licenseNo   && <DataRow label="Licence" value={form.licenseNo}   />}
                {form.nfcUid      && <DataRow label="NFC UID" value={form.nfcUid} mono />}
              </>
            ) : (
              <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", padding: "32px 0", textAlign: "center" }}>
                <span style={{ fontSize: 36, marginBottom: 8 }}>👤</span>
                <p style={{ fontSize: 12, color: muted }}>No driver authenticated</p>
              </div>
            )}

            {/* First weight recap in second-weigh mode */}
            {isSecondWeigh && (
              <div style={{ marginTop: 14, padding: "12px 14px", borderRadius: 12, background: isDark ? "#1a0f00" : "#fff7ed", border: "1.5px solid #d97706" }}>
                <p style={{ fontSize: 10, fontWeight: 800, textTransform: "uppercase", letterSpacing: "0.08em", color: "#d97706", marginBottom: 6 }}>First Weight (Gross)</p>
                <p style={{ fontSize: 26, fontWeight: 900, fontFamily: "monospace", color: txt, margin: 0 }}>
                  {existingTransaction?.firstWeight ?? existingTransaction?.grossWeight ?? "—"}
                  <span style={{ fontSize: 13, fontWeight: 400, color: muted }}> kg</span>
                </p>
                <p style={{ fontSize: 11, color: "#92400e", margin: "4px 0 0" }}>
                  {existingTransaction?.createdAt ? new Date(existingTransaction.createdAt).toLocaleString() : "—"}
                </p>
              </div>
            )}
          </SectionCard>

          {/* ── Column 3: Weight — ALWAYS GRAY ────────────────────────────────── */}
          <div style={{ background: weightBg, border: `1px solid ${weightBdr}`, borderRadius: 14, overflow: "hidden", display: "flex", flexDirection: "column" }}>
            {/* Weight card header */}
            <div style={{ padding: "10px 16px", borderBottom: `1px solid ${weightBdr}`, background: weightCard, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
              <p style={{ fontSize: 11, fontWeight: 800, textTransform: "uppercase", letterSpacing: "0.08em", color: "#6b7280", margin: 0 }}>
                {isSecondWeigh ? "Second (Tare) Weight" : "Weight Reading"}
              </p>
              {/* Mode toggle */}
              <div style={{ display: "flex", borderRadius: 8, overflow: "hidden", border: "1.5px solid #9ca3af" }}>
                {["captured", "manual"].map(m => (
                  <button key={m} onClick={() => setWeightMode(m)}
                    style={{ padding: "4px 10px", fontSize: 10, fontWeight: 700, cursor: "pointer", border: "none", transition: "all 0.15s", background: weightMode === m ? "#374151" : weightCard, color: weightMode === m ? "#fff" : "#9ca3af" }}>
                    {m === "captured" ? "⚡ Live" : "✏️ Manual"}
                  </button>
                ))}
              </div>
            </div>

            {/* Big weight display */}
            <div style={{ flex: 1, display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", padding: "20px 16px", gap: 8 }}>
              {weightMode === "captured" ? (
                <>
                  <p style={{ fontSize: 64, fontWeight: 900, fontFamily: "monospace", color: "#374151", margin: 0, lineHeight: 1, letterSpacing: "-2px" }}>
                    {capturedWeight.toLocaleString()}
                  </p>
                  <p style={{ fontSize: 14, fontWeight: 600, color: "#9ca3af", margin: 0 }}>KG</p>
                  {/* Stability indicator */}
                  <span style={{
                    padding: "4px 14px", borderRadius: 20, fontSize: 11, fontWeight: 700,
                    background: isStable ? "#d1fae5" : "#e5e7eb",
                    color: isStable ? "#065f46" : "#6b7280",
                    border: `1px solid ${isStable ? "#6ee7b7" : "#d1d5db"}`,
                  }}>
                    {isStable ? "● Stable" : "● Stabilising…"}
                  </span>

                  {/* Net weight preview for 2nd weigh */}
                  {isSecondWeigh && capturedWeight > 0 && (existingTransaction?.firstWeight ?? 0) > 0 && (
                    <div style={{ marginTop: 8, padding: "10px 20px", borderRadius: 12, background: weightCard, border: `1px solid ${weightBdr}`, textAlign: "center" }}>
                      <p style={{ fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.08em", color: "#6b7280", marginBottom: 4 }}>Net Weight Preview</p>
                      <p style={{ fontSize: 24, fontWeight: 900, fontFamily: "monospace", color: "#374151", margin: 0 }}>
                        {Math.abs((existingTransaction?.firstWeight ?? 0) - capturedWeight).toLocaleString()}
                        <span style={{ fontSize: 13, fontWeight: 400, color: "#9ca3af" }}> kg</span>
                      </p>
                    </div>
                  )}

                  <div style={{ display: "flex", gap: 8, flexWrap: "wrap", justifyContent: "center", marginTop: 4 }}>
                    <span style={{ fontSize: 11, padding: "4px 10px", borderRadius: 8, fontWeight: 600, background: weightCard, color: "#6b7280", border: `1px solid ${weightBdr}` }}>
                      🔒 {form.weighBridgeName || "Factory A"}
                    </span>
                    <span style={{ fontSize: 11, padding: "4px 10px", borderRadius: 8, fontWeight: 600, background: weightCard, color: "#6b7280", border: `1px solid ${weightBdr}` }}>
                      Mode: Kiosk
                    </span>
                  </div>
                </>
              ) : (
                <>
                  <input type="number" placeholder="0" value={manualWeight}
                    onChange={e => setManualWeight(e.target.value)}
                    style={{ width: 160, padding: "8px 12px", fontSize: 36, fontFamily: "monospace", fontWeight: 900, textAlign: "center", borderRadius: 10, border: "2px solid #9ca3af", background: weightCard, color: "#374151", outline: "none" }}
                  />
                  <p style={{ fontSize: 13, fontWeight: 600, color: "#9ca3af" }}>KG (manual)</p>
                </>
              )}
            </div>
          </div>
        </div>

        {/* ── Transaction Details Row ───────────────────────────────────────────── */}
        <SectionCard
          title={isSecondWeigh ? "Transaction Details (from first weighing)" : "Transaction Details"}
          badge={<span style={{ fontSize: 11, color: muted }}>{isSecondWeigh ? "Pre-filled — update if needed" : "🔒 = auto-filled from RFID"}</span>}
        >
          <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 14 }}>
            {/* Row 1 */}
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Commodity</label>
              <Select showSearch placeholder="Search commodity…" onSearch={debouncedP}
                onChange={id => { const item = products.find(i => i.id === id); setForm(p => ({ ...p, commodityID: id, commodityName: item?.name ?? "" })); }}
                value={form.commodityID || undefined} style={{ width: "100%" }} allowClear>
                {products.map(i => <Option key={i.id} value={i.id}>{i.name}</Option>)}
              </Select>
            </div>
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Customer</label>
              <input placeholder="Customer name" value={form.customerName} onChange={e => setField("customerName", e.target.value)} style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
            </div>
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Origin</label>
              <input placeholder="Origin" value={form.originName} onChange={e => setField("originName", e.target.value)} style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
            </div>
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Destination</label>
              <input placeholder="Destination" value={form.destinationName} onChange={e => setField("destinationName", e.target.value)} style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
            </div>

            {/* Row 2 */}
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>
                Weighbridge {!isSecondWeigh && <span style={{ color: "#ef4444" }}>*</span>}
              </label>
              {isSecondWeigh ? (
                <div style={{ display: "flex", alignItems: "center", gap: 6, padding: "8px 12px", height: 36, borderRadius: 8, background: isDark ? "#1a1a0f" : "#fffbeb", border: "1.5px solid #fcd34d" }}>
                  <span style={{ fontSize: 11 }}>🔒</span>
                  <span style={{ fontSize: 12, fontWeight: 600, color: txt }}>{form.weighBridgeName}</span>
                </div>
              ) : (
                <Select value={form.weighBridgeName || undefined}
                  onChange={v => { const wb = weighbridges.find(w => (w.location ?? w.name) === v); setForm(p => ({ ...p, weighBridgeID: wb?.id || null, weighBridgeName: v, scaleName: v })); }}
                  style={{ width: "100%" }} placeholder="Select weighbridge…">
                  {weighbridges.map(wb => <Option key={wb.id} value={wb.location ?? wb.name}>{wb.location ?? wb.name}</Option>)}
                </Select>
              )}
            </div>
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Weigh Mode</label>
              <div style={{ display: "flex", alignItems: "center", gap: 6, padding: "8px 12px", height: 36, borderRadius: 8, background: isDark ? "#1a1a0f" : "#fffbeb", border: "1.5px solid #fcd34d" }}>
                <span style={{ fontSize: 11 }}>🔒</span>
                <span style={{ fontSize: 12, fontWeight: 600, color: txt }}>Kiosk</span>
              </div>
            </div>
            <div>
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Operation</label>
              {isSecondWeigh ? (
                <div style={{ display: "flex", alignItems: "center", padding: "8px 12px", height: 36, borderRadius: 8, background: sub, border: `1.5px solid ${bdr}` }}>
                  <span style={{ fontSize: 12, fontWeight: 600, color: txt }}>{form.operation}</span>
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
              <label style={{ display: "block", fontSize: 10, fontWeight: 700, textTransform: "uppercase", letterSpacing: "0.07em", color: muted, marginBottom: 5 }}>Notes</label>
              <input placeholder="Additional notes…" value={form.notes} onChange={e => setField("notes", e.target.value)} style={inputStyle} onFocus={onFoc} onBlur={onBlr} />
            </div>
          </div>
        </SectionCard>

        {/* Error banner */}
        {(apiError || parentError) && (
          <div style={{ marginTop: 14, padding: "12px 16px", borderRadius: 10, background: "#fef2f2", border: "1px solid #fecaca", display: "flex", alignItems: "center", gap: 10 }}>
            <span style={{ fontSize: 18 }}>⚠️</span>
            <p style={{ fontSize: 13, fontWeight: 600, color: "#dc2626", margin: 0 }}>{apiError || parentError}</p>
          </div>
        )}
      </div>

      {/* ── FOOTER ─────────────────────────────────────────────────────────────── */}
      <footer style={{ flexShrink: 0, padding: "14px 24px", background: card, borderTop: `1px solid ${bdr}`, display: "flex", alignItems: "center", justifyContent: "space-between", boxShadow: "0 -2px 8px rgba(0,0,0,0.06)" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 20 }}>
          <button onClick={onBack} style={{ padding: "8px 16px", borderRadius: 8, fontSize: 13, fontWeight: 600, background: sub, border: `1px solid ${bdr}`, color: txt, cursor: "pointer" }}>
            ← Back
          </button>
          <div style={{ fontSize: 13, color: muted }}>
            {isSecondWeigh ? "Tare: " : "Weight: "}
            <strong style={{ color: txt }}>{effectiveWeight.toLocaleString()} kg</strong>
            {isSecondWeigh && (existingTransaction?.firstWeight ?? 0) > 0 && effectiveWeight > 0 && (
              <span style={{ marginLeft: 12, fontSize: 12, fontWeight: 700, color: "#d97706" }}>
                Net: {Math.abs((existingTransaction?.firstWeight ?? 0) - effectiveWeight).toLocaleString()} kg
              </span>
            )}
          </div>
        </div>

        <button onClick={handleCapture} disabled={!canSubmit} style={{
          padding: "12px 32px", borderRadius: 12, fontSize: 14, fontWeight: 800, color: "#fff",
          border: "none", cursor: canSubmit ? "pointer" : "not-allowed", transition: "all 0.15s",
          background: !canSubmit ? "#9ca3af"
            : isSecondWeigh ? "linear-gradient(135deg,#dc2626,#b91c1c)"
            : "linear-gradient(135deg,#d97706,#b45309)",
          boxShadow: canSubmit ? `0 4px 14px ${isSecondWeigh ? "rgba(220,38,38,0.4)" : "rgba(180,83,9,0.4)"}` : "none",
        }}>
          {submitting ? "Saving…" : isSecondWeigh ? "⚡ Capture Tare Weight" : "⚡ Capture Weight"}
        </button>
      </footer>

      <style>{`
        @keyframes pulse { 0%,100%{opacity:1}50%{opacity:0.4} }
        @keyframes spin  { to{transform:rotate(360deg)} }
      `}</style>
    </div>
  );
}