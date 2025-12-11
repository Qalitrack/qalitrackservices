// src/hooks/useCameraRealtime.js
import { useState, useEffect, useRef } from 'react';

const BASE_IP = 'http://172.16.0.215:5000';
const WS_URL = `ws://${window.location.hostname}:5000/ws/plates`; // or wss:// if HTTPS

export const useCameraRealtime = (cameraId = 'npr1') => {
  const [plateData, setPlateData] = useState(null);
  const [isOnline, setIsOnline] = useState(true);
  const [streamConnected, setStreamConnected] = useState(true);

  const wsRef = useRef(null);
  const streamImgRef = useRef(null);

  // === WebSocket: Real-time plate updates (global stream, filtered by cameraId) ===
  useEffect(() => {
    const connectWs = () => {
      try {
        const ws = new WebSocket(WS_URL);

        ws.onopen = () => console.log('[WS] Connected');
        ws.onmessage = (event) => {
          try {
            const data = JSON.parse(event.data);
            if (data.cameraId === cameraId && data.plateNumber) {
              setPlateData({
                plateNumber: data.plateNumber,
                confidence: data.confidence || 0.95,
                timestamp: data.timestamp || new Date().toISOString(),
              });
            }
          } catch (e) {
            console.warn('Bad WS message', e);
          }
        };

        ws.onclose = () => {
          console.warn('[WS] Disconnected. Reconnecting...');
          setTimeout(connectWs, 2000);
        };

        ws.onerror = () => setIsOnline(false);

        wsRef.current = ws;
      } catch (err) {
        setTimeout(connectWs, 3000);
      }
    };

    connectWs();

    return () => {
      wsRef.current?.close();
    };
  }, [cameraId]);

  // === MJPEG Stream Auto-Reconnect ===
  const startMjpegStream = (imgElement) => {
    if (!imgElement) return;

    // Set initial source
    const url = `${BASE_IP}/api/Camera/${cameraId}/stream?t=${Date.now()}`;
    imgElement.src = url;

    imgElement.onerror = () => {
      setStreamConnected(false);
      setTimeout(() => {
        if (imgElement) {
          imgElement.src = url + '&retry=' + Date.now();
          setStreamConnected(true);
        }
      }, 2000);
    };

    imgElement.onload = () => setStreamConnected(true);
  };

  // Expose ref callback
  const streamRef = (el) => {
    streamImgRef.current = el;
    if (el) startMjpegStream(el);
  };

  // Snapshot URL with auto-refresh
  const getSnapshotUrl = () => `${BASE_IP}/api/Camera/${cameraId}/snapshot?t=${Date.now()}`;

  // Health check fallback
  useEffect(() => {
    const check = async () => {
      try {
        await fetch(`${BASE_IP}/api/Camera/${cameraId}/status`, { method: 'HEAD' });
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
    streamRef,              // use on <img ref={streamRef} />
    getSnapshotUrl,         // call to get fresh snapshot
  };
};
