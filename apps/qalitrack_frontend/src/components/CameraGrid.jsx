import React, { useEffect, useState } from "react";
import clsx from "clsx";
import { useCameraRealtime } from "../hooks/useCameraRealtime";

export default function CameraGrid({ type, cameraId = "npr1", onPlateConfirmed }) {
  if (type === "live") return <LiveStreamCard cameraId={cameraId} />;
  if (type === "snapshot") return <SnapshotCard cameraId={cameraId} />;
  if (type === "plate") return <PlateCard cameraId={cameraId} onPlateConfirmed={onPlateConfirmed} />;
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
function PlateCard({ cameraId, onPlateConfirmed }) {
  const { plateData, isOnline } = useCameraRealtime(cameraId);
  const [editing, setEditing] = useState(false);
  const [editedPlate, setEditedPlate] = useState("");

  // When a new plate is auto-detected, update the edit field
  useEffect(() => {
    if (plateData?.plateNumber && !editing) {
      setEditedPlate(plateData.plateNumber);
    }
  }, [plateData?.plateNumber, editing]);

  const handleUse = (source) => {
    const plate = editedPlate.trim().toUpperCase();
    if (!plate) return;
    if (typeof onPlateConfirmed === "function") onPlateConfirmed(plate, source);
    setEditing(false);
  };

  return (
    <div className="bg-black rounded-xl overflow-hidden shadow-lg h-full flex flex-col">
      <div className="relative aspect-[16/8.5] flex-1 flex items-center justify-center bg-gradient-to-br from-yellow-900 to-black">
        {editing ? (
          <div className="text-center px-4 w-full">
            <input
              autoFocus
              value={editedPlate}
              onChange={(e) => setEditedPlate(e.target.value.toUpperCase())}
              onKeyDown={(e) => e.key === "Enter" && handleUse("manual")}
              className="w-full text-center text-3xl font-mono font-black tracking-widest bg-transparent border-b-2 border-amber-500 text-amber-400 outline-none pb-1"
              placeholder="KXX 000X"
            />
            <div className="flex justify-center gap-2 mt-3">
              <button onClick={() => handleUse("manual")}
                className="bg-amber-500 hover:bg-amber-600 text-black text-xs font-bold px-3 py-1 rounded">
                Use
              </button>
              <button onClick={() => setEditing(false)}
                className="bg-neutral-700 hover:bg-neutral-600 text-white text-xs px-3 py-1 rounded">
                Cancel
              </button>
            </div>
          </div>
        ) : editedPlate ? (
          <div className="text-center">
            <div className="text-4xl md:text-5xl font-mono font-black tracking-widest text-amber-500">
              {editedPlate}
            </div>
            {plateData?.confidence && (
              <div className="mt-1 text-xs text-amber-300">
                {(plateData.confidence * 100).toFixed(1)}% confidence
              </div>
            )}
            <div className="flex justify-center gap-2 mt-3">
              <button onClick={() => handleUse("auto")}
                className="bg-amber-500 hover:bg-amber-600 text-black text-xs font-bold px-3 py-1 rounded">
                Use Plate
              </button>
              <button onClick={() => setEditing(true)}
                className="bg-neutral-700 hover:bg-neutral-600 text-white text-xs px-3 py-1 rounded">
                Edit
              </button>
            </div>
          </div>
        ) : (
          <div className="text-center">
            <div className="text-gray-600 text-sm">Waiting for vehicle…</div>
          </div>
        )}

        <StatusBadge label="PLATE" color="yellow" />
      </div>

      <div className="p-2 text-center text-[11px] text-gray-400">
        {plateData
          ? `Detected at ${new Date(plateData.timestamp).toLocaleTimeString()}`
          : isOnline ? "Real-time ANPR" : "Camera offline"}
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
