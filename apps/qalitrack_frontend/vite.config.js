import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import electron from 'vite-plugin-electron';

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      // Your PWA configuration
    }),
    electron({
      entry: 'electron/main.cjs',
    }),
  ],

  base: './',

  server: {
    port: 5173,
    host: true,
    
    proxy: {
      // ✅ AUTH API (unchanged)
      '/Auth': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Auth Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke/api${req.url}`;
            console.log(`📤 Auth: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 Auth Response: ${proxyRes.statusCode}`);
          });
        },
      },

      // ✅ TRANSACTION API (UPDATED: target to /api/transactions for gateway)
      // TRANSACTION API - UPDATED to direct /api/Transaction
      '/api/Transaction': {
  target: 'https://qalitrack.cseco.co.ke',
  changeOrigin: true,
  secure: false,
  rewrite: (path) => path.replace(/^\/api\/Transaction/, '/api/Transaction'), // optional, but explicit
  configure: (proxy, _options) => {
    proxy.on('error', (err, _req, _res) => {
      console.log('❌ Transaction Proxy Error:', err.message);
    });
    proxy.on('proxyReq', (proxyReq, req, _res) => {
      const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
      console.log(`📤 Transaction: ${req.method} ${req.url} → ${fullUrl}`);
    });
    proxy.on('proxyRes', (proxyRes, req, _res) => {
      console.log(`📥 Transaction Response: ${proxyRes.statusCode} ${proxyRes.statusMessage || ''}`);
    });
  },
},

      // ✅ MASTER DATA API (unchanged)
      '/MasterData': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ MasterData Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke/api${req.url}`;
            console.log(`📤 MasterData: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 MasterData Response: ${proxyRes.statusCode}`);
          });
        },
      },

      // ✅ USERS API (unchanged)
      '/Users': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke/api${req.url}`;
            console.log(`📤 Users: ${req.method} ${req.url} → ${fullUrl}`);
          });
        },
      },

      // ✅ FALLBACK FOR OTHER /api ROUTES (unchanged)
      '/api': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📤 API Fallback: ${req.method} ${req.url} → https://qalitrack.cseco.co.ke${req.url}`);
          });
        },
      },
    },
  },

  build: {
    outDir: 'dist',
  },
});