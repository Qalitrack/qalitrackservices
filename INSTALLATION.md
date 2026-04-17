# Qalitrack Installation Guide

This guide covers installing the full Qalitrack system on a Windows machine — both the backend services and the hardware client.

---

## What You Are Installing

| Component | What it is | How it runs |
|---|---|---|
| **Backend** | API services (users, master data, transactions, backups) | Docker containers |
| **Hardware Client** | Weighbridge + camera + RFID/NFC collector | Windows Service (.exe) |

---

## Part 1 — Backend (Docker)

### Before you start

You need two things ready:
1. **Docker Desktop for Windows** — download and install from [docker.com](https://www.docker.com/products/docker-desktop/). Accept all defaults, restart when asked, and open it once so it finishes first-time setup.
2. **The `.env` file for this site** — provided separately by the Qalitrack team. Do not create or edit it yourself.

> **Enable Docker auto-start now:**
> Docker Desktop → Settings → General → check **"Start Docker Desktop when you log in"**.
> This ensures the backend survives reboots without anyone manually starting anything.

---

### Step 1 — Run the Installer

The installer is a batch file — no terminal knowledge or policy changes needed.

1. Copy the `.env` file provided for this site into the same folder as `install.bat`.
2. Double-click **`install.bat`**.
3. Windows will ask if you want to allow it to make changes — click **Yes**.

That's it. The installer handles everything:
- Checks Docker Desktop and Git versions meet the minimum requirements
- Clones the `katani` branch **inside WSL2** (the Linux subsystem) — this completely avoids Windows filename restrictions. Files like `sha256:<hash>` that NTFS cannot hold are checked out cleanly in the Linux filesystem.
- Copies the `.env` into the right place inside WSL2
- Starts Docker Desktop if it isn't already running
- Builds all service images and starts the containers (5–15 minutes on first run)
- Reports the health of every service when done

---

### Step 2 — Verify

Open a browser:

| Service | URL |
|---|---|
| Gateway | `http://localhost:7000` |
| User Service | `http://localhost:7001` |
| Master Data | `http://localhost:7002` |
| Backup Service | `http://localhost:7003` |
| Transactions | `http://localhost:7004` |

All should return a response. If any are not yet ready, wait 60 seconds — databases take a moment on first boot.

---

### Restarting After a Reboot

All containers use `restart: unless-stopped`. Once Docker Desktop is running (which happens automatically on login), everything starts on its own. No manual steps needed.

To stop or restart manually:

```powershell
cd C:\Qalitrack\source\Deployment
docker compose down        # stop
docker compose up -d       # start again (images already built)
```

---

### Updating the Backend

```powershell
cd C:\Qalitrack\source
git pull
cd Deployment
docker compose up --build -d
```

Existing data (databases, volumes) is preserved across updates.

---

## Part 2 — Hardware Client

The hardware client collects weight data from the weighbridge, video from the RTSP camera, and optionally RFID/NFC reads. It runs as a Windows Service.

No build tools, scripts, or source code are needed. The release package ships a single `Qalitrack.exe` that contains its own setup wizard.

### Step 1 — Run the Installer

1. Locate `Qalitrack.exe` in the release folder.
2. Right-click it → **"Run as administrator"** (required to register a Windows Service).
3. The setup wizard opens. Fill in the details for this site:

**Weighbridge**
- Choose **TCP / Network** (weighbridge has a network port) or **Serial / RS-232** (COM port).
- TCP: weighbridge IP address and port (default `3002`).
- Serial: COM port (e.g. `COM1`), baud rate, and serial parameters from the weighbridge manual.

**Camera (RTSP)**
- Camera IP address. RTSP port is usually `554`.
- RTSP path from the camera's manual (e.g. `/stream1`).
- Username / password (often `admin` / `admin` by default).
- Tick **"Supports Snapshots"** if the camera has an HTTP snapshot endpoint.
- Tick **"Enable Plate Recognition (NPR)"** if you have a plate recognition module — enter its HTTP and WebSocket URLs.

**RFID Reader (optional)**
- Tick **"RFID Reader Installed"** → enter the reader IP and port (default `2022`).

**NFC Reader (optional)**
- Tick **"NFC Reader Installed"** → select the COM port.

4. Click **"Install Service"**.

The wizard copies the exe to `C:\Program Files\Qalitrack\`, registers the Windows Service, and starts it. A dialog confirms success and shows the local API at `http://localhost:5000`.

> The service is set to **start automatically** and **restart on failure** — no action needed after a reboot.

---

### Step 2 — Verify the Hardware Client

Open a browser and go to `http://localhost:5000`. You should see the Qalitrack hardware API.

To check service status in PowerShell:

```powershell
Get-Service QalitrackPlatformService
```

Expected output: `Status: Running`.

---

### Updating the Hardware Client

1. Get the new `Qalitrack.exe` from the release package.
2. Right-click → **"Run as administrator"**.
3. The wizard detects the existing configuration, pre-fills all fields, and the button reads **"Update & Reinstall"**.
4. Click it — the service stops, the new binary is copied in, and the service restarts. Your configuration is preserved.

---

## Troubleshooting

### Docker containers keep restarting

Run `docker compose logs <service-name>` to see the error. The most common cause is a missing or wrong password in the `.env` file.

### `docker compose up` fails with "context not found" or missing files

This usually means the repo was cloned on the Windows filesystem instead of inside WSL2. Delete `C:\Qalitrack\source` if it exists, then re-run `install.bat` — the installer clones into the WSL2 Linux filesystem (`/home/qalitrack/source`) where there are no filename restrictions.

### WSL is not installed

If the installer reports WSL is missing, run this in PowerShell as Administrator then restart the PC:
```powershell
wsl --install
```
After restart, re-run `install.bat`.

### Hardware client says "Installation Failed — run as Administrator"

Right-click `Qalitrack.exe` and choose **"Run as administrator"**. Windows Services can only be registered by an Administrator.

### Service shows "Stopped" after reboot

Check that Docker Desktop is set to start on login (Part 1, Step 1). The containers themselves restart automatically once Docker is running.

### Camera not connecting

- Verify the IP and RTSP path by testing in VLC: **Media → Open Network Stream** → enter `rtsp://<username>:<password>@<ip>:<port>/<path>`.
- The service will keep retrying — it does not need to be restarted manually.

### Weighbridge shows no data (TCP mode)

- Confirm the weighbridge IP and port with the site engineer.
- Check that the weighbridge controller is powered on and the network cable is connected.
- Try pinging the weighbridge: `ping <ip>` in PowerShell.

### Weighbridge shows no data (Serial mode)

- Confirm the COM port in **Device Manager** under "Ports (COM & LPT)".
- Check baud rate and parity settings against the weighbridge manual.
- The USB-to-serial driver (`CH341SER.EXE`) is included in the release package if needed.

---

## Port Reference

| Port | Service |
|---|---|
| 5000 | Hardware client local API |
| 7000 | Gateway |
| 7001 | User Service |
| 7002 | Master Data Service |
| 7003 | Backup Service |
| 7004 | Transaction Service |
| 7010 | RabbitMQ Management (user service) |
| 7011 | RabbitMQ Management (backup service) |
| 7012 | RabbitMQ Management (transaction service) |
