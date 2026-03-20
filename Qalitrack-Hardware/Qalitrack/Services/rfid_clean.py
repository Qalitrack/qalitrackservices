#!/usr/bin/env python3
"""
RFID Reader Client — CP-202910 / UHF Prime Reader  [v2]
Protocol reverse-engineered from live pcap capture.

Confirmed handshake sequence (byte-exact from pcap):
  1. TCP connect  →  172.16.0.117:2022
  2. Reader       →  00 00                             (2B connection ack)
  3. Client       →  CF FF 00 70 00 24 15              (INIT, 7B)
  4. Reader       →  CF 00 00 70 99 00 [...] CRC       (banner, 160B)
  5. Client       →  CF FF 00 72 00 17 A5              (GET CONFIG, 7B)
  6. Reader       →  CF 00 00 72 1A 00 [...] CRC       (config, 33B)
  7. Reader streams tag frames continuously:
       CF 00 00 01 0E 00 FD [RSSI] [CNT] [EPC x12] [CRC x2]  = 21B each
"""

import socket
import struct
import time
import csv
import os
from datetime import datetime

# ── Config ────────────────────────────────────────────────────────────────────
HOST    = "172.16.0.117"
PORT    = 2022
TIMEOUT = 30        # seconds to wait for a tag before printing idle
LOG_CSV = "rfid_log.csv"   # set to None to disable CSV logging
# ─────────────────────────────────────────────────────────────────────────────

# Exact bytes verified from pcap
CMD_INIT       = bytes([0xCF, 0xFF, 0x00, 0x70, 0x00, 0x24, 0x15])
CMD_GET_CONFIG = bytes([0xCF, 0xFF, 0x00, 0x72, 0x00, 0x17, 0xA5])


def calc_crc16(data: bytes) -> int:
    """CRC-16/MCRF4XX — verified against all captured frames."""
    crc = 0xFFFF
    for byte in data:
        crc ^= byte
        for _ in range(8):
            crc = (crc >> 1) ^ 0x8408 if (crc & 1) else (crc >> 1)
    return crc


def recv_n(sock, n, timeout=5.0):
    """Receive exactly n bytes within timeout."""
    buf = b""
    deadline = time.time() + timeout
    while len(buf) < n:
        sock.settimeout(max(0.1, deadline - time.time()))
        try:
            chunk = sock.recv(n - len(buf))
            if not chunk:
                break
            buf += chunk
        except socket.timeout:
            break
    return buf


def recv_frame(sock, timeout=5.0):
    """
    Read one complete CF frame from the socket.
    Frame layout: CF [ADDR] [CMD_HI] [CMD_LO] [LEN] [DATA x LEN] [CRC x2]
    Skips bytes until 0xCF start marker is found.
    Returns raw bytes of the complete frame, or b'' on timeout/disconnect.
    """
    deadline = time.time() + timeout

    # Read up to CF start marker + 4 header bytes = 5 bytes total
    hdr = b""
    while len(hdr) < 5:
        remaining = deadline - time.time()
        if remaining <= 0:
            return b""
        sock.settimeout(max(0.05, remaining))
        try:
            byte = sock.recv(1)
        except socket.timeout:
            return b""
        if not byte:
            return b""
        if not hdr and byte[0] != 0xCF:
            continue      # skip until CF start marker
        hdr += byte

    data_len = hdr[4]
    tail = recv_n(sock, data_len + 2, timeout=max(0.1, deadline - time.time()))
    return hdr + tail


def parse_banner(frame: bytes) -> dict:
    """Parse the 160-byte reader info frame."""
    info = {"model": "?", "reader": "?", "firmware": "?"}
    try:
        body = frame[5:-2]   # strip 5-byte header and 2-byte CRC
        parts = [s.decode("ascii", errors="replace")
                 for s in body.split(b"\x00")
                 if len(s) > 3 and all(0x20 <= c < 0x7F for c in s)]
        if len(parts) >= 1: info["model"]    = parts[0]
        if len(parts) >= 2: info["reader"]   = parts[1]
        if len(parts) >= 3: info["firmware"] = parts[2]
    except Exception:
        pass
    return info


def parse_tag(frame: bytes):
    """
    Parse a 21-byte tag notification frame.
    Layout: CF 00 00 01 0E 00 FD [RSSI] [CNT] [EPC x12] [CRC x2]
    Returns dict or None if not a tag frame.
    """
    if len(frame) < 21:
        return None
    if frame[0] != 0xCF:
        return None
    cmd = struct.unpack_from(">H", frame, 2)[0]
    if cmd != 0x0001 or frame[4] != 0x0E:
        return None

    crc_ok   = calc_crc16(frame[1:-2]) == struct.unpack_from(">H", frame, -2)[0]
    rssi_raw = frame[7]
    rssi_dbm = rssi_raw - 256 if rssi_raw > 127 else rssi_raw
    count    = frame[8]
    epc_raw  = frame[9:21].hex().upper()
    epc_fmt  = " ".join(epc_raw[i:i+4] for i in range(0, len(epc_raw), 4))

    return {
        "epc":       epc_fmt,
        "epc_raw":   epc_raw,
        "rssi_raw":  rssi_raw,
        "rssi_dbm":  rssi_dbm,
        "count":     count,
        "crc_ok":    crc_ok,
        "timestamp": datetime.now().isoformat(timespec="milliseconds"),
    }


