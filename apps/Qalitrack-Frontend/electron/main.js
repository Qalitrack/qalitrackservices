const { app, BrowserWindow } = require('electron');

function createWindow() {
  const win = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: { nodeIntegration: true }
  });
  win.loadURL('http://localhost:19006'); // Expo web dev server
}

app.on('ready', createWindow);
