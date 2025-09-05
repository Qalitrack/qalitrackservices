import { useState, useEffect } from "react";

export default function HardwareControls({ onStatusChange }) {
  const [status, setStatus] = useState({
    vehiclePosition: "Approaching",
    gateStatus: "Operational",
    trafficLights: "Green",
    systemStatus: "Online",
  });

  // Simulate hardware polling (mock) — replace with Masterdom API later
  useEffect(() => {
    const interval = setInterval(() => {
      // 🔴 Mock random changes to simulate hardware updates
      const randomize = Math.random();
      setStatus((prev) => ({
        vehiclePosition:
          randomize > 0.8 ? "Not Positioned" : "Approaching",
        gateStatus: randomize > 0.9 ? "Faulty" : "Operational",
        trafficLights: randomize > 0.7 ? "Red" : "Green",
        systemStatus: randomize > 0.95 ? "Offline" : "Online",
      }));
    }, 5000);

    return () => clearInterval(interval);
  }, []);

  // Compute if all statuses are "healthy"
  const allHealthy =
    status.vehiclePosition === "Approaching" &&
    status.gateStatus === "Operational" &&
    status.trafficLights === "Green" &&
    status.systemStatus === "Online";

  // Notify parent (like WeighbridgePanel) whenever status changes
  useEffect(() => {
    if (onStatusChange) onStatusChange(allHealthy, status);
  }, [status, allHealthy, onStatusChange]);

  return (
    <div className="border rounded bg-white shadow p-4 mt-4">
      <h3 className="text-lg font-semibold text-amber-600 mb-3">
        Hardware Controls
      </h3>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Vehicle Position</p>
          <p
            className={`font-bold ${
              status.vehiclePosition === "Approaching"
                ? "text-green-600"
                : "text-red-600"
            }`}
          >
            {status.vehiclePosition}
          </p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Gate Status</p>
          <p
            className={`font-bold ${
              status.gateStatus === "Operational"
                ? "text-blue-600"
                : "text-red-600"
            }`}
          >
            {status.gateStatus}
          </p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">Traffic Lights</p>
          <p
            className={`font-bold ${
              status.trafficLights === "Green"
                ? "text-green-600"
                : "text-red-600"
            }`}
          >
            {status.trafficLights}
          </p>
        </div>
        <div className="p-2 border rounded bg-gray-50">
          <p className="font-medium">System Status</p>
          <p
            className={`font-bold ${
              status.systemStatus === "Online"
                ? "text-green-600"
                : "text-red-600"
            }`}
          >
            {status.systemStatus}
          </p>
        </div>
      </div>
    </div>
  );
}
