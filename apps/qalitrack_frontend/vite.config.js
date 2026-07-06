import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import electron from 'vite-plugin-electron';

/* -------------------------------------------------------------------------- */
/*                           VITE CONFIGURATION                               */
/* -------------------------------------------------------------------------- */

export default defineConfig(({ mode }) => {
const env = loadEnv(mode, process.cwd(), '');
const API_TARGET = env.VITE_API_TARGET || 'https://qalitrack.cseco.co.ke';
const IS_PRODUCTION = mode === 'production';
const APP_TARGET = process.env.VITE_APP_TARGET || 'main'; // 'main' | 'kiosk'
const IS_KIOSK = APP_TARGET === 'kiosk';
// When kiosk is being packaged into Electron, output to dist/ (not dist-kiosk/)
// so electron/main.cjs can find dist/index.html after the rename step.
const IS_ELECTRON_KIOSK = IS_KIOSK && process.env.ELECTRON_KIOSK === '1';

console.log('🎯 API Target:', API_TARGET);
console.log('🏭 Environment:', IS_PRODUCTION ? 'PRODUCTION' : 'DEVELOPMENT');

const createProxyConfig = (routeName, target = API_TARGET, options = {}) => ({
  target,
  changeOrigin: true,
  secure: false,
  timeout: 60000,
  proxyTimeout: 60000,
  ...options,
  configure: (proxy, _options) => {
    proxy.on('error', (err, req, res) => {
      console.error(`❌ ${routeName} Proxy Error:`, {
        message: err.message,
        code: err.code,
        url: req.url,
        target: target
      });
      if (!res.headersSent) {
        res.writeHead(502, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          error: 'Proxy Error',
          message: err.message,
          route: routeName,
          target: target,
          hint: 'Check if backend server is running and accessible'
        }));
      }
    });

    proxy.on('timeout', (req, res) => {
      console.error(`⏱️ ${routeName} Timeout:`, { url: req.url, target: target, method: req.method });
      if (!res.headersSent) {
        res.writeHead(504, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ error: 'Gateway Timeout', message: `Request to ${target}${req.url} timed out after 30 seconds`, route: routeName }));
      }
    });

    proxy.on('proxyReq', (proxyReq, req, _res) => {
      console.log(`📤 ${routeName}: ${req.method} ${req.url} → ${target}${req.url}`);
      proxyReq.setHeader('Accept', 'application/json');
      if (!IS_PRODUCTION && ['POST', 'PUT', 'PATCH'].includes(req.method)) {
        let body = '';
        req.on('data', chunk => { body += chunk.toString(); });
        req.on('end', () => {
          if (body) {
            try {
              const parsed = JSON.parse(body);
              if (parsed.password) parsed.password = '***';
              console.log(`📦 ${routeName} Body:`, JSON.stringify(parsed, null, 2).substring(0, 500));
            } catch { console.log(`📦 ${routeName} Body:`, body.substring(0, 500)); }
          }
        });
      }
    });

    proxy.on('proxyRes', (proxyRes, req, _res) => {
      const statusEmoji = proxyRes.statusCode >= 400 ? '❌' : '✅';
      console.log(`${statusEmoji} ${routeName} Response: ${proxyRes.statusCode} for ${req.url}`);
      if (!IS_PRODUCTION && proxyRes.statusCode >= 400) {
        let body = '';
        proxyRes.on('data', chunk => { body += chunk.toString(); });
        proxyRes.on('end', () => { if (body) console.error(`❌ ${routeName} Error Body:`, body.substring(0, 500)); });
      }
    });

    if (options.configure) options.configure(proxy, _options);
  },
});

return {
  plugins: [
    react(),
    // PWA and Electron only for the main app
    ...(IS_KIOSK ? [] : [
      VitePWA({
        registerType: 'autoUpdate',
        includeAssets: ['favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
        manifest: {
          name: 'QaliTrack',
          short_name: 'QaliTrack',
          theme_color: '#ffffff',
          icons: [
            { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
            { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
          ],
        },
      }),
      ...(process.env.WEB_ONLY ? [] : [electron({ entry: 'electron/main.cjs' })]),
    ]),
  ],

  base: './',

  server: {
    port: 5173,
    host: true,
    ws: true,
    
    hmr: {
      timeout: 30000,
    },
    
    proxy: {
      /* -------------------------------------------------------------------- */
      /*              PRIMARY API ROUTES (Most specific first)                */
      /* -------------------------------------------------------------------- */
      
      // ✅ All /api routes proxy to backend
      '/api': createProxyConfig('API', API_TARGET, {
        // Don't rewrite the path - keep /api prefix
        rewrite: undefined,
      }),
      
      /* -------------------------------------------------------------------- */
      /*          BACKWARD COMPATIBILITY (Non-prefixed routes)                */
      /* -------------------------------------------------------------------- */
      
      '/Transaction': createProxyConfig(
        'Transaction (Legacy)', 
        API_TARGET,
        {
          rewrite: (path) => `/api${path}`,
        }
      ),
      '/Reports': createProxyConfig(
        'Reports (Legacy)', 
        API_TARGET,
        {
          rewrite: (path) => `/api${path}`,
        }
      ),
      '/Roles': createProxyConfig(
        'Roles (Legacy)', 
        API_TARGET,
        {
          rewrite: (path) => `/api${path}`,
        }
      ),
      '/Users': createProxyConfig(
        'Users (Legacy)', 
        API_TARGET,
        {
          rewrite: (path) => `/api${path}`,
        }
      ),
      '/MasterData': createProxyConfig(
        'MasterData (Legacy)', 
        API_TARGET,
        {
          rewrite: (path) => `/api${path}`,
        }
      ),
      
      /* -------------------------------------------------------------------- */
      /*                  WEBSOCKET SUPPORT (Optional)                        */
      /* -------------------------------------------------------------------- */
      
      '/ws': {
        target: API_TARGET.replace('https://', 'wss://').replace('http://', 'ws://'),
        ws: true,
        changeOrigin: true,
        secure: false,
      },
    },
  },

  build: {
    outDir: IS_KIOSK && !IS_ELECTRON_KIOSK ? 'dist-kiosk' : 'dist',
    sourcemap: !IS_PRODUCTION,
    minify: IS_PRODUCTION ? 'esbuild' : false,

    rollupOptions: {
      input: IS_KIOSK ? 'kiosk.html' : 'index.html',
      output: {
        manualChunks: IS_KIOSK
          ? { 'react-vendor': ['react', 'react-dom'] }
          : {
              'react-vendor': ['react', 'react-dom', 'react-router-dom'],
              'ui-vendor': ['antd'],
              'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
            },
      },
    },
  },

  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router-dom', 'axios', 'antd'],
  },
};
});