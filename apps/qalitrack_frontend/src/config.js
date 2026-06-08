// src/config.js  (or directly in your hook / component)

export const HARDWARE_BASE_URL = import.meta.env.VITE_HARDWARE_BASE_URL;

export const CAMERA_CONFIG = {
  cameraId: "npr1",
  streamUrl:    `${HARDWARE_BASE_URL}/api/Camera/npr1/stream`,
  snapshotUrl:  `${HARDWARE_BASE_URL}/api/Camera/npr1/snapshot`,
  platesUrl:    `${HARDWARE_BASE_URL}/api/plates/stream`,
};