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
      // ✅ TRANSACTION API - Most specific route first
      '/api/Transaction': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Transaction Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 Transaction: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 Transaction Response: ${proxyRes.statusCode} for ${req.url}`);
          });
        },
      },

      // ✅ REPORTS API - For user shifts and reports
      '/api/Reports': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Reports Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 Reports: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 Reports Response: ${proxyRes.statusCode} for ${req.url}`);
          });
        },
      },

      // ✅ ROLES API
      '/api/Roles': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Roles Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 Roles: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 Roles Response: ${proxyRes.statusCode}`);
          });
        },
      },

      // ✅ AUTH API
      '/api/Auth': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ Auth Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 Auth: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 Auth Response: ${proxyRes.statusCode}`);
          });
        },
      },

      // ✅ MASTER DATA API
      '/api/MasterData': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('error', (err, _req, _res) => {
            console.log('❌ MasterData Proxy Error:', err.message);
          });
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 MasterData: ${req.method} ${req.url} → ${fullUrl}`);
          });
          proxy.on('proxyRes', (proxyRes, req, _res) => {
            console.log(`📥 MasterData Response: ${proxyRes.statusCode}`);
          });
        },
      },

      // ✅ USERS API
      '/api/Users': {
        target: 'https://qalitrack.cseco.co.ke',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            const fullUrl = `https://qalitrack.cseco.co.ke${req.url}`;
            console.log(`📤 Users: ${req.method} ${req.url} → ${fullUrl}`);
          });
        },
      },

      // ✅ Handle old-style routes without /api prefix for backward compatibility
      '/Reports': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📤 Old Reports route: ${req.method} ${req.url} → https://qalitrack.cseco.co.ke/api${req.url}`);
          });
        },
      },

      '/Roles': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📤 Old Roles route: ${req.method} ${req.url} → https://qalitrack.cseco.co.ke/api${req.url}`);
          });
        },
      },

      '/Users': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📤 Old Users route: ${req.method} ${req.url} → https://qalitrack.cseco.co.ke/api${req.url}`);
          });
        },
      },

      '/MasterData': {
        target: 'https://qalitrack.cseco.co.ke/api',
        changeOrigin: true,
        secure: false,
        configure: (proxy, _options) => {
          proxy.on('proxyReq', (proxyReq, req, _res) => {
            console.log(`📤 Old MasterData route: ${req.method} ${req.url} → https://qalitrack.cseco.co.ke/api${req.url}`);
          });
        },
      },

      // ✅ FALLBACK FOR OTHER /api ROUTES - Keep this last
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