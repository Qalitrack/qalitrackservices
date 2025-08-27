const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const path = require('path');

const healthController = require('./controllers/healthController');
const docsController = require('./controllers/docsController');
const servicesController = require('./controllers/servicesController');
const { createProxyMiddleware } = require('http-proxy-middleware');

const app = express();
const port = process.env.PORT || 3000;
const projectName = process.env.PROJECT_NAME || 'Microservices';

// Security and middleware
const isProduction = process.env.NODE_ENV === 'production';

if (isProduction) {
  // Use strict CSP in production
  app.use(helmet({
    contentSecurityPolicy: {
      directives: {
        defaultSrc: ["'self'"],
        styleSrc: ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
        scriptSrc: ["'self'", "'unsafe-inline'"],
        fontSrc: ["'self'", "https://fonts.gstatic.com"],
        imgSrc: ["'self'", "data:", "https:"],
        upgradeInsecureRequests: [],
      },
    },
  }));
} else {
  // Disable CSP in development to avoid HTTP/HTTPS issues
  app.use(helmet({
    contentSecurityPolicy: false,
  }));
}

app.use(cors());
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

// Rate limiting disabled for development
// const limiter = rateLimit({
//   windowMs: 15 * 60 * 1000, // 15 minutes
//   max: 100, // limit each IP to 100 requests per windowMs
//   message: 'Too many requests from this IP, please try again later.'
// });
// app.use(limiter);

// Routes
app.use('/api/health', healthController);
app.use('/api/docs', docsController);
app.use('/api/services', servicesController);

// Project configuration endpoint
app.get('/api/config', (req, res) => {
  const ConfigService = require('./services/configService');
  const configService = new ConfigService();
  
  res.json({
    projectName: projectName,
    applicationName: configService.getApplicationName(),
    version: require('../package.json').version,
    environment: process.env.NODE_ENV || 'development',
    servers: configService.getServers()
  });
});

// --- Service Guides --- 

// API endpoint to get services with guides
app.get('/api/services/with-guides', (req, res) => {
  const ConfigService = require('./services/configService');
  const configService = new ConfigService();
  const services = configService.getAllServices();
  const servicesWithGuides = services
    .filter(s => s.guidesPath)
    .map(s => ({ name: s.description, key: s.name })); // Use description for a friendlier name
  res.json(servicesWithGuides);
});

// Serve the guides selection page
app.get('/guides', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'guides.html'));
});

// Handle static assets that browsers request at wrong paths due to relative URLs
// This MUST come before ANY other /guides/* routes
app.use('/guides/public/*', (req, res) => {
  console.log(`[STATIC-REDIRECT] Misrouted static asset: ${req.originalUrl}`);
  // Redirect to the correct path with the service name
  // For now, assume qalitrack-masterdata is the main service
  const correctedPath = req.originalUrl.replace('/guides/public/', '/guides/qalitrack-masterdata/public/');
  console.log(`[STATIC-REDIRECT] Redirecting to: ${correctedPath}`);
  res.redirect(301, correctedPath);
});

// Debug middleware for guides routes (disabled to avoid interfering with specific routes)
// app.use('/guides/*', (req, res, next) => {
//   console.log(`[DEBUG] Guides route hit: ${req.method} ${req.path}`);
//   console.log(`[DEBUG] Full URL: ${req.url}`);
//   console.log(`[DEBUG] Original URL: ${req.originalUrl}`);
//   next();
// });

