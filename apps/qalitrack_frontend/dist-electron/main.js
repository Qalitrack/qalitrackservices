import w from "electron";
import g from "path";
import v from "os";
import $ from "fs";
function b(e) {
  return e && e.__esModule && Object.prototype.hasOwnProperty.call(e, "default") ? e.default : e;
}
var f = {}, u;
function y() {
  if (u) return f;
  u = 1;
  const { app: e, BrowserWindow: c, ipcMain: m } = w, a = g, s = v, r = $, l = !e.isPackaged;
  function p() {
    var n;
    const t = s.networkInterfaces(), o = ((n = Object.values(t).flat().find((i) => i && !i.internal && i.mac && i.mac !== "00:00:00:00:00:00")) == null ? void 0 : n.mac) ?? "nomac";
    return Buffer.from(`${s.hostname()}:${o}`).toString("base64").replace(/[^a-zA-Z0-9]/g, "").slice(0, 24);
  }
  function h() {
    const t = e.getPath("userData"), o = a.join(t, "machine-id");
    if (r.existsSync(o)) {
      const i = r.readFileSync(o, "utf8").trim();
      if (i) return i;
    }
    const n = p();
    return r.mkdirSync(t, { recursive: !0 }), r.writeFileSync(o, n, "utf8"), n;
  }
  m.handle("get-machine-id", () => h());
  function d() {
    const t = new c({
      width: 1400,
      height: 900,
      webPreferences: {
        nodeIntegration: !1,
        contextIsolation: !0,
        preload: a.join(__dirname, "preload.cjs"),
        webSecurity: l
      }
    });
    l ? (t.loadURL("http://localhost:5173"), t.webContents.openDevTools()) : t.loadFile(a.join(__dirname, "../dist/index.html")), t.webContents.on("before-input-event", (o, n) => {
      n.key === "F12" && t.webContents.toggleDevTools();
    });
  }
  return e.whenReady().then(d), e.on("window-all-closed", () => {
    process.platform !== "darwin" && e.quit();
  }), e.on("activate", () => {
    c.getAllWindows().length === 0 && d();
  }), f;
}
var j = y();
const D = /* @__PURE__ */ b(j);
export {
  D as default
};
