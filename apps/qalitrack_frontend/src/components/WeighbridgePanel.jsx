import React, { useEffect, useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { fetchSimulatedWeight, setDetectedPlate, setCapturedWeight } from "../store/weighingSlice";
import { Button, Spin, message } from "antd";

const WeighingPanel = () => {
  const dispatch = useDispatch();
  const { currentWeight, vehiclePosition, detectedPlate, loading, error } = useSelector(
    (state) => state.weighing
  );

  // Memoize camera feeds to prevent unnecessary re-renders
  const cameraFeeds = useMemo(
    () => [
      { id: 1, src: "https://via.placeholder.com/150?text=Camera+1" },
      { id: 2, src: "https://via.placeholder.com/150?text=Camera+2" },
      { id: 3, src: "https://via.placeholder.com/150?text=Camera+3" },
      { id: 4, src: "https://via.placeholder.com/150?text=Camera+4" },
    ],
    []
  );

  useEffect(() => {
    const interval = setInterval(() => {
      dispatch(fetchSimulatedWeight());
    }, 2000);

    const detectionInterval = setInterval(() => {
      const mockPlate = `ABC${Math.floor(Math.random() * 1000)}`;
      dispatch(setDetectedPlate(mockPlate));
      console.log("📷 Detected plate:", mockPlate);
    }, 3000);

    return () => {
      clearInterval(interval);
      clearInterval(detectionInterval);
    };
  }, [dispatch]);

  const handleCapture = () => {
    if (vehiclePosition === "Fully On" && detectedPlate) {
      dispatch(setCapturedWeight(currentWeight)); // Set captured weight for form
      message.success(`Captured weight: ${currentWeight} kg, Plate: ${detectedPlate}`);
      console.log("🚀 Auto-populating form with weight:", currentWeight);
    } else {
      message.warning("Vehicle must be fully on the bridge and a plate detected to capture.");
    }
  };

  return (
    <div className="p-6 bg-white rounded-2xl shadow-md max-w-4xl mx-auto">
      <h2 className="text-2xl font-bold mb-6 text-gray-800">Smart Weighing Panel</h2>
      {loading ? (
        <div className="flex justify-center items-center p-8">
          <Spin size="large" />
        </div>
      ) : error ? (
        <div className="text-red-500 text-center">Error: {error}</div>
      ) : (
        <>
          <div className="mb-6 p-4 bg-gray-100 rounded-lg text-center animate-pulse">
            <h3 className="text-lg font-semibold text-gray-700">Current Weight</h3>
            <p className="text-4xl font-bold text-blue-600">{currentWeight || 0} kg</p>
          </div>

          <div className="mb-6 p-4 bg-gray-100 rounded-lg text-center">
            <h3 className="text-lg font-semibold text-gray-700">Vehicle Position</h3>
            <p className="text-xl font-medium text-green-600">
              {vehiclePosition || "Not Detected"}
            </p>
            <div className="mt-2 h-4 bg-gray-300 rounded-full">
              <div
                className={`h-full rounded-full ${
                  vehiclePosition === "Fully On" ? "bg-green-500" : "bg-yellow-500"
                }`}
                style={{ width: vehiclePosition === "Fully On" ? "100%" : "50%" }}
              ></div>
            </div>
          </div>

          <div className="mb-6 grid grid-cols-2 gap-4">
            <h3 className="col-span-2 text-lg font-semibold text-gray-700">Camera Feeds</h3>
            {cameraFeeds.map((feed) => (
              <div key={feed.id} className="bg-gray-200 p-2 rounded-lg text-center">
                <img src={feed.src} alt={`Camera ${feed.id}`} className="w-full h-auto rounded" />
                {detectedPlate && (
                  <p className="mt-2 text-sm font-medium text-blue-600">
                    Detected Plate: {detectedPlate}
                  </p>
                )}
              </div>
            ))}
          </div>

          <div className="text-center">
            <Button
              type="primary"
              onClick={handleCapture}
              disabled={vehiclePosition !== "Fully On" || !detectedPlate}
              className="bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-6 rounded-lg"
            >
              Capture Weight
            </Button>
          </div>
        </>
      )}
    </div>
  );
};

export default WeighingPanel;