import React, { useEffect, useState } from "react";
import { Alert, Button } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";
import AuthenticationMethodModal from "./AuthenticationMethodModal.jsx";

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
    <div className={`min-h-screen ${isDark ? 'bg-gradient-to-br from-gray-900 via-gray-800 to-black' : 'bg-gradient-to-br from-gray-50 via-white to-gray-100'}`}>
      {/* Header */}
      <div className={`${isDark ? 'bg-gray-900 border-gray-700' : 'bg-white border-gray-200'} border-b px-8 py-6 shadow-sm`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 bg-gradient-to-br from-amber-500 to-orange-600 rounded-xl flex items-center justify-center shadow-lg">
              <span className="text-white text-2xl font-bold">KW</span>
            </div>
            <div>
              <h1 className={`text-3xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Self-Service Weighing
              </h1>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                Automated tea collection system
              </p>
            </div>
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
            <div className={`flex items-center gap-3 px-4 py-2 rounded-lg ${
              isDark 
                ? 'bg-green-900/30 text-green-300' 
                : 'bg-green-50 text-green-700'
            }`}>
              <span className="w-3 h-3 bg-green-500 rounded-full animate-pulse"></span>
              <span className="font-medium text-sm">ONLINE</span>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex items-center justify-center p-8 min-h-[calc(100vh-180px)]">
        <div className="max-w-6xl w-full">
          {error ? (
            <div className="text-center">
              <Alert 
                type="error" 
                message="Vehicle Not Recognized" 
                description={error} 
                showIcon 
                className="mb-6" 
              />
              <Button 
                size="large"
                onClick={onReset} 
                className="px-8 py-6 h-auto bg-amber-600 hover:bg-amber-700 text-white rounded-xl font-semibold text-lg border-0"
              >
                Try Again
              </Button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {/* ANPR Section */}
              <div className={`${
                isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'
              } rounded-2xl p-8 border-2 shadow-xl`}>
                <div className="flex items-center gap-3 mb-6">
                  <div className={`w-14 h-14 rounded-xl ${
                    isDark ? 'bg-blue-900/50' : 'bg-blue-50'
                  } flex items-center justify-center`}>
                    <span className="text-3xl">📸</span>
                  </div>
                  <div>
                    <h3 className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                      ANPR Number Plate
                    </h3>
                    <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                      {detectedPlate ? "✓ Detected" : "🎭 Simulating..."}
                    </p>
                  </div>
                </div>

                {/* Camera Feed Placeholder */}
                <div className={`mb-6 rounded-xl overflow-hidden ${
                  isDark ? 'bg-gray-900' : 'bg-gray-100'
                } flex items-center justify-center h-56 shadow-inner`}>
                  <div className={`text-center ${isDark ? 'text-gray-600' : 'text-gray-400'}`}>
                    <div className="text-7xl mb-3 opacity-50">🚗</div>
                    <p className="text-sm font-medium">
                      {anprImage ? "Camera Feed Active" : "Waiting for vehicle..."}
                    </p>
                  </div>
                </div>

                {/* Plate Display */}
                <div className="text-center">
                  {detectedPlate ? (
                    <div className="bg-white rounded-xl py-8 px-6 shadow-2xl border-4 border-gray-200">
                      <div className="text-6xl font-black text-gray-900 tracking-wider mb-3">
                        {detectedPlate}
                      </div>
                      <div className="flex items-center justify-center gap-2 text-green-600 font-bold text-lg">
                        <span className="text-2xl">✓</span>
                        <span>DETECTED</span>
                      </div>
                    </div>
                  ) : (
                    <div className="py-16">
                      <div className="w-20 h-20 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                      <p className={`text-lg ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                        🎭 Simulating detection...
                      </p>
                      <p className={`text-xs mt-2 ${isDark ? 'text-gray-600' : 'text-gray-500'}`}>
                        (3 seconds)
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {/* RFID Section */}
              <div className={`${
                isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'
              } rounded-2xl p-8 border-2 shadow-xl`}>
                <div className="flex items-center gap-3 mb-6">
                  <div className={`w-14 h-14 rounded-xl ${
                    isDark ? 'bg-purple-900/50' : 'bg-purple-50'
                  } flex items-center justify-center`}>
                    <span className="text-3xl">📡</span>
                  </div>
                  <div>
                    <h3 className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>
                      RFID Tag
                    </h3>
                    <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                      {rfidDetected ? "✓ Verified" : "🎭 Simulating..."}
                    </p>
                  </div>
                </div>

                <div className="text-center py-16">
                  {rfidDetected ? (
                    <div>
                      <div className={`w-40 h-40 mx-auto mb-8 rounded-full ${
                        isDark ? 'bg-green-900/50' : 'bg-green-50'
                      } flex items-center justify-center shadow-2xl`}>
                        <span className="text-7xl">✓</span>
                      </div>
                      <p className="text-3xl font-bold text-green-500 mb-3">RFID DETECTED</p>
                      <p className={isDark ? 'text-gray-400' : 'text-gray-600'}>
                        Vehicle authorized
                      </p>
                    </div>
                  ) : (
                    <div>
                      <div className="w-40 h-40 mx-auto mb-8 rounded-full border-4 border-purple-500 border-dashed flex items-center justify-center animate-pulse">
                        <span className="text-6xl">📡</span>
                      </div>
                      <p className={`text-2xl mb-3 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                        🎭 Simulating RFID scan...
                      </p>
                      <p className={`text-sm ${isDark ? 'text-gray-600' : 'text-gray-500'}`}>
                        (5 seconds)
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Instructions */}
          {!error && (
            <div className="mt-8 space-y-4">
              {/* Simulation Info */}
              <div className={`${
                isDark 
                  ? 'bg-purple-900/20 border-purple-700' 
                  : 'bg-purple-50 border-purple-200'
              } border rounded-xl p-6`}>
                <div className="flex items-start gap-4">
                  <span className="text-3xl">🎭</span>
                  <div className="flex-1">
                    <h4 className={`text-lg font-semibold mb-2 ${
                      isDark ? 'text-purple-300' : 'text-purple-900'
                    }`}>
                      Simulation Mode Active
                    </h4>
                    <p className={`text-sm mb-4 ${isDark ? 'text-purple-200' : 'text-purple-800'}`}>
                      ANPR and RFID detection are being simulated. In production, these will connect to real hardware.
                    </p>
                    <Button 
                      type="primary" 
                      onClick={handleManualTrigger} 
                      disabled={detectedPlate}
                      className="bg-purple-600 hover:bg-purple-700 border-0 h-10"
                    >
                      🚀 Trigger Detection Now
                    </Button>
                  </div>
                </div>
              </div>

              {/* Instructions */}
              <div className={`${
                isDark 
                  ? 'bg-blue-900/20 border-blue-700' 
                  : 'bg-blue-50 border-blue-200'
              } border rounded-xl p-6`}>
                <h4 className={`text-lg font-semibold mb-4 ${
                  isDark ? 'text-blue-300' : 'text-blue-900'
                }`}>
                  Production Mode Instructions:
                </h4>
                <ol className={`space-y-3 ${isDark ? 'text-blue-200' : 'text-blue-800'}`}>
                  <li className="flex items-start gap-3">
                    <span className="font-bold text-blue-500">1.</span>
                    <span>Drive slowly towards the weighbridge until your number plate is detected</span>
                  </li>
                  <li className="flex items-start gap-3">
                    <span className="font-bold text-blue-500">2.</span>
                    <span>Ensure your RFID tag is active and readable</span>
                  </li>
                  <li className="flex items-start gap-3">
                    <span className="font-bold text-blue-500">3.</span>
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
      <div className={`${
        isDark ? 'bg-gray-900 border-gray-700' : 'bg-white border-gray-200'
      } border-t px-8 py-5 text-center shadow-sm`}>
        <p className={`text-sm ${isDark ? 'text-gray-500' : 'text-gray-600'}`}>
          Powered by <span className="font-bold text-amber-500">QALIBRATED SYSTEMS</span>
          <span className="text-purple-500"> • SIMULATION MODE</span>
        </p>
      </div>
    </div>
  );
}