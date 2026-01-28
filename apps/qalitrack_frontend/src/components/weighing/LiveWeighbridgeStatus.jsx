import React, { useEffect, useState, useRef } from "react";

export default function LiveWeighbridgeStatus({ onManualCapture }) {
  const [totalWeight, setTotalWeight] = useState("---");
  const [isStable, setIsStable] = useState(false);

  const bufferRef = useRef(null);
  const lastStableRef = useRef(null);
  const stabilityCounterRef = useRef(0);

  const STABILITY_CYCLES = 3;
  const UPDATE_INTERVAL_MS = 2000;

  /* ----------------------------- STREAM ----------------------------- */
  useEffect(() => {
    const source = new EventSource(
      "http://172.16.0.219:5000/api/PlatformData/stream"
    );

    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data?.type === "total" && data?.weight !== undefined) {
          bufferRef.current = data.weight;
        }
      } catch (err) {
        console.error(err);
      }
    };

    return () => source.close();
  }, []);

  /* ---------------------------- STABILITY ---------------------------- */
  useEffect(() => {
    const interval = setInterval(() => {
      const current = bufferRef.current;
      if (current === null || current === undefined) return;

      setTotalWeight(current);

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
    }, UPDATE_INTERVAL_MS);

    return () => clearInterval(interval);
  }, []);

  /* ----------------------------- ACTION ------------------------------ */
  const handleCapture = () => {
    if (!isStable || typeof onManualCapture !== "function") return;

    let val = bufferRef.current;
    if (typeof val === "string") {
      val = Number(val.replace(/[^0-9.-]/g, ""));
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
              isStable ? "bg-green-500" : "bg-yellow-500"
            }`}
          />
          <span className="text-neutral-400">
            {isStable ? "Stable" : "Live"}
          </span>
        </div>
      </div>

      {/* WEIGHT */}
      <div className="text-center my-3">
        <span className="text-3xl font-mono font-semibold text-amber-500">
          {totalWeight}
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
