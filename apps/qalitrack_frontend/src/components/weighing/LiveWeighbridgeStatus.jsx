import React, { useEffect, useState, useRef } from "react";

export default function LiveWeighbridgeStatus({ onManualCapture }) {
    const [totalWeight, setTotalWeight] = useState("--- kg");
    const [isStable, setIsStable] = useState(false);
    const bufferRef = useRef(null);
    const lastStableRef = useRef(null);
    const stabilityCounterRef = useRef(0);

    const STABILITY_CYCLES = 3;
    const UPDATE_INTERVAL_MS = 2000;

    useEffect(() => {
        const source = new EventSource("http://172.16.0.215:5000/api/PlatformData/stream");
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

    useEffect(() => {
        const interval = setInterval(() => {
            const current = bufferRef.current;
            if (current !== undefined && current !== null) {
                const formatted = typeof current === "number" ? `${current} kg` : String(current);
                setTotalWeight(formatted);

                if (current === lastStableRef.current) {
                    stabilityCounterRef.current += 1;
                } else {
                    lastStableRef.current = current;
                    stabilityCounterRef.current = 1;
                    setIsStable(false);
                }

                if (stabilityCounterRef.current >= STABILITY_CYCLES && !isStable) {
                    setIsStable(true);
                }
            }
        }, UPDATE_INTERVAL_MS);
        return () => clearInterval(interval);
    }, [isStable]);

    const handleCapture = () => {
        if (typeof onManualCapture === "function" && bufferRef.current !== null) {
            let val = bufferRef.current;
            if (typeof val === "string") val = Number(val.replace(/[^0-9.-]/g, ""));
            onManualCapture(val);
        }
    };

    return (
        <div className="bg-black text-amber-600 rounded-2xl shadow-xl p-6">
            <h3 className="text-lg font-semibold tracking-widest text-center">LIVE WEIGHT</h3>
            <div className="text-5xl md:text-6xl font-mono font-bold mt-4 text-center">{totalWeight}</div>
            <div className="mt-3 text-sm text-amber-400 flex items-center justify-center">
                <span className={`w-3 h-3 rounded-full mr-2 ${isStable ? "bg-green-500 animate-pulse" : "bg-yellow-500"}`} />
                <span>{isStable ? "STABLE - Auto-capture available" : "Live reading"}</span>
            </div>
            <div className="flex justify-center mt-5">
                <button
                    onClick={handleCapture}
                    className="bg-amber-600 hover:bg-amber-700 text-black font-semibold px-4 py-2 rounded-lg shadow"
                >
                    CAPTURE WEIGHT
                </button>
            </div>
        </div>
    );
}