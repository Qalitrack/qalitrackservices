const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const os   = require('os');
const fs   = require('fs');

const isDev = !app.isPackaged;

// ─────────────────────────────────────────────────────────────────────────────
// MACHINE ID
// Read from HKLM\SOFTWARE\Microsoft\Cryptography\MachineGuid — assigned once
// by Windows at OS install, unaffected by network adapters, Docker, VPNs, or
// app reinstalls. New OS install = new GUID = customer needs new license.
// Falls back to hostname+MAC only if the registry read fails (non-Windows).
// ─────────────────────────────────────────────────────────────────────────────
const { execSync } = require('child_process');

function deriveMachineId() {
  // Primary: Windows Registry MachineGuid (stable across reboots/reinstalls)
  try {
    const out = execSync(
      'reg query "HKLM\\SOFTWARE\\Microsoft\\Cryptography" /v MachineGuid',
      { encoding: 'utf8', stdio: ['pipe', 'pipe', 'pipe'] }
    );
    const match = out.match(/MachineGuid\s+REG_SZ\s+([^\r\n]+)/);
    if (match && match[1]) {
      return match[1].trim().replace(/[^a-zA-Z0-9\-]/g, '').slice(0, 36);
    }
  } catch (_) {
    // Not Windows or registry unavailable — fall through to MAC fallback
  }

  // Fallback: hostname + first physical (non-virtual) MAC address
  const ifaces = os.networkInterfaces();
  const mac = Object.values(ifaces)
    .flat()
    .filter(n => n && !n.internal && n.mac && n.mac !== '00:00:00:00:00:00')
    .sort((a, b) => a.mac.localeCompare(b.mac))[0]?.mac ?? 'nomac';
  return Buffer.from(`${os.hostname()}:${mac}`)
    .toString('base64')
    .replace(/[^a-zA-Z0-9]/g, '')
    .slice(0, 24);
}

function getMachineId() {
  return deriveMachineId();
}

ipcMain.handle('get-machine-id', () => getMachineId());

// ─────────────────────────────────────────────────────────────────────────────

function createWindow() {
  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.cjs'),
      webSecurity: isDev,
    }
  });

  if (isDev) {
    win.loadURL(process.env.VITE_DEV_SERVER_URL || 'http://localhost:5173');
    win.webContents.openDevTools();
  } else {
    win.loadFile(path.join(__dirname, '../dist/index.html'));
  }

  // Toggle DevTools with F12 in any mode
  win.webContents.on('before-input-event', (event, input) => {
    if (input.key === 'F12') {
      win.webContents.toggleDevTools();
    }
  });
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});