import { useEffect, useState, useRef } from "react";
import { getHardwareConfig } from "../hooks/useHardwareConfig";

// Accept a callback function from the parent via props
export default function LiveWeighbridgeStatus({ onWeightStable }) {
  // Initial state shows a placeholder until the first reading is captured
  const [totalWeight, setTotalWeight] = useState("--- kg"); 
  const [isStable, setIsStable] = useState(false); // New state for UI indicator
  
  const bufferRef = useRef(null); // Stores the latest raw weight from SSE
  const lastReportedStableWeightRef = useRef(null); // Stores the weight value last sent to the parent
  const stabilizationCounterRef = useRef(0);
  
  // Configuration
  const STABILITY_CYCLES = 1; // Report immediately on first stable reading
  const UPDATE_INTERVAL_MS = 1000; // UI update frequency

  // --- 1. LISTEN TO SSE STREAM (High Frequency) ---
  useEffect(() => {
    const source = new EventSource(getHardwareConfig().scaleStreamUrl);

    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data?.weight !== undefined) {
          bufferRef.current = data.weight;
        } else if (typeof data === "number") {
          bufferRef.current = data;
        }
      } catch {
        const num = parseFloat(String(event.data).replace(/[^0-9.-]/g, ""));
        if (!isNaN(num)) bufferRef.current = num;
      }
    };

    source.onerror = (err) => {
    };

    return () => source.close();
  }, []); // Run only on mount

  // --- 2. UPDATE UI AND CHECK STABILIZATION (Low Frequency - every 2s) ---
  useEffect(() => {
    const interval = setInterval(() => {
      const currentBufferWeight = bufferRef.current;
      
      if (currentBufferWeight !== null && currentBufferWeight !== undefined) {
        
        // 1. Update UI
        const formatted =
          typeof currentBufferWeight === "number"
            ? `${currentBufferWeight} kg`
            : String(currentBufferWeight); // Handle if the SSE sends weight as a string

        setTotalWeight(formatted);
        
        // 2. Stabilization Check
        
        // Use string comparison for robustness (handles "100.0" vs 100)
        const currentWeightString = String(currentBufferWeight); 
        const lastReportedString = String(lastReportedStableWeightRef.current);

        // Check if the current reading is the same as the last reading used for stabilization
        if (currentWeightString === lastReportedString) {
          stabilizationCounterRef.current += 1;
        } else {
          // Weight has changed, reset counter
          stabilizationCounterRef.current = 1; // Start counting from 1 for the current weight
          lastReportedStableWeightRef.current = currentBufferWeight; // Update the weight we are tracking for stability
          setIsStable(false);
        }

        // 3. Report Stable Weight
        if (stabilizationCounterRef.current >= STABILITY_CYCLES && !isStable) {
          
          // Ensure we don't report the same stable weight multiple times
          if (currentWeightString !== lastReportedString) {
              onWeightStable(currentBufferWeight); 
              setIsStable(true);
          }
        }
      }
    }, UPDATE_INTERVAL_MS);

    return () => clearInterval(interval);
  }, [onWeightStable, isStable]); // Include props/state used inside the effect

  return (
    <div className="flex justify-center mt-8">
      <div className="bg-black text-amber-600 rounded-2xl shadow-xl p-8 w-80 flex flex-col items-center relative">
        <h1 className="text-xl font-semibold tracking-widest">TOTAL WEIGHT</h1>

        <div className="text-6xl font-mono font-bold mt-4">
          {totalWeight}
        </div>

        <div className="mt-2 text-sm text-amber-400 flex items-center">
            {/* Visual Indicator */}
            <span 
                className={`w-3 h-3 rounded-full mr-2 ${isStable ? 'bg-green-500 animate-pulse' : 'bg-yellow-500'}`}
            ></span>
            
            {/* Status Text */}
            {isStable ? 'STABLE - Weight Captured' : 'Live reading'}
        </div>
      </div>
    </div>
  );
}