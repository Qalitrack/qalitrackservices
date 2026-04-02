import { useState, useEffect, useRef } from "react";

const BASE_IP = "http://localhost:5000";
const PLATE_STREAM_URL = `${BASE_IP}/api/plates/stream`;

export const useCameraRealtime = (cameraId = "npr1") => {
  const [plateData, setPlateData] = useState(null);
  const [isOnline, setIsOnline] = useState(true);
  const [streamConnected, setStreamConnected] = useState(true);

  const eventSourceRef = useRef(null);

  /* ===============================
     PLATE STREAM (SSE)
  =============================== */
  useEffect(() => {
    let source;

    const connect = () => {
      source = new EventSource(PLATE_STREAM_URL);

      source.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);

          if (data.cameraId === cameraId && data.plateNumber) {
            setPlateData({
              plateNumber: data.plateNumber,
              confidence: data.confidence ?? 0.95,
              timestamp: data.timestamp ?? new Date().toISOString(),
            });
          }
        } catch (err) {
          console.warn("[PLATES] Bad data", err);
        }
      };

      source.onerror = () => {
        console.warn("[PLATES] SSE disconnected — reconnecting");
        source.close();
        setTimeout(connect, 2000);
      };

      eventSourceRef.current = source;
    };

    connect();

    return () => source?.close();
  }, [cameraId]);

  /* ===============================
     MJPEG CAMERA STREAM
  =============================== */
  const streamRef = (img) => {
    if (!img) return;

    const baseUrl = `${BASE_IP}/api/Camera/${cameraId}/stream`;

    const load = () => {
      img.src = `${baseUrl}?t=${Date.now()}`;
    };

    img.onerror = () => {
      setStreamConnected(false);
      setTimeout(load, 2000);
    };

    img.onload = () => setStreamConnected(true);

    load();
  };

  /* ===============================
     SNAPSHOT
  =============================== */
  const getSnapshotUrl = () =>
    `${BASE_IP}/api/Camera/${cameraId}/snapshot?t=${Date.now()}`;

  /* ===============================
     HEALTH CHECK
  =============================== */
  useEffect(() => {
    const check = async () => {
      try {
        await fetch(`${BASE_IP}/api/Camera/${cameraId}/status`, {
          method: "HEAD",
        });
        setIsOnline(true);
      } catch {
        setIsOnline(false);
      }
    };

    check();
    const id = setInterval(check, 10000);
    return () => clearInterval(id);
  }, [cameraId]);

  return {
    plateData,
    isOnline,
    streamConnected,
    streamRef,
    getSnapshotUrl,
  };
};
