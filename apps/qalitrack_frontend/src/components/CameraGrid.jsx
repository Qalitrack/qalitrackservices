// src/components/CameraGrid.jsx
import React, { useEffect, useState } from "react";
import { CAMERA_CONFIG } from "../config";

const { cameraId, streamUrl, snapshotUrl, platesUrl } = CAMERA_CONFIG;

export default function CameraGrid({ type }) {
  if (type === "live") return <LiveStreamCard />;
  if (type === "snapshot") return <SnapshotCard />;
  if (type === "plate") return <PlateCard />;
  return null;
}

/* -------------------------------------------------------------------------- */
/*                              LIVE STREAM CARD                               */
/* -------------------------------------------------------------------------- */
function LiveStreamCard() {
  const [reloadKey, setReloadKey] = useState(0);

  return (
    <div className="bg-black rounded-2xl overflow-hidden shadow-xl h-full flex flex-col">
      <div className="relative aspect-video bg-gray-950 flex-1">
        <img
          key={reloadKey}
          src={`${streamUrl}?t=${Date.now()}`}
          alt="Live camera stream"
          className="w-full h-full object-cover"
          onError={() => setReloadKey((k) => k + 1)}
        />

        <span className="absolute top-3 left-3 px-4 py-1.5 rounded-full text-sm font-bold bg-amber-500 text-black animate-pulse">
          LIVE
        </span>
      </div>
      <div className="p-3 text-center">
        <p className="text-amber-400 font-semibold tracking-wide">
          Continuous Stream
        </p>
      </div>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*                             SNAPSHOT CARD                                   */
/* -------------------------------------------------------------------------- */
function SnapshotCard() {
  const [tick, setTick] = useState(0);

  useEffect(() => {
    const id = setInterval(() => setTick((t) => t + 1), 1500);
    return () => clearInterval(id);
  }, []);

  return (
    <div className="bg-black rounded-2xl overflow-hidden shadow-xl h-full flex flex-col">
      <div className="relative aspect-video bg-gray-950 flex-1">
        <img
          src={`${snapshotUrl}?t=${tick}`}
          alt="Latest snapshot"
          className="w-full h-full object-cover"
        />
        <span className="absolute top-3 left-3 px-4 py-1.5 rounded-full text-sm font-bold bg-green-500 text-black">
          SNAPSHOT
        </span>
      </div>
      <div className="p-3 text-center">
        <p className="text-green-400 font-semibold">Latest Frame</p>
        <p className="text-xs text-gray-500">Auto refresh • 1.5s</p>
      </div>
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*                             PLATE CARD                                       */
/* -------------------------------------------------------------------------- */
function PlateCard() {
  const [plate, setPlate] = useState(null);

  useEffect(() => {
    const fetchPlate = async () => {
      try {
        const res = await fetch(platesUrl, { cache: "no-store" });
        if (!res.ok) return;

        const data = await res.json();
        let latest = null;

        if (Array.isArray(data)) {
          latest =
            data
              .filter((p) => p.cameraId === cameraId)
              .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))[0] ||
            null;
        } else if (data?.cameraId === cameraId) {
          latest = data;
        }

        setPlate(latest);
      } catch {
        // silent retry
      }
    };

    fetchPlate();
    const id = setInterval(fetchPlate, 2000);
    return () => clearInterval(id);
  }, []);

  return (
    <div className="bg-black rounded-2xl overflow-hidden shadow-xl h-full flex flex-col">
      <div className="relative aspect-video flex-1 flex items-center justify-center bg-gradient-to-br from-purple-900 to-black">
        {plate ? (
          <div className="text-center">
            <div className="text-5xl md:text-6xl font-mono font-black tracking-widest text-amber-500">
              {plate.plateNumber || plate.plate}
            </div>
            <div className="mt-2 text-sm text-amber-300">
              {(plate.confidence * 100).toFixed(1)}% confidence
            </div>
          </div>
        ) : (
          <div className="text-gray-600 text-lg">Waiting for vehicle…</div>
        )}

        <span className="absolute top-3 left-3 px-4 py-1.5 rounded-full text-sm font-bold bg-purple-600 text-white">
          PLATE
        </span>
      </div>

      <div className="p-3 text-center text-xs text-gray-400 flex-shrink-0">
        {plate
          ? `Detected at ${new Date(plate.timestamp).toLocaleTimeString()}`
          : "Real-time ANPR"}
      </div>
    </div>
  );
}
