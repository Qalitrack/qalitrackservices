import v from "electron";
import _ from "path";
import y from "os";
import "fs";
import M from "child_process";
function $(e) {
  return e && e.__esModule && Object.prototype.hasOwnProperty.call(e, "default") ? e.default : e;
}
var m = {}, u;
function b() {
  if (u) return m;
  u = 1;
  const { app: e, BrowserWindow: a, ipcMain: f } = v, c = _, s = y, l = !e.isPackaged;
  process.platform === "linux" && (e.commandLine.appendSwitch("no-sandbox"), e.commandLine.appendSwitch("no-zygote"));
  const { execSync: h } = M;
  function w() {
    var i;
    try {
      const o = h(
        'reg query "HKLM\\SOFTWARE\\Microsoft\\Cryptography" /v MachineGuid',
        { encoding: "utf8", stdio: ["pipe", "pipe", "pipe"] }
      ).match(/MachineGuid\s+REG_SZ\s+([^\r\n]+)/);
      if (o && o[1])
        return o[1].trim().replace(/[^a-zA-Z0-9\-]/g, "").slice(0, 36);
    } catch {
    }
    const n = s.networkInterfaces(), d = ((i = Object.values(n).flat().filter((t) => t && !t.internal && t.mac && t.mac !== "00:00:00:00:00:00").sort((t, o) => t.mac.localeCompare(o.mac))[0]) == null ? void 0 : i.mac) ?? "nomac";
    return Buffer.from(`${s.hostname()}:${d}`).toString("base64").replace(/[^a-zA-Z0-9]/g, "").slice(0, 24);
  }
  let r = null;
  function g() {
    return r || (r = w()), r;
  }
  f.handle("get-machine-id", () => g());
  function p() {
    const n = new a({
      width: 1400,
      height: 900,
      webPreferences: {
        nodeIntegration: !1,
        contextIsolation: !0,
        preload: c.join(__dirname, "preload.cjs"),
        webSecurity: !l
      }
    });
    l ? (n.loadURL(process.env.VITE_DEV_SERVER_URL || "http://localhost:5173"), n.webContents.openDevTools()) : n.loadFile(c.join(__dirname, "../dist/index.html")), n.webContents.on("before-input-event", (d, i) => {
      i.key === "F12" && n.webContents.toggleDevTools();
    });
  }
  return e.whenReady().then(p), e.on("window-all-closed", () => {
    process.platform !== "darwin" && e.quit();
  }), e.on("activate", () => {
    a.getAllWindows().length === 0 && p();
  }), m;
}
var q = b();
const C = /* @__PURE__ */ $(q);
export {
  C as default
};
