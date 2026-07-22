import { useState, useEffect, useRef } from "react";

const BASE_IP = import.meta.env.VITE_HARDWARE_BASE_URL;
const PLATE_STREAM_URL = `${BASE_IP}/api/plates/stream`;
const CRLFCRLF = new TextEncoder().encode("\r\n\r\n");

function concatBytes(a, b) {
  const out = new Uint8Array(a.length + b.length);
  out.set(a, 0);
  out.set(b, a.length);
  return out;
}

function indexOfSequence(haystack, needle) {
  outer: for (let i = 0; i <= haystack.length - needle.length; i++) {
    for (let j = 0; j < needle.length; j++) {
      if (haystack[i + j] !== needle[j]) continue outer;
    }
    return i;
  }
  return -1;
}

// Pulls one complete frame (if any) off the front of a multipart/x-mixed-replace
// byte buffer. Relies on each part carrying a Content-Length header (true for
// this app's camera stream), so a frame's exact byte range is known without
// needing to scan for the next boundary marker.
function extractFrame(buffer) {
  const headerEnd = indexOfSequence(buffer, CRLFCRLF);
  if (headerEnd === -1) return null; // headers not fully buffered yet

  const headerText = new TextDecoder().decode(buffer.slice(0, headerEnd));
  const lengthMatch = headerText.match(/Content-Length:\s*(\d+)/i);
  if (!lengthMatch) {
    // Malformed part (no length) — drop past these headers so parsing can
    // resync on the next boundary instead of looping forever on garbage.
    return { bytes: null, rest: buffer.slice(headerEnd + CRLFCRLF.length) };
  }

  const frameStart = headerEnd + CRLFCRLF.length;
  const frameEnd = frameStart + parseInt(lengthMatch[1], 10);
  if (buffer.length < frameEnd) return null; // frame body not fully buffered yet

  return { bytes: buffer.slice(frameStart, frameEnd), rest: buffer.slice(frameEnd) };
}

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
    let reconnectTimer = null;
    let cancelled = false;

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
        }
      };

      source.onerror = () => {
        source.close();
        // Without the `cancelled` guard this timer still fires after
        // unmount, reconnecting a source nothing owns or can clean up —
        // if THAT one also errors it reschedules itself again, an
        // unbounded chain of orphaned connections that (confirmed via
        // stress-tests/_repro-sse-exhaustion.mjs) exhausts the browser's
        // per-origin connection limit after enough navigation churn.
        if (!cancelled) reconnectTimer = setTimeout(connect, 2000);
      };

      eventSourceRef.current = source;
    };

    connect();

    return () => {
      cancelled = true;
      if (reconnectTimer) clearTimeout(reconnectTimer);
      source?.close();
    };
  }, [cameraId]);

  /* ===============================
     MJPEG CAMERA STREAM
  =============================== */
  // Previously an <img src="...?t=..."> reload loop — a plain callback ref
  // has no unmount hook, and clearing img.src on unmount doesn't reliably
  // make Chromium tear down the underlying multipart/x-mixed-replace
  // connection promptly. Under repeated navigation these linger long enough
  // to exhaust the browser's per-origin connection limit (6 for HTTP/1.1),
  // after which NO further requests to the hardware host can complete for
  // the rest of the session (confirmed via stress-tests/_repro-sse-exhaustion.mjs,
  // and it reproduced even after fixing the two more obvious listener/timer
  // leaks in this file).
  //
  // fetch() + AbortController gives an actual, guaranteed cancellation
  // contract — abort() reliably releases the connection — so frames are
  // now read and parsed manually from the stream body instead of letting
  // the <img> tag manage the connection itself.
  const imgElRef = useRef(null);
  const abortRef = useRef(null);
  const reconnectTimerRef = useRef(null);
  const currentObjectUrlRef = useRef(null);

  const stopMjpegStream = () => {
    abortRef.current?.abort();
    abortRef.current = null;
    if (reconnectTimerRef.current) {
      clearTimeout(reconnectTimerRef.current);
      reconnectTimerRef.current = null;
    }
    if (currentObjectUrlRef.current) {
      URL.revokeObjectURL(currentObjectUrlRef.current);
      currentObjectUrlRef.current = null;
    }
  };

  const startMjpegStream = (img, id) => {
    const controller = new AbortController();
    abortRef.current = controller;

    (async () => {
      try {
        const res = await fetch(`${BASE_IP}/api/Camera/${id}/stream`, { signal: controller.signal });
        if (!res.ok || !res.body) throw new Error(`HTTP ${res.status}`);

        setStreamConnected(true);
        const reader = res.body.getReader();
        let buffer = new Uint8Array(0);

        for (;;) {
          const { value, done } = await reader.read();
          if (done) break;
          if (value) buffer = concatBytes(buffer, value);

          let frame;
          // eslint-disable-next-line no-cond-assign
          while ((frame = extractFrame(buffer))) {
            buffer = frame.rest;
            if (frame.bytes && imgElRef.current === img) {
              const objectUrl = URL.createObjectURL(new Blob([frame.bytes], { type: "image/jpeg" }));
              const prevUrl = currentObjectUrlRef.current;
              currentObjectUrlRef.current = objectUrl;
              img.src = objectUrl;
              if (prevUrl) URL.revokeObjectURL(prevUrl);
            }
          }
        }
      } catch (err) {
        if (controller.signal.aborted) return; // expected on unmount, not a real failure
        setStreamConnected(false);
        reconnectTimerRef.current = setTimeout(() => {
          if (imgElRef.current === img) startMjpegStream(img, id);
        }, 2000);
      }
    })();
  };

  const streamRef = (img) => {
    if (!img) {
      stopMjpegStream();
      imgElRef.current = null;
      return;
    }

    imgElRef.current = img;
    startMjpegStream(img, cameraId);
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
