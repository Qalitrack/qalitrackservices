import React, { useEffect, useState } from "react";
import { Alert, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import AuthenticationMethodModal from "../SelfService/AuthenticationMethodModal.jsx";

export default function VehicleDetectionScreen({ onVehicleDetected, error, onReset }) {
  const { theme, isDark } = useTheme();
  const [detectedPlate, setDetectedPlate] = useState(null);
  const [rfidDetected, setRfidDetected] = useState(false);
  const [anprImage, setAnprImage] = useState(null);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [selectedAuthMethod, setSelectedAuthMethod] = useState(null);
  const [vehicleData, setVehicleData] = useState(null);

  // Simulated ANPR Detection
  useEffect(() => {
    console.log("🎭 SIMULATION MODE: ANPR detection active");
    
    const simulateANPR = setTimeout(() => {
      const testPlates = ["KBU 510 G", "KBC 123 A", "KBC 465 B"];
      const randomPlate = testPlates[Math.floor(Math.random() * testPlates.length)];
      
      console.log("📸 SIMULATED ANPR detected:", randomPlate);
      setDetectedPlate(randomPlate);
      setAnprImage("/api/placeholder/400/300");
    }, 3000);

    return () => clearTimeout(simulateANPR);
  }, []);

  // Simulated RFID Detection
  useEffect(() => {
    console.log("🎭 SIMULATION MODE: RFID detection active");
    
    const simulateRFID = setTimeout(() => {
      const testTags = ["RFID-ABC123456", "RFID-DEF789012", "RFID-GHI345678"];
      const randomTag = testTags[Math.floor(Math.random() * testTags.length)];
      
      console.log("📡 SIMULATED RFID detected:", randomTag);
      setRfidDetected(true);
    }, 5000);

    return () => clearTimeout(simulateRFID);
  }, []);

  // Show authentication modal when both detected
  useEffect(() => {
    if (detectedPlate && rfidDetected && !showAuthModal) {
      setVehicleData({
        plateNumber: detectedPlate,
        rfidTag: rfidDetected,
        timestamp: new Date().toISOString(),
      });
      
      setTimeout(() => {
        console.log("🔐 Showing authentication modal");
        setShowAuthModal(true);
      }, 1000);
    }
  }, [detectedPlate, rfidDetected, showAuthModal]);

  const handleAuthMethodSelect = (method) => {
    console.log("🔐 Authentication method selected:", method);
    setSelectedAuthMethod(method);
    setShowAuthModal(false);
    onVehicleDetected(vehicleData);
  };

  const handleManualTrigger = () => {
    if (!detectedPlate) {
      setDetectedPlate("KBU 510 G");
      setTimeout(() => setRfidDetected(true), 1000);
    }
  };

  return (
    <div className={`h-screen bg-gradient-to-br ${theme.gradientBg} flex flex-col`}>
      {/* Header */}
      <div className={`${theme.bgPrimary} border-b ${theme.borderPrimary} px-8 py-6`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 bg-gradient-to-br from-amber-500 to-orange-600 rounded-lg flex items-center justify-center">
              <span className="text-white text-2xl font-bold">KW</span>
            </div>
            <div>
              <h1 className={`text-3xl font-bold ${theme.textPrimary}`}>Self-Service Weighing</h1>
              <p className={`${theme.textSecondary} text-sm`}>Automated tea collection system</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            {/* Simulation Badge */}
            <div className="flex items-center gap-3 bg-purple-900 text-purple-300 px-4 py-2 rounded-lg border border-purple-600">
              <span className="w-3 h-3 bg-purple-500 rounded-full animate-pulse"></span>
              <span className="font-medium">SIMULATION MODE</span>
            </div>
            <div className="flex items-center gap-3 bg-green-900 text-green-300 px-4 py-2 rounded-lg">
              <span className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></span>
              <span className="font-medium">ONLINE</span>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="max-w-4xl w-full">
          {error ? (
            <div className="text-center">
              <Alert type="error" message="Vehicle Not Recognized" description={error} showIcon className="mb-6" />
              <button onClick={onReset} className="px-8 py-3 bg-amber-600 hover:bg-amber-700 text-white rounded-lg font-semibold text-lg">
                Try Again
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-2 gap-8">
              {/* ANPR Section */}
              <div className={`${theme.bgCard} rounded-2xl p-8 border-2 ${theme.borderPrimary}`}>
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-12 h-12 rounded-full bg-blue-900 flex items-center justify-center">
                    <span className="text-2xl">📸</span>
                  </div>
                  <div>
                    <h3 className={`text-xl font-bold ${theme.textPrimary}`}>ANPR Number Plate</h3>
                    <p className={`text-sm ${theme.textSecondary}`}>
                      {detectedPlate ? "Detected" : "🎭 Simulating..."}
                    </p>
                  </div>
                </div>

                {/* Camera Feed Placeholder */}
                <div className={`mb-4 rounded-lg overflow-hidden ${theme.bgTertiary} flex items-center justify-center h-48`}>
                  <div className={`text-center ${theme.textMuted}`}>
                    <div className="text-6xl mb-2">🚗</div>
                    <p className="text-sm">{anprImage ? "Camera Feed" : "Waiting for vehicle..."}</p>
                  </div>
                </div>

                {/* Plate Display */}
                <div className="text-center">
                  {detectedPlate ? (
                    <div className="bg-white rounded-lg py-6 px-4 shadow-lg">
                      <div className="text-5xl font-bold text-gray-900 tracking-wider">{detectedPlate}</div>
                      <div className="mt-2 text-green-600 font-semibold flex items-center justify-center gap-2">
                        <span className="text-2xl">✓</span>
                        <span>DETECTED</span>
                      </div>
                    </div>
                  ) : (
                    <div className="py-12">
                      <div className="w-16 h-16 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                      <p className={theme.textSecondary}>🎭 Simulating detection...</p>
                      <p className={`text-xs ${theme.textMuted} mt-2`}>(3 seconds)</p>
                    </div>
                  )}
                </div>
              </div>

              {/* RFID Section */}
              <div className={`${theme.bgCard} rounded-2xl p-8 border-2 ${theme.borderPrimary}`}>
                <div className="flex items-center gap-3 mb-6">
                  <div className="w-12 h-12 rounded-full bg-purple-900 flex items-center justify-center">
                    <span className="text-2xl">📡</span>
                  </div>
                  <div>
                    <h3 className={`text-xl font-bold ${theme.textPrimary}`}>RFID Tag</h3>
                    <p className={`text-sm ${theme.textSecondary}`}>
                      {rfidDetected ? "Verified" : "🎭 Simulating..."}
                    </p>
                  </div>
                </div>

                <div className="text-center py-12">
                  {rfidDetected ? (
                    <div>
                      <div className="w-32 h-32 mx-auto mb-6 rounded-full bg-green-900 flex items-center justify-center">
                        <span className="text-6xl">✓</span>
                      </div>
                      <p className="text-2xl font-bold text-green-500 mb-2">RFID DETECTED</p>
                      <p className={theme.textSecondary}>Vehicle authorized</p>
                    </div>
                  ) : (
                    <div>
                      <div className="w-32 h-32 mx-auto mb-6 rounded-full border-4 border-purple-500 border-dashed flex items-center justify-center animate-pulse">
                        <span className="text-5xl">📡</span>
                      </div>
                      <p className={`text-xl ${theme.textPrimary} mb-2`}>🎭 Simulating RFID scan...</p>
                      <p className={`text-sm ${theme.textMuted}`}>(5 seconds)</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Instructions */}
          {!error && (
            <div className="mt-8">
              {/* Simulation Info */}
              <div className="bg-purple-900 bg-opacity-30 border border-purple-700 rounded-lg p-6 mb-4">
                <div className="flex items-start gap-3">
                  <span className="text-2xl">🎭</span>
                  <div>
                    <h4 className="text-lg font-semibold text-purple-300 mb-2">Simulation Mode Active</h4>
                    <p className={`${theme.textSecondary} text-sm mb-3`}>
                      ANPR and RFID detection are being simulated. In production, these will connect to real hardware.
                    </p>
                    <Button type="primary" onClick={handleManualTrigger} disabled={detectedPlate} className="bg-purple-600 hover:bg-purple-700 border-0">
                      🚀 Trigger Detection Now
                    </Button>
                  </div>
                </div>
              </div>

              {/* Instructions */}
              <div className={`${theme.infoBg} border ${theme.borderPrimary} rounded-lg p-6`}>
                <h4 className={`text-lg font-semibold ${theme.info} mb-3`}>Production Mode Instructions:</h4>
                <ol className={`${theme.textSecondary} space-y-2`}>
                  <li className="flex items-start gap-3">
                    <span className={`${theme.info} font-bold`}>1.</span>
                    <span>Drive slowly towards the weighbridge until your number plate is detected</span>
                  </li>
                  <li className="flex items-start gap-3">
                    <span className={`${theme.info} font-bold`}>2.</span>
                    <span>Ensure your RFID tag is active and readable</span>
                  </li>
                  <li className="flex items-start gap-3">
                    <span className={`${theme.info} font-bold`}>3.</span>
                    <span>Wait for both confirmations before proceeding</span>
                  </li>
                </ol>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Authentication Modal */}
      <AuthenticationMethodModal
        visible={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        onSelectNFC={() => handleAuthMethodSelect('nfc')}
      />

      {/* Footer */}
      <div className={`${theme.bgPrimary} border-t ${theme.borderPrimary} px-8 py-4 text-center`}>
        <p className={`${theme.textMuted} text-sm`}>
          Powered by <span className="font-bold text-amber-500">QALIBRATED SYSTEMS</span>
          <span className="text-purple-400"> • SIMULATION MODE</span>
        </p>
      </div>
    </div>
  );
}