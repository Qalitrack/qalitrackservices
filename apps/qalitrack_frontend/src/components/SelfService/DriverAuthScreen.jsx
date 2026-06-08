import React, { useEffect, useState } from "react";
import { Alert } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";

export default function DriverAuthScreen({ vehicleData, onDriverAuthenticated, onBack, error }) {
  const { theme, isDark } = useTheme();
  const [nfcDetected, setNfcDetected] = useState(false);
  const [authenticating, setAuthenticating] = useState(false);
  const [nfcCardData, setNfcCardData] = useState(null);

  // ═════════════════════════════════════════════════════════════════════════
  // 🎭 SIMULATED NFC DETECTION (for testing without hardware)
  // ═════════════════════════════════════════════════════════════════════════
  useEffect(() => {
    
    // Simulate NFC card detection after 3 seconds
    const simulateNFC = setTimeout(() => {
      const testCards = [
        { cardId: "NFC-ABC123456", driverId: "DRV001", driverName: "John Kamau Mwangi" },
        { cardId: "NFC-DEF789012", driverId: "DRV002", driverName: "Mary Wanjiru Njeri" },
        { cardId: "NFC-GHI345678", driverId: "DRV003", driverName: "Peter Omondi Otieno" }
      ];
      
      const randomCard = testCards[Math.floor(Math.random() * testCards.length)];
      
      setNfcCardData(randomCard);
      setNfcDetected(true);
      
      // Auto-authenticate after detection
      setTimeout(() => {
        handleAuthenticate(randomCard);
      }, 1500);
      
    }, 3000); // 3 seconds after page load

    return () => clearTimeout(simulateNFC);
  }, []);

  // ═════════════════════════════════════════════════════════════════════════
  // 🔌 REAL NFC DETECTION (uncomment when ready to use real hardware)
  // ═════════════════════════════════════════════════════════════════════════
  /*
  useEffect(() => {
    
    const eventSource = new EventSource("http://172.16.0.93:5000/api/NFC/stream");
    
    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data.cardId || data.nfcId) {
          const cardId = data.cardId || data.nfcId;
          
          setNfcCardData(data);
          setNfcDetected(true);
          
          setTimeout(() => {
            handleAuthenticate(data);
          }, 1000);
        }
      } catch (err) {
      }
    };
    
    eventSource.onerror = (error) => {
      eventSource.close();
    };
    
    return () => {
      eventSource.close();
    };
  }, []);
  */

  // ═════════════════════════════════════════════════════════════════════════
  // AUTHENTICATION HANDLER with Backend Verification
  // ═════════════════════════════════════════════════════════════════════════
  const handleAuthenticate = async (nfcData) => {
    setAuthenticating(true);
    
    try {
      // In production, this would verify against backend database
      // Similar to how RFID verification works
      // const response = await fetch('/api/drivers/verify-nfc', {
      //   method: 'POST',
      //   body: JSON.stringify({ cardId: nfcData.cardId })
      // });
      // const driverData = await response.json();
      
      // For simulation, create mock driver data
      const mockDriverData = {
        id: "DRIVER_" + Date.now(),
        driverId: nfcData.driverId || "DRV_" + Math.random().toString(36).substr(2, 9),
        fullName: nfcData.driverName || "John Kamau Mwangi",
        name: nfcData.driverName?.split(' ').slice(0, 2).join(' ') || "John Kamau",
        license: "DL-" + Math.floor(Math.random() * 1000000),
        idNo: "28000000" + Math.floor(Math.random() * 1000),
        nfcCardId: nfcData.cardId,
        company: "Fresh Leaf Carriers Ltd",
        phone: "+254 712 " + Math.floor(Math.random() * 1000000),
        email: nfcData.driverName?.toLowerCase().replace(/ /g, '.') + "@flc.co.ke" || "driver@flc.co.ke",
        verified: true,
        verifiedAt: new Date().toISOString()
      };
      
      
      // Pass NFC data to parent component
      await onDriverAuthenticated({
        ...nfcData,
        driverData: mockDriverData
      });
      
    } catch (err) {
      setAuthenticating(false);
    }
  };

  // 🎭 Manual trigger for testing (shows button in simulation mode)
  const handleManualTrigger = () => {
    if (!nfcDetected) {
      const mockCard = { 
        cardId: "NFC-MANUAL-TEST", 
        driverId: "DRV999",
        driverName: "Test Driver Manual"
      };
      setNfcCardData(mockCard);
      setNfcDetected(true);
      setTimeout(() => handleAuthenticate(mockCard), 1000);
    }
  };

  return (
    <div className={`min-h-screen ${
      isDark 
        ? 'bg-gradient-to-br from-gray-900 via-gray-800 to-black' 
        : 'bg-gradient-to-br from-gray-50 via-white to-gray-100'
    } flex flex-col`}>
      {/* ═══════════════════════════════════════════════════════════════════
          HEADER
          ═══════════════════════════════════════════════════════════════════ */}
      <div className={`${
        isDark ? 'bg-gray-900 border-gray-700' : 'bg-white border-gray-200'
      } border-b px-8 py-6 shadow-sm`}>
        <div className="flex items-center justify-between">
          <div>
            <h1 className={`text-3xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>
              Driver Authentication
            </h1>
            <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
              Please tap your NFC card on the reader
            </p>
          </div>
          <div className="flex items-center gap-3">
            {/* Simulation Badge */}
            <div className={`flex items-center gap-3 px-4 py-2 rounded-lg border ${
              isDark 
                ? 'bg-purple-900/30 text-purple-300 border-purple-700' 
                : 'bg-purple-50 text-purple-700 border-purple-200'
            }`}>
              <span className="w-3 h-3 bg-purple-500 rounded-full animate-pulse"></span>
              <span className="font-medium text-sm">SIMULATION MODE</span>
            </div>
            <button
              onClick={onBack}
              className={`px-6 py-2 rounded-lg font-semibold transition-colors ${
                isDark 
                  ? 'bg-gray-700 hover:bg-gray-600 text-white' 
                  : 'bg-gray-200 hover:bg-gray-300 text-gray-900'
              }`}
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
          <div className={`${
            isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'
          } rounded-2xl p-6 border shadow-xl`}>
            <div className="flex items-center gap-3 mb-6">
              <span className="text-3xl">🚗</span>
              <div>
                <h3 className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                  Vehicle Detected
                </h3>
                <p className="text-sm text-green-500">Ready for driver authentication</p>
              </div>
            </div>

            <div className="space-y-4">
              {/* Number Plate */}
              <div className="bg-white rounded-lg py-4 px-5 shadow-md">
                <p className="text-gray-600 text-xs mb-1 font-medium">Number Plate</p>
                <p className="text-3xl font-black text-gray-900 tracking-wider">
                  {vehicleData?.plateNumber}
                </p>
              </div>

              {/* Additional Vehicle Info */}
              {vehicleData?.commodityName && (
                <div className={`${
                  isDark ? 'bg-gray-900' : 'bg-gray-50'
                } rounded-lg p-4`}>
                  <p className={`text-xs mb-1 font-medium ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                    Product
                  </p>
                  <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                    {vehicleData.commodityName}
                  </p>
                </div>
              )}

              {vehicleData?.supplierName && (
                <div className={`${
                  isDark ? 'bg-gray-900' : 'bg-gray-50'
                } rounded-lg p-4`}>
                  <p className={`text-xs mb-1 font-medium ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                    Supplier
                  </p>
                  <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                    {vehicleData.supplierName}
                  </p>
                </div>
              )}

              {vehicleData?.transporterName && (
                <div className={`${
                  isDark ? 'bg-gray-900' : 'bg-gray-50'
                } rounded-lg p-4`}>
                  <p className={`text-xs mb-1 font-medium ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                    Transporter
                  </p>
                  <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                    {vehicleData.transporterName}
                  </p>
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
              <div className="text-center max-w-md">
                <Alert
                  type="error"
                  message="Authentication Failed"
                  description={error}
                  showIcon
                  className="mb-6"
                />
                <button
                  onClick={onBack}
                  className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-xl font-semibold text-lg transition-colors"
                >
                  Try Again
                </button>
              </div>
            </div>
          ) : (
            <div className={`h-full ${
              isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'
            } rounded-2xl border-2 flex flex-col items-center justify-center p-12 shadow-xl`}>
              {authenticating ? (
                // Authenticating State
                <div className="text-center">
                  <div className="w-40 h-40 mx-auto mb-8 rounded-full bg-green-900/50 flex items-center justify-center animate-pulse shadow-2xl">
                    <span className="text-7xl">✓</span>
                  </div>
                  <h2 className={`text-4xl font-bold mb-3 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                    Authenticating...
                  </h2>
                  <p className={`text-xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                    Verifying driver credentials
                  </p>
                  <div className="mt-6">
                    <div className="w-20 h-20 border-4 border-green-500 border-t-transparent rounded-full animate-spin mx-auto"></div>
                  </div>
                </div>
              ) : nfcDetected ? (
                // Card Detected State
                <div className="text-center">
                  <div className="w-48 h-48 mx-auto mb-8 rounded-full bg-green-900/50 flex items-center justify-center shadow-2xl">
                    <span className="text-8xl">✓</span>
                  </div>
                  <h2 className="text-5xl font-bold text-green-500 mb-4">Card Detected!</h2>
                  <p className={`text-2xl ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    Verifying credentials...
                  </p>
                  {nfcCardData && (
                    <div className="mt-6">
                      <p className={`text-sm ${isDark ? 'text-gray-500' : 'text-gray-600'}`}>
                        Card ID: {nfcCardData.cardId}
                      </p>
                    </div>
                  )}
                </div>
              ) : (
                // Waiting for Card State
                <div className="text-center">
                  <div className="w-48 h-48 mx-auto mb-8 rounded-full border-4 border-dashed border-amber-500 flex items-center justify-center animate-pulse">
                    <span className="text-8xl">📡</span>
                  </div>
                  <h2 className={`text-5xl font-bold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                    Tap Your NFC Card
                  </h2>
                  <p className={`text-2xl mb-8 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                    Hold your card against the reader
                  </p>

                  {/* Progress indicator */}
                  <div className="max-w-md mx-auto mb-8">
                    <div className="flex items-center justify-between mb-2">
                      <span className={`text-sm ${isDark ? 'text-gray-500' : 'text-gray-600'}`}>
                        Waiting for card...
                      </span>
                      <span className={`text-sm ${isDark ? 'text-gray-500' : 'text-gray-600'}`}>
                        🎭 Simulating (3s)
                      </span>
                    </div>
                    <div className={`h-2 ${isDark ? 'bg-gray-700' : 'bg-gray-200'} rounded-full overflow-hidden`}>
                      <div className="h-full bg-gradient-to-r from-amber-500 to-orange-600 animate-pulse"></div>
                    </div>
                  </div>

                  {/* Manual Trigger (Development Only) */}
                  <button
                    onClick={handleManualTrigger}
                    className="px-8 py-3 bg-purple-600 hover:bg-purple-700 text-white rounded-xl font-semibold transition-colors"
                  >
                    🚀 Trigger NFC Now (Dev)
                  </button>
                </div>
              )}

              {/* Instructions */}
              {!nfcDetected && !authenticating && (
                <div className="mt-12 max-w-lg">
                  <div className={`${
                    isDark 
                      ? 'bg-blue-900/20 border-blue-700' 
                      : 'bg-blue-50 border-blue-200'
                  } border rounded-xl p-6`}>
                    <h4 className={`text-lg font-semibold mb-3 ${
                      isDark ? 'text-blue-300' : 'text-blue-900'
                    }`}>
                      Instructions:
                    </h4>
                    <ol className={`space-y-2 text-sm ${isDark ? 'text-blue-200' : 'text-blue-800'}`}>
                      <li className="flex items-start gap-3">
                        <span className="text-blue-500 font-bold">1.</span>
                        <span>Remove your NFC card from your wallet</span>
                      </li>
                      <li className="flex items-start gap-3">
                        <span className="text-blue-500 font-bold">2.</span>
                        <span>Hold the card flat against the NFC reader</span>
                      </li>
                      <li className="flex items-start gap-3">
                        <span className="text-blue-500 font-bold">3.</span>
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
      <div className={`${
        isDark ? 'bg-gray-900 border-gray-700' : 'bg-white border-gray-200'
      } border-t px-8 py-4 shadow-sm`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-8 text-sm">
            <p className={isDark ? 'text-gray-500' : 'text-gray-600'}>
              Step 2 of 4: <span className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Driver Authentication
              </span>
            </p>
            <div className={`flex items-center gap-4 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
              <span>✓ Vehicle detected: {vehicleData?.plateNumber}</span>
              <span>→ Awaiting driver card</span>
            </div>
          </div>
          <p className={`text-sm ${isDark ? 'text-gray-500' : 'text-gray-600'}`}>
            Powered by <span className="font-bold text-amber-500">QALIBRATED SYSTEMS</span>
          </p>
        </div>
      </div>
    </div>
  );
}