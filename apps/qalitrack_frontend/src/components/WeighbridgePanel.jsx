import { useState, useEffect } from "react";

export default function WeighbridgePanel({ id = 1, name = `Weighbridge #1`, onCaptureFirst, onCaptureSecond }) {
  const [currentWeight, setCurrentWeight] = useState(0);

  // Simulate live weighbridge updates
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentWeight(10000 + Math.floor(Math.random() * 2000)); // 10–12 tons
    }, 1500);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600">
        {name || `Weighbridge #${id}`}
      </h2>

      <div className="mt-4 text-gray-700">
        <p className="text-sm font-medium">Current</p>
        <p className="text-2xl font-bold text-green-600">
          {currentWeight.toLocaleString()} kg
        </p>
      </div>

      <div className="flex justify-between mt-6">
        <button
          onClick={() => onCaptureFirst?.(id, currentWeight)}
          className="px-3 py-1 rounded bg-blue-500 text-white text-sm"
        >
          Capture First
        </button>
        <button
          onClick={() => onCaptureSecond?.(id, currentWeight)}
          className="px-3 py-1 rounded bg-blue-500 text-white text-sm"
        >
          Capture Second
        </button>
      </div>
    </div>
  );
}
