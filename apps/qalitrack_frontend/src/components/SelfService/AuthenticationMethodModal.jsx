import React from "react";
import { Modal } from "antd";

export default function AuthenticationMethodModal({ 
  visible, 
  onClose, 
  onSelectNFC
}) {
  return (
    <Modal
      open={visible}
      onCancel={onClose}
      footer={null}
      width={600}
      centered
      closeIcon={
        <span className="text-2xl text-gray-500 hover:text-gray-700">×</span>
      }
      styles={{
        body: { padding: '40px' }
      }}
    >
      <div className="text-center">
        <h2 className="text-3xl font-bold text-gray-900 mb-3">
          Driver Authentication
        </h2>
        <p className="text-lg text-gray-600 mb-8">
          Please tap your NFC card on the reader
        </p>

        {/* NFC Card Option - Full Width */}
        <button
          onClick={onSelectNFC}
          className="w-full p-8 bg-gradient-to-r from-amber-500 to-orange-600 hover:from-amber-600 hover:to-orange-700 text-white rounded-2xl border-4 border-amber-600 transition-all transform hover:scale-105 shadow-2xl"
        >
          <div className="flex flex-col items-center gap-4">
            <div className="text-7xl animate-pulse">📡</div>
            <span className="text-3xl font-bold">NFC Card</span>
            <span className="text-sm text-amber-100">Tap your card to continue</span>
          </div>
        </button>

        {/* Instructions */}
        <div className="mt-6 p-4 bg-gray-50 rounded-lg">
          <p className="text-sm text-gray-600">
            <strong>Instructions:</strong> Hold your NFC card against the reader until you hear a beep
          </p>
        </div>
      </div>
    </Modal>
  );
}