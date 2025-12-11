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
      

      {/* Camera Grid */}
      

      {/* Capture Button */}
 
    </div>
  );
};

export default WeighingPanel;