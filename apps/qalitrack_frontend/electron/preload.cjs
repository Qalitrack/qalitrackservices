const { contextBridge } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  // Expose any APIs you need here
});