import React, { useEffect, useRef, useState } from "react";
import { useDispatch } from "react-redux";
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";
import { Tag } from "antd";
import LiveWeighbridgeStatus from "./LiveWeighbridgeStatus";
import CameraGrid from "../CameraGrid";
import { useLicenseFeature } from "../../hooks/useLicenseFeature";
import { LicenseFeatures } from "../../utils/LicenseFeatures";
import { useHardwareConfig } from "../../hooks/useHardwareConfig";
import { fetchTransactions } from "../../store/weighingSlice";

dayjs.extend(relativeTime);

// Same physical scale/camera the unmanned kiosk itself reads from
// (hwConfig.scaleStreamUrl/anprStreamUrl) — this panel only ever watches,
// it never drives a transaction, since the kiosk is the one actually doing that.

// Lightweight connectivity probe for a raw SSE endpoint (RFID/NFC) — the kiosk's
// own screens open a real EventSource and drive a full UI off it; here we only
// need "is something listening on this stream right now", so the connection is
// torn down again on unmount without ever handling individual messages.
function useStreamHealth(url) {
  const [connected, setConnected] = useState(false);
  useEffect(() => {
    if (!url) { setConnected(false); return; }
    setConnected(false);
    let es;
    try {
      es = new EventSource(url);
      es.onopen = () => setConnected(true);
      es.onmessage = () => setConnected(true);
      es.onerror = () => setConnected(false);
    } catch {
      setConnected(false);
    }
    return () => es?.close();
  }, [url]);
  return connected;
}

function HealthDot({ label, connected }) {
  return (
    <div className="flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[10px] font-bold border bg-white">
      <span className={`w-1.5 h-1.5 rounded-full ${connected ? "bg-green-500" : "bg-red-500"}`} />
      <span className="text-gray-600">{label}</span>
      <span className={connected ? "text-green-600" : "text-red-500"}>{connected ? "OK" : "Down"}</span>
    </div>
  );
}

export default function UnmannedMonitorPanel() {
  const dispatch = useDispatch();
  const anprLicensed = useLicenseFeature(LicenseFeatures.ANPR);
  const hwConfig = useHardwareConfig();

  const rfidOk = useStreamHealth(hwConfig.rfidStreamUrl);
  const nfcOk  = useStreamHealth(hwConfig.nfcStreamUrl);

  const [pending, setPending] = useState([]);
  const [recentComplete, setRecentComplete] = useState([]);
  const [loading, setLoading] = useState(false);
  const pollRef = useRef(null);

  const loadUnmannedActivity = async () => {
    setLoading(true);
    try {
      // weighMode="Kiosk" is what WeighingScreen.jsx (the unmanned kiosk) tags every
      // transaction it creates with — this filters to unmanned-originated activity only.
      const [pendingRes, completeRes] = await Promise.all([
        dispatch(fetchTransactions({ weighMode: "Kiosk", isCompleted: false, pageNumber: 1, pageSize: 20 })).unwrap(),
        dispatch(fetchTransactions({ weighMode: "Kiosk", isCompleted: true, pageNumber: 1, pageSize: 10 })).unwrap(),
      ]);
      setPending(pendingRes?.items ?? []);
      setRecentComplete(completeRes?.items ?? []);
    } catch {
      // advisory monitoring view only — a failed poll shouldn't break the panel
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUnmannedActivity();
    pollRef.current = setInterval(loadUnmannedActivity, 15_000);
    return () => clearInterval(pollRef.current);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="flex flex-col h-full gap-2 overflow-hidden">
      {/* Kiosk hardware health strip */}
      <div className="flex items-center gap-2 shrink-0 px-1">
        <span className="text-[10px] font-bold text-gray-400 uppercase tracking-wide mr-1">Kiosk Hardware:</span>
        <HealthDot label="RFID" connected={rfidOk} />
        <HealthDot label="NFC" connected={nfcOk} />
        {loading && <span className="text-[10px] text-gray-400 ml-auto">Refreshing…</span>}
      </div>

      <div className="flex gap-2 h-[55%] shrink-0">
        <div className="w-1/2 h-full">
          <LiveWeighbridgeStatus readOnly />
        </div>
        <div className="w-1/2 h-full bg-black rounded overflow-hidden">
          {anprLicensed ? <CameraGrid type="live" /> : <AnprLockedTile />}
        </div>
      </div>

      <div className="flex-1 min-h-0 flex gap-2">
        <UnmannedList title="Pending 2nd Weight" rows={pending} emptyLabel="No unmanned transactions waiting" />
        <UnmannedList title="Recently Completed" rows={recentComplete} emptyLabel="No completed unmanned transactions yet" />
      </div>
    </div>
  );
}

function UnmannedList({ title, rows, emptyLabel }) {
  return (
    <div className="flex-1 min-h-0 flex flex-col rounded-lg border border-gray-200 bg-white shadow-sm overflow-hidden">
      <div className="px-3 py-1.5 border-b border-gray-100 bg-gray-50 shrink-0">
        <span className="text-[10px] font-bold uppercase tracking-wide text-gray-500">{title}</span>
      </div>
      <div className="flex-1 overflow-auto">
        {rows.length === 0 ? (
          <div className="h-full flex items-center justify-center text-[11px] text-gray-400 p-4 text-center">
            {emptyLabel}
          </div>
        ) : (
          <div className="divide-y divide-gray-50">
            {rows.map((r) => (
              <div key={r.id || r.ticketID} className="px-3 py-2 flex items-center justify-between gap-2 text-xs">
                <div className="min-w-0">
                  <div className="font-bold text-gray-900 truncate">{r.noPlate || "—"}</div>
                  <div className="text-[10px] text-gray-400 truncate">
                    {r.driverName || "No driver"} · {r.createdAt ? dayjs(r.createdAt).fromNow() : "—"}
                  </div>
                </div>
                <Tag color={r.secondWeight ? "green" : "orange"} className="shrink-0 text-[9px]">
                  {r.secondWeight ? `${r.firstWeight}→${r.secondWeight} kg` : `${r.firstWeight || 0} kg`}
                </Tag>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

function AnprLockedTile() {
  return (
    <div className="w-full h-full flex flex-col items-center justify-center gap-2 bg-gray-950 text-gray-600">
      <p className="text-[10px] font-medium">Live Feed</p>
      <p className="text-[9px] text-gray-700">ANPR not licensed</p>
    </div>
  );
}
