import React, { useEffect, useState, useRef } from "react";
import { Alert } from "antd";

export default function WeighingScreen({ vehicleData, driverData, onWeighingComplete, onBack, error }) {
  // ═════════════════════════════════════════════════════════════════════════
  // STATE MANAGEMENT
  // ═════════════════════════════════════════════════════════════════════════
  const [totalWeight, setTotalWeight] = useState("---");
  const [isStable, setIsStable] = useState(false);
  const [capturing, setCapturing] = useState(false);

  // ═════════════════════════════════════════════════════════════════════════
  // REFS FOR STABILITY TRACKING
  // ═════════════════════════════════════════════════════════════════════════
  const bufferRef = useRef(null);
  const lastStableRef = useRef(null);
  const stabilityCounterRef = useRef(0);

  // ═════════════════════════════════════════════════════════════════════════
  // CONFIGURATION
  // ═════════════════════════════════════════════════════════════════════════
  const STABILITY_CYCLES = 3;        // Number of consecutive same readings for stability
  const UPDATE_INTERVAL_MS = 2000;   // Check stability every 2 seconds

  // ═════════════════════════════════════════════════════════════════════════
  // REAL-TIME WEIGHT STREAM from Platform
  // ═════════════════════════════════════════════════════════════════════════
  useEffect(() => {
    console.log("🎯 Connecting to weight platform stream...");
    
    const source = new EventSource("http://172.16.0.219:5000/api/PlatformData/stream");

    source.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data?.type === "total" && data?.weight !== undefined) {
          bufferRef.current = data.weight;
          console.log("⚖️ Weight update:", data.weight);
        }
      } catch (err) {
        console.error("❌ Weight stream parse error:", err);
      }
    };

    source.onerror = (error) => {
      console.error("❌ Weight stream connection error:", error);
      // Don't close connection on error - it will auto-reconnect
    };

    source.onopen = () => {
      console.log("✅ Weight stream connected");
    };

    return () => {
      console.log("🛑 Closing weight stream");
      source.close();
    };
  }, []);

  // ═════════════════════════════════════════════════════════════════════════
  // STABILITY DETECTION
  // ═════════════════════════════════════════════════════════════════════════
  useEffect(() => {
    const interval = setInterval(() => {
      const current = bufferRef.current;
      
      // Skip if no weight data yet
      if (current === null || current === undefined) {
        return;
      }

      // Update display
      setTotalWeight(current);

      // Check stability
      if (current === lastStableRef.current) {
        // Same weight as last check - increment stability counter
        stabilityCounterRef.current += 1;
      } else {
        // Weight changed - reset stability
        lastStableRef.current = current;
        stabilityCounterRef.current = 1;
        setIsStable(false);
      }

      // Mark as stable if we've had enough consecutive same readings
      if (stabilityCounterRef.current >= STABILITY_CYCLES) {
        setIsStable(true);
      }
    }, UPDATE_INTERVAL_MS);

    return () => clearInterval(interval);
  }, []);

  // ═════════════════════════════════════════════════════════════════════════
  // WEIGHT CAPTURE HANDLER
  // ═════════════════════════════════════════════════════════════════════════
  const handleCapture = async () => {
    if (!isStable || capturing) {
      console.warn("⚠️ Cannot capture - weight not stable or already capturing");
      return;
    }

    setCapturing(true);
    console.log("📸 Capturing weight...");

    // Get current weight value
    let val = bufferRef.current;
    
    // Parse if string
    if (typeof val === "string") {
      val = Number(val.replace(/[^0-9.-]/g, ""));
    }

    // Validate weight
    if (!val || val <= 0) {
      console.error("❌ Invalid weight:", val);
      alert("Invalid weight detected. Please ensure vehicle is properly positioned on the platform.");
      setCapturing(false);
      return;
    }

    console.log("✅ Weight captured:", val, "kg");

    // Simulate brief processing delay (for better UX)
    await new Promise(resolve => setTimeout(resolve, 1000));

    // Pass weight data to parent
    onWeighingComplete({
      weight: val,
      weighBridgeId: "KIOSK_WEIGHBRIDGE_01",
      weighBridgeName: "KTDA WEIGHBRIDGE",
      scaleName: "Factory Unit A",
      timestamp: new Date().toISOString(),
    });
  };

  // ═════════════════════════════════════════════════════════════════════════
  // RENDER
  // ═════════════════════════════════════════════════════════════════════════
  return (
    <div className="h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-black flex flex-col">
      {/* ═══════════════════════════════════════════════════════════════════
          HEADER
          ═══════════════════════════════════════════════════════════════════ */}
      <div className="bg-gray-900 border-b border-gray-700 px-8 py-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white">Weighing in Progress</h1>
            <p className="text-gray-400 text-sm">Drive onto the weighbridge platform and wait for stable reading</p>
          </div>
          <div className="flex items-center gap-3">
            {/* Connection Status */}
            <div className="flex items-center gap-3 bg-green-900 text-green-300 px-4 py-2 rounded-lg border border-green-600">
              <span className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></span>
              <span className="font-medium">PLATFORM CONNECTED</span>
            </div>
            <button
              onClick={onBack}
              disabled={capturing}
              className="px-6 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg font-semibold disabled:opacity-50"
            >
              ← Back
            </button>
          </div>
        </div>
      </div>

      {/* ═══════════════════════════════════════════════════════════════════
          MAIN CONTENT
          ═══════════════════════════════════════════════════════════════════ */}
      <div className="flex-1 flex p-8 gap-8">
        {/* ─────────────────────────────────────────────────────────────────
            LEFT SIDEBAR: Session Info
            ───────────────────────────────────────────────────────────────── */}
        <div className="w-1/3 space-y-6">
          {/* Driver Details */}
          <div className="bg-gray-800 rounded-2xl p-6 border border-gray-700">
            <div className="flex items-center gap-3 mb-4">
              <span className="text-3xl">👤</span>
              <h3 className="text-xl font-bold text-white">Driver Details</h3>
            </div>
            
            <div className="flex items-center gap-4 bg-gray-900 rounded-lg p-4 mb-4">
              <div className="w-16 h-16 rounded-full bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white text-2xl font-bold">
                {driverData?.name?.charAt(0).toUpperCase() || 'D'}
              </div>
              <div>
                <p className="text-white font-semibold text-lg">
                  {driverData?.fullName || driverData?.name || 'John Kamau'}
                </p>
                {driverData?.license && (
                  <p className="text-gray-400 text-sm">License: {driverData.license}</p>
                )}
                {driverData?.idNo && (
                  <p className="text-gray-400 text-sm">ID: {driverData.idNo}</p>
                )}
              </div>
            </div>

            {driverData?.company && (
              <div className="bg-gray-900 rounded-lg p-3">
                <p className="text-gray-400 text-xs mb-1">Company</p>
                <p className="text-white font-semibold">{driverData.company}</p>
              </div>
            )}
          </div>

          {/* Vehicle Info */}
          <div className="bg-gray-800 rounded-2xl p-6 border border-gray-700">
            <div className="flex items-center gap-3 mb-4">
              <span className="text-3xl">🚗</span>
              <h3 className="text-xl font-bold text-white">Vehicle & Delivery</h3>
            </div>

            <div className="space-y-3">
              <div className="bg-white rounded-lg py-3 px-4">
                <p className="text-gray-600 text-xs mb-1">Number Plate</p>
                <p className="text-3xl font-bold text-gray-900 tracking-wider">
                  {vehicleData?.plateNumber}
                </p>
              </div>

              {vehicleData?.deliveryNumber && (
                <div className="bg-gray-900 rounded-lg p-3">
                  <p className="text-gray-400 text-xs mb-1">Delivery Number</p>
                  <p className="text-white font-mono text-sm">{vehicleData.deliveryNumber}</p>
                </div>
              )}

              {vehicleData?.commodityName && (
                <div className="bg-gray-900 rounded-lg p-3">
                  <p className="text-gray-400 text-xs mb-1">Product</p>
                  <p className="text-white font-semibold">{vehicleData.commodityName}</p>
                </div>
              )}

              {vehicleData?.supplierName && (
                <div className="bg-gray-900 rounded-lg p-3">
                  <p className="text-gray-400 text-xs mb-1">Supplier</p>
                  <p className="text-white font-semibold">{vehicleData.supplierName}</p>
                </div>
              )}

              {vehicleData?.transporterName && (
                <div className="bg-gray-900 rounded-lg p-3">
                  <p className="text-gray-400 text-xs mb-1">Transporter</p>
                  <p className="text-white font-semibold">{vehicleData.transporterName}</p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ─────────────────────────────────────────────────────────────────
            RIGHT MAIN AREA: Weight Display
            ───────────────────────────────────────────────────────────────── */}
        <div className="flex-1">
          {error ? (
            <div className="h-full flex items-center justify-center">
              <div className="text-center">
                <Alert
                  type="error"
                  message="Weighing Failed"
                  description={error}
                  showIcon
                  className="mb-6"
                />
                <button
                  onClick={onBack}
                  className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-lg font-semibold text-lg"
                >
                  Go Back
                </button>
              </div>
            </div>
          ) : (
            <div className="h-full bg-black rounded-2xl border-4 border-gray-700 flex flex-col overflow-hidden">
              {/* Weight Header */}
              <div className="bg-gray-900 px-8 py-4 flex items-center justify-between border-b border-gray-700">
                <span className="text-sm tracking-widest text-gray-400 font-bold">
                  LIVE WEIGHT PLATFORM
                </span>
                <div className="flex items-center gap-3">
                  <span
                    className={`w-3 h-3 rounded-full ${
                      isStable ? "bg-green-500 animate-pulse" : "bg-yellow-500 animate-pulse"
                    }`}
                  />
                  <span className={`text-sm font-bold ${isStable ? "text-green-400" : "text-yellow-400"}`}>
                    {isStable ? "STABLE - READY TO CAPTURE" : "STABILIZING..."}
                  </span>
                </div>
              </div>

              {/* Main Weight Display */}
              <div className="flex-1 flex flex-col items-center justify-center p-12">
                {/* Large Weight Display */}
                <div className="text-center mb-12">
                  <div className="text-9xl font-mono font-bold text-green-500 mb-4 tracking-wider drop-shadow-[0_0_30px_rgba(34,197,94,0.5)]">
                    {totalWeight}
                  </div>
                  <div className="text-4xl text-gray-500 font-light tracking-widest">KILOGRAMS</div>
                </div>

                {/* Weight Info Grid */}
                <div className="w-full max-w-md space-y-3 text-left mb-8">
                  <div className="bg-gray-900 rounded-lg p-4">
                    <div className="flex justify-between items-center text-gray-400">
                      <span className="text-sm">Platform Status:</span>
                      <span className="text-green-400 font-semibold text-sm">
                        {bufferRef.current !== null ? "ACTIVE" : "WAITING..."}
                      </span>
                    </div>
                  </div>
                  
                  <div className="bg-gray-900 rounded-lg p-4">
                    <div className="flex justify-between items-center text-gray-400">
                      <span className="text-sm">Stability Counter:</span>
                      <span className="text-white font-mono font-semibold text-sm">
                        {stabilityCounterRef.current} / {STABILITY_CYCLES}
                      </span>
                    </div>
                  </div>

                  <div className="bg-gray-900 rounded-lg p-4">
                    <div className="flex justify-between items-center text-gray-400">
                      <span className="text-sm">Update Interval:</span>
                      <span className="text-white font-mono font-semibold text-sm">
                        {UPDATE_INTERVAL_MS / 1000}s
                      </span>
                    </div>
                  </div>
                </div>

                {/* Capture Button */}
                <button
                  disabled={!isStable || capturing}
                  onClick={handleCapture}
                  className={`
                    w-full max-w-md py-6 rounded-xl text-2xl font-bold transition-all transform shadow-2xl
                    ${
                      isStable && !capturing
                        ? "bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white hover:scale-105 hover:shadow-[0_0_40px_rgba(34,197,94,0.6)]"
                        : "bg-gray-700 text-gray-400 cursor-not-allowed"
                    }
                  `}
                >
                  {capturing ? (
                    <span className="flex items-center justify-center gap-3">
                      <div className="w-6 h-6 border-3 border-white border-t-transparent rounded-full animate-spin"></div>
                      Processing Weight...
                    </span>
                  ) : isStable ? (
                    <span className="flex items-center justify-center gap-3">
                      <span>📸</span>
                      CAPTURE WEIGHT
                    </span>
                  ) : (
                    <span className="flex items-center justify-center gap-3">
                      <span>⏳</span>
                      WAITING FOR STABLE READING
                    </span>
                  )}
                </button>

                {/* Instructions */}
                {!isStable && !capturing && (
                  <div className="mt-6 text-center">
                    <p className="text-gray-400 text-sm animate-pulse">
                      Vehicle must remain stationary on platform for {STABILITY_CYCLES * (UPDATE_INTERVAL_MS / 1000)} seconds
                    </p>
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* ═══════════════════════════════════════════════════════════════════
          FOOTER
          ═══════════════════════════════════════════════════════════════════ */}
      <div className="bg-gray-900 border-t border-gray-700 px-8 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-8 text-sm">
            <p className="text-gray-500">
              Step 3 of 4: <span className="text-white font-semibold">Weighing</span>
            </p>
            <div className="flex items-center gap-4 text-gray-400">
              <span>📍 Position vehicle on platform</span>
              <span>⏸️ Stop completely</span>
              <span>⏳ Wait for stability ({STABILITY_CYCLES} cycles)</span>
              <span>📸 Capture when green</span>
            </div>
          </div>
          <p className="text-gray-500 text-sm">
            Powered by <span className="font-bold text-amber-500">QALIBRATED SYSTEMS</span>
          </p>
        </div>
      </div>
    </div>
  );
}