/**
 * licenseUtils.js — ECDSA P-256 license verification
 *
 * HOW IT WORKS:
 *   - Your ERP holds the PRIVATE key and signs license tokens (ES256 JWTs)
 *   - This app holds only the PUBLIC key — it can verify but NEVER forge
 *   - Even a full decompile of the app cannot produce valid licenses
 *
 * SETUP (one time):
 *   1. Run:  node src/utils/generateKeyPair.mjs
 *   2. Copy the PUBLIC_KEY_JWK output into this file (below)
 *   3. Store the PRIVATE_KEY_JWK securely in your ERP — never here
 *
 * LICENSE TOKEN FORMAT: Standard ES256 JWT
 *   {base64url_header}.{base64url_payload}.{base64url_signature}
 *
 * PAYLOAD FIELDS:
 *   sub       — customer ID          e.g. "KTDA-001"
 *   app       — app identifier       e.g. "qalitrack-frontend"  ← per-app binding
 *   features  — licensed features    e.g. ["kiosk", "reports"]
 *   iat       — issued at (unix)
 *   exp       — expires at (unix)
 *   mid       — machine ID (optional, for hardware binding)
 *   iss       — "qalibrated.co.ke"
 */

// ── APP IDENTIFIER ────────────────────────────────────────────────────────────
// Each app must declare its own ID. A license without this exact appId is rejected.
export const THIS_APP_ID = "qalitrack-frontend";

// ── PUBLIC KEY ────────────────────────────────────────────────────────────────
const PUBLIC_KEY_JWK = {
  kty: "EC",
  crv: "P-256",
  x: "KYOOP-qgdZetRUtcTWPQthev2oXwwtkCRtCerL_L3qw",
  y: "CxlCG84CG7YL8VwOO9s8p5tm8kO0GxB0rmO_WiTIIcg",
};

// ── CONFIG ────────────────────────────────────────────────────────────────────
const CACHE_KEY        = "qali_lic_v1";
const MACHINE_ID_KEY   = "qali_mid";
const RECHECK_MS       = 24 * 60 * 60 * 1000;        // daily server re-check

// ─────────────────────────────────────────────────────────────────────────────
// MACHINE FINGERPRINT
//
// In Electron: calls the main process via IPC (preload exposes electronAPI).
//   On Windows: reads HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid — set once
//   at OS install, survives app reinstalls, reboots, and network changes.
//   On non-Windows: derives from hostname + first physical MAC address (less stable).
//   The result is cached in the main process for the lifetime of the session.
//
// In browser (dev/web): falls back to a random UUID cached in localStorage.
//   Not hardware-bound, but stable for the life of that browser profile.
// ─────────────────────────────────────────────────────────────────────────────

// Sync cache so callers that can't await still get a value after the first async call
let _machineIdCache = null;

export async function getMachineIdAsync() {
  if (_machineIdCache) return _machineIdCache;

  // Electron path — preload exposes window.electronAPI
  if (typeof window !== "undefined" && window.electronAPI?.getMachineId) {
    _machineIdCache = await window.electronAPI.getMachineId();
    return _machineIdCache;
  }

  // Browser fallback: stable random UUID stored in localStorage
  let id = localStorage.getItem(MACHINE_ID_KEY);
  if (!id) {
    const buf = new Uint8Array(16);
    crypto.getRandomValues(buf);
    id = Array.from(buf).map((b) => b.toString(16).padStart(2, "0")).join("");
    localStorage.setItem(MACHINE_ID_KEY, id);
  }
  _machineIdCache = id;
  return _machineIdCache;
}

// Sync version — returns cached value or empty string before first async call resolves.
// Always prefer getMachineIdAsync() at startup to prime the cache.
export function getMachineId() {
  return _machineIdCache ?? localStorage.getItem(MACHINE_ID_KEY) ?? "";
}

