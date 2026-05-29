/**
 * usePendingTransaction.js
 *
 * Given a vehicle registration number (plate), queries the Transaction API for
 * any existing "Incomplete" / first-weighing-only transaction for that vehicle.
 *
 * Kiosk flow:
 *   • First visit  — no pending txn → normal first-weight capture
 *   • Second visit — pending txn found → skip to second-weight capture (tare / exit)
 *
 * The hook is intentionally lazy: call `checkPending(plate)` manually (not on
 * every render) so you control exactly when the lookup fires.
 *
 * Returns:
 *   {
 *     checking:      boolean,          // network request in flight
 *     pendingTxn:    object | null,    // full transaction object if found
 *     checkPending:  (plate) => void,  // trigger the lookup
 *     clearPending:  () => void,       // reset after handling
 *   }
 */

import { useState, useCallback } from "react";

const BASE_URL = import.meta.env.VITE_API_URL || "/api";

// ── Auth token (same helper pattern as WeighingScreen.jsx) ───────────────────
function getToken() {
  try {
    const s1 = sessionStorage.getItem("authSession");
    if (s1) { const p = JSON.parse(s1); const t = p?.token ?? p?.accessToken ?? p?.access_token ?? p?.userData?.token; if (t) return t; }
    const s2 = sessionStorage.getItem("user");
    if (s2) { const p = JSON.parse(s2); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
    const l1 = localStorage.getItem("token");      if (l1) return l1;
    const l2 = localStorage.getItem("authToken");  if (l2) return l2;
    const l3 = localStorage.getItem("authSession");
    if (l3) { const p = JSON.parse(l3); const t = p?.token ?? p?.accessToken ?? p?.access_token; if (t) return t; }
  } catch {}
  return null;
}

async function fetchTransactions(plate) {
  const token = getToken();
  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  // Try primary endpoint — search by plate, return recent 20 records
  const url = `${BASE_URL}/Transaction/Transaction/Transaction?noPlate=${encodeURIComponent(plate)}&pageSize=20&sortBy=createdAt&sortDir=desc`;
  const res  = await fetch(url, { headers });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

// ── Extract items from any API wrapper shape ──────────────────────────────────
function extractItems(raw) {
  return (
    raw?.items              ??
    raw?.data?.items        ??
    raw?.data?.data?.items  ??
    raw?.data?.data         ??
    raw?.data               ??
    (Array.isArray(raw) ? raw : [])
  );
}

// ── Determine if a transaction is "pending" (first weight only) ───────────────
// Adjust the field names to match your actual API response shape.
function isPending(txn) {
  if (!txn) return false;

  // Explicitly incomplete
  const status = (txn.status ?? txn.transactionStatus ?? "").toLowerCase();
  if (status === "incomplete" || status === "pending" || status === "first weight") return true;
  if (status === "complete"   || status === "completed") return false;

  // Has firstWeight but no secondWeight
  const first  = Number(txn.firstWeight  ?? txn.grossWeight ?? 0);
  const second = Number(txn.secondWeight ?? txn.tareWeight  ?? txn.netWeight ?? 0);
  if (first > 0 && second === 0) return true;

  // isCompleted flag
  if (txn.isCompleted === false) return true;

  return false;
}

export function usePendingTransaction() {
  const [checking,   setChecking]   = useState(false);
  const [pendingTxn, setPendingTxn] = useState(null);
  const [checkError, setCheckError] = useState(null);

  const checkPending = useCallback(async (plate) => {
    if (!plate?.trim()) return;
    setChecking(true);
    setCheckError(null);
    setPendingTxn(null);

    try {
      const raw   = await fetchTransactions(plate.trim().toUpperCase());
      const items = extractItems(raw);


      const pending = items.find(isPending);
      if (pending) {
        setPendingTxn(pending);
      } else {
        setPendingTxn(null);
      }
    } catch (err) {
      setCheckError(err.message);
      setPendingTxn(null); // fail open — don't block kiosk
    } finally {
      setChecking(false);
    }
  }, []);

  const clearPending = useCallback(() => setPendingTxn(null), []);

  return { checking, pendingTxn, checkError, checkPending, clearPending };
}

export default usePendingTransaction;