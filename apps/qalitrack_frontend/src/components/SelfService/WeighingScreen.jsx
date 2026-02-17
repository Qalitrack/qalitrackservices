/**
 * WeighingScreen.jsx  —  Kiosk weighing step
 * Fully corrected version – labelStyle defined, layout preserved
 */

import React, { useEffect, useState, useRef, useCallback } from "react";
import { message, Select, Input } from "antd";
const { Option } = Select;
const { TextArea } = Input;

// ─── API Helpers ─────────────────────────────────────────────────────────────
const BASE_URL = import.meta.env.VITE_API_URL || "/api";

async function apiFetch(path, opts = {}) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json", ...(opts.headers || {}) },
    ...opts,
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(body || `HTTP ${res.status}`);
  }
  return res.json();
}

async function searchVehicles(q)      { return apiFetch(`/vehicles?search=${encodeURIComponent(q)}&pageSize=20`).then(r => r.items || r || []); }
async function searchDrivers(q)       { return apiFetch(`/drivers?search=${encodeURIComponent(q)}&pageSize=20`).then(r => r.items || r || []); }
async function searchTransporters(q)  { return apiFetch(`/transporters?search=${encodeURIComponent(q)}&pageSize=20`).then(r => r.items || r || []); }
async function searchProducts(q)      { return apiFetch(`/products?search=${encodeURIComponent(q)}&pageSize=20`).then(r => r.items || r || []); }
async function searchSuppliers(q)     { return apiFetch(`/suppliers?search=${encodeURIComponent(q)}&pageSize=20`).then(r => r.items || r || []); }
async function getWeighbridges()      { return apiFetch(`/weighbridges?pageSize=50`).then(r => r.items || r || []); }
async function postTransaction(body)  { return apiFetch(`/transactions`, { method: "POST", body: JSON.stringify({ request: body }) }); }

// ─── Debounce ────────────────────────────────────────────────────────────────
function useDebounce(fn, delay) {
  const timerRef = useRef(null);
  return useCallback((...args) => {
    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => fn(...args), delay);
  }, [fn, delay]);
}

