// src/config.js  (or directly in your hook / component)

export const CAMERA_CONFIG = {
  cameraId: "npr1",

  // These three are CONFIRMED WORKING on your local network
  streamUrl:    "http://172.16.0.215:5000/api/Camera/npr1/stream",        // MJPEG live stream
  snapshotUrl:  "http://172.16.0.215:5000/api/Camera/npr1/snapshot",     // Single fresh JPEG
  platesUrl:    "http://172.16.0.215:5000/api/PlatformData/plates/stream", // Real-time plate JSON
};