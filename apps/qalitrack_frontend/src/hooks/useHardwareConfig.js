/**
 * useHardwareConfig.js
 *
 * Reads hardware stream URLs (and other settings) directly from the
 * "systemSettings" localStorage key written by SystemSettings.jsx on Save.
 *
 * Falls back to the hard-coded defaults that were previously baked into each
 * component, so nothing breaks if the user has never opened System Settings.
 *
 * Usage:
 *   const { rfidStreamUrl, nfcStreamUrl, scaleStreamUrl } = useHardwareConfig();
 *
 * The hook re-reads from localStorage on every render AND subscribes to the
 * "storage" event so sibling tabs / windows stay in sync automatically.
 */

import { useState, useEffect, useCallback } from "react";

// ── Defaults (match SystemSettings.jsx DEFAULT_SETTINGS) ─────────────────────
const DEFAULTS = {
  rfidStreamUrl:            "http://localhost:5000/api/RFID/stream",
  nfcStreamUrl:             "http://localhost:5000/api/NFC/stream",
  scaleStreamUrl:           "http://localhost:5000/api/PlatformData/stream",
  anprStreamUrl:            "http://localhost:5000/api/Camera/npr1/stream",
  rfidEnabled:              true,
  nfcEnabled:               true,
  scaleEnabled:             true,
  rfidReaderType:           "UHF Reader",
  nfcReaderType:            "MIFARE Classic",
  scaleBrand:               "Avery Weigh-Tronix",
  scaleCapacity:            60000,
  scaleStabilityThreshold:  5,
  companyName:              "QALIBRATED SYSTEMS LTD",
  companyLogo:              null,
  timezone:                 "Africa/Nairobi",
  currency:                 "KES",
  dateFormat:               "DD/MM/YYYY",
  timeFormat:               "24h",
};

const LS_KEY = "systemSettings";

function readFromStorage() {
  try {
    const raw = localStorage.getItem(LS_KEY);
    if (!raw) return { ...DEFAULTS };
    const parsed = JSON.parse(raw);
    // Merge — stored values win, but missing keys fall back to DEFAULTS
    return { ...DEFAULTS, ...parsed };
  } catch {
    return { ...DEFAULTS };
  }
}

export function useHardwareConfig() {
  const [config, setConfig] = useState(readFromStorage);

  // Re-read when another tab saves settings
  const refresh = useCallback(() => setConfig(readFromStorage()), []);

  useEffect(() => {
    window.addEventListener("storage", refresh);
    return () => window.removeEventListener("storage", refresh);
  }, [refresh]);

  return config;
}

/**
 * Non-hook helper for class components or plain JS usage.
 * Returns a snapshot (not reactive).
 */
export function getHardwareConfig() {
  return readFromStorage();
}

export default useHardwareConfig;