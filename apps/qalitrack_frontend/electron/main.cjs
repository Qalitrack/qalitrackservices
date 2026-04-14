const { app, BrowserWindow, ipcMain } = require('electron');
const path = require('path');
const os   = require('os');
const fs   = require('fs');

const isDev = !app.isPackaged;

// ─────────────────────────────────────────────────────────────────────────────
// MACHINE ID
// Derived from hostname + primary MAC address, persisted to userData/machine-id.
// userData is NOT deleted by Windows NSIS uninstall — survives reinstalls.
// New OS install = new hardware derivation = new ID = customer needs new license.
// ─────────────────────────────────────────────────────────────────────────────
function deriveMachineId() {
  const ifaces = os.networkInterfaces();
  const mac = Object.values(ifaces)
    .flat()
    .find(n => n && !n.internal && n.mac && n.mac !== '00:00:00:00:00:00')
    ?.mac ?? 'nomac';
  return Buffer.from(`${os.hostname()}:${mac}`)
    .toString('base64')
    .replace(/[^a-zA-Z0-9]/g, '')
    .slice(0, 24);
}

function getMachineId() {
  const dir  = app.getPath('userData');
  const file = path.join(dir, 'machine-id');
  if (fs.existsSync(file)) {
    const cached = fs.readFileSync(file, 'utf8').trim();
    if (cached) return cached;
  }
  const id = deriveMachineId();
  fs.mkdirSync(dir, { recursive: true });
  fs.writeFileSync(file, id, 'utf8');
  return id;
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
    win.loadURL('http://localhost:5173');
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