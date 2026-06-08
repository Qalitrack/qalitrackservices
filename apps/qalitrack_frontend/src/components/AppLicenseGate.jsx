import { useState, useEffect, useCallback } from "react";
import {
  getLicenseStatus,
  activateLicense,
  deactivateLicense,
  getMachineId,
  getMachineIdAsync,
  licenseErrorMessage,
} from "../utils/licenseUtils";

// ─────────────────────────────────────────────────────────────────────────────
// FULL-APP LICENSE GATE
//
// Wraps the entire app. On first launch (or after revoke/expiry) the app is
// completely locked — only this screen is shown. Once a valid license is
// activated it renders children normally.
//
// Machine ID is shown prominently so the customer can copy it and send it
// to Qalibrated Systems when requesting a machine-bound license.
// ─────────────────────────────────────────────────────────────────────────────
export default function AppLicenseGate({ children }) {
  const [status, setStatus]       = useState(null);   // null = still checking
  const [tokenInput, setToken]    = useState("");
  const [error, setError]         = useState(null);
  const [activating, setActing]   = useState(false);
  const [machineId, setMachineId] = useState(() => getMachineId()); // sync cache first
  const [copiedMid, setCopiedMid] = useState(false);

  // Prime machine ID from main process (Electron IPC) then check license
  useEffect(() => {
    getMachineIdAsync().then(id => {
      setMachineId(id);
      // Pass no feature — we just need the app licensed.
      // Individual modules gate their own features (e.g. KioskLicenseGate).
      return getLicenseStatus("");
    }).then(setStatus);
  }, []);

  // Live expiry check — runs every 60 seconds while the app is open.
  // Locks the screen the moment the license expires mid-session.
  useEffect(() => {
    if (!status?.valid) return;
    const timer = setInterval(() => {
      getLicenseStatus("").then(result => {
        if (!result.valid) setStatus(result);
      });
    }, 60_000);
    return () => clearInterval(timer);
  }, [status?.valid]);

  const handleActivate = useCallback(async () => {
    setActing(true);
    setError(null);
    const result = await activateLicense(tokenInput.trim());
    setActing(false);
    if (result.valid) {
      setStatus(result);
    } else {
      setError(licenseErrorMessage(result.reason));
    }
  }, [tokenInput]);

  const handleKeyDown = useCallback((e) => {
    if (e.key === "Enter" && !e.shiftKey && tokenInput.trim()) handleActivate();
  }, [handleActivate, tokenInput]);

  const copyMachineId = () => {
    navigator.clipboard.writeText(machineId).then(() => {
      setCopiedMid(true);
      setTimeout(() => setCopiedMid(false), 2000);
    });
  };

  // ── Still loading cached license ──────────────────────────────────────────
  if (status === null) {
    return (
      <div className="fixed inset-0 bg-gray-950 flex items-center justify-center">
        <div className="flex flex-col items-center gap-3">
          <div className="w-10 h-10 border-4 border-amber-500 border-t-transparent rounded-full animate-spin" />
          <p className="text-sm text-gray-400">Checking license…</p>
        </div>
      </div>
    );
  }

  // ── Valid license — render the app ────────────────────────────────────────
  if (status.valid) {
    return children;
  }

  // ── Locked — show full-screen gate ────────────────────────────────────────
  const isExpired         = status.reason === "expired";
  const isMachineMismatch = status.reason === "machine_mismatch";
  const isRevoked         = status.reason === "revoked";

  return (
    <div className="fixed inset-0 bg-gray-950 flex flex-col items-center justify-center p-4 overflow-auto">

      {/* Card */}
      <div className="w-full max-w-md bg-gray-900 rounded-2xl shadow-2xl border border-gray-800 overflow-hidden">

        {/* Header strip */}
        <div className="bg-gradient-to-r from-amber-500 to-orange-600 px-6 py-5 flex items-center gap-4">
          <div className="w-12 h-12 rounded-xl bg-white/20 flex items-center justify-center shrink-0">
            <LockIcon className="w-6 h-6 text-white" />
          </div>
          <div>
            <p className="text-xs text-amber-100 font-medium tracking-wide uppercase">
              Qalibrated Systems
            </p>
            <h1 className="text-xl font-black text-white leading-tight">
              QaliTrack
            </h1>
          </div>
        </div>

        <div className="p-6 space-y-5">

          {/* Status message */}
          {isExpired && (
            <StatusBanner type="warning">
              Your license has expired. Contact Qalibrated Systems to renew.
            </StatusBanner>
          )}
          {isRevoked && (
            <StatusBanner type="error">
              This license has been revoked. Contact Qalibrated Systems.
            </StatusBanner>
          )}
          {isMachineMismatch && (
            <StatusBanner type="error">
              This license is bound to a different machine. You need a license issued for this Machine ID.
            </StatusBanner>
          )}
          {!isExpired && !isRevoked && !isMachineMismatch && (
            <p className="text-sm text-gray-400 text-center">
              Enter your license token to unlock the application.
            </p>
          )}

          {/* Machine ID — shown always so the user knows what to give us */}
          <div className="rounded-xl bg-gray-800 border border-gray-700 p-4">
            <div className="flex items-center justify-between mb-1.5">
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide">
                Your Machine ID
              </p>
              <span className="text-[10px] text-amber-400 font-medium">
                Hardware binding
              </span>
            </div>
            <div className="flex items-center gap-2">
              <code className="flex-1 text-xs font-mono text-amber-300 break-all leading-relaxed">
                {machineId}
              </code>
              <button
                onClick={copyMachineId}
                title="Copy Machine ID"
                className="shrink-0 px-2.5 py-1.5 rounded-lg bg-gray-700 hover:bg-gray-600 text-xs text-gray-300 transition font-medium"
              >
                {copiedMid ? "✓ Copied" : "Copy"}
              </button>
            </div>
            <p className="text-[11px] text-gray-500 mt-2">
              Share this ID with Qalibrated Systems to get a license locked to this machine.
              Leave it out of your request for a floating (any-machine) license.
            </p>
          </div>

          {/* Token input */}
          <div className="space-y-2">
            <label className="block text-xs font-semibold text-gray-400 uppercase tracking-wide">
              License Token
            </label>
            <textarea
              rows={4}
              value={tokenInput}
              onChange={(e) => { setToken(e.target.value); setError(null); }}
              onKeyDown={handleKeyDown}
              placeholder="Paste your license token here…  (eyJhbGci…)"
              className={`w-full px-3 py-2.5 rounded-lg border font-mono text-xs outline-none resize-none transition bg-gray-800 text-gray-100
                ${error
                  ? "border-red-500 focus:border-red-400"
                  : "border-gray-700 focus:border-amber-500"
                }`}
            />
            {error && (
              <p className="text-xs text-red-400 flex items-start gap-1.5">
                <LockIcon className="w-3 h-3 mt-0.5 shrink-0" /> {error}
              </p>
            )}
          </div>

          {/* Activate button */}
          <button
            onClick={handleActivate}
            disabled={activating || !tokenInput.trim()}
            className={`w-full py-3 rounded-xl text-white text-sm font-bold flex items-center justify-center gap-2 transition
              ${activating || !tokenInput.trim()
                ? "bg-gray-700 cursor-not-allowed text-gray-500"
                : "bg-gradient-to-r from-amber-500 to-orange-600 hover:opacity-90 shadow-lg shadow-amber-900/30"
              }`}
          >
            {activating
              ? <><Spinner /> Verifying…</>
              : <><ShieldIcon className="w-4 h-4" /> Activate License</>
            }
          </button>

          {/* Contact line */}
          <p className="text-xs text-gray-600 text-center">
            Need a license?{" "}
            <span className="text-amber-500 font-medium">info@qalibrated.co.ke</span>
          </p>

        </div>
      </div>

      {/* Version footer */}
      <p className="mt-6 text-[11px] text-gray-700">
        QaliTrack · Qalibrated Systems Ltd
      </p>
    </div>
  );
}

// ─────────────────────────────────────────────────────────────────────────────
// Small helpers kept inline — no need for separate files
// ─────────────────────────────────────────────────────────────────────────────
function StatusBanner({ type, children }) {
  const styles = {
    warning: "bg-orange-950 border-orange-800 text-orange-300",
    error:   "bg-red-950 border-red-800 text-red-300",
  };
  return (
    <div className={`rounded-lg border px-4 py-2.5 text-xs font-medium ${styles[type]}`}>
      {children}
    </div>
  );
}

function Spinner() {
  return <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />;
}

function LockIcon({ className }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
    </svg>
  );
}

function ShieldIcon({ className }) {
  return (
    <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
      <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
    </svg>
  );
}
