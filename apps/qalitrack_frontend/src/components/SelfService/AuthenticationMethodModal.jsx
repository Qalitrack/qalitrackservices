import React from "react";
import { Modal } from "antd";
import { useTheme } from "../Context/ThemeContext.jsx";

export default function AuthenticationMethodModal({ 
  visible, 
  onClose, 
  onSelectNFC
}) {
  const { theme, isDark } = useTheme();

  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width={650}
      centered
      closeIcon={
        <span className={`text-3xl ${isDark ? 'text-gray-500 hover:text-gray-300' : 'text-gray-500 hover:text-gray-700'}`}>
          ×
        </span>
      }
      styles={{
        body: { 
          padding: '48px',
          backgroundColor: isDark ? '#1f2937' : '#ffffff'
        },
        mask: {
          backgroundColor: 'rgba(0, 0, 0, 0.7)'
        }
      }}
      className={isDark ? 'dark-modal' : 'light-modal'}
    >
      <div className="text-center">
        <h2 className={`text-4xl font-bold mb-4 ${isDark ? 'text-white' : 'text-gray-900'}`}>
          Driver Authentication
        </h2>
        <p className={`text-xl mb-10 ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
          Please tap your NFC card on the reader
        </p>

        {/* NFC Card Option - Full Width */}
        <button
          onClick={onSelectNFC}
          className="w-full p-10 bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded-2xl border-4 border-amber-600 transition-all transform hover:scale-105 shadow-2xl"
        >
          <div className="flex flex-col items-center gap-5">
            <div className="text-8xl animate-pulse">📡</div>
            <span className="text-4xl font-bold">NFC Card</span>
            <span className="text-base text-amber-100">Tap your card to continue</span>
          </div>
        </button>

        {/* Instructions */}
        <div className={`mt-8 p-5 rounded-xl ${
          isDark 
            ? 'bg-gray-800 border border-gray-700' 
            : 'bg-gray-50 border border-gray-200'
        }`}>
          <p className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
            <strong className={isDark ? 'text-white' : 'text-gray-900'}>Instructions:</strong> Hold your NFC card against the reader until you hear a beep
          </p>
        </div>
      </div>
    </Modal>
  );
}