// ══════════════════════════════════════════════════════════════════════════════
export default function WeighingScreen({
  vehicleData = {},
  driverData  = {},
  onWeighingComplete,
  onBack,
  error: parentError,
}) {
  // ── dropdown data ──
  const [vehicles,     setVehicles]     = useState([]);
  const [drivers,      setDrivers]      = useState([]);
  const [transporters, setTransporters] = useState([]);
  const [products,     setProducts]     = useState([]);
  const [suppliers,    setSuppliers]    = useState([]);
  const [weighbridges, setWeighbridges] = useState([]);

  // ── form state ──
  const [form, setForm] = useState({
    vehicleID:           null,
    noPlate:             vehicleData?.plateNumber || vehicleData?.noPlate || "",
    driverID:            null,
    driverName:          driverData?.fullName || driverData?.name || "",
    transporterID:       null,
    transporterName:     "",
    commodityID:         null,
    commodityName:       "",
    supplierID:          null,
    supplierName:        "",
    customerName:        "",
    originName:          "",
    destinationName:     "",
    weighBridgeID:       null,
    weighBridgeName:     "",
    scaleName:           "",
    weighMode:           "entry",
    operation:           "weighing",
    notes:               "",
  });

  // ── weight state ──
  const [weightMode, setWeightMode]     = useState("captured");
  const [capturedWeight, setCapturedWeight] = useState(0);
  const [manualWeight,   setManualWeight]   = useState("");
  const [isStable,       setIsStable]       = useState(false);

  // ── submission state ──
  const [submitting, setSubmitting] = useState(false);
  const [apiError,   setApiError]   = useState(null);

  // ── refs for stability ──
  const bufferRef          = useRef(null);
  const lastStableRef      = useRef(null);
  const stabilityCounter   = useRef(0);
  const STABILITY_CYCLES   = 5;

  // ── load weighbridges ──
  useEffect(() => {
    getWeighbridges().then(setWeighbridges).catch(() => {});
  }, []);

  // ── simulated weight feed ──
  useEffect(() => {
    const BASE = 19011;
    let iter = 0;
    const id = setInterval(() => {
      iter++;
      const w = iter < 6 ? BASE + Math.floor(Math.random() * 40 - 20) : BASE;
      bufferRef.current = w;
      setCapturedWeight(w);
    }, 1200);
    return () => clearInterval(id);
  }, []);

  // ── stability check ──
  useEffect(() => {
    const id = setInterval(() => {
      const curr = bufferRef.current;
      if (curr == null) return;
      if (curr === lastStableRef.current) {
        stabilityCounter.current++;
        if (stabilityCounter.current >= STABILITY_CYCLES) setIsStable(true);
      } else {
        lastStableRef.current   = curr;
        stabilityCounter.current = 1;
        setIsStable(false);
      }
    }, 800);
    return () => clearInterval(id);
  }, []);

  // ── debounced searches ──
  const debounced = {
    vehicles:     useDebounce(q => { if (q) searchVehicles(q).then(setVehicles); }, 400),
    drivers:      useDebounce(q => { if (q) searchDrivers(q).then(setDrivers); }, 400),
    transporters: useDebounce(q => { if (q) searchTransporters(q).then(setTransporters); }, 400),
    products:     useDebounce(q => { if (q) searchProducts(q).then(setProducts); }, 400),
    suppliers:    useDebounce(q => { if (q) searchSuppliers(q).then(setSuppliers); }, 400),
  };

  // ── helpers ──
  const setField = (key, val) => setForm(p => ({ ...p, [key]: val }));

  const pick = (list, id, idKey, nameKey) => {
    const item = list.find(i => i.id === id);
    setForm(p => ({
      ...p,
      [idKey]:   id,
      [nameKey]: item?.name || item?.fullName || item?.registrationNumber || "",
    }));
  };

  const effectiveWeight = weightMode === "manual"
    ? Number(manualWeight) || 0
    : capturedWeight;

  // ── CAPTURE / SUBMIT ──
  const handleCapture = async () => {
    setApiError(null);

    if (effectiveWeight <= 0) {
      message.error("Weight must be greater than 0.");
      return;
    }
    if (!form.noPlate?.trim()) {
      message.error("Vehicle plate is required.");
      return;
    }
    if (!form.weighBridgeID) {
      message.error("Please select a weighbridge.");
      return;
    }
    if (!form.transporterID) {
      message.error("Transporter is required.");
      return;
    }

    setSubmitting(true);

    const payload = {
      noPlate:          form.noPlate.toUpperCase().trim(),
      driverName:       form.driverName || "",
      firstWeight:      String(effectiveWeight),
      transporterID:    form.transporterID || null,
      transporterName:  form.transporterName || "",
      weighBridgeID:    form.weighBridgeID || null,
      weighBridgeName:  form.weighBridgeName || form.scaleName || "",
      scaleName:        form.scaleName || "",
      operatorID:       null,
      operatorName:     "Self-Service Kiosk",
      commodityID:      form.commodityID || null,
      commodityName:    form.commodityName || "",
      supplierID:       form.supplierID || null,
      supplierName:     form.supplierName || "",
      customerName:     form.customerName || "",
      originName:       form.originName || "",
      destinationName:  form.destinationName || "",
      weighMode:        form.weighMode || "entry",
      operation:        form.operation || "weighing",
      notes:            form.notes || "Self-service kiosk transaction",
    };

    // Only include valid GUIDs
    if (form.vehicleID) payload.vehicleID = form.vehicleID;
    if (form.driverID)   payload.driverID   = form.driverID;

    console.log("📤 Kiosk posting:", payload);

    try {
      const result = await postTransaction(payload);
      console.log("✅ Saved:", result);

      message.success({
        content: `Transaction saved! Receipt: ${result.receiptNo || result.ticketID || "Generated"}`,
        duration: 4,
      });

      onWeighingComplete?.({
        ...payload,
        ...result,
        weight: effectiveWeight,
        weightMode,
      });
    } catch (e) {
      console.error("❌ Failed:", e);
      const msg = e.message || "Failed to save transaction";
      setApiError(msg);
      message.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  // ─── shared styles ──────────────────────────────────────────────────────────
  const labelStyle = "block text-xs font-semibold text-gray-600 mb-1.5";

  const inputCls = {
    width: "100%",
    padding: "8px 10px",
    borderRadius: "8px",
    border: "1.5px solid #e5e7eb",
    fontSize: "13px",
    color: "#111827",
    background: "#fff",
    outline: "none",
    transition: "border-color .2s, box-shadow .2s",
  };

  const focusStyle = (e) => {
    e.target.style.borderColor = "#d97706";
    e.target.style.boxShadow = "0 0 0 3px rgba(217,119,6,0.15)";
  };

  const blurStyle = (e) => {
    e.target.style.borderColor = "#e5e7eb";
    e.target.style.boxShadow = "none";
  };

  // ─── render ─────────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen flex flex-col" style={{ background: "#fafafa" }}>

      {/* HEADER */}
      <header className="shrink-0 px-6 py-3 flex items-center justify-between" style={{ background: "#fff", borderBottom: "1px solid #e5e7eb", boxShadow: "0 1px 3px rgba(0,0,0,0.06)" }}>
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl flex items-center justify-center" style={{ background: "linear-gradient(135deg,#d97706,#f59e0b)" }}>
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#fff" strokeWidth="2.5"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>
          </div>
          <div>
            <h1 className="text-base font-bold" style={{ color: "#111827" }}>Weighing</h1>
            <p className="text-xs" style={{ color: "#6b7280" }}>Capture or enter weight</p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <span className="px-2.5 py-0.5 rounded-full text-xs font-bold" style={{ background: "#dcfce7", color: "#16a34a", border: "1px solid #bbf7d0" }}>● LIVE</span>
          <span className="text-xs font-medium" style={{ color: "#6b7280" }}>Self-Service Kiosk</span>
        </div>
      </header>

      {/* BODY */}
      <div className="flex-1 overflow-auto px-6 py-5 flex flex-col gap-5">

        {/* ROW 1: Vehicle | Driver | Weight */}
        <div className="grid grid-cols-12 gap-4">

          {/* Vehicle card */}
          <div className="col-span-3 rounded-xl overflow-hidden" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
            <div className="px-4 py-2.5" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Vehicle</p>
            </div>
            <div className="p-4">
              <Select
                showSearch
                placeholder="Search vehicle / plate..."
                onSearch={debounced.vehicles}
                onChange={(id) => pick(vehicles, id, "vehicleID", "noPlate")}
                value={form.vehicleID}
                style={{ width: "100%" }}
                allowClear
              >
                {vehicles.map(v => (
                  <Option key={v.id} value={v.id}>
                    {v.registrationNumber || v.plateNumber || v.noPlate}
                  </Option>
                ))}
              </Select>
              <p className="mt-3 text-2xl font-bold font-mono text-center" style={{ color: "#111827" }}>
                {form.noPlate || "---"}
              </p>
              <div className="mt-3 h-[90px] bg-gray-100 rounded-lg flex items-center justify-center border border-dashed border-gray-300">
                <span className="text-xs text-gray-500">Vehicle Image / ANPR</span>
              </div>
            </div>
          </div>

          {/* Driver card */}
          <div className="col-span-4 rounded-xl overflow-hidden" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
            <div className="px-4 py-2.5" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Driver Details</p>
            </div>
            <div className="p-4">
              <Select
                showSearch
                placeholder="Search driver..."
                onSearch={debounced.drivers}
                onChange={(id) => pick(drivers, id, "driverID", "driverName")}
                value={form.driverID}
                style={{ width: "100%" }}
                allowClear
              >
                {drivers.map(d => (
                  <Option key={d.id} value={d.id}>
                    {d.fullName || d.name}
                  </Option>
                ))}
              </Select>

              <div className="mt-4 space-y-1 text-sm">
                <div className="flex justify-between"><span style={{ color: "#6b7280" }}>Name</span><span>{form.driverName || "---"}</span></div>
                {/* Add more driver fields if available in your data */}
              </div>
            </div>
          </div>

          {/* Weight card */}
          <div className="col-span-5 rounded-xl overflow-hidden flex flex-col" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
            <div className="px-4 py-2.5 flex items-center justify-between" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Weight Reading</p>
              <div className="flex rounded-lg overflow-hidden" style={{ border: "1.5px solid #e5e7eb" }}>
                {["captured", "manual"].map(mode => (
                  <button
                    key={mode}
                    onClick={() => setWeightMode(mode)}
                    className="px-3 py-0.5 text-xs font-semibold transition-all"
                    style={{
                      background: weightMode === mode ? "#d97706" : "#fff",
                      color:      weightMode === mode ? "#fff"     : "#6b7280",
                    }}
                  >
                    {mode === "captured" ? "⚡ Captured" : "✏️ Manual"}
                  </button>
                ))}
              </div>
            </div>

            <div className="flex-1 flex flex-col items-center justify-center py-4 gap-2">
              {weightMode === "captured" ? (
                <>
                  <p className="text-6xl font-mono font-bold" style={{ color: isStable ? "#16a34a" : "#d97706" }}>
                    {capturedWeight.toLocaleString()}
                  </p>
                  <p className="text-sm font-semibold" style={{ color: "#9ca3af" }}>KG</p>
                  <span
                    className="px-3 py-0.5 rounded-full text-xs font-bold"
                    style={{
                      background: isStable ? "#dcfce7" : "#fef3c7",
                      color:      isStable ? "#16a34a" : "#d97706",
                      border:     isStable ? "1px solid #bbf7d0" : "1px solid #fcd34d",
                    }}
                  >
                    {isStable ? "● Stable" : "● Stabilizing…"}
                  </span>
                </>
              ) : (
                <>
                  <input
                    type="number"
                    placeholder="0"
                    value={manualWeight}
                    onChange={(e) => setManualWeight(e.target.value)}
                    style={{ ...inputCls, width: "180px", fontSize: "32px", textAlign: "center" }}
                    onFocus={focusStyle}
                    onBlur={blurStyle}
                  />
                  <p className="text-sm font-semibold" style={{ color: "#9ca3af" }}>KG  (manual entry)</p>
                </>
              )}
            </div>
          </div>
        </div>

        {/* Transaction Details */}
        <div className="rounded-xl overflow-hidden" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
          <div className="px-5 py-3 flex items-center justify-between" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
            <div className="flex items-center gap-2">
              <div className="w-1 h-5 rounded-full" style={{ background: "linear-gradient(180deg,#d97706,#f59e0b)" }} />
              <p className="text-sm font-bold" style={{ color: "#111827" }}>Transaction Details</p>
            </div>
          </div>

          <div className="p-5">
            <div className="grid grid-cols-4 gap-4 mb-4">
              <div>
                <label className={labelStyle}>Transporter <span style={{ color: "#ef4444" }}>*</span></label>
                <Select
                  showSearch
                  placeholder="Search transporter…"
                  onSearch={debounced.transporters}
                  onChange={id => pick(transporters, id, "transporterID", "transporterName")}
                  value={form.transporterID}
                  style={{ width: "100%" }}
                  allowClear
                >
                  {transporters.map(item => (
                    <Option key={item.id} value={item.id}>{item.name}</Option>
                  ))}
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Commodity</label>
                <Select
                  showSearch
                  placeholder="Search commodity…"
                  onSearch={debounced.products}
                  onChange={id => pick(products, id, "commodityID", "commodityName")}
                  value={form.commodityID}
                  style={{ width: "100%" }}
                  allowClear
                >
                  {products.map(item => (
                    <Option key={item.id} value={item.id}>{item.name}</Option>
                  ))}
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Supplier</label>
                <Select
                  showSearch
                  placeholder="Search supplier…"
                  onSearch={debounced.suppliers}
                  onChange={id => pick(suppliers, id, "supplierID", "supplierName")}
                  value={form.supplierID}
                  style={{ width: "100%" }}
                  allowClear
                >
                  {suppliers.map(item => (
                    <Option key={item.id} value={item.id}>{item.name}</Option>
                  ))}
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Customer Name</label>
                <Input
                  placeholder="Customer name"
                  value={form.customerName}
                  onChange={e => setField("customerName", e.target.value)}
                  style={inputCls}
                  onFocus={focusStyle}
                  onBlur={blurStyle}
                />
              </div>
            </div>

            <div className="grid grid-cols-4 gap-4 mb-4">
              <div>
                <label className={labelStyle}>Origin</label>
                <Input
                  placeholder="Origin"
                  value={form.originName}
                  onChange={e => setField("originName", e.target.value)}
                  style={inputCls}
                  onFocus={focusStyle}
                  onBlur={blurStyle}
                />
              </div>

              <div>
                <label className={labelStyle}>Destination</label>
                <Input
                  placeholder="Destination"
                  value={form.destinationName}
                  onChange={e => setField("destinationName", e.target.value)}
                  style={inputCls}
                  onFocus={focusStyle}
                  onBlur={blurStyle}
                />
              </div>

              <div>
                <label className={labelStyle}>Weighbridge <span style={{ color: "#ef4444" }}>*</span></label>
                <Select
                  placeholder="Select weighbridge…"
                  value={form.scaleName || form.weighBridgeName}
                  onChange={v => {
                    const wb = weighbridges.find(w => (w.location || w.name) === v);
                    setForm(p => ({ ...p, weighBridgeID: wb?.id || null, weighBridgeName: v, scaleName: v }));
                  }}
                  style={{ width: "100%" }}
                >
                  {weighbridges.map(wb => (
                    <Option key={wb.id} value={wb.location || wb.name}>{wb.location || wb.name}</Option>
                  ))}
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Weigh Mode</label>
                <Select
                  value={form.weighMode}
                  onChange={v => setField("weighMode", v)}
                  style={{ width: "100%" }}
                >
                  <Option value="entry">Entry</Option>
                  <Option value="Gross/Tare">Kiosk</Option>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelStyle}>Operation</label>
                <Select
                  value={form.operation}
                  onChange={v => setField("operation", v)}
                  style={{ width: "100%" }}
                >
                  <Option value="weighing">Weighing</Option>
                  <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                  <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Notes</label>
                <TextArea
                  rows={2}
                  placeholder="Additional notes…"
                  value={form.notes}
                  onChange={e => setField("notes", e.target.value)}
                  style={{ ...inputCls, resize: "vertical" }}
                  onFocus={focusStyle}
                  onBlur={blurStyle}
                />
              </div>
            </div>
          </div>
        </div>

        {/* Error banner */}
        {(apiError || parentError) && (
          <div className="rounded-xl px-4 py-3 flex items-center gap-3" style={{ background: "#fef2f2", border: "1px solid #fecaca" }}>
            <span className="text-lg">⚠️</span>
            <p className="text-sm font-medium" style={{ color: "#dc2626" }}>{apiError || parentError}</p>
          </div>
        )}
      </div>

      {/* FOOTER */}
      <footer className="shrink-0 px-6 py-4 flex items-center justify-between" style={{ background: "#fff", borderTop: "1px solid #e5e7eb", boxShadow: "0 -1px 3px rgba(0,0,0,0.06)" }}>
        <div className="flex items-center gap-4">
          <button
            onClick={onBack}
            className="px-4 py-2 rounded-lg text-sm font-semibold transition-all hover:shadow-md"
            style={{ background: "#f3f4f6", color: "#374151", border: "1px solid #e5e7eb" }}
          >
            ← Back
          </button>
          <div className="text-sm" style={{ color: "#6b7280" }}>
            Weight: <span className="font-bold" style={{ color: "#111827" }}>{effectiveWeight.toLocaleString()} kg</span>
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
              : "linear-gradient(135deg, #d97706, #f59e0b)",
            boxShadow: submitting ? "none" : "0 3px 10px rgba(217,119,6,0.35)",
            cursor: submitting ? "not-allowed" : "pointer",
          }}
        >
          {submitting ? "Saving…" : "⚡ Capture Weight"}
        </button>
      </footer>
    </div>
  );
}