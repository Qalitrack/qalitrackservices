/**
 * WeighingScreen.jsx  —  Kiosk weighing step
 * Now uses the same payload construction + GUID safety logic as CreateTransactionForm
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

// Kiosk uses the same wrapper your original code had
async function postTransaction(body) {
  return apiFetch(`/transactions`, {
    method: "POST",
    body: JSON.stringify({ request: body })
  });
}

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

  // ── UI state ──
  const [submitting, setSubmitting] = useState(false);
  const [apiError,   setApiError]   = useState(null);

  // ── stability helpers ──
  const bufferRef          = useRef(null);
  const lastStableRef      = useRef(null);
  const stabilityCounter   = useRef(0);
  const STABILITY_CYCLES   = 5;

  // ── load weighbridges ──
  useEffect(() => {
    getWeighbridges().then(setWeighbridges).catch(() => {});
  }, []);

  // ── simulated weight ──
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

  const effectiveWeight = weightMode === "manual"
    ? Number(manualWeight) || 0
    : capturedWeight;

  // ── Helper: same GUID check as CreateTransactionForm ────────────────────────
  const isValidGuid = (guid) => {
    return guid &&
           guid !== "00000000-0000-0000-0000-000000000000" &&
           guid.length > 0;
  };

  // ── CAPTURE / SUBMIT ── with CreateTransactionForm-style payload logic ──────
  const handleCapture = async () => {
    setApiError(null);

    // Validation ── aligned with CreateTransactionForm
    if (effectiveWeight <= 0) {
      message.error("Valid weight is required");
      return;
    }
    if (!form.noPlate?.trim()) {
      message.error("Vehicle plate is required");
      return;
    }
    if (!form.weighBridgeID) {
      message.error("Weighbridge is required");
      return;
    }
    if (!form.transporterID) {
      message.error("Transporter is required");
      return;
    }

    setSubmitting(true);

    try {
      // Build payload — exactly like CreateTransactionForm first weighing logic
      const payload = {
        noPlate: form.noPlate.toUpperCase().trim(),
        firstWeight: String(effectiveWeight),
        weighMode: form.weighMode || "entry",
        operation: form.operation || "weighing",
      };

      // Operator (kiosk version — no real user, but still include field)
      payload.operatorName = "Self-Service Kiosk";
      payload.operatorID = null; // or "00000000-0000-0000-0000-000000000000" if backend requires it

      // ── Only add fields that exist and are valid ───────────────────────────
      if (form.driverName?.trim()) payload.driverName = form.driverName.trim();

      if (isValidGuid(form.vehicleID)) payload.vehicleID = form.vehicleID;

      if (isValidGuid(form.transporterID)) {
        payload.transporterID = form.transporterID;
        if (form.transporterName?.trim()) payload.transporterName = form.transporterName.trim();
      }

      if (isValidGuid(form.weighBridgeID)) {
        payload.weighBridgeID = form.weighBridgeID;
        if (form.weighBridgeName?.trim() || form.scaleName?.trim()) {
          payload.weighBridgeName = form.weighBridgeName || form.scaleName || "";
        }
      }

      if (form.scaleName?.trim()) payload.scaleName = form.scaleName.trim();

      if (isValidGuid(form.commodityID)) {
        payload.commodityID = form.commodityID;
        if (form.commodityName?.trim()) payload.commodityName = form.commodityName.trim();
      }

      if (isValidGuid(form.supplierID)) {
        payload.supplierID = form.supplierID;
        if (form.supplierName?.trim()) payload.supplierName = form.supplierName.trim();
      }

      if (form.customerName?.trim()) payload.customerName = form.customerName.trim();

      if (form.originName?.trim()) payload.originName = form.originName.trim();
      if (form.destinationName?.trim()) payload.destinationName = form.destinationName.trim();

      if (form.notes?.trim()) payload.notes = form.notes.trim();

      console.log("📤 Kiosk → backend payload (CreateTransactionForm style):", payload);

      const result = await postTransaction(payload);

      message.success({
        content: `Transaction saved! Receipt: ${result.receiptNo || result.ticketID || "Generated"}`,
        duration: 5,
      });

      onWeighingComplete?.({
        ...payload,
        ...result,
        weight: effectiveWeight,
        weightMode,
      });
    } catch (err) {
      console.error("❌ Transaction failed:", err);
      const msg = err.message || "Failed to save transaction";
      setApiError(msg);
      message.error(msg);
    } finally {
      setSubmitting(false);
    }
  };

  // ─── shared input style ─────────────────────────────────────────────────────
  const labelStyle = "block text-xs font-semibold text-gray-600 mb-1.5";

  // ─── render ─────────────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen flex flex-col" style={{ background: "#fafafa" }}>

      {/* HEADER – your original */}
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

        {/* Top row – Vehicle | Driver | Weight – your original structure */}
        <div className="grid grid-cols-12 gap-4">
          {/* Vehicle card */}
          <div className="col-span-3 rounded-xl overflow-hidden" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
            <div className="px-4 py-2.5" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
              <p className="text-xs font-bold uppercase tracking-wide" style={{ color: "#d97706" }}>Vehicle</p>
            </div>
            <div className="p-4">
              <Select
                showSearch
                placeholder="Search vehicle..."
                onSearch={debounced.vehicles}
                onChange={id => pick(vehicles, id, "vehicleID", "noPlate")}
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
                <span className="text-xs text-gray-500">Vehicle Image</span>
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
                onChange={id => pick(drivers, id, "driverID", "driverName")}
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
              </div>
            </div>
          </div>

          {/* Weight card – unchanged */}
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
                    style={{ width: "180px", fontSize: "32px", textAlign: "center", padding: "10px", border: "1.5px solid #e5e7eb", borderRadius: "8px" }}
                  />
                  <p className="text-sm font-semibold" style={{ color: "#9ca3af" }}>KG (manual)</p>
                </>
              )}
            </div>
          </div>
        </div>

        {/* Transaction form – full fields, Antd Select */}
        <div className="rounded-xl overflow-hidden" style={{ background: "#fff", border: "1px solid #e5e7eb", boxShadow: "0 1px 4px rgba(0,0,0,0.06)" }}>
          <div className="px-5 py-3 flex items-center gap-2" style={{ background: "linear-gradient(135deg,#fffbeb,#fff7ed)", borderBottom: "1px solid #f0ecdf" }}>
            <div className="w-1 h-5 rounded-full" style={{ background: "linear-gradient(180deg,#d97706,#f59e0b)" }} />
            <p className="text-sm font-bold" style={{ color: "#111827" }}>Transaction Details</p>
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
                  {transporters.map(t => <Option key={t.id} value={t.id}>{t.name}</Option>)}
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
                  {products.map(p => <Option key={p.id} value={p.id}>{p.name}</Option>)}
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
                  {suppliers.map(s => <Option key={s.id} value={s.id}>{s.name}</Option>)}
                </Select>
              </div>

              <div>
                <label className={labelStyle}>Customer Name</label>
                <Input
                  placeholder="Customer name"
                  value={form.customerName}
                  onChange={e => setField("customerName", e.target.value)}
                  style={{ width: "100%", padding: "8px 10px", borderRadius: "6px", border: "1px solid #d1d5db" }}
                />
              </div>
            </div>

            {/* More fields – origin, destination, weighbridge, etc. */}
            <div className="grid grid-cols-4 gap-4 mb-4">
              <div>
                <label className={labelStyle}>Origin</label>
                <Input placeholder="Origin" value={form.originName} onChange={e => setField("originName", e.target.value)} />
              </div>
              <div>
                <label className={labelStyle}>Destination</label>
                <Input placeholder="Destination" value={form.destinationName} onChange={e => setField("destinationName", e.target.value)} />
              </div>
              <div>
                <label className={labelStyle}>Weighbridge <span style={{ color: "#ef4444" }}>*</span></label>
                <Select
                  placeholder="Select weighbridge"
                  value={form.scaleName || form.weighBridgeName}
                  onChange={v => {
                    const wb = weighbridges.find(w => (w.location || w.name) === v);
                    setForm(p => ({ ...p, weighBridgeID: wb?.id || null, weighBridgeName: v, scaleName: v }));
                  }}
                  style={{ width: "100%" }}
                >
                  {weighbridges.map(wb => <Option key={wb.id} value={wb.location || wb.name}>{wb.location || wb.name}</Option>)}
                </Select>
              </div>
              <div>
                <label className={labelStyle}>Weigh Mode</label>
                <Select value={form.weighMode} onChange={v => setField("weighMode", v)} style={{ width: "100%" }}>
                  <Option value="entry">Entry</Option>
                  <Option value="Gross/Tare">Gross / Tare</Option>
                </Select>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={labelStyle}>Operation</label>
                <Select value={form.operation} onChange={v => setField("operation", v)} style={{ width: "100%" }}>
                  <Option value="weighing">Weighing</Option>
                  <Option value="Inbound Product Receipt">Inbound Receipt</Option>
                  <Option value="Outbound Product Dispatch">Outbound Dispatch</Option>
                </Select>
              </div>
              <div>
                <label className={labelStyle}>Notes</label>
                <TextArea rows={2} value={form.notes} onChange={e => setField("notes", e.target.value)} placeholder="Notes..." />
              </div>
            </div>
          </div>
        </div>

        {/* Error */}
        {(apiError || parentError) && (
          <div className="bg-red-50 border border-red-200 p-4 rounded text-red-700">
            {apiError || parentError}
          </div>
        )}
      </div>

      {/* FOOTER */}
      <footer className="shrink-0 px-6 py-4 flex justify-between items-center bg-white border-t border-gray-200">
        <button onClick={onBack} className="px-5 py-2 bg-gray-100 rounded-lg">
          ← Back
        </button>
        <button
          onClick={handleCapture}
          disabled={submitting || (weightMode === "captured" && !isStable) || effectiveWeight <= 0}
          className="px-8 py-3 bg-amber-600 text-white rounded-lg disabled:bg-gray-400"
        >
          {submitting ? "Saving..." : "Capture Weight"}
        </button>
      </footer>
    </div>
  );
}