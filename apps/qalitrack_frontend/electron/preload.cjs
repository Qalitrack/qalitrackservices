const { contextBridge, ipcRenderer } = require('electron');

// Expose only what the renderer needs — nothing more.
contextBridge.exposeInMainWorld('electronAPI', {
  getMachineId: () => ipcRenderer.invoke('get-machine-id'),
});
