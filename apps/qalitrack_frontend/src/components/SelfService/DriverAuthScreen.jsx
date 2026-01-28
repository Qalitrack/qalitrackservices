import React, { useEffect, useState } from "react";
import { Alert } from "antd";

export default function DriverAuthScreen({ vehicleData, onDriverAuthenticated, onBack, error }) {
  const [nfcDetected, setNfcDetected] = useState(false);
  const [authenticating, setAuthenticating] = useState(false);

  // ═════════════════════════════════════════════════════════════════════════
  // 🎭 SIMULATED NFC DETECTION (for testing without hardware)
  // ═════════════════════════════════════════════════════════════════════════
  useEffect(() => {
    console.log("💳 SIMULATION MODE: NFC reader active");
    
    // Simulate NFC card detection after 3 seconds
    const simulateNFC = setTimeout(() => {
      const testCards = [
        { cardId: "NFC-ABC123456", driverId: "DRV001" },
        { cardId: "NFC-DEF789012", driverId: "DRV002" },
        { cardId: "NFC-GHI345678", driverId: "DRV003" }
      ];
      
      const randomCard = testCards[Math.floor(Math.random() * testCards.length)];
      
      console.log("💳 SIMULATED NFC card detected:", randomCard.cardId);
      setNfcDetected(true);
      
      // Auto-authenticate after detection
      setTimeout(() => {
        handleAuthenticate(randomCard);
      }, 1000);
      
    }, 3000); // 3 seconds after page load

    return () => clearTimeout(simulateNFC);
  }, []);

  // ═════════════════════════════════════════════════════════════════════════
  // 🔌 REAL NFC DETECTION (comment this out when ready to use real hardware)
  // ═════════════════════════════════════════════════════════════════════════
  /*
  useEffect(() => {
    console.log("🔌 REAL MODE: Connecting to NFC reader...");
    
    const eventSource = new EventSource("http://172.16.0.93:5000/api/NFC/stream");
    
    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data.cardId || data.nfcId) {
          const cardId = data.cardId || data.nfcId;
          console.log("💳 REAL NFC card detected:", cardId);
          setNfcDetected(true);
          
          setTimeout(() => {
            handleAuthenticate({ cardId, driverId: data.driverId });
          }, 1000);
        }
      } catch (err) {
        console.error("NFC parse error:", err);
      }
    };
    
    eventSource.onerror = (error) => {
      console.error("NFC stream error:", error);
      eventSource.close();
    };
    
    return () => {
      console.log("🛑 Closing NFC stream");
      eventSource.close();
    };
  }, []);
  */

  // ═════════════════════════════════════════════════════════════════════════
  // AUTHENTICATION HANDLER
  // ═════════════════════════════════════════════════════════════════════════
  const handleAuthenticate = async (nfcData) => {
    setAuthenticating(true);
    console.log("🔐 Authenticating driver with NFC:", nfcData.cardId);
    
    try {
      // Pass NFC data to parent component
      // Parent will handle creating mock/real driver data and moving to weighing
      await onDriverAuthenticated(nfcData);
      
    } catch (err) {
      console.error("❌ Authentication error:", err);
      setAuthenticating(false);
    }
  };

  // 🎭 Manual trigger for testing (shows button in simulation mode)
  const handleManualTrigger = () => {
    if (!nfcDetected) {
      const mockCard = { cardId: "NFC-MANUAL-TEST", driverId: "DRV999" };
      setNfcDetected(true);
      setTimeout(() => handleAuthenticate(mockCard), 1000);
    }
  };

  return (
    <div className="h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-black flex flex-col">
      {/* ═══════════════════════════════════════════════════════════════════
          HEADER
          ═══════════════════════════════════════════════════════════════════ */}
      <div className="bg-gray-900 border-b border-gray-700 px-8 py-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white">Driver Authentication</h1>
            <p className="text-gray-400 text-sm">Please tap your NFC card on the reader</p>
          </div>
          <div className="flex items-center gap-3">
            {/* Simulation Badge */}
            <div className="flex items-center gap-3 bg-purple-900 text-purple-300 px-4 py-2 rounded-lg border border-purple-600">
              <span className="w-3 h-3 bg-purple-500 rounded-full animate-pulse"></span>
              <span className="font-medium">SIMULATION MODE</span>
            </div>
            <button
              onClick={onBack}
              className="px-6 py-2 bg-gray-700 hover:bg-gray-600 text-white rounded-lg font-semibold"
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
            LEFT: Vehicle Info
            ───────────────────────────────────────────────────────────────── */}
        <div className="w-1/3">
          <div className="bg-gray-800 rounded-2xl p-6 border border-gray-700">
            <div className="flex items-center gap-3 mb-6">
              <span className="text-3xl">🚗</span>
              <div>
                <h3 className="text-xl font-bold text-white">Vehicle Detected</h3>
                <p className="text-sm text-green-400">Ready for driver authentication</p>
              </div>
            </div>

            <div className="space-y-4">
              {/* Number Plate */}
              <div className="bg-white rounded-lg py-3 px-4">
                <p className="text-gray-600 text-xs mb-1">Number Plate</p>
                <p className="text-2xl font-bold text-gray-900 tracking-wider">
                  {vehicleData?.plateNumber}
                </p>
              </div>

              {/* Additional Vehicle Info */}
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
            </div>
          </div>
        </div>

        {/* ─────────────────────────────────────────────────────────────────
            RIGHT: NFC Scanner
            ───────────────────────────────────────────────────────────────── */}
        <div className="flex-1">
          {error ? (
            <div className="h-full flex items-center justify-center">
              <div className="text-center">
                <Alert
                  type="error"
                  message="Authentication Failed"
                  description={error}
                  showIcon
                  className="mb-6"
                />
                <button
                  onClick={onBack}
                  className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-lg font-semibold text-lg"
                >
                  Try Again
                </button>
              </div>
            </div>
          ) : (
            <div className="h-full bg-gray-800 rounded-2xl border-2 border-gray-700 flex flex-col items-center justify-center p-12">
              {authenticating ? (
                // Authenticating State
                <div className="text-center">
                  <div className="w-32 h-32 mx-auto mb-8 rounded-full bg-green-900 flex items-center justify-center animate-pulse">
                    <span className="text-6xl">✓</span>
                  </div>
                  <h2 className="text-3xl font-bold text-white mb-3">Authenticating...</h2>
                  <p className="text-gray-400 text-lg">Please wait</p>
                  <div className="mt-6">
                    <div className="w-16 h-16 border-4 border-green-500 border-t-transparent rounded-full animate-spin mx-auto"></div>
                  </div>
                </div>
              ) : nfcDetected ? (
                // Card Detected State
                <div className="text-center">
                  <div className="w-40 h-40 mx-auto mb-8 rounded-full bg-green-900 flex items-center justify-center">
                    <span className="text-8xl">✓</span>
                  </div>
                  <h2 className="text-4xl font-bold text-green-500 mb-4">Card Detected!</h2>
                  <p className="text-gray-300 text-xl">Verifying credentials...</p>
                </div>
              ) : (
                // Waiting for Card State
                <div className="text-center">
                  <div className="w-40 h-40 mx-auto mb-8 rounded-full border-4 border-dashed border-amber-500 flex items-center justify-center animate-pulse">
                    <span className="text-8xl">📡</span>
                  </div>
                  <h2 className="text-4xl font-bold text-white mb-4">Tap Your NFC Card</h2>
                  <p className="text-gray-400 text-xl mb-8">
                    Hold your card against the reader
                  </p>

                  {/* Progress indicator */}
                  <div className="max-w-md mx-auto mb-8">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-sm text-gray-500">Waiting for card...</span>
                      <span className="text-sm text-gray-500">🎭 Simulating (3s)</span>
                    </div>
                    <div className="h-2 bg-gray-700 rounded-full overflow-hidden">
                      <div className="h-full bg-gradient-to-r from-amber-500 to-orange-600 animate-pulse"></div>
                    </div>
                  </div>

                  {/* Manual Trigger (Development Only) */}
                  <button
                    onClick={handleManualTrigger}
                    className="px-8 py-3 bg-purple-600 hover:bg-purple-700 text-white rounded-lg font-semibold transition-colors"
                  >
                    🚀 Trigger NFC Now (Dev)
                  </button>
                </div>
              )}

              {/* Instructions */}
              {!nfcDetected && !authenticating && (
                <div className="mt-12 max-w-lg">
                  <div className="bg-blue-900 bg-opacity-30 border border-blue-700 rounded-lg p-6">
                    <h4 className="text-lg font-semibold text-blue-300 mb-3">Instructions:</h4>
                    <ol className="text-gray-300 space-y-2 text-sm">
                      <li className="flex items-start gap-3">
                        <span className="text-blue-400 font-bold">1.</span>
                        <span>Remove your NFC card from your wallet</span>
                      </li>
                      <li className="flex items-start gap-3">
                        <span className="text-blue-400 font-bold">2.</span>
                        <span>Hold the card flat against the NFC reader</span>
                      </li>
                      <li className="flex items-start gap-3">
                        <span className="text-blue-400 font-bold">3.</span>
                        <span>Wait for the green checkmark before removing card</span>
                      </li>
                    </ol>
                  </div>
                </div>
              )}
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
              Step 2 of 4: <span className="text-white font-semibold">Driver Authentication</span>
            </p>
            <div className="flex items-center gap-4 text-gray-400">
              <span>✓ Vehicle detected: {vehicleData?.plateNumber}</span>
              <span>→ Awaiting driver card</span>
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