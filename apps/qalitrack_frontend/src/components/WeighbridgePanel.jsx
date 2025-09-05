import { useState, useEffect } from "react";

/**
 * WeighbridgePanel component
 * ----------------------------------------
 * This panel simulates the behavior of a physical weighbridge.
 * It continuously updates the current weight (mocked values)
 * and provides buttons to capture the weight as either the
 * first weight (gross/tare depending on operation) or the
 * second weight. These captured values can then be passed
 * back to the parent component (e.g., a weighing form) for
 * storage and transaction management.
 *
 * Props:
 * - id: numeric identifier for the weighbridge (default 1)
 * - name: display name for the weighbridge (default "Weighbridge #1")
 * - onCaptureFirst: callback fired when "Capture First" is clicked
 * - onCaptureSecond: callback fired when "Capture Second" is clicked
 *
 * Usage example:
 * <WeighbridgePanel
 *    id={1}
 *    name="Main Gate Weighbridge"
 *    onCaptureFirst={(id, weight) => console.log("First:", id, weight)}
 *    onCaptureSecond={(id, weight) => console.log("Second:", id, weight)}
 * />
 */
export default function WeighbridgePanel({
  id = 1,
  name = `Weighbridge #1`,
  onCaptureFirst,
  onCaptureSecond,
}) {
  const [currentWeight, setCurrentWeight] = useState(0);

  // Simulate real-time weight updates from the weighbridge hardware
  useEffect(() => {
    const interval = setInterval(() => {
      // Weight fluctuates between 10–12 tons (10,000–12,000 kg)
      setCurrentWeight(10000 + Math.floor(Math.random() * 2000));
    }, 1500);

    // Cleanup interval on unmount
    return () => clearInterval(interval);
  }, []);

  // Handle first weight capture
  const handleFirstCapture = () => {
    if (onCaptureFirst) {
      onCaptureFirst(id, currentWeight);
    }
  };

  // Handle second weight capture
  const handleSecondCapture = () => {
    if (onCaptureSecond) {
      onCaptureSecond(id, currentWeight);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-4 border w-full max-w-sm">
      {/* Panel Header */}
      <h2 className="text-lg font-semibold text-amber-600">
        {name || `Weighbridge #${id}`}
      </h2>

      {/* Real-time weight display */}
      <div className="mt-4 text-gray-700">
        <p className="text-sm font-medium">Current Reading</p>
        <p className="text-2xl font-bold text-green-600">
          {currentWeight.toLocaleString()} kg
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Updated every 1.5 seconds (simulated live data)
        </p>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-between mt-6 gap-2">
        <button
          onClick={handleFirstCapture}
          className="flex-1 px-3 py-2 rounded bg-blue-500 hover:bg-blue-600 text-white text-sm font-medium transition"
        >
          Capture First Weight
        </button>
        <button
          onClick={handleSecondCapture}
          className="flex-1 px-3 py-2 rounded bg-indigo-500 hover:bg-indigo-600 text-white text-sm font-medium transition"
        >
          Capture Second Weight
        </button>
      </div>

      {/* Info/Instructions */}
      <div className="mt-4 text-xs text-gray-500">
        <p>
          Use <span className="font-semibold">Capture First</span> when the
          vehicle enters the weighbridge. After loading/unloading, return and
          click <span className="font-semibold">Capture Second</span> to record
          the second weight.
        </p>
        <p className="mt-1">
          Net weight will be automatically calculated by subtracting the second
          captured value from the first.
        </p>
      </div>
    </div>
  );
}
