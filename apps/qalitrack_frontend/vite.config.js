import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';
import electron from 'vite-plugin-electron';

/* -------------------------------------------------------------------------- */
/*                           CONFIGURATION                                     */
/* -------------------------------------------------------------------------- */

const API_TARGET = process.env.VITE_API_TARGET || 'https://qalitrack.cseco.co.ke';
const IS_PRODUCTION = process.env.NODE_ENV === 'production';

/* -------------------------------------------------------------------------- */
/*                         PROXY HELPER FUNCTION                              */
/* -------------------------------------------------------------------------- */

/**
 * Creates a standardized proxy configuration
 * @param {string} routeName - Name for logging (e.g., "Transaction", "Auth")
 * @param {string} target - Target URL (defaults to API_TARGET)
 * @param {object} options - Additional options
 */
const createProxyConfig = (routeName, target = API_TARGET, options = {}) => ({
  target,
  changeOrigin: true,
  secure: IS_PRODUCTION,
  timeout: 30000, // 30 seconds
  proxyTimeout: 30000,
  ...options,
  configure: (proxy, _options) => {
    // Error handling
    proxy.on('error', (err, _req, _res) => {
      console.error(`❌ ${routeName} Proxy Error:`, err.message);
    });

    // Timeout handling
    proxy.on('timeout', (req, res, target) => {
      console.error(`⏱️ ${routeName} Timeout: Request to ${target} timed out`);
      if (!res.headersSent) {
        res.writeHead(504, { 'Content-Type': 'text/plain' });
        res.end('Gateway Timeout');
      }
    });

    // Request logging
    proxy.on('proxyReq', (proxyReq, req, _res) => {
      const fullUrl = `${target}${req.url}`;
      console.log(`📤 ${routeName}: ${req.method} ${req.url} → ${fullUrl}`);
      
      // Log request body in development for debugging
      if (!IS_PRODUCTION && ['POST', 'PUT', 'PATCH'].includes(req.method)) {
        let body = '';
        req.on('data', chunk => { body += chunk.toString(); });
        req.on('end', () => {
          if (body) {
            try {
              const parsed = JSON.parse(body);
              console.log(`📦 ${routeName} Body:`, JSON.stringify(parsed, null, 2).substring(0, 500));
            } catch {
              console.log(`📦 ${routeName} Body:`, body.substring(0, 500));
            }
          }
        });
      }
    });

    // Response logging
    proxy.on('proxyRes', (proxyRes, req, _res) => {
      const statusEmoji = proxyRes.statusCode >= 400 ? '❌' : '📥';
      console.log(`${statusEmoji} ${routeName} Response: ${proxyRes.statusCode} for ${req.url}`);
      
      // Log error response body in development
      if (!IS_PRODUCTION && proxyRes.statusCode >= 400) {
        let body = '';
        proxyRes.on('data', chunk => { body += chunk.toString(); });
        proxyRes.on('end', () => {
          if (body) {
            console.error(`❌ ${routeName} Error Body:`, body.substring(0, 500));
          }
        });
      }
    });

    // Execute custom configure if provided
    if (options.configure) {
      options.configure(proxy, _options);
    }
  },
});

/* -------------------------------------------------------------------------- */
/*                           VITE CONFIGURATION                               */
/* -------------------------------------------------------------------------- */

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.ico', 'robots.txt', 'apple-touch-icon.png'],
      manifest: {
        name: 'QaliTrack',
        short_name: 'QaliTrack',
        theme_color: '#ffffff',
        icons: [
          {
            src: 'pwa-192x192.png',
            sizes: '192x192',
            type: 'image/png',
          },
          {
            src: 'pwa-512x512.png',
            sizes: '512x512',
            type: 'image/png',
          },
        ],
      },
    }),
    electron({
      entry: 'electron/main.cjs',
    }),
  ],

  base: './',

  server: {
    port: 5173,
    host: true,
    
    // Enable WebSocket support
    ws: true,
    
    proxy: {
      /* -------------------------------------------------------------------- */
      /*              PRIMARY API ROUTES (Most specific first)                */
      /* -------------------------------------------------------------------- */
      
      // ✅ CRITICAL: Transaction endpoint with nested path structure
      // Based on curl: /api/Transaction/Transaction/Transaction
      '/api/Transaction': createProxyConfig('Transaction API'),
      
      '/api/Reports': createProxyConfig('Reports API'),
      '/api/Roles': createProxyConfig('Roles API'),
      '/api/Auth': createProxyConfig('Auth API'),
      '/api/MasterData': createProxyConfig('MasterData API'),
      '/api/Users': createProxyConfig('Users API'),
      
      /* -------------------------------------------------------------------- */
      /*          BACKWARD COMPATIBILITY (Non-prefixed routes)                */
      /* -------------------------------------------------------------------- */
      
      '/Transaction': createProxyConfig(
        'Transaction (Legacy)', 
        `${API_TARGET}/api`
      ),
      '/Reports': createProxyConfig(
        'Reports (Legacy)', 
        `${API_TARGET}/api`
      ),
      '/Roles': createProxyConfig(
        'Roles (Legacy)', 
        `${API_TARGET}/api`
      ),
      '/Users': createProxyConfig(
        'Users (Legacy)', 
        `${API_TARGET}/api`
      ),
      '/MasterData': createProxyConfig(
        'MasterData (Legacy)', 
        `${API_TARGET}/api`
      ),
      
      /* -------------------------------------------------------------------- */
      /*                  WEBSOCKET SUPPORT (Optional)                        */
      /* -------------------------------------------------------------------- */
      
      '/ws': {
        target: API_TARGET.replace('https://', 'wss://').replace('http://', 'ws://'),
        ws: true,
        changeOrigin: true,
        secure: IS_PRODUCTION,
      },
      
      /* -------------------------------------------------------------------- */
      /*                    FALLBACK FOR OTHER /api ROUTES                    */
      /* -------------------------------------------------------------------- */
      
      '/api': createProxyConfig('API (Fallback)'),
    },
  },

  build: {
    outDir: 'dist',
    sourcemap: !IS_PRODUCTION,
    minify: IS_PRODUCTION ? 'esbuild' : false,
    
    // Optimize chunk splitting
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'ui-vendor': ['antd'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
        },
      },
    },
  },

  // Dependency optimization
  optimizeDeps: {
    include: ['react', 'react-dom', 'react-router-dom', 'axios', 'antd'],
  },
});