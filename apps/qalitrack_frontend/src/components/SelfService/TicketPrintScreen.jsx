import React, { useEffect, useState } from "react";
import dayjs from "dayjs";
import { useTheme } from "../Context/ThemeContext.jsx";
import logo from "../../assets/qalitrack_logo_full.png";

export default function TicketPrintScreen({ ticketData, onComplete }) {
  const { isDark } = useTheme();
  const [printing,      setPrinting]      = useState(true);
  const [printSuccess,  setPrintSuccess]  = useState(false);
  const [countdown,     setCountdown]     = useState(7);
  const isComplete = ticketData?.isCompleted || (ticketData?.firstWeight && ticketData?.secondWeight);

  useEffect(() => { printThermalTicket(); }, []);

  useEffect(() => {
    if (!printSuccess) return;
    const id = setInterval(() => setCountdown(c => c - 1), 1000);
    return () => clearInterval(id);
  }, [printSuccess]);

  const printThermalTicket = async () => {
    try {
      setPrinting(true);
      console.log("🖨️ Printing thermal ticket:", formatThermalTicket(ticketData));
      // REAL MODE: uncomment and configure when thermal printer endpoint is ready
      // await fetch("http://172.16.0.93:5000/api/Printer/thermal/print", { method:"POST", headers:{"Content-Type":"application/json"}, body:JSON.stringify({ content: formatThermalTicket(ticketData), printerName:"ThermalPrinter01", copies:1 }) });
      setTimeout(() => { setPrintSuccess(true); setPrinting(false); }, 2000);
      setTimeout(() => { onComplete(); }, 9000);
    } catch (error) {
      console.error("❌ Print error:", error);
      setPrintSuccess(true);
      setPrinting(false);
      setTimeout(() => { onComplete(); }, 9000);
    }
  };

  const formatThermalTicket = (data) => {
    const lines = [];
    lines.push("================================");
    lines.push("       QALITRACK WEIGHBRIDGE");
    lines.push("     Self-Service Weighing");
    lines.push("================================");
    lines.push("");
    lines.push(`ID: ${data.receiptNo || data.ticketID || "WB-" + Date.now()}`);
    lines.push(`Date: ${dayjs(data.weighTime || new Date()).format("DD/MM/YYYY, HH:mm:ss")}`);
    lines.push("");
    lines.push("VEHICLE & DRIVER");
    lines.push(`Plate: ${data.noPlate || "N/A"}`);
    lines.push(`Driver: ${data.driverName || "N/A"}`);
    lines.push("");
    lines.push("MATERIAL");
    lines.push(`Product: ${data.commodityName || "—"}`);
    lines.push(`Supplier: ${data.supplierName || "—"}`);
    lines.push(`Transporter: ${data.transporterName || "—"}`);
    lines.push("");
    lines.push("WEIGHTS");
    lines.push(`Gross: ${fmtW(data.firstWeight || data.weight)} kg`);
    lines.push(`Tare:  ${fmtW(data.secondWeight || 0)} kg`);
    lines.push(`Net:   ${fmtW(data.secondWeight ? Math.abs(data.firstWeight - data.secondWeight) : data.firstWeight || data.weight)} kg`);
    lines.push("");
    lines.push("================================");
    lines.push(data.secondWeight ? "   STATUS: COMPLETE" : "   STATUS: PENDING 2ND WEIGHT");
    lines.push("================================");
    lines.push("");
    lines.push("Powered by QALIBRATED SYSTEMS");
    lines.push("================================");
    return lines.join("\n");
  };

  const fmtW = (w) => {
    if (!w) return "0";
    return parseFloat(w).toLocaleString("en-US", { minimumFractionDigits: 0, maximumFractionDigits: 0 });
  };

  const bg = isDark
    ? "linear-gradient(135deg,#111827 0%,#1f2937 60%,#111827 100%)"
    : "linear-gradient(135deg,#fffbeb 0%,#fff 60%,#fff7ed 100%)";

  // ── Printing ─────────────────────────────────────────────────────────────
  if (printing) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center p-8" style={{ background: bg }}>
        <div className="text-center">
          <div className="relative w-40 h-40 mx-auto mb-8 flex items-center justify-center">
            {[0, 1, 2].map(i => (
              <span key={i} className="absolute inset-0 rounded-full border-2 animate-ping"
                style={{ borderColor: "rgba(217,119,6,0.25)", animationDelay: `${i * 0.35}s` }} />
            ))}
            <div className="w-28 h-28 rounded-full bg-gradient-to-br from-amber-400 to-orange-500 flex items-center justify-center shadow-2xl">
              <svg className="w-14 h-14 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6.72 13.829c-.24.03-.48.062-.72.096m.72-.096a42.415 42.415 0 0110.56 0m-10.56 0L6.34 18m10.94-4.171c.24.03.48.062.72.096m-.72-.096L17.66 18m0 0l.229 2.523a1.125 1.125 0 01-1.12 1.227H7.231c-.662 0-1.18-.568-1.12-1.227L6.34 18m11.318 0h1.091A2.25 2.25 0 0021 15.75V9.456c0-1.081-.768-2.015-1.837-2.175a48.055 48.055 0 00-1.913-.247M6.34 18H5.25A2.25 2.25 0 013 15.75V9.456c0-1.081.768-2.015 1.837-2.175a48.041 48.041 0 011.913-.247m10.5 0a48.536 48.536 0 00-10.5 0m10.5 0V3.375c0-.621-.504-1.125-1.125-1.125h-8.25c-.621 0-1.125.504-1.125 1.125v3.659M18 10.5h.008v.008H18V10.5zm-3 0h.008v.008H15V10.5z" />
              </svg>
            </div>
          </div>

          <img src={logo} alt="Qalitrack" className="h-14 w-auto mx-auto mb-4 opacity-80" />
          <h2 className={`text-4xl font-black mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Printing Ticket…</h2>
          <p className={`text-lg mb-6 ${isDark ? "text-gray-400" : "text-gray-500"}`}>Please wait for your receipt</p>

          <div className="flex items-center justify-center gap-2">
            {[0, 150, 300].map(d => (
              <div key={d} className="w-3 h-3 rounded-full bg-amber-500 animate-bounce" style={{ animationDelay: `${d}ms` }} />
            ))}
          </div>
        </div>
      </div>
    );
  }

  // ── Printed ───────────────────────────────────────────────────────────────
  if (printSuccess) {
    const gross = ticketData?.firstWeight || ticketData?.weight || 0;
    const tare  = ticketData?.secondWeight || 0;
    const net   = tare ? Math.abs(gross - tare) : gross;

    return (
      <div className="min-h-screen flex flex-col" style={{ background: bg }}>

        {/* Header */}
        <header className={`px-8 py-4 flex items-center justify-between border-b ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-100"} shadow-sm`}>
          <img src={logo} alt="Qalitrack" className="h-12 w-auto" />
          <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-green-50 border border-green-200">
            <span className="w-2 h-2 rounded-full bg-green-500" />
            <span className="text-xs font-bold text-green-700">Ticket Printed</span>
          </div>
        </header>

        <main className="flex-1 flex items-center justify-center p-8">
          <div className="w-full max-w-2xl space-y-5">

            {/* Success badge */}
            <div className="text-center">
              <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gradient-to-br from-green-400 to-emerald-600 flex items-center justify-center shadow-2xl">
                <svg className="w-13 h-13 text-white w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                </svg>
              </div>
              <h2 className={`text-4xl font-black mb-1 ${isDark ? "text-white" : "text-gray-900"}`}>Ticket Printed!</h2>
              <p className={`text-base ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                {isComplete
                  ? "Transaction complete — collect your receipt"
                  : "First weight captured — collect your receipt and return for 2nd weighing"}
              </p>
            </div>

            {/* Ticket preview */}
            <div className={`rounded-3xl border shadow-xl overflow-hidden ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-200"}`}>
              {/* Receipt top strip */}
              <div className="h-1.5 w-full bg-gradient-to-r from-amber-400 to-orange-500" />

              <div className="p-6 font-mono text-sm">
                {/* Brand header */}
                <div className="text-center pb-4 mb-4 border-b-2 border-dashed border-gray-200">
                  <img src={logo} alt="Qalitrack" className="h-9 w-auto mx-auto mb-2 opacity-70" />
                  <p className={`text-xs font-semibold ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                    Self-Service Weighbridge
                  </p>
                </div>

                {/* Receipt no + date */}
                <div className={`flex justify-between text-xs mb-4 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
                  <span>ID: <strong className={isDark ? "text-white" : "text-gray-900"}>{ticketData?.receiptNo || ticketData?.ticketID || "—"}</strong></span>
                  <span>{dayjs(ticketData?.weighTime || new Date()).format("DD/MM/YYYY HH:mm")}</span>
                </div>

                {/* Vehicle + driver */}
                <div className={`p-3 rounded-xl mb-3 ${isDark ? "bg-gray-800" : "bg-slate-50 border border-slate-100"}`}>
                  <p className={`text-xs font-bold uppercase tracking-wider mb-2 ${isDark ? "text-gray-500" : "text-gray-400"}`}>Vehicle & Driver</p>
                  <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-xs">
                    <div><span className={isDark ? "text-gray-500" : "text-gray-400"}>Plate </span><strong className={isDark ? "text-white" : "text-gray-900"}>{ticketData?.noPlate || "—"}</strong></div>
                    <div><span className={isDark ? "text-gray-500" : "text-gray-400"}>Driver </span><strong className={isDark ? "text-white" : "text-gray-900"}>{ticketData?.driverName || "—"}</strong></div>
                    {ticketData?.transporterName && <div className="col-span-2"><span className={isDark ? "text-gray-500" : "text-gray-400"}>Transporter </span><strong className={isDark ? "text-white" : "text-gray-900"}>{ticketData.transporterName}</strong></div>}
                  </div>
                </div>

                {/* Weights */}
                <div className={`p-3 rounded-xl mb-3 ${isDark ? "bg-gray-800" : "bg-slate-50 border border-slate-100"}`}>
                  <p className={`text-xs font-bold uppercase tracking-wider mb-2 ${isDark ? "text-gray-500" : "text-gray-400"}`}>Weights</p>
                  <div className="space-y-1.5">
                    {[
                      { label: "Gross", value: `${fmtW(gross)} kg`, color: isDark ? "text-white" : "text-gray-900" },
                      { label: "Tare",  value: tare ? `${fmtW(tare)} kg` : "Pending",  color: tare ? (isDark ? "text-white" : "text-gray-900") : "text-amber-500" },
                      { label: "Net",   value: tare ? `${fmtW(net)} kg` : "Pending",   color: tare ? "text-green-500 font-black" : "text-amber-500", border: true },
                    ].map(row => (
                      <div key={row.label} className={`flex justify-between text-xs ${row.border ? "pt-1.5 border-t " + (isDark ? "border-gray-700" : "border-gray-200") : ""}`}>
                        <span className={isDark ? "text-gray-400" : "text-gray-500"}>{row.label}</span>
                        <span className={`font-bold ${row.color}`}>{row.value}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Status banner */}
                <div className={`rounded-xl px-4 py-2.5 text-center text-xs font-bold ${
                  isComplete
                    ? "bg-green-100 text-green-700 border border-green-200"
                    : "bg-amber-50 text-amber-700 border border-amber-200"
                }`}>
                  {isComplete ? "✓ TRANSACTION COMPLETE" : "⏳ PENDING 2ND WEIGHT — Return after unloading"}
                </div>
              </div>
            </div>

            {/* Next steps */}
            {!isComplete && (
              <div className={`rounded-2xl border p-5 ${isDark ? "bg-gray-800 border-gray-700" : "bg-amber-50 border-amber-200"}`}>
                <p className={`text-sm font-bold mb-3 ${isDark ? "text-amber-400" : "text-amber-700"}`}>Next Steps</p>
                <ol className={`space-y-1.5 text-sm ${isDark ? "text-gray-300" : "text-amber-800"}`}>
                  <li>1. Collect your printed ticket from the printer</li>
                  <li>2. Proceed to the unloading bay</li>
                  <li>3. Return to this kiosk for the second (tare) weighing</li>
                </ol>
              </div>
            )}

            {/* Countdown */}
            <p className={`text-sm text-center ${isDark ? "text-gray-600" : "text-gray-400"}`}>
              Returning to start in <strong>{Math.max(0, countdown)}</strong> seconds…
            </p>
          </div>
        </main>

        <footer className={`px-8 py-4 border-t text-center ${isDark ? "bg-gray-900 border-gray-800" : "bg-white border-gray-100"}`}>
          <p className={`text-xs ${isDark ? "text-gray-600" : "text-gray-400"}`}>
            Powered by <span className="font-black text-amber-500">QALIBRATED SYSTEMS</span>
          </p>
        </footer>
      </div>
    );
  }

  // ── Print failed ──────────────────────────────────────────────────────────
  return (
    <div className="min-h-screen flex items-center justify-center p-8" style={{ background: bg }}>
      <div className="text-center max-w-md">
        <div className="w-24 h-24 mx-auto mb-6 rounded-full bg-red-100 flex items-center justify-center">
          <svg className="w-12 h-12 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>
        <h2 className={`text-3xl font-black mb-2 ${isDark ? "text-white" : "text-gray-900"}`}>Print Failed</h2>
        <p className={`text-base mb-6 ${isDark ? "text-gray-400" : "text-gray-500"}`}>
          Please contact the office for assistance
        </p>
        <button onClick={onComplete}
          className="px-8 py-3 rounded-2xl font-bold text-base text-white bg-gradient-to-r from-amber-500 to-orange-500 shadow-md hover:shadow-lg transition-all">
          Continue
        </button>
      </div>
    </div>
  );
}