def run():
    # ── CSV setup ─────────────────────────────────────────────────────────────
    csv_file = csv_writer = None
    if LOG_CSV:
        new_file = not os.path.exists(LOG_CSV)
        csv_file = open(LOG_CSV, "a", newline="")
        csv_writer = csv.DictWriter(
            csv_file,
            fieldnames=["timestamp", "epc", "rssi_dbm", "rssi_raw", "count", "crc_ok"]
        )
        if new_file:
            csv_writer.writeheader()

    seen = {}   # epc_raw → total reads this session

    try:
        # ── 1. TCP Connect ────────────────────────────────────────────────────
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.settimeout(10)
        print(f"[*] Connecting to {HOST}:{PORT} ...")
        sock.connect((HOST, PORT))
        print("[✓] TCP connected\n")

        # ── 2. Receive 2-byte connection ack ──────────────────────────────────
        ack = recv_n(sock, 2, timeout=5)
        print(f"[*] Reader ack      : {ack.hex() or '(empty)'}")

        # ── 3. Send INIT command ──────────────────────────────────────────────
        sock.sendall(CMD_INIT)
        print(f"[*] Sent INIT       : {CMD_INIT.hex()}")

        # ── 4. Receive banner (160-byte CF frame) ─────────────────────────────
        banner = recv_frame(sock, timeout=8)
        info   = parse_banner(banner)
        print(f"[✓] Banner received : {len(banner)}B")
        print(f"      Model         : {info['model']}")
        print(f"      Reader        : {info['reader']}")
        print(f"      Firmware      : {info['firmware']}\n")

        # ── 5. Send GET CONFIG ────────────────────────────────────────────────
        sock.sendall(CMD_GET_CONFIG)
        print(f"[*] Sent GET CONFIG : {CMD_GET_CONFIG.hex()}")

        # ── 6. Receive config response (33-byte CF frame) ─────────────────────
        cfg = recv_frame(sock, timeout=5)
        if cfg:
            cmd = struct.unpack_from(">H", cfg, 2)[0] if len(cfg) >= 4 else 0
            print(f"[✓] Config response : {len(cfg)}B  cmd=0x{cmd:04X}")
            print(f"    Raw             : {cfg.hex()}\n")
        else:
            print("[!] No config response — continuing anyway\n")

        # ── 7. Live tag stream ────────────────────────────────────────────────
        print("=" * 65)
        print("  LIVE TAG STREAM  (Ctrl+C to stop)")
        print("=" * 65)
        print(f"  {'EPC':<39} {'RSSI':>7}  {'READS':>6}  TIMESTAMP")
        print(f"  {'-'*39} {'-------':>7}  {'------':>6}  ---------")

        while True:
            frame = recv_frame(sock, timeout=TIMEOUT)

            if not frame:
                print("  [idle — no tag in range or timeout]")
                continue

            tag = parse_tag(frame)

            if tag:
                seen[tag["epc_raw"]] = seen.get(tag["epc_raw"], 0) + 1
                total    = seen[tag["epc_raw"]]
                crc_warn = "  ⚠ CRC!" if not tag["crc_ok"] else ""
                print(f"  {tag['epc']:<39} {tag['rssi_dbm']:>+6}dBm  "
                      f"{total:>6}  {tag['timestamp']}{crc_warn}")

                if csv_writer:
                    csv_writer.writerow({
                        "timestamp": tag["timestamp"],
                        "epc":       tag["epc"],
                        "rssi_dbm":  tag["rssi_dbm"],
                        "rssi_raw":  tag["rssi_raw"],
                        "count":     tag["count"],
                        "crc_ok":    tag["crc_ok"],
                    })
                    csv_file.flush()

            else:
                # Non-tag frame — config push, heartbeat, etc.
                cmd = struct.unpack_from(">H", frame, 2)[0] if len(frame) >= 4 else 0
                print(f"  [reader msg  cmd=0x{cmd:04X}  {len(frame)}B : {frame.hex()}]")

    except KeyboardInterrupt:
        print("\n\n[*] Stopped by user.")

    except ConnectionRefusedError:
        print(f"[✗] Connection refused — is the reader powered on at {HOST}:{PORT}?")

    except OSError as e:
        print(f"[✗] Socket error: {e}")

    except Exception as e:
        print(f"[✗] Unexpected error: {e}")

    finally:
        try:
            sock.close()
        except Exception:
            pass
        if csv_file:
            csv_file.close()

        if seen:
            print("\n" + "=" * 65)
            print("  SESSION Summary")
            print("=" * 65)
            for epc_raw, count in sorted(seen.items(), key=lambda x: -x[1]):
                epc_fmt = " ".join(epc_raw[i:i+4] for i in range(0, len(epc_raw), 4))
                print(f"  {epc_fmt}   {count:>5} reads")
            print(f"\n  Unique tags : {len(seen)}")
            print(f"  Total reads : {sum(seen.values()):,}")
            if LOG_CSV:
                print(f"  Log saved   : {LOG_CSV}")

        print("[*] Done.")


if __name__ == "__main__":
    run()