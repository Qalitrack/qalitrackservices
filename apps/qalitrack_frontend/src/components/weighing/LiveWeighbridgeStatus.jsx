import React, { useEffect, useState, useRef } from "react";
import { getHardwareConfig } from "../../hooks/useHardwareConfig";
import { message } from "antd";

/**
 * Normalize raw weight from the stream.
 *
 * The scale indicator streams values divided by 1000 in some firmware versions
 * (e.g. indicator reads 10 kg → stream sends 0.010000).
 * Detection rule: if the parsed value is > 0 and < 1, multiply by 1000.
 * Values of 0 (tare/zero) or ≥ 1 (already correct units) pass through unchanged.
 */
function normalizeWeight(raw) {
  const val = typeof raw === "number" ? raw : parseFloat(String(raw).replace(/[^0-9.-]/g, ""));
  if (isNaN(val)) return null;
  if (val > 0 && val < 1) return Math.round(val * 1000 * 100) / 100; // reversed — fix it
  return val; // 0 or ≥ 1 — already correct
}

export default function LiveWeighbridgeStatus({ onManualCapture }) {
  const [totalWeight, setTotalWeight] = useState("---");
  const [isStable, setIsStable] = useState(false);
  const [connected, setConnected] = useState(false);

  const bufferRef = useRef(null);
  const lastStableRef = useRef(null);
  const stabilityCounterRef = useRef(0);

  const STABILITY_CYCLES = 2;
  const STABILITY_CHECK_MS = 200;

  /* ----------------------------- STREAM ----------------------------- */
  useEffect(() => {
    const source = new EventSource(getHardwareConfig().scaleStreamUrl);

    // Don't trust onopen — it only means the HTTP connection exists, not that the
    // hardware is actually sending data. Rely on the backend's heartbeat event instead,
    // which reports real device health (last-data age), plus onmessage for immediate proof of life.
    source.onerror = () => { setConnected(false); setIsStable(false); };

    source.addEventListener("heartbeat", (event) => {
      try {
        const status = JSON.parse(event.data);
        setConnected(!!status.connected);
        if (!status.connected) setIsStable(false);
      } catch {
        // malformed heartbeat payload — leave connected state as-is
      }
    });

    source.onmessage = (event) => {
      setConnected(true);
      try {
        const data = JSON.parse(event.data);
        const raw = data?.weight !== undefined ? data.weight : (typeof data === "number" ? data : null);
        if (raw !== null) {
          const w = normalizeWeight(raw);
          if (w !== null && w !== bufferRef.current) {
            bufferRef.current = w;
            setTotalWeight(w);
          }
        }
      } catch {
        const w = normalizeWeight(event.data);
        if (w !== null && w !== bufferRef.current) {
          bufferRef.current = w;
          setTotalWeight(w);
        }
      }
    };

    return () => source.close();
  }, []);

  /* ---------------------------- STABILITY ---------------------------- */
  useEffect(() => {
    const interval = setInterval(() => {
      const current = bufferRef.current;
      if (current === null || current === undefined) return;

      if (current === lastStableRef.current) {
        stabilityCounterRef.current += 1;
      } else {
        lastStableRef.current = current;
        stabilityCounterRef.current = 1;
        setIsStable(false);
      }

      if (stabilityCounterRef.current >= STABILITY_CYCLES) {
        setIsStable(true);
      }
    }, STABILITY_CHECK_MS);

    return () => clearInterval(interval);
  }, []);

  /* ----------------------------- ACTION ------------------------------ */
  const handleCapture = () => {
    if (!isStable || typeof onManualCapture !== "function") return;
    const val = normalizeWeight(bufferRef.current);
    if (val === null) return;
    if (val < 500) {
      message.warning(`Invalid weight: ${val} kg — minimum valid weight is 500 kg`);
      return;
    }
    onManualCapture(val);
  };

  /* ------------------------------ UI -------------------------------- */
  return (
    <div className="h-full flex flex-col justify-between bg-neutral-900 rounded-lg p-4">
      {/* HEADER */}
      <div className="flex items-center justify-between">
        <span className="text-xs tracking-widest text-neutral-400">
          LIVE WEIGHT
        </span>

        <div className="flex items-center gap-2 text-xs">
          <span
            className={`w-2 h-2 rounded-full ${
              !connected ? "bg-red-500" : isStable ? "bg-green-500" : "bg-yellow-500"
            }`}
          />
          <span className="text-neutral-400">
            {!connected ? "No Signal" : isStable ? "Stable" : "Live"}
          </span>
        </div>
      </div>

      {/* WEIGHT */}
      <div className="text-center flex-1 flex items-center justify-center overflow-hidden px-1">
        <span
          className={`font-mono font-black leading-none w-full text-center block ${connected ? "text-amber-500" : "text-red-500 text-2xl"}`}
          style={connected ? { fontSize: ["7rem","7rem","7rem","7rem","5rem","4rem","3.5rem"][Math.min(String(totalWeight).length, 6)] } : {}}
        >
          {connected ? totalWeight : "NO SIGNAL"}
        </span>
      </div>

      {/* ACTION */}
      <button
        disabled={!isStable}
        onClick={handleCapture}
        className={`
          w-full py-2 rounded-md text-sm font-medium transition
          ${
            isStable
              ? "bg-amber-600 hover:bg-amber-700 text-black"
              : "bg-neutral-700 text-neutral-400 cursor-not-allowed"
          }
        `}
      >
        Capture Weight
      </button>
    </div>
  );
}