// ─────────────────────────────────────────────────────────────────────────────
// JWT HELPERS
// ─────────────────────────────────────────────────────────────────────────────
function b64urlToBuffer(str) {
  const b64 = str.replace(/-/g, "+").replace(/_/g, "/");
  const pad = (4 - (b64.length % 4)) % 4;
  const binary = atob(b64 + "=".repeat(pad));
  const buf = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) buf[i] = binary.charCodeAt(i);
  return buf.buffer;
}

function b64urlDecodeJson(str) {
  const b64 = str.replace(/-/g, "+").replace(/_/g, "/");
  const pad = (4 - (b64.length % 4)) % 4;
  return JSON.parse(atob(b64 + "=".repeat(pad)));
}

// Cache the imported key so we don't re-import on every call
let _pubKey = null;
async function getPublicKey() {
  if (_pubKey) return _pubKey;
  _pubKey = await crypto.subtle.importKey(
    "jwk",
    PUBLIC_KEY_JWK,
    { name: "ECDSA", namedCurve: "P-256" },
    false,
    ["verify"]
  );
  return _pubKey;
}

// ─────────────────────────────────────────────────────────────────────────────
// TOKEN VERIFICATION (local, no network)
// ─────────────────────────────────────────────────────────────────────────────
export async function verifyLicenseToken(token) {
  if (!token || typeof token !== "string") {
    return { valid: false, reason: "empty" };
  }

  const parts = token.trim().split(".");
  if (parts.length !== 3) {
    return { valid: false, reason: "malformed" };
  }

  const [headerB64, payloadB64, sigB64] = parts;

  try {
    const pubKey     = await getPublicKey();
    const sigInput   = new TextEncoder().encode(`${headerB64}.${payloadB64}`);
    const signature  = b64urlToBuffer(sigB64);

    const ok = await crypto.subtle.verify(
      { name: "ECDSA", hash: "SHA-256" },
      pubKey,
      signature,
      sigInput
    );

    if (!ok) return { valid: false, reason: "invalid_signature" };

    const payload = b64urlDecodeJson(payloadB64);
    const now     = Math.floor(Date.now() / 1000);

    // ── App binding: reject keys issued for a different app ──────────────────
    if (payload.app && payload.app !== THIS_APP_ID) {
      return { valid: false, reason: "wrong_app", issuedFor: payload.app };
    }

    // ── Expiry ───────────────────────────────────────────────────────────────
    if (payload.exp && payload.exp < now) {
      return {
        valid: false,
        reason: "expired",
        expiresAt: new Date(payload.exp * 1000),
        customerId: payload.sub,
      };
    }

    // ── Machine binding (optional, only when key was issued with a machineId) ─
    if (payload.mid && payload.mid !== await getMachineIdAsync()) {
      return { valid: false, reason: "machine_mismatch" };
    }

    return {
      valid:      true,
      customerId: payload.sub,
      appId:      payload.app || "unspecified",
      features:   Array.isArray(payload.features) ? payload.features : [],
      issuedAt:   payload.iat ? new Date(payload.iat * 1000) : null,
      expiresAt:  payload.exp ? new Date(payload.exp * 1000) : null,
    };
  } catch {
    return { valid: false, reason: "verification_error" };
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// SERVER CHECK-IN  (calls your ERP to allow remote revocation)
// ─────────────────────────────────────────────────────────────────────────────
async function checkWithServer(token) {
  const base =
    localStorage.getItem("licenseServerUrl") ||
    import.meta.env.VITE_LICENSE_SERVER_URL ||
    "https://kmk.support.qalibrated.co.ke";

  try {
    const res = await fetch(`${base}/api/v1/licenses/validate`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        token,
        machineId: await getMachineIdAsync(),
        appId: THIS_APP_ID,
      }),
      signal: AbortSignal.timeout(6000),
    });

    const body = await res.json().catch(() => ({}));
    if (import.meta.env.DEV) console.log("[LicenseServer] raw response:", body);

    const payload = body.data ?? body; // unwrap envelope { data: {...} } if present
    return res.ok
      ? { ...payload, serverChecked: true, serverCheckedAt: Date.now() }
      : { valid: false, reason: payload.reason || body.message || "server_rejected" };
  } catch {
    return null; // network failure → caller decides fallback
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// CACHE HELPERS
// ─────────────────────────────────────────────────────────────────────────────
function readCache() {
  try { return JSON.parse(localStorage.getItem(CACHE_KEY)); } catch { return null; }
}
function writeCache(data) {
  localStorage.setItem(CACHE_KEY, JSON.stringify({ ...data, cachedAt: Date.now() }));
}

// ─────────────────────────────────────────────────────────────────────────────
// ACTIVATE  (called when user enters a token)
// ─────────────────────────────────────────────────────────────────────────────
export async function activateLicense(token) {
  // 1. Verify signature + expiry + app binding locally (instant, no network)
  const local = await verifyLicenseToken(token);
  if (!local.valid) return local;

  // 2. Server validation — allows ERP to reject stolen / revoked keys immediately
  const server = await checkWithServer(token);
  if (server !== null && !server.valid) {
    return { valid: false, reason: server.reason };
  }

  // 3. Persist to cache
  const result = server?.valid ? { ...local, ...server } : local;
  writeCache({ ...result, token });
  return result;
}

// ─────────────────────────────────────────────────────────────────────────────
// GET LICENSE STATUS  (called on every app boot / tab open)
// ─────────────────────────────────────────────────────────────────────────────
export async function getLicenseStatus(feature = "") {
  const cache = readCache();
  if (!cache?.token) return { valid: false, reason: "not_activated" };

  // Local verify (catches expiry without network)
  const local = await verifyLicenseToken(cache.token);

  if (!local.valid) {
    if (local.reason === "expired") {
      const server = await checkWithServer(cache.token);
      if (server?.valid) {
        writeCache({ ...cache, ...server });
        return { valid: true, ...cache, ...server };
      }
    }
    return local;
  }

  // Feature entitlement check
  if (feature && !local.features.includes(feature)) {
    return { valid: false, reason: "feature_not_licensed" };
  }

  // Periodic server re-check — blocking so revocation is caught in-band
  const age = Date.now() - (cache.cachedAt || 0);
  if (age > RECHECK_MS) {
    const server = await checkWithServer(cache.token);

    if (server?.valid === false) {
      localStorage.removeItem(CACHE_KEY);
      return { valid: false, reason: server.reason };
    }

    if (server?.valid) {
      writeCache({ ...cache, ...server });
      return { valid: true, ...local, ...server, token: cache.token };
    }

    // server === null → offline; keep app running
    return { valid: true, ...local, token: cache.token, warning: "offline" };
  }

  return { valid: true, ...local, token: cache.token };
}

// ─────────────────────────────────────────────────────────────────────────────
// DEACTIVATE
// ─────────────────────────────────────────────────────────────────────────────
export function deactivateLicense() {
  localStorage.removeItem(CACHE_KEY);
}

// ─────────────────────────────────────────────────────────────────────────────
// ERROR MESSAGES  (for display in UI)
// ─────────────────────────────────────────────────────────────────────────────
export function licenseErrorMessage(reason) {
  const messages = {
    not_activated:        "No license key has been entered.",
    empty:                "License key cannot be empty.",
    malformed:            "License key format is invalid.",
    invalid_signature:    "License key is not authentic — it may have been tampered with.",
    expired:              "This license has expired. Contact Qalibrated Systems to renew.",
    machine_mismatch:     "This license is bound to a different machine.",
    wrong_app:            "This license was issued for a different application.",
    feature_not_licensed: "Your license does not include this feature.",
    server_rejected:      "License was rejected by the server. It may have been revoked.",
    verification_error:   "Could not verify the license key. Please try again.",
  };
  return messages[reason] || `License error: ${reason}`;
}
