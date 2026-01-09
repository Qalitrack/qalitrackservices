import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import electron from 'vite-plugin-electron';

// 🔗 Define the base URL for the Transactions API
const TRANSACTION_API_TARGET = 'https://qalitrack.cseco.co.ke/';

export default defineConfig({
  plugins: [
    react({
      // ... (Your Babel configuration remains the same)
    }),
    VitePWA({
      // ... (Your PWA configuration remains the same)
    }),
    electron({
      entry: 'electron/main.cjs',
    }),
  ],

  base: './', // ⚡ Important for Electron - ensures relative paths work

  // 🌍 Vite Development Server Configuration (CORS Fix)
  server: {
    proxy: {
      // 💥 CRITICAL FIX APPLIED HERE 💥
      '/Transaction': {
        target: TRANSACTION_API_TARGET, 
        changeOrigin: true,       
        secure: true,   
        // 🚀 FIX: The Transactions API *root* likely handles routing based on the full path.
        // We need to ensure that when we request /Transaction, the target server gets
        // a path it expects. Since your transactionsClient in Redux is calling client.get(""),
        // the full path being sent to the proxy is just /Transaction.
        // Let's assume the actual API endpoint is https://qalitrack.cseco.co.ke/Transaction/...
        // We should NOT use rewrite here if the target API expects the /Transaction prefix.
        // Let's try the rewrite first, as it often solves the "HTML page" problem.

        // If your API endpoint for fetching transactions is JUST the target URL, then 
        // the proxy needs to remove the `/Transaction` prefix from the path before forwarding it.
        rewrite: (path) => path.replace(/^\/Transaction/, ''), 
      },
      
      // ✅ MASTER DATA PROXY: Redirects http://localhost:5173/api/... to https://qalitrack.cseco.co.ke/api/...
      // NOTE: This master data proxy is likely correct if the base is 'https://qalitrack.cseco.co.ke/api'
      '/api': {
        target: TRANSACTION_API_TARGET, // Use the same base target
        changeOrigin: true,
        secure: true,
        // The Redux client for Master Data is configured to append the MasterData endpoints,
        // e.g., /MasterData/Vehicles. We assume the API expects the path without the local '/api' prefix.
        rewrite: (path) => path.replace(/^\/api/, ''), 
      },
    }
  }
});