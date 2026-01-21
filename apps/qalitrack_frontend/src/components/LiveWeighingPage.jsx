import React, { useEffect, useState } from "react";
import { Card, Typography, Spin } from "antd";

export default function LiveWeighbridgeStatus({ onWeightStable, onLiveWeightChange }) {
  const [currentWeight, setCurrentWeight] = useState(null);
  const [stableWeight, setStableWeight] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Example: Replace this with your actual EventSource / WebSocket
    const es = new EventSource("http://172.16.0.1:5000/weight-stream");

    es.onmessage = (event) => {
      const weight = parseFloat(event.data);

      if (!isNaN(weight)) {
        setCurrentWeight(weight);
        onLiveWeightChange?.(weight);

        // Simple stable detection: consider weight stable if it hasn't changed significantly for 2 sec
        if (Math.abs(weight - (stableWeight || 0)) < 0.05) {
          setStableWeight(weight);
          onWeightStable?.(weight); // Notify parent
        } else {
          setStableWeight(null);
        }
      }
      setLoading(false);
    };

    es.onerror = () => {
      console.error("Live weighbridge stream error");
      es.close();
      setLoading(false);
    };

    return () => es.close();
  }, [onWeightStable, onLiveWeightChange, stableWeight]);

  return (
    <Card className="bg-neutral-900 border border-amber-600/30 shadow-lg rounded-2xl p-4 mb-6">
      <div className="flex flex-col items-center">
        <Typography.Text className="text-amber-400 font-mono text-lg">Live Weighbridge</Typography.Text>
        {loading ? (
          <Spin className="mt-3" />
        ) : (
          <>
            <div className="mt-2 text-4xl font-bold text-amber-300 font-mono">
              {currentWeight != null ? currentWeight.toFixed(2) + " kg" : "— — —"}
            </div>
            <div className="mt-1 text-sm text-amber-200">
              {stableWeight != null ? `Stable: ${stableWeight.toFixed(2)} kg` : "Detecting..."}
            </div>
          </>
        )}
      </div>
    </Card>
  );
}
