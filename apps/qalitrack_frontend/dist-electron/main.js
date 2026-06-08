import require$$0 from "electron";
import require$$1 from "path";
import require$$2 from "os";
import "fs";
import require$$4 from "child_process";
function getDefaultExportFromCjs(x) {
  return x && x.__esModule && Object.prototype.hasOwnProperty.call(x, "default") ? x["default"] : x;
}
var main$1 = {};
var hasRequiredMain;
function requireMain() {
  if (hasRequiredMain) return main$1;
  hasRequiredMain = 1;
  const { app, BrowserWindow, ipcMain } = require$$0;
  const path = require$$1;
  const os = require$$2;
  const isDev = !app.isPackaged;
  if (process.platform === "linux") {
    app.commandLine.appendSwitch("no-sandbox");
    app.commandLine.appendSwitch("no-zygote");
  }
  const { execSync } = require$$4;
  function deriveMachineId() {
    var _a;
    try {
      const out = execSync(
        'reg query "HKLM\\SOFTWARE\\Microsoft\\Cryptography" /v MachineGuid',
        { encoding: "utf8", stdio: ["pipe", "pipe", "pipe"] }
      );
      const match = out.match(/MachineGuid\s+REG_SZ\s+([^\r\n]+)/);
      if (match && match[1]) {
        return match[1].trim().replace(/[^a-zA-Z0-9\-]/g, "").slice(0, 36);
      }
    } catch (_) {
    }
    const ifaces = os.networkInterfaces();
    const mac = ((_a = Object.values(ifaces).flat().filter((n) => n && !n.internal && n.mac && n.mac !== "00:00:00:00:00:00").sort((a, b) => a.mac.localeCompare(b.mac))[0]) == null ? void 0 : _a.mac) ?? "nomac";
    return Buffer.from(`${os.hostname()}:${mac}`).toString("base64").replace(/[^a-zA-Z0-9]/g, "").slice(0, 24);
  }
  let _cachedMachineId = null;
  function getMachineId() {
    if (!_cachedMachineId) _cachedMachineId = deriveMachineId();
    return _cachedMachineId;
  }
  ipcMain.handle("get-machine-id", () => getMachineId());
  function createWindow() {
    const win = new BrowserWindow({
      width: 1400,
      height: 900,
      webPreferences: {
        nodeIntegration: false,
        contextIsolation: true,
        preload: path.join(__dirname, "preload.cjs"),
        webSecurity: !isDev
      }
    });
    if (isDev) {
      win.loadURL(process.env.VITE_DEV_SERVER_URL || "http://localhost:5173");
      win.webContents.openDevTools();
    } else {
      win.loadFile(path.join(__dirname, "../dist/index.html"));
    }
    win.webContents.on("before-input-event", (event, input) => {
      if (input.key === "F12") {
        win.webContents.toggleDevTools();
      }
    });
  }
  app.whenReady().then(createWindow);
  app.on("window-all-closed", () => {
    if (process.platform !== "darwin") {
      app.quit();
    }
  });
  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
  return main$1;
}
var mainExports = requireMain();
const main = /* @__PURE__ */ getDefaultExportFromCjs(mainExports);
export {
  main as default
};
