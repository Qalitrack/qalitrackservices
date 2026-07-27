// stress-tests/mock-hardware-server.mjs
//
// Simulates the physical hardware endpoints the app expects at
// VITE_HARDWARE_BASE_URL (http://localhost:5000, per .env) so the REAL
// streaming code paths (EventSource weight/plate feeds, MJPEG camera feed,
// snapshot polling, health checks) get exercised under sustained load
// instead of just hammering failed-connect/retry loops.
//
// Endpoints (matching src/hooks/useCameraRealtime.js, src/config.js,
// src/hooks/useHardwareConfig.js, src/components/weighing/LiveWeighbridgeStatus.jsx):
//   GET  /api/PlatformData/stream   SSE  — { weight: number }        every 250ms
//   GET  /api/plates/stream         SSE  — { cameraId, plateNumber, confidence, timestamp } every ~7s
//   GET  /api/Camera/:id/stream     MJPEG multipart (image/jpeg frames)  ~2fps
//   GET  /api/Camera/:id/snapshot   single JPEG
//   HEAD /api/Camera/:id/status     200 OK, no body
//
// Standalone Node http server, no extra deps. Exported `start()`/`stop()` so
// stress-tests/run.mjs can manage its lifecycle; also runnable directly:
//   node stress-tests/mock-hardware-server.mjs

import http from 'node:http';
import { pathToFileURL } from 'node:url';

// Tiny 64x36 placeholder JPEG (generated once via jimp) — content doesn't
// matter for a stress test, only that a real image/jpeg stream is flowing.
const FRAME = Buffer.from(
  '/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAf/AABEIACQAQAMBEQACEQEDEQH/xAGiAAABBQEBAQEBAQAAAAAAAAAAAQIDBAUGBwgJCgsQAAIBAwMCBAMFBQQEAAABfQECAwAEEQUSITFBBhNRYQcicRQygZGhCCNCscEVUtHwJDNicoIJChYXGBkaJSYnKCkqNDU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6g4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2drh4uPk5ebn6Onq8fLz9PX29/j5+gEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoLEQACAQIEBAMEBwUEBAABAncAAQIDEQQFITEGEkFRB2FxEyIygQgUQpGhscEJIzNS8BVictEKFiQ04SXxFxgZGiYnKCkqNTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqCg4SFhoeIiYqSk5SVlpeYmZqio6Slpqeoqaqys7S1tre4ubrCw8TFxsfIycrS09TV1tfY2dri4+Tl5ufo6ery8/T19vf4+fr/2gAMAwEAAhEDEQA/AP5T6ACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKACgAoAKAP8A/9k=',
  'base64'
);

const PLATES = ['KDA 001A', 'KDB 214T', 'KCX 552K', 'KDD 771M', 'KBZ 903P'];
const CAMERA_FPS_MS = 500;
const PLATE_EVERY_MS = 7000;
const WEIGHT_TICK_MS = 250;

function sseHeaders(res) {
  res.writeHead(200, {
    'Content-Type': 'text/event-stream',
    'Cache-Control': 'no-cache',
    Connection: 'keep-alive',
    'Access-Control-Allow-Origin': '*',
  });
}

function sseSend(res, obj) {
  res.write(`data: ${JSON.stringify(obj)}\n\n`);
}

// Weight cycle: idle at 0, ramp up to a random target, hold, ramp down.
function makeWeightSimulator() {
  let phase = 'idle'; // idle -> ramping_up -> holding -> ramping_down
  let current = 0;
  let target = 0;
  let holdTicks = 0;

  return function next() {
    switch (phase) {
      case 'idle':
        if (Math.random() < 0.05) {
          target = 800 + Math.round(Math.random() * 25000); // 800kg - 25.8t
          phase = 'ramping_up';
        }
        break;
      case 'ramping_up':
        current = Math.min(target, current + target / 20);
        if (current >= target) phase = 'holding';
        break;
      case 'holding':
        holdTicks++;
        if (holdTicks > 20) { holdTicks = 0; phase = 'ramping_down'; }
        break;
      case 'ramping_down':
        current = Math.max(0, current - target / 20);
        if (current <= 0) { current = 0; phase = 'idle'; }
        break;
    }
    // small jitter, like a real scale never sitting at an exact integer
    return Math.round((current + (Math.random() - 0.5) * 4) * 100) / 100;
  };
}

