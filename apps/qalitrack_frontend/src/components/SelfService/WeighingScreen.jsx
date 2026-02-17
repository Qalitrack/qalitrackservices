/**
 * AuthenticationMethodModal.jsx
 *
 * SIMULATION MODE — NFC hardware not yet configured.
 * Simulates a card tap after 3 seconds, then auto-advances with mock driver data.
 * No apiClient calls → no auth interceptors → no login redirect.
 *
 * When real NFC is ready, replace the SIMULATION BLOCK with the real SSE stream.
 */

import React, { useEffect, useRef, useState } from "react";
import { Modal } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";

// ── Mock drivers — swap for real API lookup when NFC is configured ────────────
const MOCK_DRIVERS = [
  {
    id:         "DRV-001",
    uid:        "NFC-SIMULATED-001",
    name:       "John Kamau Mwangi",
    employeeId: "EMP-2841",
    phone:      "+254 712 345 678",
    licenseNo:  "DL-294817",
    role:       "Senior Driver",
    photoUrl:   null,
  },
  {
    id:         "DRV-002",
    uid:        "NFC-SIMULATED-002",
    name:       "Mary Wanjiku Njoroge",
    employeeId: "EMP-1093",
    phone:      "+254 723 456 789",
    licenseNo:  "DL-187263",
    role:       "Driver",
    photoUrl:   null,
  },
  {
    id:         "DRV-003",
    uid:        "NFC-SIMULATED-003",
    name:       "Peter Otieno Odhiambo",
    employeeId: "EMP-3372",
    phone:      "+254 734 567 890",
    licenseNo:  "DL-356981",
    role:       "Driver",
    photoUrl:   null,
  },
];

