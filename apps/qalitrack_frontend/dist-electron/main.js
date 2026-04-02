import d from "electron";
import c from "path";
function u(e) {
  return e && e.__esModule && Object.prototype.hasOwnProperty.call(e, "default") ? e.default : e;
}
var r = {}, a;
function f() {
  if (a) return r;
  a = 1;
  const { app: e, BrowserWindow: n } = d, s = c, o = !e.isPackaged;
  function i() {
    const t = new n({
      width: 1400,
      height: 900,
      webPreferences: {
        nodeIntegration: !1,
        contextIsolation: !0,
        webSecurity: o
        // allow file:// → http://localhost:7000 requests in production
      }
    });
    o ? (t.loadURL("http://localhost:5173"), t.webContents.openDevTools()) : t.loadFile(s.join(__dirname, "../dist/index.html")), t.webContents.on("before-input-event", (p, l) => {
      l.key === "F12" && t.webContents.toggleDevTools();
    });
  }
  return e.whenReady().then(i), e.on("window-all-closed", () => {
    process.platform !== "darwin" && e.quit();
  }), e.on("activate", () => {
    n.getAllWindows().length === 0 && i();
  }), r;
}
var w = f();
const v = /* @__PURE__ */ u(w);
export {
  v as default
};