export function start(port = 5000) {
  const weightNext = makeWeightSimulator();
  const timers = new Set();
  const sseClients = new Set();

  const server = http.createServer((req, res) => {
    const url = new URL(req.url, `http://${req.headers.host}`);

    if (req.method === 'HEAD' && /^\/api\/Camera\/[^/]+\/status$/.test(url.pathname)) {
      res.writeHead(200, { 'Access-Control-Allow-Origin': '*' });
      res.end();
      return;
    }

    if (url.pathname === '/api/PlatformData/stream') {
      sseHeaders(res);
      sseClients.add(res);
      const t = setInterval(() => sseSend(res, { weight: weightNext() }), WEIGHT_TICK_MS);
      timers.add(t);
      req.on('close', () => { clearInterval(t); timers.delete(t); sseClients.delete(res); });
      return;
    }

    if (url.pathname === '/api/plates/stream') {
      sseHeaders(res);
      sseClients.add(res);
      const t = setInterval(() => {
        sseSend(res, {
          cameraId: 'npr1',
          plateNumber: PLATES[Math.floor(Math.random() * PLATES.length)],
          confidence: 0.9 + Math.random() * 0.09,
          timestamp: new Date().toISOString(),
        });
      }, PLATE_EVERY_MS);
      timers.add(t);
      req.on('close', () => { clearInterval(t); timers.delete(t); sseClients.delete(res); });
      return;
    }

    if (/^\/api\/Camera\/[^/]+\/snapshot$/.test(url.pathname)) {
      res.writeHead(200, {
        'Content-Type': 'image/jpeg',
        'Content-Length': FRAME.length,
        'Access-Control-Allow-Origin': '*',
      });
      res.end(FRAME);
      return;
    }

    if (/^\/api\/Camera\/[^/]+\/stream$/.test(url.pathname)) {
      const boundary = 'qalitrackframe';
      res.writeHead(200, {
        'Content-Type': `multipart/x-mixed-replace; boundary=${boundary}`,
        'Access-Control-Allow-Origin': '*',
        Connection: 'keep-alive',
        'Cache-Control': 'no-cache',
      });
      const writeFrame = () => {
        res.write(`--${boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: ${FRAME.length}\r\n\r\n`);
        res.write(FRAME);
        res.write('\r\n');
      };
      writeFrame();
      const t = setInterval(writeFrame, CAMERA_FPS_MS);
      timers.add(t);
      req.on('close', () => { clearInterval(t); timers.delete(t); });
      return;
    }

    res.writeHead(404, { 'Access-Control-Allow-Origin': '*' });
    res.end('not found');
  });

  return new Promise((resolve, reject) => {
    server.on('error', reject);
    server.listen(port, () => {
      resolve({
        server,
        stats: () => ({ activeTimers: timers.size, activeSseClients: sseClients.size }),
        stop: () => new Promise((r) => {
          timers.forEach(clearInterval);
          sseClients.forEach((res) => res.end());
          server.close(() => r());
        }),
      });
    });
  });
}

// Allow running standalone: `node stress-tests/mock-hardware-server.mjs`
// (a plain `file://${process.argv[1]}` comparison breaks on Windows, where
// argv[1] is a bare "C:\..." path — pathToFileURL() normalizes it correctly.)
if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  const { server } = await start(5000);
  console.log('Mock hardware server listening on http://localhost:5000');
  process.on('SIGINT', () => server.close(() => process.exit(0)));
}
