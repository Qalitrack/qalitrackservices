import React, { useEffect, useMemo, useRef } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchSimulatedWeight, setDetectedPlate, setCapturedWeight } from "../store/weighingSlice";
import { Button, message } from "antd";
import { motion, AnimatePresence } from "framer-motion"; // Optional: for ultra-smooth plate reveal

const WeighingPanel = () => {
  const dispatch = useDispatch();
  const { currentWeight, vehiclePosition, detectedPlate } = useSelector((state) => state.weighing);

  // Prevent unnecessary re-renders of camera config
  const cameraFeeds = useMemo(
    () => [
      { id: 1, label: "Front View" },
      { id: 2, label: "Rear View" },
      { id: 3, label: "Side Left" },
      { id: 4, label: "Side Right" },
    ],
    []
  );

  // Keep previous weight for smooth transition
  const prevWeightRef = useRef(currentWeight);
  const weightDirection = currentWeight > prevWeightRef.current ? "up" : "down";
  prevWeightRef.current = currentWeight;

  useEffect(() => {
    const weightInterval = setInterval(() => {
      dispatch(fetchSimulatedWeight());
    }, 2000);

    const plateInterval = setInterval(() => {
      const mockPlate = `TN-${Math.floor(Math.random() * 90) + 10} ${String.fromCharCode(65 + Math.floor(Math.random() * 26))}${String.fromCharCode(65 + Math.floor(Math.random() * 26))} ${Math.floor(Math.random() * 9999)}`;
      dispatch(setDetectedPlate(mockPlate));
    }, 8000); // Less frequent = more realistic

    return () => {
      clearInterval(weightInterval);
      clearInterval(plateInterval);
    };
  }, [dispatch]);

  const handleCapture = () => {
    if (vehiclePosition === "Fully On" && detectedPlate && currentWeight > 1000) {
      dispatch(setCapturedWeight(currentWeight));
      message.success({
        content: `Captured: ${currentWeight.toLocaleString()} kg | Plate: ${detectedPlate}`,
        duration: 3,
      });
    } else {
      message.warning("Vehicle not fully on bridge or no plate detected");
    }
  };

  const isStable = currentWeight > 100 && Math.abs(currentWeight - prevWeightRef.current) < 50;

  return (
    <div className="bg-gradient-to-br from-slate-50 to-slate-100 p-6 rounded-2xl shadow-xl border border-slate-200">
      

      

      {/* Vehicle Position */}
      <div className="bg-white rounded-2xl p-6 shadow-inner border border-slate-200 mb-6">
        <p className="text-sm text-slate-600 font-medium uppercase tracking-wider mb-3">Vehicle Position</p>
        <div className="space-y-3">
          <p className={`text-lg font-semibold text-center ${
            vehiclePosition === "Fully On" ? "text-green-600" : "text-amber-600"
          }`}>
            {vehiclePosition || "Waiting for vehicle..."}
          </p>
          <div className="h-6 bg-slate-200 rounded-full overflow-hidden">
            <motion.div
              className="h-full bg-gradient-to-r from-amber-500 to-green-500"
              animate={{
                width: vehiclePosition === "Fully On" ? "100%" : vehiclePosition === "Partial" ? "65%" : "30%",
              }}
              transition={{ duration: 0.8, ease: "easeOut" }}
            />
          </div>
        </div>
      </div>

      {/* Camera Grid */}
      <div className="grid grid-cols-2 gap-4 mb-6">
        {cameraFeeds.map((feed) => (
          <div
            key={feed.id}
            className="relative bg-black rounded-xl overflow-hidden shadow-lg group"
          >
            <div className="aspect-video relative">
              <div className="absolute inset-0 bg-gradient-to-br from-blue-600/20 to-purple-600/20" />
              <img
                src={`https://picsum.photos/400/300?random=${feed.id}`}
                alt={feed.label}
                className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                loading="lazy"
              />
              <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-3">
                <p className="text-white text-xs font-medium">{feed.label}</p>
              </div>

              {/* Plate Overlay - Only on one camera */}
              {detectedPlate && feed.id === 1 && (
                <motion.div
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  exit={{ y: -20, opacity: 0 }}
                  className="absolute top-3 left-3 bg-black/80 text-white px-4 py-2 rounded-lg font-mono text-lg font-bold shadow-2xl border-2 border-green-400"
                >
                  {detectedPlate}
                </motion.div>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Capture Button */}
      <div className="text-center">
        <Button
          type="primary"
          size="large"
          onClick={handleCapture}
          disabled={vehiclePosition !== "Fully On" || !detectedPlate || !isStable}
          className={`font-bold text-lg px-10 py-6 h-auto rounded-xl shadow-lg transition-all ${
            vehiclePosition === "Fully On" && detectedPlate && isStable
              ? "bg-green-600 hover:bg-green-700 scale-100"
              : "bg-gray-400 cursor-not-allowed"
          }`}
        >
          {vehiclePosition === "Fully On" && detectedPlate && isStable
            ? "Capture Weight & Plate"
            : "Waiting for Stable Vehicle..."}
        </Button>
      </div>

      {/* Optional: Small status bar */}
      <div className="mt-4 text-center text-xs text-slate-500">
        Auto-refresh: Weight every 2s • Plate detection every 8s
      </div>
    </div>
  );
};

export default WeighingPanel;