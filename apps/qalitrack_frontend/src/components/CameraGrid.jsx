import React, { useEffect, useState } from "react";
import clsx from "clsx";
import { useCameraRealtime } from "../hooks/useCameraRealtime";

export default function CameraGrid({ type, cameraId = "npr1" }) {
  if (type === "live") return <LiveStreamCard cameraId={cameraId} />;
  if (type === "snapshot") return <SnapshotCard cameraId={cameraId} />;
  if (type === "plate") return <PlateCard cameraId={cameraId} />;
  return null;
}

/* -------------------------------------------------------------------------- */
/*                              LIVE STREAM CARD                               */
/* -------------------------------------------------------------------------- */
function LiveStreamCard({ cameraId }) {
  const { streamRef, streamConnected, isOnline } =
    useCameraRealtime(cameraId);

  return (
    <div className="bg-black rounded-xl overflow-hidden shadow-lg h-full flex flex-col">
      <div className="relative aspect-[16/8.5] bg-gray-950 flex-1">
        <img ref={streamRef} className="w-full h-full object-cover" />

        <StatusBadge
          label="LIVE"
          color={streamConnected ? "amber" : "red"}
          pulse={streamConnected}
        />
      </div>

      <FooterText
        main="Continuous Stream"
        sub={isOnline ? "Camera online" : "Camera offline"}
        color="amber"
      />
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*                             SNAPSHOT CARD                                   */
/* -------------------------------------------------------------------------- */
function SnapshotCard({ cameraId }) {
  const { getSnapshotUrl, isOnline } = useCameraRealtime(cameraId);
  const [tick, setTick] = useState(0);

  useEffect(() => {
    const id = setInterval(() => setTick((t) => t + 1), 1500);
    return () => clearInterval(id);
  }, []);

  return (
    <div className="bg-black rounded-xl overflow-hidden shadow-lg h-full flex flex-col">
      <div className="relative aspect-[16/8.5] bg-gray-950 flex-1">
        <img
          src={`${getSnapshotUrl()}&tick=${tick}`}
          className="w-full h-full object-cover"
        />

        <StatusBadge label="SNAPSHOT" color="orange" />
      </div>

      <FooterText
        main="Latest Frame"
        sub={isOnline ? "Auto refresh • 1.5s" : "Camera offline"}
        color="orange"
      />
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*                             PLATE CARD                                      */
/* -------------------------------------------------------------------------- */
function PlateCard({ cameraId }) {
  const { plateData, isOnline } = useCameraRealtime(cameraId);

  return (
    <div className="bg-black rounded-xl overflow-hidden shadow-lg h-full flex flex-col">
      <div className="relative aspect-[16/8.5] flex-1 flex items-center justify-center bg-gradient-to-br from-yellow-900 to-black">
        {plateData ? (
          <div className="text-center">
            <div className="text-4xl md:text-5xl font-mono font-black tracking-widest text-amber-500">
              {plateData.plateNumber}
            </div>
            <div className="mt-1 text-xs text-amber-300">
              {(plateData.confidence * 100).toFixed(1)}% confidence
            </div>
          </div>
        ) : (
          <div className="text-gray-600 text-sm">
            Waiting for vehicle…
          </div>
        )}

        <StatusBadge label="PLATE" color="yellow" />
      </div>

      <div className="p-2 text-center text-[11px] text-gray-400">
        {plateData
          ? `Detected at ${new Date(
              plateData.timestamp
            ).toLocaleTimeString()}`
          : isOnline
          ? "Real-time ANPR"
          : "Camera offline"}
      </div>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*                                UI PARTS                                     */
/* -------------------------------------------------------------------------- */
function StatusBadge({ label, color, pulse }) {
  const colors = {
    amber: "bg-amber-500 text-black",
    orange: "bg-orange-500 text-black",
    yellow: "bg-yellow-600 text-white",
    red: "bg-red-600 text-white",
  };

  return (
    <span
      className={clsx(
        "absolute top-2 left-2 px-3 py-1 rounded-full text-xs font-bold",
        colors[color],
        pulse && "animate-pulse"
      )}
    >
      {label}
    </span>
  );
}

function FooterText({ main, sub, color }) {
  const colors = {
    amber: "text-amber-400",
    orange: "text-orange-400",
    yellow: "text-yellow-400",
  };

  return (
    <div className="p-2 text-center">
      <p className={clsx("font-semibold text-sm", colors[color])}>
        {main}
      </p>
      <p className="text-[11px] text-gray-500">{sub}</p>
    </div>
  );
}