// ─── COMPONENT ────────────────────────────────────────────────────────────────
export default function AuthenticationMethodModal({ visible, onClose, onSelectNFC }) {
  const { isDark } = useTheme();

  // status: waiting | tapping | reading | success
  const [status,     setStatus]     = useState("waiting");
  const [driverData, setDriverData] = useState(null);
  const [countdown,  setCountdown]  = useState(3);
  const [dotCount,   setDotCount]   = useState(0);

  const timerRef     = useRef(null);
  const countdownRef = useRef(null);
  const dotRef       = useRef(null);

  // Clear all timers
  const clearAll = () => {
    clearTimeout(timerRef.current);
    clearInterval(countdownRef.current);
    clearInterval(dotRef.current);
  };

  // Reset when modal opens / closes
  useEffect(() => {
    if (visible) {
      setStatus("waiting");
      setDriverData(null);
      setCountdown(3);
      setDotCount(0);
      startSimulation();
    } else {
      clearAll();
    }
    return clearAll;
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [visible]);

  // Animated dots
  useEffect(() => {
    if (status !== "waiting") return;
    dotRef.current = setInterval(() => setDotCount(d => (d + 1) % 4), 500);
    return () => clearInterval(dotRef.current);
  }, [status]);

  const startSimulation = () => {
    clearAll();

    // Countdown display: 3 → 2 → 1
    setCountdown(3);
    countdownRef.current = setInterval(() => {
      setCountdown(c => {
        if (c <= 1) { clearInterval(countdownRef.current); return 0; }
        return c - 1;
      });
    }, 1000);

    // After 3s: simulate tap
    timerRef.current = setTimeout(() => {
      clearInterval(countdownRef.current);
      setStatus("tapping");

      // After 0.6s: show "reading"
      setTimeout(() => {
        setStatus("reading");
        const mock = MOCK_DRIVERS[Math.floor(Math.random() * MOCK_DRIVERS.length)];

        // After 1.2s: show success
        setTimeout(() => {
          setDriverData(mock);
          setStatus("success");
          console.log("🎭 Simulated NFC driver:", mock);

          // After 2s: auto-advance to weighing
          setTimeout(() => onSelectNFC(mock), 2000);
        }, 1200);
      }, 600);
    }, 3000);
  };

  const dots = ".".repeat(dotCount);

  // ─── RENDER ───────────────────────────────────────────────────────────────
  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width={520}
      centered
      closeIcon={
        <span className={`text-3xl leading-none ${isDark ? "text-gray-500 hover:text-gray-300" : "text-gray-400 hover:text-gray-700"}`}>
          ×
        </span>
      }
      styles={{
        body: {
          padding: 0,
          backgroundColor: isDark ? "#111827" : "#ffffff",
          borderRadius: "1rem",
          overflow: "hidden",
        },
        mask:    { backgroundColor: "rgba(0,0,0,0.75)" },
        content: {
          borderRadius: "1rem",
          overflow: "hidden",
          border: isDark ? "1px solid #374151" : "1px solid #e5e7eb",
        },
      }}
    >
      {/* Amber top stripe */}
      <div className="h-1.5 bg-gradient-to-r from-amber-400 via-orange-500 to-amber-600" />

      {/* Simulation badge */}
      <div className={`mx-10 mt-6 flex items-center justify-center gap-2 px-4 py-2 rounded-xl text-xs font-bold border ${
        isDark ? "bg-purple-900/30 border-purple-700 text-purple-300" : "bg-purple-50 border-purple-200 text-purple-700"
      }`}>
        <span className="w-2 h-2 bg-purple-500 rounded-full animate-pulse" />
        SIMULATION MODE — NFC hardware not yet configured
      </div>

      <div className="px-10 py-8">
        {/* Header */}
        <div className="text-center mb-8">
          <h2 className={`text-3xl font-black ${isDark ? "text-white" : "text-gray-900"}`}>
            Driver Authentication
          </h2>
          <p className={`mt-2 text-sm ${isDark ? "text-gray-400" : "text-gray-500"}`}>
            {status === "waiting"  && `Waiting for NFC card tap${dots}`}
            {status === "tapping"  && "Card detected!"}
            {status === "reading"  && "Reading card & verifying driver…"}
            {status === "success"  && "Driver authenticated!"}
          </p>
        </div>

        {/* Central visual */}
        <div className="flex justify-center mb-8">
          <div className="relative">
            {/* Pulse rings — waiting */}
            {status === "waiting" && [0, 1].map(i => (
              <span key={i} className="absolute inset-0 rounded-full border-2 animate-ping"
                style={{
                  borderColor: isDark ? "rgba(168,85,247,0.3)" : "rgba(126,34,206,0.2)",
                  animationDuration: `${1.4 + i * 0.5}s`,
                  animationDelay: `${i * 0.3}s`,
                }} />
            ))}

            <div className={`w-36 h-36 rounded-full flex flex-col items-center justify-center shadow-2xl transition-all duration-500 bg-gradient-to-br ${
              status === "success"  ? "from-green-400 to-emerald-600"
              : status === "tapping" || status === "reading" ? "from-amber-400 to-orange-500"
              : isDark ? "from-purple-700 to-indigo-700" : "from-purple-600 to-indigo-600"
            }`}>
              {status === "reading" ? (
                <div className="w-12 h-12 border-4 border-white/30 border-t-white rounded-full animate-spin" />
              ) : status === "success" ? (
                <svg className="w-16 h-16 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                </svg>
              ) : status === "tapping" ? (
                <span className="text-5xl">📲</span>
              ) : (
                <>
                  <span className="text-5xl">📡</span>
                  {countdown > 0 && (
                    <span className="text-white font-black text-lg mt-1">{countdown}s</span>
                  )}
                </>
              )}
            </div>
          </div>
        </div>

        {/* Status content */}

        {/* WAITING */}
        {status === "waiting" && (
          <div className={`rounded-2xl p-5 text-center border ${
            isDark ? "bg-gray-800/60 border-gray-700" : "bg-purple-50 border-purple-200"
          }`}>
            <p className={`font-semibold text-base mb-1 ${isDark ? "text-purple-300" : "text-purple-700"}`}>
              Simulating NFC card tap in {countdown > 0 ? `${countdown}s` : "…"}
            </p>
            <p className={`text-xs ${isDark ? "text-gray-500" : "text-gray-400"}`}>
              In production this will read the physical NFC card
            </p>
          </div>
        )}

        {/* TAPPING */}
        {status === "tapping" && (
          <div className={`rounded-2xl p-5 text-center border ${
            isDark ? "bg-amber-900/20 border-amber-700" : "bg-amber-50 border-amber-200"
          }`}>
            <p className={`font-semibold ${isDark ? "text-amber-300" : "text-amber-700"}`}>
              🎉 Card tap detected! Fetching driver details…
            </p>
          </div>
        )}

        {/* READING */}
        {status === "reading" && (
          <div className={`rounded-2xl p-5 text-center border ${
            isDark ? "bg-blue-900/20 border-blue-700" : "bg-blue-50 border-blue-200"
          }`}>
            <div className="flex items-center justify-center gap-3">
              <div className="w-4 h-4 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
              <p className={`font-semibold ${isDark ? "text-blue-300" : "text-blue-700"}`}>
                Verifying driver identity…
              </p>
            </div>
          </div>
        )}

        {/* SUCCESS */}
        {status === "success" && driverData && (
          <div className={`rounded-2xl overflow-hidden border ${
            isDark ? "border-green-700" : "border-green-300"
          }`}>
            {/* Green header */}
            <div className="bg-gradient-to-r from-green-500 to-emerald-600 px-6 py-4 flex items-center gap-4">
              <div className="w-16 h-16 rounded-full bg-white/20 flex items-center justify-center text-4xl border-4 border-white/30 shadow-lg">
                👤
              </div>
              <div>
                <p className="text-white/70 text-xs font-semibold uppercase tracking-wider">Verified Driver</p>
                <p className="text-white text-2xl font-black leading-tight">{driverData.name}</p>
                {driverData.role && <p className="text-green-100 text-xs mt-0.5">{driverData.role}</p>}
              </div>
            </div>

            {/* Details */}
            <div className={`px-6 py-4 grid grid-cols-2 gap-3 ${isDark ? "bg-gray-800" : "bg-white"}`}>
              {driverData.employeeId && <NfcField label="Employee ID" value={driverData.employeeId} isDark={isDark} mono />}
              {driverData.phone      && <NfcField label="Phone"       value={driverData.phone}      isDark={isDark} />}
              {driverData.licenseNo  && <NfcField label="Licence No." value={driverData.licenseNo}  isDark={isDark} mono />}
              <NfcField label="NFC UID" value={driverData.uid} isDark={isDark} mono />
            </div>

            <div className={`px-6 py-3 text-center text-xs ${
              isDark ? "bg-gray-900 text-gray-500" : "bg-gray-50 text-gray-400"
            }`}>
              Proceeding to weighing automatically…
            </div>
          </div>
        )}

        {/* Manual trigger button — for testing */}
        {(status === "waiting") && (
          <button
            onClick={() => {
              clearAll();
              setStatus("tapping");
              setTimeout(() => {
                setStatus("reading");
                const mock = MOCK_DRIVERS[Math.floor(Math.random() * MOCK_DRIVERS.length)];
                setTimeout(() => {
                  setDriverData(mock);
                  setStatus("success");
                  setTimeout(() => onSelectNFC(mock), 2000);
                }, 1200);
              }, 600);
            }}
            className={`w-full mt-4 py-3 rounded-xl text-sm font-bold border-2 border-dashed transition-colors ${
              isDark
                ? "border-purple-700 text-purple-400 hover:bg-purple-900/30"
                : "border-purple-300 text-purple-600 hover:bg-purple-50"
            }`}
          >
            🚀 Tap Now (Skip countdown)
          </button>
        )}
      </div>
    </Modal>
  );
}

// ── Sub-component ─────────────────────────────────────────────────────────────
function NfcField({ label, value, isDark, mono }) {
  return (
    <div>
      <p className={`text-xs uppercase tracking-wider font-semibold mb-0.5 ${isDark ? "text-gray-500" : "text-gray-400"}`}>
        {label}
      </p>
      <p className={`${mono ? "font-mono" : "font-semibold"} text-sm ${isDark ? "text-white" : "text-gray-900"}`}>
        {value}
      </p>
    </div>
  );
}