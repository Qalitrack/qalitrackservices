import M from "electron";
import _ from "path";
import y from "os";
import "fs";
import S from "child_process";
function $(e) {
  return e && e.__esModule && Object.prototype.hasOwnProperty.call(e, "default") ? e.default : e;
}
var f = {}, m;
function b() {
  if (m) return f;
  m = 1;
  const { app: e, BrowserWindow: c, ipcMain: h } = M, s = _, l = y, d = !e.isPackaged;
  process.platform === "linux" && (e.commandLine.appendSwitch("no-sandbox"), e.commandLine.appendSwitch("no-zygote"));
  const { execSync: g } = S;
  function w() {
    var r;
    try {
      const i = g(
        'reg query "HKLM\\SOFTWARE\\Microsoft\\Cryptography" /v MachineGuid',
        { encoding: "utf8", stdio: ["pipe", "pipe", "pipe"] }
      ).match(/MachineGuid\s+REG_SZ\s+([^\r\n]+)/);
      if (i && i[1])
        return i[1].trim().replace(/[^a-zA-Z0-9\-]/g, "").slice(0, 36);
    } catch {
    }
    const n = l.networkInterfaces(), p = ((r = Object.values(n).flat().filter((t) => t && !t.internal && t.mac && t.mac !== "00:00:00:00:00:00").sort((t, i) => t.mac.localeCompare(i.mac))[0]) == null ? void 0 : r.mac) ?? "nomac";
    return Buffer.from(`${l.hostname()}:${p}`).toString("base64").replace(/[^a-zA-Z0-9]/g, "").slice(0, 24);
  }
  let a = null;
  function v() {
    return a || (a = w()), a;
  }
  h.handle("get-machine-id", () => v());
  let o = null;
  function u() {
    const n = new c({
      width: 1400,
      height: 900,
      webPreferences: {
        nodeIntegration: !1,
        contextIsolation: !0,
        preload: s.join(__dirname, "preload.cjs"),
        webSecurity: !d
      }
    });
    o = n, n.on("closed", () => {
      o = null;
    }), d ? (n.loadURL(process.env.VITE_DEV_SERVER_URL || "http://localhost:5173"), n.webContents.openDevTools()) : n.loadFile(s.join(__dirname, "../dist/index.html")), n.webContents.on("before-input-event", (p, r) => {
      r.key === "F12" && n.webContents.toggleDevTools();
    });
  }
  return e.requestSingleInstanceLock() ? (e.on("second-instance", () => {
    o && (o.isMinimized() && o.restore(), o.focus());
  }), e.whenReady().then(u), e.on("window-all-closed", () => {
    process.platform !== "darwin" && e.quit();
  }), e.on("activate", () => {
    c.getAllWindows().length === 0 && u();
  })) : e.quit(), f;
}
var q = b();
const C = /* @__PURE__ */ $(q);
export {
  C as default
};