// Reverse proxy for service guides
app.use('/guides/:serviceKey', (req, res, next) => {
  console.log(`[DEBUG-REDIRECT] req.path: "${req.path}", originalUrl: "${req.originalUrl}"`);
  
  // Handle trailing slash redirect for root service paths
  if (req.path === '/' && !req.originalUrl.endsWith('/')) {
    console.log(`[TRAILING-SLASH] Redirecting ${req.originalUrl} to ${req.originalUrl}/`);
    return res.redirect(301, req.originalUrl + '/');
  }

  // Continue with normal proxy handling
  console.log(`[PROXY] Service key: ${req.params.serviceKey}`);
  console.log(`[PROXY] req.path: ${req.path}`);
  console.log(`[PROXY] req.originalUrl: ${req.originalUrl}`);
  
  const ConfigService = require('./services/configService');
  const configService = new ConfigService();
  const service = configService.getService(req.params.serviceKey);

  if (!service || service.guidesPath == null) {
    return res.status(404).send('Service or guide path not found.');
  }

  const target = `http://${service.host}:${service.port}`;
  
  // Ensure guidesPath ends with / for directory serving
  let guidesPath = service.guidesPath;
  if (!guidesPath.endsWith('/')) {
    guidesPath += '/';
  }
  
  // Set Content-Type for different file types before proxying
  if (req.path.endsWith('.html') || req.path.endsWith('/') || req.path === '') {
    res.setHeader('Content-Type', 'text/html; charset=utf-8');
  } else if (req.path.endsWith('.css')) {
    res.setHeader('Content-Type', 'text/css; charset=utf-8');
  } else if (req.path.endsWith('.js')) {
    res.setHeader('Content-Type', 'application/javascript; charset=utf-8');
  } else if (req.path.endsWith('.json')) {
    res.setHeader('Content-Type', 'application/json; charset=utf-8');
  } else if (req.path.endsWith('.png')) {
    res.setHeader('Content-Type', 'image/png');
  } else if (req.path.endsWith('.jpg') || req.path.endsWith('.jpeg')) {
    res.setHeader('Content-Type', 'image/jpeg');
  } else if (req.path.endsWith('.svg')) {
    res.setHeader('Content-Type', 'image/svg+xml');
  } else if (req.path.endsWith('.ico')) {
    res.setHeader('Content-Type', 'image/x-icon');
  }
  
  const proxy = createProxyMiddleware({
    target: target,
    changeOrigin: true,
    followRedirects: true,  // Follow redirects internally
    pathRewrite: (path, req) => {
      // Use originalUrl since path gets modified by Express
      const originalPath = req.originalUrl;
      const servicePattern = `/guides/${req.params.serviceKey}`;
      
      console.log(`[PATHREWRITE] Original path: ${originalPath}`);
      console.log(`[PATHREWRITE] Service pattern: ${servicePattern}`);
      
      if (originalPath === servicePattern) {
        // Root request - return guidesPath with trailing slash
        console.log(`[PATHREWRITE] Root request -> ${guidesPath}`);
        return guidesPath;
      } else {
        // Sub-path request - replace service pattern with guides path
        const newPath = originalPath.replace(servicePattern, guidesPath.replace(/\/$/, ''));
        console.log(`[PATHREWRITE] Sub-path request -> ${newPath}`);
        return newPath;
      }
    },
    onProxyReq: (proxyReq, req, res) => {
      console.log(`[Proxy] Forwarding to ${target}${proxyReq.path}`);
    },
    onError: (err, req, res) => {
      console.error('[Proxy] Error:', err);
      res.status(502).send('Proxy error.');
    }
  });

  proxy(req, res, next);
});


// Main dashboard
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'dashboard.html'));
});

// Swagger UI
app.get('/docs', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'swagger.html'));
});

// Health check for the control hub itself
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    version: require('../package.json').version,
    uptime: process.uptime()
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    error: 'Not Found',
    message: 'The requested resource was not found',
    path: req.path
  });
});

// Error handler
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).json({
    error: 'Internal Server Error',
    message: process.env.NODE_ENV === 'development' ? err.message : 'Something went wrong'
  });
});

app.listen(port, () => {
  console.log(`🚀 ${projectName} Control Hub running at http://localhost:${port}`);
  console.log(`📊 Dashboard: http://localhost:${port}/`);
  console.log(`📋 API Docs: http://localhost:${port}/docs`);
  console.log(`💚 Health: http://localhost:${port}/health`);
});

module.exports = app;