import { useState, useEffect } from "react";

export default function WeighbridgePanel() {
  const [currentWeight, setCurrentWeight] = useState(0);

  // Simulate live weight updates
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentWeight(10000 + Math.floor(Math.random() * 2000)); // random around 10-12 tons
    }, 1500);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="bg-white shadow rounded-lg p-4 border">
      <h2 className="text-lg font-semibold text-amber-600">Weighbridge #1</h2>
      <div className="flex justify-between items-center mt-4 text-gray-700">
        <div>
          <p className="text-sm font-medium">Current</p>
          <p className="text-2xl font-bold text-green-600">{currentWeight} kg</p>
        </div>
        <div>
          <p className="text-sm font-medium">First</p>
          <p className="text-2xl font-bold">32,620</p>
        </div>
        <div>
          <p className="text-sm font-medium">Second</p>
          <p className="text-2xl font-bold">10,980</p>
        </div>
      </div>
    </div>
  );
}

